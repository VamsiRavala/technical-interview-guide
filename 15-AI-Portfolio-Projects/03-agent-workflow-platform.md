# Agent Workflow Platform

The most complex of the first three projects: a platform that orchestrates **multiple AI agents** to complete multi-step business tasks, with **human-in-the-loop approval gates**, task management, and live agent monitoring. Built on the **Microsoft Agent Framework**. **Difficulty: ⭐⭐⭐ Advanced.**

> **Build-order note:** Reuses Project 2's Azure AI Search as a RAG tool for its Research agent, and Project 1's streaming + auth foundation. Project 3's single-agent + tool-calling pattern is later generalized into the multi-agent orchestration of Projects 4 and 6.

> **GA status (mid-2026):** the **Microsoft Agent Framework** (the unified successor consolidating Semantic Kernel's agent abstractions and AutoGen's multi-agent orchestration) is the supported path for production agents on the Microsoft stack. Where features are still maturing, the design degrades gracefully to Semantic Kernel agents. Flag this in the README.

### Business Problem

A financial-services back office handles **vendor onboarding and procurement requests**: each request requires gathering vendor data, running compliance/risk checks, drafting a summary, and — critically — **a human manager must approve** before anything is committed (regulatory requirement). Today this is a manual, multi-day, multi-person relay. **Who pays:** the COO and the Procurement function, justified by cutting cycle time from days to hours and creating an auditable trail of who approved what and why. The non-negotiable constraint: AI agents can *prepare* and *recommend*, but **a human must approve** any consequential action (issuing a PO, sending an external email, writing to the system of record). The platform must show, in real time, what each agent is doing, surface decisions for approval, and never act autonomously past an approval gate.

### Architecture Diagram

```text
                         Entra ID
                            |
 ┌──────────┐  dashboard   v   ┌────────────────────────────────────────┐
 │ Manager  │──────────────────│ React dashboard (Container App / SWA)    │
 │ /Analyst │<──── live SSE/WS  │  task board, agent timeline, approvals   │
 └──────────┘     updates       └───────────────┬────────────────────────┘
                                                 | Bearer (Agent.Run / Agent.Approve)
                                                 v
 ┌────────────────────────────────────────────────────────────────────────┐
 │ ASP.NET Core API (Container App)                                         │
 │  ┌──────────────────┐  ┌───────────────────┐  ┌──────────────────────┐  │
 │  │ Orchestration     │  │ Approval gateway  │  │ Monitoring/telemetry │  │
 │  │ host (Agent       │  │ (pause/resume)    │  │ stream hub           │  │
 │  │ Framework)        │  └─────────┬─────────┘  └──────────┬───────────┘  │
 │  │  ├ Planner agent  │            │                       │              │
 │  │  ├ Research agent │            │ checkpoint            │ events       │
 │  │  ├ Compliance ag. │            v                       v              │
 │  │  └ Writer agent   │   ┌──────────────────┐   ┌──────────────────────┐ │
 │  └────────┬──────────┘   │ Cosmos DB        │   │ App Insights         │ │
 │           │ tools (MI)   │ (agent state,    │   │ (agent traces,       │ │
 │           v              │  run checkpoints,│   │  spans per step)     │ │
 │   ┌──────────────┐       │  approvals,      │   └──────────────────────┘ │
 │   │ Azure OpenAI │       │  events)         │                            │
 │   │ gpt-4o       │       └──────────────────┘                            │
 │   └──────────────┘                                                       │
 │           │ tools                                                        │
 │           v                                                              │
 │   ┌────────────────────────────────────────────────────────────────┐    │
 │   │ Tool plugins: AI Search (RAG), Graph (email draft), external     │    │
 │   │ vendor/compliance APIs (read-only until approved)                │    │
 │   └────────────────────────────────────────────────────────────────┘    │
 └──────────────────────────────────────────────────────────────────────────┘
                 durable orchestration backed by checkpoints in Cosmos
```

### Sequence Diagram

