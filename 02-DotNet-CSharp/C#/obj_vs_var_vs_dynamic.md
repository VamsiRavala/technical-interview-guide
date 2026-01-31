# 🔹 C# `object` vs `var` vs `dynamic` Cheat Sheet

| Feature              | `object` | `var` | `dynamic` |
|----------------------|----------|-------|-----------|
| **What it is**       | Base type of all types in C#. | Keyword for *implicit typing*. | Type resolved at runtime. |
| **Type Resolution**  | Always `System.Object`. | Compile-time (compiler infers exact type). | Runtime (late binding). |
| **Type Safety**      | Weak → needs casting. | Strong → compiler enforces type. | Weak → no compile-time checks. |
| **Flexibility**      | Can store any type, but must cast back. | Fixed type after initialization. | Can change type at runtime. |
| **Boxing/Unboxing**  | Yes (for value types). | No boxing/unboxing. | No boxing/unboxing. |
| **Performance**      | Slower (due to boxing/unboxing + casting). | Faster (static typing). | Slower (runtime lookup). |
| **Error Detection**  | Runtime (if wrong cast). | Compile-time. | Runtime (if method/property doesn’t exist). |
| **Use Cases**        | Generic storage, legacy code, non-generic collections. | LINQ, anonymous types, cleaner code when type is obvious. | Interop with COM, reflection, JSON, dynamic APIs. |

---

## ✅ Example: `object`
```csharp
object obj = 10;        // boxing int into object
Console.WriteLine(obj); // 10

int num = (int)obj;     // ✅ must cast back
// string str = (string)obj; ❌ Runtime error
```

## ✅ Example: `var`
```csharp
var number = 10;        // compiler infers int
Console.WriteLine(number); // 10

// number = "Hello";    // ❌ Compile-time error

```

## ✅ Example: `dynamic`
```csharp
dynamic value = 10;
Console.WriteLine(value);    // 10 (int)

value = "Hello";
Console.WriteLine(value);    // Hello (string)

value.NonExistingMethod();   // ❌ Runtime error (no compile error)

```

## Key Takeaways

- object → Universal base type, needs casting. (Old way, use generics instead)

- var → Compiler infers type, type-safe, fixed type. (Preferred in most cases)

- dynamic → Runtime typing, flexible but risky. (Use only when type is unknown until runtime)
