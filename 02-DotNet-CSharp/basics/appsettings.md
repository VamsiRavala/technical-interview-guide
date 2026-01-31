# appsettings.json — Practical Guide for ASP.NET Core

> A concise, production-focused reference for how **appsettings.json** powers configuration in ASP.NET Core, how it’s loaded, overridden, and bound to strongly-typed options.

---

## 1) What is `appsettings.json`?
`appsettings.json` is your app’s **configuration file**. It holds values you don’t want hard-coded—URLs, connection strings, feature flags, Kestrel options, logging levels, etc.—so the same build can run in multiple environments by just changing config.

**Key benefits**
- Centralizes config (no recompiles for value changes)
- Environment-specific overrides (Development/Staging/Production)
- Supports hot reload of config values (when providers are set to reload)
- Strongly-typed binding to options (`IOptions<T>`, `IOptionsSnapshot<T>`, `IOptionsMonitor<T>`)

---

## 2) How configuration is loaded (order & precedence)
When you use `var builder = WebApplication.CreateBuilder(args);`, ASP.NET Core loads configuration from several providers; **later sources override earlier ones**:

1. `appsettings.json`
2. `appsettings.{Environment}.json` (e.g., `appsettings.Production.json`)
3. **User Secrets** (Development only, if configured)
4. **Environment variables**
5. **Command-line arguments**

> Tip: Use environment variables or CLI args to override sensitive or deployment-specific values without modifying files.

---

## 3) Typical `appsettings.json` contents

```json
{
  "ConnectionStrings": {
    "Default": "Server=...;Database=...;User Id=...;Password=...;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Kestrel": {
    "Endpoints": {
      "Http":  { "Url": "http://0.0.0.0:8080" },
      "Https": {
        "Url": "https://0.0.0.0:8443",
        "Certificate": {
          "Path": "/etc/ssl/site.pfx",
          "Password": "change-me"
        }
      }
    },
    "Limits": {
      "MaxRequestBodySize": 104857600
    }
  },
  "FeatureFlags": {
    "UseNewCheckout": true
  }
}
```

### Environment-specific overrides
Place only the **differences** in files like `appsettings.Production.json`:
```json
{
  "Logging": { "LogLevel": { "Default": "Warning" } },
  "FeatureFlags": { "UseNewCheckout": false }
}
```

---

## 4) Using configuration in code

### Bind Kestrel and custom options
```csharp
// Program.cs
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Bind Kestrel from config
builder.WebHost.ConfigureKestrel(opts =>
    builder.Configuration.GetSection("Kestrel").Bind(opts));

// Bind FeatureFlags to a typed options class
builder.Services.Configure<MyFeatures>(
    builder.Configuration.GetSection("FeatureFlags"));

var app = builder.Build();

// Example usage of typed options
app.MapGet("/", (IOptions<MyFeatures> features) => new {
    NewCheckoutEnabled = features.Value.UseNewCheckout
});

app.Run();

public sealed class MyFeatures
{
    public bool UseNewCheckout { get; set; }
}
```

### Options lifetimes (which one to choose?)
- `IOptions<T>` — snapshot at startup; **does not** update on file change.
- `IOptionsSnapshot<T>` — scoped per request; picks up changes on the **next** request (web apps).
- `IOptionsMonitor<T>` — raises change notifications; use for singletons that must react to live changes.

---

## 5) Overriding values (env vars & CLI)

**Environment variables** use `__` to separate levels:
```bash
# Override log level (Linux/macOS)
export Logging__LogLevel__Default=Warning

# Example Kestrel override
export Kestrel__Endpoints__Https__Url=https://0.0.0.0:8443
```

**Command-line arguments** use `:` separators:
```bash
dotnet MyApp.dll --Logging:LogLevel:Default=Warning \
                 --Kestrel:Endpoints:Https:Url=https://0.0.0.0:8443
```

> Precedence: CLI > Environment variables > appsettings.{Environment}.json > appsettings.json

---

## 6) Hot reload of config
The default JSON provider enables `reloadOnChange` for `appsettings.json` in most project templates. Pair this with `IOptionsSnapshot<T>` or `IOptionsMonitor<T>` if you want the app to see changes without a restart.

---

## 7) Good practices & gotchas

**Do**
- Keep production secrets **out** of `appsettings.json`. Prefer environment variables, Secret Manager (dev), or a vault (e.g., Azure Key Vault).
- Keep the file **small & structured**; avoid storing large blobs.
- Mirror limits at the proxy (NGINX/IIS) and Kestrel (e.g., body size/timeouts).

**Avoid**
- Blocking calls in request handlers that depend on config values updating in real time (prefer `IOptionsMonitor<T>` for live changes).
- Divergent limits between NGINX/IIS and Kestrel (align `client_max_body_size` with `MaxRequestBodySize`, etc.).

---

## 8) Quick reference

- File names: `appsettings.json`, `appsettings.{Environment}.json`
- Environment name: `ASPNETCORE_ENVIRONMENT` = `Development|Staging|Production|...`
- Provider order (last wins): base JSON → env JSON → secrets → env vars → CLI
- Typed binding: `services.Configure<T>(configSection)`
- Live updates: `IOptionsSnapshot<T>` / `IOptionsMonitor<T>`

---

## 9) Minimal checklist

- [ ] Decide which values belong in files vs env/vault
- [ ] Add `appsettings.{Environment}.json` for differences
- [ ] Bind Kestrel/feature flags via `Configuration`
- [ ] Choose appropriate `IOptions*` lifetime
- [ ] Align proxy & Kestrel limits/timeouts
- [ ] Verify overrides via env vars or CLI in your deployment scripts

---

**In short:** `appsettings.json` is the **source of truth for configurable behavior** in ASP.NET Core, with clean environment overrides and first-class, strongly-typed binding.
