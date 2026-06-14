# .NET Runtime & Internals

> A consolidated reference covering .NET Core architecture, the major runtime components (CLR, CTS, CLS), JIT compilation and its modes (RyuJIT / ReadyToRun / NativeAOT), the Kestrel web server, `appsettings.json` configuration, and the `wwwroot` static-file root.

---

## .NET Core Architecture

.NET Core is a **cross-platform, open-source framework** for building modern applications (cloud, web, desktop, mobile, IoT). It is modular, lightweight, and designed for **performance and scalability**.

### Architecture Flow

```text
Application Code (C#, F#, VB.NET)
        |
        v
Roslyn Compiler (C#/VB)
        |
        v
IL Code + Metadata (Assemblies: DLL/EXE)
        |
        v
CoreCLR (Runtime Engine)
   |          |              |
   v          v              v
JIT Compiler  Garbage        CoreFX (BCL:
+ Execution   Collector /    Base Class
Engine        Memory Mgmt    Libraries)
   |          |              |
   +----------+--------------+
        |
        v
Operating System (Windows / Linux / macOS)
```

### Components

1. **Application Code** — Developers write code in **C#, F#, or VB.NET**.
2. **Roslyn Compiler** — Converts source code into **Intermediate Language (IL)** and **metadata**; provides rich tooling support (IntelliSense, code analysis).
3. **IL Code + Metadata** — Output stored in **assemblies (.dll / .exe)**; a platform-agnostic representation.
4. **CoreCLR (Runtime Engine)** — The heart of .NET Core. Loads assemblies, manages execution, provides **type safety**.
5. **JIT Compiler** — Converts IL into **machine code** for the target OS/CPU; ensures cross-platform support.
6. **CoreFX (BCL)** — Base Class Library for common APIs (collections, IO, networking, JSON, XML).
7. **Garbage Collector (GC)** — Handles **automatic memory management**; frees unused objects, optimizes memory layout, prevents memory leaks.
8. **Operating System Layer** — Abstracted layer so applications run on **Windows, Linux, or macOS** seamlessly.

> Memory management itself (GC generations, finalization, `IDisposable`) is covered in the dedicated garbage-collection material; this file focuses on the runtime architecture and execution pipeline.

### Summary
- **.NET Core** is modular, high-performance, and cross-platform.
- CoreCLR + CoreFX form the runtime and libraries.
- **Roslyn** compiles source code -> IL -> executed by CoreCLR via **JIT**.
- The **Garbage Collector** ensures efficient memory usage with minimal developer intervention.

---

## Major Components: CLR, CTS, CLS

### Common Language Runtime (CLR)

The Common Language Runtime (CLR) is .NET's execution engine — the part that loads your code, compiles it to machine instructions, runs it, and manages core services like memory, threading, exceptions, and interop. You write in C#/F#/VB; the compiler emits IL (CIL/MSIL) plus metadata inside an assembly, and the CLR turns that into native code and executes it.

**Key Responsibilities**
- **Loading & verification**: Loads assemblies, reads metadata, enforces type safety, and verifies IL before running it.
- **Just-in-time (JIT) compilation**: Uses **RyuJIT** to compile IL to native code at runtime. Modern .NET uses **tiered compilation** (fast code first, then re-optimizes hot paths). Supports ahead-of-time options like **ReadyToRun** and **NativeAOT** to reduce startup/JIT cost.
- **Memory management (Garbage Collection)**: Automatic GC with **generations 0/1/2** and a **Large Object Heap (LOH)**. Workstation vs. Server GC modes; background (concurrent) collection; finalization queue. `IDisposable`/`using` is for deterministic release of unmanaged resources — separate from GC.
- **Exception handling**: Structured exceptions across languages with stack unwinding and filters.
- **Type system & interop**: **CTS (Common Type System)** defines how types behave; **CLS (Common Language Specification)** is the subset for cross-language use. Interop via **P/Invoke** (native functions) and **COM interop**.
- **Threading & async**: Managed threads, thread pool, synchronization primitives; `async`/`await` builds atop the runtime's scheduling.
- **Security & diagnostics**: Code verification and permissions (legacy Code Access Security in .NET Framework; not present in modern .NET). Profiling/ETW/EventPipe, reflection, dynamic code generation (Reflection.Emit), assembly loading isolation.

