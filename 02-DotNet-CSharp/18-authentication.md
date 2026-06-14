# Authentication & Authorization in ASP.NET Core

> A consolidated guide covering authentication types, Basic auth, Bearer tokens, JWT (issuance, validation, refresh), JWT vs Bearer, OAuth 2.0 / OIDC, authentication servers, and practical implementation references.
>
> Note: .NET 9-specific authentication features are covered separately in `08-auth-2026.md`.

---

## Authentication Types — Cheatsheet

### 1. Basic Authentication
- **How it works**: Username + password are sent in the `Authorization: Basic <base64>` header.
- **Pros**: Very simple; supported everywhere.
- **Cons**: Credentials sent each request (must use HTTPS); no token revocation.
- **Use When**: Simple internal APIs, testing, prototyping.

### 2. Token-Based Authentication (Custom/Bearer Tokens)
- **How it works**: User logs in, API issues a token (string), client sends `Authorization: Bearer <token>` on each request.
- **Pros**: Credentials not sent every time; tokens can expire.
- **Cons**: Token storage and rotation logic needed.
- **Use When**: Stateless APIs, custom auth flows.

### 3. JWT (JSON Web Token) Authentication
- **How it works**: API issues a **signed JWT** containing claims; client sends it as `Authorization: Bearer <jwt>`.
- **Pros**: Stateless; self-contained (no DB lookup needed for every request); widely used.
- **Cons**: Large token size; hard to revoke early.
- **Use When**: Modern REST APIs, microservices, SPAs, mobile apps.

### 4. Cookie Authentication
- **How it works**: Server issues a session cookie on login, sent automatically with each request.
- **Pros**: Works natively with browsers; integrates with ASP.NET Identity.
- **Cons**: Not ideal for pure APIs; CSRF concerns.
- **Use When**: Traditional web apps, Razor/MVC apps with Web API backend.

### 5. OAuth 2.0
- **How it works**: Delegated authorization via an Authorization Server (Google, Azure AD, etc.); API trusts tokens issued by the IdP.
- **Pros**: Industry standard; supports scopes/roles; secure.
- **Cons**: Complex to implement; requires external identity provider or server.
- **Use When**: Third-party logins, enterprise apps, APIs exposed to public clients.

### 6. OpenID Connect (OIDC)
- **How it works**: Layer on top of OAuth 2.0; provides identity info (who the user is) plus an access token.
- **Pros**: Standard for authentication + identity; integrates with external providers (Google, Microsoft, etc.).
- **Cons**: Setup complexity.
- **Use When**: SSO, federated identity, enterprise systems.

### 7. API Keys
- **How it works**: Client includes a static key in header/query (`x-api-key`).
- **Pros**: Very simple; easy to implement.
- **Cons**: No user identity; hard to rotate/manage securely; less secure if exposed.
- **Use When**: Service-to-service communication, internal APIs.

### 8. Windows / Integrated Authentication (Kerberos/NTLM)
- **How it works**: Uses Windows credentials (Active Directory) for auth.
- **Pros**: Seamless in intranets; strong AD integration.
- **Cons**: Not suited for internet/public APIs.
- **Use When**: Internal enterprise APIs on a Windows domain.

### Quick Summary Table

| Auth Type          | Transport | State      | Best For                  |
|--------------------|-----------|------------|---------------------------|
| **Basic**          | Header    | Stateless  | Simple/test APIs          |
| **Bearer Token**   | Header    | Stateless  | Custom token flows        |
| **JWT**            | Header    | Stateless  | Modern APIs, SPAs         |
| **Cookie**         | Cookie    | Stateful   | Browser-based apps        |
| **OAuth 2.0**      | Header    | Stateless  | Public/3rd-party APIs     |
| **OpenID Connect** | Header    | Stateless  | SSO, identity federation  |
| **API Key**        | Header/QS | Stateless  | Service-to-service        |
| **Windows Auth**   | Protocol  | Stateful   | Intranet/AD apps          |

---

## Basic Authentication

### How It Works

```text
Client  --(Authorization: Basic <base64(username:password)>)-->  API
API     decodes header, validates credentials
API     -->  200 OK (valid) / 401 Unauthorized (invalid)
```

> The `Basic` header is **Base64 of `username:password`**, not encryption. **Always use HTTPS**.

### Simple Examples

**1) Client to API (HTTP)**
```text
GET /api/products HTTP/1.1
Host: api.example.com
Authorization: Basic YWRtaW46cGFzc3dvcmQ=   # base64("admin:password")
```

**2) cURL**
```text
curl -u admin:password https://api.example.com/api/products
# or explicitly:
curl -H "Authorization: Basic $(printf 'admin:password' | base64)" https://api.example.com/api/products
```

**3) ASP.NET Core (Minimal) — Demo Handler**
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

