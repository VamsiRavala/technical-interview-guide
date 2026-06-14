# RAG Fundamentals

Retrieval-Augmented Generation (RAG) is the dominant enterprise pattern for getting an LLM to answer from *your* data — current, private, and citable — instead of relying on the parametric memory baked into its weights. This file establishes the mental model, the end-to-end pipeline, and the foundational "RAG vs fine-tuning vs long-context" decision that every other file in this section builds on.

---

## 🎯 What RAG Is and Why It Exists

An LLM is trained once and frozen. Its knowledge is whatever was in the training data up to the cutoff, compressed lossily into weights. It cannot cite a source, it cannot see your private contracts, and it cannot know what changed yesterday. When you ask it something outside that frozen knowledge, it does what it was trained to do — produce the most *plausible-sounding* continuation — which is exactly how hallucinations happen.

RAG fixes this by **retrieving relevant passages at query time** from an external knowledge source (documents, a search index, a database) and **injecting them into the prompt** as grounding context, then instructing the model to answer *only* from that context.

The benefits, in the order an interviewer wants to hear them:

| Benefit | Why it matters |
|---|---|
| **Freshness** | Answers reflect up-to-date and private data the model was never trained on. Re-index, don't retrain. |
| **Grounding / accuracy** | Responses are anchored to retrieved evidence, sharply reducing hallucination. |
| **Attribution** | You return citations, so users can verify — essential for enterprise trust and compliance. |
| **No retraining** | Update the knowledge base, not the model. Far cheaper and faster than fine-tuning. |
| **Access control** | You can security-trim retrieval so users only ever see documents they are permitted to read. |

The trade-off: you take on retrieval infrastructure, added latency per call, and a hard dependency on retrieval quality. **Garbage in, garbage out** — if the right passage isn't retrieved, no amount of prompt engineering saves the answer. On Azure, RAG is canonically built with **Azure AI Search** (retrieval) + **Azure OpenAI** (embeddings + generation).

---

## 🔁 The End-to-End RAG Pipeline

RAG has two phases: an **offline indexing** phase that prepares your data, and an **online query** phase that runs per request.

```text
  INDEXING (offline, batch / event-driven)        QUERY (online, per request, streaming)
  ┌────────────────────────────────────────┐      ┌─────────────────────────────────────────┐
  │ 1. INGEST  sources (PDF/Office/HTML/DB)  │      │ 1. (opt) REWRITE the user query           │
  │ 2. EXTRACT text (Doc Intelligence)       │      │ 2. EMBED the query (same model as index)  │
  │ 3. CHUNK  into passages (+ overlap)      │      │ 3. RETRIEVE top-k (hybrid: vector + BM25) │
  │ 4. EMBED  each chunk (text-embed-3-large)│      │ 4. (opt) RERANK (semantic ranker)         │
  │ 5. STORE  vector + text + metadata + ACL │      │ 5. FILTER by metadata + security trim     │
  │           in Azure AI Search             │      │ 6. ASSEMBLE grounded prompt + citations   │
  └────────────────────────────────────────┘      │ 7. GENERATE (GPT-4o), stream the answer   │
                                                   │ 8. POST-CHECK groundedness + content safety│
                                                   └─────────────────────────────────────────┘
```

Each stage is a tunable knob, and RAG quality is the product of all of them:

1. **Ingest** — pull from SharePoint, Blob, Confluence, databases, APIs.
2. **Extract** — clean text out of messy formats. Complex PDFs and scans need **Azure AI Document Intelligence** (layout, tables, reading order) rather than naive text extraction.
3. **Chunk** — split documents into retrievable passages. *This is where most RAG quality is won or lost.* (See `02-ingestion-and-chunking.md`.)
4. **Embed** — turn each chunk into a vector with an embedding model (e.g., `text-embedding-3-large`). The same model must be used at index and query time.
5. **Store** — write vectors + the original text + metadata (source, title, page, ACL tags) into an index.

Then per query: rewrite → embed → retrieve → rerank → filter → ground → generate → verify. Production layers on top of this: incremental/freshness indexing, evaluation harnesses, caching, monitoring, and guardrails.

---

## 🧩 The Core Insight: The Model Is a Reasoner, Not a Database

The single most important mental shift for RAG is this: **stop treating the LLM as a knowledge store and start treating it as a reasoning engine over context you supply.** Your job as the engineer is to put the *right, minimal, trustworthy* evidence in front of it and constrain it to use only that. Retrieval is the hard part; generation is mostly grounding discipline.

