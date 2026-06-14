# Enterprise Copilot (Analytics & Executive Reporting)

A conversational analytics copilot — Project 5 of the portfolio. Executives ask plain-English questions ("How did EMEA margin trend vs. plan last quarter, and what drove the variance?"), and the copilot returns **grounded** narrative insights plus an embedded, drill-able **Power BI** visual. Backend combines **ASP.NET Core + Azure AI Foundry** for reasoning and **Copilot Studio** for the channel/agent surface, with **NL-to-semantic-model** querying (not blind NL-to-SQL). **Difficulty: ⭐⭐⭐⭐ Architect-tier.**

> **Build-order note:** The analytics-shaped sibling of the agent projects. It reuses Project 1's chat surface and Project 2's citation UI, but proves a distinct architect skill: choosing the *right* grounding source. Instead of a vector store, it grounds on the certified Power BI semantic model — so numbers match the official dashboards.

### Business Problem

Leadership reporting is bottlenecked by the BI team. Executives wait days for ad-hoc analysis; analysts spend ~**40%** of their week on repetitive "slice this KPI by region/time/segment" requests. Static dashboards answer *what* but not *why*, so follow-ups still route through humans.

**Solution & ROI.** A copilot grounded on the governed Power BI **semantic model** answers natural-language questions instantly, narrates the *why* (variance drivers), and embeds the live visual for self-serve drill-down. Model for a company with ~120 executives/managers and a 9-person BI team:

| Lever | Before | After | Annual impact |
|---|---|---|---|
| BI analyst time on ad-hoc | 40% of capacity | 15% | ~4.7 FTE redeployed to strategic work |
| Loaded BI analyst cost | $110k | $110k | **~$520k** capacity recovered |
| Exec time waiting for answers | 1–3 days | seconds | Faster decisions, hard to price but material |
| Decision latency on month-end review | days | same-meeting | Tighter financial close cadence |

The compelling architecture point: answers are grounded on the **certified semantic model**, so numbers match the official dashboards — eliminating the "two sources of truth" trust problem that kills most NL-to-SQL projects.

> **Mid-2026 GA note:** Power BI **Embedded** and the semantic-model query APIs are GA. Copilot Studio custom agents are GA; the **Foundry-grounded** answer pattern (agent calls a tool that executes DAX against the semantic model) is the recommended design over raw text-to-SQL.

### Architecture Diagram

```text
 ┌─────────────────────────────────────────────────────────────┐
 │  React App                                                   │
 │  ┌───────────────┐   ┌──────────────────────────────────┐   │
 │  │ NL chat panel │   │ Power BI Embedded report (drill)  │   │
 │  │ (insight cards│   │  (filters synced to chat context) │   │
 │  │  + citations) │   └──────────────────────────────────┘   │
 │  └──────┬────────┘                                           │
 └─────────┼────────────────────────────────────────────────────┘
           │ HTTPS (Entra token)            ▲ embed token
 ┌─────────▼───────────────────────────┐    │
 │  ASP.NET Core Copilot API            │    │  Power BI REST
 │  • Intent + entity extraction        │────┘  (embed token, RLS)
 │  • Foundry agent (reasoning)         │
 │  • Tool: SemanticModelQueryTool      │
 │  • Grounding + narrative synthesis   │
 └───┬─────────────┬─────────────┬──────┘
     │             │             │
 ┌───▼────┐  ┌─────▼──────┐ ┌────▼──────────────┐
 │ Azure  │  │ Copilot    │ │ Power BI / Fabric │
 │ AI     │  │ Studio     │ │ Semantic Model    │
 │Foundry │  │ agent      │ │ (DAX queries via  │
 │(models │  │ (Teams/web │ │  XMLA / exec-     │
 │+ eval) │  │  channel)  │ │  queries API)     │
 └────────┘  └────────────┘ └─────────┬─────────┘
                                       │
                              ┌────────▼─────────┐
                              │ Fabric Lakehouse │
                              │ / Azure SQL DW    │  (curated marts)
                              └──────────────────┘
   Cross-cutting: Entra ID + RLS • Managed Identity • App Insights
```

