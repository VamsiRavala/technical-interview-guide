# LLMOps Basics

LLMOps is where a senior engineer's existing instincts pay off the fastest. An LLM is just an unreliable, metered, latency-heavy external dependency — and you already know how to handle those: timeouts, retries, circuit breakers, caching, observability. This page covers the LLM-specific twists: latency (output length dominates), streaming, caching layers, cost control, rate limits / 429s, and the observability you need to debug production. The Microsoft control-plane specifics (PTU vs PAYG, APIM gateway, content filters) get full treatment in section **09**.

---

## Latency: Output Length Is the Boss

Generation is **sequential** — each output token waits for the previous one. So:

> The single biggest latency lever is usually **output length**, not prompt size.

Measure two numbers separately:

```text
TTFT  = Time To First Token  (how fast the user sees *something*)
TPS   = Tokens Per Second    (how fast it streams after that)
```

Latency-reduction tactics, roughly in order of leverage:

1. **Stream tokens** — slashes *perceived* latency (below).
2. **Reduce output tokens** — cap `max_tokens`, ask for concise answers.
3. **Shrink the prompt** + use **prompt caching** for stable prefixes.
4. **Pick a smaller/faster model** where quality allows, or **route** easy queries to cheaper models.
5. **Use PTU** for consistent low latency under load (section **09**).
6. **Parallelize** independent calls.
7. **Cache full responses** for repeated queries.
8. **Deploy in a region near users.**

---

## Streaming

Streaming sends tokens as they're generated (Server-Sent Events) instead of waiting for the full completion, so users see output in **hundreds of ms** (TTFT) instead of after several seconds.

```python
# Conceptual
stream = client.chat.completions.create(..., stream=True)
for chunk in stream:
    forward_to_client(chunk)   # SSE / WebSocket
```

```csharp
// Semantic Kernel (section 10)
await foreach (var chunk in kernel.InvokePromptStreamingAsync(prompt))
    await response.WriteAsync(chunk.ToString());
```

Considerations:
- You can't easily **validate/moderate the full output before the user sees the start** — run output content-safety carefully (buffer, or post-hoc redact).
- **Error handling mid-stream** is trickier.
- **Structured-output validation must happen after** the stream completes.

> Streaming is near-mandatory for interactive chat; for backend/batch processing, non-streaming is simpler.

---

## Caching: Three Levels

| Level | How | Catches | Risk |
|---|---|---|---|
| **Exact-match response** | Hash the full prompt → return stored completion | Identical requests (FAQs, temp-0 calls) | None if keyed correctly |
| **Semantic cache** | Embed the query → return cached answer if a prior query is close enough | Paraphrases | **False hits** — tune the threshold, scope by user/context |
| **Prompt prefix cache** | Provider reuses computation for a repeated prompt prefix | Large system prompts, few-shot blocks | None (provider-side) |

```text
exact:    hash(prompt) → completion
semantic: embed(query) → if cos_sim(query, cached) > τ → cached answer
prefix:   stable system prompt at the START → provider reuses KV computation
```

Azure API Management supports **semantic caching** as a GenAI-gateway feature. **Always invalidate** caches when source data or prompts change, and **be careful caching personalized or sensitive content.**

---

## Cost Control

```text
cost = tokens × per-token price        (output tokens priced HIGHER than input)
```

Levers:
- **Minimize prompt size** — retrieve don't dump, concise instructions, prompt caching.
- **Cap output tokens.**
- **Route** easy tasks to cheaper/smaller models.
- **Batch** where possible; **cache** responses.

**Forecast:** `avg(input + output tokens) × expected volume × unit price`. Model **PTU break-even** for high steady volume (section **09**).

**Instrument cost as a first-class metric** — per request, per feature, per user — with budgets and alerts. Use APIM for per-client token metering and chargeback.

> The discipline that marks a senior engineer: treat tokens like a **metered utility** — measured, attributed, optimized — not an invisible cost. A 50% prompt reduction is a direct 50% input-cost reduction.

---

## Rate Limits & 429s

Azure OpenAI enforces **tokens-per-minute (TPM)** and **requests-per-minute (RPM)** quotas; exceeding them returns **HTTP 429**, often with a `Retry-After` header.

```text
429 → honor Retry-After → exponential backoff WITH JITTER → retry
```

Handle it with:
- **Exponential backoff + jitter**, honoring `Retry-After`.
- A **client-side rate limiter / queue** to smooth bursts.
- **Spread load** across multiple deployments/regions via an APIM gateway or router.
- **PTU** for guaranteed throughput on critical paths, with **PAYG spillover** for bursts.
- **Right-size `max_tokens`** — TPM counts *both* prompt and completion tokens.
- **Prioritize/queue** by importance; shed or degrade non-critical load.