**Execution Flow (mental model)**
1. **Compile**: C# -> IL + metadata -> `.dll`/`.exe` assembly.
2. **Load**: CLR loads assembly, verifies IL, resolves references.
3. **JIT**: Methods compile on first use (or come precompiled via ReadyToRun/AOT).
4. **Run & manage**: GC allocates/collects, exceptions propagate, threads schedule, interop bridges native calls.

**CLR Across .NET Flavors**
- **.NET Framework (Windows-only)** used the original **CLR**.
- **.NET (5+) / .NET Core** uses **CoreCLR** (people still say "CLR" informally).
- **Mono** runtime powers some mobile/web/AOT-heavy scenarios.

**What the CLR is *not***
- Not the Base Class Library (BCL) itself.
- Not the C# compiler (Roslyn).
- Not just the GC — GC is one component of the CLR.

**When You'll Care Most**
- **Performance/startup**: JIT tiers, ReadyToRun, Server GC, span types.
- **Memory tuning**: LOH pressure, pinning, `ArrayPool<T>`, `Dispose` patterns.
- **Interop**: P/Invoke signatures, `SafeHandle`, marshaling costs.
- **Loading/isolation**: In modern .NET, **AssemblyLoadContext** replaces AppDomains for plugin-style isolation.

### Common Type System (CTS)

The **Common Type System (CTS)** is the runtime specification in .NET that defines **how types are declared, composed, and interact** so that code written in different languages can interoperate safely. CTS is the foundation that lets C#, F#, VB, and others share types and call each other's code.

**Why it exists**
- Guarantee **type safety** and verifiable execution.
- Enable **cross-language interop** and tooling (reflection, metadata).
- Provide a consistent **versioning and identity** model for types across assemblies.

**Big Picture**
- **Everything is a type** described by IL + metadata.
- All **reference** and **value** types ultimately derive from `System.Object` (value types derive via `System.ValueType`).
- Public surface area is governed by **accessibility** (public, private, protected, internal, etc.).
- Properties/events are **metadata + methods** (`get_*/set_*`, `add_*/remove_*`).

**Type Categories**
- *Reference types*: **class** (including `string`), **interface**, **delegate**, **array**. Allocated on the managed heap; referenced by object references. Support **single inheritance** for classes and **multiple interface** implementation.
- *Value types*: **struct** and **enum**. Stored inline (stack or inside objects/arrays). Copy by value. Implicitly **sealed**; cannot derive from other structs. May implement interfaces.
- *Pointer/byref types (unsafe / special)*: `void*`, `int*`, `ref T`, `out T`. Useful for interop and high-performance scenarios; not verifiable unless in `unsafe` code.

**Core Rules & Behaviors**
- **Single inheritance** for classes; **multiple interfaces** allowed.
- **Boxing/unboxing**: converting a value type to `object` or to an interface creates a **box**; unboxing requires the exact underlying type.
- **Arrays** are reference types implementing `System.Array`: **SZ arrays** (single-dimensional, zero-based, e.g., `T[]`); **multidimensional** (`T[,]`, `T[,,]`); **jagged** (arrays of arrays, e.g., `T[][]`).
- **Strings** are immutable reference types (`System.String`) with interning support.
- **Delegates** are object-oriented function pointers (multicast); events compile to `add`/`remove` methods and usually wrap a delegate field.
- **Exceptions**: all exceptions should derive from `System.Exception`.
- **Accessibility (IL terms)** roughly maps to: `private`, `assembly` (internal), `family` (protected), `famorassem` (protected internal), `famandassem` (private protected), `public`.

**Generics in the CTS**
- Generics are **reified at runtime**: the CLR knows about `List<int>` vs `List<string>`.
- *Constraints*: reference type (`class`) and non-nullable value type (`struct`); specific **base class** and **interface** constraints; **public parameterless ctor** constraint (`new()`).
- *Variance* for interfaces/delegates: **covariant (`out`)** — use a more derived type for outputs (e.g., `IEnumerable<out T>`); **contravariant (`in`)** — use a less derived type for inputs (e.g., `IComparer<in T>`).

