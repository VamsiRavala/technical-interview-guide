# Tool Calling & Agents (Intro)

On its own, an LLM is a closed box: it can only produce text from what's in its context. **Tool calling** breaks that box open — it lets the model fetch live data and take actions through your code. That, in turn, is the foundation of **agents**. This page covers the tool-calling loop, the ReAct pattern, and the crucial distinction between *workflows* and *agents* — at an intro level. The deep agent material (planner-executor, reflection, multi-agent, human-in-the-loop, frameworks) lives in section **11**.

---

## What Is Tool / Function Calling?

You describe each tool with a **name, description, and JSON-schema parameters**. The model, seeing the user request, decides **whether and which** tool to call and emits a **structured call with arguments**.

> Critical: the model **does not execute anything**. It only *requests* a call. **Your code executes it** — which makes your code the security boundary.

```text
You describe tools  →  Model picks one + fills args (JSON)  →  YOU run it  →  feed result back  →  Model answers
```

```csharp
// Conceptual: a tool the model can request (Semantic Kernel style; deep dive in section 10)
[KernelFunction, Description("Gets the current weather for a city")]
public async Task<string> GetWeatherAsync(
    [Description("City name")] string city)
    => await _weatherApi.GetAsync(city);
```

```python
# Conceptual tool schema (OpenAI-style)
tools = [{
    "type": "function",
    "function": {
        "name": "get_weather",
        "description": "Gets the current weather for a city",
        "parameters": {
            "type": "object",
            "properties": {"city": {"type": "string"}},
            "required": ["city"],
        },
    },
}]
```

Azure OpenAI supports **parallel tool calls** and **Structured Outputs** to guarantee schema-valid arguments.

### Bonus use: structured extraction

Even when you don't need to *execute* anything, you can define a "function" whose parameters *are* the schema you want, and force the model to call it. The model returns schema-valid JSON — a robust way to turn messy text (emails, documents) into typed data. (Structured Outputs now does this more directly — see [03-prompt-engineering](03-prompt-engineering.md).)

---

## The Tool-Calling Loop, End to End

```text
1. Send user message + tool definitions to the model
2. Model responds with EITHER a final answer OR one/more tool calls (name + JSON args)
3. If tool calls:
      validate args → execute each tool (auth, timeout, error handling)
                    → append each result as a "tool" message
4. Send the updated conversation back to the model
5. Model either calls more tools or produces the final answer
6. Loop until final text OR a max-iterations guard trips
```

```text
        ┌─────────────────────────────────────────┐
        │                                         ▼
  [User msg + tools] → [Model] → tool call? ──yes──→ [Your code runs tool] ──┐
                          │                                                   │
                          └── no → [Final answer]        result appended ─────┘
```

Non-negotiables in the loop:
- **Cap iterations** (and total tool calls) to prevent infinite loops.
- **Validate every argument** — the model can hallucinate or be injected into bad args.
- **Treat tool outputs as untrusted** (injection risk — see below).

Frameworks like **Semantic Kernel** automate this loop with auto function-invocation (section **10**).

---

## Error Handling & Retries Inside the Loop

Tools fail — timeouts, 4xx/5xx, bad args. Handle it gracefully:

- **Validate args before executing.**
- **Wrap calls with timeouts**; retry *transient* failures with **exponential backoff + jitter**.
- On permanent failure, **return a structured error message back to the model** as the tool result so it can retry differently, ask the user, or give up — don't crash the loop.
- Make tools **idempotent** or use **idempotency keys** so retries are safe.
- **Log every attempt** with correlation IDs.

```text
return  "Customer not found for ID 12345"      ← model recovers well
NOT     <raw stack trace / 500 HTML page>        ← model flails
```

---

## The ReAct Pattern

**ReAct = Reason + Act.** The model interleaves reasoning and tool use:

