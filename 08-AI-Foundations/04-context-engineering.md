# Context Engineering

If prompt engineering is about *how* you ask, context engineering is about *what you put in the window* — and what you leave out. The context window is a scarce, billed, attention-limited resource (see [01-what-are-llms](01-what-are-llms.md)). Filling it well is one of the highest-leverage skills in applied AI: it drives quality, cost, latency, and whether the model even sees the fact it needs. This page covers managing the window, conversation memory, summarization, and the pivotal **context vs RAG vs fine-tune** decision.

---

## The Core Principle

> More context is **not** automatically better. Relevance and *position* matter as much as raw inclusion.

Two forces work against you:
1. **Cost & latency** scale with tokens (attention is O(n²); you pay for the full context every call).
2. **"Lost in the middle"** — models attend most to the start and end of a long context, so a fact buried in the middle can be ignored.

So context engineering is a constant act of **curation**: include what's relevant, place it well, reserve room for the answer.

```text
[ system prompt ]  ← stable, cacheable, important instructions
[ key context   ]  ← most relevant retrieved chunks (top of window)
[ history / data]  ← compacted
[ the question  ]  ← restate key instructions near the END too
[ completion    ]  ← always reserve budget for this
```

---

## Budgeting the Window

Always account for **prompt + completion ≤ context window**, and leave headroom.

```text
context_window = system + few_shot + retrieved + history + question + max_tokens(output)
```

Practical rules:
- **Count tokens before sending** (`tiktoken`); trigger compaction *before* you hit the limit.
- **Reserve** enough for the completion or you'll truncate the answer.
- Track token count as a first-class metric — it's your cost dial.

---

## Managing Conversation History

A chat grows unbounded; the window doesn't. Four strategies, usually combined:

| Strategy | How | Best for |
|---|---|---|
| **Sliding window** | Keep the last N turns verbatim, drop older | Short, simple chats |
| **Summarization** | Periodically compress old turns into a running summary, prepend it | Customer support, long sessions |
| **Retrieval over history** | Embed past turns; retrieve only relevant ones for the current query | Very long conversations |
| **Entity / fact memory** | Extract key facts (name, preferences) to structured storage outside the prompt | Long-running assistants |

Always keep the **system prompt and any pinned instructions**. Choose by app: support favors summarization; long-running assistants favor external memory + retrieval.

### Summarization (compaction) pattern

```text
when token_count(history) > threshold:
    summary = LLM("Summarize the conversation so far, preserving
                   decisions, user facts, and open questions.")
    history = [summary] + recent_turns   # drop the old verbatim turns
```

Gotchas: summarization is lossy — protect critical facts by extracting them to structured memory instead of trusting a prose summary; and summarize the *older* turns while keeping recent ones verbatim for fidelity.

### External / long-term memory

Two kinds of memory for assistants and agents:

```text
Short-term  : current conversation, task state → lives in the prompt (windowed/summarized)
Long-term   : persistent facts + past interactions → lives OUTSIDE the prompt
              ├─ vector store  (semantic recall of past interactions/knowledge)
              └─ structured DB (user preferences, entities, settings)
```

At each turn, retrieve relevant long-term memories and inject them. Be deliberate — **storing everything bloats retrieval and cost**; store salient, reusable information and let retrieval surface it on demand. Microsoft tooling: Semantic Kernel memory connectors (section **10**); Azure AI Search or Cosmos DB as the backing store (section **09**).

---

## The Big Decision: Context vs RAG vs Fine-tune

This is the question interviewers ask most. The honest answer is a decision tree, not a favorite.

```text
Does the info fit comfortably in the window AND change per request?
        │
        ├─ YES → just put it in CONTEXT (stuff the prompt / pass the doc)
        │
        └─ NO ↓
Is it KNOWLEDGE (facts/docs) that's large, changing, or needs citations?
        │
        ├─ YES → RAG  (retrieve relevant chunks at query time)
        │
        └─ NO ↓
Is it a stable BEHAVIOR / FORMAT / TONE at high volume?
        │
        └─ YES → FINE-TUNE  (bake it into weights)
```

| Approach | Use when | Strengths | Watch out |
|---|---|---|---|
| **Stuff into context** | Small, per-request data; one-off docs | Zero infra, simplest | Costs tokens every call; window limits |
| **RAG** | Large/changing private knowledge; need citations & access control | Updates instantly, traceable, secure | Retrieval quality is the hard part |
| **Fine-tune** | Consistent behavior/format/tone at scale | Shrinks prompts, internalizes patterns | Stale for knowledge; can't cite; costly to refresh |

**Rules of thumb:**
- **Never fine-tune to inject knowledge** — that's RAG's job. Fine-tuned facts go stale and can't cite sources.
- The senior order: **prompt → RAG → fine-tune**. Most production tasks are solved with a good system prompt + a few examples + RAG.
- "Long context" doesn't kill RAG. Even with huge windows, dumping everything is expensive, slow, and triggers "lost in the middle." Retrieval keeps context lean and relevant. (Full RAG: section **12**.)

---

## Context Curation Tactics

- **Retrieve, don't dump.** Inject 3–5 relevant chunks, not the whole manual.
- **Rerank** so the best chunks land at the window's edges (mitigates lost-in-the-middle).
- **Deduplicate** near-identical retrieved chunks — they crowd out diverse, useful context.
- **Strip boilerplate** (headers/footers/nav) from retrieved content; it's token noise.
- **Place instructions at start *and* end** for long contexts.
- **Compact aggressively** but extract critical facts to structured memory first.

---

## Worked Example: A Support Assistant

```text
Window plan (128K model, but we keep it lean):
  system prompt ........ 400 tok   (role, "answer only from context", tone)
  running summary ...... 300 tok   (compacted older turns)
  last 4 turns ......... 600 tok   (verbatim recent context)
  retrieved chunks ..... 1,000 tok (4 × ~250, reranked, deduped)
  user question ........ 50 tok
  reserved for answer .. 500 tok   (max_tokens)
  ─────────────────────────────
  total prompt ≈ 2,350 tok  → cheap, fast, relevant
```

Versus the naive approach (dump the 40-page manual + full 30-turn history) which would be 50K+ tokens, slower, pricier, and *less* accurate.

---

## Brush-up Cheat-Sheet

```text
Principle    = curate, don't cram; relevance + position > raw volume
Budget       = system + context + history + question + output ≤ window
History       = sliding window / summarize / retrieve-over-history / fact memory
Memory        = short-term in prompt; long-term in vector + structured stores
Decision tree = fits & per-request? context | large changing knowledge? RAG | stable behavior? fine-tune
Golden rule   = never fine-tune for KNOWLEDGE; order = prompt → RAG → fine-tune
Tactics       = retrieve not dump, rerank to edges, dedupe, strip boilerplate, reserve output budget
```

**Next:** [05-tool-calling-and-agents-intro](05-tool-calling-and-agents-intro.md) — letting the model reach out to the world.