```text
Manager   Dashboard    API/Orchestrator    Planner   Worker agents   OpenAI   Cosmos
  start "Onboard vendor Acme"
  |--------->|              |                  |           |           |        |
  |          | POST /api/tasks (Agent.Run)     |           |           |        |
  |          |------------->| create run, persist state ----------------------->|
  |          |              | invoke Planner                |           |        |
  |          |              |----------------->| plan steps |           |        |
  |          |              |                  |----------------------->|        |
  |          |              |<----- step list -|            |           |        |
  |          |  SSE: step1 running             |            |           |        |
  |          |<-------------|  Research agent gathers vendor data        |        |
  |          |              |--------------------------->|  (tool: Search)|       |
  |          |  SSE: step2 running             | Compliance agent runs checks      |
  |          |<-------------|--------------------------->|------------->|        |
  |          |              | Writer drafts summary + recommends "ISSUE PO"      |
  |          |              | --> action requires approval --> PAUSE + checkpoint |
  |          |              | persist approval request --------------------------->|
  |          |  SSE: APPROVAL_REQUIRED (decision card)     |           |        |
  |<---------|<-------------|                  |           |           |        |
  | review + APPROVE/REJECT  |                 |           |           |        |
  |--------->| POST /api/approvals/{id} (Agent.Approve)    |           |        |
  |          |------------->| record decision + actor ------------------------->|
  |          |              | if APPROVED: resume from checkpoint, run action    |
  |          |              | (issue PO via external API, audit-logged)          |
  |          |  SSE: completed |              |           |           |        |
  |<---------|<-------------|                  |           |           |        |
```

### Folder Structure

```text
enterprise-ai-portfolio/
└─ projects/
   └─ 03-agent-platform/
      ├─ src/
      │  ├─ Agents.Api/                   # tasks, approvals, monitoring endpoints
      │  │  ├─ Endpoints/(TasksEndpoints.cs, ApprovalsEndpoints.cs, MonitorEndpoints.cs)
      │  │  ├─ Streaming/AgentEventHub.cs # SSE/WebSocket event fan-out
      │  │  └─ Program.cs
      │  ├─ Agents.Orchestration/         # Microsoft Agent Framework host
      │  │  ├─ Agents/(PlannerAgent.cs, ResearchAgent.cs, ComplianceAgent.cs, WriterAgent.cs)
      │  │  ├─ Workflow/(VendorOnboardingWorkflow.cs, ApprovalGate.cs, Checkpointer.cs)
      │  │  ├─ Tools/(SearchTool.cs, GraphEmailTool.cs, VendorApiTool.cs)
      │  │  └─ AgentHostSetup.cs
      │  ├─ Agents.Domain/                # Task, Run, Step, Approval, AgentEvent
      │  └─ Agents.Infrastructure/        # Cosmos repos, DI, MI clients
      ├─ web/
      │  ├─ src/
      │  │  ├─ components/(TaskBoard, RunTimeline, AgentCard, ApprovalDrawer, EventLog)
      │  │  ├─ hooks/(useTaskStream.ts, useApprovals.ts)
      │  │  └─ App.tsx
      │  └─ vite.config.ts
      ├─ infra/
      │  ├─ main.bicep
      │  └─ modules/(cosmos.bicep, openai.bicep, containerapp.bicep,
      │              search.bicep, keyvault.bicep)
      ├─ tests/(Agents.Orchestration.Tests, Agents.Api.Tests, web/)
      └─ README.md
```

### Database Design

**Store:** **Azure Cosmos DB (NoSQL)** — chosen over SQL because agent runs are document-shaped, deeply nested (steps, tool calls, events), high-write, and benefit from a flexible schema and a per-task partition key for cheap scoped reads and a natural event-sourcing model. Vector store is reused from Project 2's Azure AI Search for the RAG tool; no new vector store here.

**Container: `tasks`** (partition key `/taskId`)

| Field | Type | Notes |
|---|---|---|
| `id` | string | task id (== `taskId`) |
| `taskId` | string | partition key |
| `type` | string | e.g. `VendorOnboarding` |
| `title` | string | display |
| `requestedBy` | string | Entra oid |
| `status` | string | `Planning`/`Running`/`AwaitingApproval`/`Completed`/`Rejected`/`Failed` |
| `input` | object | request payload |
| `createdAt`/`updatedAt` | string (ISO) | timestamps |

**Container: `runs`** (partition key `/taskId`) — durable orchestration state / checkpoints

| Field | Type | Notes |
|---|---|---|
| `id` | string | runId |
| `taskId` | string | partition key |
| `checkpoint` | object | serialized Agent Framework state to resume |
| `steps` | array | `[{stepId, agent, status, startedAt, endedAt, output, toolCalls[]}]` |
| `currentStep` | string | pointer |
| `tokenUsage` | object | aggregate prompt/completion tokens |

**Container: `approvals`** (partition key `/taskId`)

| Field | Type | Notes |
|---|---|---|
| `id` | string | approvalId |
| `taskId` | string | partition key |
| `action` | string | e.g. `IssuePurchaseOrder` |
| `proposedBy` | string | agent name |
| `payload` | object | what will execute if approved |
| `status` | string | `Pending`/`Approved`/`Rejected` |
| `decidedBy` | string | Entra oid of approver |
| `reason` | string | approver note (audit) |
| `decidedAt` | string (ISO) | |

