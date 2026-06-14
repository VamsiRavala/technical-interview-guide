# Interview Questions

100+ detailed Q&A on AI agents, grouped by topic: **Microsoft Agent Framework** (50), **AutoGen** (50), and **General Agents** (the framework-agnostic fundamentals). Currency: mid-2026 — MAF's surface moves, so conceptual answers carry verification caveats where exact API names shift.

---

## Microsoft Agent Framework

**Q1.** What is the Microsoft Agent Framework and why was it created?

The Microsoft Agent Framework (MAF) is Microsoft's unified, open-source SDK for building AI agents and agentic workflows, combining Semantic Kernel's enterprise foundation with AutoGen's multi-agent orchestration research. It was created to end the confusion of two overlapping projects: SK offered production-grade connectors, DI, filters, vector data, telemetry, and Azure/Entra integration but a younger agent story; AutoGen offered cutting-edge multi-agent patterns (group chat, magentic) but less enterprise readiness. MAF merges their strengths into one supported framework spanning .NET and Python. It provides agent abstractions (instructions, tools, threads, memory), tool/function calling, MCP integration, orchestration patterns (sequential, concurrent, group chat, handoff, magentic), deterministic workflows alongside model-driven planning, and built-in security/observability. Microsoft positions MAF as the recommended path for new agentic projects on Azure, with migration routes from both SK and AutoGen. (Verify current GA status and exact API names, as the surface is fast-moving as of mid-2026.)

**Q2.** Describe the agent lifecycle in MAF.

An agent's lifecycle: *definition* — configure with a name, instructions (system prompt/persona), an underlying model/client, tools (functions, MCP servers), and optional memory. *Instantiation* — created against a chat client or the Azure AI/Foundry Agent Service. *Invocation* — run with input on a thread; it reasons, optionally calls tools in a loop (observe-act), and produces responses, supporting streaming and intermediate steps. *State management* — a thread holds conversation state across turns; you persist and resume it. *Orchestration* — the agent may participate in multi-agent workflows, handing off or collaborating. *Termination* — a run ends on a final answer, a termination condition, an approval halt, or an error. *Disposal/cleanup* — release resources; for service-managed agents, delete server-side threads/agents when done. Throughout, observability (OpenTelemetry) traces each phase, and middleware/filters intercept tool calls. Designing for resumability (serialize thread state) and bounded runs (iteration/cost limits) is key to production agents.

**Q3.** How does MAF handle threads and conversation state?

A *thread* represents an agent conversation's state — its message history and any provider-managed context. Each run advances a thread; reusing a thread preserves multi-turn context, while a new thread starts fresh. MAF supports both client-managed threads (history held in your process) and service-managed threads (the Azure AI/Foundry Agent Service stores messages server-side and you hold an id). Because web apps are stateless per request, you serialize the thread and persist it (Cosmos DB, Redis, blob) keyed by user/session, then rehydrate on the next turn.

```python
thread = agent.get_new_thread()
await agent.run("Plan a trip to Tokyo", thread=thread)
# later, after persisting/restoring
await agent.run("Make it cheaper", thread=thread)
```

State serialization is first-class so you can pause, store, and resume long-running or human-in-the-loop conversations. You apply history reduction (truncation/summarization) to manage the context window. In multi-agent orchestration, a shared thread coordinates the conversation. (Verify exact serialization API names.)

**Q4.** How do agents communicate with each other in MAF?

Agents communicate through *orchestration* over a shared conversation/thread or via explicit message passing, depending on the pattern. In *group chat*, agents post messages to a common thread and a manager/selection strategy decides who speaks next, so they "see" each other's contributions. In *sequential* orchestration, one agent's output becomes the next agent's input — a pipeline. In *handoff*, an agent explicitly transfers control (and context) to another agent best suited to the task, often via a handoff tool the model calls. In *concurrent* orchestration, the same input fans out to multiple agents and results aggregate. Under the hood, communication is mediated by the framework — messages carry roles and content, and the orchestrator routes them. This message-passing model derives largely from AutoGen's conversational multi-agent design. Key considerations: clear agent responsibilities so routing is unambiguous, structured messages, termination conditions, and observability to trace cross-agent conversations.

**Q5.** How does tool/function calling work in MAF?

Like SK, MAF exposes callable functions to the model as tools. You define functions (in .NET, methods with description metadata; in Python, decorated functions or callables with docstrings/type hints), register them with the agent, and the framework serializes them into the model's tool schema. The model decides when to call them; the framework executes and feeds results back, looping until a final answer.

```python
def get_weather(city: str) -> str:
    """Get current weather for a city."""
    return weather_api.lookup(city)

agent = ChatAgent(chat_client=client, instructions="...", tools=[get_weather])
```

You control invocation via choice behavior (auto/required/none equivalents) and can disable auto-execution to insert approval. Tools can be local functions, OpenAPI-imported operations, MCP server tools, or hosted tools (code interpreter, file/web search) in the Azure Agent Service. Middleware intercepts each call for logging, validation, and HITL gates. Good tool design — precise names and descriptions, narrow scope, validated inputs — drives accuracy. This carries the SK `[KernelFunction]` model forward into MAF.

**Q6.** What is MCP and how does MAF integrate it?

The Model Context Protocol (MCP) is an open standard for connecting AI applications to external tools, data, and prompts via a uniform client-server protocol. An MCP *server* exposes tools/resources; an MCP *client* (the agent host) discovers and invokes them. MAF integrates MCP so an agent can consume any MCP server's tools as if they were native functions — you point the agent at an MCP server and its tools become available to the model.

```csharp
// conceptual
var mcpTools = await mcpClient.ListToolsAsync();
agent = new ChatAgent(client, instructions, tools: mcpTools);
```

This is powerful: instead of bespoke plugins per integration, you reuse the growing ecosystem of MCP servers (GitHub, databases, filesystems, SaaS) and your own, with consistent discovery and auth. MAF can also *expose* agents/tools as MCP servers. Security matters — vet MCP servers, scope tool permissions (least privilege), and guard against tool-output prompt injection. (Verify exact MCP client API names.)

**Q7.** What are the memory tiers in MAF?

MAF formalizes memory beyond a single chat history into tiers. *Conversation/short-term memory* — the current thread's messages, bounded by the context window and managed via truncation/summarization. *User/working memory* — facts about the user or task that persist across turns within a session (preferences, gathered entities), maintained by a memory component that extracts and reinjects salient facts. *Long-term memory* — durable knowledge persisted in a vector store (via `Microsoft.Extensions.VectorData`) or other store, retrieved on demand via RAG/tools across sessions. The distinction matters for cost and relevance: you don't keep everything in the prompt; you store long-term knowledge externally and retrieve the right slice. A memory provider can automatically capture important information and surface it later. (Verify exact memory component/provider API names.)

**Q8.** How does MAF support human-in-the-loop and approval workflows?

MAF treats HITL as a first-class concern for high-stakes actions. The common pattern: when the model wants to call a sensitive tool (refund, delete, send), the framework pauses and surfaces the pending action — its name and arguments — for human approval instead of auto-executing. You implement this by disabling auto-invocation for those tools, or via middleware that intercepts the tool call, requests approval, and either resumes or rejects.

```python
response = await agent.run(user_input, thread=thread)
if response.requires_approval:
    if human_approves(response.pending_call):
        await agent.run(approve(response.pending_call), thread=thread)
```

Because thread state serializes, you can pause indefinitely (await a human in a UI/ticket) and resume later — true asynchronous HITL. Approval can be policy-driven (only destructive or above-threshold actions need a human). This generalizes SK's auto-function-invocation filter/`Terminate` pattern into explicit approval workflows.

**Q9.** Explain sequential orchestration in MAF.

Sequential orchestration chains agents in a fixed pipeline where each processes the previous one's output — function composition of specialists. Example: a *research* agent gathers facts, passes them to a *writer* that drafts content, which passes to an *editor* that polishes. It's deterministic in ordering (you define the sequence) while each step is model-driven internally.

```python
workflow = SequentialBuilder().participants([researcher, writer, editor]).build()
result = await workflow.run("Write an article on Azure AI agents")
```

Use it when the task decomposes into ordered stages with clear handoffs and you want predictable flow and easy debugging. Advantages: simple to reason about, focused per-agent responsibility, inspectable intermediate outputs. Limitations: rigid — no branching/iteration unless you add it. Compare with group chat (dynamic turn-taking) and handoff (conditional routing).

**Q10.** Explain concurrent orchestration in MAF.

Concurrent orchestration fans the same input out to multiple agents that run in parallel, then aggregates their outputs. Use it for tasks benefiting from diverse perspectives or independent subtasks: multiple experts review a document simultaneously, several agents draft alternatives, or independent data sources are queried at once. Because the agents don't depend on each other, parallelism cuts latency.

```python
workflow = ConcurrentBuilder().participants([legal, security, finance]).build()
results = await workflow.run("Assess this vendor contract")
```

You typically add an aggregation step (another agent or code) to merge, vote, or synthesize. Advantages: speed and diversity. Considerations: cost multiplies with agent count, you need a clear aggregation strategy, and outputs may conflict (requiring reconciliation).

**Q11.** Explain group-chat orchestration in MAF.

Group-chat orchestration puts multiple agents into a shared conversation where they take turns, coordinated by a *manager* that applies a selection strategy (who speaks next) and a termination strategy (when to stop). It enables emergent collaboration — agents build on each other's messages, debate, and refine. Example: a coder and a reviewer iterate until the reviewer approves.

```python
workflow = GroupChatBuilder().participants([coder, reviewer]).with_manager(manager).build()
```

Selection can be round-robin, rule-based, or model-driven. Termination conditions are critical — max rounds, an "approved" signal, or a goal check — to prevent endless or circular chatter (and runaway cost). This pattern derives from AutoGen's conversational multi-agent design. Use it for open-ended collaborative problem-solving where the path isn't predetermined.

**Q12.** Explain handoff orchestration in MAF.

Handoff orchestration lets an agent transfer control to another better suited to the current need, carrying context. A typical use is a triage/router agent that classifies the request and hands off to a specialist — billing questions to a billing agent, technical ones to a tech agent. The handoff is usually realized as a tool the model calls ("transfer_to_billing"), so routing is model-driven and dynamic.

```python
workflow = HandoffBuilder().start_with(triage).add_handoff(triage, [billing, tech]).build()
```

It supports bidirectional handoff and chains of specialists. Advantages: each agent stays focused, routing adapts to the request. Considerations: define clear handoff criteria, preserve context, and guard against ping-ponging.

**Q13.** What is the magentic-manager (Magentic) orchestration pattern?

