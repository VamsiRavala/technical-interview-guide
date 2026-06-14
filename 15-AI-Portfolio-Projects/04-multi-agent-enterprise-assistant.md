# Multi-Agent Enterprise Assistant

An architect-tier capstone: a collaboration of specialized agents — **Planning**, **Research**, **Execution**, and **Approval** — that decompose a business request, gather grounded evidence, draft actions, and route consequential steps through a human approver. The frontend is a React **agent collaboration dashboard** that visualizes the live plan, each agent's reasoning trace, and the HITL approval queue. **Difficulty: ⭐⭐⭐⭐ Architect-tier.**

> **Build-order note:** This project deliberately **reuses** earlier components rather than reinventing them: Project 1's chat service becomes the per-agent LLM client wrapper, Project 2's RAG pipeline is promoted into a standalone **reusable RAG microservice** consumed by the Research agent, and Project 3's single-agent + tool-calling pattern is generalized into a multi-agent orchestration layer.

> **Mid-2026 platform note:** The **Microsoft Agent Framework** (the converged successor to AutoGen + Semantic Kernel's agent abstractions) is GA for the runtime and orchestration primitives; its hosted multi-agent **workflow** designer and declarative agent YAML are in public preview. **Azure AI Foundry Agent Service** is GA. **Copilot Studio** agents and **Power BI Embedded** are GA. Treat AutoGen as the research/prototyping lineage that now flows into the Agent Framework — new code targets the Agent Framework SDK, with AutoGen patterns referenced for conversational group-chat orchestration.

### Business Problem

Enterprise operations teams (procurement, IT service management, HR ops) handle thousands of semi-structured requests a month — "renew this vendor contract," "provision a new analytics environment for the EMEA team," "onboard contractor X with these access rights." Each request requires *planning* (what steps?), *research* (policy lookup, prior tickets, vendor data), *execution* (draft POs, create tickets, generate access requests), and *approval* (a manager or compliance officer must sign off before anything irreversible happens).

Today this is a manual relay across Teams, email, ServiceNow, and SharePoint. Median cycle time is **3–5 business days** and ~**20%** of requests bounce back due to missing policy checks.

**Solution & ROI.** A multi-agent assistant collapses the relay into a single supervised workflow. Conservative model for a 1,500-employee org processing ~4,000 ops requests/month:

| Lever | Before | After | Annual impact |
|---|---|---|---|
| Avg. handling time per request | 95 min analyst time | 22 min (review + approve only) | ~58,400 analyst-hours saved |
| Loaded analyst cost | $48/hr | $48/hr | **~$2.8M** labor avoided |
| Cycle time | 3–5 days | < 4 hours (median) | Faster vendor/employee SLAs |
| Policy-bounce rate | 20% | < 4% | Fewer rework loops, audit-clean |
| LLM + infra run cost | — | ~$11k/month | **~$132k** cost |

Net first-year benefit lands in the **$2.4–2.6M** range against a build cost of ~3–4 engineer-months, with the strategic win being a **governed, auditable** automation pattern reusable across departments.

### Architecture Diagram

```text
                         ┌───────────────────────────────────────────────┐
                         │  React Agent Collaboration Dashboard (SPA)     │
                         │  • Live plan tree  • Per-agent trace timeline  │
                         │  • HITL approval queue  • SignalR live updates │
                         └───────────────┬───────────────────────────────┘
                                         │ HTTPS + WebSocket (SignalR)
                                  ┌──────▼───────┐
                                  │  API Gateway  │  (ASP.NET Core)
                                  │  Entra ID JWT │  AuthZ, rate limit
                                  └──────┬───────┘
                                         │
                    ┌────────────────────▼─────────────────────┐
                    │     Orchestrator (Microsoft Agent          │
                    │     Framework runtime + SK kernel)         │
                    │  Group-chat / handoff orchestration        │
                    │  Shared task state + message bus           │
                    └───┬──────────┬──────────┬──────────┬───────┘
                        │          │          │          │
                  ┌─────▼───┐ ┌────▼────┐ ┌───▼─────┐ ┌──▼────────┐
                  │Planning │ │Research │ │Execution│ │ Approval  │
                  │ Agent   │ │ Agent   │ │ Agent   │ │ Agent     │
                  └────┬────┘ └────┬────┘ └────┬────┘ └────┬──────┘
                       │           │           │           │
        ┌──────────────┼───────────┼───────────┼───────────┼──────────────┐
        │              │           │           │           │              │
   ┌────▼────┐   ┌─────▼─────┐ ┌───▼────┐ ┌────▼─────┐ ┌───▼────────┐ ┌────▼─────┐
   │ Azure   │   │ RAG svc   │ │ Tool   │ │ Action   │ │ HITL queue │ │ Cosmos DB│
   │ OpenAI  │   │(Proj 2    │ │ plugins│ │ connectors│ │ (pending   │ │ task &   │
   │ (Foundry│   │ reused)   │ │ (SK)   │ │ ServiceNow│ │ approvals) │ │ trace    │
   │ models) │   │AI Search  │ │        │ │ Graph/PO) │ │            │ │ store    │
   └─────────┘   └───────────┘ └────────┘ └──────────┘ └────────────┘ └──────────┘
                                         │
                              ┌──────────▼──────────┐
                              │ Azure Content Safety │  (input/output gates)
                              │ + Prompt Shields      │
                              └──────────────────────┘
        Cross-cutting: Managed Identity • Key Vault • App Insights / OTel traces
```

### Sequence Diagram

```text
User      Dashboard   Gateway   Orchestrator  Planning  Research   RAG    Execution  Approval  Approver(Human)  Connectors
 │           │          │            │           │         │        │        │          │            │             │
 │ submit req│          │            │           │         │        │        │          │            │             │
 ├──────────►│          │            │           │         │        │        │          │            │             │
 │           ├─POST /req─►│           │           │         │        │        │          │            │             │
 │           │          ├─start task─►│          │         │        │        │          │            │             │
 │           │          │            ├─plan(req)─►│         │        │        │          │            │             │
 │           │          │            │           ├─decompose into steps                  │            │             │
 │           │          │            │◄─plan─────┤         │        │        │          │            │             │
 │           │◄SignalR: plan ready───┤            │         │        │        │          │            │             │
 │           │          │            ├──research(step)─────►│        │        │          │            │             │
 │           │          │            │           │         ├─query──►│        │          │            │             │
 │           │          │            │           │         │◄grounded chunks  │          │            │             │
 │           │          │            │◄─evidence + citations┤        │        │          │            │             │
 │           │◄SignalR: trace update─┤            │         │        │        │          │            │             │
 │           │          │            ├──draft actions──────────────────────►│          │            │             │
 │           │          │            │           │         │        │        ├─compose PO/ticket draft  │          │             │
 │           │          │            │◄──proposed actions (NOT executed)─────┤          │            │             │
 │           │          │            ├──risk classify─────────────────────────────────►│            │             │
 │           │          │            │           │         │        │        │          ├─policy + risk gate       │
 │           │          │            │◄──"requires human approval"─────────────────────┤            │             │
 │           │◄SignalR: approval req─┤            │         │        │        │          │            │             │
 │           │          │            │            │         │        │        │          │            │ notify(Teams)│
 │           │          │            │            │         │        │        │          │            ├────────────►│
 │           │          │            │            │         │        │        │          │ approve/reject + note     │
 │           │          │            │            │         │        │        │          │◄───────────┤             │
 │           │          │            │◄─approved──────────────────────────────────────┤            │             │
 │           │          │            ├──execute approved actions───────────►│          │            │             │
 │           │          │            │            │         │        │        ├──────────────────────────────────►│ create ticket/PO
 │           │          │            │            │         │        │        │◄─────────────────────────────────┤ result
 │           │◄SignalR: completed────┤            │         │        │        │          │            │             │
 │◄──summary─┤          │            │            │         │        │        │          │            │             │
```

Key property: **no irreversible connector call happens before the Approval agent's gate plus a human decision** for actions classified medium/high risk. Low-risk read-only actions can be auto-approved by policy.

### Folder Structure

```text
multi-agent-assistant/
├── src/
│   ├── frontend/                          # React + Vite + TypeScript
│   │   ├── src/
│   │   │   ├── components/
│   │   │   │   ├── PlanTree.tsx            # live decomposition view
│   │   │   │   ├── AgentTraceTimeline.tsx  # per-agent reasoning trace
│   │   │   │   ├── ApprovalQueue.tsx       # HITL action cards
│   │   │   │   └── CitationPanel.tsx       # reused from Project 2
│   │   │   ├── hooks/useSignalR.ts
│   │   │   └── api/client.ts
│   │   └── package.json
│   ├── Gateway/                           # ASP.NET Core minimal API
│   │   ├── Program.cs
│   │   ├── Auth/EntraIdExtensions.cs
│   │   └── Hubs/CollaborationHub.cs       # SignalR
│   ├── Orchestrator/
│   │   ├── AgentRuntimeHost.cs            # Microsoft Agent Framework host
│   │   ├── Orchestration/
│   │   │   ├── GroupChatStrategy.cs       # AutoGen-style group chat
│   │   │   └── HandoffStrategy.cs         # plan→research→exec→approve
│   │   ├── Agents/
│   │   │   ├── PlanningAgent.cs
│   │   │   ├── ResearchAgent.cs
│   │   │   ├── ExecutionAgent.cs
│   │   │   └── ApprovalAgent.cs
│   │   ├── Plugins/                       # Semantic Kernel plugins
│   │   │   ├── RagPlugin.cs               # calls reused RAG service
│   │   │   ├── ServiceNowPlugin.cs
│   │   │   ├── GraphPlugin.cs
│   │   │   └── RiskClassifierPlugin.cs
│   │   ├── State/TaskStateStore.cs        # Cosmos-backed
│   │   └── Safety/ContentSafetyGate.cs
│   └── Shared/
│       ├── Contracts/                     # DTOs shared FE/BE
│       └── Telemetry/OtelSetup.cs
├── infra/
│   ├── main.bicep
│   └── modules/{aca,cosmos,openai,search,keyvault,appinsights}.bicep
├── tests/
│   ├── Orchestrator.Tests/
│   └── eval/agent_scenarios.jsonl         # golden multi-agent scenarios
├── .github/workflows/ci-cd.yml
└── README.md
```

### Database Design

The task/trace store is **Cosmos DB (NoSQL)** — variable-shape agent messages, append-heavy traces, and partitioning by task fit a document model far better than relational. **Azure AI Search** holds the vector + keyword index (reused from Project 2). **Azure SQL** is used only for the normalized **approval ledger** where transactional integrity and reporting matter.

**Why each store:**

| Concern | Store | Rationale |
|---|---|---|
| Task state, agent messages, reasoning traces | Cosmos DB (NoSQL) | Schema-flexible, append-heavy, partition by `taskId`, low-latency reads for live dashboard |
| Knowledge corpus + embeddings | Azure AI Search (vector + BM25 hybrid) | Reuse Project 2; hybrid retrieval with semantic ranker |
| Approval ledger (who approved what, when) | Azure SQL | ACID, immutable audit reporting, joins to identity |

**Cosmos: `tasks` container** (partition key `/taskId`)

| Field | Type | Notes |
|---|---|---|
| id | string | message/event id |
| taskId | string | partition key |
| type | string | `plan` \| `evidence` \| `proposed_action` \| `approval_request` \| `decision` \| `result` |
| agent | string | Planning/Research/Execution/Approval |
| payload | object | step list, citations, draft action, etc. |
| riskLevel | string | low/medium/high (set by Execution/Approval) |
| createdAt | datetime (UTC) | |
| ttl | int | optional auto-expiry for transient traces |

**Azure SQL: `ApprovalLedger`**

| Column | Type | Notes |
|---|---|---|
| ApprovalId | UNIQUEIDENTIFIER PK | |
| TaskId | NVARCHAR(64) | links to Cosmos task |
| ActionSummary | NVARCHAR(1000) | what was approved |
| RiskLevel | TINYINT | |
| RequestedByAgent | NVARCHAR(40) | |
| ApproverOid | NVARCHAR(64) | Entra object id |
| Decision | NVARCHAR(16) | Approved/Rejected |
| DecisionNote | NVARCHAR(2000) | |
| DecidedAtUtc | DATETIME2 | |
| ConnectorReceipt | NVARCHAR(256) | id returned by ServiceNow/Graph after execution |

### API Contracts

| Method | Path | Purpose |
|---|---|---|
| POST | `/api/tasks` | Submit a new request, starts orchestration |
| GET | `/api/tasks/{id}` | Full task state + trace |
| GET | `/api/tasks/{id}/approvals` | Pending approvals for task |
| POST | `/api/approvals/{id}/decision` | Human approve/reject |
| WS | `/hubs/collaboration` | SignalR live trace/plan/approval events |

**POST `/api/tasks`**
```json
{
  "title": "Renew Contoso CDN vendor contract",
  "description": "Annual renewal, EMEA region, ~$120k, needs security re-review",
  "context": { "department": "IT", "requesterOid": "a1b2-..." },
  "autoApproveBelowRisk": "low"
}
```
Response `202 Accepted`:
```json
{
  "taskId": "tsk_8f31",
  "status": "planning",
  "streamUrl": "/hubs/collaboration?taskId=tsk_8f31"
}
```

**POST `/api/approvals/{id}/decision`**
```json
{
  "decision": "Approved",
  "note": "Security re-review complete; spend within FY budget."
}
```
Response `200`:
```json
{
  "approvalId": "apr_55a2",
  "decision": "Approved",
  "executionStatus": "queued",
  "connectorReceipts": []
}
```

### Security Design

- **Identity / AuthN.** Entra ID OIDC for the SPA (auth code + PKCE); APIs validate JWT audience/scope. Approvers require an `Approver` app role; submitting tasks requires `Operator`.
- **RBAC.** App roles (`Operator`, `Approver`, `Admin`) enforced at the gateway and re-checked at the approval endpoint (defense in depth). Risk-level-to-approver mapping is policy-driven.
- **Managed Identity everywhere.** No keys in code. Container Apps' system-assigned managed identity authenticates to Azure OpenAI, AI Search, Cosmos, Key Vault, and Service Bus via RBAC data-plane roles.
- **Network isolation.** All PaaS dependencies exposed only via **Private Endpoints** in a VNet; public network access disabled. Frontend served via Front Door with WAF.
- **Content safety & agent governance.** Azure AI Content Safety + **Prompt Shields** gate every model input/output. The Execution agent can only call **allow-listed** tool plugins; high-risk actions are *never* auto-executed. A `RiskClassifierPlugin` deterministically tags actions, and a policy engine forces HITL above the threshold. Every agent message, tool call, and decision is written to the immutable trace (Cosmos) + ledger (SQL) for audit.
- **Secrets.** Key Vault with RBAC; secrets referenced via Container Apps secret references.

### Deployment Architecture

- **Compute.** Azure **Container Apps** (orchestrator + gateway as separate revisions). ACA chosen over AKS here because the workload is moderate-scale and event-driven; the capstone (Project 6) graduates to AKS.
- **IaC.** Bicep modules per resource; one `main.bicep` orchestrates. Parameterized per environment.
- **CI/CD.** GitHub Actions: build → test → **agent eval gate** (run `agent_scenarios.jsonl`, fail if pass-rate < threshold) → container build/push to ACR → Bicep deploy. **Blue-green** via ACA revision traffic split (10% canary → 100%).
- **Observability.** OpenTelemetry traces export to App Insights; each agent turn is a span with token/cost attributes.

```bicep
// infra/modules/aca.bicep (excerpt)
resource orchestrator 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'orchestrator'
  location: location
  identity: { type: 'SystemAssigned' }
  properties: {
    managedEnvironmentId: acaEnvId
    configuration: {
      ingress: { external: false, targetPort: 8080 }
      activeRevisionsMode: 'Multiple' // enables blue-green
    }
    template: {
      containers: [ { name: 'orchestrator', image: '${acr}/orchestrator:${tag}' } ]
      scale: { minReplicas: 1, maxReplicas: 10 }
    }
  }
}
```

### Azure Services Used

- **Azure AI Foundry / Azure OpenAI** — hosts the GPT-family reasoning models per agent; central deployment & quota.
- **Microsoft Agent Framework (runtime)** — multi-agent orchestration, group-chat & handoff strategies, agent state.
- **Semantic Kernel** — plugin/function-calling layer the agents invoke (RAG, ServiceNow, Graph).
- **Azure AI Search** — hybrid retrieval index (reused from Project 2).
- **Azure Cosmos DB** — task state and reasoning-trace store.
- **Azure SQL Database** — transactional approval ledger.
- **Azure Service Bus** — decouples execution requests from connectors (reliable, retried).
- **Azure Container Apps** — serverless container hosting with revisions for blue-green.
- **Azure Content Safety (incl. Prompt Shields)** — input/output moderation & jailbreak defense.
- **Entra ID** — authN/authZ, app roles, managed identities.
- **Azure Key Vault** — secret/cert management.
- **Application Insights + OpenTelemetry** — distributed tracing, token/cost telemetry.
- **Azure Front Door + WAF** — global entry, TLS, edge protection.

### Resume Bullet Points

- Architected a **four-agent (Planning/Research/Execution/Approval) enterprise assistant** on the Microsoft Agent Framework and Semantic Kernel, cutting median ops-request cycle time from **3–5 days to under 4 hours** and reducing policy-bounce rework from **20% to under 4%**.
- Designed a **human-in-the-loop governance gate** that risk-classifies every proposed action and blocks all medium/high-risk connector calls until a role-based Entra approver signs off, producing an **immutable, audit-clean** approval ledger in Azure SQL.
- Built a **reusable RAG microservice** (promoted from a prior project) consumed by the Research agent for grounded, citation-backed evidence, improving answer factuality and eliminating duplicated retrieval code across agents.
- Delivered a **real-time React collaboration dashboard** over SignalR that streams the live plan tree, per-agent reasoning traces, and the approval queue, giving operations leads full transparency into autonomous decisions.
- Implemented **zero-secret, managed-identity** access to all Azure data planes with Private Endpoint network isolation and Content Safety / Prompt Shields gating, passing internal security review on first submission.
- Established an **automated agent-evaluation CI gate** (golden-scenario suite) that blocks deployments below a configured pass-rate, projected to avoid **~$2.5M/year** in analyst labor at the modeled 4,000-requests/month volume.

### GitHub Portfolio Presentation

**README outline**
1. Hero GIF — dashboard showing plan tree forming, traces streaming, an approval card being approved.
2. One-paragraph problem/ROI summary (the table above).
3. Architecture diagram (the ASCII one, plus a rendered version).
4. "How the four agents collaborate" — the sequence diagram with annotations.
5. Quickstart (`azd up`), env table, and a seeded demo scenario.
6. Security & governance section (HITL, content safety, audit).
7. Eval results badge (scenario pass-rate).

**What reviewers should see:** clean agent decomposition with single-responsibility agents, an explicit HITL boundary, reuse of the Project 2 RAG service (cite the dependency), OpenTelemetry traces, Bicep IaC, and a CI pipeline with an *evaluation* gate — not just unit tests. Include `docs/architecture.md` (component + data-flow), `docs/governance.md` (risk policy + approval matrix), and `docs/adr/` with at least three Architecture Decision Records (e.g., "Cosmos vs SQL for trace store", "ACA vs AKS", "Agent Framework vs raw AutoGen").
