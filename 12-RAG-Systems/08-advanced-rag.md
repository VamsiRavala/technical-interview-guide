# Advanced RAG

Once the core pipeline (chunk → embed → hybrid retrieve → rerank → ground → cite → evaluate) is solid, the frontier is about handling harder questions and tightening cost/latency. This file covers agentic RAG, GraphRAG, multimodal RAG, query routing, semantic caching, and the cost/latency optimization playbook — plus the anti-patterns that separate a demo from a dependable system.

---

## 🤖 Agentic RAG

Standard RAG runs a **fixed** retrieve-then-generate pipeline. **Agentic RAG** puts an LLM agent *in control* of the retrieval process, so it can:

- decide **whether** retrieval is even needed,
- **reformulate** the query,
- issue **multiple** searches,
- perform **multi-hop** reasoning (retrieve → reason → retrieve again),
- choose among **multiple tools/sources** (different indexes, web, databases),
- **self-critique** whether it has enough evidence before answering.

```text
  Query ──▶ AGENT ──┬─▶ "do I need to search?" ──no─▶ answer
                    │
                   yes
                    ▼
            retrieve (tool) ──▶ reason: "enough evidence?"
                    ▲                        │
                    └──── reformulate ───────┤ no
                                             │ yes
                                             ▼
                                    ground + cite + answer
```

This handles complex, ambiguous, or multi-step questions that single-shot RAG fails on. **Implementation:** expose retrieval as a **tool** the agent calls, within an agent framework (Microsoft Agent Framework / AutoGen), with **strong termination** to bound loops (max iterations / max cost). In Semantic Kernel/MAF, the retrieval tool is just a `[KernelFunction]` that the model invokes.

```python
async def retrieve(query: str) -> list[dict]:
    """Search the knowledge base; returns chunks with ids for citation."""
    ...
agent = AssistantAgent("rag", model_client=client, tools=[retrieve],
    system_message="Answer ONLY from retrieved chunks; cite ids; "
                   "say you don't know if not found.")
```

**Azure AI Search agentic retrieval** brings this inside the search service: an LLM **plans/decomposes** a query into subqueries, runs them in parallel, and merges results.

**Trade-offs:** more LLM/retrieval calls (latency, cost) and added complexity, so reserve it for genuinely hard queries — simple FAQ-style questions don't need it. Agentic RAG is the convergence of the RAG and agent worlds. (See section `11-AI-Agents` for agent fundamentals.)

---

## 🕸️ GraphRAG

**GraphRAG** (notably from Microsoft Research) builds a **knowledge graph** from your corpus instead of (or in addition to) a flat vector index.

**Indexing:** an LLM extracts **entities and relationships** from documents and constructs a graph, then **community detection** clusters related entities and the LLM generates **summaries** for each community at multiple levels.

**Query time:**
- **Global / holistic questions** ("what are the main themes across all reports?") — traverse community summaries.
- **Multi-hop questions** — follow relationships across entities.

These are exactly what flat vector RAG (retrieving isolated chunks) struggles with.

| | Vector / Hybrid RAG | GraphRAG |
|---|---|---|
| Best at | "Find the relevant passage" lookups | Cross-document synthesis, entity relationships, whole-corpus sensemaking |
| Indexing cost | Cheap (embed chunks) | Expensive, LLM-heavy (extract entities, build + summarize graph) |
| Complexity | Low | High |

Use GraphRAG when questions require synthesizing across many documents or following entity relationships; use standard vector/hybrid RAG for typical lookups. **They combine** — graph for structure, vectors for passage retrieval. An advanced pattern worth knowing for complex enterprise knowledge bases.

---

## 🖼️ Multimodal RAG

Real enterprise documents carry information in **tables, images, charts, and scans** that naive text extraction loses. (Ingestion detail is in `02`.) Three strategies to make non-text content retrievable:

1. **Describe-then-embed** — generate a text description of an image/diagram via a vision model (gpt-4o), embed and index that.
2. **Multi-modal embeddings** — embed images directly into the same space so they're retrievable alongside text.
3. **Store-and-pass** — store the image, retrieve it, and pass it to a vision-capable model at answer time.

For **tables**, preserve them as Markdown/HTML in the chunk so the model can read them; consider one-chunk-per-table with descriptive metadata. Use **Azure AI Document Intelligence** to extract structured tables and layout faithfully. The principle: extract faithfully, preserve structure, and represent non-text content as something retrievable and groundable. This dramatically improves RAG on financial reports, manuals, and forms.

---

## 🚦 Query Routing

Not every query should hit the same pipeline. **Query routing** classifies the incoming query and routes it to the right path:

- **Retrieval needed?** — a greeting or "thanks" needs no search; skip retrieval entirely.
- **Which index/source?** — route HR questions to the HR index, code questions to the code index, recent-news questions to web.
- **Which complexity tier?** — simple lookups → single-shot RAG; complex/multi-hop → agentic RAG; whole-corpus → GraphRAG.
- **Which model?** — easy queries → cheaper/faster model; hard ones → the capable model (model routing for cost).

Routing is usually a lightweight LLM classification or intent detector at the front of the pipeline. It cuts cost and latency by *not* running expensive stages (HyDE, multi-hop, reranking) when they aren't needed.

