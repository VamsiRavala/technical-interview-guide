# Overview & Setup

Semantic Kernel (SK) is Microsoft's open-source, **.NET-first** SDK (also Python and Java) for building AI applications that orchestrate large language models alongside ordinary code. It solves the "glue" problem: an LLM is a non-deterministic text engine, but a real app needs it to call APIs, query databases, reason over private data, and follow workflows. SK gives you the `Kernel` — a dependency-injection container that holds AI services (chat completion, embeddings), plugins (callable functions), prompt templates, filters, and connectors — and an orchestration loop where the model can request tool execution and SK handles the round-trips.

**Reality check before you start.** As of mid-2026, Microsoft has been **converging Semantic Kernel and AutoGen into the Microsoft Agent Framework (MAF)**. SK is **not deprecated** — it remains the production-grade, GA, .NET-first orchestration layer (kernel, plugins, connectors, filters, telemetry), and MAF is the unified agent-runtime layer that builds on top of it (see [08-agents-and-maf.md](08-agents-and-maf.md) and section **11**). For interviews, the single most valuable signal is *understanding the trajectory*: SK's `Kernel`/`KernelFunction`/connector primitives are stable and feed directly into MAF agents, while the old high-level **planner** concept has been superseded by **automatic function calling** and **agents**.

---

## The mental model

