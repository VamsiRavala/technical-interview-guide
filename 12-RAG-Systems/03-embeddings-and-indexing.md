# Embeddings and Indexing

Embeddings turn text into vectors that capture meaning; the index stores those vectors and makes them searchable in milliseconds over millions of documents. This file covers choosing an embedding model, dimensionality and quantization trade-offs, designing the Azure AI Search index (HNSW, fields, metric), integrated vectorization, and keeping the index fresh with incremental indexing.

---

## 🧮 What Embeddings Are (and the One Rule You Cannot Break)

An embedding is a dense vector (e.g., 1536 or 3072 dimensions) that encodes the semantic meaning of text so similar meanings map to nearby vectors. The workflow: embed your corpus once at index time, store the vectors; at query time embed the query and find nearest neighbors.

> **The unbreakable rule: you must use the *same* embedding model (and dimensionality) for indexing and querying.** Mixing models or dimensions silently corrupts retrieval — the query and document vectors live in incompatible spaces. Changing the model means **re-embedding the entire corpus**, a costly migration. In a shared platform, a centralized embedding service that pins one version is critical precisely because one team upgrading silently breaks a shared index.

---

## 🎯 Choosing an Embedding Model

Pick based on quality, dimensionality/cost, language coverage, max input length, and platform fit.

On Azure OpenAI, the main choices:

| Model | Default dims | Notes |
|---|---|---|
| **text-embedding-3-large** | 3072 | Highest quality; supports dimension reduction. The strong default for Azure RAG. |
| **text-embedding-3-small** | 1536 | Cheaper/faster; use when cost/latency dominate. |
| text-embedding-ada-002 | 1536 | Older; prefer the `-3` models for new work. |

Decision factors:
- **Domain fit** — general vs specialized. *Test on your own data*, don't trust leaderboards; domain fit varies.
- **Multilingual needs** — multilingual corpora need multilingual embeddings.
- **Max input length** — so chunks aren't truncated; align chunk size to it.
- **Dimensionality** — higher can improve recall but costs more storage, memory, and search latency.

**Validate the choice** with `recall@k` / MRR on a labeled retrieval set of representative queries *before* committing to a full index build. Re-embedding later is expensive.

For most Azure RAG: **`text-embedding-3-large` (optionally dimension-reduced) with cosine similarity** is the well-justified default; `-small` when cost/latency dominate.

---

## 📐 Dimensions, Quantization, and the Distance Metric

### Dimensionality and Matryoshka reduction

Higher-dimensional embeddings capture more nuance and generally improve recall, but cost more storage, memory, and search time. The `text-embedding-3` models support **Matryoshka-style dimension reduction**: you can request fewer dimensions (e.g., 1024 / 512 / 256) and the most important information is concentrated in the leading dimensions, so you trade a *small* quality drop for *large* storage/latency savings. This matters enormously at scale.

```csharp
// Request reduced dimensions from text-embedding-3-large
var options = new EmbeddingGenerationOptions { Dimensions = 1024 };
var embedding = await embeddingClient.GenerateEmbeddingAsync(text, options);
```

**Always re-validate `recall@k` after any dimensionality change** — don't assume the trade-off is acceptable for your domain.

### Quantization

For very large indexes, Azure AI Search supports **vector compression** (scalar and binary quantization) on the vector field. This stores vectors in a compressed form (e.g., int8 or 1-bit) to cut index storage and memory dramatically, with optional reranking against full-precision vectors to recover accuracy. The trade-off is a small recall hit for big cost savings — measure it. Combine quantization + dimension reduction when storage/memory is the binding constraint.

### Distance metric

The metric must match how the embeddings were trained. **OpenAI/Azure embeddings are normalized and designed for cosine similarity** (equivalently dot product on normalized vectors). Using the wrong metric (e.g., Euclidean) silently degrades relevance.

| Metric | When |
|---|---|
| **cosine** | Default for Azure OpenAI embeddings. |
| dotProduct | Equivalent to cosine on normalized vectors; cheaper to compute. |
| euclidean | L2 distance; sensitive to magnitude — avoid for OpenAI embeddings. |

You set this in the index's vector profile.

---

## 🗂️ Designing the Azure AI Search Index

Azure AI Search is the canonical Microsoft choice: it holds the **vector field (HNSW)** + **text fields (BM25)** + **filterable metadata** in one index, and supports hybrid + semantic reranking natively. A representative chunk index:

