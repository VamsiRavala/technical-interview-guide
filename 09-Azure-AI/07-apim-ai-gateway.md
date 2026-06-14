# APIM GenAI Gateway

> Azure API Management (APIM) as a single managed entry point between all your apps and your Azure OpenAI backends — the **GenAI gateway** pattern. It turns scattered, ungoverned API calls into a governed, observable service: token quotas, metering, semantic caching, content safety, and multi-backend load balancing/failover.

---

## Why a GenAI Gateway

When many teams/microservices share Azure OpenAI capacity, uncoordinated clients collectively blow TPM/RPM and cause cascading 429s, with no cost attribution and inconsistent security. A gateway centralizes the control plane:

```text
   ┌─ App / Team A (subscription key / JWT)
   ├─ App / Team B
   └─ App / Team C
            │
            ▼
   ┌─────────────────────────────────────────────┐
   │            APIM  (GenAI gateway)              │
   │  • auth (clients) + managed identity (backend)│
   │  • token-limit + quota per consumer           │
   │  • emit-token-metric (chargeback)             │
   │  • semantic cache                             │
   │  • content-safety policy                      │
   │  • backend pool: load balance + failover      │
   └──────┬───────────────┬───────────────┬────────┘
          ▼               ▼               ▼
   AOAI region 1    AOAI region 2    AOAI PAYG (spillover)
   (PTU baseline)   (PTU)            (bursts)
```

**Benefits:** consistent governance/security in one place, smoother 429 handling (spread load + retry across backends), **per-team cost chargeback**, and the ability to add/swap backends without touching every app.

---

## The GenAI Policies

APIM ships purpose-built policies for LLM traffic (Microsoft publishes reference policies/labs for this pattern).

### `azure-openai-token-limit` (token-based rate limiting)
Enforces a **tokens-per-minute** (and optionally request) budget **per consumer** (per subscription key / tenant / app), so one consumer can't starve others. Critical for multi-tenant fairness — limiting by request count alone is wrong because requests vary wildly in token cost.

```xml
<azure-openai-token-limit
    counter-key="@(context.Subscription.Id)"
    tokens-per-minute="50000"
    estimate-prompt-tokens="true"
    remaining-tokens-header-name="x-tokens-remaining" />
```

### `azure-openai-emit-token-metric` (metering / chargeback)
Emits **token-usage metrics** (prompt/completion/total) to Application Insights, dimensioned by consumer/app/model — the basis for **per-tenant cost attribution and chargeback**.

```xml
<azure-openai-emit-token-metric namespace="genai">
  <dimension name="Consumer" value="@(context.Subscription.Name)" />
  <dimension name="Deployment" value="@(context.Request.MatchedParameters["deployment-id"])" />
</azure-openai-emit-token-metric>
```

### `azure-openai-semantic-cache-lookup` / `-store`
**Semantic caching**: embed the incoming query, and if a previous query is semantically close enough (above a similarity threshold), return the cached answer — catching paraphrases, not just exact matches. Cuts cost and latency for repeated/similar queries. Tune the threshold and scope by user/context to avoid false hits; invalidate when source data/prompts change. (Backed by an embeddings deployment + a vector store / Redis.)

```xml
<azure-openai-semantic-cache-lookup
    score-threshold="0.05"
    embeddings-backend-id="embeddings-backend"
    embeddings-backend-auth="system-assigned" />
<!-- ...on the outbound: azure-openai-semantic-cache-store with a duration -->
```

### Content-safety policy
APIM can call **Azure AI Content Safety** in-policy to screen **inbound prompts** (and/or outbound completions) — blocking harmful content or jailbreak attempts (**Prompt Shields**) at the gateway, before the request ever reaches the model. Centralizes the safety policy across all consumers (defense in depth on top of the deployment's built-in filters).

---

## Backend Pools: Load Balancing & Failover

Define a **backend pool** of multiple Azure OpenAI deployments (across regions and/or PTU + PAYG) and let APIM:

- **Load-balance** across backends (round-robin / weighted / priority).
- **Fail over** on **429 / 5xx** to a healthy backend — smoothing capacity (effective TPM = sum of backends) and improving availability.
- **Circuit-break** a backend that keeps failing.
- **Retry** with backoff at the gateway, honoring `Retry-After`.

**Priority + weighted** pools implement the **PTU baseline + PAYG spillover** strategy: send traffic to PTU first (priority 1), overflow/failover to PAYG (priority 2).

```text
Backend pool "aoai-pool":
  - aoai-ptu-eastus    priority 1   (baseline)
  - aoai-ptu-westus    priority 1   (baseline, LB)
  - aoai-payg-eastus   priority 2   (spillover on 429/5xx)
on-error: retry next backend; respect Retry-After
```

**Considerations:** keep **consistent deployment names** across regions for easy swapping; ensure **model-version parity**; respect **data residency** when choosing failover regions; replicate dependent data (e.g., AI Search indexes) per region; idempotency so retried/rerouted requests don't double-act.

---

## Where It Fits with Security

- **Clients → APIM**: subscription keys or **JWT (Entra ID)**.
- **APIM → Azure OpenAI**: **managed identity** (keyless) with **Cognitive Services OpenAI User** (see `06-security-and-networking.md`).
- Put APIM (and backends) behind **Private Endpoints**; log gateway analytics to App Insights.

This decouples clients from specific deployments and enforces governance/cost controls centrally rather than in each app.

---

## Multi-Tenant SaaS Pattern (ties it together)

For a multi-tenant SaaS on Azure OpenAI:
1. **Per-tenant token quota / rate limit** at the gateway (fairness).
2. **Per-tenant metering** for chargeback (`emit-token-metric`).
3. **Data isolation** for RAG — per-tenant index or strict **security trimming** on tenant ID (see `03-azure-ai-search.md`).
4. **Managed identity** to the backend; tenant auth (JWT/keys) at the gateway.
5. **PTU baseline + spillover**, or dedicated deployments for premium tenants.
6. Per-tenant **content-filter levels / model tiers** and **monitoring**.

---

## Brush-Up Cheat-Sheet

- GenAI gateway = APIM in front of Azure OpenAI for **token quota, metering, semantic cache, content safety, load balancing/failover**.
- **`azure-openai-token-limit`** = per-consumer TPM fairness; **`emit-token-metric`** = chargeback; **semantic-cache** = paraphrase-aware caching; **content-safety policy** = block at the edge.
- **Backend pools** with priority/weight = PTU baseline + PAYG spillover, failover on 429/5xx, circuit-break, retry with `Retry-After`.
- Clients auth via **JWT/keys to APIM**; APIM auths to backend via **managed identity**; everything behind **Private Endpoints**.
- Enables **multi-tenant** fairness, isolation, and per-tenant cost attribution.
