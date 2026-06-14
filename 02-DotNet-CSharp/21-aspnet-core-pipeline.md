# ASP.NET Core Request Pipeline

> A consolidated reference covering middleware (built-in and custom), the `Use` / `Run` / `Map` / `MapWhen` / `UseWhen` pipeline builders, service registration and lifetimes (Transient / Scoped / Singleton), `ControllerBase` vs `Controller`, and `HttpContext` vs `HttpRequest` vs `HttpResponse`.

---

## Middleware — What, Why, How

Middleware are **components that inspect, modify, or short-circuit HTTP requests and responses** as they flow through the **pipeline** in ASP.NET Core. Each middleware decides whether to:
1. **handle** the request and end the pipeline, or
2. **call the next middleware** and optionally act on the response on the way back.

### The Request Pipeline (Big Picture)

```text
[Incoming HTTP]
   |
   v
UseRouting -> UseCors -> UseAuthentication -> UseAuthorization -> Custom Middleware -> Endpoint
   ^
   |
[Outgoing HTTP Response]
```

- The pipeline is **ordered** — order matters a lot.
- Each middleware can do **before** (pre-processing) and **after** (post-processing) logic.

### Where to Use Middleware

Typical cross-cutting concerns that belong in middleware:
- **Logging & Correlation IDs**
- **Global exception handling** / custom error pages
- **Authentication** (who are you?) and **Authorization** (what can you do?)
- **CORS**, **Response compression**, **Caching headers**
- **Request/Response transformation** (e.g., add headers)
- **Localization**, **Rate limiting**, **Security headers**

> If it must happen for **many or all endpoints** and is **request/response oriented**, middleware is usually the right place.

### Using Built-in Middleware (Program.cs)

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

### Three Ways to Add Middleware

**1) `Use(...)` for inline delegates**
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

**2) `Map(...)` / `MapWhen(...)` to branch the pipeline**
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

**3) `UseWhen(...)` for conditional middleware**
```csharp
app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/admin"), adminApp =>
{
    adminApp.UseMiddleware<AdminAuditMiddleware>();
});
```

### Creating Custom Middleware (Conventional Pattern)

**a) Middleware class**
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

**b) Registration**
```csharp
app.UseMiddleware<RequestTimingMiddleware>(); // Program.cs
```

**c) Optional extension method (syntactic sugar)**
```csharp
public static class RequestTimingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder app)
        => app.UseMiddleware<RequestTimingMiddleware>();
}

// Usage
app.UseRequestTiming();
```

### Creating Custom Middleware (IMiddleware Pattern)

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

**When to prefer IMiddleware?** When you want **DI-controlled lifetime** or constructor injections without using the `RequestDelegate` constructor style.

### Global Exception Handling Middleware (Practical)

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

### Middleware vs. Filters vs. Delegating Handlers
- **Middleware**: Request/response pipeline level; affects **all** or many endpoints (e.g., auth, logging).
- **Filters** (MVC): Per-controller/action concerns (model validation, caching attributes).
- **Endpoint filters** (Minimal APIs, .NET 7+): Per-endpoint behavior in Minimal APIs.
- **HttpClient Delegating Handlers**: Outgoing HTTP pipeline for **HttpClient** (retry, headers).

Choose **middleware** when the concern is truly cross-cutting across the app.

### Best Practices Checklist
- Place **exception handling** & **HTTPS redirection** **early**.
- `UseRouting()` **before** auth; map endpoints **last**.
- Keep middleware **stateless** (avoid shared mutable state).
- Prefer **short-circuiting** judiciously (use `Run(...)` to terminate when appropriate).
- Use **`Map`/`UseWhen`** for path- or condition-specific behavior.
- Test with **WebApplicationFactory** and integration tests to verify pipeline behavior.

### Minimal API Example (with per-endpoint middleware)
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

### Testing Middleware (Integration sample)
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

> **TL;DR:** Middleware are building blocks of the ASP.NET Core HTTP pipeline. Use them for cross-cutting concerns (logging, errors, auth, CORS, headers). Create custom middleware via conventional or IMiddleware patterns. Order matters — place and branch carefully using `Use`, `Map`, `UseWhen`.