### Pros & Cons

| Pros | Cons |
|---|---|
| Very simple to implement and debug | Sends credentials on **every request** |
| Universal support (browsers, tools, SDKs) | **Must** use HTTPS; Base64 is not encryption |
| Stateless on the server | No built-in expiration/rotation; hard to revoke |
| Good for scripts/internal tools | Susceptible to replay if TLS/nonce not used |

### When to Use / Avoid

**Use Basic Auth when:**
- Internal/admin tools, scripts, CI jobs in a trusted network.
- Quick prototypes or temporary endpoints.
- Human-triggered API calls via tools (Postman/cURL).

**Avoid Basic Auth when:**
- Public or mobile/SPA clients — prefer **OAuth2/OIDC or JWT**.
- You need token **revocation**, **scopes**, or **short-lived access**.
- You require **SSO**, federation, or third-party login.
- Credentials traverse untrusted networks (unless strictly via HTTPS + additional measures).

### Hardening Tips (if you must use it)
- **HTTPS only**; HSTS enabled.
- Store only **hashed** passwords (Argon2/PBKDF2/bcrypt).
- **Rate-limit** and lockout on repeated failures.
- Rotate credentials regularly; per-service accounts.
- Prefer **tokens** (JWT/OAuth2) for external/public APIs.

---

## Bearer Token Authentication

### What Is It?
**Bearer Token** authentication sends a token (often a **JWT: JSON Web Token**) in the `Authorization` header on each request:
```text
Authorization: Bearer <token>
```
The API validates the token's signature and claims (issuer, audience, expiry) and grants access.

### How It Works

```text
Client     --(POST /token: username/password or OAuth2 flow)-->  Auth Server
Auth Server -->  200 OK { access_token: <JWT>, expires_in, refresh_token? }
Client     --(GET /api/data with Authorization: Bearer <JWT>)-->  API
API        validates signature + issuer/audience + expiry
API        -->  200 OK (claims available as User)
```

### Simple Examples

**1) Client to API (HTTP)**
```text
GET /api/products HTTP/1.1
Host: api.example.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**2) cURL**
```text
curl -H "Authorization: Bearer $TOKEN" https://api.example.com/api/products
```

**3) ASP.NET Core (Minimal API) — Configure JWT Bearer Auth**
```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1) Configure JWT Bearer authentication
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.FromMinutes(1) // keep short in prod
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// 2) A protected endpoint
app.MapGet("/api/secure", () => Results.Ok(new { Message = "You are in!" }))
   .RequireAuthorization();

app.Run();
```

**4) Issuing a JWT (Demo Only)**
```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

string CreateJwt(string username, IConfiguration config)
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(ClaimTypes.Name, username),
        new Claim("role", "admin")
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: config["Jwt:Issuer"],
        audience: config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(30),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

> In production, issue tokens from a proper **Auth Server** (e.g., OAuth2/OIDC provider). Use HTTPS, strong keys, rotation, and refresh tokens when needed.

### Built-in Classes You'll See

| Class / Constant | Namespace | What it does |
|---|---|---|
| `JwtBearerDefaults.AuthenticationScheme` | `Microsoft.AspNetCore.Authentication.JwtBearer` | The scheme name `"Bearer"` used by the middleware. |
| `AddJwtBearer(...)` / `JwtBearerOptions` | `Microsoft.AspNetCore.Authentication.JwtBearer` | Registers the JWT Bearer handler and configures how tokens are validated. |
| `TokenValidationParameters` | `Microsoft.IdentityModel.Tokens` | Controls validation: issuer, audience, lifetime, key, clock skew, etc. |
| `SymmetricSecurityKey` | `Microsoft.IdentityModel.Tokens` | Wraps your secret key for HMAC signatures. |
| `SigningCredentials` | `Microsoft.IdentityModel.Tokens` | Algorithm + key to sign tokens (e.g., `HmacSha256`). |
| `SecurityAlgorithms` | `Microsoft.IdentityModel.Tokens` | Constants for signing/encryption algorithms. |
| `JwtSecurityToken` | `System.IdentityModel.Tokens.Jwt` | In-memory representation of a JWT. |
| `JwtSecurityTokenHandler` | `System.IdentityModel.Tokens.Jwt` | Creates, reads, and validates JWTs. |
| `Claim`, `ClaimsIdentity`, `ClaimsPrincipal` | `System.Security.Claims` | Identity model exposed as `HttpContext.User`. |

### Pros & Cons

| Pros | Cons |
|---|---|
| Stateless (no server session storage) | Harder to revoke immediately (self-contained JWTs) |
| Widely supported across platforms | Token leakage grants full access until expiry |
| Scales well for microservices | Larger headers vs cookies; size matters on mobile |
| Claims travel with request (no DB hit) | Requires careful key management & rotation |

