# Microsoft Agent Framework

The Microsoft Agent Framework (MAF) is Microsoft's **unification of Semantic Kernel (SK) and AutoGen** into one supported, production agent framework, announced in 2025 and converging through 2026. The mental model:

- **Semantic Kernel** contributed the *enterprise plumbing*: connectors, function/tool calling (plugins), telemetry, DI integration, content filters, the `Kernel` orchestration object.
- **AutoGen** contributed the *multi-agent research surface*: conversational agents, group chat, the event-driven message-passing runtime, and orchestration patterns (round-robin, handoff, the Magentic manager).
- **MAF** is the merged product: one `AIAgent` abstraction, one runtime, first-class Azure AI Foundry integration, and a shared workflow/orchestration layer.

The single most important framing sentence for an interview: *"MAF is the supported, production successor to both SK Agents and AutoGen — SK's enterprise rigor plus AutoGen's multi-agent orchestration, with Azure AI Foundry as the managed hosting/identity/observability plane."*

> **(verify current GA status / API names)** — Through 2026 the .NET package surface centered on `Microsoft.Agents.AI` (agent abstractions) and `Microsoft.Agents.AI.Workflows` (orchestration), with Python under `agent_framework`. Older SK code lives under `Microsoft.SemanticKernel.*`. Confirm package names against current NuGet/PyPI before quoting them. SK's *deep* API content (Kernel, plugins, filters, RAG-with-SK) is section 10.

---

## Agent lifecycle

An agent in MAF has a predictable lifecycle: **construct (bind a model client + instructions + tools) → create a thread (conversation state) → run (single turn or streamed) → persist/serialize thread → dispose.** The key insight is that the *agent* is stateless and reusable; the *thread* (`AgentThread`) carries the per-conversation state. This separation is what makes MAF agents safe to register as DI singletons and reuse across requests.

```csharp
using Microsoft.Agents.AI;            // (verify current GA status / API names)
using Azure.AI.OpenAI;
using Azure.Identity;

// 1. Construct: agent is stateless, built once and reused (DI singleton-friendly)
var chatClient = new AzureOpenAIClient(
        new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!),
        new DefaultAzureCredential())          // Managed Identity, no keys
    .GetChatClient("gpt-4o")
    .AsIChatClient();                          // adapts to the M.E.AI abstraction

AIAgent agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
{
    Name = "support-triage",
    Instructions = "You triage customer support tickets. Be concise and cite the policy ID."
});

// 2. Create a thread = the unit of conversation state
AgentThread thread = agent.GetNewThread();

// 3. Run a turn (state accumulates on the thread, not the agent)
AgentRunResponse r1 = await agent.RunAsync("Customer can't reset password.", thread);
Console.WriteLine(r1.Text);

// follow-up reuses the same thread, so the agent "remembers" the context
AgentRunResponse r2 = await agent.RunAsync("They're on the Enterprise plan.", thread);

// 4. Persist thread for later (resume across processes / requests)
JsonElement serialized = thread.Serialize();
// ... store in Redis/Cosmos, rehydrate with agent.DeserializeThread(serialized)
```

Streaming is a first-class lifecycle path — use `RunStreamingAsync` for token-by-token UX:

```csharp
await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("Summarize the ticket.", thread))
    Console.Write(update.Text);
```

The full lifecycle, expanded: *definition* (name, instructions, model, tools, memory) → *instantiation* (against a chat client or the Foundry Agent Service) → *invocation* (run on a thread; reason, loop tools, stream) → *state management* (thread holds state; persist/resume) → *orchestration* (participate in multi-agent workflows) → *termination* (final answer, condition, approval halt, or error) → *disposal* (release resources; delete server-side threads for service-managed agents). Throughout, OpenTelemetry traces each phase and middleware intercepts tool calls.

---

## Threads, state, and serialization

A **thread** represents a conversation's state — its message history and any provider-managed context. Reusing a thread preserves multi-turn context; a new thread starts fresh. MAF supports both:

