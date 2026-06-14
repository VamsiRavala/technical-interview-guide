# C# Language Fundamentals

> A consolidated reference covering OOP pillars, abstract classes vs interfaces, inheritance keywords, access modifiers, variables, type system (value vs reference, `object`/`var`/`dynamic`), equality, collection/LINQ interfaces, object copying, and threading basics.

---

# Part 1 — OOP & Types

## The 4 Pillars of OOP in C#

### 1. Encapsulation
- **Definition**: Wrapping data (fields) and behavior (methods) together inside a class, while hiding internal details.
- **Goal**: Control access and protect the integrity of data.
- **How in C#**: Use **access modifiers** (`private`, `public`, `protected`); use **properties** to control how data is read/written.

```csharp
public class BankAccount
{
    private decimal balance; // hidden data

    public void Deposit(decimal amount)
    {
        if (amount > 0) balance += amount;
    }

    public decimal Balance
    {
        get { return balance; } // read-only
    }
}
```
Only `Deposit` can change the balance. **Analogy**: an ATM machine — you interact via buttons and screen, not the wiring inside.

### 2. Abstraction
- **Definition**: Hiding complex implementation and exposing only essential features.
- **Goal**: Expose *what* an object does, not *how* it does it.
- **How in C#**: Use **abstract classes** and **interfaces**.

```csharp
public abstract class Payment
{
    public abstract void ProcessPayment(decimal amount);
}

public class CreditCardPayment : Payment
{
    public override void ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Paid {amount} via Credit Card.");
    }
}

public class PayPalPayment : Payment
{
    public override void ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Paid {amount} via PayPal.");
    }
}
```
Client just calls `ProcessPayment` without caring how it works. **Analogy**: driving a car — you use pedals and steering, not the engine internals.

### 3. Inheritance
- **Definition**: Mechanism to create a new class from an existing class.
- **Goal**: Reuse code and establish a parent-child relationship.
- **How in C#**: Use `:` for inheritance.

```csharp
public class Vehicle
{
    public void Start() => Console.WriteLine("Vehicle starting...");
}

public class Car : Vehicle
{
    public void Honk() => Console.WriteLine("Car honking...");
}

// Usage
Car car = new Car();
car.Start(); // inherited
car.Honk();  // specific to Car
```
**Analogy**: a `Car` is a `Vehicle` with extra functionality.

### 4. Polymorphism
- **Definition**: One name, many forms. Methods behave differently depending on object type.
- **Goal**: Provide flexibility and reuse code.
- **How in C#**: **Overriding** (runtime polymorphism with `virtual`/`override`) and **Overloading** (compile-time polymorphism).

```csharp
public class Animal
{
    public virtual void Speak() => Console.WriteLine("Animal speaks");
}

public class Dog : Animal
{
    public override void Speak() => Console.WriteLine("Dog barks");
}

public class Cat : Animal
{
    public override void Speak() => Console.WriteLine("Cat meows");
}

// Usage
Animal a1 = new Dog();
Animal a2 = new Cat();
a1.Speak(); // Dog barks
a2.Speak(); // Cat meows
```
**Analogy**: "Speak" behaves differently for dog, cat, or human.

### Summary

| Pillar         | Meaning                              | Example in C#                  | Real-life Analogy   |
|----------------|--------------------------------------|--------------------------------|---------------------|
| **Encapsulation** | Bundle data + methods, hide details | Private fields + public methods | ATM machine        |
| **Abstraction**   | Expose *what*, hide *how*          | Interfaces, abstract classes    | Driving a car      |
| **Inheritance**   | Reuse/extend parent functionality  | `class Car : Vehicle`           | Car inherits Vehicle |
| **Polymorphism**  | Many forms of same action          | Overloading, overriding         | "Speak" works differently |

---

## Abstract Class vs Interface

### At a Glance (Key Differences)