### When to Use / Avoid

**Use Bearer (JWT) when:**
- Building **stateless REST APIs**, SPAs, mobile apps, or microservices.
- You need **federation/SSO** via OAuth2/OIDC providers.
- You want **fine-grained claims** and role-based authorization.

**Avoid / reconsider when:**
- You need **instant revocation** without introspection (prefer **reference tokens** + introspection).
- Simple server-rendered sites (cookies are simpler, built-in CSRF story).
- Extremely sensitive data where **short tokens + introspection** or **mTLS** is more appropriate.

### Hardening Tips
- **HTTPS only**; short `exp` (e.g., 5-30 min) + **refresh tokens**.
- Validate **issuer**, **audience**, **lifetime**, **signing key**; set small `ClockSkew`.
- **Rotate signing keys**; version keys (`kid`) in the header (JWKs if using OIDC).
- Use **scopes/roles** and **authorization policies**; deny by default.
- Store tokens securely on the client (avoid localStorage for SPAs; prefer memory + silent refresh).
- Consider **reference tokens** + token introspection for server-to-server where revocation is critical.

### Quick Policy Example (Authorization)
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminsOnly", policy =>
        policy.RequireClaim("role", "admin"));
});

app.MapGet("/api/admin", () => "secret").RequireAuthorization("AdminsOnly");
```

### Troubleshooting
- Use `JwtBearerOptions.Events.OnAuthenticationFailed` to log failures.
- Check UTC skew on `exp`.
- Ensure **Issuer/Audience** in token **match** the API configuration.

> **Bottom Line:** Bearer (JWT) is the **default choice** for modern Web APIs: portable, stateless, and scalable. Pair it with **short-lived tokens**, **refresh flow**, and **strict validation** to keep it secure.

---

## JWT — Complete Guide

### What Is a JWT?
A **JSON Web Token (JWT)** is a compact, signed token that carries **claims** (who the user is, what they can do, when the token expires). Most APIs use it as a **Bearer** token:
```text
Authorization: Bearer <jwt>
```

### How It Works

**A) Issue Token (Authorization Code w/ PKCE or Password for demo)**
```text
Client      --(Authenticate: login/consent/PKCE)-->  Auth Server (OIDC/OAuth2)
Auth Server -->  access_token (JWT) [+ refresh_token]
Client      --(Request with Authorization: Bearer <JWT>)-->  API
API         validates signature, issuer, audience, exp, nbf
API         -->  200 OK (claims in HttpContext.User)
```

**B) Validate on API**
```text
Incoming Request
  -> Has Authorization: Bearer?
       No  -> 401 Unauthorized
       Yes -> Validate signature (alg, key from JWKS)
              -> Claims valid? (iss, aud, exp, nbf)
                   No  -> 401 Unauthorized
                   Yes -> Build ClaimsPrincipal
                          -> Authorization policies check
                               Pass -> Controller/Endpoint
                               Fail -> 403 Forbidden
```

### Simple Example

**Request**
```text
GET /api/orders HTTP/1.1
Host: api.example.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**ASP.NET Core — Configure JWT Bearer**
```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-idp.example.com"; // if using OIDC
        options.Audience  = "your-api";
        // Or manual validation:
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://your-idp.example.com",
            ValidAudience = "your-api",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        // Optional: customize failure handling
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                if (ctx.Exception is SecurityTokenExpiredException)
                {
                    ctx.Response.Headers["Token-Expired"] = "true"; // hint to clients
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Orders.Read", p => p.RequireClaim("scope", "orders.read"));
    options.AddPolicy("AdminsOnly", p => p.RequireRole("admin"));
});

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/orders", () => Results.Ok("orders"))
   .RequireAuthorization("Orders.Read");

app.MapGet("/api/admin", () => Results.Ok("secret"))
   .RequireAuthorization("AdminsOnly");

app.Run();
```

