# .NET + AI Integration

This page is the **ASP.NET Core integration layer** around Semantic Kernel: how you stream to browsers, build SSE endpoints, use the raw `Azure.AI.OpenAI` SDK, run background ingestion, store vectors with EF Core, and compose cross-cutting concerns with `Microsoft.Extensions.AI`. SK orchestration concepts live in the earlier pages; this is the plumbing that ships them as a web service.

---

## Streaming `IAsyncEnumerable` to clients

`GetStreamingChatMessageContentsAsync` (and `InvokePromptStreamingAsync`) returns `IAsyncEnumerable<StreamingChatMessageContent>`. In a minimal API, iterate and write each delta to the response stream, flushing after each chunk so the client receives tokens immediately. **Forward `HttpContext.RequestAborted`** so client disconnects cancel the upstream model call — the single biggest cost-control lever for streaming chat.

```csharp
app.MapPost("/chat/stream", async (ChatReq req, Kernel kernel, HttpContext ctx) =>
{
    var chat = kernel.GetRequiredService<IChatCompletionService>();
    var history = new ChatHistory();
    history.AddUserMessage(req.Prompt);

    ctx.Response.ContentType = "text/event-stream";
    await foreach (var d in chat.GetStreamingChatMessageContentsAsync(
                       history, cancellationToken: ctx.RequestAborted))
    {
        if (d.Content is { Length: > 0 })
        {
            await ctx.Response.WriteAsync($"data: {d.Content}\n\n");
            await ctx.Response.Body.FlushAsync();
        }
    }
});
```

Never pass `CancellationToken.None` to model calls. Catch `OperationCanceledException` at the boundary and treat it as a normal stop, not an error to log loudly.

---

## A proper SSE endpoint

Server-Sent Events is the natural fit for one-way token streaming. Requirements:

