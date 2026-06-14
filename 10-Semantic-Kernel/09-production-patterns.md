# Production Patterns

SK is designed to live inside a normal .NET enterprise app: DI-first, middleware via filters, OpenTelemetry, resilience via the standard HTTP stack. This page collects the patterns that turn a demo into a production service. ASP.NET Core HTTP specifics (SSE endpoints, background ingestion, EF Core) are in [10-dotnet-ai-integration.md](10-dotnet-ai-integration.md).

---

## Streaming

Streaming yields tokens as the model generates them, improving perceived latency.

```csharp
await foreach (StreamingChatMessageContent token in
    chat.GetStreamingChatMessageContentsAsync(history, settings, kernel, ct))
{
    Console.Write(token.Content);   // pipe to SignalR / SSE for web UIs
}
```

Nuances:

- Automatic function calling + streaming still resolves tool calls **server-side before streaming the final answer** — design the UI for a "thinking/tool" phase, then the token stream resumes.
- Filters still apply; usage/token metadata typically arrives in the final chunk (`StreamOptions { IncludeUsage = true }`).
- Always forward a `CancellationToken` so abandoned requests stop generating — and stop billing.

---

## Resilience / Polly + 429 handling

Azure OpenAI returns HTTP 429 when you exceed TPM/RPM quota, usually with a `Retry-After` header. SK uses an injectable `HttpClient`, so configure resilience at the HTTP layer with `Microsoft.Extensions.Http.Resilience` (Polly v8 standard pipeline).

```csharp
builder.Services.AddHttpClient("aoai")
    .AddStandardResilienceHandler(o =>
    {
        o.Retry.MaxRetryAttempts = 4;
        o.Retry.BackoffType = DelayBackoffType.Exponential;
        o.Retry.UseJitter = true;
        // 429/503 are retried by the standard pipeline; respect Retry-After
    });
```

Rules:

- **Honor `Retry-After`** rather than guessing the delay.
- **Retry only retryable errors** (429, 500, 503, timeouts). Don't retry 400 / content-filter blocks — fix the input.
- **Add a circuit breaker** to fail fast on a sick endpoint.
- **Bound client-side concurrency** (rate limiter / semaphore) so you don't self-inflict 429s.
- For SLA-bound workloads, use **PTUs** (reserved capacity) or **multi-deployment/region load balancing** via an AI gateway (APIM).

---

## Caching

Two layers, both implementable as filters (see [07-filters-and-middleware.md](07-filters-and-middleware.md)):

- **Exact-match cache** keyed on (function + args) or (rendered prompt + settings hash). Set `context.Result` and skip `next` on hit.
- **Semantic cache** — embed the query, look up the nearest cached answer above a high similarity threshold (e.g., 0.95). Cuts cost on near-duplicate questions; too low a threshold returns wrong answers.
- **Embedding cache** — embeddings are deterministic; hash the text and store the vector to avoid re-embedding identical content.

Use `IMemoryCache` for hot in-process data and `IDistributedCache`/Redis across instances. Set TTLs, invalidate on document updates, scope by tenant, and never share permission-trimmed results across users.

---

## Token budgeting & context management

`ChatHistory` grows every turn (plus tool results), eventually exceeding the context window and inflating cost/latency. Strategies:

- **Truncation** — keep the system message + last N turns (`ChatHistoryTruncationReducer`).
- **Summarization** — periodically replace older turns with a model-generated summary (`ChatHistorySummarizationReducer`) — preserves coherence at the cost of an extra call.
- **External memory** — offload long-term facts to a vector store, retrieve on demand.

```csharp
var reducer = new ChatHistorySummarizationReducer(chatService, targetCount: 10);
var reduced = await reducer.ReduceAsync(history);   // apply before each model call
```

Track `Usage` from `FunctionResult.Metadata` / connector responses, export as an OTel metric, and alert on cost spikes.

```csharp
var fr = await kernel.InvokeAsync(fn, args);
if (fr.Metadata?.TryGetValue("Usage", out var usage) == true)
    meter.RecordTokens(usage);
```

---

## Observability with OpenTelemetry

SK emits OTel traces/metrics/logs under the `Microsoft.SemanticKernel*` source/meter names, following the GenAI semantic conventions.