**Numeric & Common Types (CTS vs CLS)**
The CTS defines the full set of built-in types; the **CLS** is a **cross-language subset**.
- CTS numeric types include `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `nint`, `nuint`, `float`, `double`, `decimal`, and `char` (UTF-16).
- For **CLS compliance** (so other languages can consume your API), avoid exposing non-CLS types like `sbyte`, `uint`, `ulong` in public APIs — or mark your assembly/class with `[assembly: CLSCompliant(true)]` and use `[CLSCompliant(false)]` on members as needed.

**Metadata, Identity, and Versioning**
- Each type has a **fully qualified name**: `Namespace.TypeName`, plus **assembly identity** (name, version, culture, public key token) for uniqueness.
- **Attributes** are stored in metadata and can decorate almost everything (assemblies, types, members, parameters, return values).
- **Reflection** reads this metadata at runtime; **emit** APIs can define new types dynamically.

**Value vs Reference: Practical Implications**
- Use **struct** for small, immutable, value-semantic data; prefer under ~16-32 bytes and avoid defensive copying issues.
- Beware **boxing** in hot paths (e.g., storing value types in `object`, non-generic collections, or interface calls).
- Arrays of value types are contiguous and cache-friendly; arrays of reference types are arrays of pointers.

**CTS & Interop**
- Marshaling between managed and native code is built on CTS types.
- Prefer `SafeHandle` over `IntPtr` for resource ownership in public APIs.
- `Span<T>`/`Memory<T>` are **ref-struct** patterns (stack-only) with special CTS rules (cannot be boxed, captured, or stored on the heap).

**CTS vs CLR vs CLS — Mental Model**
- **CLR**: the runtime that executes code and enforces the rules.
- **CTS**: the **type system** the CLR enforces.
- **CLS**: a **language-interop subset** of the CTS for library authors.

### Common Language Specification (CLS)
- The Common Language Specification (CLS) is a subset of the CTS and defines a set of rules to be followed by every .NET language.
- CLS supports interoperability or cross-language integration — it provides a common platform for interacting and sharing information. For example, every .NET language (C#, F#, VB.NET, etc.) has its own syntax. So when statements belonging to different languages get executed, a common platform is provided by the CLS to interact and share information.

---

## JIT (Just-In-Time) Compilation

### What is JIT?
The **Just-In-Time (JIT) compiler** is part of the **CoreCLR runtime**. It converts **Intermediate Language (IL)** into **native machine code** at runtime, allowing .NET apps to run on any supported OS/CPU.

### JIT Process Flow

```text
Source Code (C#, F#, VB.NET)
        |
        v
Roslyn Compiler
        |
        v
IL Code + Metadata (Assemblies)
        |
        v
JIT Compiler (RyuJIT)
        |
        v
Native Machine Code
        |
        v
Execution on CPU
```

### Key Features
- **On-demand compilation** — methods compiled only when called.
- **Cross-platform** — IL runs on Windows, Linux, macOS via JIT.
- **Optimized execution** — produces fast, CPU-specific machine code.

### Types of JIT in .NET Core
1. **RyuJIT (Default JIT)** — High-performance, 64-bit, cross-platform.
2. **Tiered Compilation** — Quick initial compilation, then re-optimizes hot methods.
3. **ReadyToRun (R2R)** — Partial AOT + JIT fallback.
4. **NativeAOT** — Full AOT, eliminates JIT.

### JIT Compilation Process
1. Source code -> compiled by **Roslyn** -> **IL**.
2. First method call -> **JIT compiles IL -> native code**.
3. Native code cached -> reused for subsequent calls.

### Optimizations by JIT
- **Inlining** — embeds small methods.
- **Dead Code Elimination** — removes unused paths.
- **Loop Unrolling** — speeds up loops.
- **Register Allocation** — efficient CPU usage.
- **Constant Folding** — precomputes constants.

### Pros & Cons

| Pros | Cons |
|------|------|
| Optimized for target CPU | Startup delay due to runtime compilation |
| Smaller binaries | Needs memory to store compiled code |
| Dynamic optimizations | Slightly slower cold start |

### Summary
- **JIT** = IL -> native code **at runtime**.
- Default engine: **RyuJIT** with **tiered compilation**.
- Balances portability + runtime optimization.
- For startup-critical apps, use **AOT (ReadyToRun/NativeAOT)**.

---

## JIT Modes: RyuJIT vs ReadyToRun vs NativeAOT — When to Use What

### 1. RyuJIT (Default JIT) — Long-running services

- **Best for**: APIs, background workers, batch jobs, real-time processing.
- **Why**: Startup cost is amortized; peak throughput matters most.

**Example**
- ASP.NET Core Web API serving millions of requests.
- Needs **dynamic libraries** (Entity Framework Core, reflection-heavy libs).
- Runs for hours/days inside a container or VM.

```text
dotnet publish -c Release
```
After a short warm-up, JIT + tiered compilation optimizes hot paths for **maximum throughput**.

### 2. ReadyToRun (R2R) — Middle ground

- **Best for**: Apps where **startup speed** matters but full .NET features are needed.
- **Why**: Precompiled IL reduces JIT work at runtime -> faster startup.

**Example**
- Desktop apps (WPF/WinForms) — quick launch improves UX.
- Microservices inside containers with frequent restarts.
- Command-line tools needing snappy response (<0.5s startup).

**.csproj setting:**
```text
<PropertyGroup>
  <PublishReadyToRun>true</PublishReadyToRun>
</PropertyGroup>
```

**Publish command:**
```text
dotnet publish -c Release -r win-x64 --self-contained
```
Faster launches at the cost of **larger binaries**.

### 3. NativeAOT — Full native executables

- **Best for**: Serverless functions, CLIs, lightweight daemons.
- **Why**: No JIT -> instant startup, small memory footprint.

**Example**
- Azure Function / AWS Lambda where cold-start time is critical.
- Cross-platform CLI tool (like `kubectl`).
- Sidecar/agent in a container where footprint must be minimal.

**.csproj setting:**
```text
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
</PropertyGroup>
```

**Publish command:**
```text
dotnet publish -c Release
```
Produces a **standalone executable** (`myapp` or `myapp.exe`). Limited reflection support (workarounds: **source generators**).

### Quick Comparison Table

| Mode        | When to Use | Example App | Command |
|-------------|-------------|-------------|---------|
| **RyuJIT**  | Long-running, throughput-sensitive | ASP.NET Core API, workers | `dotnet publish -c Release` |
| **R2R**     | Faster startup, still flexible | Desktop apps, microservices, CLIs | `dotnet publish -c Release -r linux-x64 /p:PublishReadyToRun=true` |
| **NativeAOT** | Instant startup, low memory | Serverless, sidecars, lightweight CLIs | `dotnet publish -c Release -r linux-x64 /p:PublishAot=true` |

### Rule of Thumb
- **Throughput matters most?** -> **RyuJIT (default)**.
- **Startup matters, but need full .NET features?** -> **R2R**.
- **Startup + memory critical, OK with reflection limits?** -> **NativeAOT**.

---

## Kestrel — Deployment & Threading

> A practical, production-focused reference for running ASP.NET Core with **Kestrel** as the web server — either directly on the edge or behind **NGINX/IIS** — plus the **thread pool & worker thread** behavior.

### TL;DR
- **Kestrel can serve the internet directly.** You add **NGINX/IIS** when you need multi-site routing on one IP, centralized TLS & cert automation, Windows auth, WAF/rate-limits, request buffering, or blue/green load balancing.
- Kestrel is **async and event-driven** — there is **no one-thread-per-request**. Work runs on the **.NET ThreadPool**; `await` frees the thread.
- Prefer **containerized** or **self-contained** deploys for predictable runtime; use **systemd** (Linux) or **Windows Service/IIS** for lifecycle management.

### What is Kestrel?
**Kestrel** is the built-in, cross-platform web server for ASP.NET Core. It handles HTTP/1.1, HTTP/2, and HTTP/3 (QUIC), supports TLS termination, and is included via `Microsoft.AspNetCore.Server.Kestrel`.

**Typical topologies**
- **Kestrel only (edge)** — simple, single site/service, you're fine managing certs in-app.
- **Reverse proxy in front**: **NGINX -> Kestrel** (Linux/containers); **IIS -> Kestrel** (Windows/enterprise; Windows/AD auth, app pool management). Alternative on Windows: **HTTP.sys** (kernel-mode server, IIS-like features without IIS).

### Do I Need NGINX or IIS?

```text
Start: ASP.NET Core app using Kestrel
  -> Run Kestrel on the public edge?
       No (internal only) -> Run Kestrel behind internal LB or service mesh
       Yes -> Multiple sites on same IP:443 or fancy routing?
                Yes -> Reverse Proxy (NGINX/IIS) + Central TLS, SNI routing, WAF/rate limits
                No  -> Need Windows/AD auth (Kerberos/NTLM)?
                         Yes -> IIS -> Kestrel
                         No  -> Need cert automation, caching, buffering, canary/blue-green?
                                  Yes -> Reverse Proxy (NGINX/IIS)
                                  No  -> Kestrel alone is fine (edge)
```

**Rules of thumb**
- **Use Kestrel alone** if: one site, basic TLS, no Windows auth, no special routing/limits.
- **Use NGINX** if: Linux/container-first, cert automation (Let's Encrypt), host/path routing, caching, canary/blue-green, WAF.
- **Use IIS** if: Windows/AD auth, enterprise governance, existing IIS ops model.

### Packaging & Installation Options

**A) Framework-dependent (FDD)**
```text
dotnet publish -c Release -o out
# Run (requires runtime on server)
dotnet out/MyApp.dll
```

**B) Self-contained (SCD) / Single-file**
```text
dotnet publish -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -p:PublishTrimmed=true -o out
chmod +x out/MyApp
./out/MyApp
```

**C) Containerized (recommended for consistency)**
```text
# Dockerfile (multi-stage)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet","MyApp.dll"]
```

### Binding Ports & TLS (Kestrel config)

`appsettings.json`:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http":  { "Url": "http://0.0.0.0:8080" },
      "Https": {
        "Url": "https://0.0.0.0:8443",
        "Certificate": { "Path": "/etc/ssl/mycert.pfx", "Password": "change-me" }
      }
    },
    "Limits": {
      "MaxConcurrentConnections": 1000,
      "MaxRequestBodySize": 104857600
    }
  }
}
```

`Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
    builder.Configuration.GetSection("Kestrel").Bind(options));
var app = builder.Build();
app.MapGet("/", () => "OK");
app.Run();
```

### Service/Lifecycle Management

**Linux: systemd** (`/etc/systemd/system/myapp.service`):
```text
[Unit]
Description=My ASP.NET Core app
After=network.target

[Service]
WorkingDirectory=/var/www/myapp
# Self-contained: direct exe; FDD: dotnet MyApp.dll
ExecStart=/var/www/myapp/MyApp
Restart=always
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5000

[Install]
WantedBy=multi-user.target
```
```text
sudo systemctl daemon-reload
sudo systemctl enable --now myapp
journalctl -u myapp -f
```

**Windows: Windows Service (no IIS)**
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
var app = builder.Build();
app.MapGet("/", () => "OK");
app.Run();
```
```text
sc.exe create MyApp binPath= "C:\apps\MyApp\MyApp.exe"
sc.exe start MyApp
```

**Windows: IIS -> Kestrel (reverse proxy)**
- Install the **ASP.NET Core Hosting Bundle**.
- Create an IIS site pointing to your deployment folder.
- IIS acts as a reverse proxy via the **ASP.NET Core Module** (ANCM).
- Use IIS for HTTPS bindings, logging, app pool recycling, and Windows/AD auth.

### NGINX in Front of Kestrel

Kestrel listens only on loopback (internal): `127.0.0.1:5000`. NGINX exposes :80/:443 and forwards to Kestrel.

```text
server {
    listen 80;
    server_name example.com;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl http2;
    server_name example.com;

    ssl_certificate     /etc/letsencrypt/live/example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/example.com/privkey.pem;
    add_header Strict-Transport-Security "max-age=31536000" always;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        client_max_body_size 100M; # aligns with Kestrel MaxRequestBodySize
    }
}
```

### Request/Threading Model

```text
Client sends HTTP request
  -> Kernel signals I/O readiness (IOCP/epoll/kqueue)
  -> .NET completes async read via Pipelines
  -> ASP.NET Core middleware pipeline runs on a ThreadPool worker thread
  -> Your code awaits I/O?
       Yes -> Thread returned to pool; await continuation resumes later
       No (CPU-bound) -> CPU work runs on pool thread (avoid long blocking)
  -> Write response asynchronously
  -> Kernel async write complete
  -> Request complete; connection kept-alive or closed