| Aspect | **Abstract Class** | **Interface** |
|---|---|---|
| **Purpose** | Provide a **base type with shared state & behavior** and a contract to override. | Define a **contract/capability** to implement, typically across unrelated types. |
| **Members** | Can have **fields, constructors, state**, `virtual`/`abstract`/`sealed` members, access modifiers. | **No instance fields**; methods/properties/events/indexers. C# 8+ allows **default interface methods**; C# 11+ supports **static abstract** members for generic math. |
| **Implementation** | Can **implement** some behavior; force others via `abstract`. | Traditionally **no implementation**; C# 8+ can provide **default** implementations (use sparingly). |
| **Instantiation** | Cannot instantiate directly. | Cannot instantiate directly. |
| **Inheritance** | A class can inherit **one** abstract (or concrete) class. | A class/struct can implement **multiple** interfaces. |
| **Versioning** | Safer to add members without breaking derived classes. | Adding members may break implementers (unless a default method is provided). |
| **Use with State** | Holds shared state (fields) and protected helpers. | No instance state; expresses **capabilities** only. |
| **Best For** | Hierarchies with shared code, template methods, partial default behavior. | Contracts/APIs, mix-ins (capabilities), composing behaviors across different hierarchies. |

> Rule of thumb: **Favor interfaces for public API contracts**; **use abstract classes** when you **also need shared state/implementation**.

### Minimal Examples

**A) Abstract Class (shared code + contract)**
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

**B) Interface (capability/contract only)**
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

**C) Combining Both (common in real life)**
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

### Real-World Scenarios

1. **Payment Providers** — *Interface*: `IPaymentProvider.Pay(...)` used by Stripe, PayPal, ApplePay (unrelated types share a capability). *Abstract*: `PaymentProviderBase` supplies retry/policy/logging; providers override `ExecutePayment`.
2. **Web Framework Controllers** — *Abstract*: `ControllerBase` provides context, helpers, filters. *Interfaces*: `IActionFilter`, `IAuthorizationFilter` describe optional capabilities.
3. **Storage Driver Model** — *Interface*: `IBlobStorage` (`PutAsync`, `GetAsync`, `DeleteAsync`) for S3/Azure/GCS. *Abstract*: `BlobStorageBase` for shared auth/retry/metrics; concrete drivers override only provider-specific bits.
4. **Domain Entities & Capabilities** — Entities implement interfaces like `IAggregateRoot`, `INotifiable`. An abstract `Entity` base supplies Id, equality, and a domain events list.

### When to Use What?

**Choose Interface when:**
- You expose a **public API/SDK** that many different consumers will implement.
- You need **multiple inheritance of behavior** (a class can implement several interfaces).
- The types are **unrelated** but share a **capability** (e.g., `IDisposable`, `IComparable<T>`).
- You want to **decouple** modules and ease mocking in tests (interfaces mock cleanly).

**Choose Abstract Class when:**
- You need **shared state/fields** or **protected helpers**.
- You want a **template method** with a **partial default implementation**.
- You need stronger **versioning** guarantees without breaking implementers.
- You control the hierarchy and want to centralize invariants and cross-cutting concerns.

**Consider Both when:**
- The **interface** defines the outward **contract**, and an **abstract base** offers a convenient default implementation.
- Library users can either implement the interface directly **or** inherit from the base for faster onboarding.