| Field | Type | Attributes | Purpose |
|---|---|---|---|
| `id` | `Edm.String` | key | `{docId}-{chunkIndex}` (stable key for clean updates) |
| `content` | `Edm.String` | searchable | chunk text (BM25 lexical) |
| `contentVector` | `Collection(Edm.Single)` | searchable, dims=3072, HNSW | embedding (`text-embedding-3-large`) |
| `docId` | `Edm.String` | filterable | parent document |
| `title` | `Edm.String` | searchable, retrievable | document title |
| `page` | `Edm.Int32` | retrievable | source page for citation |
| `section` | `Edm.String` | retrievable | heading/section |
| `aclGroups` | `Collection(Edm.String)` | filterable | Entra group IDs allowed to see the chunk |
| `sourceUrl` | `Edm.String` | retrievable | blob / SharePoint link |
| `lastModified` | `Edm.DateTimeOffset` | filterable, sortable | freshness filtering/boosting |

Configure it with a **vector profile (HNSW)** plus a **semantic configuration** for the L2 semantic reranker.

### HNSW: the ANN algorithm

**HNSW (Hierarchical Navigable Small World)** is the approximate nearest-neighbor algorithm Azure AI Search uses. It builds a multi-layer graph where upper layers are sparse "express lanes" and lower layers dense; search starts at the top and greedily navigates toward the query's neighbors — sub-linear time over millions of vectors with high recall.

Key tunable parameters in the vector profile:

| Parameter | Effect |
|---|---|
| **m** | Max bi-directional links per node. Higher = better recall, more memory. |
| **efConstruction** | Candidate list size during *index build*. Higher = better graph quality, slower build. |
| **efSearch** | Candidate list size at *query time*. Higher = better recall, slower query. |

Azure also offers **exhaustiveKnn** (exact search) for small datasets or ground-truth evaluation. Tune `efSearch` to balance recall vs latency — it's the main query-time lever.

---

## ⚙️ Integrated Vectorization

**Integrated vectorization** lets Azure AI Search handle chunking and embedding for you, at both index and query time, so you don't build a separate embedding pipeline.

```text
  Data source (Blob/ADLS/SQL) ─▶ INDEXER ─▶ SKILLSET
                                              ├─ extract/crack text
                                              ├─ Split skill (chunking)
                                              └─ AzureOpenAIEmbedding skill ─▶ vectors
                                                                                │
                                                                                ▼
                                                                          INDEX (vector+text+meta)
  Query text ─▶ VECTORIZER (same model) ─▶ query vector ─▶ search   (no client-side embedding)
```

- During indexing, an **indexer** with a **skillset** pulls from a data source, cracks text, applies a **Split skill** (chunking) and an **AzureOpenAIEmbedding skill** (calls your embedding deployment), and writes vectors + text + metadata to the index.
- At query time, a **vectorizer** on the index embeds the incoming query with the *same* model server-side, so you send plain text.

Benefits: less custom code, consistent chunking/embedding, easy incremental indexing via the indexer, and simpler queries. It's the recommended fast path and pairs naturally with hybrid + semantic ranking. The trade-off is less control than a hand-built pipeline — graduate to custom chunking when you need it.

---

## 🔄 Freshness and Incremental Indexing

Knowledge changes, so the index must stay current **without full rebuilds**.

- **Incremental indexing** — only process new/changed/deleted documents. In Azure AI Search, **indexers with change detection** (Blob `LastModified`/metadata, SQL change tracking, ADLS) automatically pick up deltas on a schedule, re-chunk and re-embed only changed docs (with integrated vectorization).
- **Deletions must be handled explicitly** — via a soft-delete / deletion-detection policy — so stale content can't be retrieved. Orphaned chunks are a classic bug.
- **Push-based ingestion** — your pipeline tracks document versions and upserts/removes affected chunks. Key chunks by **stable IDs** (`{docId}-{chunkIndex}`) so updates replace cleanly rather than duplicate.
- **Freshness bias** — store timestamps and either filter to a freshness window or boost recency via **scoring profiles** so the model grounds on current information (e.g., the latest policy version).
- **High churn** — schedule frequent indexer runs or event-driven indexing; monitor indexer runs for failures.

Targets to design for (from the enterprise RAG reference architecture): new/changed docs searchable within ~15 minutes, with no stale or orphaned chunks.

---

## 🗝️ Key Takeaways

- Use the **same embedding model and dimensionality** at index and query time — changing it means a full re-embed. Centralize and version-pin the embedding model on shared platforms.
- **`text-embedding-3-large` + cosine** is the strong Azure default; `-small` for cost; validate `recall@k` on your own data.
- **Dimension reduction** (Matryoshka) and **quantization** trade a small recall hit for large storage/latency savings — measure both.
- Design the index with **HNSW vector + BM25 text + filterable metadata + ACLs**; tune `efSearch` for the recall/latency balance.
- **Integrated vectorization** is the low-code fast path; custom pipelines give more control.
- Keep the index fresh with **incremental indexing + change detection + explicit deletion handling + stable keys**.