```

**Key points**
- **No one-thread-per-request.** Async I/O means **high concurrency** with few threads.
- **ThreadPool** grows/shrinks automatically (hill-climbing). You can raise minimums for bursty workloads:
  ```csharp
  ThreadPool.SetMinThreads(workerThreads: 200, completionPortThreads: 200);
  ```
- Avoid blocking calls (`.Result`, `.Wait()`) and long CPU loops in request handlers.
- For heavy CPU work, push to **background services/queues** or consider separate worker processes.

**Background jobs (the right way)**
```csharp
public sealed class QueueWorker : BackgroundService
{
    private readonly Channel<Job> _queue;
    public QueueWorker(Channel<Job> queue) => _queue = queue;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await foreach (var job in _queue.Reader.ReadAllAsync(ct))
        {
            await ProcessJobAsync(job, ct); // keep it async when possible
        }
    }
}
```

### Limits, Backpressure, and Timeouts
Kestrel uses **System.IO.Pipelines** with built-in **backpressure** (pauses reads if writes can't keep up). Useful knobs:
- `MaxConcurrentConnections`, `MaxConcurrentUpgradedConnections`
- `MaxRequestBodySize`, `RequestHeadersTimeout`, `KeepAliveTimeout`
- `MinRequestBodyDataRate` (protects from slowloris-like clients)
- Configure matching limits at the proxy (NGINX/IIS) for defense-in-depth.

### HTTP/2 & HTTP/3
- **HTTP/2**: Multiplexed streams over one TCP connection; Kestrel schedules each stream via the ThreadPool.
- **HTTP/3 (QUIC)**: Multiplexed streams over QUIC (UDP). Enable in config & ensure platform support and certs with proper ALPN.

### Security & Hardening Checklist
- Enforce HTTPS redirection and **HSTS**.
- Restrict **exposed ports** (public: only proxy 80/443; Kestrel on loopback).
- Set **request size limits** and **timeouts** (both proxy and Kestrel).
- Add security headers (X-Content-Type-Options, X-Frame-Options, CSP as needed).
- Keep runtime & base images updated; run as **non-root** in containers.

### Observability & Performance
- Enable structured logging; consider JSON logs.
- Use **dotnet counters** and **EventCounters**:
  ```text
  dotnet-counters monitor Microsoft.AspNetCore* System.Runtime*
  ```
- Track: ThreadPool starvation, GC pauses, % time in GC, CPU, allocation rate, request queuing, 99p latencies.
- Load test with realistic RPS and payload sizes (uploads/downloads).

### Two End-to-End Patterns

**A) Kestrel-only (Edge)**
1. Publish SCD, copy to server.
2. Bind HTTPS in `appsettings.json` (PFX + password) or via cert store.
3. Open :443, close everything else.
4. Set limits/timeouts. Add HTTPS redir + HSTS.

```text
Client --(443/TLS)--> Kestrel --(Middleware)--> Your ASP.NET Core app
```

**B) NGINX/IIS -> Kestrel (Recommended for complex prod)**
```text
Client --(80/443)--> Reverse Proxy (NGINX or IIS) --(loopback:5000)--> Kestrel --> Your app
```
Benefits: central TLS, routing, rate limits, buffering, caching, blue/green, Windows auth (IIS).

### Common Gotchas
- Forgetting `X-Forwarded-*` headers at the proxy -> wrong scheme/remote IP in app. (Set `UseForwardedHeaders()` when behind a proxy.)
- Client upload limits mismatch (NGINX `client_max_body_size` vs Kestrel `MaxRequestBodySize`). Keep them aligned.
- Blocking work in controllers -> ThreadPool stalls, latency spikes.
- Opening Kestrel publicly *and* via proxy simultaneously -> port conflicts or bypass paths. Bind Kestrel to loopback behind a proxy.

### Quick Reference Snippets

**Forwarded headers (behind proxy)**
```csharp
app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
```

**HTTPS redirection & HSTS**
```csharp
app.UseHttpsRedirection();
app.UseHsts(); // production only
```

**Set Kestrel URL via env**
```text
export ASPNETCORE_URLS=http://127.0.0.1:5000
```

**ThreadPool min threads (rarely needed)**
```csharp
ThreadPool.SetMinThreads(200, 200);
```

### IIS vs HTTP.sys vs Kestrel
- **Kestrel**: User-mode server for ASP.NET Core; cross-platform, high-perf, modern HTTP.
- **IIS -> Kestrel**: Reverse proxy + Windows features (Kerberos/NTLM, app pool mgmt).
- **HTTP.sys**: Kernel-mode HTTP server used directly by ASP.NET Core apps needing Windows-only features without the IIS UI.

---

## appsettings.json — Configuration

> A concise, production-focused reference for how **appsettings.json** powers configuration in ASP.NET Core, how it's loaded, overridden, and bound to strongly-typed options.

### What is `appsettings.json`?
`appsettings.json` is your app's **configuration file**. It holds values you don't want hard-coded — URLs, connection strings, feature flags, Kestrel options, logging levels, etc. — so the same build can run in multiple environments by just changing config.

**Key benefits**
- Centralizes config (no recompiles for value changes).
- Environment-specific overrides (Development/Staging/Production).
- Supports hot reload of config values (when providers are set to reload).
- Strongly-typed binding to options (`IOptions<T>`, `IOptionsSnapshot<T>`, `IOptionsMonitor<T>`).

### How Configuration is Loaded (Order & Precedence)
When you use `var builder = WebApplication.CreateBuilder(args);`, ASP.NET Core loads configuration from several providers; **later sources override earlier ones**:

1. `appsettings.json`
2. `appsettings.{Environment}.json` (e.g., `appsettings.Production.json`)
3. **User Secrets** (Development only, if configured)
4. **Environment variables**
5. **Command-line arguments**

> Tip: Use environment variables or CLI args to override sensitive or deployment-specific values without modifying files.

### Typical `appsettings.json` Contents
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

**Environment-specific overrides** (place only the **differences** in `appsettings.Production.json`):
```json
{
  "Logging": { "LogLevel": { "Default": "Warning" } },
  "FeatureFlags": { "UseNewCheckout": false }
}
```

### Using Configuration in Code

**Bind Kestrel and custom options**
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

**Options lifetimes (which one to choose?)**
- `IOptions<T>` — snapshot at startup; **does not** update on file change.
- `IOptionsSnapshot<T>` — scoped per request; picks up changes on the **next** request (web apps).
- `IOptionsMonitor<T>` — raises change notifications; use for singletons that must react to live changes.

### Overriding Values (env vars & CLI)

**Environment variables** use `__` to separate levels:
```text
# Override log level (Linux/macOS)
export Logging__LogLevel__Default=Warning

