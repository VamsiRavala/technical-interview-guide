# Agents & MAF

The **SK Agent Framework** (`Microsoft.SemanticKernel.Agents.*`) adds first-class agents on top of the kernel: an agent is an autonomous entity with instructions, a kernel (tools), and the ability to participate in conversations. This page covers SK agents, thread/state management, multi-agent orchestration, and — critically for mid-2026 interviews — how SK **converges into the Microsoft Agent Framework (MAF)**.

> Deep MAF content (agent lifecycle, MCP, memory tiers, workflows vs model-driven planning, magentic, migration) lives in section **11**. This page is the SK-to-MAF bridge.

---

## Agent types

| Agent | Backed by | State | When |
|---|---|---|---|
| `ChatCompletionAgent` | Your `IChatCompletionService` | Client-side `ChatHistory` you control | Max control/portability; any chat model; offline/local |
| `OpenAIAssistantAgent` | OpenAI/Azure OpenAI Assistants API | Server-managed threads (code interpreter, file search) | You specifically need Assistants-API features |
| `AzureAIAgent` | Azure AI / Foundry Agent Service | Managed threads + built-in tools (Bing grounding, AI Search, code interpreter) | Enterprise Azure with managed state, governance, Entra |

```csharp
using Microsoft.SemanticKernel.Agents;

ChatCompletionAgent agent = new()
{
    Name = "ResearchAgent",
    Instructions = "You research questions using the provided tools. Cite sources.",
    Kernel = kernel,
    Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    })
};

var thread = new ChatHistoryAgentThread();
await foreach (var msg in agent.InvokeAsync("Summarize Q2 revenue drivers.", thread))
    Console.WriteLine($"{msg.Message.Role}: {msg.Message.Content}");
```

```python
# Python ChatCompletionAgent
from semantic_kernel.agents import ChatCompletionAgent
agent = ChatCompletionAgent(
    kernel=kernel, name="ResearchAgent",
    instructions="Research questions using tools. Cite sources.",
)
async for response in agent.invoke("Summarize Q2 revenue drivers."):
    print(response.content)
```

---

## Threads and conversation state

An `AgentThread` represents a conversation's state — message history and any provider-managed context. Each `InvokeAsync` advances a thread; passing the same thread preserves context, while a new thread starts fresh. For `ChatCompletionAgent` the thread is a client-side `ChatHistory` wrapper you control; for `OpenAIAssistantAgent`/`AzureAIAgent` it's server-managed and you hold a thread id.

```csharp
AgentThread thread = new ChatHistoryAgentThread();
await foreach (var m in agent.InvokeAsync("Hi", thread)) { /* ... */ }
await foreach (var m in agent.InvokeAsync("Continue", thread)) { /* ... */ }
```

**Production gotcha:** Thread/state management is where things break. `ChatCompletionAgent` keeps history **in-process** — lost on restart. For durable, multi-turn enterprise agents, use a hosted thread (Azure AI Agent Service) or **persist `ChatHistory` yourself**, keyed by user/session in Cosmos DB / Redis, with **summarization-based truncation** to bound context growth and cost (see [09-production-patterns.md](09-production-patterns.md)). Web apps are stateless per request, so serialize the thread and rehydrate on the next turn.

---

## Multi-agent orchestration

The SK Agent Framework provides orchestration so specialized agents collaborate:

| Pattern | Shape | Example |
|---|---|---|
| **Sequential** | Pipeline; each agent consumes the previous output | research → draft → review |
| **Concurrent** | Fan out the same input to several agents, aggregate | multiple reviewers, ensemble |
| **Group chat** | Agents converse in a shared thread; a selection strategy picks who speaks, a termination strategy decides when to stop | coder ↔ reviewer until approved |
| **Handoff** | An agent transfers control to a better-suited agent | triage → billing/tech specialist |

