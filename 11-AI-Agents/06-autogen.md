# AutoGen

AutoGen is Microsoft Research's open-source framework for building applications where multiple LLM-backed agents (and humans) collaborate via conversation. It is **primarily a Python framework** — there is a .NET port, but the docs, examples, and ~95% of interview questions assume Python, so this file is Python-first.

> **Convergence reality (mid-2026):** AutoGen and Semantic Kernel are converging into the **Microsoft Agent Framework (MAF)**. AutoGen contributed the event-driven runtime and multi-agent orchestration ideas; SK contributed enterprise plumbing. The honest framing: *learn AutoGen for the concepts and the interview vocabulary (group chat, selector, handoff, code executors, event-driven runtime) — but build new Azure enterprise systems on MAF.* The vocabulary transfers almost 1:1. (verify current GA status / API names.)

---

## Mental model and the layered packages

AutoGen 0.4+ split the old monolith into three layers — a clean architecture a .NET dev will recognize:

```text
┌───────────────────────────────────────────────────────────┐
│  autogen-agentchat   →  high-level: AssistantAgent, teams   │  "the API you write"
├───────────────────────────────────────────────────────────┤
│  autogen-core        →  actor model, event runtime, msgs    │  "the kernel"
├───────────────────────────────────────────────────────────┤
│  autogen-ext         →  model clients, tools, executors     │  "the adapters/DI"
└───────────────────────────────────────────────────────────┘
```

- `autogen-core` ≈ an in-process actor framework (think Orleans grains / a message bus).
- `autogen-agentchat` ≈ the opinionated "happy path" on top (like ASP.NET MVC on Kestrel).
- `autogen-ext` ≈ the adapters: model clients (Azure OpenAI), tools, code executors.

Most application code targets `agentchat` + the relevant `ext` clients; drop to `core` only for custom orchestration.

```python
from autogen_ext.models.openai import AzureOpenAIChatCompletionClient
from azure.identity import DefaultAzureCredential, get_bearer_token_provider

# In production prefer AAD/Managed Identity over API keys.
token_provider = get_bearer_token_provider(
    DefaultAzureCredential(), "https://cognitiveservices.azure.com/.default")
model_client = AzureOpenAIChatCompletionClient(
    azure_deployment="gpt-4o", model="gpt-4o", api_version="2024-10-21",
    azure_endpoint="https://my-aoai.openai.azure.com/",
    azure_ad_token_provider=token_provider)
```

The identity needs the **Cognitive Services OpenAI User** role on the resource.

---

## AssistantAgent and UserProxyAgent

The two foundational agent types:

- **`AssistantAgent`** — an LLM-backed agent: name, system prompt, model client, optional tools. It receives messages, calls the model, optionally calls tools, and returns a response. This is your "worker" / "brain."
- **`UserProxyAgent`** — represents the human (or an automated stand-in). It does *not* call an LLM by default; it surfaces messages to a human and feeds the reply back. It's the human-in-the-loop boundary and, historically, the code-execution boundary.

```python
from autogen_agentchat.agents import AssistantAgent, UserProxyAgent
from autogen_agentchat.messages import TextMessage
from autogen_core import CancellationToken

assistant = AssistantAgent(
    name="planner", model_client=model_client,
    system_message="You are a concise senior architect. Answer in <=5 sentences.")

response = await assistant.on_messages(
    [TextMessage(content="Outline a retry policy for an Azure queue consumer.", source="user")],
    cancellation_token=CancellationToken())

human = UserProxyAgent(name="human", input_func=input)  # swap input_func for a web hook
```

**Version history (interviewers probe this):** in 0.2 both derived from one `ConversableAgent` toggled by config (`llm_config=False` made it a proxy); in 0.4 they are separate, cleanly-contracted classes, and code execution moved out of `UserProxyAgent` into explicit code-executor agents/tools.

---

## Teams: RoundRobin vs. Selector group chat

A single agent rarely solves a hard task well. AutoGen's signature feature is multi-agent conversation, exposed in 0.4+ as **Teams**:

```text
          RoundRobinGroupChat                    SelectorGroupChat
          (fixed rotation)                       (LLM picks next speaker)

   ┌────────┐   ┌────────┐                  ┌──────────── Selector LLM ────────────┐
   │ writer │──▶│reviewer│                  │  reads thread → emits next speaker     │
   └───▲────┘   └───┬────┘                  └───┬─────────────┬──────────────┬──────┘
       │            │                            ▼             ▼              ▼
       └────────────┘                       ┌────────┐  ┌──────────┐  ┌───────────┐
        (loops until                        │ planner│  │ coder    │  │ tester    │
         termination)                       └────────┘  └──────────┘  └───────────┘
```

