# AI Foundations — The Core Mental Models for Building with LLMs

> The vocabulary, mechanics, and judgment a production engineer needs before touching RAG, agents, or Azure AI — what LLMs actually do, how to prompt and evaluate them, and how to ship them safely and cheaply.

---

## 📚 Table of Contents

### Core Model Concepts

1. **[What Are LLMs?](01-what-are-llms.md)** - Transformers, attention, tokens, context windows, temperature/top-p, decoding, and model families at a usable level
2. **[Embeddings & Vectors](02-embeddings-and-vectors.md)** - Embeddings, similarity metrics, ANN/HNSW concepts, and how semantic search actually works

### Working With the Model

3. **[Prompt Engineering](03-prompt-engineering.md)** - System prompts, few-shot, chain-of-thought, structured/JSON output, and the patterns that actually work
4. **[Context Engineering](04-context-engineering.md)** - Managing the context window, conversation memory, summarization, and the context-vs-RAG-vs-fine-tune decision
5. **[Tool Calling & Agents (Intro)](05-tool-calling-and-agents-intro.md)** - Function/tool calling mechanics, the ReAct pattern, and workflows vs agents at a high level

### Quality, Safety & Operations

6. **[Evaluation Basics](06-evaluation-basics.md)** - Offline vs online eval, LLM-as-judge, groundedness, golden sets, regression testing, and A/B
7. **[Responsible AI](07-responsible-ai.md)** - Hallucination, bias, PII, jailbreaks, prompt injection, content safety, and layered guardrails
8. **[LLMOps Basics](08-llmops-basics.md)** - Latency, streaming, caching, cost control, rate limits/429s, and observability

### Interview Preparation

9. **[Interview Questions](09-interview-questions.md)** - ~60 detailed Q&A spanning LLM fundamentals, prompting, evaluation, LLMOps, responsible AI, and vectors

---

## 🎯 Learning Path

### Beginner (Week 1-2)
1. Read **What Are LLMs?** to build correct mental models (tokens, context, sampling)
2. Read **Embeddings & Vectors** so semantic search stops being magic
3. Skim **Prompt Engineering** and practice system prompts + structured output

### Intermediate (Week 3-4)
1. Work through **Context Engineering** — when context, RAG, or fine-tuning is the right tool
2. Learn **Tool Calling & Agents (Intro)** — the loop, ReAct, workflow vs agent
3. Start **Evaluation Basics** — build a tiny golden set for something you've prompted

### Advanced (Week 5-6)
1. Master **Responsible AI** — prompt injection, jailbreaks, content safety, guardrail layers
2. Internalize **LLMOps Basics** — latency, cost, 429 handling, observability
3. Drill **Interview Questions** and rehearse the "RAG vs fine-tune vs agent" decision out loud

---

## 🔑 Key Concepts at a Glance

| Concept | One-line takeaway |
|---|---|
| **Token** | The billing/limit unit (~4 chars EN). Count before you send. |
| **Context window** | Scarce, billed scratch space (prompt + completion). Not free. |
| **Temperature** | Low = factual/deterministic; high = creative. Tune one, not both. |
| **Embedding** | A vector encoding meaning; nearby vectors = similar meaning. |
| **Hallucination** | A feature of probabilistic generation. Manage it, don't expect zero. |
| **Tool calling** | The model *requests* a call; your code *executes* it (the security boundary). |
| **Groundedness** | Is every claim backed by the provided sources? The anti-hallucination metric. |
| **Prompt injection** | The LLM's SQL-injection. Can't be solved by prompting alone. |

---

## 💡 Choosing Your Approach: Prompt vs RAG vs Fine-tune

| Need | Reach for | Why |
|---|---|---|
| Better format, tone, or instructions | **Prompting** | Cheapest, fastest to iterate, no infra |
| Current/private *knowledge* with citations | **RAG** | Updates instantly, traceable, access-controlled |
| Stable *behavior/format* at high volume | **Fine-tuning** | Bakes in the pattern, shrinks prompts |
| Live data or actions (DB, API) | **Tool calling** | Bridges reasoning to real systems |

**The senior order of operations:** prompt first → add RAG for knowledge → fine-tune only if behavior/cost still demands it. Fine-tuning is overweighted by candidates and underused in enterprise.

---

> **Where the deep dives live:** This section is the foundation. RAG end-to-end is section **12**, Azure OpenAI / Azure AI Search / content filters are section **09**, Semantic Kernel is section **10**, and agent patterns (planner-executor, multi-agent, reflection) are section **11**. This section mentions those Microsoft tools by name but intentionally keeps them shallow.
