# Azure AI — The Microsoft AI Services Layer

> The Microsoft AI **services** layer: Azure OpenAI, Azure AI Search, Azure AI Foundry, Content Safety, and the security/gateway patterns that make them production-grade. This section complements **[03-Azure](../03-Azure/README.md)** — it does not re-teach generic Azure (resource groups, RBAC, networking, monitoring); it applies those to the AI services that sit on top.

---

## 📚 Table of Contents

### Azure OpenAI
1. **[Azure OpenAI Overview](01-azure-openai-overview.md)** — Resource → deployment → model hierarchy, deployment-name vs model-name, model families/versions, regions
2. **[Deployments, Quota & PTUs](02-deployments-quota-and-ptus.md)** — PTU vs PAYG vs spillover, TPM/RPM quota, 429 handling & retries, Batch API, cost math

### Retrieval & Platform
3. **[Azure AI Search](03-azure-ai-search.md)** — Vector + keyword + hybrid, HNSW, semantic ranker, integrated vectorization, security trimming, index design
4. **[Azure AI Foundry](04-azure-ai-foundry.md)** — Foundry overview, model catalog, prompt flow & evaluation, Foundry Agent Service

### Safety, Security & Governance
5. **[Content Safety & Responsible AI](05-content-safety-and-responsible-ai.md)** — Configurable content filters, Prompt Shields, groundedness detection, RAI process
6. **[Security & Networking](06-security-and-networking.md)** — Managed identity + DefaultAzureCredential, RBAC roles, Private Endpoints, data residency/privacy, Key Vault
7. **[APIM AI Gateway](07-apim-ai-gateway.md)** — GenAI gateway: token-limit, emit-token-metric, semantic-cache, content-safety policies, backend pools/load-balancing

### Interview Preparation
8. **[Interview Questions](08-interview-questions.md)** — 50+ detailed Q&A on Azure OpenAI and the Microsoft AI stack

---

## 🎯 Learning Path

### Beginner (Week 1-2)
1. Start with **Azure OpenAI Overview** — internalize the resource → deployment → model hierarchy and deployment-name indirection
2. Learn **Deployments, Quota & PTUs** — PAYG vs PTU, what TPM/RPM mean, and how 429s happen
3. Understand **Content Safety** basics — the built-in bidirectional filters and `content_filter` handling
4. Set up a secure call from .NET using **managed identity** (see **Security & Networking**)

### Intermediate (Week 3-4)
1. Build a RAG pipeline with **Azure AI Search** — hybrid + semantic ranker, index design, integrated vectorization
2. Implement **security trimming** so retrieval respects document-level access
3. Explore **Azure AI Foundry** — model catalog, prompt flow, and running evaluations in CI
4. Master **429 handling**, prompt caching, and the cost levers in **Deployments, Quota & PTUs**

### Advanced (Week 5-6)
1. Stand up an **APIM GenAI gateway** — token quotas, metering, semantic cache, backend pools with PTU + spillover
2. Architect **multi-region failover** and a **multi-tenant SaaS** posture (gateway + isolation + metering)
3. Implement defense-in-depth **Responsible AI** — Prompt Shields, groundedness, red-teaming, the identify→measure→mitigate→operate loop
4. Drill the **Interview Questions** until you can answer the capacity, security, and RAG questions cold

---

## 🔑 Key Concepts at a Glance

| Concept | One-liner |
|---|---|
| **Deployment-name indirection** | Code calls a deployment name; swap model/version behind it without code changes |
| **PTU vs PAYG** | Dedicated reserved throughput (predictable) vs per-token shared capacity (elastic); hybrid = PTU baseline + PAYG spillover |
| **TPM / RPM** | Tokens- and requests-per-minute quotas on Standard; exceed → 429 (honor `Retry-After`) |
| **Hybrid + semantic ranker** | Vector + BM25 fused, then cross-encoder rerank — the default Azure RAG retrieval |
| **HNSW** | Graph-based ANN vector index; tune `m` / `efConstruction` / `efSearch` (recall vs latency vs memory) |
| **Security trimming** | Filterable group/tenant fields + `$filter` on user claims → no cross-tenant data leakage |
| **Cognitive Services OpenAI User** | The data-plane RBAC role for inference (managed identity, no keys) |
| **Prompt Shields** | Detect direct jailbreaks **and** indirect (document-embedded) prompt injection |
| **APIM GenAI gateway** | Central control plane: token quotas, metering, semantic cache, content safety, backend failover |
| **Azure OpenAI vs Foundry** | Model-serving service vs the end-to-end build/evaluate/operate platform |

---

## 💡 When to Use What

| Need | Reach for |
|---|---|
| Steady, latency-sensitive production volume | **PTU** (+ PAYG spillover for bursts) |
| Variable / dev / unpredictable traffic | **PAYG (Standard)** |
| Bulk embedding / offline processing | **Batch API** (~50% cheaper, separate quota) |
| Enterprise "chat over your data" retrieval | **Azure AI Search** (hybrid + semantic ranker) |
| Fastest grounded chatbot with citations | Azure OpenAI **On Your Data** |
| Full control over reranking/prompting/multi-hop | **Custom** AI Search + Azure OpenAI pipeline |
| Many teams sharing AOAI capacity | **APIM GenAI gateway** (quota, metering, failover) |
| Managed, stateful agents with built-in tools | **Foundry Agent Service** *(verify current GA status)* |
| Embeddable, in-process agent orchestration (.NET) | **Semantic Kernel / Microsoft Agent Framework** |

---

## 🔗 Related Sections

- **[03-Azure](../03-Azure/README.md)** — generic Azure foundation (identity, networking, RBAC, monitoring) this section builds on
- **System Design** — where Azure AI Search, the APIM gateway, and Azure OpenAI sit in an end-to-end RAG/agent architecture
- **Semantic Kernel / AI Agents / RAG Systems** — deeper treatment of orchestration, agents, and retrieval that consume these services

---

> *Note: model names, GA status, and feature naming in the Azure AI space move fast. Items flagged "(verify current GA status)" should be confirmed against current Microsoft docs at interview time. Currency: mid-2026.*

*Make Azure OpenAI secure, governed, cost-efficient, reliable, and observable — that's the senior-engineer differentiator.* ☁️🤖
