# RAG System Design

Retrieval-Augmented Generation: ground the model in the enterprise's own data so it answers from facts, cites sources, refuses when it doesn't know, and never leaks a document the user isn't entitled to see. The headline insight for an interview: **retrieval quality is mostly a data-pipeline problem, not a model problem** — most "the LLM is dumb" complaints trace back to chunking, freshness, or missing metadata.

## (a) Requirements

A knowledge-base assistant over private documents (policies, contracts, product docs, tickets).

**Functional**
- Ingest heterogeneous sources (SharePoint, blobs, Confluence, DBs), chunk, embed, index.
- Retrieve via **hybrid search** (keyword + vector) + **semantic reranking**.
- **Ground** the answer in retrieved chunks with citations; refuse/abstain when retrieval is weak.
- Respect **document-level security trimming** (a user only retrieves what they may read).
- **Continuous eval** of retrieval and answer quality.

**Non-functional**

| NFR | Target |
|---|---|
| Answer latency | < 3 s P95 (streaming first-token < 1 s) |
| Retrieval recall@5 | > 0.9 on golden set |
| Groundedness | > 0.95 (answers supported by retrieved context) |
| Freshness | New/changed docs searchable within 15 min |
| Security | 100% document-level ACL trimming; zero cross-tenant leakage |

## (b) Architecture diagram

```text
  INGESTION (async, event-driven)                     QUERY (sync, streaming)
  ┌──────────────────────────────────────┐   ┌─────────────────────────────────────┐
  │ Sources: SharePoint / Blob / DB / API│   │ User ──▶ BFF ──▶ AI Gateway (APIM)   │
  │        │ change events (Event Grid)   │   │                       │             │
  │        ▼                              │   │                       ▼             │
  │ Ingestion Worker (Container Apps)     │   │  1. Embed query (Azure OpenAI emb.)  │
  │   • extract (Doc Intelligence)        │   │  2. HYBRID SEARCH (Azure AI Search): │
  │   • CHUNK (semantic/recursive)        │   │       BM25 keyword + vector (HNSW)   │
  │   • EMBED (text-embedding-3-large)    │   │       + ACL filter (security trim)   │
  │   • attach metadata + ACL tags        │   │  3. RERANK (AI Search semantic       │
  │        │                              │   │       reranker / cross-encoder)      │
  │        ▼                              │   │  4. Assemble grounded prompt         │
  │  ┌──────────────────────────────┐     │   │  5. GENERATE (GPT-4o) w/ citations   │
  │  │   Azure AI Search index       │◀────┼───┤  6. Post-check: groundedness +       │
  │  │  (vector + text + filters)    │     │   │     Content Safety → stream out      │
  │  └──────────────────────────────┘     │   └─────────────────────────────────────┘
  └──────────────────────────────────────┘
            ▲                                   EVAL (offline + online)
            │ golden Q/A set ───▶ Eval harness: recall, groundedness, faithfulness,
            └────────────────────  answer-relevance (LLM-as-judge) ──▶ App Insights
```

## (c) Component-by-component walkthrough

- **Ingestion** runs as **Azure Container Apps** (or AKS) workers, triggered by **Event Grid** when source documents change — async, idempotent, retryable. Text extraction via **Azure AI Document Intelligence** (PDFs, tables, scans).
- **Chunking** is where most RAG quality is won or lost. Default to **semantic / structure-aware chunking** (split on headings/sentences, ~300-800 tokens with 10-15% overlap), not naive fixed-size. Store rich **metadata** (source, title, section, timestamp, **ACL/group tags**) alongside each chunk.
- **Embeddings**: a single, version-pinned model (`text-embedding-3-large`) served by the shared embedding service. **Never mix embedding versions in one index** — it silently corrupts similarity. A model change means re-embedding the whole corpus.
- **Index = Azure AI Search** with a **vector field (HNSW)** + **text fields (BM25)** + **filterable metadata** — the canonical Microsoft choice, supporting hybrid + semantic reranking natively.
- **Query path**: embed the query, run **hybrid search** (Reciprocal Rank Fusion of keyword + vector), apply a **security filter** so only documents in the user's Entra groups are eligible, then **semantic reranking** to reorder the top ~50 down to the top ~5.
- **Grounding**: build a prompt containing only the reranked chunks + instructions to cite and to abstain if unsupported. Run the answer through a **groundedness/faithfulness check** and **Content Safety** before streaming.
- **Eval**: an offline harness on a **golden Q/A set** (recall@k, groundedness, answer-relevance via LLM-as-judge) gated in CI; online sampling feeds the same metrics into App Insights for drift detection. **Separate retrieval metrics from generation metrics** so you can localize a problem — a good generator can mask bad retrieval.