- **Client-managed threads** — history held in your process.
- **Service-managed threads** — the Azure AI / Foundry Agent Service stores messages server-side and you hold an id.

Because web apps are stateless per request, you **serialize the thread and persist it** (Cosmos DB, Redis, blob) keyed by user/session, then rehydrate on the next turn.

```python
thread = agent.get_new_thread()
await agent.run("Plan a trip to Tokyo", thread=thread)
state = thread.serialize()          # persist to durable store
# ... later / different process instance ...
thread = agent.deserialize_thread(state)
await agent.run("Make it cheaper", thread=thread)
```

State serialization is first-class precisely so you can pause, store, and resume long-running or human-in-the-loop conversations. Apply history reduction (truncation/summarization) to keep persisted state bounded, store it securely (encryption, access control, since it may contain PII), and version the schema. (See section 04 for memory tiers and section 05 for HITL.)

The DI rule that falls out of this: register the **agent as a singleton**, create a **thread per conversation**, and **never share a thread across users**.

---

## Agent communication

Agents communicate in two registers: **user↔agent** (the request/response turn above) and **agent↔agent** (messages routed by the runtime in multi-agent systems). MAF inherited AutoGen's **event-driven, message-passing runtime**: agents publish/subscribe to typed messages and the runtime delivers them, so an agent doesn't call another directly — it emits a message and the runtime routes it. Within one conversation, communication is a list of `ChatMessage` objects with roles (`user`, `assistant`, `tool`, `system`).

```python
# Python: agent-to-agent messaging  (verify current GA status / API names)
from agent_framework import ChatAgent
from agent_framework.azure import AzureOpenAIChatClient
from azure.identity import DefaultAzureCredential

client = AzureOpenAIChatClient(credential=DefaultAzureCredential())
researcher = ChatAgent(chat_client=client, name="researcher",
                       instructions="Gather facts. Hand structured notes to the writer.")
writer = ChatAgent(chat_client=client, name="writer",
                   instructions="Turn the researcher's notes into a 3-bullet brief.")

notes = await researcher.run("Latest on Azure AI Foundry agent hosting.")
brief = await writer.run(notes.text)
```

```csharp
// C#: the structured message envelope
var messages = new List<ChatMessage>
{
    new(ChatRole.System, "You route between specialist agents."),
    new(ChatRole.User, "I need a refund and my invoice corrected."),
};
AgentRunResponse resp = await routerAgent.RunAsync(messages, thread);
// resp.Messages contains assistant + tool messages produced this turn
```

The `tool` role matters: the model needs the tool-result message to ground its next turn. Prefer passing **structured (typed) data** between agents rather than round-tripping everything through free text. Direct method calls suit tightly coupled simple chains; the runtime's pub/sub suits scalable, decoupled, dynamic topologies (section 03).

---

## Tool / function calling

A "tool" is a C#/Python function the model can invoke. In .NET, MAF builds on `Microsoft.Extensions.AI`'s `AIFunction` / `AIFunctionFactory` — you decorate a method, the framework reflects its signature + XML doc into a JSON schema, the model emits a call, the runtime invokes it and feeds the result back, looping until a final answer. Tool design *is* the work: the model chooses tools based solely on names and descriptions, so write precise, verb-based names and descriptions that say *when* to use the tool.

```csharp
using System.ComponentModel;

[Description("Look up the order status for a given order ID.")]
static async Task<string> GetOrderStatus(
    [Description("The numeric order identifier")] int orderId)
{
    var o = await _orders.FindAsync(orderId);
    return o is null ? "not found" : $"{o.Status} (ETA {o.Eta:d})";
}

AIAgent agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
{
    Instructions = "Help with orders. Call tools rather than guessing.",
    ChatOptions = new ChatOptions
    {
        Tools = [ AIFunctionFactory.Create(GetOrderStatus) ],
        ToolMode = ChatToolMode.Auto   // Auto / Required / None
    }
});
```