| Layer | What it is | SK type(s) |
|---|---|---|
| Connectors | Talk to a model / vector store | `IChatCompletionService`, `IEmbeddingGenerator<,>`, vector store connectors |
| Kernel | DI container + function registry + filter (middleware) pipeline | `Kernel`, `Kernel.CreateBuilder()` |
| Functions | Unit of callable work (native C# or prompt) | `KernelFunction`, `[KernelFunction]` |
| Plugins | Named group of functions | `KernelPlugin` |
| Orchestration | Decides which functions to call | Automatic function calling, Agents (was: Planners) |
| Agent layer | Stateful, multi-turn, multi-agent | SK Agent Framework → Microsoft Agent Framework |

```text
        ┌─────────────────────────────────────────────┐
        │                   Kernel                     │
        │  (IServiceProvider + plugins + filters)      │
        │                                              │
  user ─┼─► IChatCompletionService ──► Azure OpenAI    │
        │        ▲          │                          │
        │        │ tool     │ tool                     │
        │        │ result   ▼ call                     │
        │   ┌────┴───────────────┐                     │
        │   │ KernelFunctions    │ (native + prompt)   │
        │   │  [KernelFunction]  │                     │
        │   └────────────────────┘                     │
        └─────────────────────────────────────────────┘
```

---

## The `Kernel` object

The `Kernel` is the central orchestrator: a lightweight container for AI services, plugins, filters, and logging. You build it with `Kernel.CreateBuilder()`, register services and plugins, then call `Build()`.

```csharp
using Azure.Identity;
using Microsoft.SemanticKernel;

var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: cfg["AzureOpenAI:ChatDeployment"]!,   // e.g. "gpt-4o"
    endpoint:       cfg["AzureOpenAI:Endpoint"]!,
    credentials:    new DefaultAzureCredential());

builder.Plugins.AddFromType<WeatherPlugin>("Weather");

Kernel kernel = builder.Build();
```

Key properties:

- It resolves services from its internal `IServiceProvider`, so it integrates cleanly with ASP.NET Core DI.
- It is **cheap to construct** and *not* meant to be a long-lived singleton holding per-request state. You can `kernel.Clone()` to scope plugins or arguments without mutating the shared instance.
- Invocation methods: `InvokePromptAsync`, `InvokeAsync(function, args)`, and (one level down) the chat service's `GetChatMessageContentAsync`.
- The kernel is the object passed to filters and functions so they can re-enter the orchestration loop.

---

## DI registration in ASP.NET Core

SK is built for `Microsoft.Extensions.DependencyInjection`. In ASP.NET Core, register services directly on the host's `builder.Services` so the kernel shares the app container, then call `AddKernel()`.

```csharp
// Program.cs
builder.Services.AddSingleton<IWeatherClient, OpenMeteoClient>();

builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: builder.Configuration["AzureOpenAI:ChatDeployment"]!,
    endpoint:       builder.Configuration["AzureOpenAI:Endpoint"]!,
    credentials:    new DefaultAzureCredential());

builder.Services.AddKernel();   // registers Kernel against the same container
```

Because the kernel resolves from the container, **plugin classes can have constructor-injected dependencies** (repositories, `HttpClient`, options). Lifetime matters:

- Plugins/connectors that are stateless → **singleton**.
- The `Kernel` itself → **transient/scoped** when you need request-scoped arguments, tenant config, or scoped dependencies. Stuffing per-user state into a singleton kernel is a cross-request data-leak waiting to happen.

```csharp
// Build a per-request kernel that carries scoped dependencies
builder.Services.AddTransient(sp =>
{
    var k = new Kernel(sp);
    k.Plugins.AddFromType<WeatherPlugin>("Weather", sp);
    return k;
});
```

---

## `AddAzureOpenAIChatCompletion` with managed identity

SK's Azure OpenAI connector (`Microsoft.SemanticKernel.Connectors.AzureOpenAI`) wires the kernel to your Azure OpenAI deployment. **Never put keys in code** — prefer **managed identity** (`DefaultAzureCredential`) in Azure-hosted apps.

```csharp
using Azure.Identity;
using Microsoft.SemanticKernel;

var builder = Kernel.CreateBuilder();

// Option A: API key (dev / local only)
builder.AddAzureOpenAIChatCompletion(
    deploymentName: cfg["AzureOpenAI:ChatDeployment"]!,
    endpoint:       cfg["AzureOpenAI:Endpoint"]!,
    apiKey:         cfg["AzureOpenAI:ApiKey"]!);

// Option B (PREFERRED in prod): Entra ID / managed identity — no secrets
builder.AddAzureOpenAIChatCompletion(
    deploymentName: cfg["AzureOpenAI:ChatDeployment"]!,
    endpoint:       cfg["AzureOpenAI:Endpoint"]!,
    credentials:    new DefaultAzureCredential());

// Embeddings (for RAG — see 05-memory-and-vectors.md)
builder.AddAzureOpenAIEmbeddingGenerator(
    deploymentName: cfg["AzureOpenAI:EmbedDeployment"]!,   // e.g. "text-embedding-3-small"
    endpoint:       cfg["AzureOpenAI:Endpoint"]!,
    credentials:    new DefaultAzureCredential());

Kernel kernel = builder.Build();
```

**Managed identity requirements:**

- The identity needs the **`Cognitive Services OpenAI User`** RBAC role on the Azure OpenAI resource (least privilege — not Contributor), and the resource must allow Entra ID auth.
- `DefaultAzureCredential` chains: Managed Identity in Azure → `az login` / Visual Studio credentials locally → environment variables. The same code works on your machine *and* in AKS / Container Apps / App Service.
- For a user-assigned identity, set `ManagedIdentityClientId` in the credential options.

**Multiple models in one kernel.** Register each with a distinct `serviceId`, then select per call (cheap/fast model for routing, powerful model for hard reasoning):

```csharp
builder.AddAzureOpenAIChatCompletion("gpt-4o-mini", ep, cred, serviceId: "fast");
builder.AddAzureOpenAIChatCompletion("gpt-4o",      ep, cred, serviceId: "smart");

var settings = new AzureOpenAIPromptExecutionSettings { ServiceId = "smart" };
await kernel.InvokePromptAsync("Solve this proof…", new(settings));
```

---

## Production gotchas

- **Deployment name vs model name.** SK's `deploymentName` is the *Azure deployment* you created, not `gpt-4o` the model id. They are often named alike, which hides the bug until someone renames a deployment. Always read endpoint/deployment from configuration; never hard-code.
- **Quotas and 429s.** Azure OpenAI enforces per-deployment TPM/RPM quotas; under load you get HTTP 429. Handle with retry + backoff that honors `Retry-After`, and consider Provisioned Throughput Units (PTUs) or multi-region load balancing for SLA-bound workloads (see [09-production-patterns.md](09-production-patterns.md)).
- **`DefaultAzureCredential` locally.** If you see auth failures on your dev box, you are not logged in (`az login`) or your account lacks the RBAC role on the resource — the credential chain silently falls through.

---

## Interview angle

> "How do you authenticate SK to Azure OpenAI in production?" The wrong answer is an API key in `appsettings.json`. The right answer is **managed identity via `DefaultAzureCredential`**, RBAC role `Cognitive Services OpenAI User`, keys only for local dev, and any residual secrets in Key Vault. Bonus points for naming PTUs and 429 handling for scale, and for noting that the kernel should be cheap/scoped rather than a stateful singleton.

---

**Next:** [Plugins & Functions](02-plugins-and-functions.md) — how a plain C# method becomes a tool the model can call.
