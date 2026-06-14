# Job-Readiness Scorecard

> **Candidate:** Vamsi — 14 years, Senior .NET / React / Azure engineer (microservices, AKS, Service Bus, DDD, CI/CD, React/TS). **AI maturity today:** early — learning LLM/RAG/agentic AI, no production AI shipped yet. **Goal:** Microsoft-centric AI Solutions Developer → AI Solutions Architect within 12 months.

---

## How to read this scorecard

An honest, opinionated assessment — not a feel-good one. Two scores per role:

- **Current Score** — what you'd score *today*, with zero AI portfolio and zero AI in production.
- **Target Score** — what you'll score after closing the named gaps (AI layer + 2–3 portfolio projects + 1–2 certs).

**The single most important insight:** you are not a junior anything. Your 14 years of architecture, distributed systems, Azure, and stakeholder communication are *exactly* the scarce half of an AI engineering hire. The AI-specific layer (LLM/RAG/agents/eval) is the cheap half to acquire for someone like you — roughly a 3–6 month skill, not a 5-year one. That asymmetry is why your readiness for **AI Solutions Developer**, **Senior AI Engineer**, and even **AI Architect** ramps *fast*, while targeting **Junior AI Engineer** would be actively underleveling.

### The 6 portfolio projects (referenced throughout)

Build specs in [15-AI-Portfolio-Projects](../15-AI-Portfolio-Projects/); month-by-month in [02-6-month-roadmap.md](02-6-month-roadmap.md).

1. **P1 — RAG over enterprise docs** (Azure AI Search + Azure OpenAI, .NET orchestration, citations, chunking)
2. **P2 — Agentic / multi-agent orchestrator** (tool-calling, planner-executor, Semantic Kernel or AutoGen)
3. **P3 — Production LLM API service** (.NET minimal API, streaming, token/cost guards, caching, observability)
4. **P4 — AI-powered React/TS frontend** (chat UX, streaming, citations, latency/error states, eval feedback loop)
5. **P5 — LLMOps & evaluation harness** (offline eval sets, LLM-as-judge, regression gates in CI/CD, tracing)
6. **P6 — Responsible AI / governance layer** (content safety, PII redaction, prompt-injection defense, audit logging, groundedness checks)

---

## 📊 Summary table

| Role | Current Score | Target Score | Time to Target | Verdict |
|---|---|---|---|---|
| Junior AI Engineer | 71 | 92 | ~1 month | **Do NOT target — underleveling. You're already past it.** |
| Mid-Level AI Engineer | 64 | 90 | ~3 months | Viable floor, but still slightly under your level. Use as fallback. |
| Senior AI Engineer | 58 | 88 | ~5 months | **Strong target. Your engineering depth carries this.** |
| AI Solutions Developer | 61 | 91 | ~4 months | **Primary near-term target. Best fit-to-effort ratio.** |
| AI Architect (AI Solutions Architect) | 54 | 87 | ~9–11 months | **The 12-month destination. Already realistic, not aspirational.** |

**Read this row first:** your *Current* scores rise as the role gets more senior relative to the market's expectation gap — counterintuitive, but real. A junior role scores you 71 because juniors are *expected* to know the AI stack and you partly don't yet; an architect role scores you 54 today only because it demands the AI layer you haven't shipped — but every other architect dimension you already own outright. Close the AI layer and the senior/architect targets clear 87+ because nobody else in the pool has your distributed-systems and Azure depth.

---

### Junior AI Engineer

Expected to: write Python AI glue code, call LLM APIs, build simple RAG, follow patterns set by seniors. Low system-design and leadership bar.

