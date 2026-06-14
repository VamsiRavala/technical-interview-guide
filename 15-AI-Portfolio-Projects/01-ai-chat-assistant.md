# AI Chat Assistant

A focused, polished streaming chat assistant — Project 1 of the portfolio and the foundation everything else builds on. The goal is to nail the fundamentals: token-by-token streaming, real authentication, durable chat history, and conversation memory — the table stakes that every later project assumes. **Difficulty: ⭐ Beginner.**

> **Build-order note:** Get the Bicep modules, the Entra auth handler, and the SSE streaming helper right here. Projects 2–6 reuse all three.

### Business Problem

A mid-size professional-services firm (think a 400-person accounting/consulting practice) wants an internal "ask-anything" assistant for its staff: drafting client emails, summarizing meeting notes, explaining internal policy language, and answering general knowledge questions — without exposing data to public ChatGPT. **Who pays:** the COO/IT budget, justified by reclaiming ~30 minutes/employee/day of drafting and lookup time, and by replacing shadow-IT usage of consumer ChatGPT (a compliance liability). Procurement requires SSO via the corporate Entra tenant, per-user chat history, and a hard guarantee that prompts never leave the firm's Azure subscription. The MVP is "ChatGPT for us, on our tenant, with audit trails."

### Architecture Diagram

```text
                          Entra ID (corp tenant)
                                  |  OIDC / OAuth2 (PKCE)
                                  v
 ┌──────────────┐   HTTPS   ┌──────────────────────────┐
 │  Browser     │──────────>│  React 19 + MUI SPA       │
 │  (employee)  │<──────────│  (static, on Container App │
 └──────────────┘  SSE      │   or Azure Static Web App) │
        |                   └──────────────┬─────────────┘
        | EventStream (SSE)                 | Bearer (access token, scope: Chat.Access)
        v                                   v
 ┌────────────────────────────────────────────────────────────┐
 │  ASP.NET Core API (Container App)                            │
 │  ┌────────────┐  ┌──────────────┐  ┌────────────────────┐   │
 │  │ Auth (JWT) │  │ ChatService  │  │ Semantic Kernel     │   │
 │  │ Entra      │  │ + memory     │  │ ChatCompletion +    │   │
 │  │ validation │  │ assembly     │  │ streaming           │   │
 │  └────────────┘  └──────┬───────┘  └─────────┬──────────┘   │
 └────────────┬────────────┼────────────────────┼──────────────┘
              │ Managed     │                    │ Managed Identity (AAD token)
              │ Identity    │                    v
              v             v          ┌────────────────────────┐
   ┌──────────────────┐  ┌─────────┐   │  Azure OpenAI          │
   │ Azure SQL DB     │  │ Key     │   │  gpt-4o (stream)       │
   │ (chats/messages) │  │ Vault   │   │  text-embedding-3-lg   │
   └──────────────────┘  └─────────┘   └────────────────────────┘
              │
              v
   ┌──────────────────────────┐
   │ App Insights / Log        │  <-- OpenTelemetry traces, token usage
   │ Analytics                 │
   └──────────────────────────┘
```

### Sequence Diagram

```text
Browser        React SPA        ASP.NET API        Entra ID      Azure OpenAI    Azure SQL
  |   type msg     |                 |                 |              |             |
  |--------------->|                 |                 |              |             |
  |                |  acquire token  |                 |              |             |
  |                |---------------------------------->|              |             |
  |                |<--- access token (Chat.Access) ---|              |             |
  |                | POST /api/conversations/{id}/messages (SSE)      |             |
  |                |---------------->|                 |              |             |
  |                |                 | validate JWT (cached keys)     |             |
  |                |                 | load last N msgs (memory)      |             |
  |                |                 |-------------------------------------------->|
  |                |                 |<------------------------- history ----------|
  |                |                 | build ChatHistory + system prompt           |
  |                |                 | GetStreamingChatMessageContentsAsync         |
  |                |                 |------------------------------>|             |
  |                |  SSE: token     |<---- token chunk -------------|             |
  |                |<----------------|  (data: {"delta":"..."} )      |             |
  |   render delta |                 |   ... repeated per token ...   |             |
  |<---------------|                 |                                |             |
  |                |                 | on complete: persist user+assistant msgs    |
  |                |                 |-------------------------------------------->|
  |                |  SSE: [DONE]    |                                |             |
  |                |<----------------|                                |             |
```

### Folder Structure

