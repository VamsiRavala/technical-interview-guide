# Memory & State

"Memory" in an agent system is not the model magically remembering — it is **plumbing**: managing the conversation window, persisting facts externally, and retrieving the right slice at the right time. MAF formalizes this into three tiers, plus serializable thread state for durability.

> The vector-store mechanics (embeddings, `Microsoft.Extensions.VectorData`, Azure AI Search connector, hybrid search) are covered in depth in section 10 and the RAG section. This file is about how *agents* use memory and persist state. (verify current GA status / API names) on memory-provider names — this is an actively evolving area.

---

## The three memory tiers

| Tier | What it is | Where it lives | Bounded by |
|---|---|---|---|
| **Short-term / conversation** | The current thread's message history | The `AgentThread` (in-process or service-managed) | Context window — trim/summarize |
| **Working / user / session** | Facts gathered this session (preferences, entities, prior decisions) | A memory component that extracts and re-injects salient facts | Session lifetime |
| **Long-term** | Durable knowledge across sessions | A vector store (`Microsoft.Extensions.VectorData`) or structured store, retrieved via RAG/tools | Storage; relevance of retrieval |

The distinction matters for **cost and relevance**: you don't keep everything in the prompt. You hold the recent window in the thread, maintain session facts via a memory provider, and offload durable knowledge to an external store retrieved on demand.

---

## Short-term: the thread, and keeping it bounded

The thread's message history is the short-term memory. It grows every turn (plus tool results), eventually exceeding the context window and inflating cost and latency. Strategies to control it:

- **Truncation** — keep the system message plus the last N turns (a sliding window).
- **Summarization** — periodically replace older turns with a model-generated summary, preserving gist while shrinking tokens (an extra model call, but coherent).
- **Selective retention** — pin important facts, drop chit-chat.
- **Retrieve instead of stuff** — offload long-term facts to a vector store and retrieve on demand rather than keeping everything in history.

```python
# Summarize older turns into a compact note, keep the tail verbatim
from agent_framework import ChatMessage, Role
async def compress(history, keep_tail=6):
    head, tail = history[:-keep_tail], history[-keep_tail:]
    summary = await summarizer.run("Summarize concisely:\n" + "\n".join(m.text for m in head))
    return [ChatMessage(Role.SYSTEM, "Conversation so far: " + summary.text)] + tail
```

Mismanaging history is a classic production bug — runaway token cost and "context lost" complaints. Apply reduction *before* each model call.

---

## Long-term: vector memory and agentic RAG

Long-term knowledge persists as embeddings in a vector store (Azure AI Search, Cosmos DB, Redis, Postgres/pgvector) and is retrieved on demand. The modern, agentic approach is to expose retrieval as a **tool** the agent calls — so the model retrieves only when it needs grounding, with self-formulated (and refinable) queries — rather than always front-loading context.

```csharp
// Vector memory: embed -> upsert -> semantic search -> ground the answer
using Microsoft.Extensions.VectorData;          // (verify current GA status / API names)

var embeddingGen = azureClient.GetEmbeddingClient("text-embedding-3-large")
                              .AsIEmbeddingGenerator();

ReadOnlyMemory<float> qVec = (await embeddingGen.GenerateAsync(["password reset policy"]))[0].Vector;

var hits = await vectorStore.GetCollection<string, PolicyDoc>("policies")
                            .SearchAsync(qVec, top: 3)
                            .ToListAsync();

string grounding = string.Join("\n", hits.Select(h => h.Record.Text));
await agent.RunAsync($"Using ONLY this policy text, answer:\n{grounding}\n\nQ: {userQ}", thread);
```

Exposing retrieval as a `[KernelFunction]`/`AIFunction` (agentic RAG) improves both **cost** (no retrieval on chit-chat) and **quality** (the model formulates better queries than raw user text and can re-search). On Azure, prefer hybrid search (vector + keyword) and semantic ranking for recall, enforce grounding/citations to reduce hallucination, and apply security trimming so retrieval only returns documents the user may see.

---

## Cross-session long-term memory

To make an agent remember a user across sessions, a **memory provider/component** extracts salient information from conversations (preferences, decisions, entities) and stores it durably — typically as embeddings keyed by user. On a new session, relevant memories are retrieved (semantic search on the user's context) and injected into the agent's context or exposed via a memory tool.

```python
# conceptual  (verify current GA status / API names)
memory.add(user_id, "Prefers concise answers and metric units")
# next session
relevant = memory.search(user_id, current_topic)
agent = ChatAgent(client, instructions, context=relevant)
```

Design decisions that separate done-well from done-poorly:
- **What to remember** — avoid hoarding noise; store salient, reusable facts.
- **When to extract/summarize** — and when to expire stale facts.
- **Retrieval relevance** — surface the right slice, not everything.
- **Privacy/consent** — memories may be personal data: encryption, access control, deletion rights.

Done well it enables personalization and continuity; done poorly it leaks privacy or pollutes context.

---

## Thread persistence: durable suspend and resume

Because threads encapsulate conversation state and are **serializable**, you can checkpoint and resume — essential for HITL (awaiting a human for minutes/days) and long-running tasks. The pattern: serialize the thread (and any workflow state) to durable storage after each turn, keyed by session/user; deserialize and continue on the next request, surviving process restarts and stateless web hosting.

```python
state = thread.serialize()          # persist to Cosmos / Redis / blob
# ... later / different instance ...
thread = agent.deserialize_thread(state)
await agent.run(next_input, thread=thread)
```

For graph-based **workflows**, MAF supports checkpointing so an entire multi-step/multi-agent run can pause (e.g., at an approval node) and resume from the checkpoint. Considerations: apply history reduction so persisted state stays bounded; store securely (encryption, access control); version the state schema; and set retention/expiry. This durability is what turns ephemeral chat loops into reliable, long-running business processes — a key enterprise differentiator. (See section 05 for the HITL flow that depends on this.)

---

## RAG vs. fine-tuning for "memory"

A common interview probe: use **RAG** for fresh, auditable, cheap-to-update *facts* (you update the knowledge base, not the model); use **fine-tuning** for *behavior/format/style*, not facts. They solve different problems — don't fine-tune to teach the model new facts when retrieval is cheaper, faster, and attributable.

---

## What interviewers probe

- *Distinguish the three memory tiers and give a store for each.* Conversation = the thread/window; working = a session memory provider; long-term = a vector store (Azure AI Search) or structured store keyed by user.
- *How do you stop context from overflowing?* Summarize-and-trim, sliding window, retrieve instead of stuff.
- *Why a managed vector store over rolling your own?* HNSW indexing, hybrid + semantic ranking, security trimming, scale, Azure RBAC/private endpoints.
- *How do you resume a long-running conversation reliably?* Serialize thread (and workflow checkpoint) state to durable storage; rehydrate on callback — don't hold an HTTP request/thread open.
- *RAG vs. fine-tuning for memory?* RAG for fresh/auditable facts; fine-tuning for behavior/format.
- *Privacy concerns with long-term memory?* It may be personal data — encryption, access control, deletion rights, staleness handling, and consent.