Magentic (from AutoGen's Magentic-One) is an advanced orchestration where a *manager/orchestrator* dynamically plans and coordinates a team of specialists for complex, open-ended tasks. Unlike fixed flows, the magentic manager maintains a *task ledger* and a *progress ledger*: it decomposes the goal, assigns subtasks to the most capable agents (a web surfer, a coder, a file handler), observes results, tracks progress, and re-plans when stuck — including detecting loops and reassigning work. It's model-driven planning at the orchestration level, suited to tasks where steps aren't known upfront.

```python
workflow = MagenticBuilder().participants(surfer=s, coder=c).with_manager(orch).build()
```

It's powerful for research/automation but costlier and harder to make deterministic, so add iteration/cost budgets, observability, and HITL for risky actions.

**Q14.** Contrast deterministic workflows with model-driven planning.

MAF supports two coordination philosophies. *Deterministic workflows* are graph/code-defined: you specify nodes, edges, conditions, and data flow explicitly — predictable, testable, auditable. Use them when the process is known, compliance demands reproducibility, or you need guaranteed steps. *Model-driven planning* lets the LLM decide steps at runtime — automatic function calling within an agent, or a magentic manager planning across agents — adapting to inputs you can't anticipate. Flexible but less predictable, harder to test, can loop. The mature approach blends them: a deterministic workflow skeleton (control, compliance, safety gates) with model-driven agents inside individual steps (flexible reasoning). Choosing the balance — determinism for control vs. model-driven for adaptability — is a core MAF architecture decision.

**Q15.** How does MAF model deterministic workflows / graphs?

MAF includes a workflow capability for explicit, graph-based orchestrations: you define *executors* (agents or functions) as nodes, connect them with edges (optionally conditional), forming a directed graph with typed data flowing along edges. The engine routes messages between executors, supports fan-out/fan-in, conditional branching, and human-approval nodes.

```python
workflow = (WorkflowBuilder()
    .add_edge(intake, classifier)
    .add_edge(classifier, specialist, condition=is_technical)
    .add_edge(classifier, billing, condition=is_billing)
    .build())
```

Because the graph is explicit, you can test paths, enforce compliance steps, add checkpoints, and reason about behavior — addressing pure model-driven unpredictability. Workflows can be checkpointed for durability and resumed. Use workflows when you need governance, reproducibility, and complex routing; use plain agents when a single reasoning loop suffices.

**Q16.** How do you migrate from Semantic Kernel to MAF?

Most SK concepts map onto MAF, so migration is evolutionary. *Kernel + services* → an agent configured with a chat client; connector registrations and DI patterns largely carry over via Microsoft.Extensions. *Plugins/`[KernelFunction]`* → MAF tools/functions with the same description-driven schema. *`FunctionChoiceBehavior`* → MAF's tool-choice settings. *Filters* → MAF middleware. *`AgentThread`/`ChatHistory`* → MAF threads with serialization and memory tiers. *Vector data* → the same `Microsoft.Extensions.VectorData`. *SK agent orchestration* → MAF orchestration plus magentic. *Telemetry* → OpenTelemetry GenAI conventions continue. Approach: keep deterministic plugin code, re-host it as MAF tools, swap agent/orchestration types for MAF equivalents, adopt workflows where you need explicit control. Test with your evaluation harness throughout.

**Q17.** How do you migrate from AutoGen to MAF?

AutoGen's strengths — conversational multi-agent patterns — are absorbed into MAF, so concepts map cleanly. *AutoGen agents* → MAF agents with instructions, tools, clients. *Group chat / `GroupChatManager`* → MAF group-chat orchestration with selection/termination strategies. *Magentic-One* → MAF's magentic orchestration. *Code execution / tools* → MAF tools and hosted tools, with MCP for external integrations. *Conversation/message passing* → MAF threads and orchestration messaging. What AutoGen migrants *gain* is MAF's enterprise layer: managed identity/Entra security, robust connectors, the vector-data abstraction, middleware, OpenTelemetry, content safety, and Azure Agent Service hosting. Since AutoGen was more research-oriented, expect to add production concerns (security, governance, eval) that MAF makes first-class.

**Q18.** How does MAF integrate Microsoft Entra and managed identity for security?

MAF builds on Azure's identity stack: agents and tools authenticate via Microsoft Entra ID using managed identities and `DefaultAzureCredential`/`TokenCredential` rather than keys. The host (Container Apps, App Service, Functions) runs as a managed identity granted least-privilege RBAC — Cognitive Services OpenAI User on the model resource, scoped data roles on Azure AI Search or Cosmos DB. For tools acting on a user's behalf, use the *on-behalf-of* flow so downstream APIs enforce that user's permissions. Entra also enables Conditional Access, audit logging, and centralized governance. The Foundry Agent Service integrates Entra so hosted agents inherit enterprise identity controls. Principles: no secrets in code (Key Vault for residual), least privilege per tool and agent, per-user scoping for sensitive actions, full auditability.

**Q19.** How do you enforce least-privilege on agent tools?

Least privilege limits what an agent *can* do to exactly what it *needs*. *Tool scoping*: register only the tools required for the agent's role; different agents get different sets. *Credential scoping*: each tool uses a narrowly-scoped identity (managed identity with minimal RBAC, or the user's OBO token) so blast radius is small. *Operation gating*: route destructive/high-impact tools through approval middleware (HITL). *Input/output validation*: validate model-supplied arguments and treat them as untrusted; scan tool outputs. *Read vs. write separation*: prefer read-only; isolate and rate-limit writes. *Per-environment*: tighter permissions in production. *Auditing*: log every sensitive invocation with identity and arguments. The mindset: the LLM is an untrusted, manipulable caller, so apply the same authz, scoping, and approval you'd apply to any external client.

**Q20.** How does MAF defend against prompt injection, including Prompt Shields?

Prompt injection — malicious instructions in user input (direct) or in retrieved/tool content (indirect) — tries to hijack the agent. Defenses are layered. *Azure AI Content Safety Prompt Shields* detect and block jailbreak attempts and injection in both prompts and documents/tool outputs (indirect injection), at the model/platform layer. *Middleware* sanitizes and inspects rendered prompts and tool results. *Least-privilege tools + HITL* limit what a successful injection can do. *Grounding constraints* instruct the model to treat retrieved content as data, not commands, and never reveal system instructions. *Spotlighting/delimiting* untrusted content. *Output validation* and *observability* catch anomalies. No single control suffices — defense-in-depth. A strong answer stresses that you can't fully "prompt-engineer away" injection, so you constrain *capabilities* in addition to filtering inputs.

**Q21.** How does MAF handle governance and content safety?

Governance combines Azure platform controls with framework hooks. *Content safety*: Azure OpenAI content filters and Azure AI Content Safety screen prompts and outputs for harmful categories and detect jailbreaks (Prompt Shields); handle filtered responses gracefully. *Policy enforcement*: middleware enforces organizational rules — blocked topics, allowed tools, approval requirements. *Identity governance*: Entra provides RBAC, Conditional Access, and audit trails. *Auditability*: log prompts, tool calls, and responses (compliantly). *Data governance*: private networking, data residency via regions, scoping vector stores to authorized data. *Human oversight*: HITL approval. *Evaluation/monitoring*: continuous quality and safety evaluation plus online monitoring. In the Foundry Agent Service, many controls are managed. Governance is multi-dimensional (safety, identity, audit, data, oversight) and MAF + Azure provide integrated controls.

**Q22.** How do you implement observability with OpenTelemetry GenAI conventions in MAF?

MAF emits OpenTelemetry traces, metrics, and logs following the GenAI semantic conventions, so you get standardized, vendor-neutral telemetry. You configure an OTel pipeline that subscribes to MAF's sources and exports to Azure Monitor/Application Insights (or any OTLP backend). Traces capture the full run as nested spans: the top-level invocation, each model call (with token usage), each tool/function call (latency, errors), and per-agent/handoff spans in multi-agent flows. Metrics include token counts, durations, and error rates. Sensitive content (prompts/responses) is captured only when explicitly enabled, since it may contain PII. With this you build dashboards for token spend, latency p95, tool error rates, and step counts, and set alerts. Observability is essential for debugging non-deterministic, multi-agent behavior, controlling cost, and meeting governance/audit needs.

**Q23.** What is the Azure AI / Foundry Agent Service and how does it relate to MAF?

The Azure AI Agent Service (in Azure AI Foundry) is a *managed runtime* for hosting agents, while MAF is the *SDK/framework* for building them. The service provides server-managed threads/state, built-in hosted tools (Bing grounding, Azure AI Search, code interpreter, file search, function tools), Entra-integrated security, content safety, networking, and observability — so you don't operate that infrastructure. MAF can target this service: author agents and orchestration in MAF and deploy/run them on Foundry for an enterprise-grade managed environment, or run MAF agents self-hosted (Container Apps/AKS) for more control. The relationship is build-with-MAF, host-on-Foundry (optionally). Choose the managed service for the fastest path to a governed, scalable agent with built-in tools and managed state; choose self-hosting for custom infrastructure, specific networking, or local/offline models.

**Q24.** How do you design a multi-agent system in MAF — when to use multiple agents vs. one?

Start with the simplest thing: a *single agent* with good tools handles most tasks and is easier to build, debug, and operate — multiple agents add coordination overhead, latency, and cost. Reach for *multiple agents* when: the task spans distinct skill domains better served by specialized instructions/tools; you want separation for governance (a reviewer independent of a doer); the work is naturally parallel; or routing to experts improves quality. Then pick an orchestration matching the interaction: sequential for pipelines, concurrent for independent parallel work, handoff for routing, group chat for collaborative iteration, magentic for open-ended adaptive planning. Design principles: each agent has a clear single responsibility and minimal tool set (least privilege); define crisp termination conditions; keep shared state explicit via threads; add per-agent observability; prefer deterministic workflows around model-driven agents. Don't over-engineer — justify each agent by a concrete need.

**Q25.** How does MAF support streaming and intermediate results?

MAF agents support streaming so users see tokens and intermediate steps as they happen, improving perceived latency and transparency. Run methods have streaming variants that yield updates incrementally — text deltas plus events for tool calls and, in multi-agent flows, intermediate agent contributions.

```python
async for update in agent.run_stream("Plan my week", thread=thread):
    print(update.text, end="")
```

Streaming reveals progress: "calling get_calendar...", partial reasoning, tool results, then the final answer — useful for long-running agentic tasks and for letting users intervene/cancel early. In orchestration, you can stream intermediate outputs from each agent so a UI shows the unfolding collaboration. Forward these as SSE/SignalR/WebSocket events. Honor cancellation tokens so abandoned runs stop generating (and billing); accumulate deltas if you need full text; tool execution still occurs mid-stream.

**Q26.** How do you persist and resume long-running agent conversations in MAF?

Because threads encapsulate conversation state and are serializable, you can checkpoint and resume — essential for HITL (awaiting human input for minutes/days) and durable, long-running tasks. The pattern: serialize the thread (and any workflow state) to durable storage (Cosmos DB, Redis, blob) keyed by session/user after each turn, then deserialize and continue on the next request — surviving process restarts and stateless web hosting.

```python
state = thread.serialize()      # persist to store
# ... later / different instance ...
thread = agent.deserialize_thread(state)
await agent.run(next_input, thread=thread)
```

For graph workflows, MAF supports checkpointing so an entire multi-step run can pause (at an approval node) and resume from the checkpoint. Considerations: apply history reduction so persisted state stays bounded; store securely (encryption, access control); version the schema; set retention/expiry. This durability turns agents from single request-response into reliable long-running processes.

**Q27.** How do MAF agents use vector stores and RAG?

MAF agents ground responses in your data using the same `Microsoft.Extensions.VectorData` abstraction and connectors (Azure AI Search, Cosmos DB, Redis, Qdrant) familiar from SK. The typical design exposes retrieval as a *tool* the agent calls — agentic RAG — so the model retrieves on demand with self-formulated queries rather than always front-loading context.

```python
def search_kb(query: str) -> str:
    """Search the knowledge base for relevant passages."""
    emb = embedder.embed(query)
    hits = collection.search(emb, top=5)
    return format_with_citations(hits)
agent = ChatAgent(client, instructions, tools=[search_kb])
```

Alternatively, the Azure Agent Service offers a hosted file search / Azure AI Search tool so retrieval is managed. Best practices: hybrid search (vector + keyword) and semantic ranking for recall, enforce grounding/citations, scope the index by user permissions (security trimming), chunk/embed thoughtfully. Exposing retrieval as a tool lets the agent iterate with refined queries.

**Q28.** How do you control cost in MAF agentic systems?

