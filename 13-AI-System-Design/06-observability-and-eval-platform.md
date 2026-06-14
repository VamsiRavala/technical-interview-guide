# Observability & Eval Platform

You cannot operate what you can't see, and AI adds three dimensions traditional APM ignores: **token/cost, quality, and safety**. Standard APM tells you the call returned 200; it can't tell you the answer was wrong. When a user says "the bot gave a wrong answer," you must trace that exact call — retrieved context, prompt, prompt *version*, model, tokens, cost, latency, and the eval verdict.

## (a) Requirements

**Functional**
- **Distributed tracing** of the full chain (BFF → gateway → RAG → model → tools), with the LLM call as a first-class span.
- **Token & cost metrics** per request/tenant/model/feature.
- **Quality/eval metrics** (groundedness, relevance, refusal rate) sampled online.
- **Safety metrics** (Content Safety blocks, jailbreak attempts).
- **Alerting on drift / hallucination / cost spikes**.

**Non-functional**

| NFR | Target |
|---|---|
| Trace coverage | 100% of AI calls traced; sampled prompt/response capture |
| Cost attribution | Per-tenant/feature cost dashboard, daily |
| Detection latency | Drift/quality regression flagged within hours, not days |
| Privacy | PII redacted before logging prompts/responses |

## (b) Architecture diagram

```text
  Request ─▶ BFF ─▶ APIM ─▶ RAG ─▶ Azure OpenAI ─▶ Tools
     │        │      │       │          │            │
     └────────┴──────┴───────┴──────────┴────────────┘  (OpenTelemetry, GenAI semconv)
                          │  spans: gen_ai.system, gen_ai.request.model,
                          │         gen_ai.usage.input_tokens / output_tokens,
                          │         gen_ai.operation.name, prompt/response events
                          ▼
           ┌────────────────────────────────────────────────┐
           │  Application Insights / Azure Monitor / Log      │
           │  Analytics  (traces, metrics, logs unified)      │
           └───────┬───────────────────┬──────────────────────┘
                   │                   │
        ┌──────────▼─────┐    ┌─────────▼───────────┐    ┌────────────────────────┐
        │ Dashboards      │   │ Online EVAL pipeline │    │ Alerting                │
        │ • token/cost    │   │ (sampled): groundedness│  │ • cost spike            │
        │ • P50/P95 lat.  │   │  relevance, refusal,  │   │ • groundedness drop     │
        │ • quality trend │   │  LLM-as-judge         │   │ • safety-block surge    │
        │ • per-tenant    │   │  ──▶ metrics back to   │   │ • latency/429 SLO breach│
        └─────────────────┘   │  App Insights          │   └────────────┬───────────┘
                              └────────────────────────┘                ▼
                                                            Action Groups → PagerDuty/Teams
```

## (c) Component-by-component walkthrough

- **Instrumentation = OpenTelemetry with the GenAI semantic conventions.** Emit spans with `gen_ai.system`, `gen_ai.request.model`, `gen_ai.operation.name`, and **`gen_ai.usage.input_tokens` / `output_tokens`**, plus prompt/response as span events (redacted). Naming the **GenAI semconv** signals you're current; Azure SDKs and Semantic Kernel emit these. Spans chain HTTP → retrieval → embed → model → tools.
- **Backend = Application Insights + Azure Monitor + Log Analytics** — unified traces/metrics/logs, KQL for ad-hoc. APIM's `emit-token-metric` policy pushes token counts straight in.
- **Cost = tokens × price**, computed per span and aggregated by tenant/feature/model — chargeback (**[08](08-cost-and-capacity.md)**) and runaway-cost guard.
- **Online eval pipeline**: sample a % of live traffic, run **groundedness / answer-relevance / refusal** checks (LLM-as-judge + heuristics) via **Azure AI Foundry evaluators**, write results back as metrics. This catches a quality regression from a model or prompt change. **Separate retrieval recall from answer groundedness** so a good generator masking bad retrieval is visible.
- **Dashboards**: token/cost, latency P50/P95, 429 rate, quality trend, per-tenant. **Alerting** via Azure Monitor alerts + **Action Groups** → Teams/PagerDuty on: cost spike, groundedness drop (proxy for hallucination), Content-Safety-block surge, latency/SLO breach. Each alert pairs with a runbook; focus on the handful of signals that map to user/business impact to avoid alert fatigue.
- **Drift detection**: when Azure announces a model version upgrade, treat it as a change event and re-run the full eval suite *before* adopting. Supplement sampling with targeted monitors for high-risk intents so rare failures don't hide.

## (d) Scalability strategies

- **Sampling**: trace 100% of metadata but sample full prompt/response capture (e.g., 5-10%) to control storage cost.
- **Async export** of telemetry (OTel batch processor) so logging never sits on the request hot path.
- **Tiered retention**: hot in App Insights, cold to **Azure Data Explorer / Storage** for long-term eval datasets.
- **Online eval on a sample**, not every request, since each eval is an extra model call.

## (e) Security patterns

- **PII redaction before logging** (Purview/Presidio) — prompts/responses are sensitive data; never log raw.
- **RBAC on telemetry** (prompts can contain customer data); separate workspace per data-sensitivity tier; region-local logging for residency.
- **Tamper-evident audit** stream for safety/compliance events (immutable storage).

## (f) Trade-offs

| Decision | Option A | Option B | Guidance |
|---|---|---|---|
| **Log everything vs sample** | Full prompt/response capture: best debuggability | Sample + metadata-only: cheaper, more private | Full metadata always; sample full content. Brutal at scale — token-heavy logs cost real money and carry PII risk. |
| **LLM-as-judge vs human/heuristic eval** | LLM-as-judge: scalable, cheap, slightly noisy | Human eval: gold standard, doesn't scale | LLM-as-judge for continuous online monitoring; periodic human-labeled golden sets to calibrate the judge. |
| **Build vs buy observability** | Buy (App Insights + Azure AI eval SDK): fast, supported | Build custom: tailored, costly to maintain | Buy on the Microsoft stack (App Insights + Azure AI Foundry evaluations); build only the thin glue for your domain metrics. |
| **Online vs offline eval** | Online (sampled live): real signal, lagging, extra inference | Offline (golden set in CI): controlled, gates deploys | Both: offline gates every change, online catches production drift. Version-pin so you can roll back on a regression — drift detection is academic without rollback. |

> **Phrase that lands:** "How do I know it's working? Groundedness and answer-relevance on a golden set in CI, sampled online via Azure AI Foundry, instrumented with OpenTelemetry GenAI semconv into App Insights, alerting on drift — and I version-pin model and prompt so I can roll back when it slips."
