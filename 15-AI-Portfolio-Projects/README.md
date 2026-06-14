# AI Portfolio Projects — Six Buildable, Microsoft-Stack AI Systems That Prove You Ship

> Six production-grade portfolio projects that climb from a single streaming chat assistant to a multi-tenant AI platform other teams build on. Every project is genuinely buildable by one senior engineer, Microsoft-centric (Azure OpenAI, Semantic Kernel, Microsoft Agent Framework, Entra ID, Bicep), and written to match what AI Solutions Developer/Architect interviews actually probe. Accurate to platform GA status as of **mid-2026**.

---

## 📚 Table of Contents

These six projects are ordered by **increasing complexity**. Build them in order — each reuses infrastructure and patterns from the one before it.

1. **[AI Chat Assistant](01-ai-chat-assistant.md)** — An internal streaming chat assistant on Azure OpenAI + Semantic Kernel: token-by-token SSE, Entra SSO, per-user history, key-free managed identity. *Difficulty: ⭐ Beginner (the fundamentals everything else assumes).*
2. **[Enterprise RAG Platform](02-enterprise-rag-platform.md)** — Upload → ingest → hybrid+semantic search → cited answers over thousands of internal docs, with document-level security trimming. *Difficulty: ⭐⭐ Intermediate (the project most tied to the Solutions Architect JD).*
3. **[Agent Workflow Platform](03-agent-workflow-platform.md)** — Multi-agent orchestration on the Microsoft Agent Framework with durable checkpointing and human-in-the-loop approval gates. *Difficulty: ⭐⭐⭐ Advanced (durable, safe, auditable agents — not a toy loop).*
4. **[Multi-Agent Enterprise Assistant](04-multi-agent-enterprise-assistant.md)** — Planning/Research/Execution/Approval agents collaborate with HITL governance and a live React collaboration dashboard. *Difficulty: ⭐⭐⭐⭐ Architect-tier (systems thinking + reuse).*
5. **[Enterprise Copilot](05-enterprise-copilot.md)** — Conversational analytics copilot grounded on the certified Power BI semantic model, with embedded drill-down and RLS-scoped answers. *Difficulty: ⭐⭐⭐⭐ Architect-tier (grounding governance + the "two sources of truth" win).*
6. **[Enterprise AI Platform (Capstone)](06-enterprise-ai-platform.md)** — A seven-service, multi-tenant AI platform on AKS with model routing, evals, governance, and chargeback-grade cost attribution. *Difficulty: ⭐⭐⭐⭐⭐ Capstone (I can design a platform other teams build on).*

---

## 🎯 Build Order & Learning Path

**Build them in numeric order.** The sequence is deliberate: each project introduces one major new capability while reusing the infrastructure you already wrote.

- **Start with Project 1.** It nails the table stakes every later project assumes — token-by-token streaming, real Entra authentication, durable history, and key-free managed identity to Azure OpenAI. Get the Bicep modules, the auth handler, and the streaming helper right here; you reuse them everywhere.
- **Next, Project 2** adds retrieval: an ingestion pipeline (Document Intelligence → chunk → embed → Azure AI Search), hybrid + semantic search, citations, and document-level security trimming. This is the single most interview-relevant project.
- **Then Project 3** adds agency: the Microsoft Agent Framework, durable checkpointing in Cosmos, and human-in-the-loop approval gates. It reuses Project 2's RAG as a tool for its Research agent.
- **Projects 4–6 prove systems thinking, not new primitives.** They explicitly **reuse and promote** earlier components:
  - **Project 1's chat service** → the user-facing surface and per-agent LLM client wrapper.
  - **Project 2's RAG pipeline** → a standalone, reusable RAG microservice every agent consumes.
  - **Project 3's single-agent + tool-calling pattern** → generalized into multi-agent orchestration.
- **Project 4** generalizes Project 3 into a four-agent (Planning/Research/Execution/Approval) collaboration with policy-driven HITL.
- **Project 5** is the analytics-shaped sibling: it grounds on the Power BI semantic model instead of a vector store, proving you can pick the *right* grounding source.
- **Project 6 is the capstone** — it productizes Projects 2–4 as managed services behind one gateway, adds model routing, evals, governance, and cost, and graduates the deployment from Container Apps to AKS.

> **Rule of thumb:** Projects 1–3 each prove an *individual* capability; Projects 4–6 prove you can assemble those capabilities into governed, observable, multi-tenant systems.

---

## 🔑 Shared Conventions

Treat all six projects as a **single mono-repo** (`enterprise-ai-portfolio/`). The mono-repo tells reviewers one coherent story and lets a single GitHub Actions workflow matrix-build everything.

### Mono-repo layout