---

## Pipeline Builders: Use vs Run vs Map vs MapWhen vs UseWhen

These are the core APIs for composing the **HTTP middleware pipeline** in ASP.NET Core. They control **order**, **branching**, and **termination** of requests.

### TL;DR Comparison

| Method        | Continues to next? | Branches? | Condition Type        | Typical Use |
|---------------|--------------------|-----------|-----------------------|-------------|
| `Use`         | Yes (via `await next()`) | No   | —                     | Cross-cutting (logging, headers, timing) |
| `Run`         | No (terminal)      | No        | —                     | Short-circuit with a final response |
| `Map`         | Yes (inside branch) | Yes      | Path **prefix**       | Sub-pipelines like `/health`, `/dashboard` |
| `MapWhen`     | Yes (inside branch) | Yes      | Arbitrary **predicate** (query/headers/etc.) | Conditional routing by logic |
| `UseWhen`     | Yes (returns to main) | Yes    | Arbitrary **predicate** | Conditional logic that should **continue** in main pipeline |

### 1) `Use` — wrap logic before & after next middleware
```csharp
app.Use(async (context, next) =>
{
    Console.WriteLine("Before next");
    await next();
    Console.WriteLine("After next");
});
```
Great for **logging**, **correlation IDs**, **timing**, **headers**.

### 2) `Run` — terminal, ends the pipeline
```csharp
app.Run(async context =>
{
    await context.Response.WriteAsync("Hello from Run");
});
```
Place at the **end** or inside a branch to **short-circuit**. No `next()`; once executed, nothing else runs after it.

### 3) `Map` — branch by path prefix
```csharp
app.Map("/health", branch =>
{
    branch.Run(async ctx => await ctx.Response.WriteAsync("OK"));
});
```
Requests like `/health` (and deeper, e.g., `/health/live`) go to this branch. Use `Map` for modular routes like `/api`, `/admin`, etc.

### 4) `MapWhen` — branch by predicate
```csharp
app.MapWhen(ctx => ctx.Request.Query.ContainsKey("debug"), branch =>
{
    branch.Run(async ctx => await ctx.Response.WriteAsync("Debug mode"));
});
```
Condition can check **query**, **headers**, **claims**, etc.

### 5) `UseWhen` — conditional sub-pipeline that returns to main
```csharp
app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/admin"), adminApp =>
{
    adminApp.Use(async (ctx, next) =>
    {
        Console.WriteLine("Admin logic");
        await next();
    });
});
```
Runs only when the condition is true, but **continues in the main pipeline** afterward.

### Combined Flow Example
```csharp
app.Use(async (ctx, next) =>
{
    Console.WriteLine("Global Use: before");
    await next();
    Console.WriteLine("Global Use: after");
});

app.Map("/health", h => h.Run(async ctx => await ctx.Response.WriteAsync("OK")));
app.MapWhen(ctx => ctx.Request.Query.ContainsKey("debug"),
    dbg => dbg.Run(async ctx => await ctx.Response.WriteAsync("Debug Info")));

app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/admin"), admin =>
{
    admin.Use(async (ctx, next) => { Console.WriteLine("Admin section"); await next(); });
});

app.Run(async ctx => await ctx.Response.WriteAsync("Default endpoint"));
```

**Behavior**
- `/health` -> returns `"OK"` (branch terminal).
- `?debug=true` -> returns `"Debug Info"` (branch terminal).
- `/admin/*` -> prints "Admin section" then continues to the final `Run`.
- Everything else -> falls through to the default `Run`.

### Flow Diagram

```text
Incoming Request
  -> Use (global pre/post)
       -> Path starts with /health?
            yes -> Run: OK (terminal)
            no  -> Query has ?debug?
                     yes -> Run: Debug Info (terminal)
                     no  -> Path starts with /admin?
                              yes -> UseWhen admin logic -> Run Default (terminal)
                              no  -> Run Default (terminal)
```

### Use / Run / Map Interaction (path-branch detail)

