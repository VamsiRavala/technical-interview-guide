# Power BI Embedding

> Executives want curated, governed reports alongside AI-generated insights — not raw charts. Embed Power BI with `powerbi-client-react`. The critical security rule: the **embed token comes from your ASP.NET Core backend** (service principal + Power BI REST API), and **Row-Level Security (RLS)** is applied so users see only their own data. This file covers the embed component, backend token generation, and RLS. React component basics are assumed (section **01-React-JS**).

---

## The security rule

> Never embed with a master/Pro user token or workspace credentials in the browser. The browser receives only a **short-lived, RLS-scoped embed token** it cannot broaden.

This is the "embed for your customers" pattern: capacity credentials stay server-side, and per-user data security is enforced through RLS roles applied when the backend generates the token.

```text
React  ──► GET /reports/{key}/embed-token  ──► ASP.NET Core
                                                  │  service principal / managed identity
                                                  │  Power BI REST: GenerateToken + EffectiveIdentity (RLS)
                                                  ▼
                                            short-lived embed token (+ embedUrl, reportId)
   ◄──────────────────────────────────────────────┘
PowerBIEmbed renders the report; token cannot be broadened by the client
```

## The embed component

The component receives only the embed config from the backend. Fetch it with TanStack Query (so it caches and you can refresh before expiry), and hide panes/filters the user shouldn't touch.

```tsx
// components/PowerBiReport.tsx  ("use client")  — embed needs browser APIs
import { PowerBIEmbed } from "powerbi-client-react";
import { models } from "powerbi-client";
import { useQuery } from "@tanstack/react-query";
import { apiFetch } from "@/lib/apiClient";

interface EmbedConfig { embedUrl: string; token: string; reportId: string; }

export function PowerBiReport({ reportKey }: { reportKey: string }) {
  // Backend generates an RLS-scoped embed token for THIS user.
  const { data } = useQuery({
    queryKey: ["pbi-embed", reportKey],
    queryFn: () => apiFetch<EmbedConfig>(`/reports/${reportKey}/embed-token`),
    staleTime: 50 * 60_000, // tokens are short-lived; refresh before expiry
  });

  if (!data) return <ReportSkeleton />;

  return (
    <PowerBIEmbed
      embedConfig={{
        type: "report",
        id: data.reportId,
        embedUrl: data.embedUrl,
        accessToken: data.token,
        tokenType: models.TokenType.Embed,
        settings: {
          panes: { filters: { visible: false } },
          navContentPaneEnabled: false,
        },
      }}
      eventHandlers={new Map([
        ["loaded", () => {/* report loaded */}],
        ["error", (e) => console.error(e?.detail)],
      ])}
      cssClassName="pbi-frame"
    />
  );
}
```

## Backend: generating an RLS-scoped embed token

The backend authenticates as a **service principal** (or managed identity), then calls the Power BI REST API to generate a token that carries an `EffectiveIdentity` — the RLS roles for *this* user. The report's dataset defines the RLS roles; the `EffectiveIdentity` activates them.

```csharp
// ASP.NET Core: generate an RLS-scoped embed token (service principal)
public async Task<EmbedTokenDto> GenerateEmbedTokenAsync(
    Guid reportId, Guid datasetId, string userUpn, string[] roles)
{
    var rls = new EffectiveIdentity(
        username: userUpn,
        roles: roles,                       // e.g. ["RegionEast"] — must match RLS roles in the dataset
        datasets: [datasetId.ToString()]);

    var request = new GenerateTokenRequestV2
    {
        Reports = [new GenerateTokenRequestV2Report(reportId)],
        Datasets = [new GenerateTokenRequestV2Dataset(datasetId.ToString())],
        Identities = [rls],                 // enforces row-level security
    };

    var token = await _pbiClient.EmbedToken.GenerateTokenAsync(request);
    return new EmbedTokenDto(token.Token, token.Expiration);
}
```

The user's identity (UPN + roles) is derived from the validated Entra access token on the API — never trusted from the client. This keeps the same end-to-end identity model as the rest of the stack (`03-entra-id-auth.md`).

## Token refresh before expiry

Embed tokens are short-lived; when one expires the report goes **blank**. Refresh proactively before expiry — either by setting `staleTime` below the token lifetime and refetching, or by listening for the SDK's token-expiration event and calling `report.setAccessToken(newToken)` without remounting the iframe.

```typescript
// proactive refresh on the powerbi-client report instance
report.on("tokenExpired", async () => {
  const fresh = await apiFetch<EmbedConfig>(`/reports/${reportKey}/embed-token`);
  await report.setAccessToken(fresh.token);
});
```

## RLS roles checklist

| Step | Where |
|---|---|
| Define RLS roles + table filters (e.g., `[Region] = USERPRINCIPALNAME()`) | Power BI dataset (Desktop) |
| Authenticate as service principal / managed identity | ASP.NET Core backend |
| Derive `userUpn` + `roles` from the validated Entra token | ASP.NET Core backend |
| Pass `EffectiveIdentity` to `GenerateToken` | ASP.NET Core backend |
| Receive only the scoped embed token | React client |
| Refresh before expiry | React client |

## Interview angle

"How do you enforce that a user only sees their data in an embedded report?" — RLS roles defined in the Power BI dataset, plus an `EffectiveIdentity` passed when the **backend** (service principal) generates the embed token. The browser receives only a short-lived, RLS-scoped embed token — it has no ability to broaden its own access. The user identity comes from the validated Entra token on the API, never the client. Refresh the token before expiry or the report goes blank.
