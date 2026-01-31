
# 🧵 Middleware in ASP.NET Core — What, Why, How (with Custom Middleware)

Middleware are **components that inspect, modify, or short‑circuit HTTP requests and responses** as they flow through the **pipeline** in ASP.NET Core. Each middleware decides whether to:
1) **handle** the request and end the pipeline, or  
2) **call the next middleware** and optionally act on the response on the way back.

---

## 🔄 The Request Pipeline (Big Picture)

```
[Incoming HTTP]
   │
   ▼
UseRouting → UseCors → UseAuthentication → UseAuthorization → Custom Middleware → Endpoint
   ▲
   │
[Outgoing HTTP Response]
```

- The pipeline is **ordered** — order matters a lot.
- Each middleware can do **before** (pre‑processing) and **after** (post‑processing) logic.

---

## 🧩 Where to Use Middleware

Typical cross‑cutting concerns that belong in middleware:

- **Logging & Correlation IDs**
- **Global exception handling** / custom error pages
- **Authentication** (who are you?) and **Authorization** (what can you do?)
- **CORS**, **Response compression**, **Caching headers**
- **Request/Response transformation** (e.g., add headers)
- **Localization**, **Rate limiting**, **Security headers**

> If it must happen for **many or all endpoints** and is **request/response oriented**, middleware is usually the right place.

---

## ⚙️ Using Built‑in Middleware (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Order is important!
app.UseExceptionHandler("/error");      // Global error handler (before other middleware)
app.UseHttpsRedirection();
app.UseStaticFiles();                   // Serve wwwroot

app.UseRouting();                       // Routing must come before authz
app.UseCors("DefaultPolicy");

app.UseAuthentication();                // Identify user
app.UseAuthorization();                 // Check permissions

// Custom middleware (example below)
app.UseMiddleware<RequestTimingMiddleware>();

app.MapControllers();                   // Endpoints
app.Run();
```

**Key ordering tips:**
- `UseRouting()` **before** `UseAuthentication()` / `UseAuthorization()` and **before** mapping endpoints.
- `UseCors()` generally **after** routing and **before** auth.
- `UseEndpoints()` / `MapControllers()` or minimal API mappings go **last** to terminate the pipeline.

---

## 🧪 Three Ways to Add Middleware

### 1) `Use(...)` for inline delegates
```csharp
app.Use(async (context, next) =>
{
    // pre
    Console.WriteLine($"--> {context.Request.Method} {context.Request.Path}");
    await next(); // pass to next
    // post
    Console.WriteLine($"<-- {context.Response.StatusCode}");
});
```

### 2) `Map(...)` / `MapWhen(...)` to branch the pipeline
```csharp
app.Map("/health", healthApp =>   // only for /health path
{
    healthApp.Run(async ctx => await ctx.Response.WriteAsync("OK"));
});

app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/api"),
    apiApp => apiApp.Use(async (ctx, next) =>
    {
        ctx.Response.Headers.Add("X-Api", "true");
        await next();
    }));
```

### 3) `UseWhen(...)` for conditional middleware
```csharp
app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/admin"), adminApp =>
{
    adminApp.UseMiddleware<AdminAuditMiddleware>();
});
```

---

## 🧰 Creating **Custom Middleware** (Conventional pattern)

### a) Middleware class
```csharp
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // BEFORE next middleware
        context.Response.Headers.Add("X-Correlation-Id", Guid.NewGuid().ToString());

        try
        {
            await _next(context); // call next in pipeline
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("Request {Method} {Path} took {Elapsed} ms",
                context.Request.Method,
                context.Request.Path,
                sw.ElapsedMilliseconds);
        }
    }
}
```

### b) Registration
```csharp
app.UseMiddleware<RequestTimingMiddleware>(); // Program.cs
```

### c) Optional extension method (syntactic sugar)
```csharp
public static class RequestTimingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder app)
        => app.UseMiddleware<RequestTimingMiddleware>();
}

// Usage
app.UseRequestTiming();
```

---

## 🧰 Creating **Custom Middleware** (IMiddleware pattern)

- Register the middleware type in **DI**.
- The container manages lifetime and dependencies.

```csharp
public class AdminAuditMiddleware : IMiddleware
{
    private readonly ILogger<AdminAuditMiddleware> _logger;
    public AdminAuditMiddleware(ILogger<AdminAuditMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments("/admin"))
        {
            _logger.LogInformation("Admin area accessed by {User}", context.User?.Identity?.Name ?? "anonymous");
        }
        await next(context);
    }
}
```

**Registration & usage:**
```csharp
builder.Services.AddTransient<AdminAuditMiddleware>(); // DI registration
app.UseMiddleware<AdminAuditMiddleware>();             // or app.UseWhen(...)
```

**When to prefer IMiddleware?**
- You want **DI‑controlled lifetime** or constructor injections without using the `RequestDelegate` constructor style.

---

## 🧨 Global Exception Handling Middleware (Practical)

```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next; _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Something went wrong." });
        }
    }
}

// Program.cs
app.UseMiddleware<ErrorHandlingMiddleware>(); // place early in the pipeline
```

---

## 🧭 Middleware vs. Filters vs. Delegating Handlers

- **Middleware**: Request/response pipeline level; affects **all** or many endpoints (e.g., auth, logging).
- **Filters** (MVC): Per‑controller/action concerns (model validation, caching attributes).
- **Endpoint filters** (Minimal APIs, .NET 7+): Per‑endpoint behavior in Minimal APIs.
- **HttpClient Delegating Handlers**: Outgoing HTTP pipeline for **HttpClient** (retry, headers).

Choose **middleware** when the concern is truly cross‑cutting across the app.

---

## ✅ Checklist (Best Practices)

- Place **exception handling** & **HTTPS redirection** **early**.
- `UseRouting()` **before** auth; map endpoints **last**.
- Keep middleware **stateless** (avoid shared mutable state).
- Prefer **short‑circuiting** judiciously (use `Run(...)` to terminate when appropriate).
- Use **`Map`/`UseWhen`** for path‑ or condition‑specific behavior.
- Test with **WebApplicationFactory** and integration tests to verify pipeline behavior.

---

## 📎 Minimal API Example (with per‑endpoint middleware)

```csharp
var app = WebApplication.CreateBuilder(args).Build();

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Add("X-App", "Sample");
    await next();
});

app.MapGet("/hello", (HttpContext ctx, ILogger<Program> log) =>
{
    log.LogInformation("Hello was requested");
    return Results.Ok(new { message = "Hello" });
})
.AddEndpointFilter(async (efiContext, next) =>  // endpoint filter (.NET 7+)
{
    var result = await next(efiContext);
    return result;
});

app.Run();
```

---

## 🧪 Testing Middleware (Integration sample)

```csharp
public class PipelineTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public PipelineTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Adds_Correlation_Header()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/");
        Assert.True(res.Headers.Contains("X-Correlation-Id"));
    }
}
```

---

## TL;DR
- **Middleware** are building blocks of the ASP.NET Core **HTTP pipeline**.
- Use them for **cross‑cutting** concerns (logging, errors, auth, CORS, headers).
- Create custom middleware via **conventional** or **IMiddleware** patterns.
- **Order matters** — place and branch carefully using `Use`, `Map`, `UseWhen`.

---
