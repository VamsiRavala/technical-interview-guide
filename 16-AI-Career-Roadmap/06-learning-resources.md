# Learning Resources — Curated for the 6-Month Build

> A hand-picked, no-filler resource list for a senior .NET/Azure engineer learning the AI layer: YouTube channels grouped by purpose, the Microsoft Learn paths that map to each phase, and the two GitHub courses worth doing cover-to-cover. Opinionated — these are the channels and paths worth your scarce 10–15 hrs/week, not an exhaustive directory. Currency: **mid-2026**; channel names are stable but specific video/playlist titles drift, so search the channel rather than chasing dead links.

> **How to use this:** videos are for *intuition and orientation*, not for depth. Watch to understand a concept, then immediately build the matching lab from [02-6-month-roadmap.md](02-6-month-roadmap.md). For the Microsoft stack specifically, **video is the weakest medium** — pair it with Microsoft Learn + the GitHub courses (see the honest note below).

---

## 📺 YouTube Channels — Grouped by Purpose

### 1. Foundations — how LLMs actually work

Watch these in Month 1. They build the mental model that stops you treating the LLM as a black box.

| Channel | Why it earns your time | Use it for |
|---|---|---|
| **3Blue1Brown** | The clearest visual intuition for neural nets, transformers, and attention anywhere. The "But what is a GPT?" / attention series is the gold standard. | Tokens, embeddings, attention, why transformers work — the *why* under the API. |
| **Andrej Karpathy** | A founding-level practitioner who teaches by building. "Let's build GPT from scratch" and "Intro to LLMs" demystify the entire stack. | Deep mechanical understanding; how training/inference actually happen. Skim the code, absorb the concepts. |
| **IBM Technology** | Short, clean, vendor-neutral explainers (RAG, embeddings, vector DBs, agents, fine-tuning vs RAG). Whiteboard format, ~8 min each. | Fast, correct definitions of every term in [01-skills-gap-analysis.md](01-skills-gap-analysis.md). Great for interview vocabulary. |

> You do **not** need to train a model. Karpathy and 3Blue1Brown are for *intuition* — enough to answer "why does it hallucinate / why does context length matter / what is an embedding" cold. Don't rabbit-hole into ML math; that's explicitly on the "do NOT do" list in [05-job-readiness-scorecard.md](05-job-readiness-scorecard.md).

### 2. Microsoft Stack — your actual day job

This is where you'll live. Lean on these for SK, Azure AI, .NET integration, and Foundry.

| Channel | Why it earns your time | Use it for |
|---|---|---|
| **Microsoft Developer** | Official; hosts Semantic Kernel, .NET AI, and Azure AI sessions, plus conference recordings. | SK and Azure OpenAI walkthroughs, official patterns, GA announcements. |
| **Microsoft Azure** | Official Azure channel; Azure AI Foundry, Azure AI Search, Azure OpenAI feature demos. | Foundry portal tours, AI Search vector/hybrid demos, service-level overviews. |
| **Microsoft Reactor** | Live/recorded community sessions, often hands-on, frequently covering SK, agents, and RAG with real code. | Workshop-style, build-along content; agent and RAG sessions. |
| **dotnet (.NET) / .NET Conf** | The .NET team channel; `Microsoft.Extensions.AI`, SK-in-ASP.NET-Core, .NET + AI integration talks. | Wiring AI into ASP.NET Core the idiomatic way — directly relevant to P3/P4. |
| **Nick Chapsas** | The most respected independent .NET voice; pragmatic, opinionated, production-minded. Increasingly covers .NET + AI / SK. | Production .NET patterns and honest takes — matches your "production rigor" positioning. |

### 3. AI Engineering — RAG, agents, and applied patterns

Vendor-broad practitioners. Use for *patterns and judgment* that transcend any one SDK; translate the Python to your C#/SK world.

