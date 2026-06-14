# Cost & Capacity

Cost is a **first-class NFR** in GenAI, not a billing afterthought — the model is billed per token, so ignoring cost reads as inexperience in an interview. Capacity in GenAI means **tokens-per-minute (TPM)**, not just CPUs. This topic covers the capacity math, the PTU-vs-PAYG break-even, the layered demand-reduction strategy (caching, routing), and how you give finance per-tenant chargeback.

## (a) Requirements

**Functional**
- Forecast and provision capacity (TPM) for the predictable baseline + bursts.
- Minimize cost via caching, model routing, and prompt trimming before either backend is hit.
- Attribute cost per tenant / feature / model for showback or billing.
- Enforce per-tenant and per-conversation budgets with alerts.

**Non-functional**

| NFR | Target |
|---|---|
| Provisioning accuracy | PTU sized to proven baseline; PAYG buffer ~20-30% |
| Cost visibility | Per-tenant/feature cost dashboard, daily |
| Budget enforcement | Hard per-tenant caps + soft alerts at 80% |
| Cost attribution | Within a few % of billed tokens |

## (b) The capacity math (do this live in an interview)

State assumptions out loud; the panel grades reasoning, not arithmetic.

| Quantity | Assumption | Math | Result |
|---|---|---|---|
| Active users | 50k employees, 10% daily, 5 queries each | 50,000 × 0.10 × 5 | 25,000 queries/day |
| Peak QPS | 8-hour workday, peak = 3× average | 25,000 / (8×3600) × 3 | ~2.6 QPS peak |
| Tokens/query | 4k context (RAG chunks) + 1k input + 500 output | 5k in + 0.5k out | 5.5k tokens |
| TPM at peak | 2.6 QPS × 60 × 5.5k | — | ~860k TPM → **need PTU or multi-deployment** |
| Daily token spend | 25k × 5.5k = 137.5M tokens; ~$5/1M blended | 137.5 × $5 | **~$690/day ≈ $20k/mo** |
| Embedding/index | 2M docs × ~5 chunks × 3KB vector+meta | ~10M chunks | ~AI Search tier S2+ |

This single table tells the interviewer that **TPM and cost are the binding constraints** and justifies PTU, caching, and model routing. Don't forget the satellite costs: embeddings (one-time ingest + per-query), AI Search tier, Cosmos RU/s, Redis, Container Apps compute, and egress.

## (c) PTU vs PAYG break-even

PTU (Provisioned Throughput Units) is a fixed monthly reservation buying a guaranteed throughput envelope and predictable latency; PAYG bills per token with no commitment but variable cost and shared-pool rate limits. The break-even is the utilization point where PTU's effective per-token cost equals the PAYG rate.

```text
PTU effective $/token = PTU monthly cost / tokens served per month
If PTU effective $/token < PAYG $/token  ->  PTU wins
```

So the question for a CFO isn't "PTU or PAYG?" but **"what is our sustained, floor-level token volume?"** That proven floor goes on PTU (committed for a reservation discount); everything above — peaks, experiments, new features — stays PAYG. **Never buy PTU on a forecast alone:** start PAYG, measure 4-6 weeks of real traffic, then convert the proven baseline to PTU and re-evaluate quarterly. PTU and GPU capacity can be scarce, so for large commitments plan and reserve ahead rather than assuming elastic supply.

## (d) Layered demand reduction (the cheapest token is the one you never send)

```text
  Request
    │
    ▼  1. SEMANTIC CACHE (gateway) ── hit? return, $0 model spend
    │      embedding cache by content-hash = safest, highest-ROI layer
    ▼  2. PROMPT TRIMMING / compression ── fewer context tokens
    │
    ▼  3. MODEL ROUTING ── cheapest sufficient model (mini → mid → frontier)
    │      cascade: try cheap, escalate only on confidence/validation fail
    ▼  4. BATCH API ── for latency-tolerant bulk work (discount + separate quota)
    │
    ▼  PTU floor + PAYG spillover backend
```

- **Caching** removes cost entirely for repeated work. **Embedding cache by content hash** is the safest layer (long TTL, fully deterministic). **Semantic cache** for FAQ-shaped traffic — but tune the threshold and exclude personalized/authenticated prompts (a loose threshold serves a confidently-wrong answer).
- **Model routing** sends each request to the cheapest model that passes its evals — but requires per-route evals to prove the small model suffices, since misrouting degrades quality silently.
- **Batch API** for non-real-time bulk classification/summarization: substantial per-token discount, separate quota pool, results within a window rather than seconds.

## (e) Cost monitoring & chargeback

You cannot optimize what you cannot attribute. Every request through the gateway carries dimensions — **tenant, feature, environment, model** — and APIM's `emit-token-metric` (plus OpenTelemetry token metrics from apps) logs prompt + completion tokens per call to **Log Analytics**. Cost = tokens × the model's rate (or PTU cost amortized across served tokens), surfaced in **Cost Management** dashboards and Power BI. Per-tenant APIM subscriptions and resource tagging reinforce attribution at the Azure billing layer.

- **PTU spend is fixed** — allocate it by share-of-usage; **PAYG is directly attributable**. Be explicit with finance about the difference, since a bursty tenant's PTU allocation can mislead.
- **Per-conversation budgets**: track cumulative tokens per run in Redis; degrade (summarize history, smaller model) as you approach the cap, stop gracefully at it. Pair with agent max-iteration caps as the backstop against runaway loops.
- **Force all traffic through the gateway** and block direct Azure OpenAI calls, or untracked spend escapes attribution.

## (f) Trade-offs

| Decision | Option A | Option B | Guidance |
|---|---|---|---|
| **PTU vs PAYG** | PTU: predictable latency, cheaper at high steady utilization, fixed cost | PAYG: zero commit, elastic, variable cost, throttle risk | PTU floor + PAYG burst. PTU is cheaper *only if kept busy* — under-utilized PTU is wasted money. |
| **Cache aggressiveness** | High hit rate, big savings | Conservative, fewer wrong hits | Embedding cache always (safe); semantic cache scoped + TTL'd + eval-monitored. Cost saving vs risk of confidently-stale answers. |
| **Cost-attribution granularity** | Full per-request token detail: rich, drives optimization | Coarse aggregation: cheaper telemetry | Full detail at platform scale — making each team see its own spend drives more optimization than any clever cache. Accept the telemetry volume cost. |
| **Reserved vs flexible** | Reservations/commit: discount, capacity guarantee | On-demand: flexibility, no idle risk | Commit only the proven floor; keep new/spiky features on-demand. The discount is worthless on idle units. |

> **Phrase that lands:** "My binding constraint is TPM and cost, so I size for that first: PTU on the proven floor, PAYG for burst, semantic + embedding caching to shrink billable volume, and per-tenant token metrics for chargeback so every team sees its own spend."