**Issuing a JWT (demo)**
```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

string IssueJwt(string subject, IEnumerable<Claim> extraClaims, IConfiguration config)
{
    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, subject),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new(ClaimTypes.Name, subject),
        new("scope", "orders.read orders.write"),
        new(ClaimTypes.Role, "admin")
    };
    claims.AddRange(extraClaims);

    var key  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer:  "https://your-idp.example.com",
        audience:"your-api",
        claims:  claims,
        notBefore: DateTime.UtcNow,
        expires:   DateTime.UtcNow.AddMinutes(15),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Built-in Classes (What They Do)

| Type / Constant | Namespace | Purpose |
|---|---|---|
| `JwtBearerDefaults.AuthenticationScheme` | `Microsoft.AspNetCore.Authentication.JwtBearer` | The string `"Bearer"` used by auth middleware. |
| `AddJwtBearer`, `JwtBearerOptions`, `JwtBearerEvents` | `Microsoft.AspNetCore.Authentication.JwtBearer` | Registers handler, configures validation & events. |
| `TokenValidationParameters` | `Microsoft.IdentityModel.Tokens` | Controls `iss`, `aud`, `exp`, keys, clock skew. |
| `JwtSecurityToken`, `JwtSecurityTokenHandler` | `System.IdentityModel.Tokens.Jwt` | Create/parse JWTs. |
| `SymmetricSecurityKey`, `SigningCredentials`, `SecurityAlgorithms` | `Microsoft.IdentityModel.Tokens` | Signing and algorithm choices. |
| `ClaimsPrincipal`, `ClaimsIdentity`, `Claim` | `System.Security.Claims` | Identity object available as `HttpContext.User`. |

### Managing Claims

- **Where claims come from**: your **Auth Server** (IdP) at issuance time (user info, roles, scopes, tenant, etc.).
- **In the API**: Claims appear in `HttpContext.User`; use **policies**:
  ```csharp
  options.AddPolicy("Orders.Read", p => p.RequireClaim("scope", "orders.read"));
  options.AddPolicy("TenantPolicy", p => p.RequireClaim("tenant_id"));
  ```
- **Role claims**: Ensure the IdP emits `role` (or map to `ClaimTypes.Role`).
  ```csharp
  builder.Services.AddAuthentication(...).AddJwtBearer(o =>
  {
      o.TokenValidationParameters = new TokenValidationParameters
      {
          RoleClaimType = "role", // match your IdP
          NameClaimType = "name"
      };
  });
  ```
- **Custom claims**: Keep them **small** (JWT size matters). For large/volatile data, prefer **claim lookup** per-request.
- **Multi-tenant**: include a `tenant_id` claim and enforce via policy/handler.

### Expiration & Keeping Sessions Alive

- **Access Token**: short-lived (e.g., **5-15 minutes**). When `exp` passes, validation fails resulting in **401 Unauthorized**.
- **Refresh Token**: long-lived, **stored securely**; exchanged for new access tokens without re-login.

**Standard Refresh Flow**
```text
Client     --(POST /token: grant_type=refresh_token, refresh_token=...)-->  Auth Server
Auth Server -->  200 OK { access_token(new), refresh_token(rotated) }
```

**Best practices**
- **Rotate** refresh tokens (one-time use) to prevent replay.
- Short access tokens + long refresh tokens give a "long session" without a long-lived access token.
- For SPAs, store refresh tokens in **http-only secure cookies** (or use a backend-for-frontend).

**Sliding Sessions (Server Web Apps)**
- Use the IdP **session cookie** with max age + idle timeout; issue fresh JWTs while the user is active.

### What Happens When a Token Expires? How to Deny Requests

- The JWT middleware validates `exp` and denies expired tokens with **401**.
- You can surface hints to clients via headers:
  ```csharp
  options.Events = new JwtBearerEvents
  {
      OnChallenge = ctx =>
      {
          // Adds WWW-Authenticate header with error details
          ctx.Response.Headers["WWW-Authenticate"] =
            "Bearer error=\"invalid_token\", error_description=\"The access token expired\"";
          return Task.CompletedTask;
      }
  };
  ```
- The client should **catch 401**, trigger a **refresh-token** call, then **retry** the request with the new access token.
- If refresh also fails (revoked/expired), force **re-authentication**.

### Scope of the Identity Class

- `ClaimsIdentity` + `ClaimsPrincipal` represent the **authenticated user identity** for **one request**.
- Built from the JWT's claims after successful validation; accessible via `HttpContext.User`.
- **Authorization policies** read these claims to allow/deny access.
- **Do not** mutate `HttpContext.User` for persistence; issue a new token if claims change.

### Pros & Cons (JWT as Bearer)

| Pros | Cons |
|---|---|
| Stateless, scales well for microservices | Harder to **instantly revoke** (self-contained) |
| No DB hit per request (claims are in token) | Token leakage means access until expiry |
| Works across platforms & gateways | Larger headers; size/bandwidth considerations |
| First-class support in ASP.NET Core | Requires solid key mgmt & rotation |

### When to Use / Avoid

**Use JWT when:**
- Stateless **REST APIs**, SPAs, mobile, microservices.
- You need **federation/SSO** and **claims-based** authorization.
- You can live with revocation at **expiry** (plus refresh rotation, blocklists for high risk).

**Avoid / reconsider when:**
- You need **immediate revocation** for all tokens (prefer **opaque tokens + introspection** or very short expiry).
- Extremely sensitive data where **mTLS** and reference tokens are mandated.
- Heavy/volatile claims (token size or change frequency) — push details to the API/DB and keep tokens slim.

### Quick Checklist

- [ ] Validate `iss`, `aud`, `exp`, `nbf`, and signature
- [ ] Short-lived access tokens (5-15 min)
- [ ] Refresh-token flow with **rotation**
- [ ] Key rotation (JWKS; `kid` in header)
- [ ] Minimal claims; map roles & scopes
- [ ] Return **401** on expiry; client retries after refresh
- [ ] Use HTTPS; secure token storage (no localStorage for SPAs)

> **Bottom Line:** JWTs make Web APIs **portable and scalable**. Keep access tokens **short-lived**, manage **claims and scopes** carefully, use **refresh tokens** for longer sessions, and **fail closed** (401) on expiry with a clear path to re-authenticate.

---

## JWT vs Bearer

### Bearer Token
- **Definition**: A *Bearer token* is just an **opaque string** (any format) sent in the `Authorization` header:
  ```text
  Authorization: Bearer <token>
  ```
- **Meaning**: "Whoever *bears* this token is authorized."
- **Format**: Arbitrary (could be a GUID, random string, or a JWT).
- **Validation**: API either checks the token directly (e.g., DB lookup) or trusts an **auth server** to have issued it.

### JWT (JSON Web Token)
- **Definition**: A **specific type of token** (self-contained, signed JSON) often used *as* a bearer token.
- **Format**: `header.payload.signature` (Base64Url-encoded). Example:
  ```text
  eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTYiLCJyb2xlIjoiYWRtaW4ifQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
  ```
- **Contains**: Claims about the user (e.g., `sub`, `role`, `exp`).
- **Validation**: Cryptographically validated by the API using signing keys.

### Key Differences

| Aspect | Bearer Token | JWT Token |
|--------|--------------|-----------|
| **Definition** | Auth token in `Authorization: Bearer <token>` | A specific *format* of token (JSON + signature) |
| **Format** | Arbitrary (opaque string, GUID, hash) | Standardized: Header.Payload.Signature |
| **Info inside** | Opaque to client/API unless looked up | Self-contained claims (username, roles, expiry) |
| **Validation** | Typically DB lookup or introspection | Cryptographic signature validation |
| **Revocation** | Easy (delete from DB, invalidate server-side) | Harder (self-contained; revocation list needed) |
| **Size** | Small (opaque string) | Larger (JSON payload + signature) |
| **Use cases** | Internal APIs, reference tokens, quick lookup | Modern REST APIs, SPAs, microservices, SSO |

### Analogy
- **Bearer Token** = your **concert wristband** — if you wear it, you're in. Doesn't matter what's on it; the bouncer checks it against a list.
- **JWT** = a **passport** — contains your info (name, nationality, expiry) and is signed by the issuing authority. The gatekeeper verifies the signature instead of looking it up.

### When to Use Which?

**Bearer (opaque):**
- When revocation/short-lived tokens matter.
- Internal APIs, service-to-service.
- Use with OAuth2 **reference tokens** (validated via introspection endpoint).

**JWT as Bearer:**
- When you want **stateless**, scalable validation (no DB lookup per request).
- REST APIs, SPAs, mobile apps.
- Need **claims inside the token** (roles, scopes, expiry).

**Don't use JWT blindly:**
- If you need instant revocation and can't support introspection.
- If token size matters (mobile, IoT with bandwidth limits).

---

## JWT Refresh Flow — How a Client Knows a Token Expired (or Will)

### 1) From the Token Response
- OAuth2/OIDC `/token` returns `expires_in` (seconds) with an access token.
- Client records:
  `issued_at = now()`
  `expires_at = issued_at + expires_in`

### 2) From the JWT Itself (if access token is a JWT)
- Decode payload (no secret needed) and read `exp` (Unix seconds).
- Compare `exp` to current time (with a **clock skew buffer**, e.g., 60s).

> This is only for client convenience. The server still enforces expiry cryptographically.

### 3) From API Responses
- When expired, the API returns **401 Unauthorized** with header details:
  ```text
  HTTP/1.1 401 Unauthorized
  WWW-Authenticate: Bearer error="invalid_token", error_description="The access token expired"
  ```
- API can add a custom header (e.g., `Token-Expired: true`) for client hints.

### 4) Opaque / Reference Tokens
- No `exp` inside the token; rely on `expires_in` from `/token` or handle **401**.
- Optionally: call the `/introspect` endpoint to check validity.

### Recommended Client Strategy

1. **Track expiry** (`expires_in` or JWT `exp`).
2. **Proactive refresh** a bit **before** expiry (60-120s early).
3. If a request hits **401**, attempt **one** refresh, then retry once.
4. If refresh fails, sign out / re-authenticate.

### Tiny Examples

**A) Decode JWT `exp` (JS/TS)**
```text
function parseJwtExp(token: string): number | null {
  const [,payload] = token.split('.');
  if (!payload) return null;
  const json = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
  return typeof json.exp === 'number' ? json.exp : null; // seconds since epoch
}
```

**B) Schedule Proactive Refresh (JS/TS)**
```text
const SKEW = 60; // seconds
function scheduleRefresh(accessToken: string, refresh: () => Promise<void>) {
  const exp = parseJwtExp(accessToken);
  if (!exp) return;
  const msUntilRefresh = Math.max((exp - SKEW) * 1000 - Date.now(), 0);
  setTimeout(() => { void refresh(); }, msUntilRefresh);
}
```

**C) Fetch Wrapper with 401 Handling (JS/TS)**
```text
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