## (d) Scalability strategies

- **Decouple ingestion from query** (separate compute, separate scaling) — ingestion is bursty/batch, query is interactive.
- **Partition the index** by tenant/domain; use **AI Search replicas** for query QPS and **partitions** for index size.
- **Cache** embeddings of frequent queries (by content hash — the safest cache layer) and full answers (semantic cache at the gateway).
- **Stream** the generation so perceived latency is the first token, not the full answer.
- **Backpressure** on ingestion via a **Service Bus** queue between Event Grid and workers; **blue/green index swap** for breaking changes (re-embed in parallel, validate via evals, flip the alias).

## (e) Security patterns

- **Document-level security trimming**: store ACL/group IDs on each chunk; inject the user's Entra group memberships as a search filter. The model must *never* receive a chunk the user can't see — **security happens at retrieval, not in the prompt.** For per-user permission mirroring (SharePoint/Graph), use the **On-Behalf-Of (OBO)** flow so retrieval reflects the user's actual access.
- **Test isolation as a release gate**: synthetic cross-tenant queries that must return zero foreign documents.
- **PII handling** via Purview classification at ingest; redact or tag sensitive chunks.
- **Private Endpoints** for AI Search and Azure OpenAI; no public exposure of the index.
- **Tenant isolation** by index-per-tenant or strict server-side filter + separate keys for hard multi-tenant. Stale ACLs in the index are a data-leak vector — propagate permission changes and deletes with the same urgency as content updates.

## (f) Trade-offs

| Decision | Option A | Option B | Guidance |
|---|---|---|---|
| **RAG vs fine-tuning** | RAG: fresh, citable, cheap to update, no training | Fine-tune: bakes knowledge/style into weights, lower per-call tokens | RAG for **facts that change** and require citations (almost always start here). Fine-tune for **format/tone/domain skill**, not facts. They compose: fine-tune the *behavior*, RAG the *knowledge*. |
| **Chunk size** | Small: precise retrieval, may lose context | Large: more context, noisier retrieval, more tokens/cost | Medium (~500 tokens) + overlap as default; use **parent-document / small-to-big retrieval** to get precision *and* context. |
| **Hybrid+rerank vs pure vector** | Pure vector: cheaper, simpler, one call | Hybrid + semantic rerank: higher recall & precision, extra latency+cost | Hybrid+rerank for enterprise quality; rerank is the single biggest quality lever. Drop it only when the latency budget is brutal. |
| **Real-time vs batch indexing** | Real-time: freshest, complex and costly | Scheduled/incremental: efficient, slight lag | Match cadence to the use case — a news bot needs minutes, a policy-manual bot tolerates daily. Drive indexing from change events, not full rebuilds. |
| **Quality vs latency vs cost** | More chunks + bigger model = better answers | Fewer chunks + smaller model = cheaper/faster | Tune top-k and model per use case; cite groundedness eval numbers as the arbiter, not vibes. |

> **Phrase that lands:** "I start with RAG, not fine-tuning, because the knowledge changes and we need citations — and I enforce ACL trimming at retrieval so the model never sees a chunk the user can't read."
