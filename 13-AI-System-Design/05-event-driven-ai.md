# Event-Driven AI

AI work is bursty, slow, and rate-limited — a textbook fit for **async, event-driven** architecture. Use it for ingestion at scale (millions of docs), agent task queues, "summarize/classify this when it arrives" pipelines, and multi-step **sagas** (approve → enrich → generate → notify). The headline pattern: **the queue is your rate limiter** — it smooths spiky producers into the model's hard TPM ceiling and absorbs 429s instead of failing them.

## (a) Requirements

**Functional**
- Decouple producers (events) from AI consumers (workers) via queues/topics.
- Buffer against **rate limits** (429s) — the queue *is* the rate limiter.
- Reliable processing: at-least-once delivery, idempotent handlers, dead-lettering, retries.
- Multi-step workflows with compensation (**saga**).

**Non-functional**

| NFR | Target |
|---|---|
| Durability | Zero message loss; DLQ for poison messages |
| Throughput | Absorb 10× burst without dropping; smooth into model TPM |
| Ordering | Per-entity ordering where needed (Service Bus sessions) |
| Reprocessing | Replay from event store for backfills |

## (b) Architecture diagram

```text
  Producers                  Routing                 AI Workers (KEDA-scaled)
  ┌─────────────┐                                    ┌───────────────────────────┐
  │ Blob upload │──┐    ┌──────────────────┐         │ Ingestion worker (embed)  │
  │ App events  │──┼───▶│   EVENT GRID     │────────▶│ Classify/summarize worker │
  │ IoT / API   │──┘    │ (routing, fan-out│         │ Agent task worker         │
  └─────────────┘       │  reactive events)│         └───────────┬───────────────┘
                        └──────┬───────────┘                     │ rate-limited
                               │ commands / work items           ▼ calls
                        ┌──────▼───────────┐            Azure OpenAI (via APIM)
                        │   SERVICE BUS    │            (queue absorbs 429s)
                        │ queues + topics  │
                        │ sessions (order) │     SAGA (Durable Functions / orchestrator):
                        │ DLQ, scheduled   │     ① validate ─▶ ② enrich(RAG) ─▶ ③ generate
                        └──────┬───────────┘     ─▶ ④ notify   (compensations on failure)
                               │ poison
                               ▼                  Event store: Event Hubs / Blob (replay)
                            DLQ ──▶ ops alert (App Insights)
```

## (c) Component-by-component walkthrough

- **Event Grid** = **reactive routing/fan-out**: "a thing happened" (blob created, document changed) routed to one or many subscribers. Lightweight, push, great for triggering ingestion.
- **Service Bus** = **command/work queues** with the enterprise features you need: **sessions** (FIFO per entity), **dead-letter queues**, **scheduled/deferred messages** (perfect for HITL parking), duplicate detection. This is where AI *work items* queue up.
- **Workers** on **Container Apps/AKS with KEDA** autoscaling on queue depth — scale to zero when idle, scale out under burst. They call Azure OpenAI **through the gateway**, and the queue smooths the burst into the model's TPM ceiling (429 → retry with backoff → stays in queue). **Pace workers to the model's TPM rather than maxing concurrency.**
- **Saga** for multi-step AI workflows via **Durable Functions** or the agent orchestrator: each step is a separate activity with retries and **compensating actions** on failure (e.g., if generation fails after enrichment, roll back the draft). Validate each step's output before the next.
- **Event store** (**Event Hubs** or append blobs) enables **replay/backfill** — re-embed everything when you upgrade the embedding model, or re-run a pipeline after a bug fix.

## (d) Scalability strategies

- **Queue-based load levelling** decouples spiky producers from rate-limited model calls (the headline pattern).
- **KEDA** scales workers on queue length; scale-to-zero saves cost off-peak.
- **Competing consumers** for throughput; **sessions** only where ordering is required (ordering limits parallelism — use sparingly).
- **Partition** topics by tenant/work type for independent scaling.
- **Bound the queue + shed load** when a backlog grows unbounded; for interactive paths, prioritize and degrade rather than queue indefinitely.

## (e) Security patterns

- **Managed Identity** for queue access (no connection strings); RBAC-scoped per worker.
- **Private Endpoints** on Service Bus/Event Grid; VNet-integrated workers.
- **Poison-message isolation** via DLQ prevents one bad payload from blocking the line; alert on DLQ depth and support replay.
- **Validate/authenticate event payloads** (Event Grid signature / SAS) to prevent spoofed events triggering AI spend.

## (f) Trade-offs

| Decision | Option A | Option B | Guidance |
|---|---|---|---|
| **Sync vs async** | Sync request/response: simple, immediate, good UX for chat | Async via queue: resilient to rate limits, scales bursts, handles long jobs | Sync (with streaming) for interactive chat; async for ingestion, batch, long agent tasks. Many systems do both: a sync facade + async backend with status polling/SignalR. |
| **Event Grid vs Service Bus** | Event Grid: reactive notifications, fan-out, cheap | Service Bus: durable work queues, ordering, DLQ, scheduled | Event Grid to *route notifications*, Service Bus to *queue work*. Common combo: Event Grid → Service Bus → worker. |
| **Choreography vs orchestration** | Choreography (events): loosely coupled, scalable, harder to trace | Orchestration (Durable Functions): visible, resumable, more coupling | Orchestration for multi-step workflows that need auditability and compensation; choreography for loosely coupled fan-out. |
| **At-least-once + idempotent vs exactly-once** | At-least-once (Service Bus default) + idempotent handlers | Pursue exactly-once semantics | At-least-once + idempotency keys is the pragmatic, robust choice (true exactly-once is largely a myth). Critical because **a retried LLM call costs money twice** — dedupe before you spend tokens. |

> **Phrase that lands:** "That call goes async through Service Bus — the queue is my rate limiter against 429s, KEDA scales workers on depth, and idempotency keys stop a retried LLM call from double-billing."