**D) ASP.NET Core: Add Expiry Hint Header (API)**
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

### Refresh Flow Checklist

- [ ] Use `expires_in` from `/token` and/or JWT `exp` to **refresh early**
- [ ] Handle **401** -> refresh once -> retry once
- [ ] Add **clock skew** buffer (60-120s)
- [ ] For opaque tokens: rely on `expires_in` (and 401), or introspection
- [ ] Server validation is the **source of truth**

> **Bottom Line:** The client knows via **`expires_in`**, **JWT `exp`**, or a **401** response. Best practice: **proactively refresh** before expiry, fallback to refresh on **401**. If refresh fails, re-authenticate the user.

---

## OAuth 2.0 for Web APIs

> Pragmatic guide for ASP.NET Core APIs & clients using OAuth 2.0 and (optionally) OpenID Connect (OIDC).

### OAuth 2.0 vs OIDC (Quick)
- **OAuth 2.0** — Authorization (get an **access token** to call APIs).
- **OpenID Connect (OIDC)** — Authentication layer on top of OAuth 2.0 (adds an **ID token** + user info & discovery).

**Most real-world setups use OIDC** for login, and OAuth 2.0 access tokens for APIs.

### Choose the Right Flow

| Scenario | Flow | Notes |
|---|---|---|
| SPA (browser JS), Mobile | **Authorization Code + PKCE** | No client secret; use PKCE |
| Server-rendered Web App | **Authorization Code** | Confidential client with secret |
| Machine-to-Machine | **Client Credentials** | No user; app identity only |
| Device/Console | **Device Code** | User authorizes on another device |