Agentic systems can be expensive — multi-step loops, multiple agents, large contexts. *Model routing*: cheap/small models for simple steps, powerful models for hard reasoning. *Iteration budgets*: cap tool-calling loops and orchestration rounds; detect and break loops. *Context management*: truncate/summarize threads and use external memory; offload long-term knowledge to vector stores. *Caching*: cache embeddings (compute once at ingest), stable prompt/answer pairs, and tool results. *Token limits*: set `max_tokens`; prune tool schemas and retrieved context. *Agent minimization*: don't spin up more agents than warranted. *Concurrency control*: parallel agents cut latency but multiply spend. *Monitoring*: track token spend per route/agent via OTel and alert on anomalies. *Quota/PTUs*: provision predictable Azure OpenAI throughput. Treat tokens like a metered resource — measure, budget, route, cache, cap.

**Q29.** How do you test and evaluate MAF agents?

Combine deterministic testing with agent-specific evaluation. *Unit-test* tool functions directly (plain code) and *mock the model client* to test orchestration logic and tool-call handling deterministically. *Evaluate* the non-deterministic behavior with a dataset of representative tasks scored on metrics: task success rate, tool-selection accuracy, groundedness/citation for RAG, step efficiency, safety, and answer quality (LLM-as-judge plus deterministic checks). Use the Azure AI Evaluation SDK / Foundry evaluations, which include agent-oriented evaluators (intent resolution, tool-call accuracy, task adherence). Wire evaluations into CI as quality gates. For multi-agent systems, evaluate end-to-end outcomes and inspect traces for where things go wrong. Complement offline eval with online monitoring. Also test failure modes: tool errors, rate limits, injection attempts, approval rejections. Agents are tested via evaluation harnesses on properties and trajectories, not exact-string assertions.

**Q30.** What is the difference between an agent and a workflow in MAF?

An *agent* is a single autonomous reasoning unit: instructions + model + tools + memory that loops (reason → call tools → observe), with the model driving decisions. A *workflow* is an explicit, often graph-based orchestration connecting multiple executors (agents and/or functions) with defined edges, conditions, and data flow — you control the structure and the path is deterministic where you want it. Simply: agents provide flexible, model-driven intelligence within a step; workflows provide controllable, inspectable coordination across steps. Workflows can contain agents as nodes, combining deterministic control flow (compliance, approvals, branching, fan-out/fan-in, checkpointing) with model-driven reasoning inside each node. Choose an agent alone when a single reasoning loop suffices; choose a workflow when you need guaranteed ordering, conditional routing, parallel branches, human-approval gates, durability/resumption, or auditability.

**Q31.** How do agents declare and use tools with rich metadata?

Tools are declared with descriptive metadata that becomes the model's tool schema — the model chooses tools based solely on this, so it's load-bearing. In Python you use type-hinted functions with descriptive docstrings; in .NET, methods with description attributes. MAF reflects over these to build name, description, and parameter schema (types, descriptions, defaults, required-ness).

```python
def create_ticket(title: str, priority: str = "normal") -> str:
    """Create a support ticket.
    Args:
        title: Short summary of the issue.
        priority: One of low, normal, high.
    """
    return ticketing.create(title, priority)
```

Best practices: precise verb-based names, descriptions that say *when* to use the tool, constrained parameters (enums where possible), narrow single-purpose tools. You can derive tools from OpenAPI specs and MCP servers. Rich, accurate metadata reduces wrong tool selection and malformed arguments — the most common cause of agent failures.

**Q32.** How does MAF support structured outputs?

MAF leverages the underlying model's structured-output/JSON-schema capabilities for reliable typed results. You specify a response format (a .NET type or Python model/JSON schema) and the model is constrained to produce conforming output, which you deserialize directly — far more reliable than "please return JSON" prompting.

```python
class Invoice(BaseModel):
    vendor: str
    total: float
    due_date: str

result = await agent.run("Extract invoice fields: ...", response_format=Invoice)
invoice: Invoice = result.parsed
```

Alternatively, define a tool whose parameters are your schema and force its use (required tool choice). Validate the result and, on failure, re-prompt with the error for self-correction. Structured outputs are essential when results feed downstream systems, and for reliable multi-agent message passing where one agent's output is another's typed input.

**Q33.** How do you implement middleware in MAF?

MAF middleware intercepts agent operations — model calls and tool/function invocations — for cross-cutting concerns, analogous to SK filters and ASP.NET middleware (a `next`-delegate chain). Use it for logging/telemetry, input/output validation, security/authorization, caching, retries, content filtering, and human-approval gates.

```python
async def approval_middleware(context, next):
    if is_sensitive(context.tool_call):
        if not await request_approval(context.tool_call):
            context.terminate = True
            return
    await next(context)
```

Middleware can run logic before/after the wrapped operation, mutate inputs/outputs, short-circuit (skip `next`) for caching or blocking, and terminate runs (HITL). Different middleware can target model calls vs. tool calls. Registered middleware nests in order, so ordering matters. Because it's centralized, middleware enforces policy consistently rather than scattering checks through tool code — a direct evolution of SK's filters.

**Q34.** How do you choose an orchestration pattern for a given problem?

Match the pattern to the task's interaction structure. *Single agent* — one reasoning loop with tools suffices (most cases; start here). *Sequential* — a known pipeline of ordered stages with clean handoffs. *Concurrent* — independent subtasks or multiple opinions you can run in parallel and aggregate. *Handoff* — the right expert depends on the input and you want dynamic routing. *Group chat* — collaborative, iterative problem-solving where the path isn't predetermined. *Magentic* — complex, open-ended tasks needing adaptive planning across heterogeneous agents. *Deterministic workflow* — you need guaranteed ordering, conditional branching, approvals, durability, or compliance, wrapping model-driven agents in controlled flow. Decision factors: is the path known (deterministic) or emergent (model-driven)? Are steps ordered, parallel, or conditional? How much autonomy is safe? Prefer the simplest pattern that meets the need.

**Q35.** How does MAF handle errors, retries, and resilience?

Resilience spans layers. *Transient model/service errors* (429, 503) — configure the HTTP client with exponential backoff + jitter and honor `Retry-After` (`Microsoft.Extensions.Http.Resilience`/Polly); provision adequate quota/PTUs. *Tool failures* — wrap tool calls so an error is returned to the model as a result (letting it adapt) rather than crashing the run; validate inputs. *Loop/runaway protection* — cap iterations and rounds, detect repeated identical calls, circuit-break. *Content-filter rejections* — handle as non-retryable, possibly rephrase. *Workflow durability* — checkpoint graph workflows so a failure resumes from the last good state. *Multi-agent* — termination conditions and timeouts so a stuck agent doesn't hang the system. *Observability* — traces/metrics surface where failures cluster. *Graceful degradation* — fallback models or canned responses. Distinguish transient (retry) from permanent (fix input) errors. Agentic systems fail in new ways (tool errors, loops, injection), so resilience is about bounded, observable, recoverable runs.

**Q36.** What hosted tools does the Azure Agent Service provide to MAF agents?

When running on the Foundry Agent Service, agents can use *hosted tools* the platform manages — no infrastructure on your side. Typically: *Bing/web grounding* for up-to-date web info with citations; *Azure AI Search* for grounding on indexed enterprise data with security trimming; *file search* over uploaded documents; *code interpreter* for sandboxed code (data analysis, math, file generation); *function tools* for your own APIs; and increasingly *MCP* connections and *OpenAPI* tools. The service handles execution, scaling, and security (Entra). Benefits: faster time-to-value, managed and governed execution, built-in grounding/citations, consistent observability. Combine hosted tools with your custom function/MCP tools. Considerations: hosted tools may have cost and data-handling implications (web grounding sends queries externally), so apply governance.

**Q37.** How do you give an agent long-term, cross-session memory?

