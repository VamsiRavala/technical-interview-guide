# Enterprise AI Platform

The umbrella. Before you design any single AI feature, you design the **platform** the features live on — a paved road that 30-40 product teams consume so that governance, cost control, and reliability are solved once, centrally, rather than badly, forty times.

## The five properties that drive every AI design decision

Keep this in your head. It is the "first principles" you derive every later answer from. An enterprise AI system is a normal distributed system whose hardest dependency is the model, and the model is:

| Property of the model dependency | Architectural consequence you must design for |
|---|---|
| **Non-deterministic** | You cannot unit-test by exact-match. You need **eval harnesses**, golden datasets, LLM-as-judge, and you must log inputs/outputs for replay. Caching is *semantic*, not byte-exact. |
| **Slow** (100s of ms to tens of seconds) | **Streaming**, async patterns, queue-based load levelling, aggressive timeouts/budgets. P95 latency (and first-token) is your headline NFR. |
| **Expensive** (per-token billing) | **Cost is a first-class NFR** alongside latency and quality. Model routing (cheap model first), caching, prompt compression, token budgets per tenant. |
| **Rate-limited** (TPM/RPM quotas, PTUs) | **Gateway-level throttling, retries with backoff, load-balancing across deployments/regions, PTU→PAYG spillover.** |
| **Occasionally wrong / unsafe** | **Grounding (RAG), content safety, guardrails, HITL, observability for hallucination/drift, governance & audit.** Trust is engineered, not assumed. |

Whenever the interviewer asks "why did you do X," tie it back to one of these five.

## (a) Business context & requirements

A large enterprise — say a bank or a global retailer — has 40+ product teams that all want to "add AI." Left alone, each team independently signs up for Azure OpenAI, hard-codes keys, invents its own prompt logging, and blows the org's token budget with no chargeback. The **Enterprise AI Platform** is the paved road: a central team owns a **gateway, shared model fleet, shared RAG/embedding services, observability, and governance**, and product teams consume it via a self-service API + SDK.

**Functional requirements**
- Single endpoint to call any approved model (Azure OpenAI GPT-4o, o-series, embeddings, plus optionally self-hosted/OSS models).
- Model **routing & fallback** (route by cost/latency/quality; fail over across deployments and regions).
- **Multi-tenancy**: per-team API keys/identities, quotas, and **cost chargeback**.
- Shared **prompt/template registry**, shared **RAG-as-a-service**, shared **agent runtime**.
- Centralized **observability** and **governance/policy enforcement**.

**Non-functional requirements**

| NFR | Target (example you can defend) |
|---|---|
| Availability | 99.9% for the gateway (it is on the critical path of every product) |
| Latency overhead | Gateway adds < 30 ms P95 on top of model latency |
| Throughput | 5,000 RPM aggregate, burst to 15,000 |
| Cost governance | Hard per-tenant token caps + soft alerts at 80% |
| Security | No standing secrets; Entra ID + Managed Identity everywhere; data residency per region |
| Auditability | 100% of prompts/responses logged with tenant, model, cost, latency |

## (b) Architecture diagram

```text
                          ENTERPRISE AI PLATFORM (umbrella)
 ┌──────────────────────────────────────────────────────────────────────────────┐
 │  Product Teams (BFFs / apps)        Entra ID (OAuth2, app roles, Managed Id)   │
 │        │  │  │                                  ▲                              │
 │        ▼  ▼  ▼                                  │ token validation             │
 │  ┌───────────────────────── AI GATEWAY (Azure API Management) ───────────────┐ │
 │  │  • AuthN/Z (validate-jwt)   • Per-tenant quota & rate-limit (TPM/RPM)      │ │
 │  │  • Token-metric + cost emit • Semantic cache  • PII redaction policy hook  │ │
 │  │  • Model ROUTER: pick deployment by {model, cost, latency, region, health}│ │
 │  │  • Retry/backoff, circuit-break, PTU→PAYG spillover, load-balance backends│ │
 │  └───────┬───────────────┬───────────────┬──────────────────┬───────────────┘ │
 │          ▼               ▼               ▼                  ▼                 │
 │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐   ┌──────────────────┐     │
 │  │ Azure OpenAI│  │ Azure OpenAI│  │ Self-hosted │   │ SHARED SERVICES  │     │
 │  │  East US    │  │  West EU    │  │ model on    │   │ • RAG-as-a-svc   │     │
 │  │ (PTU+PAYG)  │  │ (failover)  │  │ AKS (GPU)   │   │ • Embedding svc  │     │
 │  └─────────────┘  └─────────────┘  └─────────────┘   │ • Agent runtime  │     │
 │                                                       │ • Prompt registry│     │
 │   Cross-cutting (consumed by all):                    └──────────────────┘     │
 │   App Insights / Log Analytics  •  Azure AI Content Safety  •  Microsoft Purview│
 │   Key Vault  •  Cosmos DB (config/state)  •  Azure Cache for Redis (sem. cache) │
 └──────────────────────────────────────────────────────────────────────────────┘
```

## (c) Component-by-component walkthrough