```csharp
var chat = new AgentGroupChat(researcher, writer, reviewer)
{
    ExecutionSettings = new()
    {
        SelectionStrategy = mySelectionStrategy,
        TerminationStrategy = myTerminationStrategy   // ALWAYS bound this
    }
};
await foreach (var msg in chat.InvokeAsync()) { /* ... */ }
```

**Production gotcha:** Multi-agent group chat can loop forever. Always set a termination strategy / max turns, give agents clear single responsibilities, and add per-agent observability.

---

## The convergence: SK → Microsoft Agent Framework

As of 2025–2026 Microsoft unified the agent stories of **Semantic Kernel** and **AutoGen** into the **Microsoft Agent Framework (MAF)**. The lineage:

- **SK** contributed the production primitives: kernel, plugins/connectors, filters, telemetry, DI, type-safe .NET surface.
- **AutoGen** contributed research-grade multi-agent orchestration patterns (group chat, magentic).
- **MAF** is the converged, supported path going forward — a single `AIAgent` abstraction, runtime, and orchestration layer, available in .NET and Python, integrated with **Azure AI Foundry Agent Service**.

```text
   Semantic Kernel ─┐
   (connectors,     │
    filters, DI,    ├──►  Microsoft Agent Framework (MAF)
    vector data)    │     single AIAgent abstraction +
                    │     orchestration + workflows
   AutoGen ─────────┘     (Azure AI Foundry integrated)
   (multi-agent
    patterns)
```

**SK is not deprecated.** It remains the GA orchestration/connector layer, and many MAF capabilities build on or interoperate with SK types. Practical guidance for mid-2026:

- *New* multi-agent systems → target **MAF**.
- Existing SK Agent Framework code is supported, with a clear migration path.
- Single-agent + tools work and enterprise RAG are fine to ship on **SK** today.

### What carries over from SK to MAF

| SK concept | MAF equivalent |
|---|---|
| Kernel + connector registrations | Agent configured with a chat client; same DI / managed identity |
| Plugins / `[KernelFunction]` | MAF tools/functions, same description-driven schema |
| `FunctionChoiceBehavior` | MAF tool-choice settings |
| Filters | MAF middleware (cross-cutting, HITL gates) |
| `AgentThread` / `ChatHistory` | MAF threads with serialization + memory tiers |
| `Microsoft.Extensions.VectorData` | Unchanged — same abstraction and connectors |
| SK orchestration (sequential/concurrent/group-chat/handoff) | MAF orchestration + AutoGen's magentic manager |
| OpenTelemetry GenAI conventions | Carry forward and deepen |

So an engineer fluent in SK plugins, function calling, filters, vector data, and Azure integration is well positioned; the main new learning is MAF's unified agent abstractions and the workflow-vs-model-driven distinction. (Verify current GA status and exact API names — this surface is fast-moving.)

---

## Choosing an agent host

- **`ChatCompletionAgent`** — maximum control and portability; you own history, tools, state; works with any chat model including local/offline.
- **`AzureAIAgent` (Foundry Agent Service)** — managed threads, built-in tools (Bing grounding, AI Search, code interpreter), Entra security, governance; fastest path to a governed, scalable agent.
- **`OpenAIAssistantAgent`** — when you specifically need Assistants-API features.

---

## Interview angle

> "Should we use SK Agents or the Microsoft Agent Framework?" The credible answer: "For greenfield multi-agent in mid-2026, **MAF** — it's the converged, supported direction (SK + AutoGen) and integrates with Azure AI Foundry. **SK** is still the right call for single-agent tool-calling and as the connector/filter/DI foundation; it interoperates with MAF, so investing in SK skills isn't wasted." Naming the convergence — and that SK remains GA, not deprecated — is the differentiator. For state: persist `ChatHistory`/threads with summarization-based truncation; in-process history is lost on restart.

---

**Next:** [Production Patterns](09-production-patterns.md) — streaming, resilience, caching, telemetry, testing, multi-tenancy.
