# Semantic Kernel — Microsoft's .NET-First LLM Orchestration Layer

> The C#-first SDK that turns a `Kernel` + plugins + connectors + filters into production AI: function calling, RAG over `Microsoft.Extensions.VectorData`, agents, and a clean path into the Microsoft Agent Framework. Accurate to SK's GA .NET API as of **mid-2026**.

---

## 📚 Table of Contents

### Foundations

1. **[Overview & Setup](01-overview-and-setup.md)** - What SK is, the mental model, `Kernel` construction, DI in ASP.NET Core, and `AddAzureOpenAIChatCompletion` with managed identity
2. **[Plugins & Functions](02-plugins-and-functions.md)** - Plugins, `[KernelFunction]`, native vs prompt functions, `KernelArguments`, and the legacy Skills → Plugins rename

### Orchestration

3. **[Function Calling](03-function-calling.md)** - Automatic function calling, `FunctionChoiceBehavior` Auto/Required/None, the tool-call loop, and manual mode
4. **[Prompt Templates](04-prompt-templates.md)** - Default `{{$var}}`, Handlebars, Liquid, `.prompty`, variables, inline function calls, and safety

### Knowledge & Retrieval

5. **[Memory & Vectors](05-memory-and-vectors.md)** - The `Microsoft.Extensions.VectorData` abstraction, the Azure AI Search connector, embeddings, and the dimension footgun
6. **[RAG with SK](06-rag-with-sk.md)** - End-to-end RAG: ingest → retrieve → augment → generate, RAG-as-prompt vs RAG-as-tool, grounding and citations

### Production

7. **[Filters & Middleware](07-filters-and-middleware.md)** - The three filter types, logging/PII/guardrails, caching, and human-in-the-loop approval gates
8. **[Agents & MAF](08-agents-and-maf.md)** - The SK Agent Framework, threads/state, multi-agent orchestration, and SK's convergence into the Microsoft Agent Framework
9. **[Production Patterns](09-production-patterns.md)** - Streaming, resilience/Polly + 429s, caching, OpenTelemetry, testing/eval, and multi-tenancy
10. **[.NET + AI Integration](10-dotnet-ai-integration.md)** - ASP.NET Core streaming/SSE, `Azure.AI.OpenAI`, background ingestion, EF Core + vectors, and `Microsoft.Extensions.AI`

### Interview Preparation

11. **[Interview Questions](11-interview-questions.md)** - 100 detailed Q&A: 50 Semantic Kernel + 50 .NET + AI integration

---

## 🎯 Learning Path

### Beginner (Week 1-2)
1. Read **Overview & Setup** and stand up a `Kernel` wired to Azure OpenAI with `DefaultAzureCredential`
2. Work through **Plugins & Functions** — write one native `[KernelFunction]` and one prompt function
3. Do **Function Calling** — get the model to call your function with `FunctionChoiceBehavior.Auto()`

### Intermediate (Week 3-4)
1. Learn **Prompt Templates** — convert a hand-built prompt to a Handlebars template that loops over data
2. Build **Memory & Vectors** then **RAG with SK** — index docs into Azure AI Search and ground answers with citations
3. Add **Filters & Middleware** — a logging filter, a PII-redaction filter, and a tool-approval gate

### Advanced (Week 5-6)
1. Master **Agents & MAF** — build a `ChatCompletionAgent`, persist a thread, and explain the SK → MAF convergence
2. Internalize **Production Patterns** and **.NET + AI Integration** — streaming SSE, 429 resilience, OTel, multi-tenant isolation
3. Drill **Interview Questions** and rehearse the "SK vs MAF vs raw SDK" and "planners are superseded" answers out loud

---

## 🔑 Mental Model at a Glance

| Layer | What it is | SK type(s) |
|---|---|---|
| **Connectors** | Talk to a model / vector store | `IChatCompletionService`, `IEmbeddingGenerator<,>`, vector store connectors |
| **Kernel** | DI container + function registry + filter pipeline | `Kernel`, `Kernel.CreateBuilder()` |
| **Functions** | A unit of callable work (native C# or prompt) | `KernelFunction`, `[KernelFunction]` |
| **Plugins** | A named group of functions | `KernelPlugin` |
| **Orchestration** | Decides which functions to call | Automatic function calling, Agents (was: Planners) |
| **Agent layer** | Stateful, multi-turn, multi-agent | SK Agent Framework → Microsoft Agent Framework |

---

## 💡 Key Decisions

| Question | Short answer |
|---|---|
| **Auth to Azure OpenAI?** | Managed identity via `DefaultAzureCredential`, RBAC `Cognitive Services OpenAI User`. Keys only for local dev. |
| **Orchestrate multi-step?** | Automatic function calling. **Planners are superseded** — reserve only for inspectable, human-approvable plans. |
| **Cross-cutting concerns?** | Filters: `IFunctionInvocationFilter`, `IPromptRenderFilter`, `IAutoFunctionInvocationFilter`. Not events (deprecated). |
| **Vector store?** | `Microsoft.Extensions.VectorData` abstraction; Azure AI Search (hybrid + semantic ranker) for prod. |
| **New multi-agent system?** | Target the **Microsoft Agent Framework** (section 11). SK stays the connector/kernel/filter foundation. |

> **Where the rest lives:** Azure OpenAI / Azure AI Search infrastructure is section **09**; deep Microsoft Agent Framework and multi-agent patterns are section **11**; RAG architecture end-to-end is section **12**. This section keeps SK's API surface front and center and points to those for depth.