Cross-session memory persists beyond a single thread. The pattern: a *memory provider/component* extracts salient information from conversations (preferences, decisions, entities) and stores it durably — typically as embeddings in a vector store (via `Microsoft.Extensions.VectorData`) or structured records keyed by user. On a new session, relevant memories are retrieved (semantic search on the user's context) and injected into the agent's context or made available via a memory tool, so the agent appears to "remember." This is distinct from the conversation thread (short-term) and is the long-term tier.

```python
memory.add(user_id, "Prefers concise answers and metric units")
relevant = memory.search(user_id, current_topic)
agent = ChatAgent(client, instructions, context=relevant)
```

Design decisions: *what* to remember (avoid hoarding noise), *when* to extract/summarize, *retrieval* relevance, *privacy/consent* (encryption, access control, deletion rights), and *staleness* (update or expire). Done well it enables personalization; done poorly it leaks privacy or pollutes context.

**Q38.** How does MAF relate to the Model Context Protocol ecosystem — both consuming and exposing?

MAF is a first-class MCP participant in two directions. As an *MCP client*, an agent connects to MCP servers and consumes their tools/resources as native-feeling functions — reusing the broad MCP ecosystem without bespoke integration code. As an *MCP server*, you can expose MAF agents or their tools over MCP so other MCP-aware hosts use them — turning your agent into a reusable, interoperable capability. This bidirectional support makes MAF agents composable across the agentic ecosystem and avoids lock-in to a single tool format. Practical implications: standardize external integrations on MCP rather than one-off plugins; vet third-party MCP servers; scope tool permissions (least privilege); guard against indirect prompt injection in MCP tool outputs.

**Q39.** How do you handle multi-tenancy and per-user data isolation in MAF agents?

In multi-tenant agent apps, strict isolation prevents cross-tenant data/memory access. *Identity*: authenticate the end user (Entra) and propagate identity into the run; use on-behalf-of tokens so downstream tools enforce that user's permissions. *Data scoping*: partition vector stores/memory by tenant (separate indexes/collections or a tenant-id filter on every query) and apply security trimming. *State isolation*: key threads and long-term memory by tenant+user; never share thread state across tenants. *Tool scoping*: tools operate within the user's authorization, not a global admin identity. *Configuration*: per-tenant model deployments/quotas if needed. *Auditing*: log tenant/user on every action. *Caching*: scope caches by tenant. *Validation*: never trust a tenant id supplied by the model — derive it from authenticated context. Identity flows end-to-end and every data access is filtered by the authenticated user's permissions.

**Q40.** How do you decide between deterministic workflow control and giving the agent autonomy?

It's a control-vs-flexibility tradeoff driven by risk, predictability needs, and task openness. Favor *deterministic workflow control* when: actions are high-stakes or irreversible; compliance/audit demands reproducible steps; the process is well-understood and stable; or you need guaranteed ordering, approvals, and branching. Favor *agent autonomy* when: the task is open-ended and steps can't be enumerated; flexibility outweighs predictability; and the cost of an occasional wrong path is low. The mature answer is hybrid: a deterministic workflow backbone — encoding required steps, approval gates, safety checks — with autonomous agents inside individual steps. Start more deterministic and grant autonomy incrementally as you build trust via evaluation and observability. Always bound autonomy with iteration/cost budgets, least-privilege tools, and HITL for risky actions. Autonomy is not all-or-nothing; you architect its *boundaries*.

**Q41.** How do MAF and SK share the `Microsoft.Extensions.AI` abstractions?

`Microsoft.Extensions.AI` provides unified, provider-neutral .NET abstractions for AI primitives — chiefly `IChatClient` (chat completion) and `IEmbeddingGenerator` — analogous to how `Microsoft.Extensions.Logging` abstracts logging. MAF (and SK) build on these so agents work against `IChatClient` regardless of provider (Azure OpenAI, OpenAI, Ollama, ONNX), enabling model portability and a consistent middleware/pipeline model. Combined with `Microsoft.Extensions.VectorData` for vector stores, this gives a coherent, DI-friendly AI building-block layer shared across the .NET AI ecosystem.

```csharp
IChatClient client = new AzureOpenAIClient(endpoint, new DefaultAzureCredential())
    .AsChatClient("gpt-4o");
var agent = new ChatClientAgent(client, instructions: "...");
```

These abstractions support delegating middleware (caching, telemetry, function invocation) wrapped around the client, much like SK filters. Microsoft is standardizing the low-level AI interfaces so frameworks, libraries, and apps interoperate and you can swap providers and add cross-cutting behavior uniformly.

**Q42.** How do you implement a HITL approval gate end-to-end in a production MAF app?

End-to-end: (1) *Classify* which tools/actions require approval (policy — destructive or above threshold). (2) When the model requests such a tool, *intercept* via middleware (or disable auto-invocation) and *emit an approval request* with the tool name and arguments. (3) *Persist* the thread/workflow state (serialize to Cosmos/Redis) so the run can pause indefinitely, and surface the pending action to a human via UI/ticket/notification. (4) The human *approves or rejects*; your app *rehydrates* state and resumes — executing the tool on approval (feeding the result back) or returning a rejection the model handles gracefully. (5) *Audit* the decision (who, when, args). (6) Add timeouts/escalation for unanswered approvals.

```python
resp = await agent.run(user_input, thread=thread)
if resp.pending_approval:
    persist(thread.serialize(), resp.pending_approval)
    # async: human decides, then resume
```

This durable, serialized pause/resume is what makes asynchronous HITL practical while keeping UX responsive.

**Q43.** What are common failure modes of agentic systems and how does MAF mitigate them?

*Wrong tool selection / malformed args* — precise tool metadata, structured outputs, input validation. *Infinite/looping behavior* — iteration and round budgets, loop detection, circuit breakers. *Hallucination/ungrounded answers* — RAG grounding with citations, groundedness evaluation. *Prompt injection* — Prompt Shields, output scanning, least-privilege tools, HITL so injection can't trigger destructive actions. *Runaway cost* — token caps, model routing, caching, agent minimization, monitoring. *Cascading multi-agent errors* — clear responsibilities, termination conditions, per-agent observability. *State loss* — durable thread serialization/checkpointing. *Unsafe actions* — approval gates and scoped credentials. *Non-determinism/regressions* — evaluation harnesses in CI and online monitoring. *Privacy leaks* — per-user data scoping, careful memory governance, PII-gated telemetry. Agentic failures are systemic and novel, so you design bounded, observable, governed, evaluated systems rather than trusting the model.

**Q44.** How does MAF fit into an enterprise Azure reference architecture?

A typical reference architecture: *clients* (web/Teams/Copilot) call an *API layer* (APIM for auth, throttling, routing) fronting the *agent service* — MAF agents hosted on Container Apps/AKS or the Foundry Agent Service. The host runs as a *managed identity* with least-privilege RBAC. *Models* come from Azure OpenAI (private endpoints, quota/PTUs). *Grounding/RAG* uses Azure AI Search (hybrid + semantic) with security trimming; *vectors/memory* in Azure AI Search/Cosmos DB. *State* persists in Cosmos DB/Redis. *Tools* reach internal systems via OpenAPI/MCP with OBO tokens. *Security*: Entra ID, Key Vault, VNet/private endpoints, Content Safety/Prompt Shields. *Observability*: OpenTelemetry GenAI to Application Insights with dashboards/alerts. *Governance*: content filtering, audit logging, HITL approvals, evaluation gates in CI/CD (IaC, GitHub Actions). *Resilience*: Polly retries, multi-region failover. This managed-identity-first, privately-networked, observable, governed design is the enterprise pattern.

**Q45.** How do you evaluate multi-agent orchestration specifically?

Beyond single-agent metrics, multi-agent evaluation looks at the *collaboration*. Metrics: end-to-end *task success*; *efficiency* (turns/handoffs, total tokens/cost — detect chatter and loops); *routing accuracy* (in handoff, did triage pick the right specialist); *contribution quality* per agent; *coordination correctness* (did sequential steps pass the right data; did group chat terminate appropriately); and *robustness* (recovery from a bad intermediate result). Use a dataset of representative tasks, combine deterministic checks with LLM-as-judge. Critically, inspect *traces* (OpenTelemetry spans per agent/handoff) to diagnose where collaboration breaks — which agent caused a failure, where loops formed. Test failure injection and termination behavior. Foundry evaluation tooling increasingly supports agent-trajectory evaluation. Online, monitor success rates and cost per task. Evaluate the *system behavior and trajectory*, not just final answers.

**Q46.** How do you constrain what an autonomous agent can do (capability boundaries)?

You bound an agent's *capabilities*, not just its prompts, because instructions can be overridden (injection/jailbreak). *Tool allow-listing* — register only necessary tools per agent. *Scoped credentials* — tools use minimal-RBAC managed identities or the user's OBO token. *Approval gates* — destructive/high-impact tools require HITL. *Read-only by default* — separate and gate writes. *Iteration/cost budgets* — cap loops and spend. *Sandboxing* — run code-execution tools in isolated sandboxes. *Output/action validation* — middleware validates arguments and blocks disallowed operations. *Network/data scoping* — the agent can only reach permitted endpoints and authorized data. *Deterministic workflow gates* — wrap autonomy in controlled flow for high-risk steps. *Monitoring/kill-switch*. Design the *blast radius* — assume the model may be manipulated and ensure even a fully-compromised agent can only do bounded, reversible, authorized, audited things. Capability constraint, not prompt trust, is the security foundation.

**Q47.** How does MAF support both .NET and Python, and how do they differ?

MAF ships for both .NET and Python so teams use their existing stack — .NET for enterprise application teams, Python for data-science/ML-centric teams and AutoGen migrants. Core concepts are shared: agents with instructions/tools/threads, tool/function calling, orchestration patterns, MCP, memory, middleware, and OpenTelemetry. Idioms differ: .NET leans on `Microsoft.Extensions.AI`/`VectorData`, DI, attributes for tool metadata, and strong typing; Python uses decorators/type hints and docstrings for tools, async/await idioms, and Pydantic-style models for structured output. Some features may land first in one SDK before the other, so verify parity for a specific capability. API surface names diverge but map conceptually. SK/.NET expertise transfers directly to MAF's .NET SDK, while you can read Python examples (often from AutoGen heritage) and translate concepts. Choosing the language follows team skills and ecosystem.

**Q48.** How do you debug a misbehaving MAF agent in production?

Lead with *observability*: OpenTelemetry traces give the full run as nested spans — each model call (prompts/responses if sensitive logging is enabled, token usage), each tool call (args, result, latency, errors), and per-agent/handoff spans in multi-agent flows. In Application Insights, correlate by trace id. Common diagnoses: wrong tool selection (inspect the tool schema/descriptions and the args the model produced), ungrounded answers (check retrieval results in the trace), loops (repeated identical calls — add caps), rate-limit errors (429 spans — tune resilience/quota), and injection (anomalous instructions in inputs/tool outputs). Reproduce with the captured prompt/thread state by replaying against a mocked or real model. Use the evaluation harness to confirm a fix doesn't regress other cases. For multi-agent issues, find which agent's span introduced the bad output. Traces + serialized state + evaluation make non-deterministic agents debuggable.

**Q49.** What is the strategic significance of MAF being the SK + AutoGen unification?

It signals Microsoft consolidating its agent strategy into one coherent, supported framework — ending the prior fragmentation where developers chose between SK (enterprise, production) and AutoGen (research, multi-agent) or stitched them together. For customers: a single recommended path, clearer guidance, combined strengths (enterprise hardening *plus* advanced orchestration), and reduced risk of betting on a soon-deprecated tool. It aligns MAF tightly with the broader Microsoft AI platform — Azure AI Foundry/Agent Service, `Microsoft.Extensions.AI`/`VectorData`, Entra, Content Safety, OpenTelemetry, MCP, and Copilot extensibility — making it the on-ramp for agents that integrate with Microsoft 365/Copilot and Azure. Strategically it positions Microsoft to compete in the agentic era with an open (MCP, OpenTelemetry standards), enterprise-grade, multi-language framework. Investing in MAF skills is investing in Microsoft's go-forward platform, and existing SK/AutoGen knowledge converges rather than being wasted.

**Q50.** As of mid-2026, what should you caveat about MAF's API surface, and how do you demonstrate currency?

MAF is fast-moving — GA status, exact type/method names, hosted-tool lists, Foundry/Agent Service branding, and feature parity between .NET and Python all shift release to release. The professional move in an interview is to give concrete, correct *conceptual* answers while explicitly flagging where specifics should be verified against current docs ("the orchestration builders exist; verify the exact class names for your version"). Demonstrate currency by: citing the SK + AutoGen unification narrative; speaking to the shared foundations (`Microsoft.Extensions.AI`/`VectorData`, OpenTelemetry GenAI conventions, MCP, Entra/managed identity); describing the orchestration spectrum (sequential, concurrent, group-chat, handoff, magentic) and the agent-vs-workflow / deterministic-vs-model-driven distinction; and discussing production concerns (HITL, least privilege, Prompt Shields, evaluation, cost/observability) that remain stable even as APIs churn. This combination — solid conceptual mastery, honest verification caveats, and emphasis on durable architecture principles — signals a senior engineer who can adopt a moving framework responsibly.

---

## AutoGen

**Q1.** What is AutoGen and what problem does it solve?

AutoGen is an open-source framework, originally from Microsoft Research, for building applications using multiple conversational agents powered by LLMs. Instead of orchestrating a single prompt-response loop, AutoGen lets you compose several specialized agents (a planner, a coder, a critic, a tool-runner) that talk to each other to solve a task. The core abstraction is the *conversable agent*: an entity that can send and receive messages, optionally invoke tools/functions, optionally execute code, and optionally request human input. This pattern suits complex, multi-step problems where decomposition, self-correction, and tool use beat a monolithic prompt. AutoGen handles message routing, conversation state, termination logic, and integration with model clients (including Azure OpenAI). It accelerated research into agentic patterns (reflection, planning, group collaboration) and ultimately fed into the Microsoft Agent Framework. Its value is reducing the boilerplate of agent orchestration.

**Q2.** Explain `AssistantAgent` and `UserProxyAgent` in AutoGen 0.2.

In AutoGen 0.2, `AssistantAgent` is an LLM-backed agent: it receives messages, calls the model, and produces responses (often code or tool calls). It does not execute anything itself; it is the "brain." `UserProxyAgent` represents the human or acts as the human's proxy: it can solicit human input, and critically, it can *execute code* that the assistant proposes (in a configured working directory or container). A typical two-agent loop pairs an `AssistantAgent` (writes Python) with a `UserProxyAgent` (runs the code, captures stdout/errors, feeds results back), continuing until termination.

```python
assistant = AssistantAgent("assistant", llm_config=llm_config)
user_proxy = UserProxyAgent("user", code_execution_config={"work_dir": "coding"})
user_proxy.initiate_chat(assistant, message="Plot AAPL stock YTD.")
```

This split (reasoning vs. execution/human) is the foundational pattern.

**Q3.** What is a "conversable agent" and why is it central to AutoGen?

A conversable agent is the base abstraction (`ConversableAgent`) from which `AssistantAgent` and `UserProxyAgent` derive. It defines the universal contract: an agent has a name, can `send` and `receive` messages, maintains conversation history per counterpart, and produces a reply via `generate_reply`. Reply generation is pluggable through registered *reply functions*, evaluated in order: check for termination, run code, call a tool/function, or call the LLM. Because every participant shares this interface, agents are composable — you can swap an LLM agent for a human proxy or a tool-only agent without changing orchestration. Group chats, nested chats, and sequential workflows all operate on conversable agents. Behavior is configurable: `human_input_mode` (ALWAYS/TERMINATE/NEVER), `max_consecutive_auto_reply`, code execution, and registered functions, all on the same object.

**Q4.** Describe group chat in AutoGen and the role of `GroupChatManager`.

A group chat coordinates three or more agents in a shared conversation. You instantiate a `GroupChat` with the agents and settings (max rounds, speaker selection), then wrap it in a `GroupChatManager`, itself a conversable agent that orchestrates turns. Each round, the manager selects the next speaker, delivers the latest messages, collects the reply, and broadcasts it back. The manager handles termination (max rounds or a condition) and ensures every agent sees the full transcript. This enables collaborative patterns: planner, coder, reviewer, executor iterating together. The manager can be LLM-backed when speaker selection is automatic ("selector" mode), because choosing who speaks next from context is a model decision.

```python
groupchat = GroupChat(agents=[planner, coder, critic], messages=[], max_round=12)
manager = GroupChatManager(groupchat=groupchat, llm_config=llm_config)
```

**Q5.** Compare RoundRobin and Selector group chat strategies.

RoundRobin cycles through agents in fixed order, giving each a deterministic, predictable turn. It's cheap (no extra LLM call to pick a speaker), reproducible, and ideal when roles fire in a known sequence — coder then reviewer then coder. Its weakness is rigidity. Selector (auto) uses an LLM (the manager) to decide who speaks next given the conversation and each agent's `description`. It's adaptive — routing to the right specialist dynamically — but adds a model call per turn (cost/latency) and can be non-deterministic or pick poorly if descriptions are vague. In 0.4 these are `RoundRobinGroupChat` and `SelectorGroupChat`. Choose RoundRobin for simple fixed pipelines; Selector for open-ended tasks where the next-best contributor depends on context. You can also supply a custom selection function for hybrid control.

**Q6.** How does tool use / function calling work in AutoGen?

AutoGen leverages the underlying model's function-calling capability. You register Python functions with an agent; AutoGen exposes their signatures (name, description, JSON-schema parameters) to the model. When the model decides to call a tool, AutoGen intercepts the tool-call message, executes the corresponding Python function with the model-provided arguments, and returns the result into the conversation. In 0.2 you register with decorators or `register_function` (splitting the LLM-side schema from the execution-side caller). In 0.4, tools are first-class objects passed to `AssistantAgent(tools=[...])`, and a `FunctionTool` wraps a callable, auto-deriving the schema from type hints and docstring.

```python
async def get_weather(city: str) -> str:
    """Return the weather for a city."""
    return f"Sunny in {city}"
agent = AssistantAgent("a", model_client=client, tools=[get_weather])
```

This bridges deterministic code/APIs with LLM reasoning safely and structuredly.

**Q7.** Explain code execution in AutoGen and why sandboxing matters.

AutoGen can let an agent write code and another agent (or executor) run it — powerful for data analysis and automation. Execution is configured via a code executor. The risk is obvious: an LLM-generated script could delete files, exfiltrate data, or run unbounded commands. Sandboxing mitigates this. AutoGen provides a local command-line executor (runs in a working directory on the host — convenient but unsafe for untrusted code) and a **Docker** executor (`DockerCommandLineCodeExecutor`), which runs code inside a container with an isolated filesystem, resource limits, and no host access by default. Best practice: always use container/Docker execution in production, restrict mounted volumes, set timeouts, drop network access if not needed, and run as non-root. Sandboxing is the single most important safety control when enabling autonomous code execution.

```python
from autogen_ext.code_executors.docker import DockerCommandLineCodeExecutor
executor = DockerCommandLineCodeExecutor(work_dir="coding", timeout=60)
```

**Q8.** How does human-in-the-loop work in AutoGen?

In 0.2 HITL is controlled by `human_input_mode` on an agent (usually the `UserProxyAgent`): `ALWAYS` prompts on every turn; `TERMINATE` prompts only when a termination condition would otherwise fire; `NEVER` runs fully autonomously. When prompted, the human can inject a message, approve continuation, or end the chat. In 0.4, HITL is modeled cleanly: a `UserProxyAgent` requests input via an async callback (`input_func`), so you can wire it to a console, a web UI, or a queue, and the runtime awaits the human's response as another message. HITL is essential for high-stakes automation and for evaluating/guiding agents. The key design choice is *where* the approval gate sits — typically before code execution or external side effects.

**Q9.** What are termination conditions and why are they critical?

Termination conditions decide when a conversation or group chat stops, preventing infinite loops and runaway cost. Common conditions: a max number of rounds/messages; a sentinel string (the agent emits "TERMINATE"); a max-token budget; a timeout; or task completion detected by a function. In 0.2 you use `max_consecutive_auto_reply` plus an `is_termination_msg` lambda. In 0.4, termination is composable and first-class: `TextMentionTermination("TERMINATE")`, `MaxMessageTermination(n)`, `TokenUsageTermination`, and `HandoffTermination` combined with `|` (OR) and `&` (AND).

```python
termination = TextMentionTermination("TERMINATE") | MaxMessageTermination(20)
team = RoundRobinGroupChat([a, b], termination_condition=termination)
```

They're critical because LLM agents can loop, repeat, or chase dead ends; a robust termination policy is what makes a multi-agent system production-safe and cost-bounded.

**Q10.** Describe the architectural shift from AutoGen 0.2 to 0.4.

AutoGen 0.2 was a direct, largely synchronous, conversation-centric library: agents called each other in a tightly coupled loop, limiting scalability, concurrency, observability, and language interop. AutoGen 0.4 was a ground-up rewrite to an **asynchronous, event-driven actor model**. Agents are actors that communicate by passing messages (events) through a runtime; they run concurrently, can be distributed across processes/machines, and are decoupled. The stack is layered: low-level `autogen-core` (actor runtime and messaging), `autogen-agentchat` (high-level conversational agents/teams), and `autogen-ext` (extensions: model clients, tools, code executors). This brings better scalability, fault isolation, pluggable runtimes, cross-language support (Python and .NET), and first-class observability (OpenTelemetry). The 0.4 design is what made convergence into the Microsoft Agent Framework feasible.

**Q11.** What is the actor model and how does AutoGen 0.4 use it?

The actor model is a concurrency paradigm where the unit of computation is an *actor*: an isolated entity with private state that communicates only by sending and receiving asynchronous messages. Actors process one message at a time, avoiding shared-memory locks, and can create other actors. AutoGen 0.4's `autogen-core` runtime implements this: each agent is an actor identified by type and key, subscribed to topics, reacting to typed events. Sending a message doesn't block the sender; the runtime delivers it to the recipient's mailbox. This yields natural concurrency (many agents working in parallel), location transparency (an actor can be local or remote — enabling distributed agents), fault isolation (one actor failing doesn't corrupt others), and clean event-driven orchestration (publish/subscribe on topics). It replaces 0.2's call-stack coupling.