| Channel | Why it earns your time | Use it for |
|---|---|---|
| **AssemblyAI** | Excellent applied tutorials on RAG, embeddings, evaluation, and LLM app patterns; clear and current. | RAG architecture and eval intuition (Month 3); chunking/retrieval trade-offs. |
| **Cole Medin** | Strong, current agentic-AI build content — RAG, agents, MCP, real projects end-to-end. | Agent patterns and MCP (Month 4); end-to-end "ship a real thing" framing. |
| **LangChain (official)** | Even though you'll build in SK/.NET, LangChain's content is the clearest on agent/RAG *concepts* (chains, tools, memory, eval). | Conceptual reference for agentic patterns — map the ideas onto SK, don't adopt the framework. |
| **AI Jason** | Practical, fast-moving builds of agents, RAG, and multi-agent systems; good for seeing the frontier. | Multi-agent topologies and "what's possible now" orientation (Month 4). |
| **Mervin Praison** | Prolific hands-on agent/multi-agent and tooling tutorials; good breadth across frameworks. | Quick orientation to many agent frameworks; survey before you commit to SK/Agent Framework. |

> **Translation discipline:** most of group 3 is Python-first. That's fine — you're learning the *pattern* (planner-executor, ReAct, group-chat, eval-as-judge), then implementing it in Semantic Kernel / Agent Framework in C#. Resist switching languages; per [01-skills-gap-analysis.md](01-skills-gap-analysis.md), Python is a "read and operate" skill for you.

### 4. News & Landscape — stay current without doom-scrolling

| Channel | Why it earns your time | Use it for |
|---|---|---|
| **Matt Wolfe** | The most efficient single "what shipped this week in AI" digest. High signal on launches, low on hype-chasing if you watch selectively. | A weekly ~20-min scan so you're never blindsided in an interview by a model/feature you've never heard of. |

> Budget **≤30 min/week** here. News is the lowest-ROI category for *building* skill — it's interview small-talk insurance, not a study activity. Don't let it displace lab time.

---

## 📘 Microsoft Learn Paths — mapped to the phases

Microsoft Learn is the **authoritative, current** source for the Microsoft stack and the best coverage checklist for AI-102. Do the C#/.NET tracks where they exist. *(Path titles shift as Foundry evolves — search Microsoft Learn for the current title; verify GA on preview content.)*