```text
Request
  |
  v
[Use] --> [Map "/test"] --(if match)--> [Branch Run] --> END
  |
  +----------> [Main Run] -----------> END
```

- **Request to `/test`**: `Use before` runs; path matches `/test` -> branch executes; branch response returned. The **main `Run` is NOT called** because the branch had its own `Run`.
- **Request to `/anything-else`**: `Use before` runs; path does not match -> continues in main pipeline; main `Run` executes. `Use after` does **not** run, because the pipeline ended at the terminal `Run`.

> If a request goes into a `Map` branch with its own `Run`, the **main `Run` will not execute**.

### Related / Similar Methods You'll See
- **`UseRouting()` / `UseEndpoints()`** (pre-.NET 6): set up endpoint routing; now generally replaced by minimal-API mapping methods.
- **`MapGet` / `MapPost` / `MapPut` / `MapDelete`**: **terminal** endpoint mappings in Minimal APIs. They behave like a specialized `Map + Run`.
  ```csharp
  app.MapGet("/hello", () => "hello"); // terminal for /hello
  ```
- **`MapGroup("/api")`** (.NET 7+): group endpoints and apply filters/middleware to the **group**.
  ```csharp
  var api = app.MapGroup("/api").AddEndpointFilter(new AuthFilter());
  api.MapGet("/orders", GetOrders);
  ```
- **`MapFallback`**: terminal for unmatched routes (e.g., SPA fallback to `index.html`).
  ```csharp
  app.MapFallbackToFile("index.html");
  ```
- **`UseStaticFiles()`**: middleware for serving `wwwroot` content early in the pipeline.
- **`UseCors()` / `UseAuthentication()` / `UseAuthorization()`**: built-in middleware that must be ordered correctly with routing.

> Think of `MapGet`/friends as **endpoints**; `Use/Run/Map/MapWhen/UseWhen` are **pipeline builders**.

### Best Practices
- Put **exception handling** and **HTTPS redirection** early (before most other middleware).
- `UseRouting()` before `UseAuthentication()`/`UseAuthorization()`; map endpoints last.
- Use **`Map`** for path-based sub-pipelines; **`MapWhen`** for arbitrary conditions.
- Prefer **`UseWhen`** if you need to **continue** in the main pipeline after conditional logic.
- Keep middleware **stateless**; avoid storing per-request data in singletons.

### Quick Reference Cheat Sheet
```csharp
// Cross-cutting
app.Use(/* pre/post */);

// Terminal
app.Run(/* produce response */);

// Path branch
app.Map("/metrics", m => m.Run(/* terminal */));

// Predicate branch (is mobile user? has header?)
app.MapWhen(ctx => ctx.Request.Headers.ContainsKey("X-Debug"),
            b => b.Run(/* terminal */));

// Conditional sub-pipeline that re-joins main
app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/admin"),
            b => b.Use(/* more middleware */));
```

---

## Services in .NET Core

In **ASP.NET Core**, a **service** is any class that is registered in the **Dependency Injection (DI) container** so it can be injected and reused.

### Examples of Services
- **Built-in framework services**: `ILogger<T>` (logging), `IConfiguration` (configuration settings), `DbContext` (database access via EF Core).
- **Custom services**: `IOrderService` (business logic), `IEmailSender` (email sending), `IMessageFormatter` (string utilities).

### Registering Services (Program.cs)
```csharp
var builder = WebApplication.CreateBuilder(args);

// Framework services
builder.Services.AddLogging();
builder.Services.AddDbContext<AppDbContext>();

// Your custom services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<ISystemClock, SystemClock>();

var app = builder.Build();
app.MapControllers();
app.Run();
```

### Fetching Services from Other Classes

**1. Constructor Injection (Best Practice)**
```csharp
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    // Services are injected by the framework
    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost("orders")]
    public async Task<IActionResult> Create(Order order)
    {
        var id = await _orderService.PlaceOrderAsync(order);
        _logger.LogInformation("Order {id} created", id);
        return Ok(id);
    }
}
```
ASP.NET Core automatically resolves `IOrderService` and `ILogger<T>` from DI.

