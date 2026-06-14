# RAG Systems — Grounding LLMs in Your Own Data, the Azure Way

> The end-to-end pattern for making an LLM answer from *your* documents — current, private, citable, and access-controlled — built on **Azure AI Search** (hybrid + semantic ranker) and **Azure OpenAI** (embeddings + GPT-4o). From chunking and embeddings through retrieval, reranking, grounding, evaluation, and agentic/GraphRAG. Practical, Microsoft-centric, accurate to **mid-2026**.

---

## 📚 Table of Contents

### Foundations

1. **[RAG Fundamentals](01-rag-fundamentals.md)** - What RAG is and why, the index/query pipeline, the "reasoner not database" mental model, and RAG vs fine-tuning vs long-context
2. **[Ingestion & Chunking](02-ingestion-and-chunking.md)** - Loaders, Document Intelligence for complex PDFs, chunk size/overlap, fixed vs semantic vs structure-aware, parent-child, metadata, dedup/MMR

### Indexing & Retrieval

3. **[Embeddings & Indexing](03-embeddings-and-indexing.md)** - Embedding-model choice, dimensions/quantization (Matryoshka), the AI Search index + HNSW + cosine, integrated vectorization, incremental/freshness indexing
4. **[Retrieval: Vector, Hybrid & Semantic](04-retrieval-vector-hybrid-semantic.md)** - Vector vs keyword vs hybrid + RRF, HNSW tuning, the semantic ranker, metadata filtering (pre/post), and security trimming
5. **[Reranking & Query Rewriting](05-reranking-and-query-rewriting.md)** - Cross-encoder reranking, query rewriting and chat decontextualization, HyDE, multi-hop, multi-query fusion

### Generation & Quality

6. **[Grounding & Citations](06-grounding-and-citations.md)** - The grounding prompt, tag/enforce/verify citations, layered hallucination control, refusal/no-answer, confidence thresholds, prompt-injection defense
7. **[RAG Evaluation](07-rag-evaluation.md)** - recall@k/MRR/nDCG, groundedness/faithfulness, answer relevance, the RAG triad, RAGAS, Azure AI Foundry evaluators, A/B and regression
8. **[Advanced RAG](08-advanced-rag.md)** - Agentic RAG, GraphRAG, multimodal, query routing, semantic caching, the cost/latency playbooks, and anti-patterns

### Interview Preparation

9. **[Interview Questions](09-interview-questions.md)** - 50 detailed RAG Q&A consolidated and de-duplicated from the question banks, Azure-centric

---

## 🎯 Learning Path

### Beginner (Week 1-2)
1. Read **RAG Fundamentals** — internalize the two-phase pipeline and the "RAG for knowledge, fine-tune for behavior" rule
2. Work through **Ingestion & Chunking** — chunk a real PDF set with Document Intelligence at ~512 tokens + overlap, attach metadata
3. Do **Embeddings & Indexing** — build an Azure AI Search index (HNSW + cosine) with `text-embedding-3-large` and integrated vectorization

### Intermediate (Week 3-4)
1. Build **Retrieval** — issue a hybrid query, add the **semantic ranker**, and wire **security trimming** by Entra group
2. Add **Reranking & Query Rewriting** — retrieve wide → rerank → pass top 3–5, and decontextualize chat follow-ups
3. Implement **Grounding & Citations** — strict grounding prompt, structured citations, refusal on weak retrieval, prompt-injection spotlighting

### Advanced (Week 5-6)
1. Stand up **RAG Evaluation** — a golden set with recall@k + groundedness gated in CI via Azure AI Foundry; treat groundedness as an SLO
2. Master **Advanced RAG** — agentic retrieval, GraphRAG for cross-document questions, semantic caching, and the cost/latency playbooks
3. Drill **Interview Questions** — rehearse "hybrid+semantic+rerank by default," "security trimming at retrieval not the prompt," and the anti-patterns out loud

---

## 🔑 The RAG Pipeline at a Glance

| Stage | What it does | Azure-native answer | Deep dive |
|---|---|---|---|
| **Ingest / extract** | Pull sources, recover structure | Event Grid → Container Apps, **Document Intelligence** | `02` |
| **Chunk** | Split into retrievable units | Split skill; semantic/structure-aware ~512 tok + overlap | `02` |
| **Embed** | Text → vector | **text-embedding-3-large** (cosine), integrated vectorization | `03` |
| **Index** | Store + make searchable | **Azure AI Search**: HNSW vector + BM25 text + filterable meta | `03` |
| **Retrieve** | Surface candidates | **Hybrid** (BM25 + vector, RRF) + **security trim** | `04` |
| **Rerank** | Reorder for precision | **Semantic ranker** (cross-encoder), pass top 3–5 | `04` / `05` |
| **Ground** | Answer only from context | Grounding prompt + structured citations + verify | `06` |
| **Generate** | Produce the answer | **Azure OpenAI** (gpt-4o), streamed | `01` / `06` |
| **Evaluate** | Measure & gate | recall@k, groundedness via **Foundry eval** | `07` |

---

## 💡 Key Decisions

