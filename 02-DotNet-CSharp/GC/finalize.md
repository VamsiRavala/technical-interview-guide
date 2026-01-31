# 🛑 Finalize in .NET CLR

In .NET, **finalization** is how the **Garbage Collector (GC)** cleans up unmanaged resources if they weren’t explicitly released via `Dispose()`.

---

## 1. What is Finalize?
- `Finalize` is a special method called by the **GC before reclaiming an object’s memory**.
- In **C#**, you cannot override `Finalize()` directly. Instead, you declare a **destructor (`~ClassName`)**, which the compiler translates into an override of `Object.Finalize()`.

**Example**:
```csharp
class MyClass
{
    // Destructor (C# syntax) → compiled into Finalize()
    ~MyClass()
    {
        Console.WriteLine("Finalizer called!");
    }
}
```

---

## 2. Lifecycle of Finalization
1. Object becomes **unreachable**.
2. GC checks if it has a finalizer.
3. If yes → object is moved to the **finalization queue**.
4. Finalizer (`Finalize`) is executed on a dedicated GC thread.
5. Memory is released in the **next GC cycle**.

👉 Objects with finalizers live longer (at least two GC cycles).

---

## 3. Why Use Finalize?
- As a **safety net** to release **unmanaged resources** (file handles, sockets, OS handles, native pointers) if `Dispose` was not called.

---

## 4. Problems with Finalize
- **Non-deterministic**: You don’t know when it will run.
- **Performance cost**: Objects with finalizers survive longer.
- **Complex**: Should not touch managed objects (they may already be finalized).
- **Not guaranteed**: Finalizers may never run if process terminates abruptly.

---

## 5. Best Practices
1. Prefer **IDisposable + Dispose** for deterministic cleanup.
2. Implement `Finalize` only as a **backup** for unmanaged resources.
3. If implementing both:
   - Call `Dispose()` from `Finalize()` if not already called.
   - Use `GC.SuppressFinalize(this)` in `Dispose()`.

---

## 6. Example: Full Dispose + Finalize Pattern
```csharp
using System;

public class ResourceHolder : IDisposable
{
    private bool _disposed = false;

    // Destructor → compiles to Finalize()
    ~ResourceHolder()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this); // Skip finalizer if already cleaned up
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        // Always release unmanaged resources
        Console.WriteLine("Releasing unmanaged resources...");

        if (disposing)
        {
            // Release managed resources if needed
            Console.WriteLine("Releasing managed resources...");
        }

        _disposed = true;
    }
}
```

**Usage**:
```csharp
using (var obj = new ResourceHolder())
{
    Console.WriteLine("Using object...");
}
// Dispose() is called → GC.SuppressFinalize prevents Finalize
```

---

## ✅ Summary
- `Finalize` = last chance cleanup called by GC before object destruction.
- In C# written as **destructor (`~ClassName`)**.
- Should be used **only for unmanaged resources**.
- Always combine with **IDisposable** for deterministic cleanup.
- Use `GC.SuppressFinalize(this)` inside `Dispose` to avoid double cleanup.

---
