# AI Agents — From a Single Reasoning Loop to Governed Multi-Agent Systems

> How to build production AI agents on the Microsoft stack: the Microsoft Agent Framework (MAF, the unification of Semantic Kernel + AutoGen), the orchestration patterns (sequential, concurrent, group-chat, handoff, magentic), memory and durable state, human-in-the-loop, security/governance, and AutoGen as the conceptual ancestor. C#-first for MAF, Python for AutoGen. Currency: **mid-2026** — MAF's surface is fast-moving, so API/type names are flagged **(verify current GA status / API names)** where they shift.

---

## 📚 Table of Contents

### Foundations

1. **[Agents vs. Workflows](01-agents-vs-workflows.md)** - What an agent actually is, the agentic loop (reason → act → observe), ReAct, and the senior instinct: prefer a deterministic workflow and add autonomy only when the task demands it
2. **[Microsoft Agent Framework](02-microsoft-agent-framework.md)** - The SK + AutoGen unification; agent lifecycle, threads/state and serialization, agent↔agent communication, tool/function calling, and MCP

### Orchestration & State

3. **[Orchestration Patterns](03-orchestration-patterns.md)** - Sequential, concurrent, group-chat, handoff, and magentic/manager — each with an ASCII diagram and a "when to use"
4. **[Memory & State](04-memory-and-state.md)** - The three memory tiers (short-term, long-term, vector), thread persistence, and history reduction
5. **[Human-in-the-Loop](05-human-in-the-loop.md)** - Approval workflows, durable suspend/resume, and approval gates for high-stakes actions

### Multi-Agent in Practice

6. **[AutoGen](06-autogen.md)** - AssistantAgent/UserProxyAgent, RoundRobin/Selector group chat, tool use and code-execution sandboxing, the 0.2 → 0.4 event-driven actor rewrite, and AutoGen Studio
7. **[Multi-Agent Systems](07-multi-agent-systems.md)** - Planner/research/execution/approval team patterns and how to design reliable, observable multi-agent systems

### Production

8. **[Security & Governance](08-security-and-governance.md)** - Entra managed identity, least-privilege tools, prompt-injection / Prompt Shields, content safety, audit, and OpenTelemetry
9. **[Framework Comparison](09-framework-comparison.md)** - MAF vs. SK Agents vs. AutoGen vs. LangGraph vs. LangChain across language/model/orchestration/state/tooling/enterprise-fit/maturity, plus "when to pick which" and the SK/AutoGen → MAF migration note

### Interview Preparation

10. **[Interview Questions](10-interview-questions.md)** - 100+ detailed Q&A: Microsoft Agent Framework (50), AutoGen (50), and the General Agents category

---

## 🎯 Learning Path

### Beginner (Week 1-2)
1. Read **Agents vs. Workflows** and be able to defend "use a workflow unless the path is genuinely emergent" out loud
2. Work through **Microsoft Agent Framework** — build a single `ChatClientAgent`, run a turn on an `AgentThread`, and add one tool with `AIFunctionFactory.Create`
3. Internalize the framing sentence: *MAF is the supported successor to both SK Agents and AutoGen — SK's enterprise rigor plus AutoGen's multi-agent orchestration, on Azure AI Foundry*

### Intermediate (Week 3-4)
1. Learn **Orchestration Patterns** — match each of sequential / concurrent / group-chat / handoff / magentic to a concrete problem and sketch its diagram from memory
2. Build **Memory & State** then **Human-in-the-Loop** — serialize a thread to durable storage, rehydrate it, and add an approval gate on a destructive tool
3. Study **AutoGen** — the 0.2 → 0.4 actor rewrite, RoundRobin vs. Selector, and Docker-sandboxed code execution; map every concept back to MAF

### Advanced (Week 5-6)
1. Master **Multi-Agent Systems** and **Security & Governance** — design a triage→specialist team with least-privilege tools, Prompt Shields, OTel audit, and durable HITL
2. Internalize **Framework Comparison** — rehearse the "MAF is the default; SK is the legacy you migrate from; AutoGen is where you prototyped; LangGraph/LangChain are the non-Azure picks" answer
3. Drill **Interview Questions** and practice the verification caveat: solid conceptual answers + "verify exact API names against current docs" signals a senior who adopts a moving framework responsibly

---

## 🔑 Mental Model at a Glance

| Layer | What it is | MAF type(s) **(verify current GA status / API names)** |
|---|---|---|
| **Model client** | Provider-neutral chat/embedding access | `IChatClient`, `IEmbeddingGenerator` (`Microsoft.Extensions.AI`) |
| **Agent** | Stateless, reusable config: instructions + model + tools | `AIAgent` / `ChatClientAgent` |
| **Thread** | Serializable per-conversation state | `AgentThread` |
| **Tools** | Functions the model can call | `AIFunction` / `AIFunctionFactory`, MCP, Foundry hosted tools |
| **Orchestration** | Coordinates multiple agents | Sequential, concurrent, group-chat, handoff, magentic |
| **Workflow** | Explicit, deterministic graph of steps | `Microsoft.Agents.AI.Workflows` (nodes/edges, checkpoints, HITL) |
| **Memory** | Conversation / working / long-term tiers | Thread + memory providers + `Microsoft.Extensions.VectorData` |

---

## 💡 Key Decisions

| Question | Short answer |
|---|---|
| **Agent or workflow?** | Start with the simplest thing. Workflow when the path is known/auditable; agent when the path is emergent. Combine both: deterministic skeleton, model-driven agents inside steps. |
| **Where does state live?** | The **thread**, not the agent. Agent is a stateless singleton; thread carries serializable conversation state — never share a thread across users. |
| **Which orchestration pattern?** | Sequential = pipeline; concurrent = independent parallel; handoff = routing/triage; group chat = collaborative iteration; magentic = open-ended, re-planning tasks. |
| **Auth to Azure?** | Entra managed identity via `DefaultAzureCredential`, least-privilege RBAC. No API keys in production. Flow the end-user identity (on-behalf-of) for data access. |
| **How do tools stay safe?** | Tools run with *your service's* privileges — least-privilege per agent, validate model-supplied args, gate destructive actions behind HITL, sandbox code execution. |
| **New multi-agent system?** | Target **MAF**. SK is the migration runway; AutoGen is the prototype/teaching vehicle; LangGraph/LangChain are the non-Azure choices. |

> **Where the rest lives:** deep Semantic Kernel API content (Kernel, plugins, filters, RAG-with-SK) is section **10**; Azure OpenAI / Azure AI Search / Foundry infrastructure is the Azure AI section; end-to-end RAG architecture is the RAG section. This section keeps the *agent* abstraction and multi-agent orchestration front and center.
