# 🏛 The 4 Pillars of OOP in C#

---

## 1. Encapsulation
- **Definition**: Wrapping data (fields) and behavior (methods) together inside a class, while hiding internal details.
- **Goal**: Control access and protect the integrity of data.
- **How in C#**:
  - Use **access modifiers** (`private`, `public`, `protected`).
  - Use **properties** to control how data is read/written.

**Example**:
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
🔑 Only `Deposit` can change the balance.

**Analogy**: ATM machine → you interact via buttons and screen, not the wiring inside.

---

## 2. Abstraction
- **Definition**: Hiding complex implementation and exposing only essential features.
- **Goal**: Expose *what* an object does, not *how* it does it.
- **How in C#**: Use **abstract classes** and **interfaces**.

**Example**:
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
🔑 Client just calls `ProcessPayment` without caring how it works.

**Analogy**: Driving a car → you use pedals and steering, not the engine internals.

---

## 3. Inheritance
- **Definition**: Mechanism to create a new class from an existing class.
- **Goal**: Reuse code and establish parent-child relationship.
- **How in C#**: Use `:` for inheritance.

**Example**:
```csharp
public class Vehicle
{
    public void Start() => Console.WriteLine("Vehicle starting...");
}

public class Car : Vehicle
{
    public void Honk() => Console.WriteLine("Car honking...");
}
```

**Usage**:
```csharp
Car car = new Car();
car.Start(); // inherited
car.Honk();  // specific to Car
```

**Analogy**: A `Car` is a `Vehicle` with extra functionality.

---

## 4. Polymorphism
- **Definition**: One name, many forms. Methods behave differently depending on object type.
- **Goal**: Provide flexibility and reuse code.
- **How in C#**:
  - **Overriding** (runtime polymorphism with `virtual`/`override`).
  - **Overloading** (compile-time polymorphism).

**Example (Runtime Polymorphism)**:
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
```

**Usage**:
```csharp
Animal a1 = new Dog();
Animal a2 = new Cat();

a1.Speak(); // Dog barks
a2.Speak(); // Cat meows
```

**Analogy**: “Speak” → different behavior for dog, cat, human.

---

# ✅ Summary

| Pillar         | Meaning                              | Example in C#                      | Real-life Analogy          |
|----------------|--------------------------------------|------------------------------------|----------------------------|
| **Encapsulation** | Bundle data + methods, hide details | Private fields + public methods     | ATM machine                |
| **Abstraction**   | Expose *what*, hide *how*          | Interfaces, abstract classes        | Driving a car              |
| **Inheritance**   | Reuse/extend parent functionality  | `class Car : Vehicle`               | Car inherits Vehicle       |
| **Polymorphism**  | Many forms of same action          | Overloading, overriding             | “Speak” works differently  |

---