| Dimension | Weight | Current (0–100) | Target | Gap |
|---|---|---|---|---|
| Programming / .NET (general SWE) | 15% | 95 | 95 | 0 |
| Python AI stack | 15% | 45 | 80 | 35 |
| LLM fundamentals | 15% | 50 | 85 | 35 |
| Prompt / context engineering | 10% | 55 | 85 | 30 |
| RAG | 10% | 45 | 85 | 40 |
| Agentic / multi-agent | 5% | 30 | 70 | 40 |
| Azure AI services | 10% | 70 | 90 | 20 |
| System design | 5% | 95 | 95 | 0 |
| Frontend / AI UX | 5% | 90 | 92 | 2 |
| MLOps / LLMOps & eval | 5% | 40 | 80 | 40 |
| Responsible AI / governance | 2.5% | 45 | 80 | 35 |
| Communication / leadership | 2.5% | 95 | 95 | 0 |

**Weighted (Current):** `0.15·95 + 0.15·45 + 0.15·50 + 0.10·55 + 0.10·45 + 0.05·30 + 0.10·70 + 0.05·95 + 0.05·90 + 0.05·40 + 0.025·45 + 0.025·95 = 14.25 + 6.75 + 7.50 + 5.50 + 4.50 + 1.50 + 7.00 + 4.75 + 4.50 + 2.00 + 1.125 + 2.375` = **≈ 71**

**Weighted (Target):** `14.25 + 12.00 + 12.75 + 8.50 + 8.50 + 3.50 + 9.00 + 4.75 + 4.60 + 4.00 + 2.00 + 2.375` = **≈ 92**

> **Current: 71 · Target: 92**

