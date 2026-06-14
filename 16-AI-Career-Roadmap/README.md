# AI Career Roadmap — From Senior .NET/Azure Engineer to AI Solutions Architect

> The opinionated, Microsoft-centric playbook for a 14-year senior .NET/Azure engineer making the deliberate move into AI: AI Solutions Developer first, AI Solutions Architect within 12 months. Built around a *thin-but-deep* AI layer bolted onto an already strong enterprise foundation. Currency: **mid-2026** — fast-moving cert codes and GA features are flagged "(verify ...)".

This is not a "break into tech" guide. You already operate at the architecture and delivery level. Roughly **60–70%** of what these roles require, you already do. The remaining 30–40% — LLM, RAG, agents, eval, Responsible AI — is the cheap half to acquire for someone like you. This section tells you exactly what to learn, in what order, what to certify, how to position yourself, and how to know you're ready.

---

## 📚 Table of Contents

### Diagnosis

1. **[Skills Gap Analysis](01-skills-gap-analysis.md)** — Strong / Transferable / Missing / Critical skill tables with importance, learning difficulty, time-to-productive, and time-to-interview-ready; the single biggest gap (eval & grounding) and your single biggest advantage (production engineering)

### The Plan

2. **[6-Month Roadmap](02-6-month-roadmap.md)** — Month-by-month across the 5 phases, daily study plans, hands-on labs, deliverables, milestones, and the 6 portfolio projects; references repo sections 08–15
3. **[Certifications](03-certifications.md)** — ROI-ranked cert strategy (AI-102, AZ-305, DP-600, AZ-204, DP-203, AI-900), the order to take them, recruiter value vs architect value, and the "don't over-invest" rule

### Positioning

4. **[Resume & LinkedIn](04-resume-and-linkedin.md)** — Positioning strategy, rewritten AI summary and bullets, 2 summary variants, 5 LinkedIn headlines, full About section, GitHub + portfolio strategy, and an ATS keyword table
5. **[Job-Readiness Scorecard](05-job-readiness-scorecard.md)** — 0–100 weighted scorecards for Junior / Mid / Senior AI Engineer, AI Solutions Developer, and AI Architect; current vs target, gaps, required projects/certs, timelines, and the "don't underlevel" argument in numbers

### Fuel

6. **[Learning Resources](06-learning-resources.md)** — Curated YouTube channels (Foundations / Microsoft stack / AI engineering / News), Microsoft Learn paths, the two flagship GitHub courses, and a channel-to-phase map

### Certification Prep

7. **[AI-103 Exam Prep](07-ai-103-exam-prep.md)** — *Developing AI Apps and Agents on Azure* (successor to AI-102): exam overview, skills-measured breakdown, a 6–8 week prep path mapped to repo sections 08–13, and 32 sample questions with answers

---

## 🎯 The 12-Month Plan at a Glance

The 6-month build (section 02) is the intensive phase; months 7–12 are the architect ramp. Each phase leans on a deep-dive section elsewhere in this repo.

| Phase | Months | Focus | Outcome | Repo deep-dive |
|---|---|---|---|---|
| **1 — AI Foundations** | M1 | LLM fundamentals, prompting, context engineering, Azure OpenAI from C# | AI Chat Assistant v0; explain tokens/cost/context cold | [08-AI-Foundations](../08-AI-Foundations/) · [09-Azure-AI](../09-Azure-AI/) |
| **2 — Microsoft AI Dev** | M2–M3 | Semantic Kernel, Azure AI Foundry, RAG, Azure AI Search | RAG Platform with eval harness; SK-powered assistant | [10-Semantic-Kernel](../10-Semantic-Kernel/) · [12-RAG-Systems](../12-RAG-Systems/) |
| **3 — Agentic AI** | M3–M4 | Microsoft Agent Framework, multi-agent topologies, HITL, MCP | Agent Workflow + Multi-Agent Assistant | [11-AI-Agents](../11-AI-Agents/) |
| **4 — Enterprise AI Architecture** | M5 | AI gateway, semantic cache, async inference, security, observability | Enterprise Copilot (production-shaped) | [13-AI-System-Design](../13-AI-System-Design/) · [14-AI-Frontend](../14-AI-Frontend/) |
| **5 — AI Architect Level** | M6 | Responsible AI, governance, FinOps, platform & strategy | Enterprise AI Platform + strategy deck — Architect-ready | [13-AI-System-Design](../13-AI-System-Design/) · [15-AI-Portfolio-Projects](../15-AI-Portfolio-Projects/) |
| **Architect ramp** | M7–M12 | Deepen P2/P5/P6 to platform scale; AZ-305; lead an AI initiative | AI Solutions Architect title from the inside | all of 08–15 |

> The companion sections above (08-AI-Foundations through 15-AI-Portfolio-Projects) are the deep-dives for each phase.

## 🔑 The Five Targets at a Glance

| Target role | Current score | Target score | Time to target | Verdict |
|---|---|---|---|---|
| Junior AI Engineer | 71 | 92 | ~1 month | Do NOT target — underleveling |
| Mid-Level AI Engineer | 64 | 90 | ~3 months | Fallback floor only |
| Senior AI Engineer | 58 | 88 | ~5 months | Strong target |
| **AI Solutions Developer** | 61 | 91 | ~4 months | **Primary near-term target** |
| AI Solutions Architect | 54 | 87 | ~9–11 months | The 12-month destination |

## 🔑 The Three-Cert Strategy

| Order | Cert | Why | Prep (for you) |
|---|---|---|---|
| 1 | **AI-102** Azure AI Engineer Associate | The identity pivot — "senior Azure engineer" → "AI engineer" | 3–5 weeks |
| 2 | **AZ-305** Azure Solutions Architect Expert | The "Architect" title keyword recruiters pattern-match | 4–6 weeks |
| 3 (optional) | **DP-600** Fabric Data Engineer *(verify code)* | Data-grounding edge for RAG-over-enterprise-data roles | 5–7 weeks |

> **Skip** AI-900/AZ-900 (beneath your level — can read as padding). **AZ-204** is a near-freebie to bank only if a target role lists it. Certs open doors; portfolio + system-design close offers. Spend ~30% on certs, ~70% on building and rehearsing.

## 🔑 The Six Portfolio Projects

| # | Project | Proves |
|---|---|---|
| P1 | RAG over enterprise docs | Production RAG, retrieval quality, citations, eval |
| P2 | Multi-agent orchestrator (Semantic Kernel / Agent Framework) | Agentic seniority — the differentiator |
| P3 | Production LLM API service (.NET, streaming, cost guards) | Distributed-systems depth applied to AI |
| P4 | AI-powered React/TS frontend | AI UX, streaming, citations, latency states |
| P5 | LLMOps & evaluation harness | The #1 senior signal — proving it works and doesn't regress |
| P6 | Responsible AI / governance layer | Content safety, PII, prompt-injection defense, audit |

---

> **The thesis in one line:** an *architect with no AI is one course away; an AI engineer with no architecture is five years away.* You are the former. Wrap a thin, deep AI layer around the Azure delivery surface you already own, ship the six projects, and you target Architect-track roles — not entry-level ones.
