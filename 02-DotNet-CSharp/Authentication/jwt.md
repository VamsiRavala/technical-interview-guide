# JWT for Web APIs — Complete Nutshell

## 🔹 What Is a JWT?
A **JSON Web Token (JWT)** is a compact, signed token that carries **claims** (who the user is, what they can do, when the token expires). Most APIs use it as a **Bearer** token:

```
Authorization: Bearer <jwt>
```

---

## 🔹 How It Works (Diagrams)

### A) Issue Token (Authorization Code w/ PKCE or Password for demo)
```mermaid
sequenceDiagram
    participant Client
    participant AuthServer as Auth Server (OIDC/OAuth2)
    participant API

    Client->>AuthServer: Authenticate (login/consent/PKCE)
    AuthServer-->>Client: access_token (JWT) [+ refresh_token]
    Client->>API: Request with Authorization: Bearer <JWT>
    API->>API: Validate signature, issuer, audience, exp, nbf
    API-->>Client: 200 OK (claims in HttpContext.User)
```

### B) Validate on API
```mermaid
flowchart LR
  A[Incoming Request] --> B{Has Authorization: Bearer}
  B -- No --> C[401 Unauthorized]
  B -- Yes --> D[Validate signature alg, key from JWKS]
  D --> E{Claims valid? iss, aud, exp, nbf}
  E -- No --> C[401 Unauthorized]
  E -- Yes --> F[Build ClaimsPrincipal]
  F --> G[Authorization policies check]
  G -->|Pass| H[Controller/Endpoint]
  G -->|Fail| I[403 Forbidden]
```

---

## 🔹 Simple Example

### Request
```http
GET /api/orders HTTP/1.1
Host: api.example.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### ASP.NET Core — Configure JWT Bearer
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

### Issuing a JWT (demo)
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

---

## 🔹 Built-in Classes (What They Do)

| Type / Constant | Namespace | Purpose |
|---|---|---|
| `JwtBearerDefaults.AuthenticationScheme` | `Microsoft.AspNetCore.Authentication.JwtBearer` | The string `"Bearer"` used by auth middleware. |
| `AddJwtBearer`, `JwtBearerOptions`, `JwtBearerEvents` | `Microsoft.AspNetCore.Authentication.JwtBearer` | Registers handler, configures validation & events. |
| `TokenValidationParameters` | `Microsoft.IdentityModel.Tokens` | Controls `iss`, `aud`, `exp`, keys, clock skew. |
| `JwtSecurityToken`, `JwtSecurityTokenHandler` | `System.IdentityModel.Tokens.Jwt` | Create/parse JWTs. |
| `SymmetricSecurityKey`, `SigningCredentials`, `SecurityAlgorithms` | `Microsoft.IdentityModel.Tokens` | Signing and algorithm choices. |
| `ClaimsPrincipal`, `ClaimsIdentity`, `Claim` | `System.Security.Claims` | Identity object available as `HttpContext.User`. |

---

## 🔹 Managing Claims

- **Where claims come from**: your **Auth Server** (IdP) at issuance time (user info, roles, scopes, tenant, etc.).
- **In the API**: Claims appear in `HttpContext.User` → use **policies**:  
  ```csharp
  options.AddPolicy("Orders.Read", p => p.RequireClaim("scope", "orders.read"));
  options.AddPolicy("TenantPolicy", p => p.RequireClaim("tenant_id"));
  ```
- **Role claims**: Ensure IdP emits `role` (or map to `ClaimTypes.Role`).  
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
- **Multi-tenant**: include `tenant_id` claim and enforce via policy/handler.

---

## 🔹 Expiration & Keeping Sessions Alive

- **Access Token**: short-lived (e.g., **5–15 minutes**). When `exp` passes, validation fails ⇒ **401 Unauthorized**.
- **Refresh Token**: long-lived, **stored securely**; exchanged for new access tokens without re-login.

### Standard Refresh Flow
```mermaid
sequenceDiagram
    participant Client
    participant AuthServer as Auth Server

    Client->>AuthServer: POST /token (grant_type=refresh_token, refresh_token=...)
    AuthServer-->>Client: 200 OK { access_token(new), refresh_token(rotated) }
```
**Best practices**  
- **Rotate** refresh tokens (one-time use) to prevent replay.  
- Short access tokens + long refresh tokens ⇒ “long session” without long-lived access token.  
- For SPAs, store refresh tokens ** http-only secure cookies** (or use backend-for-frontend).

### Sliding Sessions (Server Web Apps)
- Use IdP **session cookie** with max age + idle timeout; issue fresh JWTs while user is active.

---

## 🔹 What Happens When Token Expires? How to Deny Requests

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
- Client should **catch 401**, trigger **refresh-token** call, then **retry** request with the new access token.  
- If refresh also fails (revoked/expired) → force **re-authentication**.

---

## 🔹 Scope of the Identity Class

- `ClaimsIdentity` + `ClaimsPrincipal` represent the **authenticated user identity** for **one request**.  
- Built from the JWT’s claims after successful validation; accessible via `HttpContext.User`.  
- **Authorization policies** read these claims to allow/deny access.  
- **Do not** mutate `HttpContext.User` for persistence; issue a new token if claims change.

---

## 🔹 Pros & Cons (JWT as Bearer)

| ✅ Pros | ❌ Cons |
|---|---|
| Stateless, scales well for microservices | Harder to **instantly revoke** (self-contained) |
| No DB hit per request (claims are in token) | Token leakage == access until expiry |
| Works across platforms & gateways | Larger headers; size/bandwidth considerations |
| First-class support in ASP.NET Core | Requires solid key mgmt & rotation |

---

## 🔹 When to Use / Avoid

**Use JWT when…**
- Stateless **REST APIs**, SPAs, mobile, microservices.
- You need **federation/SSO** and **claims-based** authorization.
- You can live with revocation at **expiry** (plus refresh rotation, blocklists for high risk).

**Avoid / reconsider when…**
- You need **immediate revocation** for all tokens (prefer **opaque tokens + introspection** or very short expiry).
- Extremely sensitive data where **mTLS** and reference tokens are mandated.
- Heavy/volatile claims (token size or change frequency) — push details to the API/DB and keep tokens slim.

---

## 🔹 Quick Checklist

- [ ] Validate `iss`, `aud`, `exp`, `nbf`, and signature
- [ ] Short-lived access tokens (5–15 min)
- [ ] Refresh-token flow with **rotation**
- [ ] Key rotation (JWKS; `kid` in header)
- [ ] Minimal claims; map roles & scopes
- [ ] Return **401** on expiry; client retries after refresh
- [ ] Use HTTPS; secure token storage (no localStorage for SPAs)

---

### ✅ Bottom Line
JWTs make Web APIs **portable and scalable**. Keep access tokens **short-lived**, manage **claims and scopes** carefully, use **refresh tokens** for longer sessions, and **fail closed** (401) on expiry with a clear path to re-authenticate.
