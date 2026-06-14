# Azure OpenAI — Resource, Deployments & Models

> The Microsoft AI **services** layer. This section assumes you already know generic Azure (resource groups, regions, RBAC, networking) from **[03-Azure](../03-Azure/README.md)** — here we focus only on the AI-specific services that sit on top of that foundation.

---

## What Azure OpenAI Is

Azure OpenAI Service exposes OpenAI's models (GPT-4o, GPT-4.1, o-series, embeddings, image/audio) **inside your Azure tenant** with enterprise governance: Entra ID auth, RBAC, Private Endpoints, regional data residency, content filtering, SLAs, and provisioned throughput (PTU). Functionally the models match OpenAI's; the difference is the **enterprise wrapper** — data isn't used to train the foundation models, stays in your region, and integrates with the rest of Azure (AI Search, Key Vault, Monitor, APIM).

It is **one service inside Azure AI Foundry** (see `04-azure-ai-foundry.md`). You *consume* Azure OpenAI models; you *build, evaluate, secure, and operate* in Foundry.

---

## The Resource → Deployment → Model Hierarchy

This is the single most important mental model and a frequent interview opener.

```text
Subscription
  └─ Resource Group
       └─ Azure OpenAI resource  (a "Cognitive Services" / AI Services account, per region)
            ├─ Deployment "gpt-4o-prod"   → model: gpt-4o,            version 2024-08-06,  capacity: 30 PTU
            ├─ Deployment "gpt-4o-mini"   → model: gpt-4o-mini,       version 2024-07-18,  capacity: 200K TPM
            └─ Deployment "embed-3-large" → model: text-embedding-3-large
```

**You never call a model by its raw name.** You create a **deployment** that binds:
- a specific **model** (e.g., `gpt-4o`),
- a specific **model version** (a dated snapshot, e.g., `2024-08-06`),
- a **capacity** (TPM quota for Standard, or PTUs for Provisioned),
- a **content-filter** configuration,
- a **region** (inherited from the resource).

Your code targets the **deployment name**, not the model name.

### Deployment-name vs model-name (the key gotcha)

```csharp
// Azure.AI.OpenAI v2 (OpenAI SDK shape)
var client = new AzureOpenAIClient(
    new Uri("https://my-aoai.openai.azure.com/"),
    new DefaultAzureCredential());

// "gpt-4o-prod" is the DEPLOYMENT NAME you chose — not necessarily the model name.
var chat = client.GetChatClient("gpt-4o-prod");
```

Why the indirection matters:
- **Blue/green model upgrades** — deploy a new version under a parallel deployment, evaluate it, then point config at it. No code change.
- **Environment isolation** — `gpt-4o-dev` vs `gpt-4o-prod` with different filters/quota.
- **Version pinning** — pin an exact snapshot in production so behavior doesn't silently drift.
- **Reproducibility** — log the deployment + model version with every request.

> Rule: keep the deployment name in **configuration** (App Config / Key Vault), never hard-coded. Treat model upgrades as managed changes (see `08-interview-questions.md` Q on versioning).

---

## Model Families (mid-2026)

| Family | Use for | Notes |
|---|---|---|
| **GPT-4o / GPT-4o-mini** | General chat, RAG answering, tool calling, multimodal | Fast, cheap; `4o-mini` is the workhorse for routine traffic |
| **GPT-4.1 family** | Strong general reasoning, very large context | Good default for hard generation tasks (verify current GA status) |
| **o-series (o1/o3/o4-mini, etc.)** | Deep step-by-step reasoning: complex math, logic, advanced coding | Higher latency/cost (hidden reasoning tokens); use a `reasoning effort` param; route only hard tasks here (verify current GA status) |
| **text-embedding-3-large / -small** | Embeddings for RAG/search | `-large` = 3072 dims, supports dimension reduction; cosine similarity |
| **text-embedding-ada-002** | Legacy embeddings | Prefer `-3` for new work |
| **Image / audio (DALL·E, GPT-4o realtime, Whisper, TTS)** | Vision, image gen, speech | Availability varies by region (verify current GA status) |

**Model routing** is the senior pattern: a cheap model (`gpt-4o-mini`) handles most traffic and escalates genuinely hard queries to a reasoning model — balancing cost, latency, and quality.

---

## Versions: model version vs API version

Two independent things people conflate:

- **Model version** — the dated weights snapshot (e.g., `gpt-4o` `2024-08-06`). Pin it in prod.
- **API version** — the REST/SDK contract version (e.g., `2024-10-21`). Set it explicitly; update intentionally when you need new request/response features.

Azure offers an **auto-update** option that moves a deployment to newer model versions automatically. For critical production deployments, **disable auto-update and pin** — then upgrade deliberately after running your eval suite. Track Azure's published **retirement/deprecation dates** and migrate before forced cutover.

---

## Regions

- A resource lives in **one region**; deployments inherit it. Capacity (quota) is **regional and per-model-family**.
- Pick regions for: **data residency / compliance**, **model availability** (newest models land in specific regions first), **latency** (near your users/compute), and **capacity** (quota differs per region).
- For HA/throughput you deploy the **same model to multiple regions** and route across them (APIM gateway or client router) — see `07-apim-ai-gateway.md` and `06-security-and-networking.md`.
- **Deployment types** also affect residency vs capacity (covered next file): **Standard (regional)**, **Data Zone**, **Global** — these trade residency for capacity/availability.

---

## The Chat Message Roles (quick reference)

| Role | Purpose |
|---|---|
| **system** (or **developer** on newer models) | Durable behavior, persona, rules, format, guardrails — highest priority |
| **user** | The human's per-turn input/data |
| **assistant** | Model's prior replies; **tool calls** appear here |
| **tool** | Results of executed tool/function calls, tied to a tool-call ID |

Put guardrails and format requirements in **system**; clearly delimit untrusted/retrieved content inside **user**; return tool results as **tool** messages with matching IDs.

---

## APIs: Completions → Chat Completions → Responses

- **Completions** (legacy, single prompt string) — deprecated for chat.
- **Chat Completions** — the workhorse: role-tagged messages, tool calling, structured outputs, multimodal.
- **Responses API** — newer, agentic/stateful: server-managed conversation state + built-in tools (file search, code interpreter). Use it for agent scenarios; pairs with the **Foundry Agent Service** (`04-azure-ai-foundry.md`). (verify current GA status)

---

## Brush-Up Cheat-Sheet

- Resource → **deployment** (binds model + version + capacity + filter + region) → your code calls the **deployment name**.
- **Pin model version** + **set API version explicitly** in production; disable silent auto-update.
- `gpt-4o-mini` for routine, `gpt-4o`/`4.1` for hard generation, **o-series** for deep reasoning; **route by difficulty**.
- `text-embedding-3-large` + **cosine** is the default RAG embedding; same model for index + query.
- Capacity/quota is **regional, per-model-family**; multi-region for HA.
- It's a service **inside Azure AI Foundry**; Foundry is the platform, Azure OpenAI is the model-serving service.