**2. Property Injection (Less common)**
```csharp
public class ReportGenerator
{
    public ILogger<ReportGenerator>? Logger { get; set; }

    public void Generate()
    {
        Logger?.LogInformation("Report generated.");
    }
}
```

**3. Manual Resolution (Avoid unless necessary)**
```csharp
public class LegacyHelper
{
    public static void DoWork(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<LegacyHelper>>();
        logger.LogInformation("Doing legacy work...");
    }
}
```
Use only if constructor injection is not possible. Avoid the **Service Locator anti-pattern**.

### Flow: Service Registration -> Injection -> Usage
```text
Program.cs (Service Registration)
        |
        v
DI Container ----------------------------+
        |                                |
        v                                v
Controller/Service Constructor   Framework (ILogger, DbContext)
        |
        v
Injected Service Instance is used in Business Logic
```

### Best Practice Summary
- Always prefer **Constructor Injection**.
- Register dependencies in **Program.cs**.
- Use `IServiceProvider` directly only at the **composition root**, not in business logic.

---

## Service Lifetimes (Scoped vs Transient vs Singleton)

ASP.NET Core Dependency Injection supports **3 main lifetimes**: **Transient, Scoped, Singleton**. Understanding them is crucial for HTTP request handling, performance, and correctness.

### 1. Transient
- **Definition**: A new instance is created **every time** it is requested.
- **Lifecycle**: Each resolve -> new object. Even within one request, multiple resolves = multiple instances.
- **Best for**: Lightweight, **stateless** operations.
```csharp
services.AddTransient<IIdGenerator, GuidIdGenerator>();
```

### 2. Scoped
- **Definition**: One instance **per HTTP request** (or logical scope).
- **Lifecycle**: Same instance reused across all services in one request. Disposed when the request ends.
- **Best for**: Request-context services like **EF Core DbContext**.
```csharp
services.AddScoped<IOrderService, OrderService>();
services.AddScoped<AppDbContext>(); // EF Core context
```

### 3. Singleton
- **Definition**: One instance for the **entire application lifetime**.
- **Lifecycle**: Created once, reused across all requests.
- **Best for**: **Caches, configuration readers, logging, HttpClientFactory**.
```csharp
services.AddSingleton<ISystemClock, SystemClock>();
```

### Lifecycle Comparison (HTTP Request Context)

| Lifetime    | Instance per Request | Instance per Resolve | Instance per App |
|-------------|----------------------|----------------------|------------------|
| **Transient** | No                 | Yes (new each time)  | No               |
| **Scoped**    | Yes (one per request)| Same in request      | No               |
| **Singleton** | Yes (same for all) | Same for all resolves| Yes              |

### Visual Lifecycle in HTTP Requests
```text
HTTP Request 1
   |- Scoped Service A (same across this request)
   |- Transient Service B (new each time resolved)
   +- Singleton Service C (shared app-wide)

HTTP Request 2
   |- Scoped Service A' (different from Request 1)
   |- Transient Service B' (new again)
   +- Singleton Service C (same as Request 1)
```

### Common Pitfalls
- Injecting **Scoped into Singleton** -> runtime errors ("captured scoped instance").
- Singleton with mutable state -> thread safety issues.
- Correct usage:
  - `DbContext` -> Scoped
  - Business services -> Scoped
  - Utilities/config -> Singleton
  - Lightweight helpers -> Transient

### Summary
- **Transient** -> new every resolve. Best for **stateless helpers**.
- **Scoped** -> one per HTTP request. Best for **DbContext, request-based services**.
- **Singleton** -> one per application lifetime. Best for **logging, caching, config**.

### Stateless Helpers (Transient Examples)

**Stateless helpers** are lightweight, utility-style services that do **not maintain state** across calls or requests. They are best registered as **Transient** services.

**1. Password Hasher**
```csharp
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
        => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));

    public bool Verify(string password, string hash)
        => Hash(password) == hash;
}

// Registration
services.AddTransient<IPasswordHasher, PasswordHasher>();
```
Stateless: each call is independent.

