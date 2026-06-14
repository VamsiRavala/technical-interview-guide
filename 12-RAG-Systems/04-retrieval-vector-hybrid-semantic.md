# Retrieval: Vector, Hybrid, and Semantic Ranking

Retrieval is the heart of RAG — the LLM can only reason over what you surface. This file covers the three retrieval modes (keyword, vector, hybrid), how Reciprocal Rank Fusion combines them, how HNSW makes vector search fast, the Azure AI Search semantic ranker, metadata filtering (pre vs post), and security trimming. Get this stage right and most "the model is wrong" problems disappear.

---

## 🔍 Vector vs Keyword vs Hybrid

| Mode | How it matches | Strength | Blind spot |
|---|---|---|---|
| **Keyword (lexical / BM25)** | Exact term matching, sparse vectors over the vocabulary. | Product codes, names, acronyms, error codes, exact identifiers. | Misses synonyms and paraphrase ("car" vs "automobile"). |
| **Vector (dense / semantic)** | Embeds query + docs, finds nearest neighbors by meaning. | Captures meaning, synonyms, paraphrase. | Misses rare exact terms; may retrieve "topically related but wrong" passages. |
| **Hybrid** | Runs both, fuses the result lists (RRF). | Semantic recall **and** exact-term precision. | Slightly more compute; still needs reranking for top precision. |

**Hybrid consistently beats either alone** on real enterprise workloads, which are full of both natural language *and* codes/jargon. Pure vector is rarely optimal for documents containing identifiers, code, or domain terms.

There's also **learned sparse** retrieval (e.g., SPLADE) — sparse vectors with model-predicted term weights/expansions, combining lexical precision with some semantic expansion while staying interpretable. In Azure AI Search, "hybrid" specifically means **BM25 (sparse/lexical) + vector (dense), fused by RRF**.

> **The standard Azure recommendation: hybrid retrieval + semantic ranking as the default.** This is the answer interviewers expect.

```csharp
// Hybrid query in Azure AI Search: one call combines keyword + vector
var results = await searchClient.SearchAsync<Chunk>(
    searchText: userQuestion,                              // BM25 keyword leg
    new SearchOptions
    {
        VectorSearch = new()
        {
            Queries = { new VectorizedQuery(queryVector)   // vector leg
                { KNearestNeighborsCount = 50, Fields = { "contentVector" } } }
        },
        QueryType = SearchQueryType.Semantic,              // + semantic reranker
        SemanticSearch = new() { SemanticConfigurationName = "default" },
        Size = 5
    });
```

---

## 🔗 Reciprocal Rank Fusion (RRF)

The problem hybrid must solve: BM25 scores and cosine similarities live on **incompatible scales** — you can't just add them. RRF sidesteps this by fusing on **ranks, not raw scores**.

For each document, RRF sums `1 / (k + rank)` across each result list it appears in, where `rank` is its position in that list and `k` is a constant (commonly 60):

```text
  BM25 list:    [docA(1), docC(2), docF(3), ...]
  Vector list:  [docC(1), docB(2), docA(3), ...]

  RRF(docA) = 1/(60+1)  + 1/(60+3)   ← high in BM25, mid in vector
  RRF(docC) = 1/(60+2)  + 1/(60+1)   ← high in BOTH → rises to the top
```

A document ranked highly in *either* list gets a strong contribution; one ranked high in *both* wins. RRF is simple, robust, and needs no score calibration — which is why it's the default fusion for Azure hybrid search. The semantic ranker then reranks the fused top results for final precision.

---

## ⚡ HNSW: Fast Approximate Nearest Neighbors

Vector search over millions of vectors must be sub-linear, so Azure AI Search uses **HNSW (Hierarchical Navigable Small World)** — a multi-layer graph where sparse upper layers are "express lanes" and dense lower layers refine. Search navigates greedily from the top toward the query's nearest neighbors.

Tunables (in the vector profile): `m` (links per node — recall vs memory), `efConstruction` (build quality vs build time), `efSearch` (query-time recall vs latency). `exhaustiveKnn` gives exact search for small sets or ground-truth eval. (Full detail in `03-embeddings-and-indexing.md`.) For retrieval tuning, **`efSearch` is the main query-time recall/latency lever.**

---

## 🎚️ The Semantic Ranker (Second-Stage Reranking)

First-stage retrieval (BM25/vector/hybrid) optimizes for **recall and speed** over a large corpus, so it returns a broad candidate list that's imperfectly ordered. The **semantic ranker** is a second-stage reranker that re-scores the top results (up to ~50) using a deep learning model from Microsoft's Bing/research lineage, reordering so the most genuinely relevant passages rise to the top.

```text
  Hybrid retrieval ──▶ top ~50 candidates ──▶ SEMANTIC RANKER ──▶ reordered ──▶ pass top 3–5 to LLM
  (recall, fast)                              (precision, deep)
```

It also produces:
- **Semantic captions** — the most relevant extract from each result (useful for grounding/citations).
- **Semantic answers** — a directly extracted answer span when the ranker is confident.