# Example Kestrel override
export Kestrel__Endpoints__Https__Url=https://0.0.0.0:8443
```

**Command-line arguments** use `:` separators:
```text
dotnet MyApp.dll --Logging:LogLevel:Default=Warning \
                 --Kestrel:Endpoints:Https:Url=https://0.0.0.0:8443
```

> Precedence: CLI > Environment variables > appsettings.{Environment}.json > appsettings.json

### Hot Reload of Config
The default JSON provider enables `reloadOnChange` for `appsettings.json` in most project templates. Pair this with `IOptionsSnapshot<T>` or `IOptionsMonitor<T>` if you want the app to see changes without a restart.

### Good Practices & Gotchas

**Do**
- Keep production secrets **out** of `appsettings.json`. Prefer environment variables, Secret Manager (dev), or a vault (e.g., Azure Key Vault).
- Keep the file **small & structured**; avoid storing large blobs.
- Mirror limits at the proxy (NGINX/IIS) and Kestrel (e.g., body size/timeouts).

**Avoid**
- Blocking calls in request handlers that depend on config values updating in real time (prefer `IOptionsMonitor<T>` for live changes).
- Divergent limits between NGINX/IIS and Kestrel (align `client_max_body_size` with `MaxRequestBodySize`, etc.).

### Quick Reference
- File names: `appsettings.json`, `appsettings.{Environment}.json`
- Environment name: `ASPNETCORE_ENVIRONMENT` = `Development|Staging|Production|...`
- Provider order (last wins): base JSON -> env JSON -> secrets -> env vars -> CLI
- Typed binding: `services.Configure<T>(configSection)`
- Live updates: `IOptionsSnapshot<T>` / `IOptionsMonitor<T>`

> **In short:** `appsettings.json` is the **source of truth for configurable behavior** in ASP.NET Core, with clean environment overrides and first-class, strongly-typed binding.

---

## wwwroot — Web Root & Static Files

> A concise, production-focused reference for how **`wwwroot`** works, how to serve and cache static assets, and how to customize or extend the web root.

### What is `wwwroot`?
`wwwroot` is the app's **web root** — everything under it is **publicly accessible** via HTTP as static files (not processed by MVC/Razor). It's a security boundary: **only `wwwroot` (and explicitly mapped folders) are web-exposed**; your code, views, configs, and secrets outside it are not.

**Examples**
- CSS: `wwwroot/css/site.css` -> `/css/site.css`
- JS: `wwwroot/js/app.js` -> `/js/app.js`
- Images/fonts: `wwwroot/images/logo.png` -> `/images/logo.png`
- SPA assets / `index.html`

### Enabling Static Files
Most templates already add the static-file middleware. Ensure it's in your pipeline:
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Optional: serve default docs (index.html) before static files
app.UseDefaultFiles(); // looks for index.html, default.html, etc.

// Serve files from wwwroot (default web root)
app.UseStaticFiles();

app.Run();
```
**Order matters:** `UseDefaultFiles()` must come **before** `UseStaticFiles()` to rewrite `/` to `/index.html` automatically.