### Subtleties (Modern C#)
- **Default interface methods (C# 8+)** let you ship non-breaking additions, but overuse can blur the line with abstract bases — use mainly for **small opt-in defaults**.
- **Static abstract interface members (C# 11+)** enable generic math and static polymorphism (e.g., `INumber<T>`).
- **Records** can implement interfaces or inherit abstract bases; prefer interfaces for **behaviors**, bases for **shared mechanics/state**.

### Quick Decision Flow
1. Need to **share code or state**? -> **Abstract class**.
2. Need **multiple contracts** on the same type or **public extensibility**? -> **Interface**.
3. Need both? -> Interface **+** Abstract base.

> **TL;DR:** Use **interfaces** to describe **what** something can do. Use **abstract classes** to share **how** things are partially done while forcing key pieces to be provided by derived classes.

---

# Part 2 — Methods, Inheritance & Keywords

## `virtual` vs `override` vs `new` vs `abstract`

### High-Level Differences

| Keyword     | What it is | Where allowed | Implementation in declaring class? | Polymorphism (runtime dispatch)? | Notes |
|-------------|------------|---------------|------------------------------------|----------------------------------|-------|
| `virtual`   | Marks a base member as **overridable**. | Concrete or abstract **class** | Required (has a body) | Yes | Provides a **default** behavior derived classes *may* override. |
| `override`  | **Replaces** a `virtual`/`abstract` (or another `override`) member. | Derived **class** | Required | Yes | Must match signature. Can combine with `sealed` to stop further overrides. |
| `new`       | **Hides** a base member intentionally. | Derived **class** | Required (has a body) | No | **Shadowing**: call chosen by **reference type** at **compile time**, not runtime type. |
| `abstract`  | Declares a member **without implementation**; must be overridden. | **Abstract class** only | No body | Yes (after overridden) | Forces derived non-abstract classes to implement the member. |

### Minimal Examples

**A. `virtual` + `override`**
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

**B. `abstract` + `override`**
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

**C. `new` (method hiding / shadowing)**
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
a.Log("Hi"); // [Base] Hi   (reference type = Logger -> hidden member ignored)
b.Log("Hi"); // [Derived] Hi
```

### When to Use What?
- **`virtual`** -> when the base class provides a **default behavior** that *can* be overridden.
- **`override`** -> when you want to **replace** base behavior (mandatory if base is `abstract`).
- **`abstract`** -> when you want to define a **contract** without a default implementation.
- **`new`** -> when you want to **hide** base behavior but not replace it polymorphically (use sparingly).

### Real-World Examples

**Payment Processing (`virtual` + `override`)**
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

**Document Export (`abstract` + `override`)**
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

**Legacy Hiding (`new`)**
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

### Why "new"?
When you inherit from a third-party class (or legacy code), you sometimes can't modify the base implementation because:
- It's in a compiled DLL (no source available).
- It wasn't designed with virtual methods (so you cannot override).
- You need different behavior for your derived class without breaking the existing API contract.

In such cases, the `new` keyword lets you hide (shadow) the base method with your own version.

### Final Takeaways
- Use **`virtual` + `override`** for extensible, polymorphic behavior.
- Use **`abstract` + `override`** to enforce contracts in base classes.
- Use **`new`** only when necessary (hiding can confuse).

---

## Sealed Class

A **sealed class** is a class marked with the `sealed` keyword. It **cannot be inherited** but can be instantiated and extended with extension methods.

### Key Properties

**1. Can a sealed class be instantiated?** Yes.
```csharp
public sealed class Logger
{
    public void Log(string message)
    {
        Console.WriteLine(message);
    }
}

var log = new Logger();
log.Log("Hello");
```

**2. Can a sealed class be inherited?** No — attempting to do so causes a compile-time error.
```csharp
public class FileLogger : Logger // Error: Logger is sealed
{
}
```

**3. Can a sealed class have extension methods?** Yes.
```csharp
public static class LoggerExtensions
{
    public static void LogWarning(this Logger logger, string message)
    {
        logger.Log("WARNING: " + message);
    }
}

var log = new Logger();
log.LogWarning("Low disk space!");
```
Even though `Logger` is sealed, extension methods work fine since they're just syntactic sugar for static methods.

### Summary

| Feature                     | Sealed Class |
|------------------------------|--------------|
| Can be instantiated?         | Yes          |
| Can be inherited?            | No           |
| Can have extension methods?  | Yes          |

### When to Use Sealed Classes?
- **Security** — Prevents untrusted classes from inheriting.
- **Design** — Restricts extensibility when a class should be final (e.g., `System.String`).
- **Performance** — JIT compiler optimizations are possible when classes are sealed.

---

## Access Specifiers (Access Modifiers)

Access specifiers define the **scope and visibility** of classes, methods, and variables.

### 1. Public
Accessible from **anywhere**, including other assemblies. Use when functionality should be globally available.
```csharp
public class Car
{
    public void Drive()
    {
        Console.WriteLine("Car is driving...");
    }
}
```

### 2. Private
Accessible **only within the same class**. Use to hide internal details (encapsulation).
```csharp
public class BankAccount
{
    private decimal balance; // hidden from outside

    public void Deposit(decimal amount)
    {
        if (amount > 0) balance += amount;
    }
}
```

### 3. Protected
Accessible within the same class and **derived classes**. Use to allow subclass customization but hide from external code.
```csharp
public class Vehicle
{
    protected void StartEngine()
    {
        Console.WriteLine("Engine started.");
    }
}

public class Car : Vehicle
{
    public void Drive()
    {
        StartEngine(); // Accessible here
        Console.WriteLine("Car is driving...");
    }
}
```

### 4. Internal
Accessible **within the same assembly/project**, but not from external assemblies.
```csharp
internal class Logger
{
    internal void Log(string message)
    {
        Console.WriteLine("Log: " + message);
    }
}
```

### 5. Protected Internal
Accessible from the **same assembly** (like `internal`) OR any **derived class** (like `protected`), even in other assemblies. Use in frameworks/libraries where inheritance is allowed outside the assembly.
```csharp
public class BaseClass
{
    protected internal void Display()
    {
        Console.WriteLine("Protected Internal Method");
    }
}
```

### 6. Private Protected (C# 7.2+)
Accessible **only within the same assembly** *and* **only by derived classes**. More restrictive than `protected internal`.
```csharp
public class BaseClass
{
    private protected void Secret()
    {
        Console.WriteLine("Private Protected Method");
    }
}
```

### Summary Table

| Access Modifier       | Same Class | Derived Class (Same Assembly) | Derived Class (Other Assembly) | Same Assembly (Non-derived) | Other Assembly |
|-----------------------|------------|-------------------------------|--------------------------------|------------------------------|----------------|
| **public**            | Yes        | Yes                           | Yes                            | Yes                          | Yes            |
| **private**           | Yes        | No                            | No                             | No                           | No             |
| **protected**         | Yes        | Yes                           | Yes                            | No                           | No             |
| **internal**          | Yes        | Yes                           | No                             | Yes                          | No             |
| **protected internal**| Yes        | Yes                           | Yes                            | Yes                          | No             |
| **private protected** | Yes        | Yes (same assembly only)      | No                             | No                           | No             |

---

## Variables

| Concept              | Syntax / Example | Notes |
|----------------------|------------------|-------|
| **Declaration**      | `int age;` | Declares a variable without assigning a value. |
| **Initialization**   | `int age = 25;` | Declares and assigns a value. |
| **Assignment**       | `age = 30;` | Updates value of variable. |
| **Naming Rules**     | `string userName;` | Start with letter/`_`; case-sensitive; no spaces/symbols. |
| **Value Types**      | `int x = 10;` / `double pi = 3.14;` / `char grade = 'A';` / `bool isReady = true;` | Stored directly in memory. |
| **Reference Types**  | `string name = "Tom";` / `object obj = 5;` | Store a reference (address). |
| **var**              | `var msg = "Hello";` | Type inferred at compile time. |
| **dynamic**          | `dynamic val = 10;` | Type checked at runtime, flexible but less safe. |
| **object**           | `object num = 100;` | Base type for all data types. Requires casting. |
| **const**            | `const double Pi = 3.14159;` | Compile-time constant (must assign at declaration). |
| **readonly**         | `readonly int year;` then `year = 2025;` (in constructor) | Assign once at declaration or in constructor. |
| **Scope: Local**     | `void Test(){ int x=10; }` | Exists only inside method/block. |
| **Scope: Field**     | `private int count;` | Declared at class level, accessible in class. |
| **Scope: Static**    | `static int total;` | Shared across all objects of a class. |

**Tips**
- Prefer `var` for readability when the type is obvious.
- Use `const` for fixed values, `readonly` for runtime-assigned constants.
- Avoid `dynamic` unless necessary.
- Always use meaningful variable names.

---

# Part 3 — Value vs Reference & Equality

## Object (Reference) Type vs Value Type

| Feature              | Value Type                          | Object / Reference Type              |
|----------------------|-------------------------------------|---------------------------------------|
| **Definition**       | Stores the **actual value**.        | Stores a **reference (address)** to the value. |
| **Examples**         | `int`, `double`, `bool`, `char`, `struct`, `enum` | `object`, `string`, `class`, `array`, `delegate` |
| **Memory Location**  | Stored in **stack**.                | Reference in stack, data in **heap**. |
| **Copy Behavior**    | Copies the **value** (independent). | Copies the **reference** (points to same object). |
| **Nullability**      | Cannot be `null` by default (use `int?`). | Can be `null` (risk of `NullReferenceException`). |
| **Performance**      | Faster (no GC overhead).            | Slower (GC overhead, heap access). |
| **Boxing/Unboxing**  | Required to store in `object`.      | Not required (already a reference). |
| **Mutability**       | Changes don't affect other copies.  | Changes via one reference affect all. |

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
```

---

## `object` vs `var` vs `dynamic`

| Feature              | `object` | `var` | `dynamic` |
|----------------------|----------|-------|-----------|
| **What it is**       | Base type of all types in C#. | Keyword for *implicit typing*. | Type resolved at runtime. |
| **Type Resolution**  | Always `System.Object`. | Compile-time (compiler infers exact type). | Runtime (late binding). |
| **Type Safety**      | Weak — needs casting. | Strong — compiler enforces type. | Weak — no compile-time checks. |
| **Flexibility**      | Can store any type, but must cast back. | Fixed type after initialization. | Can change type at runtime. |
| **Boxing/Unboxing**  | Yes (for value types). | No boxing/unboxing. | No boxing/unboxing. |
| **Performance**      | Slower (boxing/unboxing + casting). | Faster (static typing). | Slower (runtime lookup). |
| **Error Detection**  | Runtime (if wrong cast). | Compile-time. | Runtime (if member doesn't exist). |
| **Use Cases**        | Generic storage, legacy code, non-generic collections. | LINQ, anonymous types, cleaner code when type is obvious. | Interop with COM, reflection, JSON, dynamic APIs. |

**Example: `object`**
```csharp
object obj = 10;        // boxing int into object
Console.WriteLine(obj); // 10

int num = (int)obj;     // must cast back
// string str = (string)obj; // Runtime error
```

**Example: `var`**
```csharp
var number = 10;        // compiler infers int
Console.WriteLine(number); // 10

// number = "Hello";    // Compile-time error
```

**Example: `dynamic`**
```csharp
dynamic value = 10;
Console.WriteLine(value);    // 10 (int)

value = "Hello";
Console.WriteLine(value);    // Hello (string)

value.NonExistingMethod();   // Runtime error (no compile error)
```

**Key Takeaways**
- `object` -> universal base type, needs casting (old way; prefer generics instead).
- `var` -> compiler infers type, type-safe, fixed type (preferred in most cases).
- `dynamic` -> runtime typing, flexible but risky (use only when type is unknown until runtime).

> Note: `var` is compile-time inference (fast, type-safe), while `dynamic` resolves at runtime (flexible but slower and error-prone). Use `var` when the type is clear and fixed; use `dynamic` when the type is unknown until runtime.

---

## `Equals()` vs `==`

### High-Level Difference

| Feature | `Equals()` | `==` |
|---------|------------|------|
| Type | Virtual **method** (from `object`) | **Operator**, can be overloaded |
| Default (Reference Types) | Compares **references** (identity) | Compares **references** unless overloaded |
| Default (Value Types) | Field-by-field comparison (`ValueType.Equals`) | Value comparison (for built-in types); not defined unless overloaded |
| Overridable? | Yes (override in your class/struct) | Yes (operator overload) |
| Polymorphic? | Yes (virtual dispatch at runtime) | No (resolved at compile-time unless overloaded) |
| Null Handling | `null.Equals(x)` throws, but `object.Equals(null, x)` works | `null == null` is safe (`true`) |

### Default Behavior
- **Reference types (classes)**: `Equals()` compares references (same object?); `==` compares references unless overridden.
- **Value types (structs)**: `Equals()` compares fields; `==` works for built-in structs (`int`, `double`, etc.) — custom structs must overload.

### Special Case: `string`
Both are overridden to compare **contents**:
```csharp
string a = "hello";
string b = new string("hello".ToCharArray());

Console.WriteLine(a == b);       // True (value comparison)
Console.WriteLine(a.Equals(b));  // True (value comparison)
```

### When to Use

**Use `==` when:**
- Comparing primitives (`int`, `double`, `bool`).
- Comparing strings (clean syntax, compares values).
- When the type properly overloads `==` (e.g., `DateTime`, `Guid`).

**Use `Equals()` when:**
- You need polymorphic equality (base class reference -> derived class).
- Implementing equality in custom types (`override Equals()` + `GetHashCode()`).
- Avoiding surprises with overloaded operators.

**Avoid pitfalls:**
- If a class doesn't overload `==`, it defaults to reference equality.
- Use `== null`, not `Equals(null)` (avoids `NullReferenceException`).

### Example
```csharp
public class Person
{
    public string Name { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is Person other)
            return Name == other.Name;
        return false;
    }

    public override int GetHashCode() => Name.GetHashCode();

    // Overload operators for consistency
    public static bool operator ==(Person left, Person right) =>
        Equals(left, right);

    public static bool operator !=(Person left, Person right) =>
        !Equals(left, right);
}

var p1 = new Person { Name = "Alice" };
var p2 = new Person { Name = "Alice" };

Console.WriteLine(p1.Equals(p2)); // True
Console.WriteLine(p1 == p2);      // True (because we overloaded)
```
Without the operator overload, `p1 == p2` would be `false` (different references).

### Best Practices
- If you override `Equals()`, also override `GetHashCode()` and (ideally) `==`/`!=`.
- Use `==` for primitives and strings.
- Use `Equals()` for polymorphism or null-safe static call (`object.Equals(a,b)`).
- Keep `Equals`, `==`, and `GetHashCode` consistent.

### How Comparison Works
```text
Compare x and y
  -> Are they value types?
       Yes -> == compares values directly; Equals compares fields
       No (reference types) ->
            Is == overloaded?
                 No  -> == compares references
                 Yes -> == uses overloaded implementation
            Is Equals overridden?
                 No  -> Equals compares references
                 Yes -> Equals uses custom value comparison
```

### Quick Summary
- `==` -> operator, can be overloaded, defaults to reference equality (except for value types/strings).
- `Equals()` -> virtual method, polymorphic, usually overridden for value semantics.
- **Strings** -> both compare values.
- Always override `Equals`, `GetHashCode`, and `==` together for consistency.

---

## Object Copying: Shallow vs Deep Copy

| Feature              | Shallow Copy | Deep Copy |
|----------------------|--------------|-----------|
| **Definition**       | Copies the object but references inside still point to the same memory. | Creates a completely new object including copies of all nested objects. |
| **Method**           | `MemberwiseClone()` | Manual copy (constructor, method) or serialization (Binary/JSON/XML). |
| **Performance**      | Faster (simple copy). | Slower (creates new objects). |
| **Nested Objects**   | Shared between original and copy. | Independent, fully cloned. |
| **Impact on Changes**| Changes to nested reference types affect both objects. | Changes affect only the copied object. |
| **Use Cases**        | When the object is flat (no nested refs). | When the object has nested objects and you need full independence. |

**Shallow Copy**
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
p2.Addr.City = "LA";     // Both p1 and p2 affected
```

**Deep Copy**
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
p2.Addr.City = "LA";     // p1 unaffected
```

**Key Takeaway**
- `=` -> just reference copy (both point to the same object).
- Shallow Copy -> new object, but nested refs are shared.
- Deep Copy -> new object, with fully independent nested copies.

---

# Part 4 — Collections & LINQ Interfaces

## IEnumerable, IQueryable, and List<T>

### 1. IEnumerable<T>
- **Definition**: The most basic collection interface. Represents a sequence of elements that can be enumerated (looped over).
- **Namespace**: `System.Collections.Generic`
- **Execution**: **Deferred execution** (especially with LINQ). Pulls data in-memory one item at a time.
- **Operations**: Supports **forward-only iteration** (can't go backward, can't use indexes); works best with **in-memory collections**.
```csharp
IEnumerable<int> numbers = new List<int> { 1, 2, 3 };
foreach (var num in numbers)
{
    Console.WriteLine(num);
}
```
- **Use Case**: When you just need to **iterate** through a sequence of data, without worrying about querying optimization or database execution.

### 2. IQueryable<T>
- **Definition**: Extends `IEnumerable<T>` to support **LINQ-to-SQL**, **LINQ-to-Entities**, or other providers.
- **Namespace**: `System.Linq`
- **Execution**: **Deferred execution**, but the query is translated into a **data source-specific query language** (like SQL).
- **Operations**: Builds an **expression tree** instead of running in-memory; useful when working with **remote data sources** (databases, APIs).
```csharp
IQueryable<Employee> employees = dbContext.Employees
                                          .Where(e => e.Age > 30);
// This gets converted to SQL: SELECT * FROM Employees WHERE Age > 30
```
- **Use Case**: When you need to **query a database or external source** and let the provider optimize execution.

### 3. List<T>
- **Definition**: A **concrete class** that implements `IEnumerable<T>`, `ICollection<T>`, and `IList<T>`.
- **Namespace**: `System.Collections.Generic`
- **Execution**: **Immediate execution** (data is already in memory).
- **Operations**: Supports **index-based access** (`list[0]`); allows adding/removing items dynamically; more flexible than arrays.
```csharp
List<string> names = new List<string> { "Alice", "Bob" };
names.Add("Charlie");
Console.WriteLine(names[1]); // Bob
```
- **Use Case**: When you want a **mutable collection** in memory with fast index-based access.

### Key Differences

| Feature                  | IEnumerable<T> | IQueryable<T> | List<T> |
|---------------------------|----------------|---------------|---------|
| **Type**                  | Interface      | Interface     | Class   |
| **Execution**             | Deferred       | Deferred (remote translation) | Immediate |
| **Data Source**           | In-memory      | Database / Remote | In-memory |
| **Indexing**              | No             | No            | Yes     |
| **LINQ Support**          | In-memory LINQ | Translates to SQL (etc.) | In-memory LINQ |
| **Performance**           | Good for small/simple iteration | Optimized for remote queries | Fast in-memory access |

### Real-World Example: Filtering Employees

We have a database table `Employees` with thousands of rows. We want employees whose **Age > 30**, then select just their **Name**.

**1. Using IEnumerable**
```csharp
IEnumerable<Employee> employees = dbContext.Employees.ToList();
// .ToList() brings ALL rows into memory!

var result = employees
                .Where(e => e.Age > 30)  // Filtering happens in MEMORY
                .Select(e => e.Name);

foreach (var name in result)
{
    Console.WriteLine(name);
}
```
Behavior: Fetches all rows into memory first; filtering happens locally in C#. **Inefficient** for large datasets.

**2. Using IQueryable**
```csharp
IQueryable<Employee> employees = dbContext.Employees;
// Query is not executed yet

var result = employees
                .Where(e => e.Age > 30)   // Builds expression tree
                .Select(e => e.Name);     // Still not executed

foreach (var name in result)
{
    Console.WriteLine(name);
}
```
Behavior: Translates to SQL `SELECT Name FROM Employees WHERE Age > 30;` and fetches only filtered results from the database. **Efficient** for large datasets.

**3. Using List<T>**
```csharp
List<Employee> employees = dbContext.Employees.ToList();
// Everything is in memory immediately

var result = employees
                .Where(e => e.Age > 30)
                .Select(e => e.Name);

foreach (var name in result)
{
    Console.WriteLine(name);
}
```
Behavior: Like IEnumerable, all rows are loaded first; supports index access (`employees[0]`). Still inefficient for large DB queries.

### Key Takeaway
- `IEnumerable` / `List<T>` -> query executes **in memory** after fetching all data.
- `IQueryable` -> query executes at the **database/source** level.

Always prefer **IQueryable** for database queries, and **List<T>** or **IEnumerable<T>** for in-memory work.

---

# Part 5 — Threading Basics

## Thread vs Task vs Process

### Process
- A **process** is an independent program in execution with its own memory space, system resources, and state.
- Each process is isolated from others to ensure stability and security.
- Processes are managed by the operating system's scheduler.

Examples: running a web browser, a text editor, a database server.
```csharp
Process.Start("notepad.exe");
```

### Thread
- A **thread** is the smallest unit of execution within a process.
- Threads within the same process share memory and resources but execute independently.
- Context switching between threads is faster than between processes.

Examples: a browser with one thread for the UI and another for network requests; a word processor with one thread for typing and another for spell-checking.
```csharp
Thread t = new Thread(() =>
{
    Console.WriteLine("Running in new thread");
});
t.Start();
```

### Task
- A **task** is a unit of work scheduled for execution.
- Conceptually it represents a scheduled job; it can map to either a thread or a process depending on the OS/environment.

Examples: a scheduled background backup job, a database query execution, an asynchronous operation (`async`/`await`).
```csharp
await Task.Run(() =>
{
    Console.WriteLine("Running in a task");
});
```

### Key Differences

| Aspect          | Process                     | Thread                       | Task                        |
|-----------------|-----------------------------|------------------------------|-----------------------------|
| Memory          | Independent memory space    | Shares memory with process   | Depends on implementation   |
| Scheduling Unit | OS-level scheduling         | Lightweight scheduling       | Logical unit of execution   |
| Overhead        | High (more resources)       | Low (lightweight)            | Varies                      |
| Communication   | Inter-process communication | Shared memory within process | Depends (async/IPC/etc.)    |

### Summary
- **Processes** are heavy, independent, isolated units of execution.
- **Threads** are lightweight, sharing resources but running independently within a process.
- **Tasks** are logical units of work that may be implemented as processes or threads depending on context.

---

## ThreadPool

A thread pool is a collection of reusable threads managed by a scheduler.

- Instead of creating a new thread for each task, tasks are submitted to the pool and assigned to available worker threads.
- This reduces overhead (creating/destroying threads is expensive).
- Pools also support queuing: if all threads are busy, tasks wait in a queue until a thread is free.
- This model is widely used in servers and async frameworks for handling large numbers of short-lived tasks.

```csharp
using System;
using System.Threading;

class Program
{
    static void Worker(object? taskId)
    {
        Console.WriteLine($"Task {taskId} executed by Thread {Thread.CurrentThread.ManagedThreadId}");
    }

    static void Main()
    {
        for (int i = 1; i <= 5; i++)
        {
            ThreadPool.QueueUserWorkItem(Worker, i);
        }

        Console.ReadLine(); // Keep app alive until tasks finish
    }
}
```

### What is `Task.Run`?
- A helper method introduced in .NET 4.5.
- Runs code asynchronously on the ThreadPool.
- The simplest way to say "I want this work to run in the background."
- Always uses safe, async-friendly defaults.

```csharp
await Task.Run(() => HeavyCalculation());
```
Runs `HeavyCalculation` on a background thread, freeing the caller thread.

### What is `Task.Factory.StartNew`?
- The original way (from .NET 4.0) to start tasks.
- More flexible, but also more complex.
- Lets you control: the scheduler (where the task runs, e.g., UI context, custom thread pool), task creation options (e.g., `LongRunning`, `DenyChildAttach`), and cancellation tokens.

```csharp
Task.Factory.StartNew(() => HeavyCalculation(),
    CancellationToken.None,
    TaskCreationOptions.LongRunning,
    TaskScheduler.Default);
```