> Avoid implicit flow. Always prefer **Authorization Code (+PKCE)** for public clients.

### High-Level Flows

**A) Authorization Code (+PKCE)**
```text
1. Native App --(GET /authorize?client_id&redirect_uri&scope&response_type=code&code_challenge)--> IdP
2. IdP        --(302 Redirect to redirect_uri, code={authorization_code})--> Native App
3. Native App --(POST /token: grant_type=authorization_code, code, client_id, redirect_uri, code_verifier)--> IdP
4. IdP        --(200 { access_token, refresh_token, expires_in })--> Native App
5. Native App --(GET /resource, Authorization: Bearer {access_token})--> Web API
6. Web API    validates token (signature, iss, aud, exp), returns 200 Secure data
   ... after a short period, the access token expires ...
7. Native App --(POST /token: grant_type=refresh_token, refresh_token, client_id)--> IdP
8. IdP        --(200 { access_token(new), refresh_token(rotated) })--> Native App
9. Native App --(GET /resource, Authorization: Bearer {new access_token})--> Web API, 200 Secure data
```

**B) Refresh Token Rotation**
```text
Client     --(POST /token: grant_type=refresh_token, refresh_token=old)-->  Auth Server
Auth Server -->  200 OK { access_token:new, refresh_token:rotated }
```

### IdP Setup (Generic Steps)
1. **Create Application** in your Identity Provider (Auth0/Okta/Azure AD/Keycloak/etc.).
2. Configure **Redirect URI** (e.g., `https://app.example.com/callback`) and **Logout URI**.
3. Define **API / Resource** (audience) and **scopes** (e.g., `orders.read`, `orders.write`).
4. Define **roles** and assign them to **users** or **groups**.
5. (Optional) Add **custom claims** in tokens via IdP rules/policies (e.g., `tenant_id`, `department`).
6. Turn on **Refresh Tokens** and **Rotation** (if using long-lived sessions).

### ASP.NET Core — Protecting the API (JWT Bearer)

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-idp.example.com"; // OIDC discovery
        options.Audience  = "your-api";                     // the API identifier ('aud')
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // Optional mappings based on your IdP's claim types:
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Scope-based policy
    options.AddPolicy("Orders.Read", p => p.RequireClaim("scope", "orders.read"));
    options.AddPolicy("Orders.Write", p => p.RequireClaim("scope", "orders.write"));

    // Role-based policy
    options.AddPolicy("AdminsOnly", p => p.RequireRole("admin"));
});

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/orders", () => Results.Ok(new[] { "A1001", "A1002" }))
   .RequireAuthorization("Orders.Read");

