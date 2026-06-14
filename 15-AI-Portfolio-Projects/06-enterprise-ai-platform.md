# Enterprise AI Platform (Capstone)

The portfolio capstone: a **multi-service, multi-tenant AI platform** that productizes everything above — a unified gateway, model routing, RAG, multi-agent orchestration, evaluation, governance, and cost monitoring — deployed on **AKS** with **Azure Service Bus** for inter-service async, and a **Next.js/TypeScript** admin + console frontend. This is the "I can design a platform other teams build on" capstone. **Difficulty: ⭐⭐⭐⭐⭐ Capstone.**

> **Build-order note:** This project **promotes Projects 2–4 into reusable services**: Project 2's RAG pipeline becomes the multi-tenant RAG Service, Projects 3–4's agent orchestration becomes the Agent Service, and the chat/streaming patterns from Project 1 ride behind the gateway. The deployment graduates from Container Apps (Projects 1–4) and App Service (Project 5) to AKS.

### Business Problem

In a large enterprise, dozens of teams each spin up their own LLM apps — duplicated RAG pipelines, no shared governance, no cost visibility, inconsistent safety, and a sprawl of API keys. Finance can't attribute spend; security can't enforce policy; nobody can compare model quality.

**Solution & ROI.** A **central AI platform** that every internal app calls through one gateway. Teams get RAG, agents, model routing, evals, and safety as managed services; the platform gives the org **chargeback-grade cost attribution**, **uniform governance**, and **30–50% model-cost reduction** via routing cheaper models for easy requests. Model for an org with ~25 internal AI apps:

| Lever | Before | After | Annual impact |
|---|---|---|---|
| Duplicated RAG/agent builds | ~25 bespoke stacks | 1 shared platform | ~15–20 engineer-months/yr avoided |
| LLM spend (no routing) | $1.8M/yr | $1.0–1.2M/yr | **~$600–800k** via model routing + caching |
| Cost attribution | none | per-tenant/app/user | Enables chargeback + budget enforcement |
| Governance incidents | ad-hoc | centrally enforced | Reduced risk; audit-ready |
| Time-to-first-AI-feature for a team | weeks | days | Faster delivery org-wide |

The platform turns AI from scattered projects into **governed infrastructure** — the architect-level thesis of the whole portfolio.

> **Mid-2026 GA note:** AKS, Service Bus, APIM, Cosmos, AI Search, Azure OpenAI, Foundry, and Entra are all GA. The Microsoft Agent Framework runtime is GA; declarative workflow authoring is in preview. Semantic caching and model-router patterns are commonly built atop APIM/AI Gateway features (some AI-gateway policies are GA, advanced semantic-cache policies vary — flagged in `docs`).

### Architecture Diagram

```text
            ┌──────────────────────────────────────────────────────────────┐
            │  Next.js + TypeScript Console (admin, tenant mgmt, dashboards) │
            └───────────────────────────┬──────────────────────────────────┘
                                         │ HTTPS (Entra)
                          ┌──────────────▼──────────────┐
                          │  AI Gateway (APIM + ASP.NET) │  authN/Z, quota,
                          │  tenant resolution, routing  │  rate-limit, semantic cache
                          └──┬───────┬───────┬───────┬───┴──────┬──────────┐
        ┌────────────────────┘       │       │       │          │          │
        │             ┌──────────────┘       │       │          │          │
   ┌────▼─────┐  ┌─────▼──────┐  ┌────────────▼──┐ ┌──▼────────┐ ┌▼────────┐ ┌▼──────────┐
   │ Model     │  │ RAG        │  │ Agent          │ │ Eval      │ │Governance│ │ Cost      │
   │ Router    │  │ Service    │  │ Service        │ │ Service   │ │ Service  │ │ Service   │
   │ svc       │  │ (Proj 2)   │  │ (Proj 3+4 MAF) │ │           │ │          │ │           │
   └────┬──────┘  └────┬───────┘  └───────┬────────┘ └────┬──────┘ └────┬─────┘ └────┬──────┘
        │              │                  │               │             │            │
        │         ┌────▼──────┐      ┌────▼─────┐         │        ┌────▼─────┐ ┌────▼──────┐
        │         │ AI Search │      │ Cosmos    │         │        │ Policy/   │ │ Cost ledger│
        │         │ (vectors) │      │ (agent    │         │        │ audit log │ │ (SQL)      │
        │         └───────────┘      │  state)   │         │        │ (SQL)     │ └───────────┘
        │                            └──────────┘          │        └──────────┘
   ┌────▼────────────────────────────────────────────────▼───┐
   │  Azure OpenAI / Foundry model deployments (S/M/L tiers)  │
   └──────────────────────────────────────────────────────────┘

   Async backbone:  ───►  Azure Service Bus topics: {eval.requested, cost.usage, audit.event, agent.tasks}
   Platform-wide:  AKS (namespaces per service) • KEDA autoscale • Entra workload identity •
                   Key Vault CSI • App Insights/OTel + Prometheus/Grafana • Private Endpoints
```

