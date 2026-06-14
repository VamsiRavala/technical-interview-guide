# Technical Interview & Mastery Guide

> A structured, basics-to-advanced learning and interview-preparation repository for the journey **Senior .NET Developer → AI Solutions Developer → AI Architect / .NET Architect**. Microsoft-centric (Azure, .NET, React, and the Microsoft AI stack).

---

## 🧭 Who this is for

This repo serves four overlapping personas. Pick your track and follow the sections in order:

| Persona | Start here | Then | Goal sections |
|---------|-----------|------|---------------|
| **Senior .NET Developer** (brush up) | `02-DotNet-CSharp`, `01-React-JS` | `03-Azure`, `04-Microservices` | `06-Interview-Preparation` |
| **.NET Architect** | `04-Microservices`, `03-Azure` | `02-DotNet-CSharp` (Clean Arch, SOLID) | `13-AI-System-Design` |
| **AI Solutions Developer** | `08-AI-Foundations` | `09-Azure-AI`, `10-Semantic-Kernel`, `12-RAG-Systems` | `15-AI-Portfolio-Projects` |
| **AI Architect** | `08-AI-Foundations` | `11-AI-Agents`, `13-AI-System-Design` | `16-AI-Career-Roadmap` |

---

## 📚 Sections

### Foundation (existing engineering skills)

1. **[01-React-JS](01-React-JS/)** — React 19, hooks, architecture, interview questions
2. **[02-DotNet-CSharp](02-DotNet-CSharp/)** — C# 13, .NET 9, ASP.NET Core, EF Core, Clean Architecture, DSA
3. **[03-Azure](03-Azure/)** — Core services, Functions, Service Bus, Cosmos DB, messaging
4. **[04-Microservices](04-Microservices/)** — Communication patterns, design patterns, resilience
5. **[05-JavaScript](05-JavaScript/)** — Language fundamentals and patterns
6. **[06-Interview-Preparation](06-Interview-Preparation/)** — Study plans, roadmaps, daily prep
7. **[07-Coding-Algorithms](07-Coding-Algorithms/)** — DSA, Big-O, coding patterns, and FAANG practice strategy

### AI track (Senior .NET → AI Solutions Developer → AI Architect)

8. **[08-AI-Foundations](08-AI-Foundations/)** — LLMs, prompt & context engineering, embeddings, evaluation, Responsible AI
9. **[09-Azure-AI](09-Azure-AI/)** — Azure OpenAI, Azure AI Search, Azure AI Foundry, Content Safety
10. **[10-Semantic-Kernel](10-Semantic-Kernel/)** — Plugins, function calling, memory, agents, .NET + AI integration
11. **[11-AI-Agents](11-AI-Agents/)** — Microsoft Agent Framework, AutoGen, multi-agent orchestration, HITL
12. **[12-RAG-Systems](12-RAG-Systems/)** — Chunking, vector/hybrid search, reranking, grounding, RAG evaluation
13. **[13-AI-System-Design](13-AI-System-Design/)** — Enterprise AI platforms, AI gateway, observability, governance, cost
14. **[14-AI-Frontend](14-AI-Frontend/)** — The AI-specific React layer: streaming, chat UI, SignalR, Entra ID, Power BI embed
15. **[15-AI-Portfolio-Projects](15-AI-Portfolio-Projects/)** — Six buildable projects from chat assistant to full AI platform
16. **[16-AI-Career-Roadmap](16-AI-Career-Roadmap/)** — Skills gap, 6-month roadmap, certifications, resume/LinkedIn, scorecard, learning resources

---

## 🎯 The AI track learning path (basics → advanced)

```text
08 Foundations ──► 09 Azure AI ──► 10 Semantic Kernel ──► 12 RAG ──► 11 Agents ──► 13 System Design
     (concepts)      (services)       (.NET SDK)         (retrieval)  (orchestration)  (architect)
                                          │                                               │
                                          └──────────────► 14 AI Frontend ◄───────────────┘
                                                                  │
                                              15 Portfolio Projects (apply everything)
                                                                  │
                                              16 Career Roadmap (certs, resume, interviews)
```

| Phase | Weeks | Sections | Outcome |
|-------|-------|----------|---------|
| Foundations | 1–3 | 08 | Speak LLM/RAG/agents fluently; understand eval & Responsible AI |
| Microsoft AI | 3–6 | 09, 10 | Build grounded AI features in ASP.NET Core with Azure OpenAI + Semantic Kernel |
| Retrieval | 6–8 | 12 | Build a production RAG pipeline on Azure AI Search |
| Agentic | 8–11 | 11, 14 | Orchestrate multi-agent workflows with HITL and a monitoring UI |
| Architect | 11–16 | 13, 15, 16 | Design, govern, and cost-optimize enterprise AI platforms; interview-ready |

---

## 📌 Conventions

Every section folder follows the same shape so the repo stays uniform:

- `README.md` — table of contents, learning path (Beginner / Intermediate / Advanced), and key comparison tables.
- `NN-topic.md` — numbered topic files ordered **basics → advanced**.
- `NN-interview-questions.md` — interview questions **with answers**, kept inside the relevant skill folder.
- Sub-folders (e.g. `patterns/`, `examples/`) group related subtopics.

> **Currency note (mid-2026):** The Microsoft AI stack moves fast. **Microsoft Agent Framework** (the Semantic Kernel + AutoGen unification), **Azure AI Foundry**, and parts of **Semantic Kernel** change naming and GA status frequently. Where exact API/package names, exam codes, or GA status matter, the docs flag it — verify against [Microsoft Learn](https://learn.microsoft.com) before relying on specifics.