The three tool modes (carried forward from SK's `FunctionChoiceBehavior`): **Auto** lets the model decide (the default for agentic behavior); **Required** forces at least one call (deterministic extraction); **None** advertises tools for context but forbids calling (dry-run / explain modes). You can also disable auto-execution to insert an approval step. Tools can be local functions, OpenAPI-imported operations, MCP server tools, or Foundry hosted tools (code interpreter, file/web search). Middleware intercepts each call for logging, validation, and HITL gates.

> Tools run in **your process with your privileges** — this is the security boundary. See section 08 for least-privilege scoping and prompt-injection defense.

---

## MCP (Model Context Protocol)

**MCP** is an open standard for connecting AI applications to external tools, data, and prompts via a uniform client–server protocol. An MCP *server* exposes tools/resources; an MCP *client* (the agent host) discovers and invokes them. MAF is a first-class MCP participant in **both directions**:

- **As an MCP client**, an agent consumes any MCP server's tools as if native — reusing the growing ecosystem (GitHub, databases, filesystems, SaaS, your own servers) with consistent discovery and auth, instead of writing bespoke plugins per integration.
- **As an MCP server**, you can expose MAF agents or their tools so other MCP-aware hosts (other agents, IDEs, apps) use them — turning your agent into a reusable, interoperable capability.

```python
# Attach an MCP server's tools to an agent  (verify current GA status / API names)
from agent_framework import ChatAgent, MCPStdioTool
fs_tools = MCPStdioTool(name="filesystem", command="npx",
                        args=["-y", "@modelcontextprotocol/server-filesystem", "/data"])
agent = ChatAgent(chat_client=client, tools=[fs_tools],
                  instructions="Use the filesystem tools to read project files.")
```

```csharp
// conceptual: consume an MCP server's tools
var mcpTools = await mcpClient.ListToolsAsync();
agent = new ChatClientAgent(client, new ChatClientAgentOptions { /* tools: mcpTools */ });
```

MCP is the "USB-C of AI tools" — standardize external integrations on it rather than one-off plugins. Security still applies: vet third-party MCP servers, scope tool permissions (least privilege) since MCP tools execute real actions, and guard against indirect prompt injection in their outputs.

---

## Shared foundations: `Microsoft.Extensions.AI` and `VectorData`

MAF and SK both build on `Microsoft.Extensions.AI` — provider-neutral .NET abstractions for AI primitives, chiefly `IChatClient` (chat) and `IEmbeddingGenerator` (embeddings), analogous to how `Microsoft.Extensions.Logging` abstracts logging. Combined with `Microsoft.Extensions.VectorData` for vector stores, this gives a coherent, DI-friendly building-block layer shared across the .NET AI ecosystem, with delegating middleware (caching, telemetry, function invocation) wrapped around the client. The strategic point: your SK/.NET expertise transfers directly to MAF, because the agent abstractions sit on the same low-level interfaces.

---

## What interviewers probe

- *What is MAF and why does it exist?* The unification of SK (enterprise plumbing) and AutoGen (multi-agent runtime/patterns) into one supported framework, with Azure AI Foundry as the hosting/identity/observability plane.
- *Where does conversation state live — agent or thread?* The thread. Agents are stateless, reusable, singleton-friendly; threads carry serializable per-conversation state, never shared across users.
- *How do you resume after a process restart?* Serialize the thread to durable storage, deserialize to rehydrate.
- *How does MAF do tools?* `AIFunction` from `Microsoft.Extensions.AI` (schema auto-generated from signatures + descriptions), plus MCP client/host and Foundry hosted tools; Auto/Required/None modes.
- *What is MCP and why does it matter?* Open protocol to expose tools/resources to any agent; MAF is both client and host — interop without bespoke adapters.
- *Why pass `DefaultAzureCredential` instead of a key?* Managed identity, no secret sprawl, works with Key Vault / Entra (section 08).
