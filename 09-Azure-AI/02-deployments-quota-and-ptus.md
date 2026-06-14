# Deployments, Quota & PTUs — Capacity Engineering

> How Azure OpenAI capacity actually works: the two billing/throughput models, the quotas that throttle you, how to handle 429s, and the cost math. This is the area where a senior .NET/Azure engineer stands out.

---

## Two Capacity Models: PAYG (Standard) vs PTU

These are fundamentally **different** capacity models. Confusing them is a common mistake.

| | **Standard (Pay-As-You-Go)** | **Provisioned Throughput (PTU)** |
|---|---|---|
| Billing | **Per token** (input + output) | **Reserved capacity**, billed hourly/monthly regardless of use |
| Capacity | Shared pool, limited by **TPM quota** | **Dedicated** provisioned units |
| Latency | Variable (shared, can degrade under load) | **Predictable, consistent** |
| Throttling | 429 when you exceed TPM/RPM | Requests queue/throttle when you exceed provisioned throughput (unless **spillover** to PAYG) |
| Best for | Variable / low–medium / unpredictable / getting started | High, **steady** production volume; SLA-grade latency |
| Commitment | None | Often **reservations** (monthly/yearly) for discounts |

**TPM quota** = *permission to consume the shared pool, billed per token.*
**PTU** = *dedicated reserved capacity with predictable performance, billed for the reservation.*

### The crossover & the hybrid pattern

Above a certain **sustained TPM**, PTU is cheaper per token *and* more reliable than PAYG. The mature production pattern is **hybrid**:

```text
                 ┌──────────────────────────┐
   traffic ────► │  APIM GenAI gateway       │
                 │  (load balance + failover)│
                 └─────┬───────────────┬─────┘
                       │ baseline       │ bursts / overflow
                       ▼                ▼
              ┌────────────────┐  ┌────────────────┐
              │ PTU deployment │  │ PAYG deployment │  ← "spillover"
              │ (predictable)  │  │ (elastic)       │
              └────────────────┘  └────────────────┘
```

PTU for the **baseline** steady load; **PAYG spillover** absorbs bursts. The gateway routes/fails over (see `07-apim-ai-gateway.md`).

---

## TPM and RPM Quotas

- **TPM (Tokens Per Minute)** — the primary Standard quota: max tokens (**prompt + completion**) per minute. You allocate TPM to deployments from your **subscription's regional quota** for that model family.
- **RPM (Requests Per Minute)** — a derived request-rate limit, historically tied to TPM by a fixed ratio (e.g., ~6 RPM per 1K TPM).
- Exceeding **either** returns **HTTP 429 (Too Many Requests)**.
- Quota is **regional and per-model-family**; split it across deployments, request increases via Azure support.

`max_tokens` counts against TPM (it reserves completion budget), so right-size it.

---

## Deployment Types: residency vs capacity

| Type | Where requests process | Use when |
|---|---|---|
| **Standard (regional)** | In the resource's region | Strict **single-region** data residency |
| **Data Zone** | Within a broader geography (e.g., EU, US) | Keep data in a compliance **geography** but want more capacity than one region |
| **Global** | Across Microsoft's global infra | Max throughput / newest-model access; cross-region processing acceptable |

Each has PAYG and Provisioned (PTU) variants. Match the type to **residency requirement + capacity need**.

---

## Handling 429s & Retries (must-know)

A 429 means you hit TPM/RPM quota (Standard) or provisioned throughput (PTU).

**Do:**
1. **Exponential backoff with jitter**, and crucially **honor the `Retry-After` header** Azure returns.
2. **Distribute load** across multiple deployments/regions (APIM gateway does smart routing + retry across backends).
3. Use **PTU baseline + PAYG spillover** for predictable capacity.
4. **Throttle/queue client-side** to stay under limits; prioritize/shed non-critical load.
5. **Reduce token usage** (shorter prompts, reranked RAG context, prompt caching) to lower TPM pressure.
6. For bulk/offline work use the **Batch API** (separate quota, ~50% cheaper).
7. **Monitor 429 rate** as a capacity SLO.

