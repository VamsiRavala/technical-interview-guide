# 🗑️ Garbage Collection in .NET CLR

Garbage Collection (GC) in .NET is the automatic memory management system that reclaims memory used by objects no longer in use.

---

## 1. Basic Concept
- Objects created with `new` are stored on the **Managed Heap**.
- GC automatically detects and removes **unreachable objects** (not referenced anymore).
- This avoids manual memory management (like `free` or `delete` in C/C++).

---

## 2. Generations
GC uses a **generational model**:

- **Generation 0 (Gen 0)** → short-lived objects (local variables, temp data). Most frequent collections.
- **Generation 1 (Gen 1)** → medium-lived objects, acts as a buffer.
- **Generation 2 (Gen 2)** → long-lived objects (static fields, caches). Full collections include Gen 2.

### Memory Layout Diagram

```
+--------------------+--------------------+--------------------+
|   Generation 0     |   Generation 1     |   Generation 2     |
+--------------------+--------------------+--------------------+
         ↑                    ↑                    ↑
    Short-lived          Medium-lived         Long-lived
```

---

## 3. Large Object Heap (LOH)
- Objects **> 85 KB** are stored in the **Large Object Heap**.
- Collected only during **Gen 2 collections**.
- Prone to **fragmentation** because compaction is rare.

```
+----------------------+
|   Large Object Heap  |
+----------------------+
```

---

## 4. GC Process
The collection process follows **Mark → Sweep → Compact**:

1. **Mark**: Identify all **reachable/live objects** (roots like local vars, static fields).
2. **Sweep**: Reclaim memory of unreferenced objects.
3. **Compact**: Move live objects together to reduce fragmentation (not applied to LOH).

```
Before GC:   [Live][Dead][Live][Dead][Live]
After Sweep: [Live]     [Live]     [Live]
After Compact: [Live][Live][Live]
```

---

## 5. Types of GC
- **Workstation GC** → optimized for desktop, low-latency.
- **Server GC** → optimized for throughput, uses multiple threads/CPUs.
- **Background (Concurrent) GC** → runs alongside application threads to reduce pauses.

---

## 6. Finalization & IDisposable
- Objects with destructors (`~ClassName`) go through **finalization queue**, collected in the next cycle.
- For unmanaged resources (files, DB connections, sockets), use **IDisposable** with `using`:

```csharp
using (var file = new StreamWriter("test.txt"))
{
    file.WriteLine("Hello GC!");
} // Automatically calls Dispose()
```

---

## 7. When Does GC Run?
- When **Gen 0** heap is full.
- When system is **low on memory**.
- When `GC.Collect()` is called (not recommended).

---

## 8. Performance Tips
- Minimize allocations → fewer GCs.
- Reuse objects when possible.
- Avoid frequent large object allocations (LOH fragmentation).
- Use `using` / `Dispose` for unmanaged resources.

---

# ✅ Summary
- GC is **automatic memory management** in .NET CLR.
- Uses **generations (0, 1, 2)** for efficiency.
- Has a special **Large Object Heap** for big allocations.
- Runs in **Mark, Sweep, Compact** phases.
- Supports **Finalization** and **IDisposable** patterns.
- Improves developer productivity by handling memory safely.