**2. GUID Generator**
```csharp
public interface IIdGenerator
{
    Guid NewId();
}

public class GuidIdGenerator : IIdGenerator
{
    public Guid NewId() => Guid.NewGuid();
}

// Registration
services.AddTransient<IIdGenerator, GuidIdGenerator>();
```
Stateless: every call produces a new `Guid`.

**3. String Formatter**
```csharp
public interface IMessageFormatter
{
    string Format(string message);
}

public class MessageFormatter : IMessageFormatter
{
    public string Format(string message)
        => $"[{DateTime.UtcNow}] {message}";
}

// Registration
services.AddTransient<IMessageFormatter, MessageFormatter>();
```
Stateless: formatting has no memory or side effects.

**4. Math/Calculation Helper**
```csharp
public interface IDiscountCalculator
{
    decimal ApplyDiscount(decimal amount, decimal percentage);
}

public class DiscountCalculator : IDiscountCalculator
{
    public decimal ApplyDiscount(decimal amount, decimal percentage)
        => amount - (amount * (percentage / 100));
}

// Registration
services.AddTransient<IDiscountCalculator, DiscountCalculator>();
```
Stateless: always computes based on input only.

**Key Takeaway**: Stateless helpers = pure, utility-style services. They don't cache, store, or depend on request state. Best suited for **Transient lifetime** in ASP.NET Core DI.

---

## ControllerBase vs Controller (with IActionResult patterns)

### TL;DR
- **`ControllerBase`** -> lean base class for **Web APIs** (no view engine).
- **`Controller`** -> adds **view support** (`View()`, `PartialView()`) on top of `ControllerBase` for MVC apps.

Use **`ControllerBase`** for pure REST APIs. Use **`Controller`** when you also render Razor views.

### Inheritance Hierarchy
```text
object
  -> ControllerContext
       -> ControllerBase      (API helpers: Ok, BadRequest, NotFound, Created, etc.)
            -> Controller     (adds View(), PartialView(), ViewComponent(), RedirectToAction(), ...)
```

### Typical Usage

**Web API (recommended): `ControllerBase` + `[ApiController]`**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        if (id <= 0) return BadRequest("Invalid id");
        var product = new { Id = id, Name = "Laptop" };
        return Ok(product);
    }
}
```

**MVC + API mixed: `Controller`**
```csharp
public class HomeController : Controller
{
    public IActionResult Index() => View(); // Razor view

