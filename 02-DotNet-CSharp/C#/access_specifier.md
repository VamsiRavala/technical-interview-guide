# 🔒 Access Specifiers in C#

Access specifiers (a.k.a. access modifiers) define the **scope and visibility** of classes, methods, and variables.  

---

## 1. Public
- **Definition**: Accessible from **anywhere**, including other assemblies.
- **Use case**: When functionality should be globally available.

**Example**:
```csharp
public class Car
{
    public void Drive()
    {
        Console.WriteLine("Car is driving...");
    }
}
```

---

## 2. Private
- **Definition**: Accessible **only within the same class**.
- **Use case**: Hide internal details (encapsulation).

**Example**:
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

---

## 3. Protected
- **Definition**: Accessible within the same class and **derived classes**.
- **Use case**: Allow subclass customization but hide from external code.

**Example**:
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

---

## 4. Internal
- **Definition**: Accessible **within the same assembly/project**, but not from external assemblies.
- **Use case**: Limit usage to the same project.

**Example**:
```csharp
internal class Logger
{
    internal void Log(string message)
    {
        Console.WriteLine("Log: " + message);
    }
}
```

---

## 5. Protected Internal
- **Definition**: Accessible from:
  - The **same assembly** (like `internal`).
  - Any **derived class** (like `protected`), even in other assemblies.
- **Use case**: Frameworks/libraries where inheritance is allowed outside the assembly.

**Example**:
```csharp
public class BaseClass
{
    protected internal void Display()
    {
        Console.WriteLine("Protected Internal Method");
    }
}
```

---

## 6. Private Protected (C# 7.2+)
- **Definition**: Accessible **only within the same assembly** *and* **only by derived classes**.
- **Use case**: More restrictive than `protected internal`.

**Example**:
```csharp
public class BaseClass
{
    private protected void Secret()
    {
        Console.WriteLine("Private Protected Method");
    }
}
```

---

# ✅ Summary Table

| Access Modifier       | Same Class | Derived Class (Same Assembly) | Derived Class (Other Assembly) | Same Assembly (Non-derived) | Other Assembly |
|-----------------------|------------|-------------------------------|--------------------------------|------------------------------|----------------|
| **public**            | ✅         | ✅                            | ✅                             | ✅                           | ✅             |
| **private**           | ✅         | ❌                            | ❌                             | ❌                           | ❌             |
| **protected**         | ✅         | ✅                            | ✅                             | ❌                           | ❌             |
| **internal**          | ✅         | ✅                            | ❌                             | ✅                           | ❌             |
| **protected internal**| ✅         | ✅                            | ✅                             | ✅                           | ❌             |
| **private protected** | ✅         | ✅ (same assembly only)        | ❌                             | ❌                           | ❌             |

---
