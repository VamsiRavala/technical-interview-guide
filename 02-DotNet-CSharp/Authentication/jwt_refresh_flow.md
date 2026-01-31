# How a Client Knows a Token Expired (or Will)

---

## 🔹 1) From the Token Response
- OAuth2/OIDC `/token` returns `expires_in` (seconds) with an access token.
- Client records:  
  `issued_at = now()`  
  `expires_at = issued_at + expires_in`

---

## 🔹 2) From the JWT Itself (if access token is a JWT)
- Decode payload (no secret needed) → read `exp` (Unix seconds).  
- Compare `exp` to current time (with **clock skew buffer**, e.g., 60s).

> ⚠️ This is only for client convenience. The server still enforces expiry cryptographically.

---

## 🔹 3) From API Responses
- When expired, the API returns **401 Unauthorized** with header details:
  ```http
  HTTP/1.1 401 Unauthorized
  WWW-Authenticate: Bearer error="invalid_token", error_description="The access token expired"
  ```

- API can add a custom header (e.g., `Token-Expired: true`) for client hints.

---

## 🔹 4) Opaque / Reference Tokens
- No `exp` inside the token → rely on `expires_in` from `/token` or handle **401**.  
- Optionally: call `/introspect` endpoint to check validity.

---

## 🔹 Recommended Client Strategy

1. **Track expiry** (`expires_in` or JWT `exp`).  
2. **Proactive refresh** a bit **before** expiry (60–120s early).  
3. If request hits **401** → attempt **one** refresh, then retry once.  
4. If refresh fails → sign out / re-authenticate.  

---

## 🔹 Tiny Examples

### A) Decode JWT `exp` (JS/TS)
```ts
function parseJwtExp(token: string): number | null {
  const [,payload] = token.split('.');
  if (!payload) return null;
  const json = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
  return typeof json.exp === 'number' ? json.exp : null; // seconds since epoch
}
```

### B) Schedule Proactive Refresh (JS/TS)
```ts
const SKEW = 60; // seconds
function scheduleRefresh(accessToken: string, refresh: () => Promise<void>) {
  const exp = parseJwtExp(accessToken);
  if (!exp) return; 
  const msUntilRefresh = Math.max((exp - SKEW) * 1000 - Date.now(), 0);
  setTimeout(() => { void refresh(); }, msUntilRefresh);
}
```

### C) Fetch Wrapper with 401 Handling (JS/TS)
```ts
let refreshing: Promise<void> | null = null;

async function apiFetch(input: RequestInfo, init: RequestInit = {}) {
  const token = await getAccessToken();
  const res = await fetch(input, {
    ...init,
    headers: { ...init.headers, Authorization: `Bearer ${token}` }
  });

  if (res.status !== 401) return res;

  if (!refreshing) refreshing = refreshTokens();
  await refreshing.catch(() => {});
  refreshing = null;

  const newToken = await getAccessToken();
  return fetch(input, {
    ...init,
    headers: { ...init.headers, Authorization: `Bearer ${newToken}` }
  });
}
```

### D) ASP.NET Core: Add Expiry Hint Header (API)
```csharp
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            if (ctx.Exception is SecurityTokenExpiredException)
                ctx.Response.Headers["Token-Expired"] = "true";
            return Task.CompletedTask;
        },
        OnChallenge = ctx =>
        {
            ctx.Response.Headers["WWW-Authenticate"] =
              "Bearer error=\"invalid_token\", error_description=\"The access token expired\"";
            return Task.CompletedTask;
        }
    };
});
```

---

## 🔹 Quick Checklist

- [ ] Use `expires_in` from `/token` and/or JWT `exp` to **refresh early**  
- [ ] Handle **401** → refresh once → retry once  
- [ ] Add **clock skew** buffer (60–120s)  
- [ ] For opaque tokens: rely on `expires_in` (and 401), or introspection  
- [ ] Server validation is the **source of truth**  

---

### ✅ Bottom Line
The client knows via **`expires_in`**, **JWT `exp`**, or a **401** response.  
Best practice: **proactively refresh** before expiry, fallback to refresh on **401**.  
If refresh fails, re-authenticate the user.