**Don't:** hot-retry without backoff — that *amplifies* the overload (429 storm).

```csharp
// The Azure.AI.OpenAI SDK has built-in retry that honors Retry-After.
// Configure via client options; still design your app for capacity, not just retries.
var options = new AzureOpenAIClientOptions
{
    // RetryPolicy / max retries configurable; 429 + Retry-After honored by default
};
var client = new AzureOpenAIClient(endpoint, new DefaultAzureCredential(), options);
```

Build retry/backoff into a **shared client wrapper** so every call benefits.

---

## The Batch API

- Processes large volumes **asynchronously** at ~**50% discount**, target turnaround (e.g., within 24h), using **separate batch quota** that doesn't consume real-time TPM.
- Workflow: upload a **JSONL** file of requests → submit a batch job → poll → retrieve results.
- Use for **non-interactive, high-volume** jobs: bulk **embedding generation** for indexing, mass classification/summarization/extraction, dataset labeling, offline evals.
- Not for real-time; completion time isn't instant.

---

## PTU Capacity Planning

PTUs are sized to your **peak sustained load** for a model.

1. **Measure/forecast** traffic: calls/min (RPM) and avg **input + output token** sizes per call (include RAG context).
2. Use Microsoft's **PTU estimator** (Azure portal / Foundry) — input model + RPM + token sizes → required PTUs for your latency target.
3. Account for **burstiness**: provision for peak, or **baseline PTUs + PAYG spillover**.
4. Consider **reservations** (monthly/yearly) for discounts on steady load.
5. **Load-test** and monitor **PTU utilization** to right-size; under-provisioning → throttling, over-provisioning → wasted spend.

---

## Cost Math (worked)

Cost ≈ **tokens × per-token price**, with **output tokens priced higher** than input.

**Per-request estimate:**
```text
cost_request = (input_tokens × price_in) + (output_tokens × price_out)
```

**Monthly forecast (PAYG):**
```text
monthly = requests_per_day × 30
          × (avg_input_tokens × price_in + avg_output_tokens × price_out)
```

**PTU break-even:** compare `PTU_monthly_reservation_cost` against the PAYG monthly cost at your sustained TPM. Above the crossover, PTU wins on both cost and latency.

**Concrete optimization wins:**
- Replacing a 3,000-token document dump with 4 retrieved 200-token chunks ≈ **75% input-cost cut** *and* better relevance.
- **Prompt caching**: keep the large static system prompt/context at the **prefix**; Azure bills repeated **cached input tokens at a steep discount** and serves them faster (lower TTFT). No code change beyond prefix-stable prompt ordering.
- **Dimension-reduced embeddings** (3072→1024) cut storage/search cost at modest recall loss.
- Cap `max_tokens`; route easy tasks to `gpt-4o-mini`.

> Treat tokens like a **metered utility**: measure cost per request/feature/user (Azure Monitor + APIM metering), set budgets/alerts, optimize the biggest consumers. A 50% prompt reduction is a direct 50% input-cost reduction.

---

## Brush-Up Cheat-Sheet

- **TPM quota = shared-pool permission, per-token billed; PTU = dedicated reserved throughput, predictable latency.** Hybrid = PTU baseline + PAYG spillover.
- 429 → **backoff + jitter + honor `Retry-After`**; never hot-retry. Spread load via APIM; raise quota; reduce tokens.
- **Batch API** = async, ~50% off, separate quota — for bulk/offline (e.g., embedding a corpus).
- PTU sizing: measure token/throughput profile → **PTU estimator** → baseline + spillover → load-test → monitor utilization.
- Cost levers: right-size model, **prompt caching** (static prefix), trimmed RAG context, dimension-reduced embeddings, cap `max_tokens`, Batch API.
