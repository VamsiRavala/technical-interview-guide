# Embeddings & Vectors

Embeddings are how text (or images, code, audio) becomes math you can search. They are the backbone of semantic search, RAG retrieval, clustering, deduplication, and recommendation. If you internalize one idea from this page: **an embedding turns meaning into a point in space, and "similar meaning" becomes "nearby points."** The vector-database and Azure AI Search mechanics live in sections **09** and **12** — here we build the concept so those sections make sense.

---

## What Is an Embedding?

An **embedding** is a dense vector of floating-point numbers (e.g., 1536 or 3072 dimensions from Azure OpenAI's `text-embedding-3-large`) that encodes the *semantic meaning* of its input. The model is trained so that inputs with similar meaning produce vectors that point in similar directions.

```text
"car"        → [ 0.021, -0.118,  0.339, ... ]   ┐
"automobile" → [ 0.019, -0.121,  0.341, ... ]   ├─ close together
"banana"     → [-0.402,  0.255, -0.087, ... ]   ┘   far away
```

Because it captures *meaning* rather than keywords, "car" and "automobile" land near each other even though they share no letters. That's the superpower over old keyword search.

### The workflow

```text
INDEX TIME (once):
  documents → chunk → embed → store vectors (+ metadata) in an index

QUERY TIME (per request):
  user query → embed → find nearest vectors → return top-k chunks
```

Two non-negotiable rules:
1. **Use the same embedding model for indexing and querying.** Mixing models means the spaces don't align and retrieval silently breaks.
2. **Changing the model means re-embedding the whole corpus** — a real migration cost. Plan a blue/green index swap.

```python
# Conceptual: embed a query (Azure OpenAI)
resp = client.embeddings.create(
    model="text-embedding-3-large",
    input="How do I reset my password?"
)
vector = resp.data[0].embedding   # list[float], length 3072
```

```csharp
// .NET conceptual shape
float[] vector = await embeddingClient.GenerateEmbeddingAsync(
    "How do I reset my password?");
```

The `text-embedding-3` family supports a `dimensions` parameter, letting you trade a little accuracy for smaller, cheaper, faster vectors (e.g., 3072 → 1024).

---

## Similarity Metrics: Cosine vs Dot Product vs Euclidean

Once everything is a vector, "find similar" means "find nearby." Three metrics dominate:

| Metric | Measures | Sensitive to magnitude? | Notes |
|---|---|---|---|
| **Cosine similarity** | Angle between vectors | No | **Default for text embeddings** |
| **Dot product** | Angle *and* magnitude | Yes | Equals cosine for unit-normalized vectors; cheaper to compute |
| **Euclidean (L2)** | Straight-line distance | Yes | Common in some image/spatial use cases |

```text
Cosine: cares only about DIRECTION (semantic meaning)
        cos(θ) = (A · B) / (|A| · |B|)

A —————→
        \  small angle θ → high similarity
B ————→
```

The practical truth for OpenAI/Azure embeddings: they're effectively **unit-normalized**, so **cosine and dot product give equivalent rankings** — pick whichever your index supports efficiently. The cardinal rule is **consistency**: match the metric your embedding model was trained/recommended for, and use the same metric for indexing and querying. Mismatching metrics silently degrades quality. For Azure AI Search vector profiles, cosine is the default and the safe choice.

---

## Vector Search: Brute Force vs ANN

To answer "which stored vectors are closest to this query vector?" the naive approach compares the query against **every** stored vector — **O(n)** per query. That's fine for a few thousand vectors, but at millions it's far too slow.

So vector indexes use **Approximate Nearest Neighbor (ANN)** algorithms that trade a tiny bit of recall for enormous speed.

```text
Exact NN  : check all N vectors  → perfect recall, slow, O(N)
ANN       : check a clever subset → ~99% recall, fast, sub-linear
```

### HNSW — the algorithm to know

**HNSW (Hierarchical Navigable Small World)** is the most popular ANN index and what Azure AI Search uses. It builds a **multi-layer graph**:

```text
Layer 2:   o——————————————o          (sparse, long-range "express lanes")
            \             /
Layer 1:   o——o———o———o——o            (medium connections)
            \  |   |   |  /
Layer 0:   o-o-o-o-o-o-o-o-o          (dense local connections, all nodes)
```

A search enters at the top, takes long hops to get *near* the target fast, then drops to lower, denser layers for precision — greedily hopping toward the nearest neighbors.

**Tuning knobs (recall vs latency vs memory):**

| Knob | Higher value → |
|---|---|
| `M` (connections per node) | Better recall, more memory |
| `efConstruction` (build-time candidates) | Better index quality, slower build |
| `efSearch` (query-time candidates) | Better recall, slower queries |

> The trade-off triangle is always **recall ↔ latency ↔ memory**. There's no free lunch; tune against your eval set.

---

## Size, Dimensionality & Cost

Storage and memory scale with **(number of vectors) × (dimensions) × (bytes per value)**.

```text
3072-dim float32 vector ≈ 3072 × 4 bytes ≈ 12 KB each
1,000,000 of them        ≈ 12 GB just for raw vectors (HNSW graph adds more)
```

Levers to shrink the bill:
- **Reduce dimensions** via the `text-embedding-3` `dimensions` parameter (modest accuracy loss).
- **Quantization** — compress each value:

| Quantization | Compression | Idea |
|---|---|---|
| **Scalar** (int8) | ~4× smaller | Fewer bits per dimension |
| **Binary** (1 bit/dim) | ~32× smaller | Hamming-distance search, very fast |
| **Product (PQ)** | High | Sub-vectors encoded via codebooks |

Quantization costs some recall, mitigated by **oversampling** (retrieve more candidates, then rescore against full-precision vectors). Azure AI Search supports scalar and binary quantization with oversampling. **Always validate the recall hit on your eval set before adopting it.**

---

## Choosing an Embedding Model

Decide on:
- **Domain fit** (general vs specialized)
- **Retrieval quality on your data** — benchmark, don't assume
- **Dimensionality** (accuracy vs storage/compute)
- **Max input length** and **language coverage**
- **Cost / latency**

For Azure, `text-embedding-3-large` (3072 dims, supports dimension reduction) is a strong default; `text-embedding-3-small` is the cheaper option. Multilingual corpora need multilingual embeddings. Validate with retrieval **recall@k** on representative queries before committing to a full index build.

---

## Keeping the Index in Sync

The vector index is a **derived datastore** — it must be continuously reconciled with the source of truth, not loaded once.

```text
On create/update:  re-chunk → re-embed → upsert vectors (keyed by stable doc ID)
On delete:         explicitly delete the doc's vectors (don't let stale content linger)
On model upgrade:  re-embed EVERYTHING → full rebuild → blue/green swap
```

Azure AI Search **indexers** can pull from Blob, SQL, or Cosmos DB on a schedule and detect changes; **integrated vectorization** re-embeds automatically. Monitor freshness lag and indexing failures. (Full pipeline details: section **12**.)

---

## Brush-up Cheat-Sheet

```text
Embedding   = dense vector encoding meaning; same model for index + query
Similarity  = cosine (default for text); cosine ≈ dot for normalized vectors
ANN/HNSW    = graph index, ~linear-free search; tune M / efSearch (recall vs speed vs mem)
Dimensions  = bigger = more accurate but pricier; 3-series lets you shrink them
Quantization= scalar (4×) / binary (32×); recover recall via oversampling
Sync        = upsert on change, delete explicitly, full rebuild on model change
Where deep  = vector DBs / Azure AI Search → sections 09 & 12
```

**Next:** [03-prompt-engineering](03-prompt-engineering.md) — getting the model to do what you want.