**Service responsibilities**

| Service | Responsibility |
|---|---|
| **AI Gateway** | Single entry; Entra authN, tenant resolution, quota/rate-limit, semantic cache, request routing, usage emission |
| **Model Router** | Picks cheapest-capable model per request (complexity heuristic + policy), fallback/failover, A/B routing |
| **RAG Service** | Reused Project 2 pipeline: ingest, chunk, embed, hybrid retrieve, cite — multi-tenant indexes |
| **Agent Service** | Reused Projects 3–4: Agent Framework orchestration, HITL, tool plugins |
| **Eval Service** | Offline + online evals (groundedness, relevance, safety), regression gates, scorecards |
| **Governance Service** | Policy enforcement (allowed models/tools, content safety config), immutable audit log, data-residency rules |
| **Cost Service** | Per-tenant/app/user token & $ accounting, budgets, alerts, chargeback reports |

### Sequence Diagram

```text
App   Gateway   Governance  ModelRouter  RAGsvc   AgentSvc   AzureOpenAI  EvalSvc(async)  CostSvc(async)  Audit(async)
 │       │          │            │          │        │            │            │              │              │
 │ POST /v1/chat (tenant=acme, app=support)│        │            │            │              │              │
 ├──────►│          │            │          │        │            │            │              │              │
 │       ├─resolve tenant + authZ (Entra)  │        │            │            │              │              │
 │       ├─policy check──────────►│         │        │            │            │              │              │
 │       │◄─allowed: models[S,M], tools[rag]┤        │            │            │              │              │
 │       ├─semantic cache lookup  │         │        │            │            │              │              │
 │       │  (miss)                │         │        │            │            │              │              │
 │       ├─route───────────────────────────►│        │            │            │              │              │
 │       │              classify complexity → pick model tier "M" │            │              │              │
 │       │◄─model=gpt-M, ground=true────────┤        │            │            │              │              │
 │       ├─retrieve──────────────────────────────────►│            │            │              │              │
 │       │◄─chunks + citations (tenant index)─────────┤            │            │              │              │
 │       ├─(agentic? no) call model──────────────────────────────►│            │              │              │
 │       │◄─completion + token usage───────────────────────────────┤            │              │              │
 │       ├─content safety (out) ok │         │        │            │            │              │              │
 │       ├─emit usage event ─────────────────────────────────────────────────────────────────►│ (cost)       │
 │       ├─emit audit event ──────────────────────────────────────────────────────────────────────────────►│
 │       ├─sample → eval.requested ────────────────────────────────────────────►│ (groundedness)│              │
 │       ├─store in semantic cache│         │        │            │            │              │              │
 │◄──────┤ response + citations + usageId    │        │            │            │              │              │
 │       │                        │          │        │            │            │              │              │
 │   (async) EvalSvc scores groundedness → writes scorecard; CostSvc updates tenant budget,    │              │
 │           fires alert if >80%; Audit persists immutable record.                              │              │
```

For an **agentic** request, the Gateway forwards to the **Agent Service**, which runs the Project 4 plan→research→execute→approve flow (with HITL) and emits the same usage/audit/eval events.

### Folder Structure