- **`RoundRobinGroupChat`** — agents speak in fixed rotation. Deterministic, cheap, reproducible. Great for known pipelines (writer → reviewer → writer).
- **`SelectorGroupChat`** — an LLM (the manager) reads the conversation and *chooses* the next speaker by name, using each agent's `description`. Dynamic and capable, but adds a model call per turn and is non-deterministic.

```python
from autogen_agentchat.teams import RoundRobinGroupChat, SelectorGroupChat
from autogen_agentchat.conditions import TextMentionTermination, MaxMessageTermination
from autogen_agentchat.ui import Console

writer = AssistantAgent("writer", model_client,
    system_message="Draft marketing copy. When the reviewer says APPROVE, stop.")
reviewer = AssistantAgent("reviewer", model_client,
    system_message="Critique the draft. Reply 'APPROVE' only when it is excellent.")

# Composable termination: stop on 'APPROVE' OR after 8 messages (safety valve).
termination = TextMentionTermination("APPROVE") | MaxMessageTermination(8)
rr_team = RoundRobinGroupChat([writer, reviewer], termination_condition=termination)
await Console(rr_team.run_stream(task="Write a tagline for an Azure cost tool."))

sel_team = SelectorGroupChat([planner, coder, tester],
    model_client=model_client, termination_condition=MaxMessageTermination(12))
```

**When each:** RoundRobin for known, ordered pipelines (determinism, lower cost, reproducibility); Selector when the path is dynamic and you want content-based routing, accepting extra cost and non-determinism. The agent `description` field is the *only* signal the selector uses to route — write distinct, action-oriented descriptions. **Termination conditions are composable** (`|` OR, `&` AND), and forgetting termination is the classic production bug — runaway loops burning tokens.

---

## Tool use vs. code execution

Two distinct mechanisms — keep them separate:

1. **Function tools** — typed Python functions (signature + docstring) the model can call. AutoGen exposes them as tool schemas; the model decides when to call, AutoGen executes and feeds results back. Bounded, controllable — prefer for production integrations.
2. **Code execution** — the agent *writes* code and an executor *runs* it. Far more powerful, far more dangerous: treat it as **LLM-driven remote code execution**.

```python
# (1) Function tools: just typed functions
def get_azure_vm_price(sku: str, region: str) -> str:
    """Return the hourly price for an Azure VM SKU in a region."""
    return {("D2s_v5", "eastus"): "$0.096/hr"}.get((sku, region), "unknown")

tool_agent = AssistantAgent("pricing", model_client, tools=[get_azure_vm_price],
    reflect_on_tool_use=True, system_message="Use tools to answer Azure pricing questions.")

# (2) Code execution in a sandboxed Docker container — the production-safe default
from autogen_ext.code_executors.docker import DockerCommandLineCodeExecutor
async with DockerCommandLineCodeExecutor(work_dir="coding", timeout=60) as executor:
    coder = AssistantAgent("coder", model_client,
        system_message="Write Python in a fenced block to solve the task.")
    runner = CodeExecutorAgent("runner", code_executor=executor)
```

**Securing code execution:** always use `DockerCommandLineCodeExecutor` (container isolation), disable network egress unless needed, set CPU/memory/time limits, use an ephemeral work dir, run as non-root, destroy the container after each run, and gate risky execution behind HITL approval. Never use `LocalCommandLineCodeExecutor` against a host you care about. Microsoft interviewers love this security framing.

---

## Human-in-the-loop

Two shapes:

1. **`UserProxyAgent` in a team** — the human is a participant; the team pauses for input at that agent's turn (synchronous).
2. **Termination + resume** — run the team to an approval point, **persist the team state** (`save_state()`), return control, collect the human decision asynchronously (webhook / queue / Teams card), then `load_state(...)` and resume. This is the right pattern for non-blocking web apps — don't block on `input()`.

```python
from autogen_agentchat.conditions import HandoffTermination, TextMentionTermination
team = RoundRobinGroupChat([assistant, human],
    termination_condition=HandoffTermination(target="user") | TextMentionTermination("APPROVE"))
```

Durable HITL turns ephemeral chats into reliable business processes: pause at a handoff, persist state, resume days later from any worker. Pair with idempotent external actions so resuming doesn't double-execute.

---

## Handoff and other workflow patterns

- **Reflection (generator/critic)** — a generator drafts, a critic evaluates against criteria, loop until approved. `RoundRobinGroupChat([generator, critic])` with `TextMentionTermination("APPROVE")`.
- **Handoff / Swarm** — an agent transfers control to a named peer (`handoffs=[Handoff(target="billing", ...)]`), surfaced as a handoff tool the model calls. Combined with `HandoffTermination(target="user")`, this is how you implement HITL escalation.
- **Nested chat / Society of Mind** — a `SocietyOfMindAgent` wraps an entire team and presents it as a single agent, so you compose and reuse proven sub-teams (teams of teams) while keeping the outer transcript clean.