app.MapPost("/api/orders", () => Results.Created("/api/orders/123", new { Id = 123 }))
   .RequireAuthorization("Orders.Write");

app.MapGet("/api/admin", () => Results.Ok(new { secret = "42" }))
   .RequireAuthorization("AdminsOnly");

app.Run();
```

**What happens?**
- Middleware validates token signature/issuer/audience/lifetime.
- Builds a **ClaimsPrincipal** (`HttpContext.User`).
- **Authorization policies** enforce scopes/roles before your endpoint runs.

### Claims & Roles (Where They Come From and How to Use Them)

**Where claims/roles come from**
- The **IdP issues them** at token creation. You typically configure this in the IdP's admin UI (rules/mappings).

**Common claims**
- **OIDC**: `sub`, `name`, `preferred_username`, `email`
- **Authorization**: `scope` (space-separated) and **`role`** claims
- **Custom**: `tenant_id`, `department`, `permissions` (keep it small!)

**Using them in ASP.NET Core**
```csharp
// Scope-based authorization
options.AddPolicy("Reports.View", p => p.RequireClaim("scope", "reports.view"));

// Role-based authorization
options.AddPolicy("ManagersOnly", p => p.RequireRole("manager"));

app.MapGet("/api/reports", () => "ok").RequireAuthorization("Reports.View");
app.MapGet("/api/manage",  () => "ok").RequireAuthorization("ManagersOnly");
```

> If your IdP uses different claim names (e.g., `roles` or `permissions`), set `RoleClaimType` and/or use custom **authorization handlers**.

### Client: Getting Tokens (Auth Code + PKCE - Pseudocode)

```text
// 1) Redirect to /authorize with code_challenge (PKCE)
const verifier = generateRandomString();
const challenge = base64url(sha256(verifier));
location.href = `https://idp/authorize?client_id=...&response_type=code&redirect_uri=${cb}&scope=openid%20profile%20orders.read&code_challenge=${challenge}&code_challenge_method=S256`;

// 2) On callback, exchange code + verifier for tokens at /token
POST https://idp/token
  grant_type=authorization_code
  code=...
  redirect_uri=...
  code_verifier=<verifier>