```text
enterprise-ai-portfolio/
├─ projects/
│  ├─ 01-chat-assistant/
│  ├─ 02-rag-platform/
│  ├─ 03-agent-platform/
│  ├─ 04-multi-agent-assistant/
│  ├─ 05-enterprise-copilot/
│  └─ 06-enterprise-ai-platform/
├─ infra/            # cross-cutting Bicep modules (networking, Key Vault, Log Analytics, ACR)
├─ shared/           # common .NET concerns (auth handlers, telemetry, streaming helpers)
├─ CONVENTIONS.md    # the conventions below, pinned and referenced from every project README
└─ .github/workflows/
```

### Common stack

| Concern | Standard choice |
|---|---|
| Backend | **.NET 9** ASP.NET Core minimal APIs |
| Frontend | **React 19 + TypeScript + Vite** (Next.js for the Project 6 console) |
| Orchestration | **Semantic Kernel** (kernel/plugins/filters) → **Microsoft Agent Framework** for multi-agent |
| Models | **Azure OpenAI** — `gpt-4o` / `gpt-4o-mini`, `text-embedding-3-large` (via Azure AI Foundry) |
| Auth | **Entra ID** for users; **managed / workload identity** to every Azure resource — zero connection-string secrets |
| IaC | **Bicep** (Helm too, for the Project 6 AKS capstone) |
| Hosting | **Azure Container Apps** (Projects 1–4), **App Service** (Project 5), **AKS** (Project 6) |
| Observability | **OpenTelemetry → Application Insights** (+ Prometheus/Grafana on AKS) |
| Safety | **Azure AI Content Safety** + Prompt Shields on model I/O |

### What every project README should show reviewers

- A one-line pitch and a **hero GIF/Loom** of the thing actually working.
- The **architecture diagram** (ASCII or rendered) and the **sequence diagram**.
- **"Run in 5 minutes"** quickstart (`azd up` / `helm install`) with a seeded demo so reviewers can try it.
- A **security model** section: managed identity (no keys), Entra scopes/app roles, data isolation.
- An **evaluation** section — showing you *measure* quality (citation accuracy, groundedness, agent pass-rate) is a strong differentiator.
- **Bicep IaC** and a CI pipeline that includes an eval gate, not just unit tests.
- For Projects 4–6: a `docs/adr/` folder justifying the big calls (Cosmos vs SQL, ACA vs AKS, Agent Framework vs raw AutoGen, semantic-model querying vs NL-to-SQL).

---

## 🗂️ Skill → Project map

Each project exercises specific repo sections. Use this table to study the underlying skill before (or alongside) building.

| Project | Primary repo sections | What it draws on |
|---|---|---|
| **1 — AI Chat Assistant** | [08-AI-Foundations](../08-AI-Foundations/), [09-Azure-AI](../09-Azure-AI/), [10-Semantic-Kernel](../10-Semantic-Kernel/) | SK kernel + streaming chat completion, SSE in ASP.NET Core, managed-identity auth to Azure OpenAI |
| **2 — Enterprise RAG Platform** | [12-RAG-Systems](../12-RAG-Systems/), [10-Semantic-Kernel](../10-Semantic-Kernel/), [09-Azure-AI](../09-Azure-AI/) | Vector + hybrid retrieval, semantic reranking, chunking/embeddings, citations, security trimming |
| **3 — Agent Workflow Platform** | [11-AI-Agents](../11-AI-Agents/), [10-Semantic-Kernel](../10-Semantic-Kernel/), [13-AI-System-Design](../13-AI-System-Design/) | Multi-agent orchestration, durable checkpointing, HITL approval gates, tool plugins |
| **4 — Multi-Agent Enterprise Assistant** | [11-AI-Agents](../11-AI-Agents/), [13-AI-System-Design](../13-AI-System-Design/), [12-RAG-Systems](../12-RAG-Systems/), [14-AI-Frontend](../14-AI-Frontend/) | Group-chat/handoff orchestration, RAG-as-a-service reuse, risk-classified HITL, SignalR streaming |
| **5 — Enterprise Copilot** | [09-Azure-AI](../09-Azure-AI/), [12-RAG-Systems](../12-RAG-Systems/), [14-AI-Frontend](../14-AI-Frontend/) | Foundry-hosted agents, Copilot Studio, semantic-model grounding, RLS, grounding governance |
| **6 — Enterprise AI Platform** | [13-AI-System-Design](../13-AI-System-Design/), [11-AI-Agents](../11-AI-Agents/), [12-RAG-Systems](../12-RAG-Systems/), [09-Azure-AI](../09-Azure-AI/) | Multi-tenancy, model routing, eval/safety gates, cost attribution, AKS + Service Bus, governance |

> Section folders `08-…` through `14-…` are the sibling directories in this repo (`08-AI-Foundations`, `09-Azure-AI`, `10-Semantic-Kernel`, `11-AI-Agents`, `12-RAG-Systems`, `13-AI-System-Design`, `14-AI-Frontend`). Follow the topic if a path differs in your checkout.