> **Never hot-retry without backoff** — that amplifies the overload. Build retry logic into a **shared client wrapper** so every call benefits.

---

## Resilience: Fallback & Graceful Degradation

Plan for every dependency failing — the patterns are straight from distributed systems:

| Failure | Response |
|---|---|
| Model timeout / 429 / 5xx | Retry w/ backoff → fail over to alternate deployment/region or smaller backup model |
| Retrieval failure | "I can't access that right now" — **don't hallucinate** |
| Content-filter block | Return a safe canned message |
| Tool failure | Let the agent recover or escalate to a human |
| Total failure | Helpful error, **not a stack trace** |

Use **circuit breakers** to stop hammering a failing dependency. Define and **test these paths explicitly** — bake them into a shared client layer.

---

## Versioning & Reproducibility

- **Pin explicit model versions** in Azure OpenAI deployments — never rely on auto-update in prod.
- Treat **prompts as versioned code** (source control or a prompt registry), deployed behind feature flags.
- **Log model version + prompt version** with each response for debugging and reproducibility.
- To upgrade: deploy new version **alongside** → run golden eval set + ideally A/B → cut over → keep instant rollback.

For determinism: temperature 0 (don't also set top-p), pin the version, fix the prompt/context, use `seed` where supported (Azure returns a `system_fingerprint`). True determinism still isn't guaranteed — assert on **semantic equivalence** in tests, or add a **cache** keyed on input for strict cases.

> This prevents the classic *"it changed and we don't know why"* failure mode.

---

## Observability

For any user complaint, you should be able to **reconstruct exactly what happened end to end.** Instrument and trace:

```text
✓ full request/response (prompt, completion, model + prompt version)
✓ token counts (input/output) + cost per call
✓ latency split into TTFT and total
✓ retrieval traces (which chunks, their scores)
✓ tool/agent traces (each step: thought, tool call, result)
✓ finish reasons (incl. content-filter blocks)
✓ error / 429 rates
✓ user feedback (thumbs up/down)
✓ sampled LLM-judge quality scores
```

Use **distributed tracing (OpenTelemetry)** with correlation IDs across the chain. **Azure Monitor / Application Insights** and **Azure AI Foundry tracing** support GenAI semantic conventions. The goal: spot regressions in cost, latency, or quality **before users do**.

> **Logs of prompts may contain PII** — govern access accordingly (see [07-responsible-ai](07-responsible-ai.md)).

---

## Capacity Planning (briefly)

Capacity is measured in **tokens-per-minute**, not just requests:

```text
load ≈ (requests/min) × (avg input + output tokens)
```

Load-test with **realistic prompt sizes and output lengths**, ramping until you see 429s, latency degradation, and TTFT under concurrency. Size PTUs from **peak sustained TPM + headroom**. Validate that retries/backoff don't amplify load. Re-test after prompt/model changes (they shift token usage). The mindset shift from classic load testing: **token throughput and output length, not request count, are the binding constraints.**

---

## The GenAI Gateway (preview of section 09)

A **GenAI gateway** (commonly **Azure API Management**) is one managed entry point between all apps and your LLM backends. It centralizes auth/key management, token-based rate limiting per consumer, load balancing/failover across deployments and regions (PTU + PAYG spillover), semantic caching, content safety, and logging/metering for chargeback. For an enterprise with many teams sharing capacity, it turns scattered, ungoverned calls into a managed, observable service. (Architecture and config: section **09**.)

---

## Brush-up Cheat-Sheet

```text
Latency      = OUTPUT length dominates; measure TTFT + TPS separately
Streaming    = SSE → fast perceived latency; moderate after stream / validate after
Caching      = exact (hash) | semantic (embed+threshold) | prefix (provider KV reuse); invalidate!
Cost         = tokens × price (output higher); retrieve-don't-dump, cap output, route, instrument
429s         = backoff + JITTER, honor Retry-After, queue, spread load, PTU+spillover; never hot-retry
Resilience   = retries, circuit breakers, fallbacks; degrade gracefully, no stack traces
Versioning   = pin model + prompt versions; log both; alongside-deploy + A/B + rollback
Observability= trace prompt/tokens/cost/latency/retrieval/tools/feedback; OTel + App Insights; PII-aware
Capacity     = plan in TPM, not RPM; load-test with realistic token sizes
```

**Next:** [09-interview-questions](09-interview-questions.md) — drill the Q&A.