Enable it with a semantic configuration on the index and `query_type="semantic"` (or semantic + hybrid). It's a billed add-on tier, but the relevance lift for RAG is substantial — **the rerank step is often the single biggest quality lever.** It saves you from running your own cross-encoder reranker. (More on reranking strategy in `05-reranking-and-query-rewriting.md`.)

---

## 🧱 Metadata Filtering: Pre-filter vs Post-filter

Metadata filtering restricts which documents are eligible — by category, date range, document type, tenant, or security label — combined with vector similarity. It's a cheap precision win *before* expensive reranking.

Azure AI Search exposes `vectorFilterMode`:

| Mode | Behavior | Trade-off |
|---|---|---|
| **preFilter** | Apply the filter first, then run ANN over the qualifying subset. | Best recall *within* the allowed set; can be costlier. **Use for selective/security-critical filters.** |
| **postFilter** | Run ANN, then filter the results. | Cheaper, but may return fewer than k after filtering if the ANN top-k didn't include enough matching docs. |

```csharp
new SearchOptions
{
    Filter = "category eq 'finance' and lastModified ge 2025-01-01",
    VectorSearch = new() { FilterMode = VectorFilterMode.PreFilter, /* ... */ }
}
```

**Prefer pre-filter for selective and security-critical filters** so you never miss relevant authorized documents. Filtering is essential for security trimming, multi-tenant isolation, freshness windows, and scoping by document type. The combination of semantic relevance + precise metadata constraints is core to enterprise RAG correctness.

---

## 🔐 Security Trimming

The non-negotiable enterprise requirement: **users only retrieve documents they're authorized to see — and security happens at retrieval, never in the prompt.** The model must *never receive* a chunk the caller can't see; you cannot trust prompt instructions to enforce access control.

**The standard approach:**

```text
  1. At index time:  store an ACL field per chunk — group IDs that may access it (aclGroups).
  2. At query time:  read the caller's Entra ID group memberships from their token.
  3. Add a FILTER:   restrict results to chunks whose aclGroups intersect the user's groups.
```

```csharp
// Inject the user's Entra groups as a retrieval filter — security trimming
string userGroups = string.Join(",", currentUser.GroupIds);
options.Filter = $"aclGroups/any(g: search.in(g, '{userGroups}'))";
```

The Azure OData idiom is `search.in(group_ids, 'g1,g2,g3')`. For large/dynamic ACLs, Azure AI Search has added **native document-level access control** integrated with Entra ID and source permissions (e.g., from SharePoint/ADLS) so trimming stays in sync automatically as source permissions change.

**Why this is interview-critical:** without retrieval-layer trimming, RAG leaks data across tenants and users. Use **pre-filter** so the ANN search runs only over authorized documents. Combine with Private Endpoints, managed identity, and per-tenant index isolation (index-per-tenant or strict filter + separate keys) for hard multi-tenancy.

---

## 🎯 Top-k and Reranking Depth

Two knobs: how many candidates to **retrieve** (first-stage k) and how many to **pass to the LLM** after reranking (final k).

- **Retrieve broad** (top 30–50) so the reranker has enough good candidates — too small and the best passage never enters the funnel.
- **Rerank** that set (Azure semantic ranker reranks up to ~50).
- **Pass narrow** (commonly top 3–5) to the LLM.

Passing more isn't better — it adds cost, latency, "lost in the middle" risk, and noise that misleads the model. Tune empirically: increase first-stage k until `recall@k` plateaus, then minimize final k while maintaining groundedness.

> **The recipe: retrieve wide, rerank, pass narrow.** Default starting point: retrieve 30–50, semantic rerank, pass top 3–5.

---

## 🧭 Scoring Profiles and Boosting

Beyond default ranking, **scoring profiles** boost documents by field values — boost newer docs (freshness function), boost by popularity/rating, boost title matches over body, or boost by tag/category. You define functions (magnitude, freshness, distance, tag) and weights, then apply a profile at query time.

For RAG this biases retrieval toward recent or authoritative content *without* hard filtering — e.g., a freshness function so the model grounds on current policy versions. Note scoring profiles primarily affect the **keyword/BM25** component; with hybrid + semantic ranking the semantic reranker is the dominant signal, but profiles still shape the lexical leg and candidate ordering. Use them to encode business priorities (recency, authority, source preference).

---

## 🗝️ Key Takeaways

- **Hybrid (BM25 + vector) + semantic ranker is the default** — vector alone misses exact identifiers; keyword alone misses paraphrase.
- **RRF** fuses hybrid results on ranks (not scores), so no calibration is needed; documents strong on both legs win.
- **HNSW** makes vector search sub-linear; tune `efSearch` for recall vs latency.
- The **semantic ranker** is a second-stage cross-encoder-class reranker and often the single biggest quality lever; it also yields captions/answers for grounding.
- **Metadata filtering** (prefer pre-filter for selective/security filters) is a cheap precision win.
- **Security trimming happens at retrieval, never in the prompt** — store ACLs per chunk, filter by the caller's Entra groups.
- **Retrieve wide, rerank, pass narrow** (30–50 → rerank → top 3–5).