### Sequence Diagram

```text
Exec    React    CopilotAPI   Foundry Agent   SemanticModelTool   PowerBI/Fabric   ContentSafety
 │        │           │             │                │                  │               │
 │ "Why did EMEA margin miss plan in Q1?"            │                  │               │
 ├───────►│           │             │                │                  │               │
 │        ├─POST /ask─►│            │                │                  │               │
 │        │           ├─moderate input──────────────────────────────────────────────►│
 │        │           │◄─ok─────────────────────────────────────────────────────────┤
 │        │           ├─invoke agent─►│              │                  │               │
 │        │           │              ├─plan: need margin actual vs plan by driver      │
 │        │           │              ├─call tool(DAX: margin actual/plan by driver,EMEA,Q1)
 │        │           │              │               ├─exec DAX (XMLA)─►│               │
 │        │           │              │               │◄─resultset (RLS-scoped)         │
 │        │           │              │◄─rows + metadata                 │               │
 │        │           │              ├─synthesize narrative + identify top variance drivers
 │        │           │              ├─attach citations (measure names, filters used)   │
 │        │           │◄─insight + structured result─┤                  │               │
 │        │           ├─moderate output─────────────────────────────────────────────►│
 │        │           │◄─ok──────────────────────────────────────────────────────────┤
 │        │           ├─request embed token (report + RLS identity)────►│               │
 │        │           │◄─embed token─────────────────────────────────────             │
 │        │◄─insight card + embed token + filter context─┤             │               │
 │◄───────┤ render narrative + auto-filtered Power BI visual            │               │
 │ (drills further in embedded report; new questions reuse context)     │               │
```

The agent never invents numbers: it **must** call `SemanticModelQueryTool`; the narrative cites the exact measures and filters used. RLS in the semantic model guarantees the exec only sees data they're entitled to.

### Folder Structure

```text
enterprise-copilot/
├── src/
│   ├── frontend/                         # React + TypeScript
│   │   ├── src/
│   │   │   ├── components/
│   │   │   │   ├── ChatPanel.tsx
│   │   │   │   ├── InsightCard.tsx        # narrative + citations
│   │   │   │   ├── PowerBiEmbed.tsx       # powerbi-client-react
│   │   │   │   └── FilterSync.ts          # chat <-> report filter bridge
│   │   │   └── api/copilot.ts
│   │   └── package.json
│   ├── CopilotApi/                        # ASP.NET Core
│   │   ├── Program.cs
│   │   ├── Agents/InsightAgent.cs         # Foundry-hosted agent client
│   │   ├── Tools/
│   │   │   ├── SemanticModelQueryTool.cs  # DAX via Power BI exec-queries API
│   │   │   └── MetadataCatalogTool.cs     # measures/dimensions discovery
│   │   ├── Grounding/NarrativeComposer.cs
│   │   ├── PowerBi/EmbedTokenService.cs
│   │   ├── Safety/ContentSafety.cs
│   │   └── Models/InsightDtos.cs
│   ├── CopilotStudio/                     # exported agent + topics (channel surface)
│   │   └── insight-agent.solution.zip
│   └── Shared/Telemetry/
├── semantic-model/                        # Tabular model project (TMDL)
│   ├── model.tmdl
│   └── measures/                          # certified DAX measures
├── infra/
│   ├── main.bicep
│   └── modules/{appservice,foundry,keyvault,appinsights}.bicep
├── tests/
│   ├── CopilotApi.Tests/
│   └── eval/nl_to_insight_goldens.jsonl   # Q -> expected measures/filters
├── .github/workflows/ci-cd.yml
└── README.md
```

### Database Design

This project deliberately does **not** introduce a new analytical store — that is the architectural point. It queries the **governed Power BI / Fabric semantic model** (DAX over a tabular model), which sits on a **Fabric Lakehouse** / Azure SQL data mart. Operational copilot data (conversation memory, feedback) lives in a small Azure SQL DB.

