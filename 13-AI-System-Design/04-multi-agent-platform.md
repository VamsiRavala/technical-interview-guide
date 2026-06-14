# Multi-Agent Platform

Beyond single Q&A, the business wants **agents** that *do things*: a procurement agent that drafts POs, a support agent that reads tickets, calls APIs, and escalates. Agents plan, call tools, maintain state across turns, and sometimes hand off to humans. They are powerful and dangerous because they convert a single request into an **unbounded sequence of model + tool calls** — so the architecture is deterministic guardrails wrapped around a non-deterministic core.

## (a) Requirements

**Functional**
- **Orchestrator** that runs multi-step plans (sequential, parallel, group-chat, or graph/handoff).
- **Agent registry** (what agents exist, capabilities) and **tool/skill registry** (schemas, permissions, risk tier).
- Durable **state/memory** (conversation + task state surviving restarts).
- **Human-in-the-loop (HITL)** approval gates for risky actions.
- Guardrails, budgets (max steps/tokens/cost per task), and full tracing.

**Non-functional**

| NFR | Target |
|---|---|
| Task durability | A task survives worker crash; resumes from last checkpoint |
| Step budget | Hard cap (e.g., 15 tool calls) to prevent runaway loops |
| HITL SLA | Approval requests delivered < 5 s; task parks durably until decision |
| Auditability | Every plan step, tool call, and decision logged for replay |
| Isolation | Tool execution sandboxed; least-privilege per tool |

## (b) Architecture diagram

```text
  User / Event ─▶ AI Gateway (APIM) ─▶ ORCHESTRATOR (Container Apps / AKS)
                                            │
        ┌───────────────────────────────────┼──────────────────────────────────┐
        │                                   │                                   │
        ▼                                   ▼                                   ▼
  AGENT REGISTRY                    PLANNER (LLM: GPT-4o)               TOOL/SKILL REGISTRY
  (Cosmos DB: agent defs,           "decide next step"                 (Cosmos DB: tool schema,
   capabilities, model,             ▲        │                          auth scope, risk tier)
   guardrails)                      │        ▼
                                    │   TOOL EXECUTOR ──▶ tools: REST APIs, SQL,
   STATE / MEMORY                   │     (sandbox)        Azure Functions, MCP servers
   (Cosmos DB: task state,          │        │
    checkpoints; Redis: hot         └────────┤  risky action?
    session; Service Bus:                    ▼
    durable task queue)              HITL GATE ──▶ Teams/email approval card
                                       │  (task parked durably until human acts)
                                       ▼
                                 GUARDRAILS: Content Safety, budget caps,
                                 step limit, output validation ──▶ App Insights (full trace)
```

## (c) Component-by-component walkthrough

- **Orchestrator runtime**: hosted on **Container Apps** or **AKS**. The Microsoft-native frameworks are **Semantic Kernel** and the **Microsoft Agent Framework / Azure AI Foundry Agent Service**. Patterns: sequential, concurrent, group-chat, and **handoff** (one agent delegates to a specialist).
- **Planner**: an LLM call (GPT-4o / reasoning model) that, given goal + state + available tools, decides the next action. Wrapped in a loop with a **hard step budget**. Separate cheaper "planner" calls from "executor" calls, and re-ground tool outputs before trusting them, so the agent doesn't act on hallucinated intermediate results.
- **Agent registry** (Cosmos DB): each agent's system prompt, allowed tools, model, and guardrails — so agents are config/data, not redeploys.
- **Tool/skill registry** (Cosmos DB): each tool's **JSON schema**, the **auth scope/Managed Identity** it runs as, and a **risk tier** (read-only vs writes vs irreversible) that drives whether HITL is required. Consider **MCP servers** as a standard tool interface.
- **State/memory**: **durable task state + checkpoints in Cosmos DB**, hot session in **Redis**, and a **Service Bus** queue for durable task hand-off so a crashed worker doesn't lose the task (resume from checkpoint). This is where your saga/idempotency background pays off.
- **HITL gate**: when the planner selects a high-risk tool, the orchestrator emits an approval (Teams Adaptive Card / email), **parks the task durably** (Service Bus deferred message or Durable Functions `wait-for-external-event`), and resumes on the human's decision.
- **Guardrails & budgets**: Content Safety on agent outputs, max-steps/max-tokens/max-cost per task, schema validation of tool inputs/outputs.

## (d) Scalability strategies

- **Stateless orchestrator + externalized state** → scale workers horizontally; any worker resumes any task from its checkpoint.
- **Service Bus** queues per agent type for **independent scaling** and backpressure; KEDA-scale workers on queue depth.
- **Parallel tool calls** (fan-out/fan-in) where the plan allows, to cut wall-clock latency.
- **Model tiering**: cheap model for routing/simple steps, frontier model only for hard reasoning steps.

## (e) Security patterns

- **Least privilege per tool**: each tool runs as its own Managed Identity with only the scopes it needs; the agent never holds broad credentials. For user-scoped actions, execute via **OBO** so the tool runs as the *user*, not the over-privileged app.
- **HITL on irreversible/high-blast-radius actions** (payments, deletes, external comms), with authorization on the *approver* too.
- **Sandbox tool execution** (separate Container App / restricted network egress); validate and constrain tool inputs to prevent injection-driven misuse.
- **Prompt-injection defense**: treat retrieved/tool content as untrusted; clearly delimit and label it; never let it escalate the agent's tool permissions or auto-trigger an irreversible action. Assume injection *will* sometimes succeed and limit blast radius by constraining what the model is *empowered* to do.

## (f) Trade-offs

| Decision | Option A | Option B | Guidance |
|---|---|---|---|
| **Single powerful agent vs multi-agent** | One agent + many tools: simpler, fewer hops, cheaper | Many specialists + orchestrator: modular, better at complex/long tasks, harder to debug | Start single-agent; split into multi-agent only when the task clearly decomposes into distinct skills or the prompt becomes unmanageable. Complexity has real cost. |
| **Autonomy vs control (HITL)** | More autonomy: faster, less human cost | More HITL gates: safer, auditable, slower | Gate by **risk tier**: read-only fully autonomous; writes/irreversible require approval. Don't gate everything (defeats the point) or nothing (unsafe). |
| **Durable orchestration vs in-memory** | Durable (Service Bus/Durable Functions/checkpoints): survives crashes, resumable, auditable | In-memory loop: simple, fast, lost on crash | Durable for anything multi-step, long-running, or money-touching; in-memory only for short, idempotent, fire-and-forget tasks. |
| **Explicit orchestration vs group-chat** | Sequential/handoff: deterministic, auditable, predictable cost | Group-chat: flexible, emergent, unpredictable cost/behavior | Explicit for production workflows where auditability and cost predictability matter; group-chat for open-ended exploration with hard budget caps. |

> **Phrase that lands:** "I gate this tool with HITL because it's an irreversible action; read-only tools run autonomously — and a hard step + token budget in the orchestrator, not the prompt, is the backstop against runaway loops."