```csharp
using var tracer = Sdk.CreateTracerProviderBuilder()
    .AddSource("Microsoft.SemanticKernel*")
    .AddAzureMonitorTraceExporter()       // App Insights
    .Build();

using var meter = Sdk.CreateMeterProviderBuilder()
    .AddMeter("Microsoft.SemanticKernel*")
    .Build();
```

Capture: tokens (prompt/completion), latency per function, tool-call count, retrieval hit scores, cache hit rate, and error/refusal rate. Traces show the full orchestration including each auto-invoked function as a child span. Enable sensitive prompt data (`Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive` or similar) **only in non-prod** — prompts may contain PII. Combine with filters for custom spans and cost counters.

---

## Testing & evaluation

Separate "does my code work" (unit tests) from "is the output good" (evals):

- **Unit-test native functions** as plain C# — they're just methods, no kernel needed.
- **Mock `IChatCompletionService`** to return canned `ChatMessageContent` / tool calls; assert orchestration logic and that filters fire. Don't call the live model in unit tests.
- **Use `InMemoryVectorStore`** for deterministic RAG retrieval tests.
- **Golden/eval tests** for prompts and RAG: a fixed Q→expected-behavior set, run against a pinned model, scored with LLM-as-judge or rule-based faithfulness/groundedness checks; gate CI on regression. Azure AI Foundry evaluations integrate with `.prompty` assets.
- **Integration tests** hit the real deployment sparingly — pin temperature 0, assert structure (valid JSON, contains a citation), not exact text.

```csharp
var mockChat = new Mock<IChatCompletionService>();
mockChat.Setup(c => c.GetChatMessageContentsAsync(
        It.IsAny<ChatHistory>(), It.IsAny<PromptExecutionSettings>(),
        It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync([new ChatMessageContent(AuthorRole.Assistant, "stubbed")]);
```

---

## Multi-tenancy

Never share a single configured kernel's *state* across tenants.

- **Per-request/scoped kernels** with tenant-specific config (deployment, system prompt, vector index).
- **Carry tenant context** in `KernelArguments` and enforce per-tenant data isolation in retrieval via filters.
- **Separate vector collections/indexes per tenant** (or a shared index with a server-enforced tenant filter — never derive tenant from the request body).
- **Per-tenant rate limits/quotas** and usage metering for billing.
- Resolve the tenant claim from Entra ID on every request.

> A `Kernel` is cheap to construct and **not** meant to be a long-lived singleton holding per-request state. Plugins/connectors = singletons; `Kernel` + `KernelArguments` = request/tenant-scoped. Stuffing per-user state into a singleton kernel is a cross-request data-leak waiting to happen.

---

## Best-practices checklist

- Use **managed identity**, not keys. `DefaultAzureCredential`; secrets in Key Vault.
- Write tool **descriptions like API docs** — the model selects from them.
- **Scope advertised functions** — fewer tools = cheaper, more accurate selection.
- `Temperature = 0` for tool selection / structured output.
- **Bound the tool-call loop** — cap iterations, idempotent functions, approval gate for side effects.
- **Pin embedding dimensions + distance function**; reindex on model change.
- Prefer **hybrid search** on Azure AI Search.
- **Build kernels per request** when state is involved.
- **Cross-cutting concerns → filters**, not call sites.
- **Don't build new systems on planners**; use function calling and agents.
- **Truncate/summarize history** before it blows the context window.
- **Treat the LLM as untrusted** — validate tool arguments, sanitize retrieved content.
- **Instrument with OpenTelemetry from day one.**
- **Track the convergence** — new multi-agent work → Microsoft Agent Framework; SK remains the foundation.

---

## Interview angle

> "What production patterns matter for SK?" Security (managed identity, least-privilege RBAC, prompt-render filters for injection defense), resilience (Polly retry/circuit-breaker honoring `Retry-After`, cancellation propagation), cost control (cap `MaxTokens`, limit tool-call iterations, cache embeddings and prompts, token metering via OTel), context management (truncate/summarize history), and multi-tenancy (per-request kernels, per-tenant isolation). The senior framing: agentic loops need explicit budgets — iteration caps, token caps, and filter-based circuit breakers — to be cost-predictable.

---

**Next:** [.NET + AI Integration](10-dotnet-ai-integration.md) — ASP.NET Core HTTP specifics, SDKs, ingestion, and `Microsoft.Extensions.AI`.
