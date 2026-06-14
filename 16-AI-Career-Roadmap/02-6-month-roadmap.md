# 6-Month Roadmap — .NET Engineer → Microsoft AI Solutions Architect

> **Candidate:** Vamsi Raavala — 14 yrs, Senior .NET/React/Azure engineer (C#, ASP.NET Core, microservices, AKS, Service Bus, EF Core, React/TS, CI/CD, DDD). **Target (12 months):** Microsoft-centric **AI Solutions Developer → AI Solutions Architect**. This roadmap covers the **first 6 months** — the intensive build phase that gets you interview-ready. Months 7–12 are the architect ramp (deepen P2/P5/P6, AZ-305, lead an initiative). Authored mid-2026.

---

## ⚖️ Why 6 Months, Not 3 (and How to Compress Anyway)

The 5 phases are **not equal-weight.** Phases 1–2 (foundations + Microsoft AI dev) are genuinely learnable in ~6–8 weeks for someone with your background — you already understand APIs, async, distributed systems, and SDK ergonomics. The slope gets steep in **Phase 3 (agentic systems)** and near-vertical in **Phases 4–5 (enterprise architecture + governance)**. Architect-level depth is not "knowing the API surface" — it's having *opinions backed by scar tissue*: when RAG beats fine-tuning, why your token costs exploded, how a multi-agent loop went rogue, what an eval harness caught that manual testing missed. That scar tissue only accumulates by shipping and breaking things over time. You cannot speed-run failure modes.

**The math.** At 10–15 hrs/week you have ~260–390 hours over 6 months. The 6 portfolio projects (each a real, deployable artifact with evals and a cost story) eat the majority of that. Compressing to 3 months at the same weekly pace halves your build time to ~130–195 hours — enough to *demo* the projects but not to develop the architectural judgment that separates "I followed a tutorial" from "I designed this, and here's why."

### How to compress to 3 months (full-time, ~40 hrs/week)

| 6-month month | 3-month full-time equivalent |
|---|---|
| Months 1–2 (Phase 1 + start Phase 2) | **Weeks 1–3** |
| Months 2–3 (Phase 2 RAG + start Phase 3) | **Weeks 4–6** |
| Months 3–4 (Phase 3 agents) | **Weeks 7–8** |
| Month 5 (Phase 4 enterprise) | **Weeks 9–10** |
| Month 6 (Phase 5 architect) | **Weeks 11–12** |

**Full-time compression rules:** (1) Do not skip the evals/observability work — that's the architect signal; cut tutorial-watching instead. (2) Run projects in parallel rather than sequentially (RAG Platform and Agent Workflow can overlap). (3) Replace "read the book" with "skim the book, build the lab, read the section when the lab breaks." (4) Keep one full week (11–12) for governance/strategy — least code-heavy, most interview-leverage per hour.

### Phase → Month → Repo-section map

| Phase | Spans | Companion deep-dive section |
|---|---|---|
| Phase 1 — AI Foundations | Month 1 (+ tail into M2) | [08-AI-Foundations](../08-AI-Foundations/), [09-Azure-AI](../09-Azure-AI/) |
| Phase 2 — Microsoft AI Development | Months 2–3 | [10-Semantic-Kernel](../10-Semantic-Kernel/), [12-RAG-Systems](../12-RAG-Systems/) |
| Phase 3 — Agentic AI | Months 3–4 | [11-AI-Agents](../11-AI-Agents/) |
| Phase 4 — Enterprise AI Architecture | Month 5 | [13-AI-System-Design](../13-AI-System-Design/), [14-AI-Frontend](../14-AI-Frontend/) |
| Phase 5 — AI Architect Level | Month 6 | [13-AI-System-Design](../13-AI-System-Design/), [15-AI-Portfolio-Projects](../15-AI-Portfolio-Projects/) |

### The 6 portfolio projects (referenced throughout)

Detailed build specs live in [15-AI-Portfolio-Projects](../15-AI-Portfolio-Projects/). They thread through the months below.

| # | Project | Built in |
|---|---|---|
| P1 | RAG over enterprise docs | Month 3 |
| P2 | Multi-agent orchestrator | Month 4 |
| P3 | Production LLM API service | Months 1, 5 |
| P4 | AI-powered React/TS frontend | Months 1, 3 |
| P5 | LLMOps & evaluation harness | Months 1–6 (incremental) |
| P6 | Responsible AI / governance layer | Month 6 |

---

## 📅 Month 1 — AI Foundations (Phase 1)

**Theme:** Stop treating the LLM as a black box. Build the mental model + ship your first real Azure OpenAI app. → deep-dive [08-AI-Foundations](../08-AI-Foundations/), [09-Azure-AI](../09-Azure-AI/).

### Topics
- LLM fundamentals: tokens, embeddings, context windows, temperature/top-p, sampling, why models hallucinate, pretraining vs instruction-tuning vs RLHF.
- OpenAI APIs vs **Azure OpenAI** (deployment model, regional capacity, content filters, PTUs vs pay-as-you-go).
- Prompt engineering: system/user/assistant roles, few-shot, chain-of-thought, structured output / JSON mode, function-calling basics.
- **Context engineering:** what goes in the window, retrieval vs recitation, context compression, the "lost in the middle" problem, managing conversation state.

### Learning Objectives
- Explain token economics and estimate cost/latency for a workload from first principles.
- Call Azure OpenAI from C# (`Azure.AI.OpenAI`) with streaming, structured outputs, and tool/function calling.
- Write prompts that are testable and version-controlled, not vibes.
- Articulate when prompting alone is insufficient and you need RAG or fine-tuning.

### Daily Study Plan
- **Weekdays (1.5–2 hrs):** 30 min reading/Learn module, 60–90 min hands-on coding against Azure OpenAI. Keep a running `prompts.md` log.
- **Weekends (4–5 hrs/day):** longer build block. Saturday = feature build; Sunday = refactor + notes + cost estimate.
- **Rhythm:** every Friday, write a 5-bullet "what I learned / what surprised me" — interview ammunition.

### Hands-On Labs
1. **Lab 1.1 — Token & cost calculator:** C# console tool that tokenizes input (`Tiktoken`/`SharpToken`) and estimates $ for a model + volume. *(Builds cost intuition early — pays off in Month 6.)*
2. **Lab 1.2 — Streaming chat console:** Azure OpenAI streaming completion with conversation history + token-budget trimming.
3. **Lab 1.3 — Structured extraction:** force JSON-schema output from unstructured text; handle malformed output gracefully.
4. **Lab 1.4 — Prompt eval harness (mini):** 10 test prompts with expected-property assertions, run as an xUnit suite. *(P5 seed.)*

### Deliverables
- **P3/P4 kickoff — AI Chat Assistant (v0):** ASP.NET Core + React/TS chat app over Azure OpenAI with streaming, system-prompt config, and conversation memory. Deployed to App Service or a container.
- `prompts.md` + the mini eval suite committed to the repo.

### Milestones
- ☐ Azure OpenAI resource provisioned; you can deploy/swap models confidently.
- ☐ AI Chat Assistant v0 live and streaming.
- ☐ You can whiteboard "what is a token, why context windows matter, how cost scales" cold.

---

## 📅 Month 2 — Microsoft AI Development: Semantic Kernel + AI Foundry (Phase 2, part 1)

**Theme:** Move from raw API calls to the Microsoft orchestration stack — the platform an architect standardizes on. → deep-dive [10-Semantic-Kernel](../10-Semantic-Kernel/).

### Topics
- **Semantic Kernel (SK):** kernel, plugins/functions, prompt vs native functions, planners (and why auto-planners fell out of favor vs function-calling), filters, memory connectors. (.NET-first — your strength.)
- **Azure AI Foundry:** projects, model catalog, deployments, prompt flow, evaluation, the Foundry SDK. *Flag: Foundry moves fast — confirm GA status of the specific SDK/Agent features you depend on before committing an architecture (verify current GA status).*
- Embeddings deep-dive: models, dimensions, similarity metrics, chunking strategies.
- SK vs raw SDK vs Foundry Agent Service — when to use which.

### Learning Objectives
- Build an SK app where business logic lives in plugins and the LLM orchestrates.
- Use Foundry for model deployment + run a basic evaluation in the portal.
- Choose chunking + embedding strategy for a given document type with reasons.

### Daily Study Plan
- **Weekdays (1.5–2 hrs):** alternate SK days (code) and Foundry days (portal + SDK).
- **Weekends (4–5 hrs):** migrate the Chat Assistant from raw SDK to Semantic Kernel; add 2–3 plugins.
- Keep an **ADR** per significant choice — a direct architect-interview artifact.

### Hands-On Labs
1. **Lab 2.1 — Refactor Chat Assistant onto SK:** replace direct calls with a kernel + chat-completion service.
2. **Lab 2.2 — Native + prompt plugins:** add a "current-time" native plugin and a "summarize" prompt plugin; let function-calling pick.
3. **Lab 2.3 — SK filters:** logging filter + PII-redaction filter (foreshadows Month 5 security/observability).
4. **Lab 2.4 — Foundry eval:** deploy a model, build a small eval dataset, run groundedness/relevance metrics. *(P5.)*

### Deliverables
- **AI Chat Assistant (v1):** SK-powered, plugin-based, with logging + redaction filters. The foundation everything else extends.
- First 3–4 ADRs (model choice, SK vs raw SDK, chunking strategy).

### Milestones
- ☐ Comfortable in Semantic Kernel and the Foundry portal/SDK.
- ☐ Chat Assistant v1 demonstrates plugin orchestration + filters.
- ☐ You can defend "why SK over LangChain/raw SDK" in an interview.

---

## 📅 Month 3 — RAG & Vector Search → First Agents (Phase 2 part 2 + Phase 3 start)

**Theme:** Build the single most-asked-about enterprise pattern (RAG), then take the first step into agents. → deep-dive [12-RAG-Systems](../12-RAG-Systems/).

### Topics
- **RAG end to end:** ingestion → chunking → embedding → indexing → retrieval → re-ranking → grounded generation → citation.
- **Azure AI Search:** vector + hybrid search, semantic ranker, integrated vectorization, indexers, "import and vectorize data."
- Vector search fundamentals: ANN, HNSW, recall vs latency, metadata filtering.
- RAG evaluation: groundedness, relevance, retrieval precision/recall; the "RAG triad."
- **Phase 3 on-ramp:** tool calling as the bridge from RAG to agents; "agent" vs "pipeline."

### Learning Objectives
- Stand up a production-shaped RAG pipeline on Azure AI Search with hybrid + semantic ranking.
- Measure RAG quality quantitatively, not anecdotally.
- Diagnose the top RAG failure modes (bad chunking, missing re-rank, context overflow, ungrounded answers).

### Daily Study Plan
- **Weekdays (2 hrs):** Mon–Wed RAG pipeline build; Thu–Fri evaluation + tuning.
- **Weekends (5 hrs):** ingestion + index design Saturday; eval harness + re-ranking experiments Sunday.
- Track retrieval metrics in a spreadsheet each weekend — show the *trend* in your portfolio writeup.

### Hands-On Labs
1. **Lab 3.1 — Document ingestion pipeline:** chunk + embed a real corpus into Azure AI Search.
2. **Lab 3.2 — Hybrid + semantic retrieval:** compare keyword-only, vector-only, hybrid, and hybrid+semantic-ranker on the same queries.
3. **Lab 3.3 — Grounded generation with citations:** SK orchestrates retrieval → answer with inline citations + "I don't know" guardrail.
4. **Lab 3.4 — RAG eval harness:** automated groundedness/relevance scoring (Foundry evaluators or LLM-as-judge). *(P5.)*
5. **Lab 3.5 — First tool-calling agent:** single-agent SK app that decides between "search the index" and "answer directly."

### Deliverables
- **P1 — RAG Platform:** ingestion service, Azure AI Search index, SK retrieval/answer API, React UI with citations (P4 extension), and a documented eval harness with metric trends. Deployed to AKS (play to your strength).
- A short "RAG design writeup" covering chunking decisions, hybrid vs vector tradeoffs, and eval results.

### Milestones
- ☐ RAG Platform live with measurable groundedness/relevance scores.
- ☐ You can explain hybrid search + semantic ranker and *when each earns its cost*.
- ☐ First tool-calling agent working — Phase 3 has begun.

---

## 📅 Month 4 — Agentic AI & Multi-Agent Systems (Phase 3)

**Theme:** The frontier. Single agents → orchestrated multi-agent systems with humans in the loop. Where senior candidates separate themselves. → deep-dive [11-AI-Agents](../11-AI-Agents/).

### Topics
- **Microsoft Agent Framework** — the converged stack unifying the **AutoGen** research line and **Semantic Kernel Agents**. *Flag: this space moved fast in 2025–2026 — Agent Framework superseded/absorbed standalone AutoGen and SK Agent Framework messaging; verify current GA naming and the recommended SDK before locking architecture. Learn the concepts, stay loose on the brand (verify current GA status).*
- **AutoGen** patterns (conversable agents, group chat) — still the best mental model for multi-agent conversation.
- **Semantic Kernel Agents** — agent abstraction over SK; agent threads, agent-as-tool.
- **Multi-agent topologies:** orchestrator/worker, sequential, group chat, hierarchical. When multi-agent beats a single well-prompted agent (and the frequent case where it doesn't).
- **MCP & tool calling at scale:** tool schemas, parallel tool calls, error/retry, tool-result grounding, building/consuming MCP servers.
- **Human-in-the-loop (HITL):** approval gates, interrupt/resume, confidence thresholds, escalation.
- Agent failure modes: loops, runaway cost, hallucinated tool args, context bloat.

### Learning Objectives
- Build a multi-agent system with a clear orchestration topology and a reason for it.
- Implement HITL approval gates and durable interrupt/resume.
- Put hard guardrails on agent loops (max-steps, cost ceilings, timeouts).

### Daily Study Plan
- **Weekdays (2 hrs):** Mon–Tue single-agent depth (tools, retries); Wed–Thu multi-agent orchestration; Fri HITL + guardrails.
- **Weekends (5 hrs):** build the agent workflow Saturday; chaos-test it (force failures, watch loops) Sunday — *failure logs are gold for interviews.*
- Maintain a "things that went wrong" doc — runaway loops, cost spikes, bad tool calls.

### Hands-On Labs
1. **Lab 4.1 — Multi-tool single agent:** 4–5 tools (search, calculator, API caller, your RAG index) + parallel tool calls.
2. **Lab 4.2 — Sequential agent workflow:** research → draft → review pipeline with handoffs.
3. **Lab 4.3 — Group-chat multi-agent:** orchestrator + specialists (researcher, writer, critic) solving a task collaboratively.
4. **Lab 4.4 — HITL approval gate:** agent pauses for human approval before a "destructive" tool call; durable resume.
5. **Lab 4.5 — Guardrails:** max-step circuit breaker, per-run token/cost ceiling, timeout, structured failure logging.

### Deliverables
- **P2 — Multi-Agent Orchestrator:** orchestrator + ≥3 specialist agents via Agent Framework/SK Agents, with HITL gates and guardrails, deployed on AKS. Builds on the RAG Platform as one of its tools. Documented topology + a written justification for "why multi-agent here."
- A "multi-agent design decisions" ADR set + the failure-modes doc.

### Milestones
- ☐ Multi-Agent Orchestrator completes a non-trivial task end-to-end.
- ☐ HITL + guardrails demonstrably prevent runaway loops/cost.
- ☐ You can argue *both sides* of "single agent vs multi-agent" with concrete tradeoffs — a top architect-interview signal.

---

## 📅 Month 5 — Enterprise AI Architecture (Phase 4)

**Theme:** Your home turf, AI-flavored. Apply everything you know about distributed systems to the unique pain of AI workloads. → deep-dive [13-AI-System-Design](../13-AI-System-Design/), front-end depth in [14-AI-Frontend](../14-AI-Frontend/).

### Topics
- **AI microservices & event-driven AI:** decomposing AI capabilities into services, async inference via **Service Bus**/queues (your existing skill!), backpressure, the "long-running, expensive, non-deterministic call" problem.
- **Distributed AI patterns:** semantic caching, request batching, fallback model chains, retry-with-downgrade, circuit breakers for token budgets, the gateway/router pattern.
- **Security for AI:** prompt injection (an OWASP-LLM-class threat), data exfiltration via tools, secrets in prompts, RBAC + managed identity for Azure OpenAI/Search, network isolation (private endpoints), content filtering, the **Azure API Management** AI gateway / token-limiting policies.
- **Observability for AI:** tracing LLM/agent calls (OpenTelemetry + GenAI semantic conventions), token/latency/cost telemetry, eval-in-production, prompt/version tracking, App Insights for AI, **Foundry tracing**.

### Learning Objectives
- Design an AI system that degrades gracefully under cost, latency, and failure pressure.
- Threat-model an AI app and apply concrete mitigations (esp. prompt injection + tool abuse).
- Instrument an agent/RAG system so you can answer "what did it cost, why was it slow, was the answer grounded" from telemetry.

### Daily Study Plan
- **Weekdays (2 hrs):** Mon resilience patterns, Tue gateway/caching, Wed security/threat-model, Thu observability, Fri integrate.
- **Weekends (5 hrs):** Saturday build the AI gateway + caching; Sunday instrument tracing + run a prompt-injection red-team against your own app.

### Hands-On Labs
1. **Lab 5.1 — AI gateway:** APIM (or a custom gateway) in front of your Azure OpenAI deployments with token-limit + load-balancing policies.
2. **Lab 5.2 — Semantic cache + fallback chain:** cache semantically-similar requests; on rate-limit/failure, fall back to a cheaper model.
3. **Lab 5.3 — Async inference via Service Bus:** offload long agent runs to a queue + worker; client polls/streams results.
4. **Lab 5.4 — Prompt-injection red team:** attack your RAG/agent app; document successful injections + mitigations (input/output filtering, tool allow-lists, isolation). *(P6 seed.)*
5. **Lab 5.5 — Full observability:** OpenTelemetry tracing across the agent flow + cost/token/latency dashboards in App Insights/Foundry. *(P5.)*

### Deliverables
- **P3 hardened → Enterprise Copilot:** a production-shaped copilot combining RAG + agents behind an AI gateway, with security mitigations, semantic caching, async inference, and full observability. *This is the project that reads as senior.*
- A **threat-model document** (STRIDE-style, AI-specific) + an observability dashboard screenshot set.

### Milestones
- ☐ Enterprise Copilot runs behind a gateway with caching + fallback.
- ☐ You have a documented prompt-injection attack and its mitigation.
- ☐ End-to-end tracing answers cost/latency/groundedness questions.
- ☐ You can lead an AI-systems design interview — the inflection point to "Architect."

---

## 📅 Month 6 — AI Architect Level: Governance, Strategy & Platform (Phase 5)

**Theme:** Zoom out from "I build AI systems" to "I set the standards, platform, and strategy for an org's AI." Least code, highest interview leverage. → capstone in [15-AI-Portfolio-Projects](../15-AI-Portfolio-Projects/); architecture framing in [13-AI-System-Design](../13-AI-System-Design/).

### Topics
- **AI governance & Responsible AI:** Microsoft's Responsible AI Standard (fairness, reliability/safety, privacy/security, inclusiveness, transparency, accountability), content safety, the **Azure AI Content Safety** service, model/use-case risk assessment, **EU AI Act** risk tiers.
- **Enterprise platform design:** the "AI platform team" model — shared gateway, model catalog, prompt registry, eval-as-a-service, guardrail libraries, golden-path templates, multi-team tenancy, FinOps for AI.
- **Cost optimization:** PTU vs pay-as-you-go framework, prompt/context compression, caching tiers, model right-sizing (route easy queries to small models), batch APIs, chargeback.
- **AI strategy:** build-vs-buy, hosted copilot vs custom, the AI maturity model, roadmapping AI for an org, measuring ROI, change management.

### Learning Objectives
- Produce a Responsible AI assessment for a real use case + mitigations + an approval workflow.
- Design a reusable enterprise AI platform (the "paved road" other teams build on).
- Build a defensible cost-optimization + FinOps story.
- Pitch an AI strategy to a non-technical exec audience.

### Daily Study Plan
- **Weekdays (1.5–2 hrs):** Mon–Tue governance/RAI, Wed cost/FinOps, Thu–Fri platform design + writing.
- **Weekends (4–5 hrs):** Saturday build the platform capstone integration; Sunday *write* — strategy doc, architecture diagrams, rehearse the exec pitch out loud.
- This month, **writing and diagramming > coding.** Produce architect artifacts.

### Hands-On Labs
1. **Lab 6.1 — Content Safety integration:** wire Azure AI Content Safety into the Enterprise Copilot (input + output moderation, severity thresholds). *(P6.)*
2. **Lab 6.2 — Prompt/eval registry:** a shared service/repo where prompts are versioned and every prompt has an attached eval suite. *(P5/P6.)*
3. **Lab 6.3 — Model router for cost:** route by query complexity to small/large models; measure cost savings vs quality delta.
4. **Lab 6.4 — Governance gate:** an approval workflow + RAI checklist any new AI use case must pass. *(P6.)*
5. **Lab 6.5 — FinOps dashboard:** per-team/per-feature token + cost attribution.

### Deliverables
- **P6 capstone → Enterprise AI Platform:** shared AI gateway, model catalog/router, prompt+eval registry, guardrail/content-safety library, observability + FinOps dashboards, and a golden-path template repo other teams could clone. Your *architect-level* artifact.
- **AI Strategy & Governance deck (10–15 slides):** RAI standard applied to a use case, platform architecture, cost-optimization framework, org AI roadmap — your interview centerpiece.
- A polished portfolio README tying all 6 projects into one narrative (see [04-resume-and-linkedin.md](04-resume-and-linkedin.md)).

### Milestones
- ☐ Enterprise AI Platform demonstrable as a "paved road."
- ☐ You can run a Responsible AI assessment and design a governance gate.
- ☐ You can deliver a cost-optimization + AI-strategy pitch to executives.
- ☐ **You are interview-ready for AI Solutions Architect roles**, with 6 deployable projects and the judgment to defend every decision.

---

## 📊 6-Month Milestone Summary

| Month | Theme (Phase) | Key Deliverable | Interview-Readiness Unlock |
|---|---|---|---|
| **1** | AI Foundations (P1) | **AI Chat Assistant v0** — streaming Azure OpenAI app + mini eval suite | Explain LLM/token/context fundamentals & cost from first principles; call Azure OpenAI fluently in C#. |
| **2** | Microsoft AI Dev — SK + Foundry (P2a) | **AI Chat Assistant v1** — Semantic Kernel, plugins, filters, first ADRs | Defend SK vs raw SDK/LangChain; operate Foundry; orchestrate via plugins. |
| **3** | RAG & Vector Search (P2b → P3) | **RAG Platform** — Azure AI Search hybrid+semantic, citations, eval harness | Design + measure production RAG; explain hybrid/semantic ranker tradeoffs; first tool-calling agent. |
| **4** | Agentic & Multi-Agent (P3) | **Multi-Agent Orchestrator** — Agent Framework/SK Agents, HITL, guardrails | Argue single vs multi-agent with real tradeoffs; design HITL + loop/cost guardrails. |
| **5** | Enterprise AI Architecture (P4) | **Enterprise Copilot** — AI gateway, semantic cache, async inference, security, full observability | Lead an AI systems-design interview; threat-model prompt injection; instrument cost/latency/groundedness. |
| **6** | AI Architect Level (P5) | **Enterprise AI Platform** + **AI Strategy & Governance deck** | Run RAI assessments & governance gates; design an enterprise AI platform; pitch cost + strategy — **Architect-ready.** |

---

### ⚠️ A note on the fast-moving stack (mid-2026)

The Microsoft agent tooling consolidated rapidly through 2025–2026 (the AutoGen research line and Semantic Kernel agents converging into the **Microsoft Agent Framework**, and **Azure AI Foundry** continuing to absorb model catalog, agent service, evaluation, and tracing). **Learn the concepts deeply and stay loose on brand names and exact SDK surfaces** — before committing an architecture to any preview feature, confirm its current GA status (verify ...). As an architect, your value isn't knowing today's API; it's knowing the durable patterns (RAG, orchestration topologies, guardrails, evals, gateways, governance) that survive every rename.