This is why "the LLM gave a wrong answer" is, in practice, *usually a retrieval failure* — the correct passage was never surfaced — not a generation failure. Debugging RAG starts at retrieval recall, not at the prompt.

---

## ⚖️ RAG vs Fine-Tuning vs Long-Context

These three are constantly confused in interviews. The clean framing:

| Approach | What it's for | When to use |
|---|---|---|
| **RAG** | Injecting **knowledge** — facts, documents, fresh or proprietary data the model should reason over. | Almost always start here for "answer over our data." Knowledge that changes, needs citations, or is access-controlled. |
| **Fine-tuning** | Teaching **behavior / form** — a consistent style, tone, output format, domain vocabulary, or compressing long few-shot prompts into weights. | When prompting + few-shot can't reliably enforce a *persistent behavioral or format* need. Not for facts. |
| **Long-context** | Stuffing the **entire relevant corpus** into a huge context window and skipping retrieval. | Small, bounded, fully-relevant inputs (one contract, one codebase module). Not a substitute for retrieval at scale. |

The rule of thumb that wins interviews:

> **RAG for what the model needs to *know*; fine-tuning for *how* the model should respond.**

They **compose**: you can fine-tune a model for a domain's tone and use RAG to feed it current facts. Fine-tuning does *not* reliably add new factual knowledge — it bakes facts into weights that go stale, can't be cited, and require expensive retraining to update. It also risks catastrophic forgetting.

**Why long-context doesn't kill RAG**, even with 128K–1M token windows:
- **Cost** — you pay for every input token on every call. Dumping a 200-page manual into each prompt is wildly expensive at scale.
- **Latency** — attention is O(n²); more context means slower prefill.
- **"Lost in the middle"** — models attend best to the start and end of a long context and overlook the middle, so *more* context often means *worse* answers (see `04-retrieval-vector-hybrid-semantic.md`).
- **Security** — you can't security-trim a blob you've dumped wholesale; retrieval lets you filter per user.

So the senior answer is: long context is a *complement* to RAG (it lets you pass slightly larger, fewer chunks), not a replacement. Retrieve narrow, ground tightly.

---

## 💡 Minimal RAG in C# (Semantic Kernel-style sketch)

```csharp
// 1. Embed the user query (same model as the index)
ReadOnlyMemory<float> queryVector =
    await embeddingService.GenerateEmbeddingAsync(userQuestion);

// 2. Hybrid retrieve top-k from Azure AI Search (vector + keyword), security-trimmed
var results = await searchClient.SearchAsync<Chunk>(
    searchText: userQuestion,
    new SearchOptions
    {
        VectorSearch = new() { Queries = { new VectorizedQuery(queryVector) { KNearestNeighborsCount = 30, Fields = { "contentVector" } } } },
        QueryType = SearchQueryType.Semantic,            // semantic reranker
        SemanticSearch = new() { SemanticConfigurationName = "default" },
        Filter = $"aclGroups/any(g: search.in(g, '{userGroups}'))" // security trimming
    });

// 3. Assemble a grounded prompt: only the top reranked chunks + strict instructions
string context = string.Join("\n\n", results.Value.GetResults()
    .Take(5).Select(r => $"[{r.Document.Id}] {r.Document.Content}"));

string prompt = $"""
    Answer the question using ONLY the context below. Cite the [id] for each claim.
    If the answer is not in the context, say you don't know.

    Context:
    {context}

    Question: {userQuestion}
    """;

// 4. Generate (stream to the client)
await foreach (var token in chatService.GetStreamingChatMessageContentsAsync(prompt))
    yield return token;
```

This is the skeleton. Every other file in this section is about making each of these steps *good*: chunking well (`02`), embedding and indexing right (`03`), retrieving with hybrid + semantic ranking (`04`), reranking and rewriting (`05`), grounding and citing (`06`), evaluating (`07`), and going advanced with agentic/GraphRAG (`08`).

---

## 🗝️ Key Takeaways

- RAG = retrieve relevant evidence at query time + inject into prompt + answer only from it. It buys freshness, grounding, attribution, no-retrain updates, and access control.
- The pipeline is **index offline** (ingest → extract → chunk → embed → store) and **query online** (rewrite → embed → retrieve → rerank → filter → ground → generate → verify).
- Treat the LLM as a reasoner over supplied context, not a database. Most "wrong answers" are retrieval (recall) failures.
- **RAG for knowledge, fine-tuning for behavior, long-context as a complement.** They compose; they don't replace each other.
- On Azure the default stack is **Azure AI Search (hybrid + semantic ranker) + Azure OpenAI (embeddings + GPT-4o)**.