### Typical Structure
```text
wwwroot/
  css/
    site.css
  js/
    app.js
  images/
    logo.png
  index.html    # optional for landing pages / SPAs
```
URL mapping is **relative to `wwwroot`**. File `wwwroot/images/logo.png` is requested at `/images/logo.png`.

### Customize the Web Root
Change the folder name/path if you prefer, e.g. `public`:

**Project file (`.csproj`):**
```text
<PropertyGroup>
  <WebRoot>public</WebRoot>
</PropertyGroup>
```

**Programmatic (rare):**
```csharp
builder.Environment.WebRootPath = Path.Combine(builder.Environment.ContentRootPath, "public");
```

### Serve Additional Folders
Mount an extra directory under a custom request path:
```csharp
using Microsoft.Extensions.FileProviders;

app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "SharedStatic")),
    RequestPath = "/static"
});
// Now /static/foo.js -> {ContentRoot}/SharedStatic/foo.js
```
You can register multiple such mounts (e.g., for shared assets between services).

### Caching, Versioning & Headers

**Cache-control** for long-lived assets:
```csharp
app.UseStaticFiles(new StaticFileOptions {
    OnPrepareResponse = ctx => {
        // 1 year
        const int duration = 60 * 60 * 24 * 365;
        ctx.Context.Response.Headers["Cache-Control"] = $"public,max-age={duration},immutable";
    }
});
```

