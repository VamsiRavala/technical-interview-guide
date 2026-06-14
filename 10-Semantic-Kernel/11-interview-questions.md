# Interview Questions

100 detailed Q&A for Semantic Kernel and .NET + AI integration roles. The first 50 cover Semantic Kernel itself; the next 50 cover the broader .NET + AI integration surface (ASP.NET Core, SDKs, ingestion, resilience, hosting). Microsoft Agent Framework-specific questions live in section **11**.

---

### Semantic Kernel

**Q1.** What is Semantic Kernel and what problem does it solve?

Semantic Kernel (SK) is Microsoft's open-source SDK (.NET, Python, Java) for building AI applications that orchestrate LLMs alongside conventional code. It solves the "glue" problem: LLMs are non-deterministic text engines, but real apps need them to call APIs, query databases, reason over data, and follow workflows. SK provides the `Kernel` as a DI container holding AI services (chat completion, embeddings), plugins (callable functions), prompt templates, filters, and memory. The kernel is an orchestration layer where the model can request tool execution and SK handles the round-trips. Its value proposition is enterprise readiness: Azure OpenAI integration, managed identity, OpenTelemetry, pluggable connectors, and a consistent abstraction that survives model swaps. As of mid-2026 it is converging into the Microsoft Agent Framework while remaining the GA connector/orchestration core.

**Q2.** What is the `Kernel` object and how do you build one?

The `Kernel` is the central orchestrator: a lightweight container for AI services, plugins, filters, and logging. Build it with `Kernel.CreateBuilder()`, register services/plugins, then `Build()`. It resolves services from its internal `IServiceProvider`, integrating cleanly with ASP.NET Core DI. It's intentionally cheap to create — clone it per-request with `kernel.Clone()` to scope plugins/arguments without mutating the shared instance. Invocation methods: `InvokePromptAsync`, `InvokeAsync(function, args)`, and the chat service's `GetChatMessageContentAsync`. The kernel is the object passed to filters and functions so they can re-enter the orchestration loop. It is not meant to be a long-lived singleton holding per-request state.

**Q3.** What is a plugin in Semantic Kernel?