```text
enterprise-ai-platform/
├── apps/
│   └── console/                          # Next.js + TypeScript
│       ├── app/
│       │   ├── tenants/                  # tenant CRUD, quotas
│       │   ├── dashboards/               # cost, eval scorecards, usage
│       │   ├── governance/               # policies, audit viewer
│       │   └── playground/               # try-a-prompt
│       └── package.json
├── services/
│   ├── gateway/            (ASP.NET Core) # APIM-fronted; routing, cache, usage emit
│   ├── model-router/       (ASP.NET Core)
│   ├── rag-service/        (ASP.NET Core) # reused from Project 2
│   ├── agent-service/      (ASP.NET Core) # reused from Projects 3-4 (MAF + SK)
│   ├── eval-service/       (ASP.NET Core)
│   ├── governance-service/ (ASP.NET Core)
│   └── cost-service/       (ASP.NET Core)
├── libs/
│   ├── Platform.Contracts/               # shared DTOs + Service Bus message schemas
│   ├── Platform.Telemetry/               # OTel + Prometheus exporters
│   └── Platform.Tenancy/                 # tenant context middleware
├── deploy/
│   ├── helm/                             # one chart per service
│   │   └── platform/{gateway,rag,agent,...}/
│   ├── k8s/                              # base manifests, KEDA scaledobjects
│   └── bicep/
│       ├── main.bicep                    # AKS, ACR, Service Bus, Cosmos, SQL, search, Foundry
│       └── modules/{aks,servicebus,cosmos,sql,search,apim,keyvault,monitor}.bicep
├── eval/
│   └── suites/{groundedness,safety,regression}.jsonl
├── .github/workflows/
│   ├── ci.yml                            # build, test, eval gate
│   └── cd.yml                            # helm deploy, canary, multi-region
├── docs/
│   ├── architecture.md
│   ├── multi-tenancy.md
│   ├── governance.md
│   ├── cost-model.md
│   └── adr/
└── README.md
```

### Database Design

Polyglot persistence, chosen per workload, with **multi-tenant isolation**, **audit**, and **cost** as first-class tables.

**Store choice rationale**

| Concern | Store | Rationale |
|---|---|---|
| Tenant/app config, policies, audit, cost ledger | Azure SQL (per-platform; tenant column + RLS) | Relational, ACID, reporting/chargeback, RLS for tenant isolation |
| Agent state, conversation traces | Cosmos DB (partition by `tenantId`) | Flexible, append-heavy, low-latency, native multi-tenant partitioning |
| Vectors / knowledge | Azure AI Search (index-per-tenant or filtered) | Hybrid retrieval; tenant isolation via separate indexes for sensitive tenants |
| Semantic cache | Azure Cache for Redis (vector search) | Sub-ms cache hits keyed by embedding similarity, TTL per tenant |
| Telemetry/metrics | App Insights + Prometheus/Grafana | Traces + time-series; not a transactional store |

**SQL: `Tenants` (multi-tenant)**

| Column | Type | Notes |
|---|---|---|
| TenantId | UNIQUEIDENTIFIER PK | |
| Name | NVARCHAR(200) | |
| DataResidency | NVARCHAR(20) | e.g., `EU`, `US` — drives region routing |
| Tier | NVARCHAR(20) | Free/Standard/Enterprise (quota tier) |
| IsolationMode | NVARCHAR(20) | `shared` \| `dedicated-index` |
| CreatedAtUtc | DATETIME2 | |

**SQL: `AuditEvents` (audit)**

| Column | Type | Notes |
|---|---|---|
| EventId | BIGINT IDENTITY PK | |
| TenantId | UNIQUEIDENTIFIER | RLS predicate column |
| AppId | NVARCHAR(64) | |
| UserOid | NVARCHAR(64) | Entra object id |
| Action | NVARCHAR(64) | `chat`, `rag.query`, `agent.execute`, `policy.deny`... |
| ModelUsed | NVARCHAR(64) | |
| PolicyDecision | NVARCHAR(16) | Allow/Deny + reason |
| ContentSafetyFlags | NVARCHAR(256) | |
| CorrelationId | NVARCHAR(64) | links to trace |
| CreatedAtUtc | DATETIME2 | append-only (no UPDATE/DELETE grant) |

**SQL: `CostUsage` (cost)**

| Column | Type | Notes |
|---|---|---|
| UsageId | BIGINT IDENTITY PK | |
| TenantId | UNIQUEIDENTIFIER | RLS |
| AppId / UserOid | NVARCHAR(64) | attribution dimensions |
| Model | NVARCHAR(64) | |
| PromptTokens / CompletionTokens | INT | |
| EstimatedCostUsd | DECIMAL(12,6) | from price table × tokens |
| CacheHit | BIT | routed-from-cache flag |
| RequestId | NVARCHAR(64) | |
| CreatedAtUtc | DATETIME2 | |

**SQL: `Budgets`** — `BudgetId, TenantId, PeriodMonth, LimitUsd, SpentUsd, AlertThresholdPct`. Cost Service updates `SpentUsd` from usage events and raises alerts.

> **Tenant isolation:** SQL uses **Row-Level Security** with a `TenantId` session predicate set by `Platform.Tenancy` middleware; Cosmos partitions by `tenantId`; sensitive tenants get dedicated AI Search indexes (`IsolationMode = dedicated-index`).

### API Contracts