**Cache busting** — Tag Helpers append a fingerprint automatically in Razor views:
```text
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
<script src="~/js/app.js" asp-append-version="true"></script>
```
Consider **ETag** and **Last-Modified** headers (the framework emits them for static files). Align caching with NGINX/IIS if a reverse proxy is used.

### Compression
Enable response compression for text assets (if not handled by your proxy):
```csharp
builder.Services.AddResponseCompression();
var app = builder.Build();
app.UseResponseCompression();
app.UseStaticFiles();
```
> If NGINX/IIS terminates TLS and handles compression, prefer enabling gzip/brotli there.

### SPA Hosting & Fallback
For SPAs, serve compiled assets from `wwwroot` and provide a fallback to `index.html` for client-side routes:
```csharp
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapFallbackToFile("index.html"); // after static files
```

### Security Notes
- **Everything in `wwwroot` is public.** Never place secrets or server-side-only files there.
- Static files **bypass authorization** by default. Authorization runs in MVC/endpoint middleware, not in the static-file middleware.
- Avoid enabling directory browsing in production; if you must:
  ```csharp
  builder.Services.AddDirectoryBrowser();
  app.UseDirectoryBrowser(new DirectoryBrowserOptions {
      RequestPath = "/assets" // mount read-only listings if required
  });
  ```
