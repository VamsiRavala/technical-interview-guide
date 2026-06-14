# Function Calling

Function calling is how the model decides to invoke your plugins. In SK you control it via **`FunctionChoiceBehavior`** on the execution settings. This is the modern, GA mechanism that **replaced both the old `ToolCallBehavior` API and, for most scenarios, planners** (see "Planners are superseded" below).

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var chat = kernel.GetRequiredService<IChatCompletionService>();

var settings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),   // model chooses; SK runs the loop
    Temperature = 0.0,                                        // determinism for tool selection
};

var history = new ChatHistory("You are a concise assistant.");
history.AddUserMessage("What's the temperature in Seattle, and is that warmer than London?");

ChatMessageContent result = await chat.GetChatMessageContentAsync(history, settings, kernel);
Console.WriteLine(result.Content);
```

> Note: passing the `Kernel` into the chat service is what enables function calling at this layer — the service uses the kernel's plugins when a `FunctionChoiceBehavior` is set.

---

## The tool-call loop

Under the hood, `Auto()` runs the loop for you:

```text
  model returns tool calls
        │
        ▼
  SK invokes matching KernelFunctions
        │
        ▼
  SK appends FunctionResultContent to history
        │
        ▼
  SK re-calls the model
        │
        └──► repeat until the model returns a plain message
             (bounded by a max-iterations guard)
```

You never write the round-trip plumbing; SK serializes all advertised functions as tools, sends them with the prompt, executes the model's chosen calls, appends results, and re-invokes — multiple iterations, fully managed.

---

## `Auto` vs `Required` vs `None`

These three modes control how the model is allowed to use tools.

```csharp
FunctionChoiceBehavior.Auto();                            // model freely decides (assistants)
FunctionChoiceBehavior.Auto(functions: [weatherFn]);      // scoped to a subset
FunctionChoiceBehavior.Required(functions: [extractFn]);  // MUST call (structured extraction)
FunctionChoiceBehavior.None();                            // advertise but forbid calling (dry-run)
```

| Mode | Behavior | Use case |
|---|---|---|
| `Auto()` | Model decides whether and which functions to call; SK runs the loop | Assistants, agentic behavior (default) |
| `Required()` | Forces the model to call at least one (optionally specific) function on the next turn | Deterministic data extraction, "must use a tool" pipelines |
| `None()` | Advertises functions for context but forbids calling them | Dry-runs, previews, explanation/confirmation UX |

Each accepts options such as `autoInvoke: false` — return the tool-call *request* without executing it, so you handle execution yourself (the basis for approval gates and manual mode).

```python
# Python equivalent
from semantic_kernel.connectors.ai.function_choice_behavior import FunctionChoiceBehavior
from semantic_kernel.connectors.ai.open_ai import OpenAIChatPromptExecutionSettings

settings = OpenAIChatPromptExecutionSettings(
    function_choice_behavior=FunctionChoiceBehavior.Auto(),
    temperature=0.0,
)
result = await chat.get_chat_message_content(history, settings=settings, kernel=kernel)
```

---

## Scoping advertised functions

Every advertised tool consumes prompt tokens for its schema, and a large catalog degrades selection accuracy. Advertise only what's relevant to the turn:

```csharp
FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(
    functions: [kernel.Plugins["Weather"]["get_current_temperature"]]);
```

---

## Manual vs automatic function calling

With `Auto()`, SK executes the loop for you. In **manual mode** you set `autoInvoke: false`, inspect the returned `FunctionCallContent` yourself, validate arguments, and invoke functions explicitly — needed when you must:

- validate or sanitize arguments before execution,
- require **human approval** for consequential actions,
- audit every call, or
- run tools out-of-process / sandboxed (untrusted tools).

The senior signal is knowing *why* you'd go manual, not just that you can. For most flows you stay automatic and gate risky tools with an `IAutoFunctionInvocationFilter` (see [07-filters-and-middleware.md](07-filters-and-middleware.md)).

---

## Bounding the loop — production safety

Auto-invocation can loop: the model can ping-pong tool calls and burn tokens/latency, or get stuck repeating an identical call. Controls:

- **Cap iterations.** `FunctionChoiceBehavior.Auto` options expose a maximum number of auto-invoke attempts (verify the exact option name for your SK version; historically ~5 by default depending on connector).
- **Terminate early** from an `IAutoFunctionInvocationFilter` by setting `context.Terminate = true` once a goal is met, after N calls, or when you detect a repeated identical call.
- **Set `Temperature = 0`** for tool-selection determinism.
- **Make functions idempotent** or guard side effects.
- **Scope the advertised function set** so the model has fewer tools to chain.
- **Log every tool call** (a filter is the natural home).

```csharp
public sealed class IterationGuardFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext ctx, Func<AutoFunctionInvocationContext, Task> next)
    {
        await next(ctx);
        if (ctx.RequestSequenceIndex >= 5)   // hard stop after N model turns
            ctx.Terminate = true;
    }
}
```

---

## Function calling + streaming

Automatic function calling works alongside streaming: SK still resolves tool calls server-side before streaming the final answer, so design your UI for the "thinking / tool" phase, then the token stream resumes after tools run (see [10-dotnet-ai-integration.md](10-dotnet-ai-integration.md) for emitting tool-call events on the same channel).

---

## Planners are superseded

This is a **trap question** that separates people who read 2023 tutorials from people who track the framework. SK's original orchestration was **planners** (`SequentialPlanner`, `ActionPlanner`, `StepwisePlanner`, later Handlebars and Function-Calling Stepwise planners): given a goal and a function catalog, the LLM composed an executable plan.

Modern models do native tool calling well enough that **automatic function calling + agents** cover the vast majority of what planners did — with less brittleness, fewer prompt-injection surfaces, and no fragile plan-DSL parsing. Three reasons planners lost:

1. **Reliability** — native function calling produces structured tool calls directly; no separate plan-generation step to malform or hallucinate.
2. **Latency/cost** — planners required an upfront LLM call to synthesize the plan, then more to execute; auto function calling interleaves reasoning and action.
3. **Adaptability** — a precomputed plan is brittle; turn-by-turn calling lets the model observe each result and adjust.

| | Planners (legacy) | Automatic function calling | Agents (current) |
|---|---|---|---|
| Reasoning | LLM emits a plan up front | LLM calls tools step-by-step | LLM + tools + memory + multi-turn |
| Brittleness | High (plan parsing) | Low | Low |
| Inspectable plan | Yes | No (but you can log) | Partial |
| Recommended new code | No | Yes | Yes |

**The only defensible planner use** is a regulated workflow that must surface an explicit, human-approvable plan — and even then, prefer manual function calling with an approval gate.

---

## Interview angle

> "Manual vs automatic function calling — when manual?" Manual when you must validate arguments, require human approval, audit, or sandbox untrusted tools; you inspect `FunctionCallContent` and invoke yourself. "What replaced planners and why?" Native function calling plus agents — planners' plan-DSL was brittle, injection-prone, and added a round-trip; reserve them only for inspectable human-approvable plans. Naming the supersession honestly is the differentiator.

---

**Next:** [Prompt Templates](04-prompt-templates.md) — the template engines behind prompt functions.
