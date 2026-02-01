# Quick Review - Part 2

## C# OOP Topics

### Polymorphism
Polymorphism allows methods to do different things based on the object it is acting upon.

**Example:**
```csharp
public class Animal {
    public virtual void Speak() {
        Console.WriteLine("Animal speaks");
    }
}

public class Dog : Animal {
    public override void Speak() {
        Console.WriteLine("Bark");
    }
}

Animal myDog = new Dog();
myDog.Speak();  // Output: Bark
```

### Abstraction
Abstraction is the concept of exposing only the essential features of an object while hiding the unnecessary details.

**Example:**
```csharp
public abstract class Shape {
    public abstract double Area();
}

public class Circle : Shape {
    public double Radius { get; set; }
    public override double Area() {
        return Math.PI * Radius * Radius;
    }
}
``` 

### Interfaces
Interfaces define a contract that implementing classes must adhere to.

**Example:**
```csharp
public interface IVehicle {
    void Drive();
}

public class Car : IVehicle {
    public void Drive() {
        Console.WriteLine("Car is driving");
    }
}
``` 

### Sealed Classes
A sealed class cannot be inherited. This is used to restrict further derivation of a class.

**Example:**
```csharp
public sealed class FinalClass {
    // Class implementation
}
``` 

### var/dynamic/object
- `var` is implicitly typed variable.
- `dynamic` is a runtime type, and the type is resolved at runtime.
- `object` is the base type from which all types derive.

### Value vs Reference Types
- **Value Types:** Stored directly and include types such as `int`, `char`, `float`.
- **Reference Types:** Store references to their data, include `classes`, `interfaces`, `arrays`.

### ThreadPool
A thread pool helps manage a pool of worker threads for background tasks, avoiding the overhead of creating new threads for each task.

### async/await
Used for asynchronous programming, allowing methods to run concurrently without blocking the main thread.

**Example:**
```csharp
public async Task PerformAsyncTask() {
    await Task.Delay(1000); // Simulate async work
}
``` 

## REST APIs
REST (Representational State Transfer) is an architectural style that uses HTTP requests to access and use data.

## Entity Framework
- **DbContext:** Core class responsible for querying and saving data.
- **Migrations:** Easier management of database schema changes.
- **LINQ:** Language Integrated Query for querying data more elegantly.
- **Lazy/Eager Loading:** Strategies for loading related data.

## Dependency Injection
A design pattern used to implement IoC (Inversion of Control), allowing for better testability and separation of concerns.

## Azure Services
- **Functions:** Serverless compute service.
- **Durable Functions:** For orchestrating complex workflows.
- **Service Bus & Event Hubs:** Messaging services for decoupled application components.
- **Cosmos DB:** Globally distributed database service.

## Microservices
- **Communication Patterns:** Handle interactions between services.
- **gRPC & GraphQL:** Protocols for remote procedure calls and querying data.
- **Circuit Breaker, CQRS, Saga:** Patterns for resilience and data handling.

## JavaScript Topics
- **Variables, Functions, Objects, Arrays:** Basic types used in JS.
- **Callbacks, Promises, async/await:** Patterns for handling asynchronous operations.
- **Closures:** Functions retaining access to their scope.
- **Destructuring:** Unpacking values from arrays or properties from objects.

## SQL Basics
- **SELECT, JOIN, INSERT, UPDATE, DELETE:** Fundamental commands for database manipulation.
- **Indexes:** To enhance data retrieval speed.
- **Stored Procedures:** Prepared SQL code for reuse.
- **Transactions & Views:** Mechanisms for managing group operations and representing data.

---

This document covers vital topics necessary for mastering technology relevant in a 13-year experienced setting, complete with production-ready code examples and detailed definitions.