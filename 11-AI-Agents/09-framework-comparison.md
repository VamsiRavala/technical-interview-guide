# Framework Comparison

Five frameworks dominate the agent conversation in mid-2026: **Microsoft Agent Framework (MAF)**, **Semantic Kernel (SK) Agents**, **AutoGen**, **LangGraph**, and **LangChain**. For a Microsoft-centric role the through-line is simple: *MAF is the strategic default; SK and AutoGen are its parents converging into it; LangGraph/LangChain are what you'd pick if you weren't committed to Azure.*

> Currency: mid-2026. MAF's surface is fast-moving — **(verify current GA status / API names)**. SK's deep API content is section 10; AutoGen depth is section 06.

---

## The big table

| Dimension | **Microsoft Agent Framework (MAF)** | **Semantic Kernel Agents** | **AutoGen** | **LangGraph** | **LangChain** |
|---|---|---|---|---|---|
| **Primary language** | C# (.NET) **and** Python, co-equal | C# (.NET) first; Python/Java | Python first; .NET port (lagging) | Python first; JS/TS | Python first; JS/TS |
| **Programming model** | Unified `AIAgent` + Workflows graph; event-driven runtime; DI-native | `Kernel` + plugins; `Agent` abstraction; DI-native | Async event-driven actors + conversational `AgentChat` | Explicit state-machine **graph** (nodes/edges, shared state) | Chains/LCEL pipelines + agent executors |
| **Orchestration** | Sequential, concurrent, group chat, handoff, magentic/manager (built-in) | Agent group chat, handoff (SK Agents) | Group chat, RoundRobin/Selector, handoff, Magentic-One | Arbitrary directed graphs with cycles, conditional edges | Linear chains + ReAct agent loop |
| **State / memory** | Stateless agent + serializable `AgentThread`; vector connectors; durable workflow checkpoints | Thread + memory connectors; vector stores | Per-agent (actor) state; serializable team state | First-class persisted graph state + checkpointer (durable) | Memory classes + external vector stores |
| **Tooling** | `AIFunction` (M.E.AI), MCP client/host, Foundry hosted tools | Plugins/functions, OpenAPI, MCP | Tool functions, code executors, MCP | Tools via LangChain ecosystem | Largest tool/integration ecosystem |
| **Enterprise / Azure fit** | Deepest: Foundry hosting, Entra MI, OTel, Content Safety, APIM | Strong: Azure connectors, OTel, content filters | Good; you wire up Azure yourself | Cloud-agnostic; LangSmith for ops | Cloud-agnostic; broad provider support |
| **Maturity (mid-2026)** | Converging to GA; the strategic, supported path | Mature; **folding into MAF** | Mature research lineage; **folding into MAF** | Mature, widely adopted for stateful agents | Very mature, huge community |
| **Best-fit use cases** | Production agents on Azure, .NET shops, governed multi-agent systems | Existing SK/.NET apps migrating toward MAF | Research, experimentation, multi-agent prototypes | Complex stateful/cyclic workflows, HITL graphs | Rapid prototyping, RAG, broad integrations |

---

## Strengths and weaknesses

### Microsoft Agent Framework (MAF)
**Strengths:** first-class C# *and* Python; deepest Azure/Foundry/Entra/OTel/Content-Safety integration; unifies SK + AutoGen so you don't pick a dead-end; built-in orchestration including Magentic; durable workflows with HITL; Microsoft's long-term supported path.
**Weaknesses:** newest of the five — API churn through 2026 (**verify GA**), smaller community/sample base than LangChain, some docs lag the code; teams must tolerate a moving target.

### Semantic Kernel Agents
**Strengths:** mature, stable, well-documented; excellent DI and connector ecosystem; strong enterprise features (filters, telemetry, OpenAPI/MCP tooling); battle-tested in .NET shops.
**Weaknesses:** multi-agent orchestration was less rich than AutoGen's; planners are deprecated; **strategic direction is to merge into MAF**, so it's no longer the forward investment target.

### AutoGen
**Strengths:** pioneered rich multi-agent patterns (group chat, RoundRobin/Selector, handoff, Magentic); clean event-driven actor runtime; strong code-execution agents; fast to prototype.
**Weaknesses:** Python-centric; historically lighter on enterprise hardening (identity, governance, durable HITL); **converging into MAF**, so production teams should target MAF.

### LangGraph
**Strengths:** explicit graph model (nodes/edges/conditional cycles) gives fine control and inspectability; first-class persisted state with checkpointers (great for HITL/resumability); strong LangSmith observability; large Python/JS community.
**Weaknesses:** Python/JS-first (no first-class .NET); cloud-agnostic rather than deeply Azure/Entra-integrated; graph authoring has a learning curve; enterprise identity/governance is bring-your-own.

### LangChain
**Strengths:** largest ecosystem and community; integrations for nearly every provider; LCEL for composable chains; fastest path to a working RAG/agent prototype.
**Weaknesses:** historically heavy abstractions and churn; for complex stateful agents teams graduate to LangGraph; not Azure/Entra-native; enterprise governance is DIY.

