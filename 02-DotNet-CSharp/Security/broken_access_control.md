# Broken Access Control
Broken Access Control is any flaw that lets users perform actions or read/write data they’re not authorized to. It’s not about who you are (authentication); it’s about what you’re allowed to do (authorization). You fix it by enforcing rules on the server—consistently—at every layer that handles resource access.

## Canonical failure modes

- IDOR / BOLA (object-level): /api/orders/{id} returns an order you don’t own.

- BFLA (function-level): Non-admin can call admin endpoints.

- Horizontal/vertical escalation: Read/update other users’ rows, or jump to higher privileges.

- Multi-tenant leaks: Tenant A can see Tenant B’s data.

- Mass assignment: Client sets fields like isAdmin, tenantId.

- Enumeration: Guessing predictable IDs without rate limits or opaque keys.

- Client-side checks only: Hidden buttons ≠ security.

- Over-permissive CORS: Lets a hostile origin use user cookies.

## Design principles (you’ll use these everywhere)

- Deny by default. Require explicit allow.

- Centralize enforcement. Policies/handlers, not scattered ifs.

- Server-side only. Client checks are UX, not security.

- Least privilege. Prefer fine-grained policies over mega “Admin”.

- Per-resource checks. Role/claim gates + ownership/tenant gates.

- Whitelisting DTOs. Never bind entities directly from user input.

- Defense in depth. Rate limits, opaque IDs, strict CORS, audit logs.
  
## ASP.NET Core: the right building blocks
  ### 1) Make authorization default
  ```csharp
    // Program.cs (.NET 7/8)
using Microsoft.AspNetCore.Authorization;

builder.Services.AddAuthorization(options =>
{
    // Everything requires an authenticated user unless explicitly allowed
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Example role/claim policy
    options.AddPolicy("CanManageUsers", p => p.RequireRole("Admin"));
});

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

// Lock down a whole area via endpoint routing
app.MapGroup("/admin").RequireAuthorization("CanManageUsers")
   .MapGet("/users", /* ... */);
```
Add [AllowAnonymous] only where you truly need it (login, health checks, webhooks with alt security).

### 2) Resource-based authorization (object-level)
Use IAuthorizationService + a custom AuthorizationHandler<TRequirement, TResource> to check ownership/tenant per record.
```csharp
public sealed class CanEditDocument : IAuthorizationRequirement { }

public sealed class CanEditDocumentHandler 
    : AuthorizationHandler<CanEditDocument, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext ctx,
        CanEditDocument req,
        Document doc)
    {
        var userId = ctx.User.FindFirst("sub")?.Value; // your subject/user id claim
        if (userId is not null && doc.OwnerId == userId)
            ctx.Succeed(req);
        return Task.CompletedTask;
    }
}

// Registration
builder.Services.AddSingleton<IAuthorizationHandler, CanEditDocumentHandler>();

// Use in endpoint
app.MapPut("/docs/{id}", async (
    string id,
    ClaimsPrincipal user,
    IAuthorizationService authz,
    IDocRepo repo,
    DocumentUpdate dto) =>
{
    var doc = await repo.GetAsync(id);
    if (doc is null) return Results.NotFound();

    var decision = await authz.AuthorizeAsync(user, doc, new CanEditDocument());
    if (!decision.Succeeded) return Results.Forbid(); // or NotFound() to avoid leaking existence

    doc.Title = dto.Title;   // map only allowed fields
    await repo.SaveAsync(doc);
    return Results.NoContent();
});
```
