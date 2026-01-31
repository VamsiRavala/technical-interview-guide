# 📌 C# Object Copying: Shallow vs Deep Copy

| Feature              | Shallow Copy | Deep Copy |
|----------------------|--------------|-----------|
| **Definition**       | Copies the object but references inside still point to the same memory. | Creates a completely new object including copies of all nested objects. |
| **Method**           | `MemberwiseClone()` | Manual copy (constructor, method) or Serialization (Binary/JSON/XML). |
| **Performance**      | Faster (simple copy). | Slower (creates new objects). |
| **Nested Objects**   | Shared between original and copy. | Independent, fully cloned. |
| **Impact on Changes**| Changes to nested reference types affect both objects. | Changes affect only the copied object. |
| **Use Cases**        | When object is flat (no nested refs). | When object has nested objects and you need full independence. |

---

## ✅ Example: Shallow Copy
```csharp
class Person
{
    public string Name;
    public Address Addr;

    public Person ShallowCopy()
    {
        return (Person)this.MemberwiseClone();
    }
}

class Address { public string City; }

Person p1 = new Person { Name = "Tom", Addr = new Address { City = "NY" } };
Person p2 = p1.ShallowCopy();

p2.Name = "Jerry";       // Only p2 changes
p2.Addr.City = "LA";     // ❌ Both p1 and p2 affected
```

## ✅ Example: Deep Copy
```csharp
class Person
{
    public string Name;
    public Address Addr;

    public Person DeepCopy()
    {
        return new Person
        {
            Name = this.Name,
            Addr = new Address { City = this.Addr.City }
        };
    }
}

class Address { public string City; }

Person p1 = new Person { Name = "Tom", Addr = new Address { City = "NY" } };
Person p2 = p1.DeepCopy();

p2.Name = "Jerry";       // Only p2 changes
p2.Addr.City = "LA";     // ✅ p1 unaffected

```

## Key Takeaway

= → just reference copy (both point to same object).

Shallow Copy → New object, but nested refs are shared.

Deep Copy → New object, with fully independent nested copies.