```text
enterprise-ai-portfolio/
└─ projects/
   └─ 01-chat-assistant/
      ├─ src/
      │  ├─ ChatAssistant.Api/            # ASP.NET Core minimal API
      │  │  ├─ Endpoints/
      │  │  │  ├─ ConversationsEndpoints.cs
      │  │  │  └─ MessagesEndpoints.cs    # SSE streaming
      │  │  ├─ Services/
      │  │  │  ├─ IChatService.cs
      │  │  │  ├─ ChatService.cs          # SK orchestration + memory
      │  │  │  └─ ConversationStore.cs    # EF Core
      │  │  ├─ Auth/EntraJwtSetup.cs
      │  │  ├─ Telemetry/OtelSetup.cs
      │  │  ├─ Program.cs
      │  │  └─ appsettings.json
      │  ├─ ChatAssistant.Domain/         # entities, DTOs
      │  └─ ChatAssistant.Infrastructure/ # EF Core DbContext, migrations
      ├─ web/
      │  ├─ src/
      │  │  ├─ components/ (ChatWindow, MessageBubble, Composer)
      │  │  ├─ hooks/useChatStream.ts     # consumes SSE
      │  │  ├─ auth/msalConfig.ts
      │  │  └─ App.tsx
      │  ├─ index.html
      │  └─ vite.config.ts
      ├─ infra/
      │  ├─ main.bicep
      │  └─ modules/ (openai.bicep, sql.bicep, containerapp.bicep)
      ├─ tests/
      │  ├─ ChatAssistant.Api.Tests/
      │  └─ web/ (vitest)
      └─ README.md
```

### Database Design

**Store:** Azure SQL Database (relational, transactional, cheap at this scale; conversation/message data is naturally relational and benefits from foreign keys + indexed reads). No vector store needed in Project 1 — there's no retrieval.

**Table: `Conversations`**

| Column | Type | Notes |
|---|---|---|
| `Id` | `uniqueidentifier` | PK, default `NEWSEQUENTIALID()` |
| `UserId` | `nvarchar(64)` | Entra object id (`oid` claim); indexed |
| `Title` | `nvarchar(200)` | auto-generated from first message |
| `CreatedAt` | `datetime2` | UTC |
| `UpdatedAt` | `datetime2` | UTC; for sorting recent |
| `IsArchived` | `bit` | soft delete |

**Table: `Messages`**

| Column | Type | Notes |
|---|---|---|
| `Id` | `uniqueidentifier` | PK |
| `ConversationId` | `uniqueidentifier` | FK → Conversations, `ON DELETE CASCADE`; indexed |
| `Role` | `nvarchar(16)` | `user` / `assistant` / `system` |
| `Content` | `nvarchar(max)` | message text |
| `PromptTokens` | `int` | nullable; usage capture |
| `CompletionTokens` | `int` | nullable |
| `Model` | `nvarchar(64)` | e.g. `gpt-4o-2024-11-20` |
| `CreatedAt` | `datetime2` | UTC; ordering within conversation |

**Index:** composite `(ConversationId, CreatedAt)` on `Messages` to load history in order. Row-level isolation is enforced in code via `UserId` filter (see Security Design).

### API Contracts

| Method | Path | Auth scope | Success | Errors |
|---|---|---|---|---|
| `GET` | `/api/conversations` | `Chat.Access` | `200` | `401` |
| `POST` | `/api/conversations` | `Chat.Access` | `201` | `401` |
| `GET` | `/api/conversations/{id}/messages` | `Chat.Access` | `200` | `401`, `403`, `404` |
| `POST` | `/api/conversations/{id}/messages` | `Chat.Access` | `200` (SSE stream) | `401`, `403`, `404`, `429` |
| `DELETE` | `/api/conversations/{id}` | `Chat.Access` | `204` | `401`, `403`, `404` |

**Create conversation — request/response**

```json
// POST /api/conversations  -> 201
// Request
{ "title": "Draft onboarding email" }
// Response
{
  "id": "5f3c...",
  "title": "Draft onboarding email",
  "createdAt": "2026-06-14T09:00:00Z"
}
```

**Send message (streaming)** — request body is JSON, response is `text/event-stream`:

```json
// POST /api/conversations/{id}/messages
// Request
{ "content": "Summarize this meeting note: ..." }
```

```text
// Response: 200, Content-Type: text/event-stream
data: {"type":"delta","content":"Here"}
data: {"type":"delta","content":" is"}
data: {"type":"delta","content":" the summary..."}
data: {"type":"usage","promptTokens":312,"completionTokens":88}
data: [DONE]
```

The SK streaming call in `ChatService`:

```csharp
var chat = kernel.GetRequiredService<IChatCompletionService>();
var history = await store.LoadHistoryAsync(conversationId, take: 20, ct);
history.AddUserMessage(request.Content);

await foreach (var chunk in chat.GetStreamingChatMessageContentsAsync(
                   history, settings, kernel, ct))
{
    if (chunk.Content is { Length: > 0 } delta)
        await sse.WriteAsync(new { type = "delta", content = delta }, ct);
}
```

### Security Design

