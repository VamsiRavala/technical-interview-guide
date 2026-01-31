# 🔹 C# `var` vs `dynamic` Cheat Sheet

| Feature              | `var` | `dynamic` |
|----------------------|-------|-----------|
| **Type Resolution**  | Compile-time (decided by compiler). | Runtime (decided during execution). |
| **Type Safety**      | Strongly typed (compiler enforces). | Weakly typed (no compile-time checks). |
| **Flexibility**      | Fixed type after initialization. | Type can change at runtime. |
| **Performance**      | Faster (no runtime overhead). | Slower (runtime binding needed). |
| **Error Detection**  | Compile-time errors. | Runtime errors. |
| **Use Cases**        | LINQ queries, anonymous types, when type is obvious. | Reflection, COM objects, JSON parsing, dynamic APIs. |

---

## ✅ Example: `var`
```csharp
var number = 10;         // Compiler infers int
Console.WriteLine(number); // 10

number = "Hello";        // ❌ Compile-time error (cannot assign string to int)
```

## Example: `dynamic`
```csharp
dynamic value = 10;
Console.WriteLine(value);    // 10 (int)

value = "Hello";
Console.WriteLine(value);    // Hello (string)

value.NonExistingMethod();   // ❌ Runtime error (method not found)
```

## Key Takeaway

- Use var → when the type is clear and fixed. (Safe & efficient)

- Use dynamic → when type is unknown until runtime. (Flexible but risky)
