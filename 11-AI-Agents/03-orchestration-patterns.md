# Orchestration Patterns

A single agent rarely solves a hard, multi-domain task well. MAF's orchestration layer (inherited and unified from AutoGen + SK Agents) lets you compose **specialist agents** instead of building one mega-agent — focused instructions and smaller tool surfaces per agent mean better accuracy, lower cost, easier testing, and least-privilege.

This file covers the five canonical patterns: **sequential, concurrent, group-chat, handoff, and magentic/manager.** The decision rule for all of them is the same: *match the pattern to the task's interaction structure, and prefer the simplest one that fits.*

> Always start with a **single agent** with good tools — most tasks need nothing more. Multiple agents add coordination overhead, latency, and cost. (verify current GA status / API names) on the builder/class names below.

---

## Sequential — fixed pipeline

Each agent's output feeds the next. Deterministic in ordering (you define the sequence) while each step is model-driven internally.

```text
[Researcher] --> [Writer] --> [Editor] --> result
```

```csharp
using Microsoft.Agents.AI.Workflows;   // (verify current GA status / API names)
var workflow = AgentWorkflowBuilder
    .CreateSequential(researcher, writer, editor)
    .Build();
var result = await workflow.RunAsync("Write a brief on Azure AI Foundry.");
```

**When to use:** the task naturally decomposes into ordered stages with clean handoffs (research → write → edit; extract → validate → summarize). You want predictable flow, focused per-agent responsibility, and inspectable intermediate outputs.

**Limitation:** rigid — no branching or iteration unless you add it. Suits linear processes, not dynamic ones.

---

## Concurrent — fan-out, then aggregate

The same input fans out to N agents in parallel, then results are aggregated.

```text
            +--> [Legal review] ---+
[Dispatch] -+--> [Security review] -+--> [Aggregate] --> result
            +--> [Cost review] ----+
```

```csharp
var workflow = AgentWorkflowBuilder
    .CreateConcurrent(legal, security, finance)
    .Build();
var results = await workflow.RunAsync("Assess this vendor contract.");
```

**When to use:** independent subtasks or "get N opinions" — multiple experts review a document simultaneously, several agents draft alternatives, or independent sources are queried at once. Because agents don't depend on each other, parallelism cuts latency.

**Considerations:** cost multiplies with the number of agents; you need a clear **aggregation strategy** (another agent, voting, or code) to merge/reconcile outputs, which may conflict.

---

## Group chat — collaborative shared transcript

A **manager** invites agents into a shared conversation; a **selection strategy** picks who speaks next; a **termination strategy** ends it. Agents build on each other's messages, debate, and refine toward a goal — emergent collaboration, mirroring a team discussion.

```text
        +--------- Group Chat (shared transcript) ---------+
        |  Manager picks next speaker each turn            |
        |  [PM] <-> [Engineer] <-> [QA]  ... until DONE     |
        +--------------------------------------------------+
```

```python
# Group chat with selection + termination  (verify current GA status / API names)
from agent_framework import GroupChatBuilder
chat = (GroupChatBuilder()
        .participants([pm, engineer, qa])
        .manager(manager_agent)        # round-robin, rule-based, or model-driven
        .max_turns(12)                 # termination guard — mandatory
        .build())
result = await chat.run("Design the rollout plan for the new feature.")
```

**When to use:** open-ended, iterative problem-solving where agents must critique each other and the path isn't predetermined (coder ↔ reviewer until approved).

**Critical:** termination strategy is non-negotiable — max turns, an explicit "DONE"/"APPROVED" sentinel, or a judge agent — or the chat loops forever burning tokens. This is the classic production bug.

---

## Handoff — routing / triage

An agent decides to transfer control (and context) to a more suitable agent. The handoff is usually realized as a tool the model calls (`transfer_to_billing`), so routing is model-driven and dynamic. Supports bidirectional handoff (a specialist hands back) and chains of specialists.

```text
[Triage] --billing?--> [Billing agent]
   |  \--technical?--> [Tech agent]
   |   \--refund?----> [Refund agent (HITL)]
```

```python
workflow = (HandoffBuilder()
    .start_with(triage)
    .add_handoff(triage, [billing_agent, tech_agent])
    .build())
```

**When to use:** the right expert depends on the input (customer-service-style triage → specialists). Each agent stays focused and simple; routing adapts to the actual request rather than a fixed pipeline.

**Considerations:** define clear handoff criteria in instructions, preserve context across the handoff, and guard against ping-ponging between agents.

---

## Magentic / manager — adaptive planning for open-ended tasks

A central **Magentic manager** (from AutoGen's Magentic-One) maintains a **task ledger** and a **progress ledger**: it decomposes the goal, dynamically assigns subtasks to the most capable specialist, observes results, tracks progress, and **re-plans when stuck** — including detecting loops and reassigning work. This is model-driven planning at the *orchestration* level.

```text
        +-------------------- Magentic Manager --------------------+
        | Task ledger + plan + progress tracker; re-plans on stall |
        +----------------------------------------------------------+
            |            |                |               |
        [WebSurfer]  [Coder]        [FileSurfer]     [Terminal]
```

```python
workflow = (MagenticBuilder()
    .participants(web_surfer=surfer, coder=coder, file_agent=files)
    .with_manager(orchestrator_model)
    .build())
```

**When to use:** complex, open-ended tasks where the steps aren't known upfront and require adaptive coordination across heterogeneous capabilities (research/automation).

**Considerations:** the most autonomous and costly end of the spectrum, and hardest to make deterministic — add iteration/cost budgets, observability, and HITL for risky actions. What it tracks that a plain group chat doesn't: an explicit task ledger + progress state, enabling re-planning.

---

## Picking a pattern

| Pattern | Interaction structure | Use when |
|---|---|---|
| **Single agent** | One reasoning loop | Most cases — start here |
| **Sequential** | Ordered pipeline | Known stages with clean handoffs |
| **Concurrent** | Independent parallel | Independent subtasks or N opinions to aggregate |
| **Handoff** | Conditional routing | Right expert depends on the input (triage) |
| **Group chat** | Iterative collaboration | Open-ended, agents critique/build on each other |
| **Magentic** | Adaptive planning | Open-ended, steps unknown, heterogeneous agents |
| **Deterministic workflow** | Engineered graph | Guaranteed ordering, approvals, durability, audit |

Decision factors: is the path **known (deterministic)** or **emergent (model-driven)**? Are steps **ordered, parallel, or conditional**? How much autonomy is safe? For production systems needing both governance and adaptability, wrap model-driven orchestration inside a **deterministic workflow** (section 01) — control where you need it, flexibility inside the steps.

---

## What interviewers probe

- *Match pattern to problem.* Sequential = fixed pipeline; concurrent = independent parallel reviews; group chat = collaborative debate/refinement; handoff = routing/triage; magentic = open-ended, multi-tool, re-planning tasks.
- *How do you stop a group chat looping forever / running up cost?* Termination strategy: max turns, a sentinel, or a judge agent.
- *Why decompose into specialists instead of one big prompt?* Focused instructions, smaller per-agent tool surfaces → better accuracy, cheaper, more testable, least-privilege.
- *What does the Magentic manager track that a plain group chat doesn't?* An explicit task ledger + progress state, enabling re-planning when stuck.
- *RoundRobin vs. model-driven (Selector) speaker selection?* RoundRobin is deterministic, cheap, reproducible (known pipelines); model-driven adapts routing to conversation state at the cost of an extra LLM call and non-determinism.