- **AuthN:** React SPA uses **MSAL.js** with Authorization Code + PKCE against the corporate Entra tenant. API validates JWTs via `Microsoft.Identity.Web` (`AddMicrosoftIdentityWebApi`), checking issuer, audience, and the `scp` claim contains `Chat.Access`.
- **AuthZ / data isolation:** every query is filtered by the caller's `oid`. A user can never read another user's conversation; `403` if the row's `UserId` mismatches. Enforced in a single `IUserContext` accessor injected into the store, plus an EF Core global query filter.
- **Managed identity to Azure OpenAI:** API runs with a **user-assigned managed identity** granted the `Cognitive Services OpenAI User` role on the Azure OpenAI resource — no API keys anywhere. SK uses `new AzureOpenAIChatCompletionService(deployment, endpoint, new DefaultAzureCredential())`.
- **Secrets:** the only secrets (e.g., SQL connection if not using MI auth) live in **Key Vault**, referenced from Container Apps via Key Vault references; prefer **Azure AD auth to SQL** (managed identity) so there's no connection-string secret at all.
- **Content safety + prompt injection:** enable **Azure AI Content Safety** prompt-shield and the Azure OpenAI default content filters; strip/escape user content before placing it in the system prompt context, and use a fixed system prompt that instructs the model to ignore instructions embedded in user-supplied text.
- **Rate limiting:** per-user token-bucket (ASP.NET Core rate limiting middleware) returns `429` to protect the OpenAI quota.

### Deployment Architecture

- **Hosting:** **Azure Container Apps** (one app for the API, one for the static SPA via nginx sidecar — or host SPA on **Azure Static Web Apps**). Container Apps scale-to-zero keeps the portfolio cheap; min replicas 1 in prod to avoid cold-start on streaming.
- **Environments:** `dev` and `prod` Container Apps environments, parameterized by Bicep. PR builds deploy to an ephemeral `pr-<n>` revision label.
- **CI/CD:** **GitHub Actions** — build + test → build/push image to **ACR** → `az containerapp update` with the new image digest. OIDC federation from GitHub to Entra (no stored cloud credentials).
- **IaC:** **Bicep**, `main.bicep` composing modules.

```bicep
// modules/openai.bicep (excerpt)
resource oai 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: oaiName
  location: location
  kind: 'OpenAI'
  sku: { name: 'S0' }
  properties: { customSubDomainName: oaiName, publicNetworkAccess: 'Enabled' }
}
resource gpt 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: oai
  name: 'gpt-4o'
  sku: { name: 'GlobalStandard', capacity: 30 }
  properties: { model: { format: 'OpenAI', name: 'gpt-4o', version: '2024-11-20' } }
}
```

### Azure Services Used

- **Azure OpenAI** — `gpt-4o` for chat completion (streaming), `text-embedding-3-large` available for later memory features. (GA.)
- **Azure Container Apps** — serverless container hosting for API and SPA. (GA.)
- **Azure SQL Database** — durable chat/message storage. (GA.)
- **Microsoft Entra ID** — SSO, token issuance, app registrations for SPA + API. (GA.)
- **Azure Key Vault** — secret/cert storage; KV references in Container Apps. (GA.)
- **Azure Container Registry** — private image registry. (GA.)
- **Application Insights / Log Analytics** — OpenTelemetry traces, token-usage metrics. (GA.)
- **Azure AI Content Safety** — prompt shields + content filtering. (GA.)

### Resume Bullet Points

- Designed and shipped an internal streaming AI chat assistant on **Azure OpenAI + Semantic Kernel**, serving **400+ employees** with **token-by-token SSE streaming** that cut perceived response latency by **~70%** versus blocking responses.
- Implemented **passwordless, key-free** access to Azure OpenAI and Azure SQL using **user-assigned managed identities** and Entra ID, eliminating **100% of stored credentials** from the runtime and passing the firm's security review on first pass.
- Built per-user conversation memory and history on **Azure SQL** with EF Core global query filters, enforcing strict **row-level data isolation** across all tenants/users with zero cross-user leakage in penetration testing.
- Integrated **Azure AI Content Safety prompt shields** and per-user token rate limiting, reducing prompt-injection exposure and capping OpenAI spend at a predictable **per-seat cost**.
- Automated build-test-deploy with **GitHub Actions OIDC + Bicep** to **Azure Container Apps**, achieving **sub-5-minute** deployments and reproducible `dev`/`prod` environments.

### GitHub Portfolio Presentation

- **README outline:** one-line pitch → animated GIF of streaming chat → "Architecture" (embed the ASCII/Mermaid diagram) → "Run locally in 5 minutes" (docker-compose with an Azure OpenAI key for local dev, MI in cloud) → "How streaming works" (SSE + SK code snippet) → "Security model" (MI, scopes, isolation) → "Deploy to Azure" (`azd up`) → cost note.
- **Demo:** record a 30-second Loom/GIF showing login → streamed answer → reopening history. Provide a public read-only demo behind Entra "guest" sign-in if budget allows; otherwise the GIF + a "Deploy your own" button.
- **Highlight to reviewers:** the `useChatStream.ts` hook and the SSE endpoint (clean streaming), the managed-identity wiring (no keys), and the Bicep. Pin this repo and lead the README with "Microsoft-stack, production patterns, not a notebook."