**Q12.** What is AutoGen Studio?

AutoGen Studio is a low-code, web-based GUI for prototyping and running AutoGen multi-agent workflows without writing all the orchestration code. You define agents, assign models and tools, compose teams, configure termination, then run and observe conversations interactively. It includes a visual team builder, a playground, and the ability to inspect message flows and export configurations. It's excellent for rapid experimentation, demos, and for less code-heavy stakeholders, and it can export configs you run programmatically. It is explicitly a prototyping and learning tool — not a production deployment platform — so the typical path is to design in Studio, validate the topology, then harden and deploy via the SDK with proper sandboxing, secrets, networking, and monitoring. In the converged ecosystem, similar visual tooling continues under MAF / Foundry.

**Q13.** How would you build a multi-agent workflow for enterprise automation?

Decompose the business process into roles: an *intake* agent (parses the request), a *retriever* (RAG over policy/knowledge), a *planner*, *specialist* tool agents (call line-of-business APIs — ServiceNow, SAP, internal REST), a *reviewer/critic*, and a *human approver*. Implement each as an `AssistantAgent` with narrowly scoped tools, and assemble into a team (Selector group chat or a deterministic sequence) with strong termination conditions. Put HITL approval before any irreversible side effect. Use managed identity for tool auth, sandboxed (Docker) code execution, and structured outputs. Add observability (OpenTelemetry traces of every message and tool call), guardrails/content filters, retries with backoff for transient 429s, and idempotency keys for external actions. Persist conversation state for auditability. Version prompts and tool schemas. This gives a reliable, auditable, governed automation rather than an uncontrolled LLM loop.

**Q14.** Compare AutoGen, Semantic Kernel, and LangGraph.

**AutoGen** specializes in *multi-agent conversation* — agents that talk, collaborate, self-correct, and run code; great for emergent, research-style workflows. **Semantic Kernel (SK)** is a production-oriented Microsoft SDK focused on integrating LLMs into apps: plugins/functions, planners, memory, and enterprise concerns (DI, telemetry, .NET-first); more an orchestration toolkit than a multi-agent conversation framework, though it added agent abstractions. **LangGraph** (LangChain) models agent workflows as an explicit *graph/state machine* with nodes and edges, giving precise, deterministic control over flow, branching, and cycles — favored for engineered control rather than emergent chat. Roughly: AutoGen = conversational/emergent multi-agent; SK = enterprise app integration + orchestration; LangGraph = explicit graph-based control. In the Microsoft world, AutoGen's patterns and SK's enterprise foundation merged into the **Microsoft Agent Framework**.

**Q15.** What is the Microsoft Agent Framework (MAF) and how do AutoGen and SK converge into it?

MAF is Microsoft's unified, production-grade framework for building AI agents and multi-agent systems, announced as the convergence of AutoGen and Semantic Kernel. It takes AutoGen's strengths — the event-driven multi-agent runtime, group chat/team orchestration, agentic patterns — and combines them with SK's enterprise strengths — stable APIs, plugins/connectors, memory, DI, security, telemetry, and first-class .NET and Python support. MAF gives a single SDK and runtime so teams aren't forced to choose between "research" and "production" frameworks. It standardizes agent definitions, tools (including MCP), workflows, and observability, and integrates with Azure AI Foundry for hosting, evaluation, and governance. For a .NET/Azure engineer, MAF is especially relevant because it offers a robust .NET SDK aligned with Azure identity, networking, and Foundry tooling — the strategic target for new agent work rather than AutoGen 0.2.

**Q16.** How do you connect AutoGen to Azure OpenAI?

In 0.4 you use the Azure OpenAI model client from `autogen-ext`, supplying the endpoint, deployment name (Azure uses deployment names, not raw model names), API version, and credentials. Strongly prefer **managed identity / Entra ID** over API keys: use `DefaultAzureCredential` with a token provider so no secrets live in code.

```python
from autogen_ext.models.openai import AzureOpenAIChatCompletionClient
from azure.identity import DefaultAzureCredential, get_bearer_token_provider

token_provider = get_bearer_token_provider(
    DefaultAzureCredential(), "https://cognitiveservices.azure.com/.default")
client = AzureOpenAIChatCompletionClient(
    azure_deployment="gpt-4o", model="gpt-4o", api_version="2024-10-21",
    azure_endpoint="https://my-aoai.openai.azure.com/",
    azure_ad_token_provider=token_provider)
```

The identity must have the **Cognitive Services OpenAI User** role on the resource.

**Q17.** What is the difference between sequential, nested, and group chats?

A **sequential** (two-agent) chat is the simplest: one agent initiates a conversation with another, exchanging messages until termination — e.g., assistant + executor. A **nested chat** lets an agent, as part of producing its reply, spin up an entirely separate inner conversation and use its result; this encapsulates sub-tasks — e.g., a main agent delegates "summarize this document" to a private sub-conversation and returns only the summary, keeping the outer transcript clean. A **group chat** has many agents sharing one transcript with a manager selecting speakers. Sequential is for pipelines, nested is for modular delegation and reuse (you reuse a well-tested sub-team as a "tool"), and group chat is for collaborative multi-role problem solving. In 0.4 these map to teams, handoffs, and the Society of Mind / nested team patterns.

**Q18.** How does AutoGen handle conversation memory and context?