- `Content-Type: text/event-stream`; write `data:` lines terminated by a blank line; **flush after each event**.
- **Disable buffering** everywhere in the path: at the server, and at any reverse proxy (`X-Accel-Buffering: no` for nginx; ensure Front Door / gateway doesn't buffer). **Avoid response compression** on the SSE route — it buffers.
- Honor `RequestAborted`; send a terminal `event: done` so the client knows it's complete.
- For idle connections, emit periodic comment lines (`: ping`) to keep the connection healthy.

```csharp
app.MapPost("/chat/sse", (ChatReq req, IChatService svc, HttpContext ctx) =>
    Results.Stream(async stream => await svc.StreamAsync(req, stream, ctx.RequestAborted),
                   "text/event-stream"))
   .RequireAuthorization()
   .RequireRateLimiting("perUser");
```

### Streaming tool-call and citation events

For transparent agent UIs, define a structured event protocol — emit typed JSON events (`token`, `tool_call`, `citation`, `done`) on the same channel. Hook an `IFunctionInvocationFilter` to emit tool-call events as they happen and a retrieval step to emit citations.

```csharp
await ctx.Response.WriteAsync($"event: tool_call\ndata: {JsonSerializer.Serialize(call)}\n\n");
await ctx.Response.WriteAsync($"event: token\ndata: {delta}\n\n");
```

**Mid-stream errors:** you've already sent a 200 and partial data, so you can't change the status. Emit an `event: error`, persist the partial as `interrupted`, distinguish cancellation from real errors, and flush a terminal event in `finally`.

---

## The Azure OpenAI SDK (`Azure.AI.OpenAI`) directly

Use the SDK directly when you want fine control over OpenAI features (logprobs, structured outputs, vision) without SK's abstraction. Register the client as a **singleton** — it's thread-safe and pools connections. Prefer `DefaultAzureCredential` over `AzureKeyCredential`.

```csharp
var client = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
var chat = client.GetChatClient(deploymentName);

await foreach (var update in chat.CompleteChatStreamingAsync(messages, options, ct))
    foreach (var part in update.ContentUpdate)
        await Response.WriteAsync(part.Text);
```

### Structured outputs

Constrain output to a JSON schema so it's parseable — eliminates brittle regex parsing.

```csharp
var options = new ChatCompletionOptions
{
    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
        "invoice", BinaryData.FromString(schemaJson), jsonSchemaIsStrict: true)
};
var result = await chat.CompleteChatAsync(messages, options, ct);
var invoice = JsonSerializer.Deserialize<Invoice>(result.Value.Content[0].Text);
```

In SK, the equivalent is `ResponseFormat = typeof(InvoiceData)` on the execution settings, or `FunctionChoiceBehavior.Required` targeting a function whose parameters *are* your schema.

---

## Background document ingestion

Don't ingest inside an HTTP request. Decouple via a queue and process in a background worker.

```csharp
public class IngestWorker(IServiceScopeFactory scopes, ServiceBusProcessor proc) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        proc.ProcessMessageAsync += async args =>
        {
            using var scope = scopes.CreateScope();   // scope per message (worker is singleton)
            var pipeline = scope.ServiceProvider.GetRequiredService<IIngestPipeline>();
            await pipeline.ProcessAsync(args.Message.Body, ct);
            await args.CompleteMessageAsync(args.Message);
        };
        await proc.StartProcessingAsync(ct);
    }
}
```

**Choosing the runtime:**

| Option | When |
|---|---|
| `BackgroundService` (hosted service) | Lightweight, always-on, in-process work; scales with your web app |
| **Azure Functions** + Service Bus/Blob trigger | Spiky, event-driven, auto-scaling ingestion; serverless economics |
| **Container Apps jobs / KEDA** | Heavy, long-running embedding work exceeding Function limits |
| **Service Bus** | The durable queue decoupling uploads from workers — back-pressure, retries, DLQ, sessions |

**Idempotency is mandatory** (queues deliver at-least-once): use deterministic chunk ids (hash of document + index) so re-ingestion upserts rather than duplicates, track processed message ids, and checkpoint multi-step pipelines so retries resume. For large files, stream from Blob Storage (don't buffer), extract with Azure AI Document Intelligence, chunk on semantic boundaries with overlap, embed in batches with bounded concurrency + 429 handling.

---

## EF Core + vectors

Azure SQL supports a native `VECTOR` type with `VECTOR_DISTANCE`. With EF Core you map a vector column and run similarity queries — attractive when you already have relational data and want transactional consistency and joins between business data and embeddings.

```csharp
var results = await db.Database.SqlQuery<Chunk>(
    $"""
     SELECT TOP 5 *, VECTOR_DISTANCE('cosine', Embedding, {queryVec}) AS Dist
     FROM Chunks ORDER BY Dist
     """).ToListAsync();
```

**Trade-off:** SQL keeps everything in one system; specialized vector stores (Azure AI Search, Cosmos DB, pgvector) scale and rank better at large volumes and add hybrid + semantic reranking. For most enterprise Microsoft RAG, Azure AI Search is the default; co-locate vectors in Azure SQL only when relational consistency dominates.

---

## `Microsoft.Extensions.AI` (MEAI)

`Microsoft.Extensions.AI` provides unified abstractions — `IChatClient` and `IEmbeddingGenerator` — over different providers (Azure OpenAI, OpenAI, Ollama, etc.), **plus a decorator pipeline** for caching, telemetry, function invocation, and logging. It's the **lower-level building block that Semantic Kernel and the Agent Framework build on**. Use it when you want a thin, swappable abstraction without SK's full orchestration, or to compose cross-cutting concerns declaratively.

```csharp
IChatClient client = new AzureOpenAIClient(uri, cred)
    .GetChatClient(deployment).AsIChatClient();

client = new ChatClientBuilder(client)
    .UseDistributedCache(cache)
    .UseOpenTelemetry()
    .UseFunctionInvocation()
    .Build();
```

Embeddings via the same ecosystem:

```csharp
var generator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
var embeddings = await generator.GenerateAsync(chunks, cancellationToken: ct);
```

MEAI standardizes the .NET AI ecosystem so libraries interoperate — the same `IChatClient` flows through SK, MAF, and your own code.

---

## ASP.NET Core cross-cutting concerns

- **Options pattern.** Bind a strongly-typed options class, `ValidateOnStart()`, source secrets from Key Vault / App Configuration (not appsettings); managed identity for the AI call path. `IOptionsSnapshot` for per-request/per-tenant config.
- **Rate limiting.** ASP.NET Core's rate limiter middleware with a partitioned policy keyed on the user/tenant claim; return 429 + `Retry-After`. Back it with Redis or APIM quotas for distributed enforcement.
- **Health checks.** Liveness (process up) + readiness (can reach Azure OpenAI / vector store / Service Bus) — use a *lightweight* connectivity check, not a full model completion. Wire to Container Apps/AKS probes.
- **Resilience.** `AddStandardResilienceHandler()` on the connector's `HttpClient`; don't double-retry on top of the SDK's built-in retry. See [09-production-patterns.md](09-production-patterns.md).
- **DI hygiene.** Singleton clients (pooled), scoped per-request services, keyed services (`AddKeyedSingleton<IChatClient>("fast"/"smart")`) for model routing; never resolve scoped services from a singleton — use `IServiceScopeFactory` in workers.

---

## Hosting

- **Azure Container Apps (ACA)** — serverless containers with KEDA autoscaling (scale to zero, scale on HTTP/queue depth), managed ingress; the usual default for Azure-OpenAI-backed APIs and ingestion workers.
- **AKS** — full Kubernetes control: custom networking, GPU node pools for self-hosted models, service mesh. Reserve for advanced scenarios.
- Both support managed identity and VNet integration. Front with Azure Front Door (no buffering for SSE) and APIM as an AI gateway for token-based rate limiting, load balancing across deployments, semantic caching, and cost attribution.

---

## Common pitfalls and fixes

| Pitfall | Fix |
|---|---|
| Not forwarding `CancellationToken` | Plumb `RequestAborted` everywhere; wastes tokens on disconnects |
| Buffering the whole stream | Stream and flush; disable proxy/compression buffering (kills TTFT) |
| Self-inflicted 429s | Bound concurrency (rate limiter/semaphore); respect `Retry-After` |
| Storing keys | Managed identity via `DefaultAzureCredential` |
| Re-embedding identical content | Cache embeddings by content hash |
| Stuffing entire history into context | Reduce/summarize history |
| Mismatched embedding models ingest vs query | Pin one model; reindex on change |
| Logging PII in prompts | Log metadata only; gate content capture behind a flag + secure sink |
| Non-idempotent ingestion | Deterministic chunk ids; upsert, dedupe message ids |
| Output quality as unit tests | Use evals (Foundry); unit tests for code correctness |

---

## Interview angle

> "How do you stream from SK to a browser?" `GetStreamingChatMessageContentsAsync` returns `IAsyncEnumerable`; iterate, write `data:` lines over SSE, flush each chunk, disable buffering/compression, and forward `RequestAborted`. "What is `Microsoft.Extensions.AI`?" The unified `IChatClient`/`IEmbeddingGenerator` abstractions plus a middleware pipeline (caching, OTel, function invocation) that **SK itself builds on** — use it for a thin swappable layer. For ingestion: queue + background worker (Functions/Service Bus/Container Apps jobs), idempotent with deterministic ids — never ingest inside an HTTP request.

---

**Next:** [Interview Questions](11-interview-questions.md) — 100 detailed Q&A.
