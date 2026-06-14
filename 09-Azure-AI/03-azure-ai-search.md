# Azure AI Search — Retrieval for RAG

> Microsoft's managed search service and the backbone of Azure RAG. It combines keyword, vector, and hybrid search with semantic reranking, filtering, and enterprise security in **one index** — so you usually don't stand up a separate vector database.

---

## Why It's the Default Retrieval Layer on Azure

Azure AI Search gives you, in a single service and a single query:
- **Keyword (BM25)** lexical search,
- **Vector** (ANN) semantic search,
- **Hybrid** (both, fused),
- **Semantic ranker** (cross-encoder reranking),
- **Filtering / faceting** (incl. security trimming),
- **Integrated vectorization** (chunk + embed during indexing),
- **Indexers** that pull from Blob, SQL, Cosmos DB, etc.

For "chat over your data," it beats wiring up a standalone vector DB because you get hybrid + reranking + Entra-ID security in one managed, scalable service.

---

## The Three Retrieval Modes

### Keyword (BM25)
Exact lexical matching. Nails product codes, names, acronyms, rare jargon — but misses paraphrase/synonyms.

### Vector
Embed documents and queries (e.g., `text-embedding-3-large`), store vectors, find nearest neighbors by **cosine** similarity. Captures meaning and synonyms — but can miss exact identifiers.

### Hybrid (the recommended default)
Runs vector **and** keyword in one call and fuses results, commonly via **Reciprocal Rank Fusion (RRF)**. Each covers the other's blind spot. **Pure vector is rarely optimal** for enterprise docs that contain IDs, code, or domain terms.

```text
query ──┬─► BM25 (keyword) ──┐
        │                    ├─► RRF fusion ─► semantic ranker ─► top 3–5 ─► LLM
        └─► vector (HNSW) ───┘
```

---

## HNSW (the vector index algorithm)

Azure AI Search uses **HNSW (Hierarchical Navigable Small World)** for ANN vector search.

- Multi-layer graph: upper layers have long-range links for fast coarse navigation; lower layers have dense local links for precision. Search greedily hops top→bottom toward nearest neighbors.
- Trades a tiny bit of **recall** for huge **speed** vs exact O(n) search.
- Tuning knobs (recall vs latency vs memory triangle):
  - **`m`** — connections per node (higher = better recall, more memory),
  - **`efConstruction`** — build-time candidate list (higher = better graph, slower build),
  - **`efSearch`** — query-time candidate list (higher = better recall, slower query).
- Configured in the index's **vector profile** (algorithm + metric). **Cosine** is the standard metric for OpenAI/Azure embeddings (normalized, so cosine ≈ dot product).

### Quantization (compress vectors)
For large indexes, enable **scalar** (int8, ~4× smaller) or **binary** (~32× smaller) quantization, with **oversampling** + rescoring against full-precision vectors to recover recall. Use when index size/cost or latency is the constraint; validate recall impact on an eval set.

---

## Semantic Ranker (reranking)

