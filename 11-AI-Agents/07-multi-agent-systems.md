# Multi-Agent Systems

A multi-agent system splits a problem across specialized agents — each with focused instructions and a narrow tool set — coordinated by an orchestration pattern. The orchestration *mechanics* are in section 03; this file is about **designing reliable, governed multi-agent systems**: the team patterns that recur in enterprise work, and the engineering discipline that makes autonomous systems shippable.

> The first design decision is always: **do you need multiple agents at all?** Start with a single agent plus good tools — it's easier to build, debug, and operate. Multiple agents add coordination overhead, latency, cost, and compounding errors. Reach for them only when decomposition demonstrably pays off.

---

## When multiple agents earn their cost

Use multiple agents when:

- The task spans **distinct skill domains** better served by specialized instructions/tools (research vs. coding vs. compliance).
- You want **separation for governance** — a reviewer agent independent of the doer agent, so the critic isn't the author.
- The work is **naturally parallel** (concurrent fan-out).
- **Routing to experts** improves quality (handoff to specialists).

Don't use them when: a single well-prompted call (with tools) solves it; the workflow is fully deterministic and better as plain code/a state machine; latency is critical (each turn is another round-trip); or you can't bound/observe the system enough to make it safe. Many "agent" problems are really **RAG + one good prompt** or a single function-calling loop.

---

## The canonical enterprise team patterns

### Planner / executor

Splits "decide what to do" from "do it." A **planner** (a strong reasoning model) decomposes the goal into an ordered plan; an **executor** (or several specialist tool agents) carries out each step, returning results the planner uses to adapt. Have the planner emit a **typed list of steps** (structured output) so dispatch is deterministic. Add a verification/critic step and strong termination to prevent drift.

```text
[Planner] --emits typed steps--> [Dispatcher]
                                    |  step 1 --> [Specialist A]
                                    |  step 2 --> [Specialist B]
                                    |  step 3 --> [Code executor (sandboxed)]
                                    v
                               [Planner re-checks progress] --> done / re-plan
```

Shines for complex multi-step automation ("onboard a new vendor: validate → create records → notify") because it separates high-level reasoning from reliable, auditable execution.

### Research team (research → synthesis → fact-check)

A retriever/searcher gathers evidence (agentic RAG over Azure AI Search), a synthesizer drafts, a fact-checker validates claims against sources. Often packaged as a reusable sub-team (Society-of-Mind / nested team) so only the final, grounded answer surfaces to the parent.

### Reflection / critic (evaluator-optimizer)

A generator produces a draft; a critic evaluates against explicit criteria and returns actionable feedback; the generator revises — looping until the critic approves or a max-round cap fires. The critic should have a sharp prompt and, ideally, **tools to verify claims** (run tests, query data) rather than just opining. Markedly improves coding/reasoning/writing quality at the cost of extra turns — so bound the loop.

### Support triage (handoff + escalation)

A triage agent classifies the request and hands off to specialists (knowledge/RAG, account/billing with scoped tools, human-escalation). Sensitive actions (refunds, cancellations) are gated behind HITL; all factual answers grounded in retrieval with citations; security trimming so users see only their own data. This mirrors the OpenAI "Swarm" handoff pattern and converts cleanly to MAF.

```text
                    +--> [Knowledge agent] (RAG + citations)
[User] --> [Triage] +--> [Account agent]  (scoped tools, OBO auth)
                    +--> [Human escalation] (HITL gate on refunds)
```

---

## Designing reliable multi-agent systems

The engineering discipline that separates a demo from production:

| Concern | Practice |
|---|---|
| **Single responsibility** | Each agent has one clear job and a minimal tool set (least privilege). |
| **Termination** | Crisp conditions on every team — max turns/rounds, sentinels, judge agents — or it loops and burns tokens. |
| **Explicit shared state** | Coordinate via threads; key state by tenant+user; never share across tenants. |
| **Structured messages** | Pass typed data between agents (one agent's output is the next's typed input), not free text. |
| **Per-agent observability** | OpenTelemetry spans per agent/handoff so you can localize where collaboration broke down. |
| **Deterministic skeleton** | Wrap model-driven agents in a deterministic workflow for ordering, approvals, and compliance. |
| **Cost control** | Right-size models per agent, cap iterations, manage context, minimize agent count. |
| **Resilience** | Tool errors return structured messages (don't crash the run); loop detection; checkpoint for resume. |

---

## Common failure modes (and mitigations)

Agentic systems fail in *novel* ways. Name them and the mitigation:

- **Wrong tool selection / malformed args** → precise tool metadata, structured outputs, input validation in middleware.
- **Infinite/looping behavior** → iteration and round budgets, loop detection, circuit breakers.
- **Hallucination / ungrounded answers** → RAG grounding with citations, groundedness evaluation.
- **Prompt injection (direct/indirect)** → Prompt Shields, output scanning, least-privilege tools, HITL so injection can't trigger destructive actions (section 08).
- **Runaway cost** → token caps, model routing, caching, agent minimization, cost monitoring.
- **Cascading multi-agent errors** → clear responsibilities, termination conditions, per-agent observability.
- **State loss** → durable thread serialization / checkpointing (section 04).
- **Non-determinism / regressions** → evaluation harnesses in CI, online monitoring.

The overarching message: design **bounded, observable, governed, evaluated** systems rather than trusting the model to behave.

---

## Evaluating multi-agent orchestration

Beyond single-agent metrics, evaluate the **collaboration and trajectory**, not just final answers:

- **Task success** — did the team achieve the goal? (labeled benchmark of representative tasks)
- **Efficiency** — turns/handoffs, total tokens/cost (detect chatter and loops).
- **Routing accuracy** — in handoff, did triage pick the right specialist?
- **Contribution quality** per agent — is each adding value or noise?
- **Coordination correctness** — did sequential steps pass the right data; did group chat terminate appropriately?
- **Robustness** — does it recover from a bad intermediate result or a tool failure mid-run?

Combine deterministic checks with LLM-as-judge for subjective quality, and inspect **OpenTelemetry traces** (spans per agent/handoff) to diagnose where collaboration breaks. Wire evaluations into CI as quality gates; complement with online monitoring (success rates, cost per task). Azure AI Foundry evaluations increasingly support agent-trajectory evaluators (intent resolution, tool-call accuracy, task adherence). Multi-agent failures hide in coordination — evaluate the system behavior, not just outputs.

---

## What interviewers probe

- *Single agent vs. multiple — how do you decide?* Start single; justify each agent by a concrete need (distinct domains, governance separation, parallelism, expert routing). Don't over-engineer.
- *Describe a multi-agent workflow you'd build.* Name the topology and justify it: reflection for quality, planner/executor for complex automation, handoff for routing, deterministic workflow when auditors need a replayable path.
- *Common failure modes?* Runaway loops, cost blowups, wrong tool selection, hallucination, prompt injection, state loss — each with a concrete mitigation.
- *How do you evaluate a multi-agent system?* Task success + efficiency + routing accuracy + coordination + robustness, via trace inspection and CI gates — not exact-string assertions.
- *When NOT to use multi-agent?* Single prompt/RAG suffices, deterministic workflow is better, latency-critical, or you can't bound/observe it safely.