---

## 💾 Semantic Caching

Semantic caching serves a cached response when a **new query is semantically similar** to a cached one — not just exact-match.

```text
  Incoming query ──▶ embed ──▶ similarity to cached queries?
                                   │
                          above threshold ──▶ return cached answer (skip retrieval + generation)
                          below threshold ──▶ run full pipeline, then cache
```

- **Benefits:** large latency reduction and cost savings for repetitive/FAQ-style traffic, common in enterprise assistants.
- **Risks:** serving a **stale or subtly-wrong** answer when the new query differs in a way the embedding missed, or when underlying data changed.

**Guardrails:** set a conservative similarity threshold; **invalidate** the cache on data updates; **exclude personalized/security-sensitive answers** (a cached answer for one user must not serve another with different permissions). You can cache at the **answer level** (most savings, most risk) or the **retrieval level** (safer — regenerate the answer from cached chunks). **Azure API Management** supports semantic caching as a GenAI-gateway feature.

---

## ⚡ Latency Optimization Playbook

RAG latency = embed query + retrieve + (rewrite/HyDE/rerank) + generate. The biggest lever is usually **generation token count** (generation is sequential).

1. **Skip unnecessary stages** — gate HyDE/rewrite/multi-hop; not every query needs them (query routing).
2. **Tune ANN params** (`efSearch`) for recall/latency; keep dimensions reasonable (dimension reduction).
3. **Cache** — query-embedding cache, semantic answer cache, and **Azure OpenAI prompt caching** for the static system prompt.
4. **Parallelize** — keyword + vector retrieval run concurrently (Azure does this in hybrid); parallelize multi-query/multi-hop.
5. **Limit context** — rerank and pass only the top few chunks to cut generation tokens (also fixes "lost in the middle").
6. **Stream** the generation so perceived latency is the first token.
7. **Right-size the model** — a smaller/faster model where quality allows.
8. **Co-locate** Search and OpenAI in the same region to cut network hops.
9. **PTUs** for predictable low-latency generation under load.

Measure each stage to find the bottleneck.

---

## 💰 Cost Optimization Playbook

Costs: embedding (indexing + per-query), search tier, reranking (semantic ranker is billed), and **generation tokens (usually dominant).**

1. **Reduce generation tokens** — rerank and pass only the best few chunks; trim chunk size; concise prompts; cap output length.
2. **Right-size models** — gpt-4o-mini when quality permits; `text-embedding-3-small` / dimension reduction for embeddings.
3. **Cache** — answer caching for repeats; **prompt caching** for the static system prompt (Azure discounts cached input tokens significantly).
4. **Index efficiency** — dimension reduction + quantization shrink vector storage; right-size the Search tier/replicas.
5. **Batch embeddings** during indexing (Azure OpenAI Batch API offers discounted async processing).
6. **Avoid redundant LLM calls** — don't always run rewrite/HyDE/multi-hop.
7. **PTUs** for steady high volume can beat PAYG.
8. **Monitor token spend by feature** via telemetry; optimize the biggest line items.

Cost optimization is mostly about **generation tokens and avoiding redundant LLM calls.**

---

## ⚠️ Top RAG Anti-Patterns

The difference between a demo and a dependable system:

1. **Vector-only retrieval** — misses exact terms/identifiers. Use hybrid + semantic ranking.
2. **No reranking** — the best evidence stays buried in first-stage results.
3. **Over-stuffing context** — "lost in the middle," noise, cost. Rerank and pass few.
4. **Bad chunking** — fixed splits that sever tables/sections. Use structure-aware chunking + overlap.
5. **No security trimming** — leaking documents users shouldn't see.
6. **No grounding/refusal prompt** — hallucinating instead of "I don't know."
7. **No citations/verification** — unverifiable, untrustworthy answers.
8. **Ignoring freshness** — stale/orphaned chunks after source updates/deletes.
9. **No evaluation harness** — tuning by vibes, can't catch regressions.
10. **Ignoring prompt injection** from retrieved content.
11. **Reaching for fine-tuning** to add facts instead of RAG.
12. **No multi-turn query rewriting** in chat.
13. **Mismatched embedding model/metric** between index and query.

Avoiding these — **hybrid + rerank + tight context + trimming + grounding + citations + eval** — is the whole game.

---

## 🗝️ Key Takeaways

- **Agentic RAG** lets the model control retrieval (multi-search, multi-hop, self-critique) — reserve for hard queries; bound loops. Azure AI Search has native agentic retrieval.
- **GraphRAG** builds an entity/relationship graph for cross-document synthesis and global questions; expensive to index, combine with vectors.
- **Multimodal RAG** makes tables/images/scans retrievable (describe-then-embed, multimodal embeddings, or store-and-pass).
- **Query routing** sends each query to the right path/index/model — a cheap cost+latency win.
- **Semantic caching** cuts cost/latency for repeat traffic; use conservative thresholds, invalidate on updates, never cross security boundaries.
- Latency: skip stages, cache, stream, limit context, right-size models. Cost: reduce generation tokens above all.
- Know the **anti-patterns** cold — they're the most common interview probe.