The **semantic ranker** is a managed **cross-encoder** second pass: first-stage retrieval (vector/keyword) fetches ~50 candidates cheaply; the ranker scores each candidate **jointly with the query** and reorders so only the top 3–5 reach the LLM. Cross-encoders are more precise than bi-encoder similarity because they attend to query and document **together**. This sharply improves precision and reduces "lost in the middle." Enable it via a **semantic configuration** on the index and `query_type="semantic"` at query time. It can also return **extractive captions/answers**. (It's a billable tier feature — verify current pricing/GA status.)

---

## Integrated Vectorization

Lets Search **chunk and embed during indexing** (calling your Azure OpenAI embedding deployment) and **vectorize queries automatically** — reducing custom code.

- A **Split skill** does page/sentence-aware chunking.
- An embedding skill calls your `text-embedding-3-*` deployment.
- Indexers pull source data (Blob/SQL/Cosmos) on a schedule and detect changes; the pipeline re-embeds automatically.
- For an embedding-model upgrade you must **re-embed the whole corpus** (full rebuild) — plan a **blue/green index swap**.

---

## Security Trimming (document-level access)

Critical for multi-tenant / enterprise RAG: users must only retrieve documents they're authorized to see.

- Store **group IDs / ACLs / tenant ID** as a **filterable field** on each document at index time.
- At query time, pass the user's group/tenant claims as an **OData `$filter`** combined with the vector/keyword query, so retrieval is scoped **before** results return.

```text
$filter=group_ids/any(g: search.in(g, 'sales,leadership')) and tenant_id eq 'contoso'
```

Without this, RAG can **leak data across tenants** or surface unauthorized PII. Combine with metadata filtering for **freshness** (date ranges) and **precision** (doc type/category). Filtering is also a cheap precision win **before** the expensive reranker.

---

## Index Design

Design the schema up front — retrofitting metadata after ingestion is painful.

| Field | Type | Attributes | Purpose |
|---|---|---|---|
| `id` | Edm.String | key | stable chunk ID for citations |
| `content` | Edm.String | searchable | the chunk text (BM25 + semantic) |
| `contentVector` | Collection(Edm.Single) | searchable (vector profile) | embedding for ANN |
| `title` / `url` / `page` | Edm.String | retrievable | citation metadata |
| `tenant_id` / `group_ids` | Edm.String / Collection | filterable | **security trimming** |
| `category` / `updated` | Edm.String / DateTimeOffset | filterable, facetable | precision + freshness filters |

```json
{
  "fields": [
    { "name": "id", "type": "Edm.String", "key": true },
    { "name": "content", "type": "Edm.String", "searchable": true },
    { "name": "contentVector", "type": "Collection(Edm.Single)",
      "dimensions": 3072, "vectorSearchProfile": "hnsw-cosine" },
    { "name": "tenant_id", "type": "Edm.String", "filterable": true },
    { "name": "group_ids", "type": "Collection(Edm.String)", "filterable": true }
  ],
  "vectorSearch": {
    "algorithms": [{ "name": "hnsw", "kind": "hnsw",
      "hnswParameters": { "m": 4, "efConstruction": 400, "metric": "cosine" } }],
    "profiles": [{ "name": "hnsw-cosine", "algorithm": "hnsw" }]
  }
}
```

**Chunking** (see also `12-RAG-Systems` if present): fixed-size with 10–20% overlap (e.g., 500–1000 tokens) is a robust default; structural chunking (headings/paragraphs) keeps chunks self-contained. Always store source/section/page metadata for citations.

---

## Two Ways to Use It for RAG

1. **Azure OpenAI "On Your Data"** — attach the index as a `data_sources` parameter on the chat call; the service retrieves, grounds, and returns **citations**. Fastest path; less control over reranking/prompting. Recommend `query_type` = **hybrid + semantic**, use **managed identity**, and set `strictness` + `in-scope`.
2. **Custom pipeline** (full control) — embed query → call AI Search **hybrid + semantic** with filters/security trimming → take top reranked chunks → assemble a grounded prompt (answer only from context, cite IDs) → call the chat deployment. Use this when you need custom reranking depth, query rewriting/HyDE, multi-hop, or structured outputs.

> See `08_system_design` / the system-design section for where AI Search sits in an end-to-end RAG architecture (indexing pipeline, query path, gateway).

---

## Brush-Up Cheat-Sheet

- **Hybrid (vector + BM25) + semantic ranker** = recommended default retrieval. Pure vector is rarely optimal.
- **HNSW** for ANN; tune **m / efConstruction / efSearch** (recall vs latency vs memory); **cosine** metric.
- **Quantization** (scalar/binary) + oversampling for large indexes.
- **Integrated vectorization** = chunk + embed in the indexer; model change ⇒ full re-embed (blue/green swap).
- **Security trimming** = filterable group/tenant fields + `$filter` on the user's claims; prevents cross-tenant leakage.
- Design the **index schema (filter fields) up front**; store stable chunk IDs for citations.