- By default, unknown file types are not served. To serve them (with caution):
  ```csharp
  app.UseStaticFiles(new StaticFileOptions {
      ServeUnknownFileTypes = false, // keep false unless you know the risks
      ContentTypeProvider = new FileExtensionContentTypeProvider()
  });
  ```

### MIME Types & Custom Mappings
Add or override content types when needed (e.g., `.webmanifest`, `.wasm`):
```csharp
using Microsoft.AspNetCore.StaticFiles;

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".webmanifest"] = "application/manifest+json";
provider.Mappings[".wasm"] = "application/wasm";

app.UseStaticFiles(new StaticFileOptions {
    ContentTypeProvider = provider
});
```

### Large Files / Range Requests
Static-file middleware supports **range requests** for efficient video/audio/file downloads. Ensure limits/timeouts align with your proxy (e.g., `client_max_body_size` in NGINX) and Kestrel's `MaxRequestBodySize` (uploads). For downloads, consider explicit `Content-Disposition` if you want "Save As".

### Reverse Proxy Interplay (NGINX/IIS)
- Offload **TLS, compression, caching** at the proxy for better performance.
- Set shared policies: cache lifetimes, max body size, timeouts, and security headers.
- Keep Kestrel bound to loopback when using a proxy; expose only 80/443 at the proxy.

### Request Flow (static vs. app)
```text
Client -> Static File Middleware
            -> File exists under wwwroot? -> Serve file (headers, cache/compress)
            -> Not found -> Next middleware / endpoints -> Controllers/Razor/Minimal APIs
```

> **Bottom line:** `wwwroot` is your **public static asset root**. Serve assets via `UseStaticFiles`, tune caching/compression, and keep sensitive content out. Customize or add mounts when you need more than one static source.