- **AI Gateway = Azure API Management (APIM).** The keystone of the Microsoft story. APIM has first-class **GenAI policies**: `azure-openai-token-limit` (enforce TPM per tenant), `azure-openai-emit-token-metric` (push token usage to App Insights), `azure-openai-semantic-cache-lookup/store` (Redis-backed semantic cache), and Content Safety integration. It load-balances and applies a **circuit breaker** across multiple Azure OpenAI backends, and does **PTU → pay-as-you-go spillover** via backend pools with priority/weight. (Detail in **[02-ai-gateway-and-model-routing](02-ai-gateway-and-model-routing.md)**.)
- **Model fleet = Azure OpenAI** deployments across regions (one region primary on **Provisioned Throughput Units** for predictable latency, others as PAYG for burst/failover), plus optional **self-hosted OSS models on AKS with GPU node pools** (or Azure AI Foundry serverless/managed endpoints) for models with data-residency or cost constraints.
- **Identity = Microsoft Entra ID** with app registrations per consuming team; **Managed Identity** for service-to-service so there are **no API keys in code**. The unavoidable secret lives in **Azure Key Vault**.
- **Shared services**: RAG-as-a-service (**[03](03-rag-system-design.md)**), a centralized **embedding service** (so every team uses the same embedding model/version — critical, because mixing embedding versions silently corrupts a shared vector index), an **agent runtime** (**[04](04-multi-agent-platform.md)**), and a **prompt/template registry** (versioned prompts in Cosmos DB or git, served via the gateway).
- **Cross-cutting**: **App Insights + Log Analytics** (observability, **[06](06-observability-and-eval-platform.md)**), **Azure AI Content Safety** (guardrails, **[07](07-governance-and-responsible-ai-platform.md)**), **Microsoft Purview** (data classification, lineage, DLP), **Azure Cache for Redis** (semantic cache + session state), **Cosmos DB** (config, routing rules, tenant metadata).

## (d) Scalability strategies

- **Horizontal**: APIM Premium scale-units + multi-region; backends are a **pool** so adding an Azure OpenAI deployment is a config change, not code.
- **Quota-aware load balancing**: spread load across deployments by remaining TPM, not round-robin; honor `retry-after` headers and fail to the next backend on 429.
- **PTU for the floor, PAYG for the spikes**: reserved throughput gives predictable P95; spillover absorbs bursts.
- **Semantic caching** in front of the model collapses duplicate questions (FAQ traffic) — can cut 20-40% of calls in support scenarios.
- **Stateless gateway**; all state (sessions, cache) externalized to Redis/Cosmos so the gateway scales freely.

## (e) Security patterns

- **Zero standing secrets**: Entra ID + Managed Identity end to end; Key Vault for the unavoidable.
- **Network isolation**: Azure OpenAI and AI Search behind **Private Endpoints**; public network access disabled; APIM in a VNet (internal/external mode).
- **Per-tenant isolation**: app roles + subscription keys scoped per team; quotas prevent one tenant starving others (noisy-neighbor).
- **Defense in depth**: PII redaction policy at the gateway *before* the prompt leaves your boundary; Content Safety on input and output; Purview DLP on retrieved documents.

## (f) What belongs in the platform vs the team

Use a **differentiation test**: undifferentiated heavy lifting that is risky to get wrong belongs in the platform; anything encoding product-specific business value belongs to the team.

| Platform-owned (paved road) | Team-owned (their value) |
|---|---|
| AI gateway, auth, rate-limit, quota | Domain prompts and prompt-engineering |
| Content safety enforcement, prompt-injection defense | Retrieval indexes and chunking strategy |
| Model routing, cost attribution, chargeback | Evaluation datasets and golden sets |
| Observability pipeline, red-team tooling | Agent tool definitions and UX |

The rule: the platform must be **opt-in-easy and hard-to-bypass**. Provide a golden-path .NET SDK so the easy path is also the compliant path. Run the platform as an internal product with an SLA and a roadmap, not a gatekeeping committee — friction is the enemy of governance, and friction breeds **shadow AI**.

## (g) Trade-offs

| Decision | Option A | Option B | When to pick which |
|---|---|---|---|
| **Centralized gateway vs direct SDK** | Central APIM gateway (governance, chargeback, one throat to choke) | Teams call Azure OpenAI directly (lower latency, less central bottleneck) | Pick gateway for enterprise (governance is the whole point); accept the ~10-30 ms overhead. Direct only for ultra-latency-sensitive internal tools that still report telemetry. |
| **PTU vs pay-as-you-go** | PTU: predictable latency, fixed cost, no 429s at your reserved level | PAYG: zero commit, pays per token, subject to shared-pool throttling | PTU for steady high-volume production; PAYG for spiky/experimental. Real answer = **both**: PTU floor + PAYG spillover. |
| **Managed (Azure OpenAI) vs self-hosted OSS on AKS** | Managed: no ops, latest models, built-in safety, SLA | Self-hosted: data never leaves your cluster, full control, fixed GPU cost | Managed by default. Self-host only when residency/compliance forbids the managed service or a fine-tuned OSS model is materially cheaper at your volume — and budget for GPU ops + your own scaling. |
| **Platform velocity vs control** | Push everything into the platform: consistent, governed, can bottleneck | Push little: agile teams, inconsistent security, duplicated cost | Centralize *enforcement*, decentralize *authoring*. The platform is opt-in-easy and hard-to-bypass, or teams route around you and you lose governance entirely. |

> **One-line closer:** "The platform is a distributed system whose hardest dependency is non-deterministic, slow, expensive, rate-limited, and occasionally unsafe — so I put a governed APIM gateway in front of an Azure OpenAI fleet, share RAG/embedding/agent services, and make the compliant path the easy path."