| Method | Path | Purpose |
|---|---|---|
| POST | `/v1/chat` | Unified chat completion (RAG/agent optional) |
| POST | `/v1/rag/query` | Direct grounded retrieval+answer |
| POST | `/v1/agents/{name}/runs` | Start a multi-agent run |
| GET | `/v1/usage?tenantId=&from=&to=` | Cost/usage report |
| GET / PUT | `/v1/governance/policies` | Read/update tenant policy |
| GET | `/v1/evals/scorecards?app=` | Quality scorecards |

**POST `/v1/chat`** (gateway)
```json
{
  "tenantId": "acme",
  "appId": "support-bot",
  "messages": [{ "role": "user", "content": "How do I reset MFA for a locked user?" }],
  "options": { "useRag": true, "maxModelTier": "M" }
}
```
Response `200`:
```json
{
  "requestId": "req_9c1f",
  "answer": "To reset MFA for a locked user... [1][2]",
  "citations": [{ "id": 1, "source": "kb/mfa-reset.md", "score": 0.83 }],
  "routing": { "modelUsed": "gpt-M", "cacheHit": false, "reason": "complexity=medium" },
  "usage": { "promptTokens": 412, "completionTokens": 188, "estimatedCostUsd": 0.0031 }
}
```

**GET `/v1/usage`**
```json
{
  "tenantId": "acme",
  "period": "2026-06",
  "totalCostUsd": 842.17,
  "byApp": [{ "appId": "support-bot", "costUsd": 511.40, "requests": 38211 }],
  "budget": { "limitUsd": 1500, "spentPct": 56.1, "alert": false }
}
```

### Security Design

- **Identity.** Entra ID for console users and for **service-to-service** auth via **AKS workload identity** (federated, no secrets). External apps authenticate to the gateway with Entra client credentials scoped per tenant/app.
- **RBAC.** Console roles: `PlatformAdmin`, `TenantAdmin`, `Auditor`, `FinanceViewer`. Data-plane RBAC to OpenAI/Search/Cosmos/SQL via workload identities; least privilege per service.
- **Managed / workload identity.** Every pod uses a federated identity to reach Key Vault (CSI driver), Service Bus, OpenAI, etc. — zero static secrets.
- **Network isolation.** Private AKS cluster; all PaaS via **Private Endpoints** in the VNet; APIM internal mode behind Front Door + WAF; egress locked via Azure Firewall/UDR. NSGs + Kubernetes NetworkPolicies restrict east-west traffic between namespaces.
- **Content safety & agent governance.** Governance Service centrally enforces per-tenant allowed models/tools, Content Safety + Prompt Shields config, and **data-residency** routing (EU tenants pinned to EU model deployments). All agent tool use is allow-listed; HITL inherited from Project 4. Immutable, append-only `AuditEvents` (no UPDATE/DELETE grants) plus tamper-evident hashing.
- **Multi-tenant isolation.** RLS in SQL, partition-by-tenant in Cosmos, optional dedicated AI Search indexes, per-tenant cache TTL, and quota enforcement at the gateway.

### Deployment Architecture

- **Compute.** **AKS** with a namespace per service; **KEDA** scales workers on Service Bus queue depth; HPA on CPU/latency for sync services. APIM fronts the gateway.
- **IaC.** Bicep provisions AKS, ACR, Service Bus, Cosmos, SQL, AI Search, Foundry, APIM, Key Vault, Log Analytics. **Helm** charts deploy each service; values per environment.
- **CI/CD.** GitHub Actions `ci.yml` (build, unit/integration tests, **eval gate** on groundedness/safety/regression suites) → `cd.yml` (push to ACR, `helm upgrade` with **canary** via a service-mesh/Argo Rollouts traffic split: 5% → 25% → 100% with automated rollback on SLO breach). **Blue-green** option for the gateway via two deployments + service selector swap.
- **Multi-region.** Active-active across two regions: Front Door routes by latency and **data-residency**; Cosmos multi-region writes; SQL with failover groups; AI Search replicated; Service Bus geo-DR (paired namespace). EU tenant traffic and EU model deployments stay in-region for residency.

```bicep
// deploy/bicep/modules/servicebus.bicep (excerpt)
resource sb 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: sbName
  location: location
  sku: { name: 'Premium', tier: 'Premium' }  // VNet + geo-DR support
  properties: { publicNetworkAccess: 'Disabled' }
}
resource costTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: sb
  name: 'cost.usage'
  properties: { enablePartitioning: true }
}
```