    [HttpGet("api/ping")]
    public IActionResult Ping() => Ok(new { message = "pong" });
}
```

### Returning Results: `IActionResult` vs `ActionResult<T>`

**1) `IActionResult`** — most flexible: you return **any** action result. Good when different branches return **different result types**.
```csharp
[HttpGet("{id:int}")]
public IActionResult Get(int id)
{
    var entity = _repo.Find(id);
    if (entity is null) return NotFound();
    return Ok(entity); // 200 + JSON body
}
```

**2) `ActionResult<T>`** (strongly-typed convenience) — combines a **typed body** with action results. Enables better **OpenAPI/Swagger** metadata and model binding.
```csharp
[HttpGet("{id:int}")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public ActionResult<ProductDto> Get(int id)
{
    var dto = _repo.FindDto(id);
    if (dto is null) return NotFound();
    return dto; // implicit 200 OK with body
}
```

> Rule of thumb: Prefer **`ActionResult<T>`** for GETs that normally return a `T`, and **`IActionResult`** when you frequently return varied results.

### Common Response Helpers (both `ControllerBase` & `Controller`)

| Helper/Result                    | Purpose / HTTP Status                  | Example                                                      |
|----------------------------------|----------------------------------------|--------------------------------------------------------------|
| `Ok(object value)`               | 200 OK                                  | `return Ok(dto);`                                            |
| `Created(uri, value)`            | 201 Created + `Location` header         | `return Created($"/api/p/{id}", dto);`                       |
| `CreatedAtAction(...)`           | 201 Created pointing to action          | `return CreatedAtAction(nameof(GetById), new { id }, dto);`  |
| `NoContent()`                    | 204 No Content                          | `return NoContent();`                                        |
| `BadRequest(object error)`       | 400 Bad Request                         | `return BadRequest(ModelState);`                             |
| `Unauthorized()`                 | 401 Unauthorized                        | `return Unauthorized();`                                     |
| `Forbid()`                       | 403 Forbidden (authorization failed)    | `return Forbid();`                                           |
| `NotFound(object value?)`        | 404 Not Found                           | `return NotFound(id);`                                       |
| `Conflict(object value?)`        | 409 Conflict                            | `return Conflict("Version mismatch");`                       |
| `StatusCode(int code, object?)`  | custom code                             | `return StatusCode(418, "I'm a teapot");`                    |
| `File(...)`                      | Return files/streams                    | `return File(bytes, "application/pdf", "file.pdf");`         |
| `PhysicalFile(...)`              | Return file from disk                   | `return PhysicalFile(path, "image/png");`                    |
| `VirtualFile(...)`               | Return file from web root               | `return VirtualFile("~/docs/readme.txt");`                   |
| `Redirect(...)`                  | 302/307/308 Redirect                    | `return Redirect("/login");`                                 |
| `Problem(...)`                   | RFC 7807 problem details                | `return Problem("Something broke");`                         |
| `ValidationProblem(...)`         | 400 with validation payload             | `return ValidationProblem(ModelState);`                      |

> MVC-only (available on `Controller`): `View()`, `PartialView()`, `ViewComponent()`, `RedirectToAction()`.

### Realistic REST Endpoints: Examples

**POST create (201 + Location)**
```csharp
[HttpPost]
public ActionResult<ProductDto> Create(CreateProductDto input)
{
    var created = _service.Create(input); // returns ProductDto with Id
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
}
```

**PUT update (204)**
```csharp
[HttpPut("{id:int}")]
public IActionResult Update(int id, UpdateProductDto input)
{
    if (!_service.Exists(id)) return NotFound();
    _service.Update(id, input);
    return NoContent();
}
```

**DELETE (404 or 204)**
```csharp
[HttpDelete("{id:int}")]
public IActionResult Delete(int id)
{
    var removed = _service.Delete(id);
    return removed ? NoContent() : NotFound();
}
```

### Model Validation with `[ApiController]`
- Automatic 400 responses when `ModelState` is invalid (unless suppressed).
- Access validation details via `ModelState` and `ProblemDetails`.
```csharp
public record CreateProductDto([Required] string Name, [Range(0, 9999)] decimal Price);

[HttpPost]
public IActionResult Create(CreateProductDto dto)
{
    if (!ModelState.IsValid) return ValidationProblem(ModelState);
    // ...
    return Ok();
}
```

### When to Choose Which Base Class?

| Scenario                                 | Choose |
|------------------------------------------|--------|
| Pure JSON/REST API                       | **ControllerBase** |
| API + Razor views in same project        | **Controller**     |
| Minimal overhead / no `View()` needed    | **ControllerBase** |
| Need `View()` helpers (MVC)              | **Controller**     |

### Bonus: Minimal APIs vs Controllers
- Minimal APIs use endpoint mapping (e.g., `app.MapGet`) and return any serializable type or `IResult`.
- Controllers (ControllerBase/Controller) are better for **complex APIs** needing filters, model binding/validation attributes, or versioning.
```csharp
app.MapGet("/api/products/{id:int}", (int id, IProductService svc) =>
{
    var dto = svc.Find(id);
    return dto is null ? Results.NotFound() : Results.Ok(dto);
});
```

### Quick Checklist
- Pure API? -> `ControllerBase`.
- Need views? -> `Controller`.
- Prefer `ActionResult<T>` for typed success results; use `IActionResult` when branching widely.
- Leverage built-in helpers (`Ok`, `CreatedAtAction`, `NoContent`, `Problem`, `File`, etc.).

---

## HttpContext vs HttpRequest vs HttpResponse

**Short version:**
- **`HttpContext`** = the *entire* HTTP transaction (request + response + user + items + session + connection + features).
- **`HttpRequest`** = details of the *incoming request* (method, path, headers, query, cookies, body).
- **`HttpResponse`** = represents the *outgoing response* (status code, headers, body, cookies).

### Object Model (Hierarchy)
```text
HttpContext
├── Request : HttpRequest   (incoming)
├── Response : HttpResponse (outgoing)
├── User : ClaimsPrincipal
├── Items : IDictionary<object, object?>
├── Session : ISession (if enabled)
├── Connection : ConnectionInfo
└── Features : IFeatureCollection (WebSockets, Http/2, etc.)
```

`HttpRequest` key members: `Method`, `Path` (`PathString`), `Query` (`IQueryCollection`), `Headers` (`IHeaderDictionary`), `Body` (`Stream`), `Cookies` (`IRequestCookieCollection`), `Host` (`HostString`), `Scheme`, `Protocol`.

`HttpResponse` key members: `StatusCode` (`int`), `Headers` (`IHeaderDictionary`), `Body` (`Stream`), `Cookies` (`IResponseCookies`), `ContentType`, `Redirect()`, `WriteAsync(string)`.

### Role of Each

| Object | Direction | Purpose |
|--------|-----------|---------|
| **HttpContext** | Both | Container for the entire transaction (request + response + user + items + session) |
| **HttpRequest** | Incoming | Data about the request (method, path, query, headers, body, cookies) |
| **HttpResponse** | Outgoing | Data you send back (status code, headers, body, cookies) |

### Middleware Examples

**1) Read request (HttpRequest) and set response (HttpResponse)**
```csharp
public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    public AuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        HttpRequest req = context.Request;
        HttpResponse res = context.Response;

        if (!req.Headers.ContainsKey("Authorization"))
        {
            res.StatusCode = StatusCodes.Status401Unauthorized;
            await res.WriteAsync("Unauthorized");
            return; // short-circuit pipeline
        }

        await _next(context);
    }
}
```

**2) Add correlation ID (HttpContext.Items + HttpResponse.Headers)**
```csharp
public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;
    public const string Key = "CorrelationId";

    public CorrelationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx)
    {
        var id = Guid.NewGuid().ToString("n");
        ctx.Items[Key] = id;                        // store per-request
        ctx.Response.Headers["X-Correlation-Id"] = id; // include in response
        await _next(ctx);
    }
}
```

### Controller Examples

**Inspect only the request**
```csharp
[HttpGet]
public IActionResult List()
{
    var method = Request.Method;       // GET
    var path = Request.Path;           // /api/orders
    var query = Request.Query["page"]; // ?page=2
    var ua = Request.Headers.UserAgent;
    return Ok(new { method, path, query, ua });
}
```

**Set response data**
```csharp
[HttpGet("{id}")]
public IActionResult Get(int id)
{
    if (id < 0)
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        return BadRequest("Invalid ID");
    }

    Response.Headers["X-Sample"] = "true";
    Response.Cookies.Append("SeenBefore", "yes");
    return Ok(new { id, timestamp = DateTime.UtcNow });
}
```

### Testing Considerations
- **HttpContext** -> create with `new DefaultHttpContext()` for unit tests.
- **HttpRequest** -> set `Method`, `Path`, `Headers`, `Body` as needed.
- **HttpResponse** -> check `StatusCode`, `Headers`, `Body` after middleware/controller runs.
```csharp
[Fact]
public async Task Middleware_Sets_401_Without_Authorization()
{
    var ctx = new DefaultHttpContext();
    var mw = new AuthMiddleware(_ => Task.CompletedTask);

    await mw.InvokeAsync(ctx);

    Assert.Equal(401, ctx.Response.StatusCode);
    Assert.Equal("Unauthorized", await new StreamReader(ctx.Response.Body).ReadToEndAsync());
}
```

### Quick Summary
- **HttpContext** = whole transaction container.
- **HttpRequest** = everything about the *incoming request*.
- **HttpResponse** = everything about the *outgoing response*.

**Rule of thumb:**
- Inspect **HttpRequest** for inbound details.
- Set **HttpResponse** to send results back.
- Use **HttpContext** when you need access to both, plus per-request storage and metadata.