**Store-choice rationale:**

| Data | Store | Rationale |
|---|---|---|
| Business metrics (KPIs, margins, sales) | Power BI semantic model (tabular/DAX) on Fabric/SQL DW | Single certified source of truth; RLS; measures already defined and trusted |
| Metadata catalog (measures/dimensions) | Read from semantic model schema | Grounds the agent's tool calls; no duplication |
| Conversation memory + thumbs feedback | Azure SQL | Lightweight transactional; joins to user identity for personalization |
| (Optional) measure-name embeddings | Azure AI Search | Maps fuzzy NL to the right certified measure (avoids hallucinated measures) |

**Azure SQL: `Conversations`, `Turns` & `Feedback`**

| Table | Key columns |
|---|---|
| Conversations | ConversationId (PK), UserOid, StartedAtUtc, ContextJson |
| Turns | TurnId (PK), ConversationId (FK), Question, MeasuresUsed (JSON), FiltersUsed (JSON), NarrativeHash, CreatedAtUtc |
| Feedback | FeedbackId (PK), TurnId (FK), Rating (-1/0/1), Comment, CreatedAtUtc |

**Why not NL-to-raw-SQL / a vector DB of facts?** Querying the semantic model guarantees numbers match certified dashboards and respects RLS; a separate vector store of facts would drift and create a second source of truth. The vector store is used only to *route* language to the right measure, never to hold the answer.

### API Contracts

| Method | Path | Purpose |
|---|---|---|
| POST | `/api/ask` | Ask a NL analytics question |
| POST | `/api/embed-token` | Get a scoped Power BI embed token |
| GET | `/api/catalog/measures` | List certified measures (for UI hints) |
| POST | `/api/feedback` | Submit thumbs/comment on an answer |

**POST `/api/ask`**
```json
{
  "conversationId": "cnv_71",
  "question": "Why did EMEA gross margin miss plan in Q1 FY26?",
  "preferredVisual": "waterfall"
}
```
Response `200`:
```json
{
  "turnId": "trn_204",
  "narrative": "EMEA gross margin was 41.2% vs a 44.0% plan (-2.8 pts). The miss was driven mainly by (1) freight cost inflation (+1.6 pts impact) and (2) unfavorable product mix in DACH (+0.9 pts). FX was roughly neutral.",
  "grounding": {
    "measuresUsed": ["Gross Margin %", "Gross Margin % Plan", "Freight Cost", "Product Mix Variance"],
    "filtersUsed": { "Region": "EMEA", "FiscalQuarter": "FY26 Q1" },
    "rows": 14
  },
  "embed": {
    "reportId": "rpt_margin",
    "filterContext": { "Region": "EMEA", "FiscalQuarter": "FY26 Q1" }
  }
}
```

**POST `/api/embed-token`** → returns `{ "token": "...", "tokenExpiry": "2026-06-14T18:30:00Z", "embedUrl": "..." }` scoped with the caller's RLS identity.

### Security Design

- **Identity & RLS.** Entra ID auth; the Power BI **embed token is generated with the caller's effective identity** so **Row-Level Security** filters the semantic model — the copilot physically cannot return data the user isn't entitled to.
- **RBAC.** App roles `Viewer` (ask + view), `Analyst` (ask + see DAX/grounding detail), `Admin`. Foundry agent and Power BI workspace access via service principal with least privilege.
- **Managed Identity.** API authenticates to Foundry, Key Vault, and SQL via managed identity; Power BI service principal credential stored in Key Vault.
- **Network isolation.** App Service + Foundry behind Private Endpoints; Front Door + WAF in front; Power BI workspace restricted to the service principal and tenant.
- **Content safety & grounding governance.** Content Safety on input/output; the agent is **constrained to answer only from `SemanticModelQueryTool` results** (no free-form numeric claims). Every answer logs the exact measures/filters used, enabling number-level auditability. Guardrail: if a question can't be grounded, the copilot says so rather than guessing.

### Deployment Architecture

