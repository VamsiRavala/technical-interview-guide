# Reranking and Query Rewriting

Two of the highest-leverage, lowest-cost improvements to RAG quality sit on either side of retrieval: **query rewriting** before you search (so you search for the *right* thing), and **reranking** after you search (so the *best* evidence reaches the model). This file also covers HyDE, multi-hop retrieval, and multi-query fusion.

---

## 🎚️ Reranking: Retrieve Wide, Rerank Precisely, Pass Narrow

First-stage retrieval (vector/BM25/hybrid) optimizes for **recall and speed** over potentially millions of items, so it returns a broad candidate list that may be imperfectly ordered. A **reranker** is a second stage that reorders that candidate set by deeper relevance scoring before the top results reach the LLM.

**Why it works:** a reranker is a **cross-encoder** — it jointly examines the query *and* each candidate in one model pass, attending to both together. This is far more accurate than first-stage **bi-encoder** similarity, which embeds query and document *independently* and compares vectors. The catch: cross-encoders are too slow to run over the whole corpus, so you use the **retrieve-then-rerank** pattern.

```text
  Query ──▶ first-stage retrieval (bi-encoder / BM25) ──▶ top 30–50 candidates
                                                              │
                                                              ▼
                                              CROSS-ENCODER RERANKER (joint scoring)
                                                              │
                                                              ▼
                                                  reordered ──▶ pass top 3–5 to LLM
```

**Why it matters:** the LLM only sees the top few chunks you include. If the best evidence is ranked 8th and you pass top-5, the answer suffers. Reranking improves `precision@k`, groundedness, and answer quality, *and* lets you retrieve a wide net cheaply then keep only the best — which also reduces "lost in the middle" (fewer, better chunks).

**On Azure:** the **semantic ranker** is the managed reranker (reranks up to ~50 results, also produces captions/answers). You can also plug in external cross-encoder models. The pattern — **retrieve broadly, rerank precisely, pass few** — is a core RAG quality lever and frequently the single biggest one.

---

## ✏️ Query Rewriting: Search for the Right Thing

Raw user queries are often suboptimal for retrieval: terse, ambiguous, full of pronouns ("what about *its* pricing?"), conversational, or phrased differently from the documents. Query rewriting transforms the query to improve retrieval.

| Technique | What it does | When |
|---|---|---|
| **Expansion** | Add synonyms / related terms. | Vague or under-specified queries. |
| **Decontextualization** | Resolve "it/that/them" from chat history into a standalone query. | **Essential** for multi-turn chat RAG. |
| **Decomposition** | Split a complex question into sub-queries. | Multi-hop questions. |
| **Multi-query** | Generate several phrasings, retrieve for each, merge results. | Broaden the candidate net. |

You typically use an LLM to do the rewrite, which costs an extra call (latency/cost) — so **gate it**: clear, specific queries don't need it; conversational assistants almost always do. Azure AI Search also offers a server-side **query rewriting** feature that generates query variants to boost recall.

Done well, rewriting is **one of the highest-impact, lowest-cost retrieval improvements** — especially for vague or follow-up questions.

### Conversational (multi-turn) RAG hinges on this

In chat, follow-ups depend on prior turns ("what about its warranty?"). Naive retrieval on the raw follow-up fails because the retriever has no idea what "it" is. The core technique is **query reformulation / condensation**:

```text
  History: "Tell me about the X500 pump."  →  "It's rated for 200°C..."
  Follow-up: "what about its warranty?"
                    │  rewrite using history
                    ▼
  Standalone query: "What is the warranty on the X500 pump?"  →  retrieve → ground → cite
```