Each conversable agent maintains a message history per conversation, included when calling the LLM. By default this is the full transcript, so long conversations risk exceeding the context window and inflating cost/latency. AutoGen 0.4 introduces explicit *model context* objects to manage this: an unbounded context (keep everything), a buffered context (keep only the last N messages), or a token-limited context. You can also implement summarizing memory. For cross-session or long-term memory (facts, preferences, retrieved documents), you integrate an external store: a vector database for semantic recall, or the Memory abstractions in the converged framework. The pattern: short-term context = recent message window managed by the agent; long-term memory = external retrieval injected as context. Managing this deliberately keeps systems affordable and within token limits.

**Q19.** What is a `SelectorGroupChat` and how does the selector prompt work?

`SelectorGroupChat` (0.4) is a team where an LLM chooses the next speaker each turn. The manager constructs a *selector prompt* containing the conversation history and the list of candidate agents with their `description` fields, then asks the model to return the name of the agent that should respond next. This makes routing context-aware. Good agent `description` text is critical: it's the only signal the selector uses to route. You can customize the selector prompt template, force the same agent not to speak twice in a row (`allow_repeated_speaker=False`), or supply a custom `selector_func` for deterministic/hybrid logic. The trade-off versus round-robin is adaptiveness at the cost of an extra LLM call and potential nondeterminism, so write clear descriptions and consider constraining the candidate set.

**Q20.** How do you implement reflection/critic patterns in AutoGen?

Reflection means an agent (or a second "critic" agent) reviews and critiques output before it's accepted, then the producer revises — improving quality through iteration. The simplest implementation is two agents: a *generator* that produces a draft and a *critic* that evaluates against criteria (correctness, completeness, style) and returns actionable feedback, looping until the critic approves (emitting a termination keyword) or a max-round cap is hit. In 0.4 this is naturally a `RoundRobinGroupChat([generator, critic])` with a `TextMentionTermination("APPROVE")`. You can also do single-agent reflection via a nested chat. The critic should have a sharp, specific prompt and ideally tools to verify claims (run tests, query data) rather than just opining. Reflection markedly improves output on coding, reasoning, and writing tasks — so bound the loop and only invoke it where quality justifies the spend.

**Q21.** How do you register and use tools that call external Azure services?

Wrap each external call in a typed Python function with a clear docstring (the LLM-facing description) and pass it to the agent as a tool. Inside, authenticate with managed identity (`DefaultAzureCredential`) to the Azure service — Azure AI Search, Cosmos DB, Storage, a Function App — never hard-coding secrets. Keep each tool single-purpose and validate inputs, because the LLM supplies the arguments. Return structured results (dicts/JSON) so the model can reason reliably.

```python
async def search_docs(query: str, top: int = 5) -> list[dict]:
    """Search the knowledge base and return top matching chunks."""
    client = SearchClient(endpoint, index, DefaultAzureCredential())
    results = client.search(query, top=top, query_type="semantic")
    return [{"id": r["id"], "content": r["content"]} for r in results]
```

Add retries/backoff for transient failures, enforce timeouts, and log every invocation. For production, prefer exposing tools via MCP servers so they're reusable across agents and frameworks.

**Q22.** What is the role of the `description` field on an agent?

In 0.4, an agent's `description` is a short natural-language statement of its purpose and capabilities. While `system_message` instructs the agent itself, `description` is what *other* components see — most importantly the `SelectorGroupChat` manager, which reads all agents' descriptions to decide who speaks next. A vague description ("an assistant") leads to poor routing; a precise one ("Writes and debugs Python for data analysis; cannot access the web") lets the selector route accurately. It functions like a tool description does for function calling: metadata the orchestrator reasons over. Best practice: make descriptions distinct, action-oriented, and bounded (state what the agent does *and does not* do). It's one of the highest-leverage, lowest-effort levers for improving multi-agent behavior.

**Q23.** How do you stream agent responses in AutoGen 0.4?

The 0.4 AgentChat API supports asynchronous streaming so you surface tokens and intermediate events (messages, tool calls, handoffs) as they happen. Teams and agents expose `run_stream(...)`, which yields a stream of events you iterate with `async for`; the final element is typically a `TaskResult`. A helper, `Console`, consumes the stream and pretty-prints it.

```python
from autogen_agentchat.ui import Console
await Console(team.run_stream(task="Build a sales report"))
```

