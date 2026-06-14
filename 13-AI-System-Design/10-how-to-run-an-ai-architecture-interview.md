# How to Run an AI Architecture Interview

You'll be handed a prompt like *"Design an AI assistant that answers questions over our internal docs for 50k employees."* The most common failure is jumping straight to boxes-and-arrows. Run this **repeatable framework** instead — spend the first 5-8 minutes on requirements, and tie every decision back to the **five properties** (non-deterministic, slow, expensive, rate-limited, unsafe — see **[01](01-enterprise-ai-platform.md)**).

## The framework (45-60 min whiteboard)

```text
 1. CLARIFY REQUIREMENTS  (5-8 min)  → what, who, scale, constraints, success metric
        │   Functional: chat? agentic actions? which sources? citations? languages?
        │   Non-functional: latency (P95), QPS, freshness, accuracy bar, budget, compliance
        ▼
 2. CAPACITY & COST ESTIMATE  (4-6 min) → tokens/req × QPS = TPM; $ /day; storage; index size
        ▼
 3. HIGH-LEVEL DESIGN  (8-10 min) → gateway, RAG/agent, model, data, async vs sync
        ▼
 4. DEEP DIVE  (12-15 min) → interviewer picks one box; go deep (chunking? routing? state?)
        ▼
 5. TRADE-OFFS  (ongoing) → name the fork, state both sides, decide, justify with an NFR
        ▼
 6. FAILURE MODES  (5 min) → model down/429, hallucination, injection, cost spike, cache stampede
        ▼
 7. EVOLUTION  (3-5 min) → v1 → v2 → v3; what you'd defer; how it scales 10×
```

## Step 1 — Clarify requirements

Quantify before you draw. Functional: is it chat, or agentic actions? Which sources? Are citations required? Languages? Non-functional: latency P95 (and first-token), QPS, freshness, the accuracy bar, the budget, compliance/residency. Pin a **success metric** — "what does good look like, and who measures it?"

## Step 2 — Capacity & cost estimate (do it live)

State assumptions out loud; the panel grades reasoning, not arithmetic. The full worked example lives in **[08-cost-and-capacity](08-cost-and-capacity.md)**; the shape is:

| Quantity | Assumption | Math | Result |
|---|---|---|---|
| Active users | 50k, 10% daily, 5 queries each | 50,000 × 0.10 × 5 | 25,000 queries/day |
| Peak QPS | 8-hr day, peak = 3× avg | 25,000 / (8×3600) × 3 | ~2.6 QPS |
| Tokens/query | 4k context + 1k in + 0.5k out | — | 5.5k tokens |
| TPM at peak | 2.6 × 60 × 5.5k | — | ~860k TPM → **PTU or multi-deployment** |
| Daily spend | 137.5M tokens × ~$5/1M | — | **~$690/day ≈ $20k/mo** |

This one table proves you understand that **TPM and cost are the binding constraints**, and it justifies PTU, caching, and model routing before anyone asks.

## Step 3 — High-level design

Lay out the spine: **AI gateway (APIM)** → orchestration (RAG and/or agent) → **Azure OpenAI** fleet → data (AI Search + Cosmos + Blob) → cross-cutting (Entra ID/MI, App Insights, Content Safety, Purview). Declare **sync vs async** per path (stream chat; queue ingestion/batch). Map every box to a *named* Azure service.

## Step 4 — Deep dive

The interviewer picks one box. Be ready to go deep on any of: chunking and rerank (**[03](03-rag-system-design.md)**), model routing and PTU spillover (**[02](02-ai-gateway-and-model-routing.md)**), durable agent state and HITL (**[04](04-multi-agent-platform.md)**), eval harness and drift (**[06](06-observability-and-eval-platform.md)**), or capacity math (**[08](08-cost-and-capacity.md)**).

## Step 5 — Trade-offs (continuous)

