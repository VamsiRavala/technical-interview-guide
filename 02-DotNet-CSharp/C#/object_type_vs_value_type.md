# 📌 C# Object (Reference) Type vs Value Type

| Feature              | Value Type                          | Object / Reference Type              |
|----------------------|-------------------------------------|---------------------------------------|
| **Definition**       | Stores the **actual value**.        | Stores a **reference (address)** to the value. |
| **Examples**         | `int`, `double`, `bool`, `char`, `struct`, `enum` | `object`, `string`, `class`, `array`, `delegate` |
| **Memory Location**  | Stored in **stack**.                | Reference in stack, data in **heap**. |
| **Copy Behavior**    | Copies the **value** (independent). | Copies the **reference** (points to same object). |
| **Nullability**      | Cannot be `null` by default (use `int?`). | Can be `null` (risk of `NullReferenceException`). |
| **Performance**      | Faster (no GC overhead).            | Slower (GC overhead, heap access). |
| **Boxing/Unboxing**  | Required to store in `object`.      | Not required (already reference). |
| **Mutability**       | Changes don’t affect other copies.  | Changes via one reference affect all. |

---

## ✅ Example

```csharp
// Value Type
int a = 10;
int b = a;     // Copy of value
b = 20;
Console.WriteLine(a); // 10 (unchanged)

// Reference Type
class Person { public string Name; }
Person p1 = new Person();
p1.Name = "Tom";

Person p2 = p1;   // Reference copied
p2.Name = "Jerry";
Console.WriteLine(p1.Name); // Jerry (changed)