A plugin is a named collection of functions the kernel can invoke — the unit of capability you expose to the model. Two flavors: *native* plugins (C# classes with `[KernelFunction]`-decorated methods) and *prompt* plugins (functions defined by a prompt template plus config). Register native plugins with `Plugins.AddFromType<T>()` (honors DI) or `AddFromObject(instance)`. Each function carries metadata — name, description, parameter descriptions — that SK serializes into the tool schema sent to the model. Good plugin design mirrors good API design: clear names, precise `[Description]` attributes, narrow single-purpose functions. Plugins can also be imported from OpenAPI specs or MCP servers. They are the bridge between deterministic code and model reasoning.

**Q4.** Explain the `[KernelFunction]` attribute.

It marks a method on a native plugin class as callable by the kernel and exposable to the LLM for function calling. SK reflects over the method to build a `KernelFunction` descriptor: name (overridable), description (from `[Description]`), and parameter schema (names, types, descriptions, defaults). This becomes the JSON tool definition the model sees. Quality of descriptions directly drives model accuracy — the model only knows what the schema tells it. Methods can be sync or async, return primitives, strings, or complex objects (JSON-serialized), and may accept the `Kernel` or `KernelArguments`. Without the attribute, a method is invisible to the kernel.

**Q5.** What is automatic function calling and how do you enable it?

It lets the model decide which functions to invoke, with SK executing them and feeding results back, looping until a final answer. Enable it via `FunctionChoiceBehavior.Auto()` on the execution settings. SK serializes kernel plugin functions as tools, sends them with the prompt, receives the model's tool-call requests, executes the matching `KernelFunction`s, appends results to chat history, and re-invokes — fully managed, multiple iterations. This replaced the older planner approach for most scenarios because native tool-calling is faster, cheaper, and more reliable. You can scope advertised functions, cap iterations, and intercept each call via an `IAutoFunctionInvocationFilter`.

**Q6.** Compare `FunctionChoiceBehavior.Auto`, `Required`, and `None`.

`Auto()` lets the model freely decide whether and which functions to call — the default for agentic behavior. `Required()` forces at least one function call on the next turn (structured extraction, "must use a tool"); you can target specific functions. `None()` advertises functions for context but forbids calling them — for dry-runs, previews, explanation modes. Each accepts options such as `autoInvoke: false` (return the tool-call request without executing) and a filtered function list. `FunctionChoiceBehavior` unified the older `ToolCallBehavior` API. Choosing the right mode is a common production decision: `Required` for deterministic extraction, `Auto` for assistants, `None` for confirmation UX.

**Q7.** What are `KernelArguments`?

A dictionary-like bag (`IDictionary<string, object?>`) that carries input variables and execution settings into an invocation. Template placeholders like `{{$city}}` bind to keys; native parameters bind by name. It also carries `PromptExecutionSettings` (temperature, max tokens, `FunctionChoiceBehavior`) per call. Because it's a single object threaded through the pipeline, arguments are visible to filters for inspection or mutation before execution. You can supply multiple service-specific settings keyed by service id for multi-model setups. It keeps the kernel stateless across calls.

**Q8.** How do prompt templates work?

Parameterized prompts with placeholders SK renders before sending to the model. The default syntax uses `{{$variable}}` for arguments and `{{plugin.function $arg}}` to call functions inline. Templates become `KernelFunction`s via `CreateFunctionFromPrompt`, optionally with `PromptTemplateConfig` declaring input variables, defaults, and execution settings. Rendering is a pipeline stage observable by prompt-render filters, so you can inspect/rewrite the final prompt (inject guardrails, redact PII) before the model sees it. Beyond the default format, SK supports Handlebars and Liquid. Templates externalize prompts from code, enabling versioning and prompt engineering without recompilation when loaded from files (`.prompty`/YAML).

**Q9.** What template engines does SK support — Handlebars and Liquid?

Beyond the native `{{$var}}` format, SK offers `HandlebarsPromptTemplateFactory` and `LiquidPromptTemplateFactory`. Handlebars adds control flow — `{{#each}}`, `{{#if}}`, helpers, partials — ideal for prompts that loop over chat history, search results, or few-shot examples. Liquid similarly supports loops, conditionals, and filters. You select the factory when creating the function; the format is recorded in config so prompts loaded from YAML declare which engine to use. Choose Handlebars/Liquid when prompts have structural logic; the native format suffices for simple substitution and inline function calls. Essential for RAG prompts iterating over retrieved chunks with citations.

**Q10.** What is `IChatCompletionService`?

SK's abstraction over chat-based LLMs. It exposes `GetChatMessageContentAsync` (single response) and `GetStreamingChatMessageContentsAsync` (token streaming), both taking a `ChatHistory`, optional `PromptExecutionSettings`, and the `Kernel`. Connectors register concrete implementations (`AddAzureOpenAIChatCompletion`, `AddOpenAIChatCompletion`, `AddOnnxRuntimeGenAIChatCompletion`, `AddOllamaChatCompletion`), so your code is provider-agnostic. Passing the `Kernel` is what enables function calling at this layer. When multiple services are registered, select by service id. It's the lowest-level building block — `InvokePromptAsync` and agents are conveniences on top — and the seam you mock in unit tests.

**Q11.** How does memory work in Semantic Kernel?

"Memory" means giving the model relevant context beyond the current prompt — typically via RAG. Classic flow: embed documents, store vectors, then at query time embed the question, retrieve nearest neighbors, inject into the prompt. SK historically offered `ISemanticTextMemory`/`MemoryStore`, but the modern approach is `Microsoft.Extensions.VectorData` plus connectors. Distinguish short-term memory (the `ChatHistory` in a single conversation) from long-term memory (persisted vectors/facts). For production, manage history size with truncation/summarization. Memory plugins can be exposed as functions so the model retrieves on demand. The key point: memory is RAG plumbing plus history management, not the model magically remembering.

**Q12.** What is `Microsoft.Extensions.VectorData`?

The modern, provider-neutral vector store abstraction shared across the .NET AI ecosystem. It defines `IVectorStore` and a typed collection with attribute-driven mapping — annotate a POCO with key/data/vector attributes, then upsert and run similarity searches against any supported store with the same code. Connectors implement it for Azure AI Search, Cosmos DB, Redis, Qdrant, Postgres/pgvector, SQL Server, and in-memory. This decoupling means you prototype with `InMemoryVectorStore` and swap to Azure AI Search in production with no logic change. It replaces the older `IMemoryStore` design with stronger typing and richer filtering. (Verify current attribute names — they evolved across previews.)

**Q13.** How do you use the Azure AI Search connector with SK?

Using `Microsoft.SemanticKernel.Connectors.AzureAISearch`, register an `AzureAISearchVectorStore` backed by a `SearchIndexClient` authenticated via `DefaultAzureCredential`. Get a typed collection, ensure the index exists, upsert records, and run vectorized search. Azure AI Search adds hybrid search (vector + BM25 keyword), semantic ranking, and filterable metadata fields — strong for enterprise RAG. Use managed identity (not admin keys), scope RBAC to the index, and keep the embedding dimension aligned with your embedding model. It pairs naturally with Azure OpenAI embeddings in the same tenant.

**Q14.** What are filters/middleware in Semantic Kernel?

SK's middleware mechanism — interceptors that wrap pipeline stages so you add cross-cutting concerns (logging, validation, caching, security, retries) without touching function code. They follow the `next` delegate pattern like ASP.NET middleware. Three types: `IFunctionInvocationFilter` (every function execution), `IPromptRenderFilter` (prompt rendering), `IAutoFunctionInvocationFilter` (each auto-invoked function in the tool-calling loop). Registered in DI. Filters can short-circuit (skip `next`), mutate arguments, rewrite results, or terminate auto-invocation early. They are the production hook for guardrails, PII redaction, cost tracking, and approval gates, and replaced the old `FunctionInvoking`/`FunctionInvoked` events.

**Q15.** Explain the function-invocation filter.

`IFunctionInvocationFilter` intercepts every `KernelFunction` execution — native or prompt. `OnFunctionInvocationAsync(context, next)` exposes `Function`, `Arguments`, `Kernel`, and (after `await next`) `Result`. Run logic before (validate/rewrite args, authorize), call `next`, then after (log latency, transform result, mask output). Skip `next` to short-circuit with a canned result (caching, blocking). It fires for *all* invocations including auto-function-calling ones, so it's broader than the auto filter. Resolved from DI, so it can inject loggers, caches, policy services. Order matters — filters nest in registration order. The workhorse for observability and policy.

**Q16.** Explain the prompt-render filter.

`IPromptRenderFilter` wraps rendering a prompt template into its final string before the model sees it. `OnPromptRenderAsync(context, next)` gives a `PromptRenderContext` with `Function`, `Arguments`, `Kernel`; calling `next` performs the render, after which `context.RenderedPrompt` holds the final text you may inspect or overwrite. Ideal for injecting guardrails, appending safety instructions, redacting PII from interpolated variables, enforcing length budgets, or prompt caching (set `context.Result` to skip rendering and the model call). Because it sees the fully resolved prompt, it's your last defense against prompt injection from user-supplied template variables. Also enables prompt logging for audit.

**Q17.** Explain the auto-function-invocation filter.

`IAutoFunctionInvocationFilter` fires once per function the model chooses to invoke inside the tool-calling loop. `OnAutoFunctionInvocationAsync(context, next)` provides an `AutoFunctionInvocationContext` with `Function`, `Arguments`, `ChatHistory`, the result, the iteration/sequence index, and a `Terminate` flag. Setting `context.Terminate = true` stops the loop early — perfect for "stop after the first successful action" or human-in-the-loop approval. You can inspect what the model decided to call before allowing it, implement per-tool approval, gate destructive operations, or short-circuit runaway loops. It differs from the function-invocation filter by being scoped to model-driven calls and exposing chat history and termination control.

**Q18.** What were planners in Semantic Kernel?

Planners used the LLM to *generate a plan* — a sequence of plugin calls — to achieve a goal, then executed it. Early planners: SequentialPlanner (ordered list of steps), ActionPlanner (single best function), Handlebars/Stepwise planners (the Stepwise one ran a ReAct-style reason-act-observe loop). The idea: give the model a goal and your function catalog and it figures out orchestration. Planners were powerful but had real drawbacks — extra LLM round-trips, brittle plan parsing, hallucinated steps, difficulty handling mid-plan errors. They predated reliable native tool-calling. SK deprecated most planners in favor of `FunctionChoiceBehavior.Auto` and, for complex orchestration, the Agent Framework.

**Q19.** Why were planners superseded by function calling and agents?

Three reasons. *Reliability*: modern models have native, well-trained function-calling that produces structured tool calls directly, so the separate plan-generation step (prone to malformed plans and hallucinated function names) became unnecessary. *Latency and cost*: planners required an upfront LLM call to synthesize the plan, then more to execute; automatic function calling interleaves reasoning and action with fewer round-trips. *Adaptability*: a precomputed plan is brittle — if a step returns unexpected data, the rigid plan can't react, but turn-by-turn calling lets the model observe each result and adjust. The simple case went to `Auto`; the complex, stateful case went to agents/workflows — leaving little room for the original planners.

**Q20.** What is the Semantic Kernel Agent Framework?

An abstraction layer (`Microsoft.SemanticKernel.Agents.*`) for building agents — entities with instructions, a kernel (tools), and the ability to participate in conversations. Core types: `ChatCompletionAgent` (backed by a chat model), `OpenAIAssistantAgent` (Assistants API), `AzureAIAgent` (Azure AI Agent Service). Agents expose `InvokeAsync`/`InvokeStreamingAsync` and carry an `AgentThread` for state. The framework adds *orchestration* for multi-agent systems: sequential pipelines, concurrent fan-out, group chat with selection/termination strategies, and handoff. This is where SK goes beyond single prompt-response into collaborating specialists. In 2026 it is converging with AutoGen into the unified Microsoft Agent Framework.

**Q21.** How do you integrate Azure OpenAI with `AddAzureOpenAIChatCompletion`?

It (in `Microsoft.SemanticKernel.Connectors.AzureOpenAI`) registers an `IChatCompletionService` bound to an Azure OpenAI deployment. Supply the *deployment name* (not the base model name), the endpoint, and credentials — prefer `TokenCredential`/managed identity over keys. Overloads exist for API key, a preconfigured `AzureOpenAIClient`, and a custom HTTP client. `serviceId` lets you register multiple deployments and select per-call. A parallel embedding registration exists for RAG. The Azure connector (vs plain OpenAI) gives managed identity, private endpoints, regional data residency, content filtering, and quota management. Read endpoint/deployment from configuration; let `DefaultAzureCredential` resolve the identity across local dev and production.

**Q22.** How do you use managed identity with SK and Azure OpenAI?

Pass a `TokenCredential` — typically `DefaultAzureCredential` — instead of an API key. The credential tries a chain (environment variables, managed identity, Azure CLI, Visual Studio), so the same code works locally (dev login) and in production (the app's managed identity). On the Azure side, assign the identity the *Cognitive Services OpenAI User* role on the resource (least privilege, not Contributor). For user-assigned identities, configure the client id. The same pattern applies to Azure AI Search and Key Vault. Benefits: no keys to rotate or leak, RBAC-auditable access, Conditional Access support. This is a near-universal expectation in enterprise Microsoft AI architectures.

**Q23.** How does SK integrate with dependency injection?

SK is built for `Microsoft.Extensions.DependencyInjection`. `Kernel.CreateBuilder()` exposes a `Services` collection, so connector registrations, filters, plugins-with-dependencies, loggers, and `HttpClient`s flow through standard DI. In ASP.NET Core, register on the host's `builder.Services`, then inject `Kernel` into handlers. Plugin classes can have constructor-injected dependencies. Lifetime matters: register the kernel transient/scoped if plugins hold per-request state, singleton if stateless. Filters registered in DI are automatically discovered. This DI-first design means SK apps follow normal .NET configuration, options, logging, and testing conventions — a key enterprise-readiness point.

**Q24.** How do you implement streaming responses in SK?

Use the streaming variants returning `IAsyncEnumerable`: `kernel.InvokePromptStreamingAsync`, `chatService.GetStreamingChatMessageContentsAsync`, or `agent.InvokeStreamingAsync`. Each yielded item is a `StreamingChatMessageContent` carrying a token delta and metadata. In ASP.NET Core, forward these as SSE or over SignalR/WebSockets. Nuances: streaming works alongside automatic function calling — SK executes tool calls mid-stream and content resumes after; filters still apply; usage/token metadata typically arrives in the final chunk; for agents, streaming surfaces intermediate steps. Handle cancellation tokens so abandoned requests stop generating — and stop billing.

**Q25.** How do you add telemetry and OpenTelemetry to SK?

SK is instrumented with `ILogger`, `Meter` (metrics), and `ActivitySource` (traces) following OpenTelemetry GenAI semantic conventions. Configure an OTel pipeline subscribing to SK's sources (`Microsoft.SemanticKernel*`) and export to Azure Monitor/Application Insights. This captures model calls, token usage, function invocations, latencies, and (gated behind a sensitive-data switch) prompt/response content. Metrics include token counts and durations; traces show the full orchestration with each auto-invoked function as a child span. Combined with filters (custom spans, cost counters) you get end-to-end observability — track p95 latency, token spend per route, and tool-call error rates.

**Q26.** How do you unit test Semantic Kernel code?

Test deterministic code directly and mock the non-deterministic boundary. Native plugin functions are plain methods — unit test with no kernel. For code that calls the model, mock `IChatCompletionService` (or the embedding generator) to return canned `ChatMessageContent`, so you assert orchestration logic, prompt construction, and result handling without hitting Azure OpenAI. Test filters by invoking them with a constructed context and a stub `next`. For function-calling flows, script a sequence of tool-call responses to verify the loop. Use `InMemoryVectorStore` for deterministic RAG retrieval. Add a small suite of *evaluation* tests against a real model asserting on properties (contains a citation, valid JSON), not exact strings — keep these separate from fast unit tests.

**Q27.** What production patterns matter when deploying SK apps?

*Security*: managed identity, least-privilege RBAC, secrets in Key Vault, prompt-render filters for injection defense, content safety. *Resilience*: `HttpClient` with Polly retry/circuit-breaker for transient 429/503, honor `Retry-After`, sensible timeouts and `CancellationToken` propagation. *Cost control*: cap `MaxTokens`, limit function-calling iterations, cache embeddings and frequent prompts, choose the cheapest adequate model, track token spend via OTel. *Observability*: OpenTelemetry to App Insights. *Context management*: truncate/summarize `ChatHistory`. *Scalability*: keep the kernel lightweight, clone per request, consider PTUs. *Reliability*: `Required` + structured outputs for extraction. *Governance*: audit logging, content filtering, HITL approval via auto-function-invocation filters. Externalize prompts and run an eval harness in CI.

**Q28.** How do you manage `ChatHistory` and context windows?

`ChatHistory` is the ordered list of messages sent to the model. It grows every turn and eventually exceeds the context window, inflating cost/latency. Strategies: *truncation* — keep the system message plus last N turns (`ChatHistoryTruncationReducer`); *summarization* — periodically replace older turns with a model-generated summary (`ChatHistorySummarizationReducer`), preserving gist while shrinking tokens; *selective retention* — pin important facts, drop chit-chat; *external memory* — offload long-term facts to a vector store and retrieve on demand. Apply reduction before each model call. Summarization preserves coherence at the cost of an extra call; truncation is cheap but forgetful. Mismanaging history is a classic production bug — runaway cost and "context lost" complaints.

**Q29.** How do you import an OpenAPI plugin into SK?

SK turns an OpenAPI (Swagger) spec into a plugin so the model can call REST endpoints as functions. Use `ImportPluginFromOpenApiAsync` (or the URI overload), supplying execution parameters such as auth callbacks and server overrides. SK parses operations into `KernelFunction`s using `operationId`, summaries, and parameter schemas — so good API documentation directly improves model tool selection. Powerful for enterprise integration without writing native wrappers. Cautions: large specs create many tools that overwhelm the model and context window (filter to needed operations); secure the auth callback (managed identity/OBO); gate destructive operations behind approval filters. OpenAPI import, alongside MCP, is a key way agents reach existing systems.

**Q30.** How do you do RAG end-to-end in Semantic Kernel?

(1) *Ingest* — chunk documents, embed with the embedding service (Azure OpenAI `text-embedding-3-large`), upsert into a vector store collection via `Microsoft.Extensions.VectorData`. (2) *Retrieve* — embed the question, run vectorized/hybrid search, return top-k chunks with metadata. (3) *Augment* — inject chunks into the prompt with a Handlebars template iterating results, including citations. (4) *Generate* — call the chat model. Better still, expose retrieval as a `[KernelFunction]` so the model retrieves on demand via function calling. Add re-ranking, citation enforcement (grounding), and a filter to detect ungrounded answers. The canonical enterprise SK use case.

**Q31.** What is the difference between native and semantic (prompt) functions?

A *native function* is C# code marked `[KernelFunction]` — deterministic logic (call an API, query a DB, do math, format data): precise, testable, fast. A *semantic (prompt) function* is defined by a prompt template plus config; executing it renders the template and calls the LLM, producing natural-language or structured output — for fuzzy, language-centric tasks (summarize, classify, rewrite). The kernel treats both as `KernelFunction`s, so the model can call either via function calling, and you can compose them — a prompt template can invoke a native function inline. You author prompt functions in code (`CreateFunctionFromPrompt`) or load from files (`skprompt.txt`+`config.json`, or `.prompty` YAML). The kernel's power is unifying deterministic code and probabilistic language operations under one invocation model.

**Q32.** How do you get structured / JSON output from SK?

Combine three techniques. First, the model's *structured outputs* / JSON schema feature via execution settings — set `ResponseFormat` to a .NET type so the model returns conforming JSON, then deserialize. Second, for tool-based extraction use `FunctionChoiceBehavior.Required` targeting a function whose parameters *are* your schema — the model fills the arguments. Third, validate and, on failure, re-prompt with the validation error (self-correction loop). Structured outputs dramatically reduce parsing failures versus "please return JSON" prompting — essential for production pipelines feeding downstream systems. Pair with a function-invocation filter that validates against the schema before accepting.

**Q33.** How do you handle errors and rate limits (429s) in SK?

Azure OpenAI returns 429 on quota exhaustion, often with `Retry-After`. SK uses an injectable `HttpClient`, so configure resilience at the HTTP layer with `Microsoft.Extensions.Http.Resilience` (Polly): exponential backoff with jitter honoring `Retry-After`, plus a circuit breaker. Beyond retries: provision adequate quota/PTUs, throttle/queue client-side to smooth bursts, consider a fallback deployment in another region. In filters, catch `HttpOperationException`/`ClientResultException` to log and surface friendly errors. Cap function-calling iterations to avoid runaway cost on errors. Distinguish transient failures (429/503 — retry) from permanent ones (400, content-filter blocks — don't retry, fix the input).

**Q34.** How do you register and use multiple models in one kernel?

Register each model with a distinct `serviceId`, then select per call — a cheap/fast model for routing and a powerful model for hard reasoning, or separate chat and embedding models. Set `ServiceId` on the execution settings, or resolve explicitly with `kernel.GetRequiredService<IChatCompletionService>("fast")`. `KernelArguments` can carry service-id-keyed settings so a single function adapts. Use cases: cost optimization, capability tiers, multi-region failover, comparing outputs. This model-routing pattern is sometimes driven by a classifier function that picks the tier. SK's service-id mechanism makes multi-model orchestration first-class without conditional plumbing throughout your code.

**Q35.** What is the role of `PromptExecutionSettings`?

It carries per-invocation knobs: `Temperature`, `TopP`, `MaxTokens`, `FrequencyPenalty`, `StopSequences`, `ResponseFormat`, `ServiceId`, and `FunctionChoiceBehavior`. Connector-specific subclasses (`AzureOpenAIPromptExecutionSettings`, etc.) expose provider features. Attach settings to `KernelArguments` or pass to chat-service calls, so the same function can run deterministically (temperature 0 for extraction) or creatively (higher for brainstorming). When loading prompt functions from YAML/`.prompty`, defaults are declared in config and overridable at call time. Because settings live in arguments, filters can inspect and adjust them (clamp `MaxTokens` for cost control). This is where you tune the determinism/cost/creativity tradeoff and enable function calling.

**Q36.** How do you expose retrieval (memory) as a function the model calls?

Rather than always front-loading retrieved context (wasting tokens when retrieval isn't needed), expose search as a `[KernelFunction]` so the model decides when to retrieve — agentic RAG. With `FunctionChoiceBehavior.Auto`, the model calls the search function only when it needs grounding, possibly multiple times with refined queries (iterative retrieval). This improves both cost (no retrieval on chit-chat) and quality (the model formulates better queries than the raw user text and can re-search). It's the foundation of modern agentic/iterative RAG and the bridge from "memory as plumbing" to "memory as a tool."

**Q37.** How does SK support content safety / responsible AI?

SK leverages Azure's safety stack and its own extensibility. Azure OpenAI applies *content filters* on prompts and completions and can block/annotate; you handle filtered-response errors. *Azure AI Content Safety* and *Prompt Shields* detect jailbreaks and prompt-injection in user input and documents (indirect injection). In SK code, you enforce responsible AI via filters: a prompt-render filter to inject guardrails and sanitize user text, a function-invocation filter to block disallowed actions or scan tool outputs, an auto-function-invocation filter for human approval of risky operations. Also: compliant audit logging, grounding to reduce RAG hallucination, output validation. The layered approach — platform filters + Prompt Shields + SK filters + grounding + HITL — is the expected enterprise answer.

**Q38.** What is the `.prompty` format and how does SK use it?

A YAML-based, language-agnostic prompt asset format (also used in Azure AI/Prompt Flow) that bundles a prompt template, model configuration, sample inputs, and metadata in one file — making prompts versionable, shareable, and tool-supported. SK loads a `.prompty` file into a `KernelFunction` via `CreateFunctionFromPrompty`, so prompt engineers iterate on the file while developers consume it as a function. Benefits: prompts live outside code (no recompile to tweak), defaults and model config travel with the prompt, and the format is portable across Microsoft AI tooling for evaluation and deployment — clean separation of concerns and prompt lifecycle management, a maturity signal for managing prompts as first-class versioned assets.

**Q39.** How do filters enable human-in-the-loop approval?

Filters pause orchestration to require human confirmation before a consequential action. The cleanest hook is `IAutoFunctionInvocationFilter`: when the model decides to call a sensitive function, the filter checks an approval policy and, if approval is needed, sets `context.Terminate = true` and surfaces a pending action to the UI rather than executing. Alternatively set `autoInvoke: false` so SK returns the tool-call request to your code, you confirm with the user, then execute and feed the result back. Because thread state serializes, you can pause indefinitely and resume later — true asynchronous HITL. In the Microsoft Agent Framework this is formalized as approval workflows. Essential for high-stakes actions and a strong governance talking point.

**Q40.** How do you cache results to reduce cost and latency in SK?

Implement caching in a filter. A *prompt-render filter* computes a cache key from the rendered prompt and, on hit, sets the result to skip the model call. A *function-invocation filter* caches deterministic outputs keyed by arguments. Use a distributed cache (Redis) for multi-instance deployments. For *semantic caching*, embed the query and serve a cached answer when a previous query is sufficiently similar — only for stable, factual queries. Also cache embeddings of ingested documents (compute once at ingest). Tradeoffs: caching saves money/latency but risks staleness and is unsafe for personalized or time-sensitive answers, so scope carefully and set TTLs.

**Q41.** How do you cap and control automatic function-calling iterations?

Without limits, the loop can iterate excessively — inflating cost/latency or getting stuck. Controls: `FunctionChoiceBehavior.Auto` options expose a maximum auto-invoke attempt count (verify the exact name for your SK version). Terminate early from an `IAutoFunctionInvocationFilter` (`Terminate = true`) once a goal is met or after N calls you track. Scope the advertised function set so the model has fewer tools to chain; set `MaxTokens` to bound each turn; detect repeated identical tool calls (a loop signal) and break. For deterministic flows without chaining, use `Required` with `autoInvoke: false` and orchestrate manually. Monitor iteration counts via OTel. Agentic loops need explicit budgets — iteration caps, token caps, filter-based circuit breakers.

**Q42.** How do you choose between OpenAI Assistants, Azure AI Agent Service, and `ChatCompletionAgent`?

`ChatCompletionAgent` runs entirely on your side using `IChatCompletionService` — you control history, tools, and state; lowest dependency, most portable, any chat model. `OpenAIAssistantAgent` delegates to the Assistants API, which manages threads, tool execution (code interpreter, file search), and state server-side — less code but tied to that API. `AzureAIAgent` (Azure AI/Foundry Agent Service) is the Azure-managed runtime with enterprise features: managed threads, built-in tools (Bing grounding, AI Search, code interpreter), Entra integration, governance — best for managed, governed agents in Azure. Choose `ChatCompletionAgent` for control/portability or local models; `AzureAIAgent` for enterprise Azure; `OpenAIAssistantAgent` for specific Assistants features. As of 2026 these converge under MAF — verify current type names.

**Q43.** How do SK agents maintain conversation state with `AgentThread`?

An `AgentThread` represents a conversation's state — message history and any provider-managed context. Each `InvokeAsync` advances a thread; passing the same thread preserves context, a new one starts fresh. For `ChatCompletionAgent` the thread is a client-side `ChatHistory` wrapper you control; for `OpenAIAssistantAgent`/`AzureAIAgent` it's server-managed and you hold a thread id. This lets you persist and resume conversations: serialize the thread/history to a store keyed by user/session and rehydrate on the next request — essential for stateless web apps. Apply history reduction to threads to manage context size. In multi-agent orchestration, threads coordinate the shared conversation. MAF formalizes thread state and serialization further.

**Q44.** How do you implement multi-agent orchestration in SK?

The Agent Framework provides orchestration primitives. *Sequential* — agents run in a pipeline, each consuming the previous output (research → draft → edit). *Concurrent* — fan out the same input to several agents and aggregate (multiple reviewers). *Group chat* — agents converse in a shared thread, with a selection strategy choosing who speaks and a termination strategy deciding when to stop. *Handoff* — an agent transfers control to another based on the task (triage routes to a specialist). Each agent has its own instructions, tools, and possibly model. Design tips: clear single responsibilities, crisp termination conditions to avoid infinite chatter, observability per agent. These patterns map directly onto MAF's orchestration, unified with AutoGen's capabilities.

**Q45.** What is the convergence of Semantic Kernel into the Microsoft Agent Framework?

In 2025–2026 Microsoft unified Semantic Kernel (enterprise SDK, connectors, DI, filters, production hardening) and AutoGen (research-driven multi-agent patterns) into the **Microsoft Agent Framework (MAF)**. SK had robust enterprise plumbing but a younger agent story; AutoGen had advanced orchestration (group chat, magentic) but less enterprise readiness. MAF combines them — SK's connectors, filters, vector data, telemetry, Azure/Entra integration plus AutoGen's orchestration — into one supported framework for agents and agentic workflows. Much carries forward (kernel, plugins, function calling, filters, connectors) while agent abstractions consolidate. Microsoft's guidance: new agentic projects target MAF, with migration paths from both. SK remains relevant as the orchestration core. (Verify current GA status and exact API names.)

**Q46.** What carries over from SK knowledge to the Microsoft Agent Framework?

A lot transfers. *Function/tool calling* — `[KernelFunction]`-style tools and model-driven invocation remain central. *Connectors* — Azure OpenAI, embeddings, and `Microsoft.Extensions.VectorData` continue as the data/AI layer. *Filters/middleware* concepts (interception, HITL gates) persist as agent middleware. *DI and configuration* — the Microsoft.Extensions ecosystem, managed identity, options patterns stay. *Telemetry* — OpenTelemetry GenAI conventions carry forward. *Prompt assets* — `.prompty`/templates remain relevant. *Orchestration* — SK's sequential/concurrent/group-chat/handoff map onto MAF's, joined by AutoGen's magentic-manager. *Threads/state* — `AgentThread` evolves into MAF's thread/state with serialization and memory tiers. An engineer fluent in SK is well positioned; the main new learning is MAF's unified abstractions and workflow-vs-model-driven distinction. Frame SK experience as directly applicable foundational knowledge.

**Q47.** How do you secure plugins that perform sensitive operations?

Treat plugin functions like privileged APIs. *Least privilege*: register only the functions an agent needs. *Authorization*: in a function-invocation filter, check the caller's identity/claims before allowing execution. *Approval gates*: route destructive operations through an auto-function-invocation filter requiring human confirmation. *Scoped credentials*: plugins use the user's on-behalf-of token or a least-privilege managed identity, not a god-mode key, so downstream systems enforce their own RBAC. *Input validation*: validate model-supplied arguments (the model can hallucinate or be manipulated via injection). *Output handling*: scan tool results for injected instructions before feeding back (indirect injection). *Auditing*: log who/what/when. *Idempotency and limits*: rate-limit, make destructive actions idempotent. The model is an untrusted caller — apply defense-in-depth.

**Q48.** How do you evaluate the quality of an SK-based AI feature?

Because output is non-deterministic, you need *evaluation*, not just unit tests. Build a dataset of representative inputs with expected properties (not exact strings). Score outputs on metrics: *groundedness* (RAG answers derive from retrieved context), *relevance*, *coherence/fluency*, *correctness* against ground truth, *safety*, and task-specific checks (valid JSON, required fields). Use LLM-as-judge for subjective metrics and deterministic checks for structured ones. Azure AI Evaluation SDK / Prompt Flow / Foundry evaluations integrate with `.prompty` assets and run these systematically. Wire evaluations into CI to catch regressions when you change prompts, models, or retrieval; set quality gates. For agents, also evaluate tool-selection accuracy, step count, and task success rate. Mature teams treat prompts/agents as regression-tested assets, not "ship on vibes."

**Q49.** How do you deploy an SK application on Azure?

Host the SK app as an ASP.NET Core service on Azure Container Apps, AKS, or App Service, fronted by APIM for throttling/auth. Use a managed identity granted least-privilege roles: Cognitive Services OpenAI User on Azure OpenAI, scoped data roles on Azure AI Search, Key Vault Secrets User for residual secrets. *Networking*: private endpoints for Azure OpenAI and Search, VNet integration, no public keys. *Config*: app settings / Key Vault references for endpoints and deployment names. *Scale*: provision quota/PTUs for predictable throughput; autoscale on CPU/queue depth; Polly resilience for 429s. *Observability*: OpenTelemetry to App Insights. *State*: persist threads in Cosmos DB/Redis. *CI/CD*: GitHub Actions/Azure DevOps with Bicep/Terraform, running the eval suite as a quality gate. Managed-identity-first, private-networking, observable, quota-aware.

**Q50.** When would you choose Semantic Kernel over LangChain or a raw SDK?

Choose SK in a *Microsoft/.NET enterprise* context valuing first-class C# (and Python/Java), deep Azure integration (Azure OpenAI, AI Search, managed identity, Entra), and the Microsoft.Extensions ecosystem (DI, options, logging, OpenTelemetry). SK's filters/middleware give clean cross-cutting governance; its connector model and vector-data abstraction keep you provider-flexible; it's enterprise-hardened and the on-ramp to the Microsoft Agent Framework. LangChain has a larger Python ecosystem and more community integrations — attractive for Python-heavy data-science teams and rapid prototyping. A *raw SDK* is best for very simple, single-call scenarios where a framework is overkill. Decision: simple one-shot → raw SDK; Python experimentation with broad integrations → LangChain; .NET/Azure enterprise needing governance, DI, observability, and a path to managed agents → Semantic Kernel / Microsoft Agent Framework.

---

### .NET + AI

**Q1.** How do you set up Semantic Kernel in an ASP.NET Core app?

Add the `Microsoft.SemanticKernel` package, register the kernel and services in DI, and configure the Azure OpenAI connector. Use `Kernel.CreateBuilder()` and `AddAzureOpenAIChatCompletion`, then build and register (the kernel is thread-safe and cheap). Plugins are registered on the kernel. Inject `Kernel` into controllers/minimal-API handlers. Prefer managed identity (`DefaultAzureCredential`) over keys. Register plugins as scoped/transient if they need per-request services. You can register against the host's `builder.Services` directly and call `AddKernel()` so the kernel shares the app container.

**Q2.** How do you use `IChatCompletionService` directly?

Resolve it from the kernel (`kernel.GetRequiredService<IChatCompletionService>()`) or inject via DI. Build a `ChatHistory` with a system message and turns, then call `GetChatMessageContentAsync` for a full response or `GetStreamingChatMessageContentsAsync` for streaming. Pass `OpenAIPromptExecutionSettings`/`AzureOpenAIPromptExecutionSettings` for temperature, max tokens, and function-calling behavior (`FunctionChoiceBehavior`). Using the interface keeps your code provider-agnostic and testable via mocking. Note: older samples use `ToolCallBehavior.AutoInvokeKernelFunctions`; the current API is `FunctionChoiceBehavior.Auto()`.

**Q3.** How do you stream `IAsyncEnumerable` from Semantic Kernel to clients?

`GetStreamingChatMessageContentsAsync` returns `IAsyncEnumerable<StreamingChatMessageContent>`. In a minimal API or controller, iterate and write each delta to the response stream, flushing after each chunk so the client receives tokens immediately. Forward the request's `CancellationToken` (`HttpContext.RequestAborted`) so client disconnects cancel the upstream call. Set `Content-Type: text/event-stream`, write `data:` lines, and flush. Don't buffer.

**Q4.** How do you build a proper SSE endpoint in ASP.NET Core?

Set `Content-Type: text/event-stream`, disable response buffering, write `data:` lines terminated by a blank line, and flush after each event. Disable Nagle/output buffering at the server and any reverse proxy (`X-Accel-Buffering: no` for nginx; ensure Front Door/gateway doesn't buffer). Use `IAsyncEnumerable` and honor `RequestAborted`. Send a terminal `event: done` so the client knows it's complete; optionally include event ids for resumability. Avoid response compression on the SSE route since it buffers. For structured events (tokens, tool calls, citations), use named `event:` fields and JSON `data:` payloads. Keep the connection alive with periodic comment lines (`: ping`) if idle gaps are long.

**Q5.** How do you use the Azure OpenAI SDK (`Azure.AI.OpenAI`) directly?

Create an `AzureOpenAIClient` with the endpoint and a credential, get a `ChatClient` for your deployment, and call `CompleteChatStreamingAsync`. Prefer `DefaultAzureCredential` (managed identity) over `AzureKeyCredential`. Use the SDK directly for fine control over OpenAI features (logprobs, structured outputs, vision) without SK's abstraction. Register the client as a singleton — it's thread-safe and pools connections.

**Q6.** How does `DefaultAzureCredential` and managed identity work for AI services?

`DefaultAzureCredential` tries a chain of credential sources: environment variables, managed identity, Visual Studio/Azure CLI. In Azure (Container Apps, App Service, AKS) it picks up the workload's managed identity automatically; locally it uses your developer login. Assign the identity the `Cognitive Services OpenAI User` role on the Azure OpenAI resource. This eliminates keys — no secrets in config, automatic credential rotation, per-identity RBAC/audit. Pass the credential to `AzureOpenAIClient` or SK's connector. For user-delegated scenarios, combine with OBO. Benefits: no key sprawl, no rotation toil, least-privilege via RBAC.

**Q7.** How do you add resilience and 429 handling with Polly?

Use `Microsoft.Extensions.Http.Resilience` (Polly v8) or a `ResiliencePipeline`. Implement retry with exponential backoff and jitter for 429/503/timeouts, honoring the `Retry-After` header Azure OpenAI returns. Add a circuit breaker to fail fast when the service is unhealthy, and a timeout strategy. The Azure SDKs also have built-in retry; tune `RetryPolicy` rather than double-retrying. Respect `Retry-After` to avoid worsening throttling. For sustained throughput, use PTUs; for scale, load-balance across deployments via an AI gateway.

**Q8.** How do you register and configure options/config for AI in ASP.NET Core?

Bind a strongly-typed options class with the options pattern, validate on startup (`ValidateDataAnnotations().ValidateOnStart()`), and source secrets from Key Vault or managed identity rather than appsettings. Use `IOptionsSnapshot` for per-request config and `IOptionsMonitor` for live reload. Keep endpoint, deployment names, and tuning params in config; keep credentials out (managed identity). For multi-tenant, key options by tenant or use a factory. Use `dotnet user-secrets` for local dev. Use Azure App Configuration for centralized, feature-flagged settings with Key Vault references. Validate required fields at startup so misconfiguration fails fast.

**Q9.** How do you implement background document ingestion with a hosted service?

Use `IHostedService`/`BackgroundService` for in-process work, or better, decouple via a queue. A `BackgroundService` reads from Azure Service Bus/Storage Queue, processes documents (extract, chunk, embed, upsert to the vector store), and acknowledges on success. Use `IServiceScopeFactory` to create a scope per message since the service is a singleton. For elastic scale-out, prefer Azure Functions with a Service Bus trigger over a long-running hosted service.

**Q10.** When do you choose Azure Functions vs hosted services vs Service Bus for ingestion?

Hosted services (`BackgroundService`) run in-process with your API — good for lightweight, always-on work but they scale with your web app and compete for its resources. Azure Functions with Service Bus/Blob triggers give independent, event-driven, auto-scaling compute, ideal for spiky ingestion and offloading scaling concerns. Service Bus is the durable queue decoupling producers (uploads) from consumers (embedding workers), giving back-pressure, retries, dead-lettering, and ordering (sessions). Typical pipeline: upload writes a blob and enqueues a message; a Function processes it. Choose Functions for bursty/serverless economics; Container Apps jobs/KEDA for heavier, long-running embedding work that exceeds Function limits.

**Q11.** How do you store and query vectors with EF Core / Azure SQL?

Azure SQL supports a native `VECTOR` type with `VECTOR_DISTANCE` functions. With EF Core you map a vector column and run similarity queries via raw SQL or provider support. Store chunk text, metadata, and the embedding vector together. For pure vector workloads a dedicated store (Azure AI Search, Cosmos DB, pgvector) is usually better, but co-locating vectors in Azure SQL is attractive when you already have relational data and want transactional consistency and joins between business data and embeddings. Trade-off: SQL keeps everything in one system; specialized vector DBs scale and rank better at large volumes.

**Q12.** How do you cache AI responses and embeddings?

Cache embeddings aggressively — they're deterministic for a given input/model, so hash the text and store the vector (Redis or a table) to avoid re-embedding identical content. For completions, use semantic caching: embed the query, look up near-duplicate prior queries within a similarity threshold, and return the cached answer if close enough (Azure APIM has a semantic caching policy for Azure OpenAI). Cache RAG retrieval per query. Use `IMemoryCache` for hot in-process data and `IDistributedCache`/Redis for shared cache. Set TTLs and invalidate on document updates. Watch semantic cache thresholds — too loose returns wrong answers. Don't cache personalized/permission-trimmed results across users.

**Q13.** How do you add OpenTelemetry to an AI .NET service?

Add `OpenTelemetry.Extensions.Hosting` with tracing, metrics, and logging exporters to Azure Monitor (`Azure.Monitor.OpenTelemetry.AspNetCore`). SK and the Azure SDKs emit traces/metrics; enable them and add custom spans around RAG retrieval, embedding, and model calls with attributes for model, token counts, and latency. Capture TTFT and total latency as metrics. Use the GenAI semantic conventions for spans (`gen_ai.system`, `gen_ai.request.model`, token usage). Correlate the client request id through to the model call. This gives end-to-end traces (HTTP → retrieval → embedding → completion) and dashboards for cost, latency percentiles, and error rates. Don't log full prompts/responses with PII; redact or sample.

**Q14.** How do you test code that calls an LLM?

Abstract model access behind an interface (`IChatCompletionService` is already one) and mock it in unit tests so you assert orchestration logic (prompt assembly, retrieval, parsing) without hitting the network or paying for tokens. Mock returns fixed `ChatMessageContent`. Test resilience (simulate 429), cancellation, and structured-output parsing. For integration tests, use a recorded/playback approach or a cheap model, gated behind a flag. Don't assert exact model wording — assert on structure, citations, or schema validity, or use LLM-as-judge evals in a separate eval pipeline (Azure AI Foundry). Separate "does my code work" (mocked unit tests) from "is the output good" (offline evals).

**Q15.** How do you build a minimal API for chat?

Map endpoints with `MapPost`, inject the kernel/services, validate input, and return streaming or JSON. Group routes, add auth with `RequireAuthorization`, and apply rate limiting middleware. Use `Results.Stream(..., "text/event-stream")` for SSE. Minimal APIs reduce ceremony, are fast, and integrate cleanly with DI, validation, OpenAPI, and the rate-limiting middleware. Keep handlers thin — delegate to services for testability.

**Q16.** How do you use the built-in rate limiter middleware per user/tenant?

Use ASP.NET Core's rate limiting middleware with a partitioned policy keyed on the authenticated user or tenant claim, applying token-bucket or sliding-window limits. Return 429 with `Retry-After`. For distributed enforcement across instances, back it with Redis or front it with Azure API Management quotas. This protects costly Azure OpenAI capacity from abuse and enforces per-tenant fairness.

**Q17.** How do you implement function calling / tool use in Semantic Kernel?

Define plugins as classes with `[KernelFunction]`-attributed methods and `[Description]` so the model understands them. Register them on the kernel and set `FunctionChoiceBehavior.Auto()` (older samples: `ToolCallBehavior.AutoInvokeKernelFunctions`) so SK automatically invokes matching functions and feeds results back. SK handles the loop: model requests a tool, SK invokes it, appends the result, and continues until a final answer. For safety, gate high-risk functions behind confirmation, validate arguments, and set a max iteration count to prevent infinite loops. This is the foundation for agentic behavior.

**Q18.** How do you implement RAG in .NET with SK and Azure AI Search?

Embed the query, retrieve top-k chunks from the vector index, build a grounded prompt with retrieved context and instructions to cite sources, then call the chat service. Use SK's vector connectors or the Azure AI Search SDK directly. Apply security trimming via filters (user's group claims) so users only see permitted documents. Use hybrid search (vector + keyword + semantic reranker) for best relevance. Pass retrieved snippets plus citations to the model. For agentic RAG, expose retrieval as a `[KernelFunction]` the model calls on demand.

**Q19.** How do you generate embeddings in .NET?

Use the Azure OpenAI embeddings deployment via the SDK or SK's `ITextEmbeddingGenerationService` / the newer `IEmbeddingGenerator<string, Embedding<float>>` from `Microsoft.Extensions.AI`. Batch inputs to reduce round-trips and cost, and cache by content hash. Choose the embedding model deliberately (dimensions, cost, multilingual). Normalize and store with metadata. For ingestion at scale, batch and parallelize with bounded concurrency to respect rate limits, and use Polly for 429s. Keep the embedding model consistent between ingestion and query — mismatched models produce meaningless similarity. `Microsoft.Extensions.AI` gives a provider-agnostic abstraction so you can swap implementations.

**Q20.** What is `Microsoft.Extensions.AI` and why use it?

`Microsoft.Extensions.AI` (MEAI) provides unified abstractions — `IChatClient` and `IEmbeddingGenerator` — over different providers (Azure OpenAI, OpenAI, Ollama, etc.), plus middleware for caching, telemetry, function invocation, and logging via a decorator pipeline. It's the lower-level building block SK and the Agent Framework build on. Use it when you want a thin, swappable abstraction without SK's full orchestration, or to compose cross-cutting concerns (OpenTelemetry, distributed caching, retry) declaratively via `ChatClientBuilder`. It standardizes the .NET AI ecosystem so libraries interoperate.

**Q21.** How do you handle long-running agent jobs in ASP.NET Core?

Don't run multi-minute orchestrations inside an HTTP request — return quickly with a job id and process asynchronously. Enqueue the job (Service Bus), process in a worker (Container Apps job, Function, or hosted service), persist intermediate state, and push status/results via SignalR or a status endpoint. This survives client disconnects, scales independently, and supports retries/idempotency. Store the agent's plan, step results, and output durably (Cosmos DB/Azure Storage). Use Durable Functions/Durable Task for complex multi-step orchestrations with fan-out/fan-in and checkpointing. Make steps idempotent so retries don't duplicate side effects. Forward cancellation so users can stop jobs.

**Q22.** How do you forward cancellation to Azure OpenAI?

Plumb `CancellationToken` from `HttpContext.RequestAborted` (or the connection token) through every async call down to the model SDK. When the client disconnects or hits Stop, the token cancels, the SDK call throws `OperationCanceledException`, and the upstream HTTP request to Azure OpenAI is aborted — so you stop generating (and paying for) tokens. Always accept and pass `ct`; never swallow it; never pass `CancellationToken.None` on model calls. Catch `OperationCanceledException` at the boundary and treat it as a normal stop, not an error to log loudly. This is the single biggest cost-control lever for streaming chat.

**Q23.** How do you implement structured outputs in .NET?

Use JSON mode / structured outputs with a JSON schema so the model returns parseable, schema-conformant JSON. With the Azure OpenAI SDK, set `ResponseFormat` to a JSON schema (`CreateJsonSchemaFormat(..., jsonSchemaIsStrict: true)`); with SK, set `ResponseFormat` to a .NET type. Deserialize into typed records and validate against domain rules. Strict schema mode guarantees valid JSON, eliminating brittle regex parsing. Essential for reliable extraction, classification, and tool-calling pipelines where downstream code consumes the output.

**Q24.** How do you optimize performance of a .NET AI service?

Stream to reduce TTFT; reuse singleton clients (connection pooling); use `IAsyncEnumerable` end-to-end without buffering. Cache embeddings and use semantic caching. Batch embedding generation. Use `System.Text.Json` source generators. Avoid synchronous-over-async. Bound concurrency to Azure OpenAI with rate limiting and semaphores to prevent self-inflicted 429s. Use PTUs for predictable latency at scale, or pay-as-you-go with capacity planning. Co-locate app and Azure OpenAI region to cut network latency. Profile with `dotnet-trace`/App Insights. Offload heavy work (ingestion) to background; use channels for producer/consumer pipelines. The dominant latency is the model itself — optimize TTFT and parallelism, not CPU micro-optimizations.

**Q25.** How do you host an AI .NET app on Azure Container Apps vs AKS?

Azure Container Apps (ACA) is serverless containers with built-in KEDA autoscaling (scale to zero, scale on HTTP/queue), Dapr, revisions, and managed ingress — ideal for most AI APIs and workers without managing Kubernetes. It supports background jobs and event-driven scaling on Service Bus depth, fitting ingestion. Choose AKS when you need full Kubernetes control: custom networking, GPU node pools for self-hosted models, service meshes, specific operators. ACA is simpler ops and faster to ship; AKS is more flexible but higher operational burden. Both support managed identity and VNet integration. For a typical Azure-OpenAI-backed app (no GPU hosting), ACA is usually the right default.

**Q26.** What is the Microsoft Agent Framework and how do you use it in .NET?

The unification of Semantic Kernel and AutoGen into a single SDK for building agents and multi-agent workflows in .NET (and Python). It provides `AIAgent` abstractions, threads/conversations, tool integration, and orchestration patterns (sequential, concurrent, group chat, handoff) with built-in state management, observability, and human-in-the-loop. You create an agent from a chat client plus instructions and tools (`chatClient.CreateAIAgent(...)`), run it with a thread (`agent.GetNewThread()` / `RunAsync`), and compose agents into workflows. It's the strategic forward path Microsoft recommends for new agentic apps, with first-class Azure AI Foundry integration. (Deep coverage in section 11.)

**Q27.** How do you implement multi-agent orchestration patterns?

The Agent Framework supports several: sequential (pipeline of agents each refining output), concurrent (fan-out to specialists, aggregate), group chat (agents converse to solve a problem with a manager deciding turns), handoff (an agent delegates to a specialist). Choose by task: sequential for staged workflows (researcher → writer → reviewer), concurrent for parallel sub-tasks, group chat for collaborative reasoning, handoff for routing/triage. Each agent has its own instructions, tools, and possibly model. Manage shared state and termination conditions (max turns) to prevent runaway loops and cost. Add observability per agent step. For determinism and auditability, prefer explicit orchestration (sequential/handoff) over free-form group chat where possible.

**Q28.** How do you secure secrets and config in a .NET AI app?

Eliminate secrets where possible via managed identity (no Azure OpenAI keys). For unavoidable secrets, store in Azure Key Vault and reference via App Configuration or Key Vault references — never in appsettings or source. Use `DefaultAzureCredential` so the same code works locally and in Azure. Grant least-privilege RBAC roles. Rotate any remaining keys automatically. Use `dotnet user-secrets` for local dev only. Scan for committed secrets in CI. For per-tenant credentials, fetch from Key Vault at runtime scoped by tenant. Encrypt sensitive data at rest (CMK) and in transit (TLS). The end state: zero static credentials in the AI call path, all access via Entra ID identities with audit trails.

**Q29.** How do you handle content safety and moderation in .NET?

Integrate Azure AI Content Safety to screen both user inputs and model outputs before display, classifying for hate, violence, self-harm, sexual content, and detecting jailbreak/prompt-injection attempts and protected material. Azure OpenAI also has built-in content filters; tune severity thresholds per use case. Call Content Safety as a pre/post step, blocking or flagging content above thresholds and returning a safe message. Log moderation decisions for audit. For RAG, scan retrieved content too, since documents may contain unsafe or injected material. Combine automated filtering with human review for borderline cases in sensitive domains. A governance requirement for most enterprise deployments.

**Q30.** How do you implement chat history persistence and context management in .NET?

Persist `ChatHistory` per conversation in Cosmos DB or Azure SQL, keyed by conversation and user id, with messages, roles, timestamps, model, and token usage. On each turn, load history but manage context window growth: truncate or summarize older turns when approaching token limits (sliding window + running summary). SK provides `ChatHistoryReducer` implementations (truncation, summarization). Store the full transcript durably while sending only the reduced window to the model. For RAG, retrieve relevant context rather than stuffing entire history. Track tokens to stay within limits and budget. Make persistence idempotent and write the final assistant message after streaming completes. Encrypt at rest and apply retention/governance policies.

**Q31.** How do you implement health checks and readiness for an AI service?

Add ASP.NET Core health checks: a liveness probe (process up) and a readiness probe that verifies dependencies (can reach Azure OpenAI, vector store, Service Bus) without expensive model calls — use a lightweight connectivity check. Expose `/health/live` and `/health/ready` and wire them to Container Apps/AKS probes so unhealthy instances are restarted or removed from rotation. Don't run a full model completion on every health check (cost/latency); check auth/connectivity. Include circuit-breaker state so a tripped breaker reports degraded. Surface dependency health in the readiness response for diagnostics. This enables safe rolling deploys and automatic recovery.

**Q32.** How do you do prompt management/versioning in .NET?

Externalize prompts from code into a versioned prompt store — SK's prompt templates (YAML/`.prompty` files) with Handlebars/Liquid templating, or a registry in config/App Configuration. Version prompts so you can A/B test and roll back without redeploying code. Track which prompt version produced each response in telemetry for eval. Use `.prompty` files which package the prompt, model config, and sample inputs. Validate template variables. Keep system prompts and guardrails server-side and access-controlled. For governance, review prompt changes like code changes. This decouples prompt iteration from code deploys and enables systematic evaluation of prompt variants against eval datasets.

**Q33.** How do you implement semantic caching in .NET?

Embed the incoming query, search a cache index (Redis with vector search, or Azure AI Search) for prior queries within a cosine-similarity threshold, and if a close match exists, return its cached answer instead of calling the model. On miss, call the model and store `{embedding, query, answer, model, ttl}`. Tune the threshold carefully — too loose returns subtly wrong answers. Scope the cache by tenant and by any context that changes the answer (don't share permission-trimmed RAG results across users). Azure APIM offers a built-in semantic caching policy for Azure OpenAI at the gateway. Invalidate when underlying data changes. Measure hit rate vs answer quality. Cuts cost and latency for repetitive queries.

**Q34.** How do you handle multi-tenancy in a .NET AI service?

Isolate by tenant at every layer: scope auth (tenant claim from Entra ID), partition data (separate vector indexes/containers or a tenant filter with row-level security), enforce per-tenant rate limits and quotas, and route to tenant-specific model deployments or configs. Use the tenant id to select options (`IOptionsSnapshot` keyed by tenant or a resolver). Apply security trimming in RAG so retrieval never crosses tenant boundaries. Track usage/cost per tenant for billing. For strong isolation use separate resources per tenant; for density use shared resources with strict logical partitioning. Validate the tenant claim on every request and never derive tenant from the request body. Encrypt with per-tenant keys (CMK) if contractually required.

**Q35.** How do you implement retries that respect Azure OpenAI quota?

Retry only on retryable errors (429, 500, 503, timeouts) with exponential backoff plus jitter, and honor the `Retry-After` header so you wait the prescribed time rather than guessing. Cap retry attempts to avoid amplifying load during outages. Combine with a circuit breaker. Don't retry non-idempotent or 400-class errors. Bound concurrency client-side so you don't generate self-inflicted 429s. For sustained throughput, use PTUs (reserved capacity, predictable limits). Track retry/throttle rates as a metric — high rates signal capacity shortfalls needing PTU or multi-region/multi-deployment spreading via an AI gateway.

**Q36.** How do you load-balance across multiple Azure OpenAI deployments?

Spread traffic across multiple deployments/regions to raise effective TPM/RPM limits and improve resilience. Use Azure APIM as an AI gateway with a load-balancing policy (round-robin or weighted) across backend pools, with circuit breaking that removes a backend on 429/5xx and retries another. Track per-backend health and remaining quota. This "GenAI gateway" pattern pools capacity across PAYG and PTU deployments, fails over regions, and centralizes auth, logging, and token quotas. Alternatively implement client-side balancing with Polly and a backend selector. The gateway approach centralizes governance and is the Microsoft-recommended pattern for enterprise scale.

**Q37.** How do you implement an AI gateway with Azure API Management?

Front all Azure OpenAI traffic with APIM, applying policies: managed-identity auth to the backends, token-based rate limiting (`azure-openai-token-limit`, which counts tokens not just calls), token usage metrics (`azure-openai-emit-token-metric`), semantic caching, load balancing across deployments with circuit breaking, and content/header transforms. APIM centralizes security (subscription keys/Entra for clients), per-team quotas, observability, and cost attribution. It decouples clients from specific deployments so you can swap models or regions without client changes. Combine with App Configuration for routing rules. A single control plane for governance, cost, and reliability across many apps and tenants.

**Q38.** How do you log token usage and cost in .NET?

Capture token counts from the model response (`Usage` on completions, streaming aggregates) and emit them as OpenTelemetry metrics tagged with model, deployment, tenant, user, and feature. Multiply by per-model pricing to estimate cost. For streaming, set `StreamOptions { IncludeUsage = true }` so usage arrives in the final chunk. Aggregate in App Insights/Log Analytics for dashboards and budgets, with alerts on anomalies. At the gateway, APIM's token metric policies do this centrally. Use this for chargeback/showback per tenant, capacity planning, and detecting runaway prompts. Don't log full prompt/response text with the metrics — keep counts and metadata. Accurate token accounting is essential for cost governance.

**Q39.** How do you implement evaluation pipelines in .NET / Azure?

Build offline eval datasets (input + expected criteria) and run candidate prompts/models through them, scoring with metrics: groundedness, relevance, coherence, fluency, and task-specific checks, using LLM-as-judge plus deterministic assertions. Azure AI Foundry provides built-in evaluators and an SDK to run evals at scale and track results across versions. Integrate evals into CI so prompt/model changes are gated on quality regression checks, like tests. Capture production feedback (thumbs up/down) and sampled traces to grow eval sets. Run safety evals (jailbreak, harmful content). Separate this from unit tests — evals measure output quality (statistical), unit tests measure code correctness. This is how you ship AI changes confidently.

**Q40.** How do you handle large file/document ingestion and chunking in .NET?

Stream large files from Blob Storage rather than loading into memory. Extract text with Azure AI Document Intelligence (PDFs, tables, layout, OCR) for complex documents. Chunk by semantic/structural boundaries (headings, paragraphs) with overlap, sized to the embedding model's optimal range — not blind fixed-size splits — to preserve context. Attach metadata (source, page, section, permissions) for citations and security trimming. Embed in batches with bounded concurrency and 429 handling, then upsert to the vector store idempotently (deterministic chunk ids so re-ingestion updates rather than duplicates). Run in background workers/Functions triggered by upload events. Handle failures with dead-lettering and retries. Good chunking and metadata are the biggest levers on RAG quality.

**Q41.** How do you implement idempotency in AI background processing?

Use deterministic ids derived from content (hash of document + chunk index) so reprocessing the same input upserts rather than duplicates vectors. Track processed message ids (dedupe store) to handle Service Bus at-least-once delivery and redeliveries. Make side effects idempotent or guard them with a processed-marker check. Use Service Bus sessions/dedup windows where appropriate. For multi-step pipelines, checkpoint progress so a retry resumes rather than restarts. Wrap external writes in idempotent upserts. This matters because queues deliver at-least-once and workers can crash mid-processing; without idempotency you get duplicate embeddings, double-counted costs, or inconsistent state. Design every step to be safely re-runnable.

**Q42.** How do you stream tool-call and citation events alongside tokens?

Define a structured event protocol over your SSE/SignalR channel: emit typed events (`token`, `tool_call_start`, `tool_call_result`, `citation`, `done`) as JSON, not just raw text. In SK's function-calling loop, hook `IFunctionInvocationFilter` to emit tool-call events as they happen, and emit citation events when RAG retrieval completes. The client renders each event type appropriately. This interleaves model token streaming with orchestration events on the same channel, each tagged with a type and ordering. It gives the frontend everything needed for transparent agent UIs (showing "calling search…", then citations, then the streamed answer).

**Q43.** How do you implement function invocation filters / middleware in SK?

SK supports filters that intercept the pipeline: `IFunctionInvocationFilter` (around each plugin function call), `IPromptRenderFilter` (around prompt rendering), and `IAutoFunctionInvocationFilter` (around the auto tool-calling loop). Register them in DI. Use them for cross-cutting concerns: logging/telemetry per function, authorization checks before invoking a tool, argument validation, caching results, redacting sensitive data in prompts, and enforcing max iterations / human-in-the-loop approval for risky functions. A filter can short-circuit by setting `ctx.Result` and skipping `next`. Filters are the clean extensibility seam for governance.

**Q44.** How do you choose between vector stores for a .NET RAG app?

Azure AI Search is the default Microsoft choice: managed, hybrid search (vector + BM25 + semantic reranker), security filters, faceting, integrated vectorization, with a first-class .NET SDK and SK connector. Cosmos DB for NoSQL/PostgreSQL adds vector search where you already store operational data (transactional consistency). Azure SQL's native VECTOR type suits relational + vector co-location. pgvector (Azure Database for PostgreSQL) suits OSS-friendly teams. Pinecone/Qdrant for specialized scale. Decide on: hybrid search quality (AI Search wins), data co-location, scale, cost, ops. For most enterprise Microsoft RAG, Azure AI Search's hybrid + semantic reranker gives the best relevance with least effort — the usual recommendation.

**Q45.** How do you implement graceful model fallback in .NET?

Wrap model calls so that on failure (sustained 429, 5xx, timeout, circuit open) you fall back to an alternate deployment/region or a secondary model (smaller/cheaper or different-region). Implement via Polly fallback policy or an AI gateway backend pool with circuit breaking. Preserve the request and degrade gracefully — return results from the backup with a flag the UI can surface. Order fallbacks by quality/cost. Ensure the fallback model's prompt/format is compatible (or maintain per-model prompt variants). Track fallback frequency as a reliability metric. Don't fall back on client errors (400, content filter) — those won't succeed elsewhere. Keeps the service available during regional outages or capacity exhaustion.

**Q46.** How do you implement request/response logging without leaking PII?

Log structured metadata (request id, user/tenant, model, token counts, latency, status, prompt version) by default, and treat prompt/response text as sensitive: redact PII (a detection step or Azure AI Language), hash or omit content, or log only sampled, consented traces to a secured store with restricted access and short retention. Never put prompt text in standard application logs. For debugging, gate full-content capture behind a flag and a secure sink with access controls and DLP. Comply with data residency and customer agreements (some tenants forbid storing prompts). Use App Insights telemetry processors that scrub sensitive fields. Balance observability against privacy — make content logging explicit and governed.

**Q47.** How do you handle streaming errors mid-response in .NET?

Errors can occur after streaming has started (network blip, content filter triggered mid-generation, upstream 5xx). You've already sent a 200 and partial data, so you can't change the status code. Emit a structured error event (`event: error`) so the client can react, then close. Persist the partial response as `interrupted`. Wrap the `await foreach` in try/catch, distinguish cancellation (normal stop) from real errors, and flush a terminal event in `finally`. Don't leave the connection hanging. On the client, render what arrived and offer regenerate. Log the error with correlation id. Designing the protocol with explicit done/error events makes mid-stream failures recoverable rather than silent truncations.

**Q48.** How do you implement dependency injection for AI services cleanly?

Register the kernel/clients as singletons (thread-safe, pooled). Register plugins and per-request services (RAG retriever, tenant resolver) with appropriate lifetimes — scoped if they depend on request context. Use keyed services (`AddKeyedSingleton`) to register multiple model deployments and resolve by key ("fast" vs "smart"). Use factory delegates for credential/config wiring. Abstract behind interfaces (`IChatService`, `IRagService`) so handlers depend on abstractions and tests can mock them. For multi-tenant, use a tenant-aware factory rather than capturing tenant in a singleton. Compose MEAI middleware in the registration. Avoid resolving scoped services from singletons directly — use `IServiceScopeFactory` in background workers. Clean DI keeps the AI plumbing testable and swappable.

**Q49.** How do you support both keyed model selection and cost-aware routing in .NET?

Register deployments as keyed services and implement a router that picks a model based on policy: request complexity, user tier, explicit "fast/smart" choice, current cost budget, or backend health/quota. The router resolves the right `IChatClient` by key and may downgrade to a cheaper model under budget pressure or upgrade for complex tasks (e.g., classify difficulty first, then route). Centralize routing logic, make it configurable (App Configuration), log which model served each request, and enforce per-tenant model allowlists. This balances cost and quality systematically rather than hard-coding model choices throughout the codebase.

**Q50.** What are common production pitfalls in .NET AI services and fixes?

(1) Not forwarding `CancellationToken` — wastes tokens on disconnects; plumb `RequestAborted` everywhere. (2) Buffering the whole stream before sending — kills TTFT; stream and flush. (3) Self-inflicted 429s from unbounded concurrency — add rate limiting/semaphores and respect `Retry-After`. (4) Storing keys instead of managed identity. (5) Re-embedding identical content — cache by hash. (6) Stuffing entire chat history into context — reduce/summarize. (7) Mismatched embedding models between ingest and query. (8) Logging PII in prompts. (9) Non-idempotent ingestion duplicating vectors. (10) Treating output quality as unit tests instead of evals. (11) Resolving scoped services in singletons. (12) No fallback/circuit breaker, so one region outage takes you down. Each has a direct, well-known remedy.
