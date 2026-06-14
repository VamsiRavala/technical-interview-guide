# AI-103 Exam Prep — Developing AI Apps and Agents on Azure

> Preparation path and sample Q&A for **Exam AI-103** → **Microsoft Certified: Azure AI Apps and Agents Developer Associate**. AI-103 is the successor to AI-102 (which retires **June 30, 2026**).
>
> ⚠️ **Beta / currency note (mid-2026):** AI-103 launched as a **beta** exam and the objective domains, weightings, and tooling (Azure AI Foundry, Foundry Agent Service) are moving fast. The percentages and bullet points below are synthesized from early public guidance and **may not match the live exam exactly**. Always confirm against the official skills-measured page before you book: https://learn.microsoft.com/credentials/certifications/resources/study-guides/ai-103

---

## 1. Exam at a glance

| Item | Detail |
|------|--------|
| Exam code | **AI-103** — *Developing AI Apps and Agents on Azure* |
| Certification | Azure AI Apps and Agents Developer Associate |
| Replaces | **AI-102** (Azure AI Engineer Associate), retiring **30 Jun 2026** |
| Status | **Beta** (scored results and final objective weights may lag/shift) |
| Primary language | **Python** + Azure SDKs (this is Python-first, not C#-first) |
| Core platform | **Azure AI Foundry** (projects, model catalog, Agent Service, evaluations) |
| Length / questions | ~120 minutes, ~40–60 items |
| Question types | Multiple choice, multi-select, case studies, scenario / drag-and-drop |
| Passing score | 700 / 1000 |
| Audience | Developers who build generative-AI apps and **agents** on Azure |

---

## 2. AI-102 or AI-103 — which should *you* take?

**Take AI-103.** Today is mid-June 2026 and AI-102 retires June 30, 2026 — there isn't a realistic window to prep and pass AI-102 before it's gone, and the credential you'd earn is the one being sunset. AI-103 is also a far better match for your target role (AI Solutions Developer / AI Engineer building **agents**), because it re-centers the exam on **generative AI, RAG, and agentic solutions** instead of the older "cognitive services" surface.

Two honest caveats about a beta exam:

- **Scoring is delayed.** Beta exams typically don't return your result immediately — you get scored once the beta period closes and Microsoft finalizes the cut score. Plan for that if you need the badge by a deadline.
- **Content can shift.** Beta objectives sometimes get re-weighted before general availability. Lean on durable fundamentals (below) rather than memorizing a fixed blueprint.

If you need a *credential in hand immediately*, AI-900 (Fundamentals) is still a quick, low-risk option — but for your career target, AI-103 is the one that matters.

---

## 3. Skills measured (synthesize, then verify)

Early guidance shows some variance in the exact weights — treat these as approximate and **verify on Microsoft Learn**:

| # | Domain | Approx. weight | What it covers |
|---|--------|----------------|----------------|
| 1 | **Plan & manage an Azure AI solution** | ~20–30% | Choose Azure AI services & Foundry; provisioning; **Responsible AI** (Content Safety, Prompt Shields, groundedness); **security** (managed identity, RBAC, Key Vault, disable local auth); **networking** (Private Endpoints, VNet); **monitoring** (Azure Monitor, diagnostics, cost/token metrics) |
| 2 | **Implement generative AI & agentic solutions** | ~30–40% (largest) | Azure AI Foundry projects & model deployments; **prompt engineering**; **RAG** (Azure AI Search, grounding, citations); **agents** (Foundry Agent Service, tools/function calling, **multi-agent**, conversation memory/threads); evaluations |
| 3 | **Implement computer vision solutions** | ~10–20% | Azure AI Vision **Image Analysis 4.0** (caption, tags, OCR/Read, smart crops); **Custom Vision** (classification & object detection); **Content Understanding** (multimodal extraction); Face |
| 4 | **Implement natural language / speech solutions** | ~10–20% | Azure AI **Language** (NER, PII, sentiment, key phrases, CLU, custom classification, question answering); **Translator**; **Speech** (STT, TTS, translation, custom speech/voice, diarization) |
| 5 | **Implement information extraction** | ~10–15% | **Document Intelligence** (prebuilt + custom + composed models); **Azure AI Search** (indexers, skillsets, integrated vectorization, semantic ranker, knowledge store) |

> The exam's center of gravity is **Domain 2** (generative AI + agents). Spend your time proportionally: agents, RAG, and Foundry first; vision/speech/extraction second.

---

## 4. Your starting point — strengths and gaps

You are *not* starting from zero. Mapped against AI-103:

**Strong already (light review):**
- Azure provisioning, RBAC, **managed identity**, Key Vault, Private Endpoints, Azure Monitor — Domain 1's security/networking/monitoring is your home turf.
- RAG concepts, Azure AI Search, prompt engineering, agents — you've built these in your portfolio and they're in this repo (sections 08–13).

**Genuine gaps (where most of your study should go):**
1. **Python.** AI-103 is Python-first. Your C# RAG/agent knowledge transfers conceptually, but you must be fluent with the **Python Azure AI SDKs** (`azure-ai-projects`, `azure-ai-agents`/Agent Service, `openai`, `azure-search-documents`, `azure-ai-documentintelligence`, `azure-ai-vision`, `azure-ai-textanalytics`, `azure-cognitiveservices-speech`). *(Verify exact package names — the Foundry SDKs are renaming frequently.)*
2. **Azure AI Foundry specifics** — projects, connections, model catalog/deployments, **Agent Service**, prompt flow, and the **evaluation** tooling.
3. **Vision, Speech, Language, Document Intelligence service APIs** — you haven't used these; they're ~40% of the exam combined. This is rote service-API knowledge: which service/feature/SDK call solves which scenario.

### Map AI-103 domains → this repo

| AI-103 domain | Study from |
|---------------|-----------|
| Plan & manage (security/networking/monitoring/RAI) | [09-Azure-AI](../09-Azure-AI/) (05-content-safety, 06-security-and-networking, 07-apim-ai-gateway), [13-AI-System-Design](../13-AI-System-Design/) (07-governance, 06-observability) |
| Generative AI & agents | [08-AI-Foundations](../08-AI-Foundations/), [09-Azure-AI](../09-Azure-AI/) (01–04), [11-AI-Agents](../11-AI-Agents/), [12-RAG-Systems](../12-RAG-Systems/) |
| RAG / information extraction (AI Search) | [12-RAG-Systems](../12-RAG-Systems/), [09-Azure-AI/03-azure-ai-search.md](../09-Azure-AI/03-azure-ai-search.md) |
| Vision / Speech / Language / Doc Intelligence | *Not yet in this repo — use the Microsoft Learn paths in §6* |

---

## 5. Preparation path (6–8 weeks, ~8–12 hrs/week)

A senior engineer who already knows Azure can move fast. The long pole is **Python fluency + the four service families you haven't touched**.

### Week 0 — Setup & Python ramp
- Provision an **Azure AI Foundry** project (hub + project), an **Azure OpenAI / model deployment**, an **Azure AI Search** service, and a multi-service **Azure AI services** resource.
- Get comfortable in **Python**: virtual envs, `async`, typing, and the Azure SDK auth pattern with `DefaultAzureCredential`. If you can write the C#, you can learn the Python equivalent in a week.
- Lab: call a chat completion from Python using **managed identity** (no keys).

### Weeks 1–2 — Domain 2: Generative AI & Agents (biggest payoff)
- Foundry: deploy models, use the **model catalog**, set up **connections**, run a **prompt flow**, run an **evaluation**.
- **RAG**: build "chat over your data" with Azure AI Search **integrated vectorization** + **hybrid + semantic ranker**; enforce **grounding + citations**.
- **Agents**: build a single agent with **tools/function calling** on **Foundry Agent Service**; add **conversation memory/threads**; then a **multi-agent** flow (planner → worker → reviewer).
- Labs map directly to your portfolio projects (sections 11, 12, 15).

### Week 3 — Domain 1: Plan & manage
- Responsible AI: **Azure AI Content Safety**, **Prompt Shields**, blocklists, **groundedness detection**.
- Security: **managed identity + RBAC** (e.g., *Cognitive Services OpenAI User*), **Key Vault**, **disable local (key) auth**.
- Networking: **Private Endpoints**, disable public network access, VNet integration.
- Monitoring: **diagnostic settings → Log Analytics**, Azure Monitor metrics, token/cost tracking, alerts.

### Week 4 — Domain 3: Computer Vision
- **Image Analysis 4.0**: captioning, dense captions, tags, object detection, **OCR/Read**, smart crops, background removal.
- **Custom Vision**: image classification vs object detection; train, evaluate, **publish**, predict.
- **Content Understanding** (multimodal extraction) and **Face** basics.

### Week 5 — Domain 4: Language & Speech
- **Language**: NER, **PII detection**, sentiment, key phrases, entity linking; **CLU** (Conversational Language Understanding), **custom text classification**, **custom question answering**.
- **Translator**: text & document translation.
- **Speech**: speech-to-text (incl. **batch** + **diarization**), text-to-speech, **custom speech / custom neural voice**, speech translation.

### Week 6 — Domain 5: Information extraction
- **Document Intelligence**: prebuilt (invoice, receipt, ID, **layout**, read), **custom** (template vs neural), **composed** models; confidence scores.
- **Azure AI Search** deeper: indexers, **skillsets** (built-in + custom Web API skill), **knowledge store**, integrated vectorization end-to-end.

### Weeks 7–8 — Integrate, evaluate, drill
- Build one **capstone** that touches multiple domains (agent that calls a RAG tool + a Document Intelligence tool + Content Safety).
- Take practice tests; review wrong answers; re-read the official skills-measured page; book the exam.

---

## 6. Resources

- **Official:** AI-103 study guide and the *Azure AI Apps and Agents Developer* certification page on Microsoft Learn (verify the live objectives). Microsoft Learn training paths for Azure AI Foundry, Azure OpenAI, Azure AI Search, Vision, Language, Speech, and Document Intelligence.
- **Hands-on:** the official **Microsoft Learn / Azure-Samples** labs for AI-102/AI-103 (Python), `Azure-Samples/azureai-samples`, and the Foundry Agent Service quickstarts.
- **GitHub courses:** `microsoft/ai-agents-for-beginners`, `microsoft/generative-ai-for-beginners`.
- **This repo:** sections 08–13 for the generative-AI/agents/RAG/architecture depth.
- **Practice tests:** use *only* to find gaps — third-party "dumps" are often wrong and beta content shifts. Trust the official skills-measured page over any third-party set.

---

## 7. Sample questions & answers

> Format mirrors the exam (single-best-answer, multi-select, and short scenarios). Each answer includes a short **why**. These are **practice items written for study**, not real exam questions, and reflect mid-2026 service behavior — verify specifics (especially SDK/package names and Foundry features) against current docs.

### Domain 1 — Plan & manage an Azure AI solution

**Q1.** You must call Azure OpenAI from a Python app running on Azure Container Apps with **no secrets in code or config**. Which approach?
A. Store the API key in app settings
B. Use a SAS token
C. Use **managed identity** with `DefaultAzureCredential` and assign the *Cognitive Services OpenAI User* role
D. Embed the key in the container image

**Answer: C.** Managed identity + Entra RBAC removes keys entirely. Assign the least-privilege data-plane role (*Cognitive Services OpenAI User* for inference). Keys (A/D) and SAS (B) are exactly what you're trying to avoid; for hardened setups also **disable local/key auth** on the resource.

**Q2.** Your security team requires that the Azure AI services resource not be reachable from the public internet. Which two actions achieve this? (Choose two.)
A. Create a **Private Endpoint** for the resource
B. Rotate the API keys weekly
C. Set **public network access to Disabled**
D. Enable diagnostic logging

**Answer: A and C.** A Private Endpoint puts the service on your VNet with a private IP; disabling public network access blocks internet routes. Key rotation (B) and diagnostics (D) are good practice but don't restrict network reachability.

**Q3.** A generative-AI app must block prompts that attempt **jailbreaks / prompt injection** and detect when an answer is **not grounded** in the provided source. Which features do you use?
A. Azure AI Content Safety **Prompt Shields** + **groundedness detection**
B. Azure Front Door WAF
C. Network Security Groups
D. Key Vault soft-delete

**Answer: A.** Prompt Shields target direct/indirect prompt-injection; groundedness detection (Content Safety / Foundry evaluators) flags ungrounded ("hallucinated") output. The others are unrelated infrastructure controls.

**Q4.** You need to track **token usage and cost per deployment** and alert when latency spikes. What do you configure?
A. Application Insights only, no other config
B. **Diagnostic settings** to send metrics/logs to **Log Analytics** + **Azure Monitor alerts**
C. A custom cron job scraping the portal
D. Azure Policy

**Answer: B.** Route resource diagnostics to Log Analytics and build Azure Monitor metric/alert rules (e.g., on token counts and latency). App Insights complements this for app-level traces but isn't sufficient alone (A); C/D don't provide telemetry.

**Q5.** Which statement about choosing between **Azure AI Foundry** and provisioning standalone Azure AI services is correct?
A. Foundry can only use OpenAI models
B. Foundry projects centralize **model catalog, deployments, connections, evaluations, and Agent Service**, which is preferred for building gen-AI apps and agents
C. Standalone services are required for managed identity
D. Foundry cannot connect to Azure AI Search

**Answer: B.** Foundry is the unified hub for building/operating gen-AI apps and agents (catalog, deployments, connections to services like AI Search, eval tooling, Agent Service). A, C, D are false.

### Domain 2 — Generative AI & agentic solutions

**Q6.** You're building **RAG** "chat over company documents" and want the simplest path to vector + keyword retrieval with relevance reranking in Azure AI Search. Which combination?
A. Vector-only search, no semantic ranker
B. **Hybrid search (vector + keyword)** with the **semantic ranker**, populated via **integrated vectorization**
C. Pure keyword (BM25) search
D. Store embeddings in a SQL table and scan them

**Answer: B.** Hybrid + semantic ranker gives the best relevance with least custom code; integrated vectorization handles chunking/embedding in the indexer pipeline. Vector-only (A) and keyword-only (C) underperform hybrid; D doesn't scale and ignores ANN.

**Q7.** In a RAG app, users sometimes get answers that aren't supported by the retrieved documents. Which two mitigations are most directly effective? (Choose two.)
A. Add a system instruction to **answer only from provided context and cite sources**, and return "I don't know" when unsupported
B. Increase `temperature`
C. Enable **groundedness/faithfulness evaluation** and gate responses
D. Remove the retrieval step

**Answer: A and C.** Grounding instructions + citations constrain the model; groundedness evaluation detects unsupported claims. Raising temperature (B) worsens it; removing retrieval (D) defeats RAG.

**Q8.** You need an agent that can **call your internal pricing API** during a conversation and keep context across turns. Which Foundry Agent Service concepts apply? (Choose two.)
A. **Tools / function calling** to invoke the API
B. **Threads** to persist conversation state
C. Custom Vision project
D. Batch transcription

**Answer: A and B.** Agents use tool/function calling to reach external APIs and threads to maintain per-conversation memory/state. C and D are unrelated services.

**Q9.** A workflow needs a **planner** agent to decompose a task and delegate to specialist agents, with a final **reviewer** before responding. What pattern is this?
A. Single-agent ReAct loop
B. **Multi-agent orchestration** (planner → workers → reviewer / approval)
C. Prompt chaining in one prompt
D. Fine-tuning

**Answer: B.** This is a multi-agent orchestration with role-specialized agents and a review/approval gate — the agentic pattern AI-103 emphasizes. A single agent (A) lacks delegation; C/D don't fit.

**Q10.** You want **deterministic, parseable** output from the model to drive downstream code. Best approach?
A. Ask politely for JSON in the prompt and hope
B. Use **structured outputs / JSON schema (response format)** and validate
C. Raise `max_tokens`
D. Lower `top_p` to 0

**Answer: B.** Structured outputs / JSON-schema response formats enforce a parseable shape; always validate server-side. Prompt-only requests (A) are unreliable; C/D don't guarantee structure.

**Q11.** Which is the correct order for a basic RAG ingestion pipeline?
A. Embed → chunk → store → retrieve
B. **Extract/parse → chunk → embed → index/store → retrieve at query time**
C. Retrieve → embed → chunk
D. Index → parse → embed

**Answer: B.** You parse documents, chunk, embed the chunks, store them in the vector index, then retrieve at query time. Order matters; A/C/D are scrambled.

**Q12.** You must reduce cost and latency for repeated, identical FAQ-style questions hitting your gen-AI endpoint. Which technique?
A. **Semantic / response caching** in front of the model (e.g., via the AI gateway)
B. Increase the deployment's TPM only
C. Switch to a larger model
D. Disable content filtering

**Answer: A.** Caching semantically similar prompts avoids redundant model calls, cutting cost and latency. Bigger model (C) or more TPM (B) raise cost; D is unsafe and irrelevant.

**Q13.** When is **fine-tuning** a better choice than RAG?
A. When answers must reflect frequently changing documents
B. When you need to teach a **consistent style/format or domain task** not easily injected via context, and data is relatively stable
C. Whenever accuracy matters
D. Never; RAG always wins

**Answer: B.** Fine-tuning shifts behavior/style; RAG injects fresh, citable knowledge. Changing facts (A) favor RAG; "accuracy" (C) is not a single lever; D is wrong.

**Q14.** Which Foundry capability do you use to **compare prompt/model variants on a labeled dataset** with metrics like groundedness and relevance before shipping?
A. Model catalog
B. **Evaluations (eval) tooling**
C. Connections
D. Content Understanding

**Answer: B.** Foundry's evaluation tooling runs prompt/model variants against datasets and computes quality metrics (groundedness, relevance, etc.). The others serve different purposes.

**Q15.** A prompt sometimes leaks the **system instructions** when users ask cleverly. Which is the most appropriate first mitigation?
A. Put secrets in the system prompt anyway
B. Use **Prompt Shields** and design the system prompt to not contain secrets; treat all user input as untrusted
C. Increase temperature
D. Remove the system prompt entirely

**Answer: B.** Defense in depth: never put true secrets in prompts, treat input as untrusted, and use Prompt Shields against injection. A is the anti-pattern; C/D don't address the risk.

### Domain 3 — Computer vision

**Q16.** You need to generate a **human-readable caption**, **tags**, and read **printed text** from product photos with a single managed service. Which service/feature?
A. Custom Vision
B. **Azure AI Vision — Image Analysis 4.0** (caption, tags, Read/OCR)
C. Document Intelligence
D. Face

**Answer: B.** Image Analysis 4.0 provides captioning, tagging, and OCR (Read) out of the box. Custom Vision (A) is for training your own classifier/detector; C is documents; D is faces.

**Q17.** Your app must detect **your company's specific products** (not generic objects) and their bounding boxes in images. Which approach?
A. Image Analysis generic object detection
B. **Custom Vision — object detection project** (train, evaluate, publish, predict)
C. OCR
D. Background removal

**Answer: B.** Custom Vision object detection lets you train on your labeled products and returns bounding boxes. Generic detection (A) won't know your SKUs.

**Q18.** Which is true about **Custom Vision** model lifecycle?
A. You train and immediately call it with no publish step
B. You **train → evaluate (precision/recall/mAP) → publish an iteration → call the prediction endpoint**
C. It requires Document Intelligence
D. It only does classification, never detection

**Answer: B.** You train, review metrics, publish an iteration, then call prediction. A skips publishing; C is unrelated; D is false (it does both classification and detection).

**Q19.** You need to extract structured fields and content from **mixed multimodal documents/images** (text + layout + visuals) using newer Azure capabilities. Which service is positioned for this?
A. **Content Understanding** (multimodal extraction)
B. Translator
C. Speech
D. Key Vault

**Answer: A.** Content Understanding targets multimodal information extraction. *(Verify current name/availability — this area is evolving.)* Translator/Speech/Key Vault don't fit.

### Domain 4 — Language & Speech

**Q20.** You must detect and **redact PII** (names, SSNs) from support transcripts. Which Azure AI Language feature?
A. Sentiment analysis
B. **PII detection** (with redaction)
C. Key phrase extraction
D. Language detection

**Answer: B.** The Language service's PII detection identifies and can redact personal data. The others analyze different aspects.

**Q21.** You're building an assistant that maps user utterances to **intents and entities** for a custom domain (e.g., "book a room"). Which feature?
A. **Conversational Language Understanding (CLU)**
B. Translator
C. Custom Vision
D. Text-to-speech

**Answer: A.** CLU is the intent/entity classification service for conversational apps (the successor to LUIS). The rest are unrelated.

**Q22.** You need **speaker-separated** text from a multi-person meeting recording, processed asynchronously at scale. Which Speech capability?
A. Text-to-speech
B. **Batch transcription with diarization**
C. Custom neural voice
D. Speech translation

**Answer: B.** Batch transcription handles large async jobs; diarization separates speakers. TTS (A) and custom voice (C) generate audio; translation (D) is a different task.

**Q23.** You want a **branded, unique synthetic voice** for your app's spoken responses. Which feature?
A. Prebuilt neural voices only
B. **Custom Neural Voice**
C. CLU
D. Translator

**Answer: B.** Custom Neural Voice creates a bespoke voice (with required responsible-AI gating/approval). Prebuilt voices (A) aren't unique to you.

**Q24.** Which service translates **whole documents while preserving layout/format**?
A. Speech translation
B. **Translator — document translation**
C. CLU
D. Read/OCR

**Answer: B.** Translator's document translation preserves structure/format. Speech translation (A) is for audio; the others don't translate documents.

### Domain 5 — Information extraction

**Q25.** You must extract **vendor, total, and line items from invoices** with minimal effort. Which Document Intelligence option?
A. Train a custom neural model from scratch
B. **Prebuilt invoice model**
C. Layout model only
D. Read model only

**Answer: B.** The prebuilt invoice model returns those fields with no training. Custom (A) is for non-standard docs; layout (C) gives structure but not invoice semantics; read (D) is OCR only.

**Q26.** Your forms are a **unique internal template** not covered by any prebuilt model. Which approach gives the best accuracy?
A. Prebuilt receipt model
B. **Custom Document Intelligence model (template or neural) trained on labeled samples**; compose models if multiple form types
C. Image Analysis tags
D. CLU

**Answer: B.** Custom models trained on your labeled forms fit unique templates; composed models route across multiple form types. Prebuilt receipt (A) won't match; C/D are wrong domains.

**Q27.** In Azure AI Search, you want to **enrich documents during ingestion** (e.g., OCR images, extract key phrases, then vectorize) automatically. What do you build?
A. A manual export script
B. An **indexer with a skillset** (built-in skills + integrated vectorization; custom Web API skill if needed)
C. A SQL trigger
D. A Logic App only

**Answer: B.** Indexers + skillsets run an enrichment pipeline at ingestion; integrated vectorization adds embeddings. A/C/D don't provide the AI-enrichment pipeline natively.

**Q28.** You need retrieved chunks to carry **document-level metadata** (e.g., department) so you can **filter** results by the signed-in user's access. Which combination?
A. Store metadata as filterable fields and apply **security trimming / filters** at query time
B. Put everything in one giant field
C. Disable filtering for performance
D. Use Custom Vision

**Answer: A.** Filterable metadata fields plus query-time filters enable security trimming (only return what the user may see). B/C harm relevance and security; D is unrelated.

### Mixed / scenario

**Q29.** (Multi-select) Which are valid ways to authenticate Azure AI service calls in a hardened production app? (Choose two.)
A. **Microsoft Entra ID with managed identity + RBAC**
B. Hard-coded API key in source
C. **Key stored in Key Vault, retrieved at runtime** (if keys must be used)
D. Anonymous access

**Answer: A and C.** Prefer managed identity + RBAC; if keys are unavoidable, store them in Key Vault and fetch at runtime. Hard-coded keys (B) and anonymous access (D) are insecure.

**Q30.** An agent must call a **Document Intelligence** tool and a **RAG search** tool, and refuse unsafe content. Which Azure pieces compose this? (Choose three.)
A. **Foundry Agent Service with function/tool calling**
B. **Azure AI Search** (RAG retrieval)
C. **Azure AI Content Safety** (guardrails)
D. Custom Neural Voice

**Answer: A, B, and C.** The agent (A) orchestrates tools — a RAG tool over AI Search (B) and a Document Intelligence tool — with Content Safety (C) guarding inputs/outputs. Custom Neural Voice (D) is irrelevant here.

**Q31.** You deployed a model in Foundry but inference calls fail with 401/403 from your Python app using managed identity. Most likely fix?
A. Increase TPM quota
B. **Assign the correct data-plane RBAC role (e.g., *Cognitive Services OpenAI User*) to the app's managed identity** and confirm the resource allows Entra auth
C. Switch models
D. Add more documents to the index

**Answer: B.** 401/403 = authn/authz: the identity lacks the data-plane role, or Entra auth/local-auth settings are misconfigured. Quota (A) yields 429, not 401/403; C/D are unrelated.

**Q32.** You need to choose between **Azure OpenAI "On Your Data"** and a **custom RAG pipeline**. Which statement guides the decision?
A. On Your Data is always better
B. **"On Your Data" is fastest to stand up for common cases; a custom pipeline gives full control over chunking, retrieval, ranking, and evaluation** for advanced needs
C. Custom pipelines can't use Azure AI Search
D. They're identical

**Answer: B.** "On Your Data" is the quick managed path; a custom pipeline wins when you need to tune chunking/retrieval/reranking/evals. A overstates; C is false; D is false.

---

## 8. Exam-day reminders

- Read scenario stems carefully for the **constraint that decides the answer** ("no secrets," "no public internet," "minimal effort," "unique template").
- When two answers look right, pick the one that is **most managed / least custom** unless the scenario demands control.
- Security defaults: **managed identity + RBAC > Key Vault > keys**; **Private Endpoint + public access disabled** for network isolation.
- For gen-AI quality: **grounding + citations + evaluation**; for safety: **Content Safety + Prompt Shields**.
- It's **beta** — if an item feels off or uses unfamiliar new tooling, answer on durable principles and move on.

> Re-verify all specifics (objective weights, SDK/package names, Foundry feature names, beta status) on the official Microsoft Learn AI-103 study guide before your exam.
