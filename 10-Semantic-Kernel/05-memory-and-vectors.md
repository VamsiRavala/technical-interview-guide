# Memory & Vectors

"Memory" in SK means giving the model relevant context beyond the current prompt — typically via **embedding text → storing vectors → semantic search**. There are two distinct ideas:

- **Short-term memory** = the `ChatHistory` of a single conversation (managed by truncation/summarization — see [09-production-patterns.md](09-production-patterns.md)).
- **Long-term memory** = persisted vectors/facts you retrieve on demand (the focus of this page; the basis of RAG in [06-rag-with-sk.md](06-rag-with-sk.md)).

The modern API is the **Vector Store abstraction** (`Microsoft.Extensions.VectorData`). The older `ISemanticTextMemory` / `MemoryBuilder` / `IMemoryStore` API still exists but is legacy.

---

## `Microsoft.Extensions.VectorData`

`Microsoft.Extensions.VectorData` is the modern, provider-neutral vector store abstraction shared across the .NET AI ecosystem. You annotate a POCO with attributes, get an `IVectorStore` / `VectorStoreCollection<TKey, TRecord>`, and the **same code works across Azure AI Search, Cosmos DB, Redis, Qdrant, Postgres/pgvector, SQL Server, and in-memory** — swap stores with a one-line connector change.

```csharp
using Microsoft.Extensions.VectorData;

public sealed class DocChunk
{
    [VectorStoreKey] public string Id { get; set; } = default!;
    [VectorStoreData(IsIndexed = true)] public string Source { get; set; } = default!;
    [VectorStoreData] public string Text { get; set; } = default!;

    // 1536 = text-embedding-3-small dimension
    [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Embedding { get; set; }
}
```

> Attribute names have evolved across previews. Mid-2026 GA uses `[VectorStoreKey]` / `[VectorStoreData]` / `[VectorStoreVector]`; some samples still show the older `[VectorStoreRecordKey]` / `[VectorStoreRecordData]` / `[VectorStoreRecordVector]` forms. (Verify current attribute names for your package version.)

This decoupling means you prototype with `InMemoryVectorStore` and ship on Azure AI Search with no logic change — and it makes RAG retrieval deterministically testable in unit tests.

---

## Embeddings

SK's embedding interface is converging on **`Microsoft.Extensions.AI`'s `IEmbeddingGenerator<string, Embedding<float>>`**; you'll still see the older `ITextEmbeddingGenerationService` in many samples. Both register via the connector builders (`AddAzureOpenAIEmbeddingGenerator`, etc.).

```csharp
var embedder = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

// Single
var emb = await embedder.GenerateAsync("Quarterly revenue grew 12%.");
ReadOnlyMemory<float> vector = emb.Vector;

// Batched (cheaper, fewer round-trips — do this at ingest time)
var batch = await embedder.GenerateAsync(chunks);
```

Choose the embedding model deliberately (dimensions, cost, multilingual), batch inputs at ingest, and **cache by content hash** — embeddings are deterministic for a given input/model, so re-embedding identical content is wasted spend.

---

## The Azure AI Search connector

Azure AI Search is the recommended production vector store for Microsoft-centric stacks. Using `Microsoft.SemanticKernel.Connectors.AzureAISearch`, you back an `AzureAISearchVectorStore` with a `SearchIndexClient` authenticated via `DefaultAzureCredential` (managed identity, not admin keys).

```csharp
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;

var searchClient = new SearchIndexClient(new Uri(endpoint), new DefaultAzureCredential());
var store = new AzureAISearchVectorStore(searchClient);

var collection = store.GetCollection<string, DocChunk>("docs");
await collection.EnsureCollectionExistsAsync();

// Index
var emb = await embedder.GenerateAsync("Quarterly revenue grew 12%.");
await collection.UpsertAsync(new DocChunk
{
    Id = "doc-1", Source = "10-Q", Text = "Quarterly revenue grew 12%.",
    Embedding = emb.Vector
});

// Search
var queryVec = (await embedder.GenerateAsync("How did revenue change?")).Vector;
await foreach (var hit in collection.SearchAsync(queryVec, top: 3))
    Console.WriteLine($"{hit.Score:F3} {hit.Record.Text}");
```

Azure AI Search adds **hybrid search** (vector + BM25 keyword), **semantic ranking**, and filterable metadata fields — strong for enterprise RAG. Grant the identity a scoped data role (e.g., *Search Index Data Reader/Contributor*) and pair it with Azure OpenAI embeddings in the same tenant.

---

## Production gotchas

- **Dimension mismatch is the #1 footgun.** The embedding model's dimension *must* equal the `Dimensions` on the vector property *and* the index definition. Switching from `text-embedding-ada-002` (1536) to `text-embedding-3-large` (3072) silently breaks relevance unless you **reindex**.
- **Pin the distance function.** Azure AI Search defaults can differ from cosine; mixing metrics across ingest and query ruins relevance.
- **Keep ingest and query models identical.** Mismatched embedders produce meaningless similarity scores.
- **PII is still personal data.** Vectors are reversible-ish; never embed PII without a redaction/consent policy (GDPR).

---

## Why the abstraction matters

| Benefit | Detail |
|---|---|
| **Store-agnostic code** | Prototype on `InMemoryVectorStore`, ship on Azure AI Search — one-line connector swap |
| **Testable retrieval** | Unit tests run against in-memory store with deterministic results |
| **Hybrid on Azure** | BM25 + vector + semantic ranker usually beats pure vector for enterprise content |
| **Strong typing** | Attribute-mapped POCOs replace the older stringly-typed `IMemoryStore` design |

---

## Interview angle

> "What is the Vector Store abstraction and why does it matter?" `Microsoft.Extensions.VectorData`: attribute-annotated record types plus `VectorStoreCollection<TKey, TRecord>` giving store-agnostic upsert/search. It decouples retrieval code from the store. The embedding-dimension footgun (model dim must equal the vector property and index, reindex on model change) and "prefer hybrid + semantic ranker on Azure AI Search over pure vector" are the two details that separate someone who has shipped RAG from someone who has only read about it.

---

**Next:** [RAG with SK](06-rag-with-sk.md) — wiring embeddings, vectors, and prompts into a grounded pipeline.