**Missing skills:** comfortable Python (typing, async, packaging), `openai`/`azure-ai` SDK fluency, basic RAG chunking/embeddings, vector store basics.
**Required projects:** P1 (RAG) alone gets you well past the bar.
**Required certifications:** none strictly needed. AI-900 only if you want a checkbox (and you don't — see [03-certifications.md](03-certifications.md)).
**Timeline to target:** ~1 month.
**Verdict — do NOT target.** You clear 71 *cold.* A junior title caps comp and forces you to relearn architecture you teach. This row exists only to prove you're over-qualified the day you start.

---

### Mid-Level AI Engineer

Expected to: own features end-to-end, build production RAG and simple agents, integrate eval, handle some ambiguity.

| Dimension | Weight | Current (0–100) | Target | Gap |
|---|---|---|---|---|
| Programming / .NET (general SWE) | 15% | 95 | 95 | 0 |
| Python AI stack | 12.5% | 45 | 82 | 37 |
| LLM fundamentals | 15% | 50 | 85 | 35 |
| Prompt / context engineering | 10% | 55 | 88 | 33 |
| RAG | 12.5% | 45 | 88 | 43 |
| Agentic / multi-agent | 7.5% | 30 | 78 | 48 |
| Azure AI services | 10% | 70 | 90 | 20 |
| System design | 7.5% | 95 | 95 | 0 |
| Frontend / AI UX | 2.5% | 90 | 92 | 2 |
| MLOps / LLMOps & eval | 5% | 40 | 82 | 42 |
| Responsible AI / governance | 2.5% | 45 | 82 | 37 |

*(Weights sum to 100% across the 11 active dimensions; communication/leadership folded into the seniority bar.)*

**Weighted (Current):** `0.15·95 + 0.125·45 + 0.15·50 + 0.10·55 + 0.125·45 + 0.075·30 + 0.10·70 + 0.075·95 + 0.025·90 + 0.05·40 + 0.025·45 = 14.25 + 5.625 + 7.50 + 5.50 + 5.625 + 2.25 + 7.00 + 7.125 + 2.25 + 2.00 + 1.125` = **≈ 64**

**Weighted (Target):** `14.25 + 10.25 + 12.75 + 8.80 + 11.00 + 5.85 + 9.00 + 7.125 + 2.30 + 4.10 + 2.05` = **≈ 90**

> **Current: 64 · Target: 90**

**Missing skills:** production RAG (re-ranking, hybrid search, eval on retrieval quality), tool-calling agents, structured eval, cost/latency tuning.
**Required projects:** P1 + P3 + a slice of P5.
**Required certifications:** AI-102.
**Timeline to target:** ~3 months.
**Verdict:** viable as a *floor*/fallback, but still slightly below your level. Don't anchor here — your architecture depth is wasted at "mid."

---

### Senior AI Engineer

Expected to: design AI systems, own production reliability/cost, set patterns, handle agentic complexity, drive eval discipline.

| Dimension | Weight | Current (0–100) | Target | Gap |
|---|---|---|---|---|
| Programming / .NET (general SWE) | 12.5% | 95 | 95 | 0 |
| Python AI stack | 10% | 45 | 82 | 37 |
| LLM fundamentals | 12.5% | 50 | 88 | 38 |
| Prompt / context engineering | 10% | 55 | 90 | 35 |
| RAG | 12.5% | 45 | 90 | 45 |
| Agentic / multi-agent | 10% | 30 | 82 | 52 |
| Azure AI services | 10% | 70 | 92 | 22 |
| System design | 10% | 95 | 95 | 0 |
| Frontend / AI UX | 2.5% | 90 | 90 | 0 |
| MLOps / LLMOps & eval | 5% | 40 | 85 | 45 |
| Responsible AI / governance | 2.5% | 45 | 85 | 40 |
| Communication / leadership | 2.5% | 95 | 95 | 0 |

**Weighted (Current):** `0.125·95 + 0.10·45 + 0.125·50 + 0.10·55 + 0.125·45 + 0.10·30 + 0.10·70 + 0.10·95 + 0.025·90 + 0.05·40 + 0.025·45 + 0.025·95 = 11.875 + 4.50 + 6.25 + 5.50 + 5.625 + 3.00 + 7.00 + 9.50 + 2.25 + 2.00 + 1.125 + 2.375` = **≈ 58**

**Weighted (Target):** `11.875 + 8.20 + 11.00 + 9.00 + 11.25 + 8.20 + 9.20 + 9.50 + 2.25 + 4.25 + 2.125 + 2.375` = **≈ 88**

> **Current: 58 · Target: 88**

**Missing skills:** multi-agent orchestration patterns, advanced RAG (query rewriting, hybrid + re-rank, groundedness eval), production LLMOps (tracing, regression gates), cost/latency SLOs for LLM systems.
**Required projects:** P1 + P2 + P3 + P5. **P2 is the differentiator** that proves seniority.
**Required certifications:** AI-102, plus AZ-305 if not already held — doubles as architect credibility.
**Timeline to target:** ~5 months.
**Verdict — strong target.** The system-design (95) and .NET (95) pillars do heavy lifting junior candidates can't match. Lift the four AI dimensions and you clear 88. A legitimate, non-aspirational landing spot inside 12 months.

---

### AI Solutions Developer (Microsoft-centric)

Expected to: build AI-powered solutions on Azure/Copilot stack end-to-end, integrate LLMs into real products, balance pragmatism + delivery. Microsoft-ecosystem fluency heavily weighted. **The role the title market rewards your exact profile for.**

| Dimension | Weight | Current (0–100) | Target | Gap |
|---|---|---|---|---|
| Programming / .NET (general SWE) | 15% | 95 | 95 | 0 |
| Python AI stack | 7.5% | 45 | 78 | 33 |
| LLM fundamentals | 12.5% | 50 | 87 | 37 |
| Prompt / context engineering | 10% | 55 | 90 | 35 |
| RAG | 12.5% | 45 | 90 | 45 |
| Agentic / multi-agent | 7.5% | 30 | 80 | 50 |
| Azure AI services | 15% | 70 | 93 | 23 |
| System design | 7.5% | 95 | 95 | 0 |
| Frontend / AI UX | 5% | 90 | 92 | 2 |
| MLOps / LLMOps & eval | 5% | 40 | 82 | 42 |
| Responsible AI / governance | 2.5% | 45 | 85 | 40 |

*(Weights sum to 100% across the 11 active dimensions; communication/leadership assumed at bar.)*

**Weighted (Current):** `0.15·95 + 0.075·45 + 0.125·50 + 0.10·55 + 0.125·45 + 0.075·30 + 0.15·70 + 0.075·95 + 0.05·90 + 0.05·40 + 0.025·45 = 14.25 + 3.375 + 6.25 + 5.50 + 5.625 + 2.25 + 10.50 + 7.125 + 4.50 + 2.00 + 1.125` = **≈ 61**

**Weighted (Target):** `14.25 + 5.85 + 10.875 + 9.00 + 11.25 + 6.00 + 13.95 + 7.125 + 4.60 + 4.10 + 2.125` = **≈ 91**

> **Current: 61 · Target: 91**

**Missing skills:** Azure OpenAI + Azure AI Search end-to-end, Semantic Kernel / Copilot extensibility, prompt+context engineering at production quality, groundedness/eval, content safety integration.
**Required projects:** P1 + P3 + P4 — this trio *is* the AI Solutions Developer portfolio. Add P6 to stand out.
**Required certifications:** AI-102 (essential). Microsoft Applied Skills credentials (e.g., agents with Azure AI Foundry / generative AI with Azure OpenAI) are high-signal and fast.
**Timeline to target:** ~4 months.
**Verdict — PRIMARY near-term target.** This role weights Azure (15%) and .NET (15%) most heavily, and you own both outright. Best fit-to-effort ratio of all five. Land here first, then promote to architect from the inside.

---

### AI Architect (AI Solutions Architect)

Expected to: own AI system architecture across multiple teams, make build/buy and platform decisions, define eval/governance standards, manage cost/risk at org scale, communicate with execs. The AI layer is *table stakes*; everything else is judgment and breadth — your strong suit.

| Dimension | Weight | Current (0–100) | Target | Gap |
|---|---|---|---|---|
| Programming / .NET (general SWE) | 7.5% | 95 | 95 | 0 |
| Python AI stack | 5% | 45 | 78 | 33 |
| LLM fundamentals | 12.5% | 50 | 88 | 38 |
| Prompt / context engineering | 7.5% | 55 | 88 | 33 |
| RAG | 10% | 45 | 90 | 45 |
| Agentic / multi-agent | 10% | 30 | 85 | 55 |
| Azure AI services | 12.5% | 70 | 93 | 23 |
| System design | 15% | 95 | 96 | 1 |
| Frontend / AI UX | 2.5% | 90 | 90 | 0 |
| MLOps / LLMOps & eval | 7.5% | 40 | 85 | 45 |
| Responsible AI / governance | 5% | 45 | 88 | 43 |
| Communication / leadership | 5% | 95 | 96 | 1 |

**Weighted (Current):** `0.075·95 + 0.05·45 + 0.125·50 + 0.075·55 + 0.10·45 + 0.10·30 + 0.125·70 + 0.15·95 + 0.025·90 + 0.075·40 + 0.05·45 + 0.05·95 = 7.125 + 2.25 + 6.25 + 4.125 + 4.50 + 3.00 + 8.75 + 14.25 + 2.25 + 3.00 + 2.25 + 4.75` = **≈ 54**

**Weighted (Target):** `7.125 + 3.90 + 11.00 + 6.60 + 9.00 + 8.50 + 11.625 + 14.40 + 2.25 + 6.375 + 4.40 + 4.80` = **≈ 87**

> **Current: 54 · Target: 87**

**Missing skills:** multi-agent system architecture and trade-offs, LLMOps at platform scale, AI governance frameworks (NIST AI RMF / Microsoft Responsible AI Standard), cost modeling for token economics, model build/buy/fine-tune decision frameworks.
**Required projects:** all six, but P2 (agentic), P5 (LLMOps/eval), and P6 (governance) are the architect-differentiators. P1/P3/P4 demonstrate hands-on credibility so you're not an "ivory-tower architect."
**Required certifications:** AI-102 + AZ-305 (the architect anchor). Optionally a Responsible AI / governance credential.
**Timeline to target:** ~9–11 months — comfortably inside your 12-month goal.
**Verdict — the destination, and it's realistic.** System Design alone contributes 14.25 of your *current* 54. You are *already* 95% of an architect; you're missing only the AI-specific 30 points.

---

## 🔑 The "don't underlevel" argument, in numbers

| | Junior | Mid | Senior | AI Sol. Dev | Architect |
|---|---|---|---|---|---|
| Current | 71 | 64 | 58 | 61 | 54 |
| Target | 92 | 90 | 88 | 91 | 87 |
| Gap to close | 21 | 26 | 30 | 30 | 33 |
| Effort (months) | 1 | 3 | 5 | 4 | 9–11 |
| Leverage of your 14 yrs | Wasted | Mostly wasted | High | High | Maximal |

The gap from Senior (30 pts) to Architect (33 pts) is only **+3 points of work** — yet the title/comp jump is enormous. Meanwhile Junior is +21 points of work for a role that *destroys* your seniority leverage. Conclusion: skip Junior and Mid as targets. Aim **AI Solutions Developer now**, **Senior AI Engineer in parallel**, **AI Architect by month 9–12**.

> **The thesis:** an architect with no AI is one course away; an AI engineer with no architecture is five years away. You are the former.

---

## 🏁 90-day fastest path to first AI offer

**Target outcome:** a signed **AI Solutions Developer** (or Senior AI Engineer) offer within 90 days, with the architect track teed up for months 4–12.

**Days 1–15 — Foundations + AI-102 prep**
- Python for AI: typing, async, `openai`/`azure-ai` SDKs. You're a 14-yr engineer; this is 2 weeks, not 2 months.
- Stand up Azure OpenAI + Azure AI Search in your own subscription. Make one "hello, RAG" call end-to-end.
- Start AI-102 study. Book the exam for ~day 30 to force the deadline.

**Days 16–40 — Ship P1 (RAG) + sit AI-102**
- Build **P1**: RAG over real enterprise docs with chunking strategy, hybrid search, citations, and a basic groundedness check. .NET orchestration so it plays to your strength.
- Pass **AI-102.** Add to LinkedIn the day it clears.
- Write a short technical post on P1's chunking/retrieval trade-offs — your senior-signal artifact.

**Days 41–65 — Ship P3 (production API) + P4 (AI UX)**
- Build **P3**: .NET minimal-API LLM service with streaming, token/cost guards, caching, observability. Where your distributed-systems depth becomes undeniable.
- Build **P4**: React/TS chat frontend with streaming, citations, latency/error states. Reuses your existing frontend strength — fast win.
- Add a thin slice of **P5**: a small offline eval set + LLM-as-judge wired into CI as a regression gate. Eval discipline is the #1 thing separating senior from mid in interviews.

**Days 66–80 — Differentiate with P2 (agentic) + start applying**
- Build **P2**: a tool-calling planner-executor agent (Semantic Kernel or AutoGen). Even a focused single-domain agent proves the agentic dimension that pushes Senior/Architect scores over the line.
- Polish all repos: READMEs with architecture diagrams, trade-off write-ups, cost/latency numbers — architecture case studies, not toy demos.
- Begin applying to **AI Solutions Developer** and **Senior AI Engineer** roles — Microsoft-stack shops first.

**Days 81–90 — Interview engine + governance signal**
- Add **P6** essentials to P1/P3 (content safety, PII redaction, prompt-injection defense, audit logging). Even partial coverage lets you speak credibly to Responsible AI — a frequent senior/architect probe.
- Rehearse the narrative: *"I'm a 14-year distributed-systems and Azure architect who has now shipped production RAG, an agentic workflow, and an LLM service with eval and governance baked in."* That sentence is worth 30 scorecard points in the room.
- Run interviews. Negotiate at Senior / Solutions-Developer level — **never** accept a Junior or plain Mid title.

**What to explicitly NOT do in 90 days:** chase deep ML theory, model training/fine-tuning math, or a data-science pivot. None of it moves your target scores; all of it burns the time your competitors *don't* have your architecture background to spare.

**Months 4–12 (the architect ramp):** deepen P2/P5/P6 into platform-grade work, take AZ-305, lead an AI initiative end-to-end at your job or new role, and convert the Solutions Developer seat into an **AI Solutions Architect** title from the inside — exactly the 12-month goal, and the numbers say it's well within reach.