**Container: `events`** (partition key `/taskId`, TTL optional) — append-only monitoring stream: `{ id, taskId, ts, agent, level, type, message, data }`. Powers the live timeline and the audit trail.

### API Contracts

| Method | Path | Scope | Success | Errors |
|---|---|---|---|---|
| `POST` | `/api/tasks` | `Agent.Run` | `202` | `400`, `401` |
| `GET` | `/api/tasks` | `Agent.Run` | `200` | `401` |
| `GET` | `/api/tasks/{id}` | `Agent.Run` | `200` | `401`, `404` |
| `GET` | `/api/tasks/{id}/events` | `Agent.Run` | `200` (SSE) | `401`, `404` |
| `GET` | `/api/approvals?status=pending` | `Agent.Approve` | `200` | `401`, `403` |
| `POST` | `/api/approvals/{id}` | `Agent.Approve` | `200` | `400`, `401`, `403`, `404`, `409` |

**Start a task**

```json
// POST /api/tasks -> 202
// Request
{ "type": "VendorOnboarding", "title": "Onboard Acme Corp",
  "input": { "vendorName": "Acme Corp", "taxId": "12-3456789", "category": "logistics" } }
// Response
{ "taskId": "t_8841", "status": "Planning", "eventsUrl": "/api/tasks/t_8841/events" }
```

**Live event stream**

```text
// GET /api/tasks/{id}/events  -> 200 text/event-stream
data: {"type":"step.start","agent":"ResearchAgent","stepId":"s1"}
data: {"type":"tool.call","agent":"ResearchAgent","tool":"VendorApi","args":{"taxId":"..."}}
data: {"type":"step.end","agent":"ComplianceAgent","stepId":"s2","result":"low-risk"}
data: {"type":"approval.required","approvalId":"a_22","action":"IssuePurchaseOrder",
       "payload":{"amount":48000,"terms":"Net-30"}}
```

**Decide an approval** (resumes or stops the run)

```json
// POST /api/approvals/{id}
// Request
{ "decision": "Approved", "reason": "Risk acceptable; budget confirmed." }
// Response 200
{ "approvalId": "a_22", "status": "Approved", "taskStatus": "Running" }
// 409 if already decided
```

Approval gate in the workflow (conceptual, Agent Framework):

```csharp
// Writer agent proposes a consequential action -> gate pauses & checkpoints
var approval = await approvalGate.RequestAsync(new ApprovalRequest(
    taskId, action: "IssuePurchaseOrder", proposedBy: "WriterAgent", payload: poDraft), ct);

await checkpointer.SaveAsync(taskId, workflow.CaptureState(), ct);  // durable pause

// resumed by POST /api/approvals/{id}; on Approved:
if (approval.Status == ApprovalStatus.Approved)
    await vendorApiTool.IssuePurchaseOrderAsync(poDraft, actor: approval.DecidedBy, ct);
else
    await orchestrator.AbortAsync(taskId, reason: approval.Reason, ct);
```

### Security Design

- **AuthN/Z with role separation:** Entra ID app roles — `Agent.Run` (analysts can start/monitor tasks) and `Agent.Approve` (only managers can approve consequential actions). The approval endpoint requires `Agent.Approve` and **rejects self-approval** when the requester == approver if policy demands segregation of duties.
- **Human-in-the-loop as a hard gate:** consequential tools (issue PO, send external email, write to system of record) are **never callable by an agent directly**; they sit behind the approval gate and only execute after an `Approved` decision, with the approver's identity recorded. Read-only tools (search, lookups) run freely.
- **Managed identity + least privilege:** orchestrator uses MI for `Cognitive Services OpenAI User`, `Search Index Data Reader`, Cosmos `Built-in Data Contributor`, and Graph application permissions scoped to draft-only (no auto-send). External vendor/compliance APIs use credentials from **Key Vault**.
- **Audit trail:** the `events` and `approvals` containers form an immutable record (who approved what, when, why, with what payload) — essential for the financial-services compliance requirement. Enable Cosmos point-in-time restore / change feed to an archive.
- **Prompt injection / safety:** agents process external/document content, so apply **Content Safety prompt shields** on tool inputs/outputs, sanitize tool results before feeding them back to the model, constrain each agent with a tight system prompt and an allow-listed tool set, and cap step counts / token budgets to prevent runaway loops. Treat tool outputs as untrusted data, never as instructions.
- **Data isolation:** task reads are partition-scoped and filtered by `requestedBy`/role; analysts see their tasks, managers see the approval queue.

### Deployment Architecture

