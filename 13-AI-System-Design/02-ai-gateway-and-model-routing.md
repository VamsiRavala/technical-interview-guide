# AI Gateway & Model Routing

The single most important box in any enterprise GenAI architecture. A direct app-to-Azure-OpenAI connection gives you no central control, so every cross-cutting concern gets reimplemented per team — or skipped. The **AI gateway centralizes** authentication, quota, cost attribution, reliability, caching, and safety. On the Microsoft stack the gateway is **Azure API Management (APIM)** with its first-class GenAI policies.

## (a) Requirements

**Functional**
- One stable, model-agnostic endpoint for many apps/teams; swap the backend model without breaking consumers.
- **Token-based** rate limiting and per-tenant quota (not just request count — tokens are what cost money).
- **Cost attribution**: emit token metrics tagged by tenant/feature/model.
- **Model routing**: pick the cheapest deployment that satisfies the request; cascade/fallback on quality or failure.
- **Load balancing & failover** across deployments/regions, with **PTU→PAYG spillover**.
- **Semantic caching** to cut cost and latency for repetitive traffic.
- **Content Safety** hooks on input and output.

**Non-functional**

| NFR | Target |
|---|---|
| Gateway availability | 99.95% (zone/region-redundant, Premium tier) |
| Added latency | < 30 ms P95 over model latency |
| Failover time | Backend out of rotation within seconds of a 429/5xx burst |
| Cost accuracy | Per-tenant token accounting within a few % of billed tokens |

## (b) Architecture diagram

```text
  Apps / BFFs ──▶ Azure API Management (AI GATEWAY) ──────────────────────────────┐
                    │                                                             │
   1. validate-jwt (Entra ID)            policies (declarative, per product):     │
   2. azure-openai-token-limit  ◀── per-tenant TPM ceiling, returns 429 + retry   │
   3. semantic-cache-lookup     ◀── Redis vector store; HIT → return, skip model  │
   4. emit-token-metric         ──▶ App Insights (tenant, feature, model dims)    │
   5. content-safety (input)    ◀── Prompt Shields / harm categories              │
   6. ROUTE to backend pool ────────────────────────────┐                         │
                    │                                    ▼                         │
            ┌───────┴────────────── BACKEND POOL ─────────────────────┐           │
            │  priority 1  east-us-PTU     (baseline, latency-guarded) │           │
            │  priority 1  west-eu-PTU     (baseline, weighted)        │           │
            │  priority 2  east-us-PAYG    (burst / spillover)         │           │
            │  priority 3  fallback model  (degraded but available)    │           │
            │  circuit-breaker: trip on 3x 429/5xx in 30s, cooldown 60s│           │
            └──────────────────────────────────────────────────────────┘          │
                    │  ◀── on success: semantic-cache-store, content-safety(output) ┘
                    ▼
              streamed / buffered response → app
```

## (c) Component-by-component walkthrough

- **Authentication & contract.** APIM validates the Entra ID token (`validate-jwt`), resolves the tenant from the *validated token* (never from a request body field), and presents a single stable API surface so backend model changes don't ripple to consumers.
- **Token-based rate limiting** (`azure-openai-token-limit`). Meters by *tokens*, not request count, attached per APIM product/subscription so each tenant has its own TPM ceiling. `estimate-prompt-tokens="true"` enforces the limit *before* the backend call (saving a wasted call); reconcile estimate vs actual completion tokens via the emitted metric.

  ```xml
  <azure-openai-token-limit
      counter-key="@(context.Subscription.Id)"
      tokens-per-minute="10000"
      estimate-prompt-tokens="true"
      remaining-tokens-header-name="x-tokens-remaining" />
  <azure-openai-emit-token-metric namespace="genai">
      <dimension name="tenant" value="@(context.Subscription.Id)" />
      <dimension name="model"  value="@(context.Request.MatchedParameters["deployment"])" />
  </azure-openai-emit-token-metric>
  ```