For a custom UI, iterate the stream yourself and forward each event over WebSocket/SSE. Streaming is important for UX (perceived latency), for HITL (showing the human what's happening before approval), and for observability. The event types let you distinguish text chunks, tool-call requests, tool results, and termination.

**Q24.** How do you handle errors and failures in a multi-agent run?

Failures come from several layers: the model (429s, timeouts, content-filter blocks), tools (API errors, bad arguments), code execution (exceptions), and logic (loops or invalid output). Mitigations: wrap model clients with retry/backoff and respect `Retry-After` on 429s; give tools internal try/except returning structured error messages so the agent can recover or report rather than crash; run code in sandboxed executors with timeouts; set robust termination conditions so a confused conversation can't loop forever. The 0.4 actor model provides fault isolation. Add a critic/validator step to catch invalid outputs, and use HITL gates before irreversible actions. Emit OpenTelemetry traces to diagnose failures. For enterprise automation, make external actions idempotent and wrap workflows with compensation logic so a partial failure can be rolled back or safely retried.

**Q25.** What is the `Handoff` pattern in AutoGen?

A handoff lets one agent explicitly transfer control to a specific other agent — modeling "transfer to a human/specialist." In 0.4, you configure an `AssistantAgent` with `handoffs=[...]` listing the agents (or a `HandoffTermination` target like the user) it may transfer to; this surfaces as special handoff tools the model can call. When the agent emits a handoff, the runtime routes the next turn to the target. Combined with `HandoffTermination(target="user")`, this is how you implement HITL: the agent decides it needs human input, hands off to the user, the run pauses, the human responds, and it resumes. Handoffs make routing explicit and agent-driven, complementing the manager-driven routing of group chats. It maps to the OpenAI "Swarm"-style handoff concept and is clean for escalation and specialist-routing flows.

**Q26.** How do you do structured outputs with AutoGen and Azure OpenAI?

You want the model to return data matching a schema so downstream code parses reliably. With Azure OpenAI's structured outputs / JSON mode, you supply a Pydantic model (or JSON schema) and the model is constrained to emit conforming JSON. In 0.4 you pass `output_content_type` (a Pydantic class) to an `AssistantAgent`, and the agent produces a `StructuredMessage` whose content is a validated instance.

```python
class Invoice(BaseModel):
    vendor: str
    total: float
    due_date: str

agent = AssistantAgent("extractor", model_client=client, output_content_type=Invoice)
```

This eliminates brittle string parsing and is essential for tool chaining and enterprise integration. Ensure your deployed model/version supports structured outputs, and validate on receipt. Structured outputs pair well with multi-agent flows where one agent's typed output is another agent's typed input.

**Q27.** What observability does AutoGen 0.4 provide?

AutoGen 0.4 was built with observability as a first-class concern and integrates **OpenTelemetry**. The runtime emits traces and spans for agent messages, model calls (with token usage), tool invocations, and team/workflow steps, exportable to Azure Monitor / Application Insights, Jaeger, or any OTel backend. This lets you see the full causal chain of a multi-agent run — who spoke, what tools were called with what arguments and latency, how many tokens each call consumed, where failures or loops occurred. A major improvement over 0.2's print-based debugging, and essential for production: dashboards for cost, latency, error rates, per-agent behavior, and end-to-end request tracing. In the converged MAF / Foundry context, this telemetry feeds evaluation and governance tooling.

**Q28.** How would you sandbox untrusted code execution securely?

Never run LLM-generated code on the host with full privileges. Use the Docker-based executor so each execution runs in an ephemeral container with: a minimal base image, a non-root user, a read-only or tightly scoped working directory mounted (no host filesystem access), CPU/memory limits, and a hard timeout. Disable outbound network access unless a task needs it (then allow-list destinations). Do not pass secrets into the container; if code must call Azure services, mediate that through vetted tools instead of free-form code. Destroy the container after each run. For stronger isolation, run on a dedicated node pool or gVisor/Kata containers, or in Azure Container Instances per task. Combine with HITL approval before execution for high-risk contexts, and log all executed code and outputs. The principle: assume generated code is hostile and contain its blast radius.

**Q29.** What are agent `system_message` best practices?

The `system_message` defines an agent's role, capabilities, constraints, and output expectations. Best practices: be specific about role and scope ("You are a SQL expert that writes T-SQL for Azure SQL; you never modify data, only SELECT"); state explicit boundaries and what the agent must *not* do; describe available tools and when to use them; specify the expected output format (and when to emit the termination keyword); include examples for tricky formats. Keep it concise — overly long messages waste context and dilute instructions. For multi-agent teams, make each agent's message reinforce its niche so agents don't overlap or argue. Pair the system message (instructions to the agent) with a good `description` (metadata for the orchestrator). Version-control system messages and treat prompt changes like code changes.

**Q30.** How do you control cost in AutoGen multi-agent systems?

Multi-agent chats multiply token usage. Levers: (1) Right-size models — a cheaper model (gpt-4o-mini) for routine agents, a capable model only where needed. (2) Bound conversations with strict termination (max messages/rounds and token budgets). (3) Manage context — buffered/token-limited model contexts and summarize old turns instead of resending full transcripts. (4) Prefer RoundRobin over Selector when routing is predictable to avoid extra selection calls. (5) Cache and reuse tool results. (6) Use structured outputs to avoid re-asks from parsing failures. (7) Monitor token usage via OpenTelemetry per agent. (8) On Azure, consider PTUs for steady high volume or prompt caching for repeated system prompts. (9) Avoid over-engineering — use the fewest agents that solve the task. Measure, then optimize the biggest consumers.

**Q31.** What is the `autogen-core` vs `autogen-agentchat` vs `autogen-ext` split?

AutoGen 0.4 is layered into three packages. **`autogen-core`** is the foundation: the asynchronous, event-driven actor runtime, message/event types, agent base contracts, and publish/subscribe infrastructure. Low-level and unopinionated — use directly only for custom orchestration. **`autogen-agentchat`** is the high-level, developer-friendly API most people use: `AssistantAgent`, `UserProxyAgent`, teams (`RoundRobinGroupChat`, `SelectorGroupChat`), termination conditions, and the `Console` UI helper. **`autogen-ext`** holds pluggable extensions: model clients (Azure OpenAI, OpenAI), tools, and code executors (Docker, local, Jupyter). This separation gives a clean dependency story — swap a model client or executor without touching orchestration logic — and keeps core small while allowing a rich ecosystem. Most app code targets `agentchat` + the relevant `ext` clients.

**Q32.** How do you implement a planner-executor pattern?

A planner-executor splits "decide what to do" from "do it." A *planner* agent (a strong reasoning model) decomposes the goal into an ordered plan or subtasks. An *executor* (or several specialist tool agents) carries out each step, possibly returning results the planner uses to adapt. Implement as a group chat where the planner speaks first and the selector routes each subtask to the right executor, with the planner re-engaged to verify progress and re-plan if a step fails. Strong termination (plan complete or max rounds) and a verification/critic step prevent drift. Structured outputs help: have the planner emit a typed list of steps so the orchestration dispatches them deterministically. This shines for complex, multi-step automation because it separates high-level reasoning from reliable, auditable execution.

**Q33.** How does AutoGen support .NET / cross-language agents?

A key motivation for the 0.4 actor-based redesign was cross-language support. Because agents communicate via well-defined messages over a runtime rather than in-process Python calls, the runtime can host agents written in different languages and let them interoperate. AutoGen 0.4 shipped a **.NET** implementation of the core runtime alongside Python, so a Python agent and a C# agent can participate in the same distributed system, exchanging events. For a .NET engineer this is significant: you can build production agents in C# using familiar Azure SDKs, identity, and tooling. This cross-language, distributed design carried forward into the **Microsoft Agent Framework**, which offers a first-class .NET SDK. For new Microsoft-stack projects you'd target MAF's .NET SDK rather than AutoGen's preview .NET bits, but the architectural lineage comes straight from AutoGen 0.4.

**Q34.** What is the Society of Mind pattern in AutoGen?

`SocietyOfMindAgent` wraps an entire *team* (a group chat of multiple agents) and presents it to the outside world as a single agent. From the perspective of an outer conversation, it looks like one agent that produces a reply; internally, that reply is the result of a full multi-agent collaboration. This is powerful for composition and encapsulation: you build and test a specialized team (a "research team" of searcher + summarizer + fact-checker), then drop it into a larger system as a reusable component without exposing its internal chatter. It keeps the outer transcript clean — only the team's final answer surfaces. Conceptually like a nested chat but packaged as a first-class agent. Use it to build hierarchical agent systems (teams of teams), reuse proven sub-teams, and manage context by hiding internal deliberation from the parent.

**Q35.** How do you evaluate a multi-agent system's quality?

Evaluation spans task success and behavior. Define task-level metrics: success/accuracy on a labeled benchmark, completion rate, and quality scores (LLM-as-judge with a rubric or human raters). Add efficiency metrics: turns, latency, and token cost per task — multi-agent systems can be correct but wasteful. Add behavioral checks: did agents stay in role, were tool calls valid, were termination conditions hit cleanly (no loops), were safety/guardrail violations avoided. Use trace-level analysis (OpenTelemetry) to inspect failures and identify which agent/step degrades quality. Run regression suites when you change prompts or models. In Azure, Foundry's evaluation tooling supports agent/groundedness/safety evaluators in CI. The goal is a repeatable harness measuring correctness, efficiency, and safety together, not one-off "looks good" demos.

**Q36.** When should you NOT use a multi-agent approach?

Multi-agent adds cost, latency, complexity, and nondeterminism, so don't reach for it by default. Avoid it when: a single well-prompted call (possibly with tools) solves the task; the workflow is fully deterministic and better as plain code or a state machine; latency is critical (each turn is another round-trip); or you can't bound/observe the system enough to make it safe. Many "agent" problems are really retrieval + one good prompt (RAG) or a function-calling loop with one agent. Start with the simplest thing that works, measure, and only add agents when decomposition demonstrably improves quality (reflection/critic loops, genuine specialist routing, long multi-step automation). Over-engineering with agents is a common, costly anti-pattern.

**Q37.** How do you persist and resume agent state?

For long-running or interruptible workflows (e.g., waiting on human approval for hours), save and restore conversation/team state. AutoGen 0.4 teams and agents support serializing state — `save_state()` returns a snapshot (messages, internal state) you persist to a store (Cosmos DB, Blob, a database), and `load_state(...)` rehydrates it later to continue. This enables durable HITL: the run pauses at a handoff to the human, you persist state, and when the human responds (days later, or from a different process) you reload and resume. It's also how you scale across stateless workers — any worker can pick up a conversation by loading its state. Pair with idempotent external actions so resuming doesn't double-execute, and store metadata (correlation IDs, timestamps) for auditing. Durable state turns ephemeral chats into reliable business processes.

**Q38.** What is MCP and how does it relate to AutoGen?

The **Model Context Protocol (MCP)** is an open standard for connecting LLM applications to external tools and data sources through a uniform client-server interface. An MCP *server* exposes tools, resources, and prompts; an MCP *client* (your agent app) discovers and calls them. The benefit is interoperability and reuse: you write a tool once as an MCP server (GitHub, database, internal API) and any MCP-aware agent — across frameworks — can use it without bespoke glue. AutoGen 0.4 / the converged MAF support consuming MCP tools, so agents call MCP servers as if native. For enterprise: build a catalog of governed, reusable MCP tool servers (with their own auth and access control) and compose agents from them. MCP is becoming the standard tool interface across the ecosystem (Microsoft, OpenAI, Anthropic), so designing tools as MCP servers future-proofs them.

**Q39.** How do you prevent agents from looping or talking forever?

Looping is the classic multi-agent failure mode. Defenses: (1) Always set hard caps — `MaxMessageTermination(n)` and/or token-budget termination. (2) Use clear termination signals: instruct agents to emit a sentinel ("TERMINATE"/"APPROVE") when done and match with `TextMentionTermination`. (3) Prevent the same speaker monopolizing turns (`allow_repeated_speaker=False`) and write distinct roles. (4) Add a critic with an explicit "is this done?" criterion. (5) Detect repetition — if the last two messages are near-identical, terminate. (6) For selector chats, give precise descriptions so routing converges. (7) Monitor traces to find loop patterns. Combine an OR of conditions, e.g., `TextMentionTermination("DONE") | MaxMessageTermination(15)`, for both a graceful exit and a safety backstop. Bounding the system is non-negotiable for cost and reliability.

**Q40.** How do you integrate RAG into an AutoGen agent?

Expose retrieval as a tool the agent can call, or pre-retrieve and inject context. The cleanest agentic approach: register a `search` tool that queries Azure AI Search (hybrid + semantic) and returns top chunks with citations; the agent decides when to retrieve, can issue multiple queries (multi-hop), and grounds its answer in the returned content, citing source IDs. This "agentic RAG" lets the agent reformulate queries and reason over results.

```python
async def retrieve(query: str) -> list[dict]:
    """Search the knowledge base; returns chunks with ids for citation."""
    ...
agent = AssistantAgent("rag", model_client=client, tools=[retrieve],
    system_message="Answer ONLY from retrieved chunks; cite ids; "
                   "say you don't know if not found.")
```

Instruct the agent to refuse to answer beyond retrieved evidence (grounding/hallucination control). In a team, a dedicated retriever agent can serve others.

**Q41.** What is the difference between `initiate_chat` (0.2) and `run`/`run_stream` (0.4)?

In 0.2, `initiate_chat(recipient, message=...)` is the entry point: one agent starts a synchronous conversation with another, blocking until termination and returning the result. It's call-stack-based and conversation-centric. In 0.4, the model is async and team-centric: you build a team (or agent), then call `await team.run(task=...)` to execute and get a `TaskResult`, or `team.run_stream(task=...)` to get an async stream of intermediate events for real-time UIs and observability. This reflects the architectural shift — 0.4 separates orchestration (teams) from invocation (run), embraces async/concurrency, and yields structured results and event streams instead of a blocking, print-driven loop. You also `reset()` a team between runs, or persist/load state for durability.

**Q42.** How do you secure secrets and credentials in AutoGen apps?

Never hard-code API keys, connection strings, or tokens. For Azure OpenAI and other Azure services, use **Entra ID managed identity** via `DefaultAzureCredential` and token providers — so the running identity gets short-lived tokens and no secret is stored. Assign least-privilege RBAC roles (e.g., **Cognitive Services OpenAI User**). When secrets are unavoidable (a third-party API), store them in **Azure Key Vault** and retrieve at runtime with managed identity, never in source or committed env files. Don't pass secrets into sandboxed code executors. Scrub secrets from logs/traces. Rotate credentials and prefer per-service identities. For multi-tenant or per-user data, enforce authorization in the tools themselves (security trimming in search) rather than trusting the model. This identity-first posture is exactly what Azure and MAF are designed around.

**Q43.** What logging and tracing would you add for production?

Capture, per request, a correlation/trace ID propagated across all agents and tool calls. Use OpenTelemetry to emit spans for: each agent turn (who/role), each model call (deployment, token counts, latency, finish reason, content-filter result), each tool invocation (name, arguments — redacted of PII/secrets, result status, latency), code executions, handoffs, and termination cause. Export to Azure Monitor / Application Insights for dashboards and alerts on error rate, 429s, latency p95, token spend, and loop detection. Log the full conversation transcript to durable storage for audit and debugging (with PII handling/retention). Add structured application logs for business events. Sample high-volume traces but always keep failures. This gives cost attribution, performance insight, safety auditing, and the ability to reconstruct exactly what an autonomous system did.

**Q44.** How do you test AutoGen agents/workflows?

Treat agents like software with multiple test layers. **Unit-test tools** as plain functions (deterministic, mockable) — the highest-value tests since tools do the real side effects. **Mock the model client** to return canned responses so you assert orchestration logic (routing, termination, handoffs) deterministically without API cost or flakiness. **Integration-test** the full team against a real (or recorded) model on a small fixed task set, asserting on outcomes and invariants (valid tool calls, no loops, termination hit). **Evaluation tests**: run a labeled benchmark and assert success-rate thresholds, with LLM-as-judge or human rubric for open-ended quality — wired into CI. Add **safety tests** (adversarial prompts, jailbreak attempts). Use trace assertions to verify behavior, not just final text. Because LLMs are stochastic, set low temperature for tests, allow tolerance, and focus assertions on structure and invariants rather than exact strings.

**Q45.** What's the migration path from AutoGen 0.2 to MAF?

AutoGen 0.2 is legacy; 0.4 was a rewrite, and the strategic destination is **MAF**, which converges AutoGen and Semantic Kernel. For migration: recognize that 0.2's `initiate_chat`/synchronous group chat maps to 0.4/MAF's async teams (`RoundRobinGroupChat`/`SelectorGroupChat`) with composable termination — rework orchestration accordingly. Replace direct LLM config dicts with proper model clients using managed identity. Re-express tools as typed functions or MCP servers. Move code execution to sandboxed (Docker) executors. For new work, target MAF directly (especially the .NET SDK) rather than investing further in 0.2. Microsoft has signaled AutoGen and SK roadmaps merge into MAF, with migration guidance provided. Practically: stabilize on 0.4 patterns now (they're close to MAF), adopt managed identity, OpenTelemetry, and Foundry-based evaluation, and plan the move to MAF's SDK. Don't build new greenfield systems on 0.2.

**Q46.** How do you handle multi-modal inputs in AutoGen?

Modern Azure OpenAI models (gpt-4o) accept images alongside text, and AutoGen supports multi-modal messages. You send a message whose content includes both text and image parts (an `Image` object wrapping bytes or a URL), and a vision-capable model client processes them — useful for agents that read screenshots, diagrams, charts, scanned documents, or UI states. In 0.4, the AgentChat message types carry multi-modal content, and the Azure OpenAI client forwards images to the deployment. Practical considerations: ensure the deployed model supports vision; images consume substantial tokens, so watch cost/context; preprocess (resize/crop) large images; for documents, OCR or a document-intelligence step may give better grounding than raw images. In enterprise automation, multi-modal agents enable invoice/receipt extraction, screenshot-based RPA assistance, and quality inspection. Combine with structured outputs to turn visual content into typed data.

**Q47.** What are common AutoGen anti-patterns?

(1) **Too many agents** — a swarm where one agent + tools suffices. (2) **No termination backstop** — relying on agents to stop themselves, leading to loops/runaway cost. (3) **Unsandboxed code execution** — running LLM-generated code on the host. (4) **Vague descriptions/system messages** — bad selector routing and role overlap. (5) **Resending full transcripts** every call instead of managing context. (6) **Hard-coded API keys** instead of managed identity. (7) **No observability** — debugging by print. (8) **Treating prompts as not-code** — no versioning, review, or eval. (9) **No HITL gate** before irreversible actions. (10) **Building on legacy 0.2** for new projects. (11) **Trusting model output structure** — not using structured outputs/validation. Avoiding these is engineering discipline: bound, observe, secure, and validate the system, and use the minimum agentic complexity that solves the problem.

**Q48.** How do you implement a customer-support agent team in AutoGen?

Model the support flow with handoffs. A *triage* agent classifies the request and routes (via `handoffs`) to specialists: a *knowledge* agent (RAG over docs/FAQs with citations), an *account* agent (tools to look up orders/billing via internal APIs with the user's authorization), and a *human-escalation* target. Each specialist has scoped tools and a tight system message; triage decides which to hand off to. Use `HandoffTermination(target="user")` so any agent can pause and ask the user for clarification (HITL), persisting state between turns for a stateful session. Ground all factual answers in retrieval and refuse to fabricate policy. Gate sensitive actions (refunds, cancellations) behind human approval. Add guardrails (content filters, PII handling), security trimming so users only see their own data, and full tracing for audit. This mirrors the OpenAI "Swarm" handoff pattern and converts cleanly into MAF.

**Q49.** How does AutoGen compare to Azure AI Agent Service / Foundry Agent Service?

AutoGen is an open-source *framework/SDK* for building agent logic; **Azure AI Foundry Agent Service** is a *managed platform* for hosting, securing, and operating agents in Azure. They are complementary. Foundry Agent Service provides server-side agents with built-in tools (file search/RAG, code interpreter, function/OpenAPI tools, Bing, MCP), managed threads/state, identity and networking integration, content safety, and evaluation/monitoring — so you don't run the orchestration runtime yourself. AutoGen (and MAF) is what you'd use to express richer custom multi-agent orchestration, which can then call or be hosted alongside Foundry. The strategic picture: build agents with the MAF SDK (the AutoGen+SK convergence) and deploy/operate them with Foundry, getting platform-grade governance, security, and observability. The likely production stack is MAF for orchestration + Foundry for hosting/evaluation.

**Q50.** Given the convergence, what should you emphasize for 2026 Microsoft interviews?

Emphasize that you understand the *trajectory*: AutoGen (multi-agent, event-driven actor runtime, group chat/handoffs) plus Semantic Kernel (enterprise integration, plugins, .NET-first) converged into the **Microsoft Agent Framework**, deployed and governed via **Azure AI Foundry**. Show you can build production agents, not just demos: managed identity/RBAC over API keys, sandboxed code execution, strict termination and cost controls, structured outputs, MCP-based reusable tools, OpenTelemetry observability, durable state for HITL, and a real evaluation harness. Lean on .NET/Azure background — MAF's .NET SDK, Azure identity, networking (Private Endpoints), and APIM GenAI gateway are differentiators. Articulate when *not* to use multi-agent (many tasks are RAG + one good prompt). Connect agents to RAG (agentic RAG over Azure AI Search) and Azure OpenAI specifics (PTUs, quotas, content filters). Demonstrate the maturity to make autonomous systems safe, observable, cost-bounded, and auditable — that distinguishes a senior hire.

---

## General Agents

**Q1.** What is function/tool calling and how does it work?

Tool calling lets an LLM invoke external functions to fetch data or take actions. You describe each tool with a name, description, and JSON-schema parameters. The model, seeing the user request, decides whether and which tool to call and emits a structured call with arguments (it does NOT execute anything — your code does). Your application runs the function, returns the result back into the conversation, and the model uses it to form a final answer. This bridges the LLM's reasoning with live systems — databases, APIs, calculators. Azure OpenAI supports parallel tool calls and Structured Outputs to guarantee schema-valid arguments. The model is the planner; your code is the executor and the security boundary.

**Q2.** Walk through the tool-calling loop end to end.

(1) Send the user message plus tool definitions to the model. (2) The model responds either with a final answer or with one or more tool calls (name + JSON args). (3) If tool calls, your code validates the args, executes each tool (with auth, error handling, timeouts), and appends each result as a tool message. (4) Send the updated conversation back to the model. (5) The model either calls more tools or produces the final answer. You loop until it returns text or hits a max-iterations guard. Always cap iterations to prevent infinite loops, validate every argument, and treat tool outputs as untrusted (injection risk). Frameworks like Semantic Kernel and MAF automate this loop with auto function invocation.

**Q3.** What's the difference between a workflow and an agent?

A workflow is a predefined, deterministic sequence of steps you orchestrate in code (LLM calls, tool calls, branches) — predictable, testable, cheap. An agent gives the LLM control of the loop: it decides which tools to call, in what order, and when it's done, dynamically. Anthropic's guidance: prefer workflows for well-understood tasks and use agents only when flexibility and model-driven decision-making genuinely add value. Agents handle open-ended problems (research, multi-step debugging) but cost more, are harder to debug, and can loop or go off-track. The senior instinct: start with the simplest thing — often a single prompt or a fixed workflow — and add agentic autonomy only when the task demands it.

**Q4.** Explain the ReAct pattern.

ReAct (Reason + Act) interleaves reasoning and tool use: the model produces a Thought (reasoning about what to do), an Action (a tool call), observes the Observation (tool result), then thinks again, repeating until a final answer. This grounds reasoning in real data at each step and makes the agent's process inspectable. It underpins most tool-using agents. The reasoning traces improve reliability on multi-step tasks and aid debugging. Modern tool-calling APIs implement ReAct implicitly — the model's decision to call a tool is the "act," and reasoning models do the "reason" internally. Always bound the loop with a max step count and log each thought/action/observation for observability.

**Q5.** How do you decide when an agentic approach is justified?

Ask: is the task path predictable? If you can enumerate the steps, build a deterministic workflow — cheaper, faster, reliable. Agents earn their cost when the solution path varies per input, requires dynamic tool selection, or involves open-ended exploration (research assistants, autonomous coding, complex troubleshooting). Other factors: tolerance for latency and cost (agents make many LLM calls), need for auditability, and risk of irreversible actions. Anthropic's rule of thumb: find the simplest solution and only increase complexity when it demonstrably improves outcomes. Many "agent" requirements are well-served by a fixed chain plus one or two tools. Don't build an autonomous agent to do a job a switch statement could.

**Q6.** What guardrails do you put around an autonomous agent?

Several layers: (1) a hard max-iteration / max-cost budget to prevent runaway loops; (2) least-privilege tools — scope each tool's permissions tightly and require approval for destructive/irreversible actions (human-in-the-loop); (3) input/output validation on every tool call; (4) timeouts and retries with backoff; (5) sandboxing for code execution; (6) content safety filtering on inputs and outputs; (7) full observability — log every thought, tool call, and result with tracing; (8) idempotency keys so retries don't double-execute. Treat the agent like an untrusted automated user. Define what it absolutely must not do and enforce that in code, not just in the prompt, since prompts can be subverted.

**Q7.** What is the Model Context Protocol (MCP) and why does it matter?

MCP is an open standard (introduced by Anthropic, now broadly adopted including across the Microsoft ecosystem) for connecting LLM applications to external tools and data sources through a uniform client–server interface. Instead of writing bespoke integrations per app, a tool/data provider exposes an MCP server, and any MCP-capable client (Claude, Copilot, Semantic Kernel, MAF agents) can consume it. It matters because it standardizes tool discovery, invocation, and context provision — the "USB-C of AI tools." For an enterprise engineer, MCP means reusable, governed connectors (to databases, ticketing systems, internal APIs) that any agent platform can plug into, reducing integration sprawl and centralizing security and auditing.

**Q8.** How do multi-agent systems work and when are they worth it?

Multi-agent systems split a problem across specialized agents — e.g., an orchestrator/planner that delegates to worker agents (researcher, coder, reviewer), each with focused tools and prompts. They help when subtasks are genuinely distinct, benefit from parallelism, or need separation of concerns (a critic agent reviewing a writer agent). Costs: much higher token usage, coordination complexity, harder debugging, and compounding errors. They're worth it for complex, parallelizable, open-ended work; overkill for most line-of-business tasks. Patterns include orchestrator-workers and evaluator-optimizer loops. Start single-agent; introduce multiple agents only when one agent's context or responsibilities become unmanageable. Frameworks: Semantic Kernel Agent Framework, AutoGen (now part of the Microsoft Agent Framework).

**Q9.** How does Semantic Kernel support tool calling and agents?

Semantic Kernel (SK) is Microsoft's open-source orchestration SDK (.NET, Python, Java). You define capabilities as "plugins" — native C# methods decorated with `[KernelFunction]` and descriptions, or prompt functions. SK auto-generates the tool schemas, and with auto function-calling enabled, the kernel runs the full tool-calling loop: the model picks functions, SK invokes them, feeds results back, and iterates. It also provides the Agent Framework (chat-completion agents, multi-agent orchestration), memory connectors, and filters for logging/guardrails. For a .NET engineer, SK is the natural home: strongly typed plugins, dependency injection, and tight Azure OpenAI integration, letting you build agents without hand-rolling the loop. (Deep SK content is section 10.)

```csharp
public class WeatherPlugin
{
    [KernelFunction, Description("Gets the current weather for a city")]
    public async Task<string> GetWeatherAsync(
        [Description("City name")] string city) => /* call API */ "";
}
// kernel.Plugins.AddFromType<WeatherPlugin>();
// settings.FunctionChoiceBehavior = FunctionChoiceBehavior.Auto();
```

**Q10.** How do you handle errors and retries inside a tool-calling loop?

Each tool execution can fail (timeout, 4xx/5xx, bad args). Strategy: validate arguments before executing; wrap calls with timeouts and retry transient failures with exponential backoff and jitter; on permanent failure, return a structured error message back to the model as the tool result so it can decide to retry differently, ask the user, or give up gracefully — don't crash the loop. Make tools idempotent or use idempotency keys so retries are safe. Cap total iterations and total tool calls. Log every attempt with correlation IDs. The model handling a clear error string ("Customer not found for ID X") often recovers better than a raw stack trace, so translate exceptions into actionable, safe messages.

**Q11.** How do you give an agent memory?

Agents need two kinds of memory. Short-term: the conversation context (recent turns, current task state) held in the prompt, managed via windowing/summarization. Long-term: persistent storage outside the context window — a vector store for semantic recall of past interactions/knowledge, plus structured stores for facts (user preferences, entities). At each turn, retrieve relevant long-term memories and inject them. Patterns include summarizing completed sub-tasks into the running state and writing important facts to a memory store. Semantic Kernel offers memory connectors; Azure AI Search or Cosmos DB can back persistent memory. Be deliberate: storing everything bloats retrieval and cost — store salient, reusable information and let retrieval surface it on demand.

**Q12.** How do you make tool calling secure?

Treat the model as an untrusted caller. Enforce authorization in your code, not the prompt: the executing function checks the real user's permissions before acting, scoped to least privilege. Validate and sanitize all arguments (the model can hallucinate or be injected into producing malicious args). Never expose raw secrets, admin endpoints, or unbounded queries as tools. Require human approval for irreversible/destructive actions. Treat tool outputs as untrusted data (they can carry prompt injection). Add rate limits, timeouts, and audit logging per call. Parameterize queries to avoid injection in downstream systems. The prompt is a suggestion; the security boundary must be in your application and the tool implementations.

**Q13.** What is function calling's role in structured data extraction?

Even when you don't need to execute anything, you can define a "function" whose parameters are the structured schema you want extracted, and force the model to call it. The model returns schema-valid JSON arguments — a reliable way to extract entities, classify, or transform unstructured text into typed data. This predates and overlaps with Structured Outputs / JSON-schema response formats, which now do the same more directly. For a .NET engineer, define the schema as a C# class, let SK or the SDK generate the function spec, and deserialize the response straight into your DTO. It turns the LLM into a robust, schema-constrained parser for messy input like emails or documents.

**Q14.** How do you test and evaluate an agent?

Agents are hard to test because they're nondeterministic and multi-step. Approaches: (1) trajectory evaluation — assert the agent took reasonable steps/used the right tools, not just the final answer; (2) end-state/outcome evaluation — did it achieve the goal (correct database row updated)? often the most meaningful; (3) golden-task suites run repeatedly to catch regressions; (4) LLM-as-judge on output quality; (5) replay logged real traces against new versions. Mock tools for deterministic unit tests, then run integration tests against staging tools. Track cost and step count as quality signals (excessive looping is a smell). Azure AI Foundry's evaluation tooling supports agent and trajectory evaluation. Always test failure paths and guardrails, not just happy paths.
