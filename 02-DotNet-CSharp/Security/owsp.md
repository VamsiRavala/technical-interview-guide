# OWASP Security Principles

The **OWASP Top 10** is the most recognized list of application security risks.  
Below are the 2021 categories with short explanations and developer guidelines.

---

## OWASP Top 10 (2021)

### 1. Broken Access Control
- Users must only access resources they’re authorized for.
- **.NET Example:** Use `[Authorize]` attributes and role-based policies. Deny by default.

---

### 2. Cryptographic Failures (Sensitive Data Exposure)
- Protect data at rest and in transit with strong encryption.
- **.NET Example:** Use `System.Security.Cryptography` APIs, enforce TLS 1.2+ in `HttpClient`.

---

### 3. Injection
- Prevent SQL/NoSQL/OS/LDAP injection.
- **.NET Example:** Always use parameterized queries or Entity Framework LINQ (not string concatenation).

---

### 4. Insecure Design
- Security must be built into the design (threat modeling, secure patterns).
- **.NET Example:** Plan for multi-tenant separation, secure session management before coding.

---

### 5. Security Misconfiguration
- Don’t use default credentials or leave unnecessary features enabled.
- **.NET Example:** Disable detailed error messages in production (`UseExceptionHandler` not `UseDeveloperExceptionPage`).

---

### 6. Vulnerable and Outdated Components
- Keep frameworks, servers, and libraries patched.
- **.NET Example:** Run `dotnet list package --outdated`, fix vulnerable NuGet dependencies, monitor NVD and GitHub Dependabot alerts.

---

### 7. Identification and Authentication Failures
- Strong authentication and session management.
- **.NET Example:** Use ASP.NET Identity with MFA, avoid session IDs in URLs, configure secure cookies.

---

### 8. Software and Data Integrity Failures
- Protect CI/CD pipelines and verify software integrity.
- **.NET Example:** Sign assemblies, validate package sources (`nuget.org`), enforce checksum validation in builds.

---

### 9. Security Logging and Monitoring Failures
- Log security-relevant events and monitor them.
- **.NET Example:** Use `ILogger` with centralized sinks (e.g., Seq, ELK), protect log storage, never log sensitive data.

---

### 10. Server-Side Request Forgery (SSRF)
- Prevent attackers from tricking servers into making unintended requests.
- **.NET Example:** Validate/whitelist URLs before making `HttpClient` calls, block internal/private IP ranges.

---

## Core OWASP Security Principles

- **Least Privilege** – Users/services should only have the minimum required access.  
- **Defense in Depth** – Multiple layers of security controls (auth + input validation + monitoring).  
- **Fail Securely** – Default to secure settings; do not reveal sensitive info in errors.  
- **Don’t Trust User Input** – Validate, sanitize, and encode all external input.  
- **Keep Security Simple** – Avoid complexity that creates security holes.  
- **Security by Design** – Integrate security into the architecture from the start.

---

## Interview Tip
Be prepared to:
- Map each OWASP principle to a **real-world .NET example**.
- Explain how to detect and fix a **vulnerable NuGet dependency**.
- Demonstrate secure API design (OAuth2, JWT, input validation).