Then run normal hybrid + semantic retrieval on the rewritten standalone query. Manage conversation context (include recent turns but summarize older ones; rely on retrieval for facts rather than stuffing all history), decide per-turn whether retrieval is even needed (a "thanks" doesn't need a search), and **security-trim every retrieval** by the user's identity throughout. Azure OpenAI On Your Data handles much of this automatically. **The standalone-query rewrite is what makes multi-turn RAG actually work.**

---

## 🧪 HyDE (Hypothetical Document Embeddings)

A query-transformation technique that improves *vector* retrieval. The insight: a short query and a relevant document live in somewhat different regions of embedding space, hurting similarity matching.

**HyDE's fix:**

```text
  Query ──▶ LLM generates a HYPOTHETICAL answer/document
                          │
                          ▼
              embed the hypothetical document (not the raw query)
                          │
                          ▼
              vector search ──▶ retrieves real, grounded documents ──▶ LLM answers from those
```

Because the hypothetical document resembles real documents in form and content, its embedding lands nearer the actual relevant passages, improving recall — especially in **sparse or zero-shot domains** where the query alone retrieves poorly.

Crucially, **the hypothetical answer can be factually wrong** — that's fine, because you only use its *embedding* to retrieve real, grounded documents, which the LLM then uses to answer. Costs an extra generation call and latency. HyDE is most useful when straightforward query embeddings underperform; it's one tool among query-rewriting strategies, **not always necessary** if hybrid + semantic already retrieves well.

---

## 🔀 Multi-Hop Retrieval

Some questions require chaining evidence across multiple documents or multiple retrieval steps, where a single retrieval can't surface everything. Example: *"Which of our products launched after the CEO's start date?"* requires finding the CEO's start date, then finding products and launch dates — two hops.

Approaches:

| Approach | How it works |
|---|---|
| **Iterative / agentic retrieval** | Retrieve, reason about what's still missing, formulate a new query, retrieve again, repeat until answerable. |
| **Query decomposition** | Split the question into sub-questions, retrieve for each, then synthesize. |
| **Graph-based RAG** | Use a knowledge graph to traverse relationships across entities (see `08`). |

This is naturally implemented with an **agent** that has a retrieval tool and loops (agentic RAG — see `08`). Trade-offs: more retrieval/LLM calls (latency, cost) and a need for **strong termination** so it doesn't loop forever. Azure AI Search's **agentic retrieval** supports query planning/decomposition for exactly these multi-step questions — it uses an LLM to break a query into subqueries, runs them in parallel, and merges results. Multi-hop is where pure single-shot RAG breaks down and agentic patterns earn their keep.

---

## 🔭 Multi-Query Fusion

Rather than betting on one phrasing, **generate several query variants** (and/or run hybrid's two legs), retrieve a candidate set for each, then **fuse** the lists — typically with **Reciprocal Rank Fusion (RRF)**, the same rank-based fusion Azure uses for hybrid search (see `04`). Documents that rank well across multiple variants rise to the top. This broadens recall for ambiguous queries at the cost of extra retrieval calls. Pair with reranking to restore precision on the fused set.

---

## 🧰 Putting It Together: The Enhanced Query Path

```text
  User query
     │
     ▼  (gate: needed?)
  REWRITE (decontextualize for chat, expand, decompose)
     │
     ├─▶ (optional) HyDE: generate hypothetical doc, embed it
     ├─▶ (optional) multi-query: N variants
     │
     ▼
  HYBRID RETRIEVE (BM25 + vector), security-trimmed, top 30–50
     │
     ▼  (fuse if multi-query, via RRF)
  RERANK (semantic ranker / cross-encoder)
     │
     ▼
  PASS top 3–5 chunks ──▶ grounded generation
```

Each enhancement costs latency/calls, so apply selectively and measure on an eval set (see `07`). The defaults that almost always pay off: **hybrid retrieval + semantic reranking + chat query rewriting.** HyDE, multi-query, and multi-hop are situational.

---

## 🗝️ Key Takeaways

- **Reranking** (cross-encoder, joint query+doc scoring) on top of broad first-stage retrieval is often the single biggest quality lever — Azure's semantic ranker is the managed option.
- **Query rewriting** (especially decontextualizing follow-ups) is high-impact and low-cost; **it's what makes multi-turn RAG work.** Gate it for simple queries.
- **HyDE** embeds a hypothetical answer to close the query-document gap; useful when plain vector retrieval underperforms, not always needed.
- **Multi-hop** chains retrieval+reasoning for questions one search can't answer — implement with agentic RAG + strong termination; Azure AI Search agentic retrieval does query decomposition natively.
- **Multi-query fusion** broadens recall via RRF over several phrasings; rerank afterward.
- Apply enhancements selectively and measure; **hybrid + semantic rerank + chat rewriting** are the always-on defaults.
