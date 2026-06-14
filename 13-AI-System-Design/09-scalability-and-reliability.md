# Scalability & Reliability

You scale a GenAI workload not by autoscaling stateless pods but by **managing demand against a hard throughput ceiling**. The constraint is rarely your compute — it's the **TPM/RPM quota** on the model backend. And because the model is non-deterministic, you commit hard SLAs only on the *operational envelope* (availability, latency, safety process) while expressing answer quality as monitored SLOs.

## (a) Requirements

**Functional**
- Scale to tens of thousands of concurrent users against a fixed model quota.
- Absorb 429 throttling without it ever reaching the user as a raw failure.
- Fail over across deployments/regions; degrade gracefully when the only model available is dumber/slower.
- Define defensible SLAs/SLOs for a non-deterministic system.

**Non-functional**

| NFR | Target |
|---|---|
| Availability | 99.9%+ end to end; multi-region failover |
| First-token latency | < 1 s P95 on interactive paths (streaming) |
| 429 handling | Absorbed by routing/backoff/spillover/queue, never raw to user |
| Capacity headroom | Surviving region/deployment can absorb shifted load |

## (b) Architecture diagram

```text
                       Azure Front Door (global LB, health probes, regional failover)
                                  │
        ┌─────────────────────────┼─────────────────────────┐
        ▼                         ▼                          ▼
   Region A (active)        Region B (active)          (interactive: stream;
   BFF + APIM gateway       BFF + APIM gateway           batch: queue + workers)
        │                         │
        ▼  AI GATEWAY backend pool (prioritized, circuit-broken):
        │   tier 1  PTU primary        ─── latency-guaranteed floor
        │   tier 2  PAYG / 2nd region  ─── spillover on 429/saturation
        │   tier 3  fallback model     ─── degraded but available
        │   tier 4  cache / retrieved passages ─── "summary unavailable" + value
        ▼   tier 5  honest graceful failure ─── clear msg + queued retry
   Azure OpenAI deployments      Cosmos DB (multi-region)   Redis (cache/session)
```

## (c) Scaling against the quota ceiling

- **Provision PTU** for guaranteed capacity; distribute load across **multiple deployments and regions** behind the gateway, balanced by *remaining TPM* (not round-robin).
- The **stateless orchestration tier** (Container Apps/AKS) scales horizontally and cheaply — but that doesn't help if Azure OpenAI throttles, so the real work is at the backend pool.
- **Push non-interactive work onto a queue** (Service Bus) with worker pools that drain at the rate the backend allows — smoothing spikes instead of failing them (**[05](05-event-driven-ai.md)**).
- **Interactive chat streams** so perceived latency is first-token, and connections free up token-by-token.
- **Reduce demand**: caching, model routing to smaller models, and prompt trimming all cut the TPM you must buy (**[08](08-cost-and-capacity.md)**).

## (d) 429 handling and the fallback chain

A 429 is *expected at scale*, not exceptional. Handle it in layers, from least to most degraded:

| Tier | Mechanism | Cost / quality impact |
|---|---|---|
| 1 | **Gateway routing**: circuit breaker trips the saturated backend, traffic shifts to a healthy deployment/region | None (if headroom exists) |
| 2 | **PTU → PAYG spillover** | Higher per-token cost; deliberate cost-for-reliability trade |
| 3 | **Exponential backoff with jitter**, honoring `Retry-After`; for async work, **shed into a queue** | Added latency; jitter prevents thundering herd |
| 4 | **Model fallback** to a smaller/equivalent model | Answer style/quality shifts — a per-use-case decision |
| 5 | **Cached or retrieved answer** (RAG returns source passages with a "summary unavailable" note) | Partial value retained |
| 6 | **Honest graceful failure**: clear message + queued retry, never a hung request or stack trace | User informed transparently |

**Don't fall back on 400 / content-filter errors** — they won't succeed elsewhere. **Circuit breaking only helps if you have somewhere to fail over to**, so it's meaningless with a single deployment — size secondaries to actually absorb shifted load rather than instantly tripping their own breakers in a cascade. Test all of this with **chaos/fault injection** — untested fallback is a false sense of safety discovered at 2 a.m.

## (e) Graceful degradation

When you must fall back to a smaller/slower model, **adapt the task to the model**: simplify the prompt (a small model often needs a *different* prompt, not the same one downgraded), narrow scope, lower max tokens, or switch from "generate a full answer" to "return top retrieved passages with a note." For agents, disable riskier autonomous actions and revert to human-confirmed flows. Inform the user transparently — silent degradation erodes trust worse than an honest banner. Whether a degraded answer beats no answer is a per-use-case policy: yes for brainstorming, a refusal may be safer for a medical/financial summary. Degradation paths are first-class, feature-flagged, and load-tested.

## (f) SLAs vs SLOs for non-deterministic systems

Split what you can **commit** from what you can only **target**:

- **SLA (contractual)** — the deterministic, measurable envelope: endpoint **availability**, **latency percentiles** (p50/p95 first-token and total), **throughput**, and the **safety process**. These compose from Azure's underlying SLAs + your architecture (multi-region failover, PTU guarantees).
- **SLO (target, monitored)** — quality: groundedness/relevance scores, max harmful-content rate, user-satisfaction thresholds, measured continuously via evals and tracked as objectives, not guarantees.

You **cannot** put answer correctness in a hard SLA — a probabilistic system can't promise per-response accuracy the way a CRUD API promises a correct read; over-promising correctness is a legal and trust landmine. Guarantee latency on the metric you've *engineered* to be stable: **first-token via streaming** and **p95 via PTU + output caps + failover**, not raw end-to-end generation time you can't fully control.

## (g) Trade-offs

| Decision | Option A | Option B | Guidance |
|---|---|---|---|
| **Active-active multi-region vs active-passive** | Active-active: low latency, fast failover | Active-passive: cheaper, slower failover | Active-active for global low-latency + survivability; ensure each region has headroom to absorb another's load. Data residency may forbid cross-region replication. |
| **Failover to another model vs fail fast** | Fallback model: available, lower/changed quality | Fail fast: honest, no value | Fallback for products tolerant of quality shift; fail-fast (with retry) where a wrong answer is worse than none. Per-use-case. |
| **Circuit breaker sensitivity** | Sensitive: trips early, protects backends, may overflow to PAYG | Lax: keeps routing to a dying backend | Tune against observed failure patterns; alert when a breaker trips so humans know capacity is degraded. |
| **Hard SLA on quality vs SLO** | Promise correctness: customers love it | Commit envelope, target quality | Never sign an SLA treating a non-deterministic model like a deterministic function. Hard-commit availability/latency/safety; monitor and evidence quality with a remediation process. |

> **Phrase that lands:** "I don't make the model deterministic — I make latency *predictable* via PTU, output caps, streaming, caching, and failover, and I never let a 429 reach the user raw: it's absorbed by routing, backoff, spillover, or a queue, with the degradation chosen deliberately."
