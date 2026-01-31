# ASP.NET Core Middleware Flow (`Use` vs `Map` vs `Run`)

This note explains how the middleware pipeline works and what happens when you combine `Use`, `Map`, and `Run`.

---

## 1. `app.Use(...)`
- Middleware with an optional `next()` delegate.
- Can run **before** and **after** the next component in the pipeline.

```csharp
app.Use(async (context, next) =>
{
    Console.WriteLine("Use before");
    await next();
    Console.WriteLine("Use after");
});
```

✅ Use `Use` for cross‑cutting concerns: logging, auth, CORS, etc.

---

## 2. `app.Run(...)`
- **Terminal middleware** — ends the pipeline.
- No `next()`; once executed, nothing else runs after it.

```csharp
app.Run(async context =>
{
    await context.Response.WriteAsync("Terminal Run");
});
```

✅ Use `Run` when you want to short‑circuit and send the response directly.

---

## 3. `app.Map(...)`
- **Branches** the pipeline based on path.
- Requests that match the path execute only the branch pipeline.

```csharp
app.Map("/test", builder =>
{
    builder.Run(async context =>
    {
        await context.Response.WriteAsync("Inside /test branch");
    });
});
```

✅ Use `Map` for modular routes like `/api`, `/admin`, etc.

---

## 4. Putting them together

```csharp
app.Use(async (context, next) =>
{
    Console.WriteLine("Use before");
    await next();
    Console.WriteLine("Use after");
});

app.Map("/test", builder =>
{
    builder.Run(async context =>
    {
        await context.Response.WriteAsync("Inside /test branch");
    });
});

app.Run(async context =>
{
    await context.Response.WriteAsync("Terminal Run in main pipeline");
});
```

### Request to `/test`
1. `Use before` runs.
2. Path matches `/test` → branch executes.
3. `Inside /test branch` response is returned.
4. **Main `Run` is NOT called**, because the branch had its own `Run`.

### Request to `/anything-else`
1. `Use before` runs.
2. Path does not match `/test` → continues in main pipeline.
3. Main `Run` executes → `Terminal Run in main pipeline`.
4. `Use after` does **not** run, because the pipeline ended.

---

## 5. Flow Diagram

```text
Request
  │
  ▼
[Use] ──► [Map "/test"] ──(if match)──► [Branch Run] ──► END
  │                                   
  └──────────► [Main Run] ───────────► END
```

---

### Key Takeaways
- `Use`: Middleware (before + after, optional continuation).
- `Run`: Terminal (no continuation).
- `Map`: Branching (path‑based sub‑pipelines).

➡️ If a request goes into a `Map` branch with its own `Run`, the **main `Run` will not execute**.
