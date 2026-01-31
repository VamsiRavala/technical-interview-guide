# Bearer Token (JWT) Authentication in Web API — Nutshell Guide

## 🔹 What Is It?
**Bearer Token** authentication sends a token (often a **JWT: JSON Web Token**) in the `Authorization` header on each request:
```
Authorization: Bearer <token>
```
The API validates the token’s signature and claims (issuer, audience, expiry) and grants access.

---

## 🔹 How It Works (Diagram)

```mermaid
sequenceDiagram
    participant Client
    participant AuthServer as Auth Server
    participant API

    Client->>AuthServer: POST /token (username/password or OAuth2 flow)
    AuthServer-->>Client: 200 OK { access_token: <JWT>, expires_in, refresh_token? }
    Client->>API: GET /api/data with Authorization: Bearer <JWT>
    API->>API: Validate signature + issuer/audience + expiry
    API-->>Client: 200 OK (claims available as User)
```

---

## 🔹 Simple Examples

### 1) Client → API (HTTP)
```http
GET /api/products HTTP/1.1
Host: api.example.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 2) cURL
```bash
curl -H "Authorization: Bearer $TOKEN" https://api.example.com/api/products
```

### 3) ASP.NET Core (Minimal API) — Configure JWT Bearer Auth
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

### 4) Issuing a JWT (Demo Only)
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

---

## 🔹 Built-in Classes You’ll See (Explained)

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

---

## 🔹 Pros & Cons

| ✅ Pros | ❌ Cons |
|---|---|
| Stateless (no server session storage) | Harder to revoke immediately (self-contained JWTs) |
| Widely supported across platforms | Token leakage grants full access until expiry |
| Scales well for microservices | Larger headers vs cookies; size matters on mobile |
| Claims travel with request (no DB hit) | Requires careful key management & rotation |

---

## 🔹 When to Use / Avoid

**Use Bearer (JWT) when…**
- Building **stateless REST APIs**, SPAs, mobile apps, or microservices.
- You need **federation/SSO** via OAuth2/OIDC providers.
- You want **fine-grained claims** and role-based authorization.

**Avoid / reconsider when…**
- You need **instant revocation** without introspection (prefer **reference tokens** + introspection).
- Simple server-rendered sites (cookies are simpler, built-in CSRF story).
- Extremely sensitive data where **short tokens + introspection** or **mTLS** is more appropriate.

---

## 🔹 Hardening Tips

- **HTTPS only**; short `exp` (e.g., 5–30 min) + **refresh tokens**.
- Validate **issuer**, **audience**, **lifetime**, **signing key**; set small `ClockSkew`.
- **Rotate signing keys**; version keys (kid) in the header (JWKs if using OIDC).
- Use **scopes/roles** and **authorization policies**; deny by default.
- Store tokens securely on the client (avoid localStorage for SPAs; prefer memory + silent refresh).
- Consider **reference tokens** + token introspection for server-to-server where revocation is critical.

---

## 🔹 Quick Policy Example (Authorization)
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminsOnly", policy =>
        policy.RequireClaim("role", "admin"));
});

app.MapGet("/api/admin", () => "secret").RequireAuthorization("AdminsOnly");
```

---

## 🔹 Troubleshooting
- Use `JwtBearerOptions.Events.OnAuthenticationFailed` to log failures.
- Check `ToShortDateString()` vs UTC skew on `exp`.
- Ensure **Issuer/Audience** in token **match** API configuration.

---

### ✅ Bottom Line
Bearer (JWT) is the **default choice** for modern Web APIs: portable, stateless, and scalable. Pair it with **short-lived tokens**, **refresh flow**, and **strict validation** to keep it secure.
