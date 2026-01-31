# 📌 C# `virtual` vs `override` vs `new` vs `abstract`

This cheat sheet covers **what they are, how they differ, when to use them, with examples and real-world scenarios**.

---

## 🔹 1) High-level Differences

| Keyword     | What it is                                                                 | Where it’s allowed              | Implementation in declaring class? | Polymorphism (runtime dispatch)? | Notes                                                                                          |
|-------------|-----------------------------------------------------------------------------|---------------------------------|------------------------------------|----------------------------------|------------------------------------------------------------------------------------------------|
| `virtual`   | Marks a base member as **overridable**.                                    | Concrete or abstract **class**  | ✅ Required (has a body)           | ✅ Yes                            | Provides a **default** behavior that derived classes *may* override.                          |
| `override`  | **Replaces** a `virtual`/`abstract` (or another `override`) member.        | Derived **class**               | ✅ Required                        | ✅ Yes                            | Must match signature. Can be combined with `sealed` to stop further overrides.                 |
| `new`       | **Hides** a base member (method/prop/etc.) intentionally.                  | Derived **class**               | ✅ Required (has a body)           | ❌ No                            | **Shadowing**: Call chosen by **reference type** at **compile time**, not runtime type.        |
| `abstract`  | Declares a member **without implementation**; must be overridden.          | **Abstract class** only         | ❌ No body                         | ✅ Yes (after overridden)         | Forces derived non-abstract classes to implement member.                                       |

---

## 🔹 2) Minimal Examples

### A. `virtual` + `override`
```csharp
public class Notifier
{
    public virtual void Send(string message) // overridable default
    {
        Console.WriteLine($"[Email] {message}");
    }
}

public class SmsNotifier : Notifier
{
    public override void Send(string message) // replaces base behavior
    {
        Console.WriteLine($"[SMS] {message}");
    }
}

// Usage
Notifier n1 = new Notifier();
Notifier n2 = new SmsNotifier();

n1.Send("Hello"); // [Email] Hello
n2.Send("Hello"); // [SMS] Hello  (polymorphic call)
```

---

### B. `abstract` + `override`
```csharp
public abstract class Shape
{
    public abstract double Area(); // no body; must be overridden
}

public class Circle : Shape
{
    public double Radius { get; set; }
    public Circle(double r) => Radius = r;

    public override double Area() => Math.PI * Radius * Radius;
}

// Usage
Shape s = new Circle(2);
Console.WriteLine(s.Area()); // 12.566...
```

---

### C. `new` (method hiding / shadowing)
```csharp
public class Logger
{
    public void Log(string msg) => Console.WriteLine($"[Base] {msg}");
}

public class FileLogger : Logger
{
    public new void Log(string msg) => Console.WriteLine($"[Derived] {msg}");
}

// Usage
Logger a = new FileLogger();
FileLogger b = new FileLogger();

a.Log("Hi"); // [Base] Hi   (reference type = Logger → hidden member ignored)
b.Log("Hi"); // [Derived] Hi
```

---

## 🔹 3) When to Use What?

- **`virtual`** → When base class provides a **default behavior** that *can* be overridden.  
- **`override`** → When you want to **replace** base behavior (mandatory if base is `abstract`).  
- **`abstract`** → When you want to define a **contract** without a default implementation.  
- **`new`** → When you want to **hide** base behavior but not replace it polymorphically (use sparingly).  

---

## 🔹 4) Real-World Examples

### Payment Processing (`virtual` + `override`)
```csharp
public class PaymentProcessor
{
    public virtual void Pay(decimal amount)
        => Console.WriteLine($"Paying ${amount} using generic processor");
}

public class StripeProcessor : PaymentProcessor
{
    public override void Pay(decimal amount)
        => Console.WriteLine($"Paying ${amount} via Stripe API");
}
```
✅ Base provides a default; derived classes specialize behavior.

---

### Document Export (`abstract` + `override`)
```csharp
public abstract class Exporter
{
    public abstract void Export(string path);
}

public class PdfExporter : Exporter
{
    public override void Export(string path)
        => Console.WriteLine($"Exporting PDF to {path}");
}
```
✅ Abstract class defines a **contract**, derived classes implement it.

---

### Legacy Hiding (`new`)
```csharp
public class BaseService
{
    public void Run() => Console.WriteLine("Base run");
}

public class CustomService : BaseService
{
    public new void Run() => Console.WriteLine("Custom run");
}
```
✅ Keeps old API intact but allows derived type to behave differently **when referenced directly**.

---

## 🔑 Final Takeaways

- Use **`virtual` + `override`** → for extensible, polymorphic behavior.  
- Use **`abstract` + `override`** → to enforce contracts in base classes.  
- Use **`new`** → only when necessary (hiding can confuse).

## why "New"

When you inherit from a third-party class (or legacy code), you sometimes can’t modify the base implementation because:

It’s in a compiled DLL (no source available).

It wasn’t designed with virtual methods (so you cannot override).

You need different behavior for your derived class, but without breaking the existing API contract.

In such cases, the new keyword lets you hide (shadow) the base method with your own version.
