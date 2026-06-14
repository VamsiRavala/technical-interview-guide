# Security & Networking for Azure AI Services

> Identity-first, network-isolated, audited. This file covers the AI-specific security posture: managed identity + `DefaultAzureCredential`, the exact RBAC roles, Private Endpoints, data residency/privacy commitments, and Key Vault. Generic Azure identity/networking lives in **[03-Azure](../03-Azure/README.md)** — here we apply it to Azure OpenAI / AI Search / Foundry.

---

## Identity-First: Managed Identity over API Keys

Prefer **Microsoft Entra ID** authentication over API keys everywhere.

- Assign your app a **managed identity** (system- or user-assigned).
- Grant it an Azure **RBAC role** on the Azure OpenAI resource.
- The app uses **`DefaultAzureCredential`** (or `ManagedIdentityCredential`) to acquire a short-lived AAD **Bearer token** for scope `https://cognitiveservices.azure.com/.default` — **no secret stored or rotated.**

Benefits: no key leakage, automatic rotation, centralized access control + auditing, conditional-access/PIM support.

**Best practices:**
- **Disable local (key) auth** on the resource to enforce identity-only access.
- **Least privilege** — User vs Contributor (below).
- Use **user-assigned identities** for shared scenarios.
- Combine with **Private Endpoints**.

### The RBAC roles (know these by name)

| Role | Grants |
|---|---|
| **Cognitive Services OpenAI User** | **Call models / run inference** (the key data-plane role for your app) |
| **Cognitive Services OpenAI Contributor** | The above **plus** management — e.g., creating deployments |
| **Cognitive Services Contributor** | Manage the Cognitive Services / AI Services account |

For an app that just calls the model, grant **Cognitive Services OpenAI User**. Naming this role precisely signals real Azure fluency in interviews.

### The canonical secure .NET pattern

```csharp
using Azure.AI.OpenAI;
using Azure.Identity;

// Managed identity in Azure; falls back to your dev identity locally.
var client = new AzureOpenAIClient(
    new Uri("https://my-aoai.openai.azure.com/"),
    new DefaultAzureCredential());

var chat = client.GetChatClient("gpt-4o-prod"); // DEPLOYMENT name, not model
var resp = await chat.CompleteChatAsync(
[
    new SystemChatMessage("You are concise."),
    new UserChatMessage("Summarize Azure OpenAI in one line.")
]);
Console.WriteLine(resp.Value.Content[0].Text);
```

- Endpoint + deployment name come from **configuration** (App Config / Key Vault), not code.
- `DefaultAzureCredential` chains: managed identity in Azure → Azure CLI / VS / env locally.
- The SDK honors **429/`Retry-After`**; enable **OpenTelemetry**; run behind **Private Endpoints / APIM**.

The same `DefaultAzureCredential` pattern works for **Azure AI Search** (`SearchClient`) — grant the identity the appropriate Search data role (e.g., **Search Index Data Reader/Contributor**) so the RAG data flow is also keyless.

---

## Private Endpoints & Network Isolation

To keep traffic off the public internet:

```text
              VNet
   ┌───────────────────────────────────────┐
   │  App Service / AKS / Functions (MI)     │
   │            │                            │
   │            ▼  private IP                 │
   │   [Private Endpoint] ── Azure OpenAI     │  public access DISABLED
   │   [Private Endpoint] ── Azure AI Search  │
   │   [Private Endpoint] ── Storage / KV     │
   │   Private DNS zones resolve to PE IPs    │
   └───────────────────────────────────────┘
        ▲ ExpressRoute / VPN for on-prem
```

Standard enterprise posture =
**Private Endpoint + disable public network access + Private DNS zones + VNet-integrated callers + managed identity**, optionally fronted by an **APIM GenAI gateway**.

- A **Private Endpoint** maps a private IP in your VNet to the resource; **disable public network access** so only VNet (or peered/on-prem via ExpressRoute/VPN) traffic reaches it.
- **Private DNS zones** resolve the resource FQDN to the private IP.
- **VNet integration** for your compute keeps calls on the Microsoft backbone.
- Give **dependent services** (AI Search, Storage) Private Endpoints too, so the **whole RAG data flow is private**.
- Layer **NSGs/firewall**; send **diagnostic logs** to Log Analytics over private links.

---

## Data Residency & Privacy Commitments

Interview-grade headline points:

- **Regional data residency** — prompts/completions are **processed in the resource's region** (or chosen Data Zone/Global scope — see `02-deployments-quota-and-ptus.md`).
- **Your data is NOT used to train** the OpenAI/Microsoft foundation models, and is **not shared with OpenAI**.
- Data is **yours**, encrypted at rest and in transit.
- By default, prompts/completions may be **temporarily stored up to 30 days for abuse monitoring**, accessible only to authorized Microsoft reviewers on flagged activity — and eligible customers can **apply to opt out of abuse monitoring / human review** so nothing is stored.
- Fine-tuned models and uploaded data stay within your **resource/region**.

Combine with **Private Endpoints**, **CMK (customer-managed keys)** for encryption, **managed identity**, and audit logging for a full posture.

---

## Key Vault & Secrets

- Store any unavoidable secrets (third-party API keys, connection strings) in **Azure Key Vault**; reference them via managed identity — never in code or config files.
- Prefer **keyless** auth (managed identity) for Azure services so there's little to store.
- Keep **endpoint/deployment names** in **App Configuration / Key Vault** so model upgrades are a config switch, not a redeploy.
- Foundry **connections** to dependent services route secrets through Key Vault.

---

## Logging & Audit for Compliance

- Enable **diagnostic settings** on the resource → **Log Analytics / Storage / Event Hub** (request metadata, errors, content-filter events, token/latency metrics).
  - Note: platform logs **don't capture full prompt/completion content** by default (privacy). For content audit, log at the **application layer** with proper access controls.
- **Application-layer audit** per request: timestamp, user/identity, deployment/model + version, token counts, finish reason, citations (RAG), correlation ID — in a secured, RBAC-controlled store with a **retention policy** and PII handling; consider **immutability**.
- Use an **APIM gateway** to centrally meter **per-consumer** usage for chargeback (see `07-apim-ai-gateway.md`).
- Track **content-filter triggers** and **abuse signals**; alert on anomalies via Azure Monitor.

---

## Monitoring (operational essentials)

Azure OpenAI emits metrics to **Azure Monitor** (token usage, request count, latency, **429/throttling**, **PTU utilization**) and diagnostic logs to **Log Analytics / App Insights**. Instrument the app with **OpenTelemetry** (prompt/response metadata, token counts, latency, finish reasons), correlating across RAG/agent steps. Build dashboards/alerts on: **cost (tokens), capacity (TPM/PTU), reliability (429/errors/latency), safety (content-filter triggers)** — with per-application attribution.

---

## Brush-Up Cheat-Sheet

- **Managed identity + `DefaultAzureCredential`** over keys; scope `https://cognitiveservices.azure.com/.default`; **disable local key auth**.
- Data-plane role = **Cognitive Services OpenAI User** (inference); **Contributor** adds deployment management.
- Network posture = **Private Endpoint + disable public access + Private DNS + VNet-integrated callers + managed identity**, optionally APIM-fronted; give AI Search/Storage PEs too.
- Privacy headlines: **regional residency**, **not used to train**, **not shared with OpenAI**, **abuse-monitoring opt-out**, **CMK** available.
- **Key Vault** for unavoidable secrets; endpoint/deployment in **App Config**; audit at platform **and** app layer with retention + RBAC.