| Phase / Month | Microsoft Learn path (search title) | Pairs with |
|---|---|---|
| **Phase 1 / M1** | "Fundamentals of Generative AI"; "Get started with Azure OpenAI"; "Develop generative AI solutions with Azure OpenAI in Azure AI Foundry" (C#/.NET track) | [08-AI-Foundations](../08-AI-Foundations/), [09-Azure-AI](../09-Azure-AI/) |
| **Phase 2 / M2** | "Build AI apps with Semantic Kernel"; "What is Azure AI Foundry" + Foundry SDK quickstarts + evaluation modules | [10-Semantic-Kernel](../10-Semantic-Kernel/) |
| **Phase 2 / M3** | "Implement RAG with Azure AI Search"; "Implement knowledge mining with Azure AI Search"; vector/hybrid/semantic-ranker modules | [12-RAG-Systems](../12-RAG-Systems/) |
| **Phase 3 / M4** | "Develop AI agents with Azure AI Foundry / Agent Service" (preview — read critically given GA churn); agent-focused modules | [11-AI-Agents](../11-AI-Agents/) |
| **Phase 4 / M5** | Azure Well-Architected "AI workload" pillar; security + monitoring modules for Azure AI; APIM GenAI gateway docs | [13-AI-System-Design](../13-AI-System-Design/), [14-AI-Frontend](../14-AI-Frontend/) |
| **Phase 5 / M6** | Responsible AI modules; Azure AI Content Safety; Cloud Adoption Framework — AI; Well-Architected cost-optimization pillar | [13-AI-System-Design](../13-AI-System-Design/), [15-AI-Portfolio-Projects](../15-AI-Portfolio-Projects/) |
| **Cert prep** | Official **AI-102** and **AZ-305** study guides (use as coverage checklists even if not sitting immediately) | [03-certifications.md](03-certifications.md) |

---

## 💻 GitHub Courses — do these cover-to-cover

These two Microsoft courses are the highest-signal *structured, hands-on* resources for your gap. Treat them as guided curricula, not reference docs.

| Course | What it gives you | When |
|---|---|---|
| **microsoft/ai-agents-for-beginners** | A structured, lesson-by-lesson intro to AI agents with Microsoft tooling (covers agent concepts, tool use, multi-agent, and the Microsoft agent stack). The single best on-ramp to Phase 3. | Month 4 (start late M3) |
| **microsoft/generative-ai-for-beginners** | A broad, well-paced GenAI curriculum (prompting, embeddings, RAG, app patterns, responsible AI) with runnable samples. The best structured complement to Months 1–3. | Months 1–3 |

> Both have C#/.NET examples in addition to Python — prefer the .NET path where offered to stay in your lane. Fork them, run every lesson, and commit your work to your own repo (feeds the green-graph habit from [04-resume-and-linkedin.md](04-resume-and-linkedin.md)).

---

## ⚠️ Honest Note — Why Video Alone Won't Cut It for the Microsoft Stack

**Deep, current YouTube content on the Microsoft Agent Framework specifically is thin.** The framework consolidated rapidly through 2025–2026 (absorbing the AutoGen research line and SK Agents), and independent creators haven't produced the volume or depth of tutorials that exist for, say, LangChain. What's out there is often *stale within a quarter* because the SDK surface kept moving. The same is partly true for the newest Azure AI Foundry agent features.

**So pair videos with the authoritative sources, in this order of trust:**

1. **Microsoft Learn paths + official docs** — current, GA-aware, the source of truth. Verify preview/GA status here before committing an architecture.
2. **The two GitHub courses above** — structured, runnable, Microsoft-maintained.
3. **The repo deep-dive sections** ([10-Semantic-Kernel](../10-Semantic-Kernel/), [11-AI-Agents](../11-AI-Agents/)) — your distilled, opinionated reference.
4. **YouTube (groups 2–3)** — for intuition, orientation, and seeing real builds; never as the *authority* on API specifics.

For agents in particular: **learn the durable concepts** (orchestration topologies, HITL, guardrails, tool-calling, eval) from groups 1–3, then implement against the *current* GA SDK confirmed on Microsoft Learn. As [02-6-month-roadmap.md](02-6-month-roadmap.md) puts it — learn the patterns deeply, stay loose on the brand names, and verify GA before you build.

---

## 🗺️ Channel-to-Phase Map (the quick reference)

| Month / Phase | Lead with | Supplement |
|---|---|---|
| **M1 — Foundations** | 3Blue1Brown, Karpathy, IBM Technology · MS Learn GenAI fundamentals · generative-ai-for-beginners | Microsoft Developer (Azure OpenAI demos) |
| **M2 — SK + Foundry** | Microsoft Developer, dotnet/.NET Conf, Nick Chapsas · MS Learn SK path · [10-Semantic-Kernel](../10-Semantic-Kernel/) | Microsoft Reactor (SK sessions) |
| **M3 — RAG** | AssemblyAI, IBM Technology · MS Learn RAG-with-AI-Search · [12-RAG-Systems](../12-RAG-Systems/) | Microsoft Azure (AI Search demos), LangChain (RAG concepts) |
| **M4 — Agents** | Cole Medin, AI Jason, Mervin Praison · ai-agents-for-beginners · MS Learn agent modules · [11-AI-Agents](../11-AI-Agents/) | LangChain (agent concepts), Microsoft Reactor |
| **M5 — Enterprise architecture** | Microsoft Azure, Nick Chapsas · Well-Architected AI pillar · [13-AI-System-Design](../13-AI-System-Design/) | Microsoft Developer (security/observability) |
| **M6 — Governance & strategy** | Microsoft Developer/Azure (Responsible AI, Content Safety) · MS Learn RAI + CAF-AI | — (writing/diagramming month; less video) |
| **All months** | Matt Wolfe (≤30 min/week, landscape) | — |
