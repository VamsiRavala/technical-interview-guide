# Authentication with Microsoft Entra ID

> Enterprise AI apps authenticate users with **Entra ID** and call a protected ASP.NET Core API with a **scoped access token** — never the ID token, and never an Azure OpenAI key. This file covers MSAL.js / `@azure/msal-react`, silent token acquisition, calling your API, and the On-Behalf-Of (OBO) flow that reaches Azure OpenAI server-side. General SPA/routing knowledge is assumed (section **01-React-JS**).

---

## The one rule everything follows from

> The browser holds a **user token scoped only to your own API**. The backend holds the **secrets** (Azure OpenAI key/managed identity, Power BI service principal) and does the privileged downstream calls.

The browser must **never** acquire a token for Azure OpenAI directly. Doing so would expose the broad `cognitiveservices` scope to client code and break per-user audit, content safety, rate limiting, and RAG context assembly. The SPA's only auth job is to obtain and send a correctly-scoped token for your API.

```text
User signs in (Entra ID)
   │  MSAL acquires token for  api://<your-api>/access_as_user
   ▼
React SPA ── Bearer (user token) ──► ASP.NET Core API
                                        │  OBO exchange (UserAssertion)
                                        │  → token for cognitiveservices.azure.com/.default
                                        ▼
                                   Azure OpenAI   (user identity preserved end-to-end)
```

## MSAL configuration

Wrap the app in `MsalProvider`. Cache tokens in **sessionStorage**, not localStorage — localStorage is readable by any XSS payload, and AI apps render untrusted model output. The scope you request is **your API's** `access_as_user`, not Microsoft Graph and not Azure OpenAI.

```tsx
// auth/msalConfig.ts
import { PublicClientApplication, Configuration } from "@azure/msal-browser";

const config: Configuration = {
  auth: {
    clientId: process.env.NEXT_PUBLIC_ENTRA_CLIENT_ID!,
    authority: `https://login.microsoftonline.com/${process.env.NEXT_PUBLIC_TENANT_ID}`,
    redirectUri: process.env.NEXT_PUBLIC_REDIRECT_URI,
  },
  cache: { cacheLocation: "sessionStorage" }, // not localStorage — XSS exposure
};

export const msalInstance = new PublicClientApplication(config);

// Scope for YOUR ASP.NET Core API, not Graph and not Azure OpenAI directly.
export const apiRequest = {
  scopes: [`api://${process.env.NEXT_PUBLIC_API_CLIENT_ID}/access_as_user`],
};
```

```tsx
// app/providers.tsx  ("use client")
import { MsalProvider } from "@azure/msal-react";
import { msalInstance } from "@/auth/msalConfig";

export function AuthProvider({ children }: { children: React.ReactNode }) {
  return <MsalProvider instance={msalInstance}>{children}</MsalProvider>;
}
```

Gate routes with `useIsAuthenticated` / `MsalAuthenticationTemplate`, and read `useMsal()` for the active account.

## Acquiring a token and calling the protected API

Acquire silently with `acquireTokenSilent` (uses MSAL's in-memory cache and hidden-iframe / refresh-token renewal). Fall back to popup or redirect **only** on `InteractionRequiredAuthError`. Attach the token as a bearer header. Acquire a **fresh** token right before each call — important for long-lived AI sessions where a previous token may have expired.

```typescript
// lib/apiClient.ts
import { msalInstance, apiRequest } from "@/auth/msalConfig";
import { InteractionRequiredAuthError } from "@azure/msal-browser";

async function getAccessToken(): Promise<string> {
  const account = msalInstance.getActiveAccount();
  if (!account) throw new Error("No active account");
  try {
    const res = await msalInstance.acquireTokenSilent({ ...apiRequest, account });
    return res.accessToken;
  } catch (e) {
    if (e instanceof InteractionRequiredAuthError) {
      const res = await msalInstance.acquireTokenPopup(apiRequest);
      return res.accessToken;
    }
    throw e;
  }
}