- **Compute.** Azure **App Service** (Linux) for the ASP.NET Core API; static frontend on App Service or Static Web Apps. App Service deployment slots enable **blue-green** (swap after smoke + eval).
- **IaC.** Bicep for App Service, Foundry hub/project, Key Vault, App Insights. Semantic model deployed via Fabric deployment pipelines (dev→test→prod).
- **CI/CD.** GitHub Actions: build/test → run `nl_to_insight_goldens.jsonl` (assert correct measures/filters chosen, within tolerance) → deploy to staging slot → smoke → slot swap. **Canary** by routing a % of traffic to the new slot first.
- **Multi-region.** Active-passive: primary region App Service + Foundry, Front Door health-probe failover to a warm secondary; semantic model replicated via Fabric.

```bicep
// deployment slot for blue-green (excerpt)
resource stagingSlot 'Microsoft.Web/sites/slots@2023-12-01' = {
  parent: copilotApi
  name: 'staging'
  location: location
  properties: { serverFarmId: planId }
}
```

### Azure Services Used

- **Azure AI Foundry** — hosts the reasoning agent and the eval harness; manages model deployments and prompt/grounding flows.
- **Copilot Studio** — provides the conversational agent surface and channel publishing (Teams/web), invoking the Foundry-backed tools.
- **Power BI Embedded + Fabric semantic model** — certified metrics, DAX execution, RLS, embedded drill-down visuals.
- **Microsoft Fabric Lakehouse / Azure SQL DW** — curated data marts behind the semantic model.
- **Azure SQL Database** — conversation memory and feedback.
- **Azure AI Search** *(optional)* — measure-name routing embeddings to prevent hallucinated measures.
- **Azure App Service** — API + frontend hosting with deployment slots.
- **Entra ID** — auth, app roles, RLS identity propagation.
- **Azure Key Vault** — Power BI SP secret, connection strings.
- **Azure Content Safety** — input/output moderation.
- **Application Insights** — telemetry, answer-grounding logs.
- **Azure Front Door + WAF** — global entry, failover, edge security.

### Resume Bullet Points

- Built an **NL-to-insight executive copilot** grounding every answer on the certified Power BI semantic model (DAX via the execute-queries API), eliminating the "two sources of truth" problem and freeing an estimated **4.7 BI FTEs** (~$520k/yr) from repetitive ad-hoc reporting.
- Designed a **grounding governance pattern** where the Foundry agent is constrained to answer only from semantic-model tool results and logs the exact measures/filters used, delivering **number-level auditability** and dashboard-matching figures.
- Integrated **Power BI Embedded with per-user RLS embed tokens**, ensuring the copilot physically cannot surface data outside a user's row-level entitlements while still allowing self-serve drill-down synced to chat context.
- Implemented a **React conversational analytics UI** that pairs narrative variance explanations (the "why") with an auto-filtered embedded report, cutting executive answer latency from **1–3 days to seconds**.
- Established a **golden NL-to-insight evaluation suite** in CI that asserts the agent selects the correct certified measures and filters within tolerance before any deployment-slot swap.
- Delivered the solution on **App Service deployment slots with canary swaps** and **Private Endpoint** isolation, integrating Copilot Studio for a Teams channel surface alongside the React app.

### GitHub Portfolio Presentation

**README outline**
1. Hero GIF — exec asks "why did margin miss plan," copilot narrates drivers, embedded waterfall auto-filters.
2. The trust story up front: "grounded on the certified semantic model — numbers match your dashboards."
3. Architecture + NL-to-insight sequence diagram.
4. RLS / embed-token security explainer.
5. Quickstart + a sample semantic model (TMDL) so reviewers can run it.
6. Eval results: % correct measure/filter selection.

**What reviewers should see:** the deliberate choice of **semantic-model querying over raw NL-to-SQL** (with an ADR justifying it), RLS-scoped embed tokens, grounding logs that make answers auditable, and the Copilot Studio + Foundry split. Include `docs/grounding.md` (how hallucination is prevented), `docs/rls.md`, and a short Loom-style demo link.
