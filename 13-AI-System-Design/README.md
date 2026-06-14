# AI System Design — Architecting Enterprise GenAI on Azure

> An enterprise AI system is a normal distributed system where one dependency is **non-deterministic, slow, expensive, rate-limited, and occasionally unsafe.** Almost every AI-specific design decision flows from those five properties. This section teaches the architecture layer that sits on top of the classic distributed-systems toolkit — the **AI gateway, model fleet, RAG, agents, observability, governance, cost, and reliability** patterns — and how to defend them in a Microsoft AI Solutions Architect loop. Microsoft/Azure-centric, accurate to **mid-2026**.

---

## 📚 Table of Contents

### Foundations

1. **[Enterprise AI Platform](01-enterprise-ai-platform.md)** - The umbrella platform: the paved road, shared model fleet, shared RAG/embedding/agent services, identity, and the five properties that drive every decision
2. **[AI Gateway & Model Routing](02-ai-gateway-and-model-routing.md)** - APIM as the GenAI gateway, token-based quotas, model routing, PTU→PAYG spillover, multi-backend failover, and semantic caching

### Knowledge & Agents

3. **[RAG System Design](03-rag-system-design.md)** - Ingestion → index → retrieve → ground at scale: chunking, hybrid + rerank, security trimming, freshness, and continuous eval
4. **[Multi-Agent Platform](04-multi-agent-platform.md)** - Orchestration patterns, agent/tool registries, durable state, HITL approval gates, budgets, and prompt-injection defense

### Async & Operations

5. **[Event-Driven AI](05-event-driven-ai.md)** - Async ingestion and task queues, Event Grid vs Service Bus, KEDA, sagas, idempotency, and the queue as rate limiter
6. **[Observability & Eval Platform](06-observability-and-eval-platform.md)** - LLM tracing, token/cost metrics, quality/eval metrics, OpenTelemetry GenAI semconv, App Insights, and alerting on drift/hallucination
7. **[Governance & Responsible-AI Platform](07-governance-and-responsible-ai-platform.md)** - Policy enforcement, Content Safety, PII/DLP, model registry, approval gates, immutable audit, and Microsoft Purview

### Economics & Resilience

8. **[Cost & Capacity](08-cost-and-capacity.md)** - Capacity math (TPM), PTU vs PAYG break-even, caching ROI, model routing, cost monitoring and chargeback
9. **[Scalability & Reliability](09-scalability-and-reliability.md)** - Throughput against a quota ceiling, 429 handling, fallback chains, graceful degradation, multi-region, and SLAs for non-deterministic systems

### Interview Preparation

10. **[How to Run an AI Architecture Interview](10-how-to-run-an-ai-architecture-interview.md)** - The repeatable framework (clarify → capacity/cost → high-level → deep dive → trade-offs → failure modes → evolution), worked capacity estimate, and the signals interviewers listen for
11. **[Interview Questions](11-interview-questions.md)** - 100 detailed Q&A: 50 AI Architect + 50 System Design

---

## 🎯 Learning Path

### Beginner (Week 1-2)
1. Read **Enterprise AI Platform** and internalize the five properties (non-deterministic, slow, expensive, rate-limited, unsafe) — derive every later answer from them
2. Work through **AI Gateway & Model Routing** — understand why APIM sits in front of Azure OpenAI and name the actual GenAI policies
3. Read **RAG System Design** — trace ingestion and query paths end to end and explain why security trimming happens at retrieval, not in the prompt

### Intermediate (Week 3-4)
1. Build mental models for **Multi-Agent Platform** and **Event-Driven AI** — durable orchestration, HITL by risk tier, and the queue as a rate limiter against 429s
2. Study **Observability & Eval Platform** — OpenTelemetry GenAI semconv, token/cost telemetry, online eval, and drift alerting
3. Study **Governance & Responsible-AI Platform** — Content Safety, Purview DLP, model registry, and approval gates as architecture

### Advanced (Week 5-6)
1. Master **Cost & Capacity** and **Scalability & Reliability** — PTU vs PAYG break-even, capacity math, fallback chains, and SLAs vs SLOs for non-deterministic systems
2. Internalize **How to Run an AI Architecture Interview** — run the framework end to end on a fresh prompt, out loud, with a live capacity estimate
3. Drill **Interview Questions** — rehearse the trade-off forks (RAG vs fine-tune, PTU vs PAYG, sync vs async, central vs autonomous) until they are reflexive

---

## 🔑 The Five Properties That Drive Every Decision

| Property of the model dependency | Architectural consequence you must design for |
|---|---|
| **Non-deterministic** | Eval harnesses, golden sets, LLM-as-judge, replay logging. Caching is *semantic*, not byte-exact. |
| **Slow** (100s ms → tens of s) | Streaming, async/queues, aggressive timeouts and budgets. P95 (and first-token) is the headline NFR. |
| **Expensive** (per-token) | Cost is a first-class NFR. Model routing, caching, prompt compression, token budgets per tenant. |
| **Rate-limited** (TPM/RPM, PTUs) | Gateway throttling, retry/backoff, load-balance across deployments/regions, PTU→PAYG spillover. |
| **Occasionally wrong / unsafe** | Grounding (RAG), Content Safety, guardrails, HITL, observability for hallucination/drift, governance & audit. |

---

## 💡 Key Decisions at a Glance

| Question | Short answer |
|---|---|
| **Front door to the model?** | **APIM as AI gateway** — token quotas, semantic cache, load balance, circuit break, Content Safety. Non-negotiable for multi-team. |
| **Reserved or on-demand?** | **PTU floor + PAYG spillover.** PTU for the proven steady baseline; PAYG for burst/experiments. Never buy PTU on a forecast alone. |
| **Knowledge vs behavior?** | **RAG** for facts that change and need citations (start here). **Fine-tune** for format/tone, not facts. They compose. |
| **Sync or async?** | Sync + streaming for interactive chat; **async via Service Bus** for ingestion, batch, long agent tasks. The queue is your rate limiter. |
| **Single agent or multi?** | Start single-agent + tools. Split to multi-agent only when the task clearly decomposes. Complexity has real cost. |
| **How do you know it works?** | Golden sets, groundedness, LLM-as-judge in CI; sampled online; **alert on drift**. Non-determinism doesn't scare you. |
| **Secrets?** | **Zero standing secrets** — Entra ID + Managed Identity end to end, Private Endpoints on Azure OpenAI and AI Search, Key Vault for the unavoidable. |

> **Where this fits:** Semantic Kernel's API surface is section **10**; Azure OpenAI / AI Search infrastructure depth is section **09**. This section is the **architecture layer** — how the boxes fit, scale, fail, and pay for themselves at enterprise scale.
