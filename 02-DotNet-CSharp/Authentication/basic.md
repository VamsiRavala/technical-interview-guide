# Basic Authentication in Web API — Nutshell Guide

## 🔹 How It Works (Diagram)

```mermaid
sequenceDiagram
    participant Client
    participant API

    Client->>API: Request with `Authorization: Basic <base64(username:password)>`
    API->>API: Decode header → validate credentials
    API-->>Client: 200 OK (valid) / 401 Unauthorized (invalid)
```
> The `Basic` header is **Base64 of `username:password`**, not encryption. **Always use HTTPS**.

---

## 🔹 Simple Examples

### 1) Client → API (HTTP)
```http
GET /api/products HTTP/1.1
Host: api.example.com
Authorization: Basic YWRtaW46cGFzc3dvcmQ=   # base64("admin:password")
```

### 2) cURL
```bash
curl -u admin:password https://api.example.com/api/products
# or explicitly:
curl -H "Authorization: Basic $(printf 'admin:password' | base64)" https://api.example.com/api/products
```

### 3) ASP.NET Core (Minimal) — Demo Handler
```csharp
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("Basic", _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/secure", [Authorize] () => Results.Ok(new { Message = "You are in!" }));

app.Run();

// BasicAuthHandler.cs
public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

        try
        {
            var header = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            if (!"Basic".Equals(header.Scheme, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(header.Parameter))
                return Task.FromResult(AuthenticateResult.Fail("Invalid Scheme"));

            var bytes = Convert.FromBase64String(header.Parameter);
            var parts = System.Text.Encoding.UTF8.GetString(bytes).Split(':', 2);
            if (parts.Length != 2) return Task.FromResult(AuthenticateResult.Fail("Invalid Credentials Format"));

            var username = parts[0];
            var password = parts[1];

            // TODO: validate against a user store, hash compare, etc.
            if (username == "admin" && password == "password") // demo only
            {
                var claims = new[] { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
        }
    }
}
```
> Production: validate against a proper user store, use password hashing, rate limiting, and audit logging.

---

## 🔹 Pros & Cons

| ✅ Pros | ❌ Cons |
|---|---|
| Very simple to implement and debug | Sends credentials on **every request** |
| Universal support (browsers, tools, SDKs) | **Must** use HTTPS; Base64 is not encryption |
| Stateless on the server | No built-in expiration/rotation; hard to revoke |
| Good for scripts/internal tools | Susceptible to replay if TLS/nonce not used |

---

## 🔹 When to Use / Avoid

**Use Basic Auth when…**
- Internal/admin tools, scripts, CI jobs in a trusted network
- Quick prototypes or temporary endpoints
- Human-triggered API calls via tools (Postman/cURL)

**Avoid Basic Auth when…**
- Public or mobile/SPA clients → prefer **OAuth2/OIDC or JWT**
- You need token **revocation**, **scopes**, or **short-lived access**
- You require **SSO**, federation, or third‑party login
- Credentials traverse untrusted networks (unless strictly via HTTPS + additional measures)

---

## 🔹 Hardening Tips (if you must use it)
- **HTTPS only**; HSTS enabled
- Store only **hashed** passwords (Argon2/PBKDF2/bcrypt)
- **Rate-limit** and lockout on repeated failures
- Rotate credentials regularly; per‑service accounts
- Prefer **tokens** (JWT/OAuth2) for external/public APIs
