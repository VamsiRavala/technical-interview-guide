# RAG with SK

Retrieval-Augmented Generation in SK = retrieve relevant chunks from a vector store, stuff them into the prompt, then generate a grounded answer. SK provides the embedding generator, the vector store ([05-memory-and-vectors.md](05-memory-and-vectors.md)), and prompt templating ([04-prompt-templates.md](04-prompt-templates.md)) to wire this cleanly. This is the canonical enterprise SK use case.

> Deep RAG *architecture* (ingestion pipelines, chunking strategy, reranking, security trimming at scale) lives in section **12**. This page is the SK-API view.

---

## The four stages

```text
  INGEST (offline)                          QUERY (per request)
  ───────────────                           ───────────────────
  documents                                 user question
    │ chunk (~300–800 tok, overlap)           │ embed
    ▼                                          ▼
  embed each chunk                          vector / hybrid search
    │                                          │ top-k chunks + metadata
    ▼                                          ▼
  upsert into vector store                  AUGMENT prompt (cite sources)
                                             │
                                             ▼
                                           GENERATE (chat model)
```

1. **Ingest** — chunk documents, embed with `text-embedding-3-small`/`-large`, upsert into a vector store collection.
2. **Retrieve** — embed the user question, run vector (or hybrid) search, return top-k chunks with metadata.
3. **Augment** — inject retrieved chunks into the prompt with citations and a grounding instruction.
4. **Generate** — call the chat model with the augmented prompt.

---

## RAG-as-tool (recommended for agents)

The idiomatic SK pattern exposes **search as a `[KernelFunction]`** so the model retrieves *on demand* via function calling — agentic RAG. The model retrieves only when it needs grounding, and can re-query with refined terms.

```csharp
public sealed class RetrievalPlugin
{
    private readonly VectorStoreCollection<string, DocChunk> _collection;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embedder;

    public RetrievalPlugin(VectorStoreCollection<string, DocChunk> c,
                           IEmbeddingGenerator<string, Embedding<float>> e)
        => (_collection, _embedder) = (c, e);

    [KernelFunction, Description("Search internal docs for relevant passages.")]
    public async Task<string> SearchAsync(
        [Description("The user question")] string query, int top = 4)
    {
        var vec = (await _embedder.GenerateAsync(query)).Vector;
        var sb = new StringBuilder();
        await foreach (var r in _collection.SearchAsync(vec, top))
            sb.AppendLine($"[{r.Record.Source}] {r.Record.Text}");
        return sb.ToString();
    }
}
```

With `FunctionChoiceBehavior.Auto()`, the model calls `SearchAsync` only when it needs context — saving tokens on chit-chat and improving quality (the model formulates better queries than the raw user text and can search iteratively).

---

## RAG-as-prompt-injection (simple, predictable)

For single-shot Q&A, retrieve first, then template the result in — simpler and more deterministic.

```csharp
const string ragPrompt = """
    Use ONLY the context to answer. If the answer isn't in the context, say you don't know.
    Cite sources in [brackets].

    Context:
    {{ Retrieval.SearchAsync query=$question }}

    Question: {{$question}}
    Answer:
    """;

var rag = kernel.CreateFunctionFromPrompt(ragPrompt,
    new OpenAIPromptExecutionSettings { Temperature = 0.1 });

var answer = await kernel.InvokeAsync(rag,
    new KernelArguments { ["question"] = "How did revenue change last quarter?" });
```

Or retrieve explicitly and pass context as a variable:

```csharp
var qVec = (await embedder.GenerateAsync(question)).Vector;
var sb = new StringBuilder();
await foreach (var r in collection.SearchAsync(qVec, top: 5))
    sb.AppendLine($"[{r.Record.Source}] {r.Record.Text}");

var answer = await kernel.InvokePromptAsync(
    "Answer using only this context:\n{{$ctx}}\n\nQ: {{$q}}",
    new() { ["ctx"] = sb.ToString(), ["q"] = question });
```

| | RAG-as-prompt | RAG-as-tool |
|---|---|---|
| Retrieval timing | Always, up front | Only when the model decides it needs it |
| Re-querying | No | Yes (iterative refinement) |
| Predictability | High | Lower (model-driven) |
| Best for | Single-shot Q&A | Agents, multi-turn |

---

## Grounding and citations

Naive RAG fails in predictable ways — guard against each:

- **Chunking.** Too large dilutes relevance; too small loses context. Aim ~300–800 tokens with overlap, split on semantic/structural boundaries (headings, paragraphs), not blind fixed sizes.
- **No grounding guardrail.** Without "answer only from context," the model answers from parametric memory (hallucinates). Always instruct grounding, enforce `[bracketed]` citations, and provide an explicit "I don't know" path.
- **Stale index.** Wire reindexing to source-of-truth changes (event-driven ingest).
- **Pure vector misses keywords.** On Azure, prefer **hybrid + semantic ranker** — pure cosine misses exact entities and literal matches.
- **Prompt injection lives in your documents.** Sanitize retrieved content and treat it as data, not instructions.

Detect ungrounded answers with a faithfulness/groundedness check (LLM-as-judge or rule-based), and gate on it. A function-invocation or prompt-render filter is a natural place to enforce citation presence or block low-score retrievals.

```csharp
// Low-confidence guard: if top score is weak, refuse rather than hallucinate
var hits = new List<VectorSearchResult<DocChunk>>();
await foreach (var r in collection.SearchAsync(qVec, top: 5)) hits.Add(r);
if (hits.Count == 0 || hits[0].Score < 0.20)
    return "I don't have enough information in the indexed documents to answer that.";
```

---

## Interview angle

> Explain **RAG-as-prompt-injection** (retrieve, then template — simple, predictable, single-shot) vs **RAG-as-tool** (expose search as a function the model calls when it decides it needs context — better for agents, allows re-querying). For "how do you stop RAG hallucination?": instruct "answer only from context," enforce citations, add an "I don't know" path, run faithfulness/groundedness evals, and sanitize retrieved text. The senior signal is mentioning hybrid + semantic ranker and groundedness evaluation rather than just "embed and search."

---

**Next:** [Filters & Middleware](07-filters-and-middleware.md) — the cross-cutting layer for logging, guardrails, and caching.
