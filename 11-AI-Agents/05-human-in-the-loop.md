# Human-in-the-Loop

Few enterprise agent systems should be fully autonomous. For high-stakes actions — refunds, deletes, sends, infrastructure changes — the agent should **pause and ask a human before executing**. Human-in-the-loop (HITL) is both a *safety control* (gate irreversible actions) and a *compliance/audit requirement* (who approved what, when).

The defining production challenge: you cannot block an HTTP request or hold a thread open while a human takes minutes, hours, or days to respond. The answer is **durable suspend/resume** built on serializable thread state (section 04).

> (verify current GA status / API names) on approval-request and serialization types below — the surface is evolving.

---

## The core pattern: approve the action, not the prose

When the model wants to call a sensitive tool, the framework **surfaces the pending action** — its name and arguments — for human approval instead of auto-executing. You implement this by marking a tool as requiring approval (or disabling auto-invocation), then resuming the same thread with the human's decision.

The principle: **gate the *action* before side effects, not the final answer after.** Approving prose the model already produced is too late if a destructive tool already ran.

```csharp
// Mark a tool as requiring human approval before it executes  (verify current GA status / API names)
var refund = AIFunctionFactory.Create(IssueRefund);
AIAgent agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
{
    ChatOptions = new ChatOptions { Tools = [ new ApprovalRequiredAIFunction(refund) ] }
});

AgentRunResponse resp = await agent.RunAsync("Refund order 4821, $300.", thread);

// The run surfaces a pending approval instead of executing
if (resp.UserInputRequests.OfType<FunctionApprovalRequest>().FirstOrDefault() is { } req)
{
    bool approved = await AskHumanInTeams(req.FunctionCall);   // your UI / Teams card
    // Resume the SAME thread with the human's decision
    await agent.RunAsync(new[] { req.CreateResponse(approved) }, thread);
}
```

```python
# conceptual: agent returns a function-approval request
response = await agent.run(user_input, thread=thread)
if response.requires_approval:
    if human_approves(response.pending_call):
        await agent.run(approve(response.pending_call), thread=thread)
```

This generalizes SK's older auto-function-invocation filter / `Terminate` pattern into explicit, first-class approval workflows.

---

## Durable suspend/resume

Because thread state serializes, you can pause **indefinitely** — await a human in a UI, ticket, or Teams card — and resume later, even from a different process. This is what makes asynchronous HITL practical in stateless web apps.

```text
[Agent proposes refund]
        |
        v
   [HITL gate] --suspend, persist state-->  (waits hours/days)
        |
   human approves/rejects (Teams / web / ticket)
        |
        v
   [Execute or Cancel] --> [Audit log]
```

The durable loop:

```python
resp = await agent.run(user_input, thread=thread)
if resp.pending_approval:
    persist(thread.serialize(), resp.pending_approval)   # Cosmos / Redis / blob
    return                                               # release the request
# ... async: human decides, then a callback rehydrates and resumes ...
thread = agent.deserialize_thread(load(session_id))
await agent.run(decision_message, thread=thread)
```

For graph **workflows**, the same idea is a **checkpoint / request-response node** that durably suspends the graph until input arrives, then resumes from the checkpoint.

---

## End-to-end approval gate in production

1. **Classify** which tools/actions require approval — a policy decision (destructive, irreversible, financial, or above a threshold; pure reads usually don't).
2. **Intercept** via middleware (or disable auto-invocation) when the model requests such a tool, and **emit an approval request** containing the tool name and arguments.
3. **Persist** the thread/workflow state so the run can pause indefinitely, and surface the pending action to a human (UI / ticket / notification).
4. **Decide** — the human approves or rejects; your app **rehydrates** state and resumes: execute the tool on approval (feeding the result back to the model) or return a rejection the model handles gracefully.
5. **Audit** the decision — who approved, when, with what arguments, tied to the thread/correlation ID.
6. **Timeout/escalate** — add timeouts and escalation for unanswered approvals so they don't hang forever.

```python
async def approval_middleware(context, next):
    if is_sensitive(context.tool_call):
        if not await request_approval(context.tool_call):
            context.terminate = True
            return
    await next(context)
```

---

## Where the gate sits

In AutoGen this maps to two shapes: a `UserProxyAgent` participating in a team (the human is a turn-taking participant, synchronous), or **termination + resume** (run to an approval point, persist state, collect the decision asynchronously, then resume). For non-blocking web services, always use the second shape — never call `input()` and block.

The key design choice is *where* the approval gate sits — typically **before code execution or any external side effect**, so a jailbroken or mistaken model cannot trigger an irreversible action autonomously.

---

## What interviewers probe

- *Which actions warrant HITL?* Irreversible / financial / destructive / regulated actions; pure reads usually don't. Policy-driven (e.g., above a threshold).
- *How do you suspend for hours and resume reliably?* Durable checkpointing — serialize thread/workflow state, resume on the human's callback; don't hold a thread/HTTP request open.
- *Approve the tool call or the final answer?* The action, before side effects — not the prose after.
- *How do you make approvals auditable?* Record who/when/what payload, tied to the thread/correlation ID (section 08).
- *How do you do HITL without blocking a web request?* Run to a termination/approval point, persist state, return, collect the decision async (webhook/queue/Teams), then rehydrate and resume.
- *What about unanswered approvals?* Timeouts and escalation so a pending gate doesn't hang the workflow.