Every major choice = "Option A vs B, and I chose X because [NFR]." Handle these forks crisply: RAG-vs-fine-tune, sync-vs-async, PTU-vs-PAYG, managed-vs-self-hosted, single-vs-multi-agent, central-vs-autonomous.

## Step 6 — Failure modes

Model 429/outage → routing + spillover + queue; hallucination → grounding + abstain + groundedness check; injection → untrusted-content handling + Prompt Shields; cost spike → quotas + budget alerts; cache stampede → locking/early refresh; idempotency so retried calls don't double-bill.

## Step 7 — Evolution

A simple, shippable **v1** and a credible path to **v2/v3**. Say what you'd *defer* and why, and how it scales 10×. You don't over-engineer.

## What senior architect interviewers listen for (8-10 signals)

1. **Requirements & cost first.** You quantified latency/QPS/budget and let *those* drive the design.
2. **AI-specific instincts.** RAG, grounding, eval, content safety, token budgets, semantic cache — reached for unprompted. You treat the model as non-deterministic/slow/expensive/rate-limited/fallible.
3. **Concrete Azure mapping.** Real services (APIM AI-gateway policies, AI Search hybrid+rerank, Azure OpenAI PTU+spillover, Content Safety, Purview, Container Apps/AKS, Service Bus vs Event Grid) — not "a gateway" and "a vector DB."
4. **Explicit trade-offs.** Every choice came with "A vs B, chose X because [NFR]."
5. **Evaluation & quality engineering.** An answer to "how do you know it works?" — golden sets, groundedness, LLM-as-judge, online sampling, drift alerts. Non-determinism doesn't scare you.
6. **Failure modes & resilience.** Concrete mitigations for each, plus idempotency so retries don't double-bill.
7. **Security & governance baked in.** Managed Identity (no keys), Private Endpoints, document-level ACL trimming, PII/DLP, audit, approval gates.
8. **Scalability with the right primitive.** Queue-based load levelling, KEDA, stateless services + externalized state, PTU floor + PAYG burst, telemetry sampling.
9. **Pragmatic evolution.** Shippable v1, credible v2/v3, you can say what you'd defer. No over-engineering.
10. **Communication & ownership.** You drive the session, narrate decisions, invite redirection, tie everything back to business value and the success metric.

## Phrases that land well

- *"My binding constraint here is TPM/cost, so I'll size for that first."*
- *"I'll start with RAG, not fine-tuning, because the knowledge changes and we need citations."*
- *"That call goes async through Service Bus — the queue is my rate limiter against 429s."*
- *"I gate this tool with HITL because it's an irreversible action; read-only tools run autonomously."*
- *"How do I know it's working? Groundedness and answer-relevance on a golden set in CI, sampled online, alerting on drift."*
- *"No standing secrets — Managed Identity end to end, Private Endpoints on Azure OpenAI and AI Search."*

## Anti-patterns that fail the loop

- Drawing boxes before clarifying requirements or estimating load.
- "The LLM will handle it" with no eval, grounding, or safety story.
- Vague vendor-agnostic mush in a Microsoft interview ("some vector database").
- No cost discussion — AI is billed per token; ignoring it reads as inexperience.
- Treating the model as deterministic/reliable; no failure modes, no fallback, no idempotency.
- Over-engineering v1 (a multi-agent mesh for what a single RAG call solves).

## One-paragraph closer to memorize

> "An enterprise AI platform is a distributed system whose hardest dependency is non-deterministic, slow, expensive, rate-limited, and occasionally unsafe. So I put a governed **AI gateway (APIM)** in front of an **Azure OpenAI** fleet with **PTU + PAYG spillover**, ground answers with **RAG on Azure AI Search** (hybrid + rerank + ACL trimming), run agentic work durably through **Service Bus**, observe every call with **OpenTelemetry GenAI semconv into App Insights** including **token/cost and quality evals**, and enforce **Responsible AI** with **Content Safety, Purview, a model registry, and approval gates** — and I size and justify all of it against **latency, cost, and quality** NFRs."