- **Semantic cache** (`azure-openai-semantic-cache-lookup/store`). Embeds the request and does a vector-similarity lookup against Redis (or AI Search); on a near-match above threshold it returns the cached answer and skips the model entirely. The **similarity threshold is a precision/recall dial** — too loose serves a confidently-wrong answer to a subtly different question; too tight collapses the hit rate. Scope caches per tenant/user, exclude authenticated/time-sensitive prompts, attach TTLs, and monitor *hit quality*, not just hit rate.
- **Cost telemetry** (`azure-openai-emit-token-metric`). Pushes prompt + completion token counts tagged by tenant/feature/model into App Insights — the raw material for chargeback (**[08](08-cost-and-capacity.md)**).
- **Model router.** Routes on request difficulty: cheap small models (GPT-4o-mini class) for extraction/classification/short tasks; mid-tier for standard reasoning; frontier models only for hard reasoning, long context, or high-stakes output. Pair with **cascade/fallback** — try cheap first, escalate only when a confidence or validation check fails. The router is centralized in the gateway/orchestrator so the policy is auditable. A logical name (`chat-default`) maps to a concrete deployment/version in config, so swapping a model is a config change, not a code change across dozens of consumers.
- **Backend pool + circuit breaker.** Multiple Azure OpenAI deployments with priorities and weights: priority 1 = PTU (cheapest at utilization, latency-guaranteed); priority 2 = PAYG spillover; lower priorities = secondary region / fallback model. A circuit breaker trips a backend out of rotation on repeated failures/429s and re-probes after a cooldown, so traffic shifts to healthy backends without per-app retry logic.

## (d) Scalability strategies

- **PTU floor + PAYG burst** as the routing backbone — predictable latency on the floor, elastic spillover for spikes.
- **Quota-aware balancing** by *remaining TPM*, not round-robin; honor `Retry-After` and fail to the next backend on 429.
- **Semantic cache** removes whole calls for FAQ-shaped traffic.
- **Stateless gateway**, scaled by APIM units; all state (cache, counters) externalized to Redis.
- **Scope backend pools per residency boundary** so cross-region balancing never violates data sovereignty.

## (e) Security patterns

- **Single ingress, no bypass.** Force all AI traffic through the gateway; block direct Azure OpenAI access via network rules + RBAC so quota, cost, and safety can't be skipped.
- **Managed Identity** from APIM to Azure OpenAI (`Cognitive Services OpenAI User`); no keys in policies or apps.
- **Tenant resolved from the validated token**, never a client-supplied field.
- **Private Endpoints** on the backends; APIM in a VNet.

## (f) Trade-offs

| Decision | Option A | Option B | Guidance |
|---|---|---|---|
| **Gateway vs direct** | Central gateway: governance, cost, reliability, one place to enforce | Direct app→AOAI: lower latency, no SPOF | Gateway for any multi-team estate; deploy it HA/multi-region so the SPOF risk is mitigated. Direct only for niche ultra-latency tools that still report telemetry. |
| **Token-based vs request-based limiting** | Token limit: accurate cost control, reflects real capacity | Request count: simple, imprecise | Token-based — it's what Azure OpenAI capacity and billing actually consume. Accept the extra complexity of estimating/reconciling streamed tokens. |
| **Model routing vs single default** | Routing: cheapest-sufficient model per request, big savings at volume | One good default model: simpler, no misroute risk | Start with one default. Add routing only when telemetry shows a large, separable population of cheap-to-serve requests — misrouting a hard problem to a weak model degrades quality *silently*, so require per-route evals. |
| **Semantic cache aggressiveness** | Loose threshold: high hit rate, big savings | Tight threshold: safe, lower hit rate | Conservative threshold + per-tenant scoping + TTL + exclude personalized/authenticated prompts. Monitor hit *quality* via evals — a confidently-wrong cache hit is a trust failure, not a saving. |

> **Phrase that lands:** "Everything points through APIM at a stable logical model name, so PTU-first / PAYG-burst, circuit breaking, token quotas, semantic cache, and Content Safety are gateway *configuration* — consumers get one resilient endpoint while I manage a fleet behind it."