```csharp
// libs/Platform.Tenancy/TenantContextMiddleware.cs (excerpt)
public async Task InvokeAsync(HttpContext ctx, ITenantResolver resolver)
{
    var tenant = await resolver.ResolveAsync(ctx.User); // from Entra claim/app reg
    ctx.Items["TenantId"] = tenant.TenantId;
    // set SQL session context so RLS predicate filters rows for this tenant
    await _db.SetSessionTenantAsync(tenant.TenantId);
    await _next(ctx);
}
```

### Azure Services Used

- **Azure Kubernetes Service (AKS)** — hosts all microservices; namespaces per service, KEDA + HPA autoscaling, private cluster.
- **Azure API Management** — fronts the AI Gateway: auth, quotas, rate limits, AI-gateway/semantic-cache policies.
- **Azure Service Bus (Premium)** — async backbone for usage, audit, eval, and agent-task events; geo-DR.
- **Azure OpenAI / AI Foundry** — tiered model deployments (S/M/L) and eval tooling; region-pinned for residency.
- **Azure AI Search** — multi-tenant vector + hybrid retrieval (RAG service).
- **Azure Cosmos DB** — agent state/traces, partitioned by tenant, multi-region writes.
- **Azure SQL Database** — tenants, policies, audit, cost, budgets with RLS.
- **Azure Cache for Redis** — semantic response cache.
- **Microsoft Agent Framework + Semantic Kernel** — agent orchestration and plugins (reused from Projects 3–4).
- **Entra ID (+ workload identity)** — user and service-to-service auth, app roles.
- **Azure Key Vault (CSI driver)** — secret/cert management, zero static secrets in pods.
- **Azure Front Door + WAF** — global routing, residency-aware failover, edge security.
- **Azure Container Registry** — service images.
- **Application Insights + Azure Monitor (+ Prometheus/Grafana)** — distributed tracing, metrics, dashboards, alerting.

### Resume Bullet Points

- Architected a **multi-tenant enterprise AI platform** of seven microservices (AI gateway, model router, RAG, agent, eval, governance, cost) on **AKS** with **Azure Service Bus**, turning scattered team-by-team LLM builds into governed shared infrastructure and avoiding an estimated **15–20 engineer-months/year** of duplicated work.
- Designed a **model-routing + semantic-cache layer** that selects the cheapest-capable model per request, projected to cut org LLM spend **30–45% (~$600–800k/yr)** while preserving quality via an automated eval gate.
- Built **chargeback-grade cost attribution** (per-tenant/app/user token and dollar accounting with budgets and alerts) and **immutable, append-only audit** in Azure SQL with Row-Level Security, giving finance and security first-class visibility and control.
- Implemented **end-to-end multi-tenant isolation** — SQL RLS, Cosmos partition-by-tenant, dedicated AI Search indexes for sensitive tenants, and **data-residency-aware** routing pinning EU tenants to EU model deployments.
- Delivered **zero-secret service-to-service security** using AKS workload identity, Private Endpoints, internal APIM, NetworkPolicies, and a centralized Governance Service enforcing allowed models/tools and Content Safety policy per tenant.
- Established a **canary + automated-rollback CD pipeline** (Helm + traffic-split rollouts, eval/safety regression gates) and **active-active multi-region** topology with Front Door residency routing, Cosmos multi-region writes, and Service Bus geo-DR.

### GitHub Portfolio Presentation

**README outline**
1. Hero diagram — the seven-service decomposition + async backbone.
2. "Why a platform, not another app" — the ROI table and the duplicated-builds problem.
3. Service-by-service responsibilities table with links to each service's README.
4. Full request-lifecycle sequence diagram.
5. Multi-tenancy, governance, and cost-model deep-dive links.
6. Quickstart: `az deployment` + `helm install` on a dev AKS (or kind), with a seeded demo tenant.
7. Dashboards screenshots (cost, eval scorecards, audit viewer) and a demo video.
8. Eval/SLO badges.

**What reviewers should see:** clean service boundaries with explicit contracts and Service Bus message schemas (`libs/Platform.Contracts`), real multi-tenancy (RLS + partitioning + isolation modes), first-class cost and audit tables, workload-identity (zero-secret) security, Helm + Bicep IaC, KEDA autoscaling, canary rollouts with automated rollback, and OpenTelemetry tracing across services. The `docs/adr/` folder should justify the big calls — **AKS vs Container Apps for the platform**, **Service Bus vs Event Grid for the backbone**, **SQL+RLS vs database-per-tenant**, **model-router design**, and **how Projects 2–4 components were promoted into reusable services**. This is the project that demonstrates end-to-end **AI Solutions Architect** judgment, not just implementation.