export async function apiFetch<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = await getAccessToken();
  const res = await fetch(`${process.env.NEXT_PUBLIC_API_BASE}${path}`, {
    ...init,
    headers: { ...init.headers, Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw new Error(`API ${res.status}`);
  return res.json() as Promise<T>;
}
```

Never refresh tokens by exposing refresh tokens to JS — rely on MSAL's silent renewal. Tokens live in MSAL's cache + sessionStorage, not in your own variables or localStorage.

## Validating the token on the ASP.NET Core API

The API accepts the audience-correct user token and validates audience + scope before doing anything.

```csharp
// Program.cs — protect the API with Entra ID
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

app.MapPost("/chat/stream", [Authorize] async (HttpContext ctx, ChatRequest req) =>
{
    // RequiredScope verifies the token carries access_as_user; user.Identity is the signed-in user.
    // ...
});
```

```csharp
// require the delegated scope explicitly
app.MapControllers().RequireAuthorization(p => p.RequireClaim("scp", "access_as_user"));
```

## On-Behalf-Of (OBO) to reach Azure OpenAI

The API takes the incoming **user** token and exchanges it for a downstream token scoped to Azure OpenAI's `https://cognitiveservices.azure.com/.default`. This preserves the user's identity for downstream authorization — per-user audit, RAG security-trimming over SharePoint/SQL RLS — while the privileged exchange stays server-side.

```csharp
// ASP.NET Core: exchange the incoming user token for an Azure OpenAI token (OBO)
public class AzureOpenAiTokenService(IConfidentialClientApplication cca)
{
    private static readonly string[] Scopes =
        ["https://cognitiveservices.azure.com/.default"];

    public async Task<string> GetTokenOnBehalfOfAsync(string incomingUserToken)
    {
        var assertion = new UserAssertion(incomingUserToken);
        var result = await cca
            .AcquireTokenOnBehalfOf(Scopes, assertion)
            .ExecuteAsync();
        return result.AccessToken; // used as bearer for Azure OpenAI with Entra auth
    }
}
```

For RAG over SharePoint/OneDrive, the same OBO pattern obtains a Microsoft Graph token as the user, so retrieval respects the user's actual document permissions (security trimming). React's only job throughout: acquire and send the correctly-scoped token for *your* API — it never touches downstream tokens.

## Token expiry during a long stream

A streaming chat or a SignalR connection can outlive a short-lived access token:

- **Per request:** call `acquireTokenSilent` immediately before initiating each stream so it starts with a fresh token.
- **SignalR:** supply an `accessTokenFactory` (see `04-signalr-realtime.md`) so the library fetches a current token on connect and reconnect.
- **Mid-stream 401:** abort, re-acquire the token, and retry (from a checkpoint if the backend supports resumable streams). Prefer keeping individual streaming requests bounded and offloading truly long jobs to background processing with SignalR status push.

## Frontend security checklist

| Rule | Why |
|---|---|
| No API keys (Azure OpenAI, Search) in client/`NEXT_PUBLIC_*` | All model calls go through the authenticated BFF/API |
| SPA token scoped only to your API | Limits blast radius; OBO does the downstream calls |
| sessionStorage + in-memory cache, not localStorage | XSS cannot exfiltrate tokens as easily |
| Enforce auth on every backend route | The client token is not a substitute for server checks |
| Validate audience + scope on the API | Prevents token-substitution / confused-deputy |
| Don't expose refresh tokens to JS | Rely on MSAL silent renewal |
| Apply CSP, limit `connect-src` | Restrict where the SPA can send tokens |

## Interview angle

"Why not just call Azure OpenAI from the browser?" — Three reasons: (1) it would require shipping a secret or a broad cognitive-services token to the client; (2) you lose per-user audit, content safety, rate limiting, and RAG context assembly; (3) OBO lets you enforce the *user's* permissions on downstream data (Graph, SharePoint, SQL RLS) inside the same call chain. The browser token is deliberately scoped only to your own API.