```text
  Reflection (generator/critic)        Handoff / Swarm
  ───────────────────────────          ─────────────────────────
   ┌──────────┐  draft  ┌────────┐      ┌─────────┐  handoff   ┌──────────┐
   │generator │ ──────▶ │ critic │      │ triage  │ ─────────▶ │ billing  │
   └────▲─────┘ ◀────── └────────┘      └────┬────┘            └────┬─────┘
        │   revise (loop)                    │ handoff              │ handoff
        └──── until critic OK                ▼                      ▼
                                        ┌─────────┐           ┌──────────┐
                                        │  tech   │           │  human   │
                                        └─────────┘           └──────────┘
```

---

## The 0.2 → 0.4 architecture shift (event-driven runtime)

This is the single most likely deep-dive question. AutoGen 0.2 was a **synchronous, conversation-centric** library: agents called each other in a blocking, tightly-coupled loop — hard to scale or observe. AutoGen 0.4 was a ground-up rewrite to an **asynchronous, event-driven actor model**:

- **Actor model** — each agent is an isolated actor with private state; they communicate only via **messages**, never shared mutable state (Orleans/Akka-style). Actors process one message at a time, avoiding locks.
- **Event-driven runtime** — a runtime dispatches messages by topic; agents subscribe and react. Decouples *who sends* from *who handles*.
- **Async-first** — everything is `async`/`await`, enabling concurrency and throughput.
- **Local *and* distributed runtimes** — the same agents run in-process or across processes/machines (gRPC), so you scale out without changing the agents.
- **Cross-language** — a .NET implementation of the core runtime shipped alongside Python; a Python agent and a C# agent can participate in the same distributed system.

```text
        AutoGen 0.2                          AutoGen 0.4+
   (synchronous, coupled)              (async, event-driven actors)

   Agent ──direct call──▶ Agent        Agent ──msg──▶ [ Runtime / Event Bus ]
     │  blocking, shared                                 │  routes by topic
     ▼  conversation state                               ▼
   Agent ◀──direct call── Agent        Agent ◀──msg──  Agent   (isolated state,
                                                                 local or distributed)
```

One-breath answer: *"0.4 replaced the synchronous, tightly-coupled conversation model with an asynchronous, event-driven actor runtime — isolated agents communicating by message, with local and distributed runtimes and a layered package design. It's Orleans-style actors for LLM agents, and it's the foundation that fed the Microsoft Agent Framework."* Note the practical cost: 0.2 → 0.4 was a **breaking rewrite**, not a version bump.

Also worth knowing: 0.2's `initiate_chat(recipient, message=...)` (synchronous, blocking) maps to 0.4's `await team.run(task=...)` / `team.run_stream(task=...)` (async, team-centric, yielding a `TaskResult` or an event stream for streaming UIs).

---

## AutoGen Studio (low-code)

**AutoGen Studio** is a web UI for *prototyping* agent teams without writing orchestration code: visually compose agents, assign models/tools/prompts, run teams, watch the message flow, and export configs.

```bash
pip install autogenstudio
autogenstudio ui --port 8080      # open http://localhost:8080
```

Position it correctly: Studio is for **rapid prototyping, demos, and letting non-engineers experiment** — it is *not* a production deployment artifact. The senior answer: prototype the topology in Studio, then export/rebuild it as version-controlled, tested code (or migrate to MAF) for production, with CI/CD, observability, and security wired in. Knowing where low-code stops is itself a maturity signal.

---

## What interviewers probe

- *AssistantAgent vs. UserProxyAgent?* AssistantAgent is LLM-driven and tool-capable; UserProxyAgent is the human boundary (and classically the code-execution boundary). In 0.4 they're separate classes.
- *RoundRobin vs. Selector?* RoundRobin for known ordered pipelines (deterministic, cheap); Selector for dynamic content-based routing (extra LLM call, non-deterministic). Good `description` text is critical for Selector.
- *Function tools vs. code execution — risk?* Tools are bounded and production-preferred; code execution is LLM-driven RCE — always sandbox (Docker, no egress, limits, ephemeral) and gate with HITL.
- *Explain the 0.2 → 0.4 shift.* Synchronous/coupled → async, event-driven actor runtime; isolated message-passing agents; local + distributed; layered packages; the foundation of MAF. Breaking rewrite.
- *Termination conditions?* Mandatory; composable with `|`/`&`; forgetting them is the classic runaway-loop bug.
- *Where does AutoGen go from here?* Concepts and vocabulary converge into MAF; build new Azure enterprise systems on MAF, keep AutoGen as the teaching/prototyping vehicle.