- **Hosting:** **Azure Container Apps** — API + orchestrator. Long-running/durable agent runs survive replica restarts because state is **checkpointed in Cosmos** (the orchestrator can resume any run on any replica). Min replicas ≥ 1; consider Container Apps **dedicated** workload profile for steadier agent workloads. (AKS is the alternative if you need fine-grained networking or GPU; called out in README as "why Container Apps, not AKS" — simpler ops for this scale.)
- **Environments:** `dev`/`prod`; Cosmos in serverless (dev) / autoscale (prod). OpenAI capacity sized for concurrent multi-agent runs.
- **CI/CD:** GitHub Actions (or Azure DevOps) — build orchestrator + API images, run agent integration tests with a mocked tool layer and a recorded-LLM harness, deploy via Bicep. Blue/green via Container Apps revisions with traffic splitting for safe agent-logic rollouts.
- **IaC:** Bicep for Cosmos, OpenAI, Search, Container Apps, Key Vault.

```bicep
// modules/cosmos.bicep (excerpt)
resource cosmos 'Microsoft.DocumentDB/databaseAccounts@2024-11-15' = {
  name: cosmosName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    capabilities: [ { name: 'EnableServerless' } ]      // dev; autoscale in prod
    consistencyPolicy: { defaultConsistencyLevel: 'Session' }
    disableLocalAuth: true                               // force AAD/MI, no keys
  }
}
```

### Azure Services Used

- **Azure OpenAI** — `gpt-4o` reasoning/generation for all agents. (GA.)
- **Microsoft Agent Framework** (on ASP.NET Core) — multi-agent orchestration, workflow, checkpointing, human-in-the-loop primitives. (Maturing/GA-track mid-2026; degrade to Semantic Kernel agents where needed.)
- **Azure Cosmos DB (NoSQL)** — durable agent/run state, checkpoints, approvals, append-only event log. (GA.)
- **Azure AI Search** — reused as a RAG tool for the Research agent (from Project 2). (GA.)
- **Microsoft Graph** — draft (not auto-send) outbound emails behind approval. (GA.)
- **Azure Container Apps** — API + orchestrator hosting with revisions/traffic splitting. (GA.)
- **Azure AI Content Safety** — prompt shields on tool I/O. (GA.)
- **Entra ID, Key Vault, ACR, App Insights** — identity/app roles, external API secrets, registry, agent tracing. (GA.)

### Resume Bullet Points

- Built a multi-agent **Agent Workflow Platform** on the **Microsoft Agent Framework** + ASP.NET Core that orchestrates Planner/Research/Compliance/Writer agents to automate vendor onboarding, cutting cycle time from **~3 days to under 2 hours**.
- Engineered **human-in-the-loop approval gates** with durable **Cosmos DB checkpointing**, pausing/resuming long-running agent runs across replica restarts and ensuring **no consequential action executes without a recorded human approval** — meeting financial-services compliance.
- Implemented role-separated authorization (Entra **`Agent.Run`** vs **`Agent.Approve`** app roles) with **segregation-of-duties** enforcement and an immutable, queryable **audit trail** of every approval (actor, payload, reason, timestamp).
- Delivered a real-time **agent monitoring dashboard** (React) streaming per-step agent activity and tool calls over SSE, giving managers full visibility into autonomous-yet-gated workflows.
- Hardened the agent layer against **prompt injection** by treating tool outputs as untrusted, applying **Content Safety prompt shields**, allow-listing tools per agent, and capping step/token budgets to prevent runaway loops.
- Deployed with **Bicep + GitHub Actions** to **Azure Container Apps** using revision-based **blue/green** rollouts and key-free **managed-identity** access to OpenAI, Cosmos, Search, and Graph.

### GitHub Portfolio Presentation

- **README outline:** pitch ("Agents that prepare, humans that approve") → GIF of the dashboard: task starts → agents work live → approval card pops → manager approves → task completes → "Why human-in-the-loop" (the regulatory framing) → "Orchestration architecture" diagram → "Durable runs & checkpointing" (how a run survives a restart) → "Approval gate" (the pause/resume code) → "Agent safety" (prompt injection, tool allow-lists) → "Why Microsoft Agent Framework + Container Apps" → deploy.
- **Demo:** ship the `VendorOnboarding` workflow with mocked external APIs so reviewers can run a full task end-to-end, watch the live timeline, and exercise both Approve and Reject paths. Include a "trigger a prompt-injection attempt" demo doc to show the shields working.
- **Highlight to reviewers:** the approval gate + checkpointing (this is the senior/architect signal — durable, safe, auditable agents, not a toy loop), the role-separated auth, the live monitoring stream, and the agent safety story. Frame it as "production agent patterns on the Microsoft stack."
