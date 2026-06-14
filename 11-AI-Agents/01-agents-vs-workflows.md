# Agents vs. Workflows

The single most important judgement call in agentic engineering is *not* which framework to use — it is whether you need an agent at all. Most "agent" requirements are well-served by a deterministic workflow plus one or two tools. The senior instinct, echoed across Anthropic's and Microsoft's guidance, is: **find the simplest thing that works, and add autonomy only when the task demonstrably demands it.**

> Note: MAF (Microsoft Agent Framework) is the unification of Semantic Kernel and AutoGen. SK's deep API surface (Kernel, plugins, filters, RAG) lives in section 10; this section is about the *agent* abstraction itself.

---

## What an agent actually is

An **agent** gives the LLM control of the loop: the model decides which tools to call, in what order, and when the task is done — dynamically, at runtime, based on what it observes. Contrast that with a **workflow**, a predefined sequence of steps you orchestrate in code (LLM calls, tool calls, branches) where *you* own the control flow.

| | **Workflow** | **Agent** |
|---|---|---|
| Control flow | You define it in code | The model decides at runtime |
| Predictability | High — testable, reproducible | Lower — emergent, can loop or drift |
| Cost / latency | Lower (bounded calls) | Higher (many model round-trips) |
| Debuggability | Easy (deterministic) | Harder (non-deterministic trajectory) |
| Best for | Known, stable processes | Open-ended, variable-path tasks |

The practical rule: if you can enumerate the steps, build a workflow. Reach for an agent when the solution path varies per input, requires dynamic tool selection, or involves open-ended exploration (research, autonomous debugging, complex troubleshooting). Don't build an autonomous agent to do a job a `switch` statement could.

---

## The agentic loop

An agent runs a **reason → act → observe** loop until it reaches a final answer or hits a guard:

```text
        ┌──────────────────────────────────────────┐
        │                                          │
        ▼                                          │
   [Reason]  the model thinks about what to do     │
        │                                          │
        ▼                                          │
   [Act]     emits a tool call (name + JSON args)  │
        │                                          │
        ▼                                          │
   [Observe] your code executes the tool, returns  │
        │     the result into the conversation     │
        │                                          │
        └──── not done? loop ──────────────────────┘
        │
        ▼
   [Final answer]   (or: max-iteration / cost / time guard fires)
```

Crucially, **the model is the planner; your code is the executor and the security boundary.** The model emits a *request* to call a tool — it never executes anything itself. This separation is what makes the loop governable.

```csharp
// MAF: give the agent tools and let the loop plan implicitly.  (verify current GA status / API names)
AIAgent planner = new ChatClientAgent(chatClient, new ChatClientAgentOptions
{
    Name = "trip-planner",
    Instructions = "Plan a trip. Use the tools to check weather, flights, and budget before answering.",
    ChatOptions = new ChatOptions
    {
        Tools = [ AIFunctionFactory.Create(GetWeather),
                  AIFunctionFactory.Create(SearchFlights),
                  AIFunctionFactory.Create(CheckBudget) ]
    }
});
// The model emits a plan as a sequence of tool calls; the runtime executes the loop.
var plan = await planner.RunAsync("Plan a 3-day Seattle trip under $1500.", planner.GetNewThread());
```

### Capping the loop

A loop that can run forever is a production incident waiting to happen. Always bound it: a max-iteration / max-tool-call cap, a token/cost budget, and a timeout. This is the difference between "an agent" and "a runaway bill."

---

## ReAct: the pattern underneath

**ReAct (Reason + Act)** interleaves reasoning and tool use: the model produces a *Thought* (what to do), an *Action* (a tool call), then observes the *Observation* (the result), and reasons again — repeating until a final answer. This grounds reasoning in real data at each step and makes the process inspectable.

Modern tool-calling APIs implement ReAct *implicitly* — the model's decision to call a tool is the "act," and reasoning models do the "reason" internally. You rarely hand-write ReAct prompts anymore, but the mental model still explains why you log every thought/action/observation: that trace is your debugging and audit surface.

---

## When is an agent justified?

Ask, in order:

1. **Is the path predictable?** If you can enumerate the steps → workflow. Cheaper, faster, reliable.
2. **Does the solution path vary per input?** Dynamic tool selection, open-ended exploration → agent earns its cost.
3. **What's the tolerance for latency and cost?** Agents make many LLM calls; each turn is a round-trip.
4. **What's the risk of irreversible actions?** Higher risk → more deterministic control, more human-in-the-loop.
5. **Do you need auditability?** Deterministic workflows are inherently more auditable.

Many "agent" problems are really **retrieval + one good prompt** (RAG) or a **function-calling loop with a single agent**. Multi-agent systems multiply cost, latency, and non-determinism — reserve them for problems whose decomposition genuinely pays off (covered in section 07).

---

## The hybrid answer (what interviewers want)

The mature architecture is rarely "pure workflow" or "pure agent." It is a **deterministic workflow skeleton** — encoding required steps, approval gates, and safety checks for control and compliance — with **model-driven agents embedded inside individual steps** for flexible reasoning. For example: a deterministic workflow enforces "always validate → approve → execute" ordering, while each step's agent uses tool-calling freely within that step.

In MAF this maps directly: plain **agents** for a single reasoning loop with tools; the **Workflows** layer (graph of executors, conditional edges, checkpoints, HITL nodes) when you need guaranteed ordering, branching, durability, or auditability. You architect the *boundaries* of autonomy proportional to risk — autonomy is not all-or-nothing.

```text
   Deterministic workflow (control)        Model-driven agent (flexibility)
   ──────────────────────────────          ────────────────────────────────
   [Intake] → [Validate] → {ok?}            Inside [Resolve]:
                 │  yes                       agent loops: reason → call tools
                 ▼                            → observe → answer, picking the
            [Resolve(agent)] → [Approve]      path the input demands
                 │  no
                 ▼
            [Reject] → [Audit]
```

---

## What interviewers probe

- *Workflow vs. agent — when each?* Workflow for predictable paths (cheaper, testable, reliable); agent only when the path is emergent and model-driven decisions add real value. Start simple, add autonomy when demanded.
- *What is the agentic loop?* Reason → act → observe, repeating until done; the model proposes tool calls, your code executes them and is the security boundary.
- *How do you stop a runaway agent?* Max-iteration / max-tool-call caps, token/cost budgets, timeouts, loop detection.
- *Is ReAct still relevant?* Conceptually yes; it explains the loop. Practically it's implicit in modern tool-calling APIs, but you still log thought/action/observation for observability.
- *Do you default to multi-agent?* No — "don't build an autonomous agent to do a job a switch statement could." Many tasks are RAG + one prompt.