// 3) Store access_token securely (and refresh_token if allowed)
```

> SPAs: avoid localStorage for long-term storage; use **httpOnly secure cookies** or a **BFF** pattern.

### Refresh Workflow (Keep Sessions Alive)

**Access Tokens**
- Short-lived (e.g., **5-15 minutes**). API rejects expired tokens with **401**.

**Refresh Tokens**
- Long-lived, **rotate** on each use. Store **securely** (httpOnly cookie or server session).

**Client refresh call**
```text
POST /token
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token&client_id=...&refresh_token=<old>
```

**Response**
```json
{
  "access_token": "...",
  "refresh_token": "...",
  "expires_in": 900,
  "token_type": "Bearer",
  "scope": "orders.read orders.write"
}
```

**Best practices**
- Enable **reuse detection** and token **rotation** in the IdP.
- Refresh **proactively** (e.g., 60-120s before expiry) + **retry on 401**.
- Re-authenticate when refresh fails (revoked/expired).

### Client Credentials (Machine-to-Machine)

```text
POST /token
grant_type=client_credentials&client_id=...&client_secret=...&scope=orders.read
```
- Token has **no user**; claims represent the **application**.
- Use for **backend-to-backend** calls and scheduled jobs.

### Handling Authentication Failures in API

```csharp
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnChallenge = ctx =>
        {
            // Adds standard WWW-Authenticate header for clients
            ctx.Response.Headers["WWW-Authenticate"] =
              "Bearer error=\"invalid_token\"";
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = ctx =>
        {
            if (ctx.Exception is SecurityTokenExpiredException)
                ctx.Response.Headers["Token-Expired"] = "true";
            return Task.CompletedTask;
        }
    };
});
```
- **401 Unauthorized** — token invalid/expired/missing.
- **403 Forbidden** — token valid, but **missing required scopes/roles**.

### OAuth 2.0 Security Checklist

- [ ] **HTTPS only** (HSTS enabled)
- [ ] Use **Authorization Code + PKCE** for SPAs/mobile
- [ ] Short-lived access tokens; **refresh rotation** enabled
- [ ] Validate `iss`, `aud`, `exp`, `nbf`; small clock skew
- [ ] Minimal claims; set `RoleClaimType`/`NameClaimType` if needed
- [ ] Least-privilege **scopes**; deny by default with policies
- [ ] Key rotation (JWKs, `kid` header); monitor token errors
- [ ] Avoid storing tokens in localStorage; prefer **BFF** or secure cookies

> **Bottom Line:** Configure your **IdP** (scopes, roles, claims). Protect your **API** with `AddJwtBearer` and **policies**. Use **Authorization Code (+PKCE)** to get tokens; **refresh** for long sessions. Enforce **scopes/roles** at the API and return **401/403** appropriately.

---

## Authentication Server (Auth Server)

### What is an Auth Server?
An **Auth Server** is an **API** that issues and validates tokens for clients and APIs, following **OAuth 2.0** and **OpenID Connect (OIDC)** standards. Your Web API trusts tokens issued by this service.

**Common endpoints (per spec):**
- `/authorize` — user login/consent
- `/token` — exchange credentials for tokens
- `/introspect` — check opaque token validity
- `/userinfo` — fetch user claims
- `/.well-known/openid-configuration` — discovery document
- `/jwks` — JSON Web Key Set (public signing keys for JWTs)

### Options for Auth Servers

**1. Managed (Third-Party) IdP**
Examples: Azure AD (Entra ID), Auth0, Okta, AWS Cognito, Google Identity

- **Pros**: Fastest to production; high reliability & security; supports MFA, SSO, social logins out of the box; handles key rotation, logging, monitoring.
- **Cons**: Ongoing per-user costs; vendor lock-in; limited custom flows.
- **Use when**: You need SSO, MFA, social logins quickly; compliance requirements (SOC2, SAML, OIDC); you don't want to maintain auth infra.

**2. Self-Hosted IdP (You Run It)**
Examples: Keycloak, FusionAuth, Ory (Hydra/Kratos), Zitadel, Duende IdentityServer

- **Pros**: Full control of policies and data; can be more cost-effective at scale; on-prem or private cloud ready.
- **Cons**: You own upgrades, patching, scaling; requires in-house security expertise.
- **Use when**: Strict compliance/data residency requirements; complex multi-tenant or custom auth flows; you want to avoid SaaS per-user fees.

**3. Build Your Own (Roll Your Own)**
Implement `/token`, signing, rotation, revocation, refresh, MFA, device flows, etc.

- **Pros**: Maximum flexibility.
- **Cons**: **High risk** (security, correctness, maintenance); easy to mis-implement (rotation, replay, PKCE, nonce, JOSE, etc.).
- **Use when**: Only if **standards IdPs cannot satisfy requirements** (rare).

### How It Fits with Your Web API

```text
Client (SPA/Mobile/Server)  --(OAuth2/OIDC flow)-->  Auth Server / IdP
Auth Server                 --(Issues Token: JWT or opaque)-->  Client
Client                      --(Authorization: Bearer token)-->  Your Web API
Your Web API                --(Validate signature or introspect)-->  Auth Server
```

- **JWT bearer** — API validates using the IdP's `/jwks` (stateless).
- **Opaque bearer** — API calls the IdP's `/introspect` (stateful).

### Decision Table

| Constraint | Best Fit |
|------------|----------|
| Need quick launch + SSO + MFA | Managed IdP (Auth0, Okta, Azure AD) |
| On-prem / strict residency | Self-hosted IdP (Keycloak, FusionAuth, Zitadel) |
| Custom login flows | Self-hosted IdP |
| Easy revocation required | Opaque tokens + introspection |
| Large scale, low latency | JWT + JWKS validation |
| Small team, low security bandwidth | Managed IdP |

### ASP.NET Core Integration

**JWT Bearer**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-idp";
        options.Audience = "your-api";
    });
```

**OIDC (web apps)**
```csharp
builder.Services.AddAuthentication()
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://your-idp";
        options.ClientId = "client-id";
        options.ClientSecret = "secret";
    });
```

### Gotchas & Tips
- Always use **HTTPS**.
- Use **PKCE** for SPAs/mobile.
- Enable **key rotation**; validate `iss`, `aud`, `exp`.
- For **instant revocation**, prefer opaque tokens or short JWT expiry.
- Centralize roles/scopes in the IdP; enforce in the API with policies.

> **Bottom Line:** An **Auth Server is an API** that issues and validates tokens. **Don't build your own** unless absolutely necessary. Prefer **Managed IdPs** first, **Self-Hosted** if compliance/customization demand it. Your API just **validates tokens** and applies **authorization policies**.

---

## Practical Guide — External References

For hands-on walkthroughs of JWT verification and OIDC integration:

- Getting started with Azure APIM and JWT token verification:
  `https://medium.com/devops-dudes/getting-started-with-azure-apim-and-jwt-token-verification-ddf2ef68966b`
- How to implement OpenID Connect in ASP.NET Core Web API:
  `https://medium.com/@callmeyaz/how-to-implement-openid-connect-1-5-asp-net-core-web-api-0aa7263f0bec`
