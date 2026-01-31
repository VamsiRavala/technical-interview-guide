# 🔹 RyuJIT vs ReadyToRun (R2R) vs NativeAOT – When to Use What

This guide explains **when to use RyuJIT, R2R, and NativeAOT** in .NET Core, with practical examples.

---

## 🔹 1. RyuJIT (Default JIT) – Long-running services

✅ **Best for:** APIs, background workers, batch jobs, real-time processing  
⚡ **Why:** Startup cost is amortized; peak throughput matters most.

### 🛠 Example
- **ASP.NET Core Web API** serving millions of requests.
- Needs **dynamic libraries** (Entity Framework Core, reflection-heavy libs).
- Runs for hours/days inside a container or VM.

**Command:**

```bash
dotnet publish -c Release
```

👉 After a short warm-up, JIT + tiered compilation optimizes hot paths for **maximum throughput**.

---

## 🔹 2. ReadyToRun (R2R) – Middle ground

✅ **Best for:** Apps where **startup speed** matters but full .NET features are needed.  
⚡ **Why:** Precompiled IL reduces JIT work at runtime → faster startup.

### 🛠 Example
- **Desktop apps** (WPF/WinForms) → quick launch improves UX.  
- **Microservices** inside containers with frequent restarts.  
- **Command-line tools** needing snappy response (<0.5s startup).

**.csproj setting:**

```xml
<PropertyGroup>
  <PublishReadyToRun>true</PublishReadyToRun>
</PropertyGroup>
```

**Publish command:**

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

👉 Faster launches at the cost of **larger binaries**.

---

## 🔹 3. NativeAOT – Full native executables

✅ **Best for:** **Serverless functions, CLIs, lightweight daemons**.  
⚡ **Why:** No JIT → instant startup, small memory footprint.

### 🛠 Example
- **Azure Function / AWS Lambda** where cold-start time is critical.  
- **Cross-platform CLI tool** (like `kubectl`).  
- **Sidecar/agent** in a container where footprint must be minimal.

**.csproj setting:**

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
</PropertyGroup>
```

**Publish command:**

```bash
dotnet publish -c Release
```

👉 Produces a **standalone executable** (`myapp` or `myapp.exe`).  
⚠️ Limited reflection support (workarounds: **source generators**).

---

## ✅ Quick Comparison Table

| Mode        | When to Use | Example App | Command |
|-------------|-------------|-------------|---------|
| **RyuJIT**  | Long-running, throughput-sensitive | ASP.NET Core API, workers | `dotnet publish -c Release` |
| **R2R**     | Faster startup, still flexible | Desktop apps, microservices, CLIs | `dotnet publish -c Release -r linux-x64 /p:PublishReadyToRun=true` |
| **NativeAOT** | Instant startup, low memory | Serverless, sidecars, lightweight CLIs | `dotnet publish -c Release -r linux-x64 /p:PublishAot=true` |

---

## ✅ Rule of Thumb
- **Throughput matters most?** → **RyuJIT (default)**.  
- **Startup matters, but need full .NET features?** → **R2R**.  
- **Startup + memory critical, OK with reflection limits?** → **NativeAOT**.

