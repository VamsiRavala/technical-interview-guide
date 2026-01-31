# Authentication Types in Web API — Cheatsheet

---

## 🔹 1. Basic Authentication
- **How it works**: Username + password are sent in the `Authorization: Basic <base64>` header.
- **Pros**: Very simple; supported everywhere.
- **Cons**: Credentials sent each request (must use HTTPS); no token revocation.
- **Use When**: Simple internal APIs, testing, prototyping.

---

## 🔹 2. Token-Based Authentication (Custom/Bearer Tokens)
- **How it works**: User logs in → API issues a token (string) → Client sends `Authorization: Bearer <token>` on each request.
- **Pros**: Credentials not sent every time; tokens can expire.
- **Cons**: Token storage and rotation logic needed.
- **Use When**: Stateless APIs, custom auth flows.

---

## 🔹 3. JWT (JSON Web Token) Authentication
- **How it works**: API issues **signed JWT** containing claims; client sends it as `Authorization: Bearer <jwt>`.
- **Pros**: Stateless; self-contained (no DB lookup needed for every request); widely used.
- **Cons**: Large token size; hard to revoke early.
- **Use When**: Modern REST APIs, microservices, SPAs, mobile apps.

---

## 🔹 4. Cookie Authentication
- **How it works**: Server issues a session cookie on login → sent automatically with each request.
- **Pros**: Works natively with browsers; integrates with ASP.NET Identity.
- **Cons**: Not ideal for pure APIs; CSRF concerns.
- **Use When**: Traditional web apps, Razor/MVC apps with Web API backend.

---

## 🔹 5. OAuth 2.0
- **How it works**: Delegated authorization via an Authorization Server (Google, Azure AD, etc.); API trusts tokens issued by IdP.
- **Pros**: Industry standard; supports scopes/roles; secure.
- **Cons**: Complex to implement; requires external identity provider or server.
- **Use When**: Third-party logins, enterprise apps, APIs exposed to public clients.

---

## 🔹 6. OpenID Connect (OIDC)
- **How it works**: Layer on top of OAuth 2.0; provides identity info (who user is) + access token.
- **Pros**: Standard for authentication + identity; integrates with external providers (Google, Microsoft, etc.).
- **Cons**: Setup complexity.
- **Use When**: SSO, federated identity, enterprise systems.

---

## 🔹 7. API Keys
- **How it works**: Client includes a static key in header/query (`x-api-key`).
- **Pros**: Very simple; easy to implement.
- **Cons**: No user identity; hard to rotate/manage securely; less secure if exposed.
- **Use When**: Service-to-service communication, internal APIs.

---

## 🔹 8. Windows / Integrated Authentication (Kerberos/NTLM)
- **How it works**: Uses Windows credentials (Active Directory) for auth.
- **Pros**: Seamless in intranets; strong AD integration.
- **Cons**: Not suited for internet/public APIs.
- **Use When**: Internal enterprise APIs on Windows domain.

---

# ✅ Quick Summary Table

| Auth Type         | Transport | State | Best For |
|-------------------|-----------|-------|----------|
| **Basic**         | Header    | Stateless | Simple/test APIs |
| **Bearer Token**  | Header    | Stateless | Custom token flows |
| **JWT**           | Header    | Stateless | Modern APIs, SPAs |
| **Cookie**        | Cookie    | Stateful | Browser-based apps |
| **OAuth 2.0**     | Header    | Stateless | Public/3rd-party APIs |
| **OpenID Connect**| Header    | Stateless | SSO, identity federation |
| **API Key**       | Header/QS | Stateless | Service-to-service |
| **Windows Auth**  | Protocol  | Stateful | Intranet/AD apps |

---