```text
Thought:      I need the customer's order history to answer this.
Action:       get_orders(customer_id="C-91")
Observation:  [{"id": 5012, "status": "shipped"}, ...]
Thought:      The latest order shipped on the 3rd. I can answer now.
Answer:       Your most recent order shipped on June 3rd.
```

This grounds reasoning in real data at each step and makes the agent's process **inspectable** — invaluable for debugging. Modern tool-calling APIs implement ReAct *implicitly*: the model's decision to call a tool is the "Act," and reasoning models do the "Reason" internally. Always **bound the loop** with a max step count and **log each Thought/Action/Observation** for observability.

---

## Workflows vs Agents

This is the single most important judgment call — and a near-universal interview question.

| | **Workflow** | **Agent** |
|---|---|---|
| Control of the loop | You, in code | The LLM decides |
| Path | Predefined, deterministic | Dynamic, model-chosen |
| Predictability | High — testable, cheap | Lower — harder to debug |
| Cost / latency | Lower | Higher (many LLM calls) |
| Best for | Well-understood tasks | Open-ended, variable-path problems |

```text
Workflow:  step A → step B → (branch) → step C        (you wrote the arrows)
Agent:     model decides: which tool, what order, when done   (the arrows are dynamic)
```

**Anthropic's guidance:** prefer **workflows** for well-understood tasks; use **agents** only when flexibility and model-driven decisions at scale genuinely add value.

### When is an agent justified?

Ask: **is the path predictable?**
- If you can enumerate the steps → build a **deterministic workflow** (cheaper, faster, reliable).
- Agents earn their cost when the **solution path varies per input**, requires **dynamic tool selection**, or involves **open-ended exploration** (research, multi-step debugging, autonomous coding).

> The senior instinct: start with the simplest thing — often a single prompt or a fixed chain plus one or two tools — and add agentic autonomy only when the task demands it. **Don't build an autonomous agent to do a job a `switch` statement could.**

---

## Guardrails Around Any Agent (intro)

Treat the agent like an **untrusted automated user**. Layers:

1. **Hard budgets** — max iterations / max cost to stop runaway loops.
2. **Least-privilege tools** — scope each tool tightly; require **human approval for destructive/irreversible actions**.
3. **Input/output validation** on every tool call.
4. **Timeouts + retries with backoff**; **sandboxing** for code execution.
5. **Content safety** on inputs and outputs.
6. **Full observability** — log every thought, tool call, and result with tracing.
7. **Idempotency keys** so retries don't double-execute.

> Enforce security **in code, not in the prompt** — prompts can be subverted. Define what the agent absolutely must not do and gate it in your application/tool implementations. (Prompt-injection and indirect-injection specifics: [07-responsible-ai](07-responsible-ai.md).)

---

## A Note on Memory & MCP

- **Agent memory** = short-term (conversation/task state in the prompt) + long-term (vector store for semantic recall, structured store for facts). See [04-context-engineering](04-context-engineering.md).
- **Model Context Protocol (MCP)** is an open standard (from Anthropic, broadly adopted including across Microsoft) for connecting LLM apps to tools/data through a uniform client–server interface — the "USB-C of AI tools." Build an MCP server once; any MCP-capable client can use it. Deep dive in section **11**.

---

## Brush-up Cheat-Sheet

```text
Tool calling = model REQUESTS a call (JSON args); YOUR code executes (security boundary)
The loop     = send tools → model calls? → run + append result → repeat → cap iterations
Validate     = args before running; treat tool OUTPUT as untrusted (injection)
Errors       = backoff+jitter on transient; return structured error to model; idempotency keys
ReAct        = Thought → Action → Observation → … → Answer; log every step
Workflow     = deterministic, cheap, testable (default for known tasks)
Agent        = model controls the loop; use only when the path is unpredictable
Guardrails   = budgets, least-privilege, validation, sandbox, content safety, observability, in CODE
Deep dive    = agents, MCP, multi-agent → section 11
```

**Next:** [06-evaluation-basics](06-evaluation-basics.md) — proving the thing actually works.