---

## When to pick which (opinionated)

For a C#/Azure shop in mid-2026:

- **Default to MAF for anything net-new.** On Azure, in .NET (or mixed .NET/Python), needing identity, audit, content safety, and supported multi-agent orchestration — MAF is the answer, and the only option here with first-class C#. Accept some API churn (**verify GA**) in exchange for being on the path Microsoft is actually investing in. This is the answer to give in a Microsoft-centric interview.
- **Stay on SK only if you already have a substantial SK codebase** and rewriting now outweighs the benefit. Treat it as a migration runway, not a destination — keep plugins/connectors clean so the move is mechanical.
- **Use AutoGen only for research/prototyping** — Python-heavy experimentation with novel multi-agent topologies. Don't ship it as your governed production runtime; port the validated pattern to MAF.
- **Reach for LangGraph when** you're genuinely multi-cloud or model-agnostic, your team lives in Python/JS, or you need its mature graph/checkpoint ergonomics *today* and can't tolerate MAF's churn. In a committed Azure shop, MAF Workflows covers the same stateful-graph + HITL ground with native identity/governance.
- **LangChain** is fine for a quick RAG prototype or spike — but don't anchor a governed enterprise agent platform on it; graduate the logic to MAF (or LangGraph if non-Azure).

One-liner for the interview: *"In a Microsoft enterprise, MAF is the default; SK is the legacy you migrate from; AutoGen is where you prototyped; LangGraph/LangChain are what you'd pick if you weren't committed to Azure."*

---

## Migration note: SK / AutoGen → MAF

MAF was deliberately designed so SK and AutoGen users **converge rather than rewrite from scratch**. Migration is **concept-preserving but API-breaking** — the topology and patterns survive; the wiring is rewritten.

**From Semantic Kernel:**
- **Plugins/connectors and content filters are the most reusable assets** — MAF tools are conceptually SK functions over `Microsoft.Extensions.AI`; tool design is unchanged.
- `Kernel` + services → an agent configured with a chat client; `FunctionChoiceBehavior` → MAF tool-choice settings; filters → MAF middleware; `AgentThread`/`ChatHistory` → MAF threads with serialization and memory tiers; vector data → the same `Microsoft.Extensions.VectorData`; SK agent orchestration → MAF orchestration + magentic.
- **Delete deprecated planner code** — replace with tool-calling loops or explicit Workflow graphs. DI registration patterns map cleanly.

**From AutoGen:**
- **Multi-agent patterns survive** — group chat, RoundRobin/Selector, handoff, and Magentic all exist in MAF, so the *topology* carries over; what changes is the API surface.
- AutoGen agents → MAF agents; `GroupChatManager` → MAF group-chat orchestration; Magentic-One → MAF magentic; code execution/tools → MAF tools + MCP.
- What you *gain* is the enterprise layer for free: Entra managed identity, durable HITL, OTel audit, content safety, Foundry hosting. Python AutoGen users can often stay in Python on `agent_framework`.

**Migration playbook:**
1. **Freeze the topology** — document agents, roles, tools, handoffs, termination conditions (framework-agnostic).
2. **Re-implement wiring in MAF**, agent by agent, keeping prompts/tools identical to isolate behavior changes.
3. **Port tools first** (nearly mechanical), then orchestration, then HITL.
4. **Re-attach observability** (OTel → App Insights) and **secrets/identity** (Entra MI, Key Vault) the MAF-native way.
5. **Golden-set regression test** — run the same tasks through old and new, diff outputs, then cut over.

**Risk management:** isolate MAF types behind a thin internal abstraction (your own `IAgentService`) so a breaking package rename doesn't ripple across the codebase. Pin package versions and upgrade deliberately.

Interview soundbite: *"Migrating to MAF is concept-preserving, API-breaking: the topology and patterns survive, the wiring is rewritten. Freeze the topology, port tools → orchestration → HITL, re-attach Azure observability/identity, and gate the cutover on a golden-set regression."*

---

## What interviewers probe

- *MAF vs. SK vs. AutoGen vs. LangGraph — when each?* MAF default for Azure/.NET; SK the legacy migration runway; AutoGen the prototype/research vehicle; LangGraph the non-Microsoft stateful-graph choice.
- *Why not just stay on SK or AutoGen?* Both fold into MAF; new investment goes to MAF, so it's the strategic, lower-risk choice.
- *Is the migration a rewrite?* No — concept-preserving, API-breaking. Reuse tools/connectors/patterns; drop deprecated bits (planners); add enterprise hardening MAF makes turnkey.
- *LangGraph vs. MAF Workflows?* Similar graph/checkpoint philosophy; MAF adds C# + native Azure identity/governance; LangGraph wins on ecosystem breadth and cloud-agnosticism.
- *How do you de-risk MAF's API churn?* Thin internal abstraction over MAF types, pinned versions, conceptual mastery + "verify exact names" caveats.
