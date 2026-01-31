# 📌 C# Abstract Class vs Interface — Differences, Real-World Examples, and When to Use Which

This cheat sheet gives you a crisp comparison, practical examples, and guidance for day‑to‑day design decisions.

---

## 🔹 1) At a Glance (Key Differences)

| Aspect | **Abstract Class** | **Interface** |
|---|---|---|
| **Purpose** | Provide a **base type with shared state & behavior** and a contract to override. | Define a **contract/capability** to implement, typically across unrelated types. |
| **Members** | Can have **fields, constructors, state**, `virtual`/`abstract`/`sealed` members, access modifiers. | **No instance fields**; methods/properties/events/indexers. C# 8+ allows **default interface methods**; C# 11+ supports **static abstract** members for generic math. |
| **Implementation** | Can **implement** some behavior; force others via `abstract`. | Traditionally **no implementation**; C# 8+ can provide **default** implementations (use sparingly). |
| **Instantiation** | ❌ Cannot instantiate directly. | ❌ Cannot instantiate directly. |
| **Inheritance** | A class can inherit **one** abstract (or concrete) class. | A class/struct can implement **multiple** interfaces. |
| **Versioning** | Safer to add members without breaking derived classes. | Adding members may break implementers (unless default method provided). |
| **Use with State** | ✅ Holds shared state (fields) and protected helpers. | ❌ No instance state; expresses **capabilities** only. |
| **Best For** | Hierarchies with shared code, template methods, partial default behavior. | Contracts/APIs, mix‑ins (capabilities), composing behaviors across different hierarchies. |

> ⚠️ Rule of thumb: **Favor interfaces for public API contracts**; **use abstract classes** when you **also need shared state/implementation**.

---

## 🔹 2) Minimal Examples

### A) Abstract Class (shared code + contract)
```csharp
public abstract class DocumentExporter
{
    // Shared state & helper
    protected readonly string CompanyName;
    protected DocumentExporter(string companyName) => CompanyName = companyName;

    // Template Method pattern
    public void Export(string path)
    {
        PreExport();
        DoExport(path);        // must be implemented by derived classes
        PostExport();
    }

    protected virtual void PreExport()  => Console.WriteLine($"[{CompanyName}] Preparing...");
    protected abstract void DoExport(string path);
    protected virtual void PostExport() => Console.WriteLine("Done.");
}

public class PdfExporter : DocumentExporter
{
    public PdfExporter(string company) : base(company) {}
    protected override void DoExport(string path)
        => Console.WriteLine($"Writing PDF to {path}");
}
```

### B) Interface (capability/contract only)
```csharp
public interface IExportable
{
    void Export(string path);
}

public class Report : IExportable
{
    public void Export(string path) => Console.WriteLine($"Report -> {path}");
}

public class ImageAsset : IExportable
{
    public void Export(string path) => Console.WriteLine($"Image -> {path}");
}
```

### C) Combining Both (common in real life)
```csharp
public interface IRepository<T>
{
    T? GetById(Guid id);
    void Add(T entity);
}

// Base with shared plumbing + optional defaults
public abstract class RepositoryBase<T> : IRepository<T>
{
    protected readonly List<T> store = new();
    public virtual T? GetById(Guid id) => store.FirstOrDefault();
    public abstract void Add(T entity);
}

public class UserRepository : RepositoryBase<User>
{
    public override void Add(User entity) => store.Add(entity);
}
```

---

## 🔹 3) Real‑World Scenarios

1) **Payment Providers**  
   - **Interface**: `IPaymentProvider.Pay(...)` used by Stripe, PayPal, ApplePay, etc. (unrelated types share capability).  
   - **Abstract**: `PaymentProviderBase` supplies retry/policy/logging; providers override `ExecutePayment`.  

2) **Web Framework Controllers**  
   - **Abstract**: `ControllerBase` provides context, helpers, filters.  
   - **Interfaces**: `IActionFilter`, `IAuthorizationFilter` describe optional capabilities that any component can implement.  

3) **Storage Driver Model**  
   - **Interface**: `IBlobStorage` (`PutAsync`, `GetAsync`, `DeleteAsync`) for S3/Azure/GCS.  
   - **Abstract**: `BlobStorageBase` for shared auth/retry/metrics; concrete drivers override only the provider‑specific bits.  

4) **Domain Entities & Capabilities**  
   - Entities implement **interfaces** like `IAggregateRoot`, `INotifiable`.  
   - An **abstract** `Entity` base supplies Id, equality, domain events list.  

---

## 🔹 4) When to Use What?

### Choose **Interface** when…
- You expose a **public API/SDK** that many different consumers will implement.  
- You need **multiple inheritance of behavior** (a class can implement several interfaces).  
- The types are **unrelated** but share a **capability** (e.g., `IDisposable`, `IComparable<T>`).  
- You want to **decouple** modules and ease mocking in tests (interfaces mock cleanly).  

### Choose **Abstract Class** when…
- You need **shared state/fields** or **protected helpers**.  
- You want a **template method** with a **partial default implementation**.  
- You need stronger **versioning** guarantees without breaking implementers.  
- You control the hierarchy and want to centralize invariants and cross‑cutting concerns.  

### Consider **Both** when…
- The **interface** defines the outward **contract**, and an **abstract base** offers a convenient default implementation.  
- Library users can either implement the interface directly **or** inherit from the base for faster onboarding.  

---

## 🔹 5) Subtleties (Modern C#)

- **Default interface methods (C# 8+)** let you ship non‑breaking additions, but overuse can blur the line with abstract bases—use mainly for **small opt‑in defaults**.  
- **Static abstract interface members (C# 11+)** enable generic math and static polymorphism (e.g., `INumber<T>`).  
- **Records** can implement interfaces or inherit abstract bases; prefer interfaces for **behaviors**, bases for **shared mechanics/state**.  

---

## 🔹 6) Quick Decision Flow

1. Need to **share code or state**? → **Abstract class**.  
2. Need **multiple contracts** on the same type or **public extensibility**? → **Interface**.  
3. Need both? → Interface **+** Abstract base.  

---

## ✅ TL;DR

- Use **interfaces** to describe **what** something can do.  
- Use **abstract classes** to share **how** things are partially done while forcing key pieces to be provided by derived classes.  