| Question | Short answer |
|---|---|
| **RAG or fine-tuning?** | **RAG for knowledge** (facts, fresh, citable, access-controlled); **fine-tune for behavior** (style/format). They compose. Start with RAG. |
| **Retrieval mode?** | **Hybrid (BM25 + vector) + semantic ranker** — the Azure default. Vector-only misses identifiers; keyword-only misses paraphrase. |
| **How to fuse hybrid?** | **RRF** on ranks (not raw scores), so no calibration needed. Then semantic-rerank the fused top. |
| **Chunk size?** | ~300–800 tokens, 10–15% overlap, **structure-aware**; use **parent-child** for precision + context. Tune on an eval set. |
| **Embedding model?** | **text-embedding-3-large + cosine**; same model at index and query; dimension-reduce/quantize at scale; `-small` for cost. |
| **Access control?** | **Security trimming at retrieval, never in the prompt** — store ACLs per chunk, filter by the caller's Entra groups (pre-filter). |
| **Anti-hallucination?** | High-recall retrieval → strict grounding prompt → cite + verify → refuse on weak scores → groundedness eval as an SLO. |
| **Vector store?** | **Azure AI Search** by default (hybrid + semantic + integrated vectorization + security + Foundry). Dedicated vector DB only with a strong reason. |
| **Too-hard query?** | **Agentic RAG / GraphRAG** for multi-hop or cross-document synthesis. Reserve for genuinely hard queries; bound the loops. |

---

## 🏗️ Concrete Build Reference: Enterprise RAG Platform

A worked end-to-end build that everything in this section maps onto — a knowledge-base assistant over a 2,000-person manufacturer's private docs (SOPs, safety manuals, specs, contracts) with **trustworthy, cited answers** and **document-level access control**:

```text
  INGESTION (async, event-driven)                    QUERY (sync, streaming)
  ┌─────────────────────────────────────┐   ┌──────────────────────────────────────┐
  │ Sources: SharePoint / Blob / DB / API│   │ User ─▶ React SPA ─▶ ASP.NET Core API │
  │   │ change events (Event Grid)       │   │           (Bearer: Rag.Read)           │
  │   ▼                                  │   │  1. Embed query (text-embed-3-large)   │
  │ Ingestion Worker (Container Apps Job)│   │  2. HYBRID SEARCH (BM25 + vector/HNSW) │
  │   • extract (Doc Intelligence)       │   │       + ACL filter (security trim)     │
  │   • CHUNK (semantic/structure-aware) │   │  3. RERANK (semantic ranker)           │
  │   • EMBED (text-embedding-3-large)   │   │  4. Assemble grounded prompt + cites   │
  │   • attach metadata + ACL tags       │   │  5. GENERATE (gpt-4o) via Semantic     │
  │   ▼                                  │   │       Kernel, stream over SSE          │
  │  ┌──────────────────────────────┐    │   │  6. Post-check groundedness +          │
  │  │  Azure AI Search index        │◀───┼───┤       Content Safety → stream out      │
  │  │ (vector + text + filters+ACL) │    │   └──────────────────────────────────────┘
  │  └──────────────────────────────┘    │   Azure SQL: doc metadata, ACLs, job state
  └─────────────────────────────────────┘   App Insights: ingest + query telemetry
```

- **Two stores by design:** Azure AI Search is the vector+keyword store (`documents-chunks` index, `contentVector` dims=3072 HNSW, `aclGroups` filterable, `page`/`section` for citations); Azure SQL holds operational metadata, ACLs, and job state — *not* retrieval.
- **Auth:** Entra ID end-to-end, **managed identity** to every backend service, **no keys**.
- **NFR targets:** answer P95 < 3 s (first-token < 1 s streaming), recall@5 > 0.9, groundedness > 0.95, freshness < 15 min, 100% ACL trimming with zero cross-tenant leakage.
- **.NET shape:** `Rag.Api` (query + upload endpoints, `CitationBuilder`), `Rag.Ingestion` (Container Apps Job: `LayoutExtractor` → `Chunker` → `Embedder` → `SearchIndexer`), `Rag.Search` (index schema + `HybridSearchClient`), all infra in Bicep.

The trade-off table that frames the build — **RAG vs fine-tuning**, **chunk size**, **hybrid+rerank vs pure vector**, **quality vs latency vs cost** — is the same decision set summarized above, with the rerank step as the single biggest quality lever.

---

## 🔗 Where the Rest Lives

| Topic | Section |
|---|---|
| LLM fundamentals, embeddings, tokens, prompting | **08 — AI Foundations** |
| Azure OpenAI deployments, PTU/PAYG, content filters, APIM gateway, On Your Data | **09 — Azure AI** |
| Semantic Kernel `Microsoft.Extensions.VectorData`, RAG-as-prompt vs RAG-as-tool | **10 — Semantic Kernel** |
| Agentic RAG runtime, multi-agent retrieval, MCP tools | **11 — AI Agents** |
| Enterprise RAG-as-a-service, gateway, multi-tenant platform architecture | **13 — AI System Design** |
| Streaming citations/SSE in the React client | **14 — AI Frontend** |
| The full Enterprise RAG Platform portfolio build | **15 — AI Portfolio Projects** |

This section keeps the **RAG pipeline and its quality levers** front and center; those sections cover the surrounding platform, SDK surface, and client.
