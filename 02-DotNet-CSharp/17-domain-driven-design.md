# Domain-Driven Design (DDD) in .NET 9 - Production Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Strategic Design](#strategic-design)
3. [Tactical Design](#tactical-design)
4. [Aggregate Design Rules](#aggregate-design-rules)
5. [Anemic vs Rich Domain Model](#anemic-vs-rich-domain-model)
6. [DDD and Clean Architecture](#ddd-and-clean-architecture)
7. [When DDD Is Overkill](#when-ddd-is-overkill)
8. [Interview Questions](#interview-questions)

## Introduction

Domain-Driven Design, introduced by Eric Evans in his 2003 book, is an approach to building software where the **domain** — the business problem and its rules — drives the design. It puts the model and the language of domain experts at the center, rather than the database or the framework.

DDD has two halves that are often conflated:
- **Strategic design** — how you carve a large domain into bounded contexts and how those contexts relate. This is the high-value, often-skipped part.
- **Tactical design** — the building blocks (entities, value objects, aggregates, domain events) used to model a single context. This is the part developers reach for first.

> **Interview tip:** Many candidates only know the tactical patterns (entity, value object, aggregate). Senior interviewers probe strategic design — bounded contexts and ubiquitous language — because that's where DDD actually prevents big-system failures. Lead with strategy to stand out.

### When DDD Pays Off
- Complex, rules-heavy domains (finance, insurance, logistics, healthcare).
- Long-lived systems where the model evolves with the business.
- Teams that include or can access **domain experts**.

## Strategic Design

### Ubiquitous Language

A shared, rigorous vocabulary used identically by developers and domain experts, in conversation, documentation, and code. If the business says "policy" and "premium," the code says `Policy` and `Premium` — not `InsuranceRecord` and `Amount`.

```csharp
// The class and method names ARE the ubiquitous language.
public class Policy
{
    public Premium CalculatePremium(RiskProfile profile) { /* ... */ }
    public void Lapse(LapseReason reason) { /* ... */ }   // "lapse" is the domain expert's word
}
```

When the language drifts (developers say "soft-delete a user" while the business says "deactivate an account"), bugs and miscommunication follow. The ubiquitous language is bounded — it is only consistent *within* a bounded context.

### Bounded Contexts

A bounded context is an explicit boundary within which a particular model and language apply. The same word means different things in different contexts, and that's fine — each context owns its own model.

```
E-Commerce System
├── Catalog Context        →  "Product" = name, description, images, price
├── Ordering Context       →  "Product" = a line item id + price snapshot
├── Inventory Context      →  "Product" = SKU + stock levels + warehouse location
└── Shipping Context       →  "Product" = weight + dimensions + fragility
```

Trying to build one universal `Product` class that serves all four contexts produces a bloated, contradictory model. DDD says: let each context have its own `Product`, and integrate via well-defined contracts.

In .NET, bounded contexts typically map to separate projects/solutions (or microservices), each with its own `DbContext`.

```csharp
// Each bounded context has its own DbContext and its own model of "Product"
public class CatalogDbContext : DbContext { public DbSet<Catalog.Product> Products => Set<Catalog.Product>(); }
public class InventoryDbContext : DbContext { public DbSet<Inventory.Product> Products => Set<Inventory.Product>(); }
```

### Context Mapping

Context maps describe the relationships and integration patterns between bounded contexts.

| Pattern | Meaning |
|---------|---------|
| Partnership | Two teams succeed or fail together; coordinate closely |
| Shared Kernel | A small shared model both contexts depend on (use sparingly) |
| Customer/Supplier | Downstream context's needs influence the upstream's roadmap |
| Conformist | Downstream simply accepts the upstream model as-is |
| Anti-Corruption Layer (ACL) | Downstream translates the upstream model into its own, protecting its language |
| Open Host Service / Published Language | Upstream publishes a well-documented contract (e.g., a REST API or events) |

```csharp
// Anti-Corruption Layer: translate an external/legacy model into our context's language
public class LegacyCrmAntiCorruptionLayer(ILegacyCrmClient legacy) : ICustomerProvider
{
    public async Task<Customer> GetCustomerAsync(Guid id, CancellationToken ct)
    {
        var record = await legacy.FetchAccountAsync(id, ct);   // legacy "Account" model
        // Translate into OUR ubiquitous language - the legacy schema never leaks inward
        return Customer.Rehydrate(
            id: record.AcctGuid,
            name: CustomerName.Create(record.Fname, record.Lname),
            tier: MapTier(record.LoyaltyCode));
    }
}
```

> **Interview tip:** The Anti-Corruption Layer is a favorite interview topic, especially in legacy-modernization scenarios. It is how you protect a clean new model from a messy upstream/legacy one.

## Tactical Design

### Value Objects

Immutable objects defined entirely by their attributes, with no identity. Two value objects with the same values are equal. They are the workhorse of a rich model — prefer them over primitives ("primitive obsession").

```csharp
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency) => (Amount, Currency) = (amount, currency);

    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0) return Result<Money>.Failure("Amount cannot be negative");
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            return Result<Money>.Failure("Currency must be an ISO 4217 code");
        return Result<Money>.Success(new Money(amount, currency));
    }

    public Money Add(Money other)
    {
        if (other.Currency != Currency)
            throw new InvalidOperationException("Cannot add money of different currencies");
        return new Money(Amount + other.Amount, Currency);
    }
}
```

C# `record` types are ideal for value objects because they give structural equality for free. EF Core 9 maps them well with **complex types** or owned types:

```csharp
// EF Core 9: map a value object as a complex type (no separate identity/table)
modelBuilder.Entity<Order>()
    .ComplexProperty(o => o.Total, b =>
    {
        b.Property(m => m.Amount).HasPrecision(18, 2);
        b.Property(m => m.Currency).HasMaxLength(3);
    });
```

### Entities

Objects with a distinct **identity** that persists through state changes. A `Customer` whose name and address change is still the same customer because its id is unchanged.

```csharp
public abstract class Entity
{
    public Guid Id { get; protected set; }

    public override bool Equals(object? obj) =>
        obj is Entity other && GetType() == other.GetType() && Id == other.Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
```

### Aggregates and Aggregate Roots

An **aggregate** is a cluster of entities and value objects treated as a single unit for data changes. One entity is the **aggregate root** — the only member outside objects may hold a reference to. All access and invariants funnel through the root.

```csharp
public class Order : AggregateRoot   // the aggregate root
{
    private readonly List<OrderLine> _lines = new();   // internal entities

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money Total => _lines.Aggregate(Money.Zero("USD"), (sum, l) => sum.Add(l.LineTotal));
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();   // expose read-only

    private Order() { }   // EF Core

    public static Order Create(Guid customerId)
    {
        var order = new Order { Id = Guid.NewGuid(), CustomerId = customerId, Status = OrderStatus.Draft };
        order.AddDomainEvent(new OrderCreated(order.Id, customerId));
        return order;
    }

    // Invariants are enforced ONLY through the root - never mutate a line directly
    public Result AddLine(Guid productId, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Draft)
            return Result.Failure("Lines can only be added to a draft order");
        if (quantity <= 0)
            return Result.Failure("Quantity must be positive");

        _lines.Add(new OrderLine(productId, unitPrice, quantity));
        return Result.Success();
    }

    public Result Submit()
    {
        if (_lines.Count == 0) return Result.Failure("Cannot submit an empty order");
        Status = OrderStatus.Submitted;
        AddDomainEvent(new OrderSubmitted(Id, CustomerId, Total));
        return Result.Success();
    }
}
```

### Domain Events

Something meaningful that happened in the domain, named in the past tense. They decouple side effects (send email, update read model, reserve stock) from the core state transition.

```csharp
public interface IDomainEvent { DateTime OccurredOn { get; } }

public record OrderSubmitted(Guid OrderId, Guid CustomerId, Money Total) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Dispatched from the DbContext after a successful SaveChanges (see Clean Architecture guide)
public class OrderSubmittedHandler(IInventoryService inventory, IEmailService email)
    : INotificationHandler<OrderSubmitted>
{
    public async Task Handle(OrderSubmitted e, CancellationToken ct)
    {
        await inventory.ReserveAsync(e.OrderId, ct);
        await email.SendOrderConfirmationAsync(e.CustomerId, e.OrderId, ct);
    }
}
```

### Repositories (DDD View)

In DDD, a repository provides collection-like access to **aggregate roots only** — one repository per aggregate, never per entity inside an aggregate.

```csharp
// One repository per aggregate root; OrderLine has no repository of its own.
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);   // loads the whole aggregate
    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
}
```

(See the [Repository Pattern guide](./15-repository-pattern.md) for the full treatment, including the Unit of Work and when the abstraction is worth it.)

### Domain Services

When an operation doesn't naturally belong to a single entity or value object — typically because it spans multiple aggregates — model it as a **domain service**: a stateless object expressed in the ubiquitous language, living in the domain layer.

```csharp
// Pricing logic that needs a customer's tier AND a product's rules belongs in neither aggregate alone.
public class PricingService
{
    public Money CalculatePrice(Product product, Customer customer)
    {
        var basePrice = product.ListPrice;
        var discount = customer.Tier switch
        {
            CustomerTier.Gold => 0.15m,
            CustomerTier.Silver => 0.08m,
            _ => 0m
        };
        return basePrice.ApplyDiscount(discount);
    }
}
```

Do not confuse a **domain service** (pure business logic, in the domain layer) with an **application service** (orchestration, transactions, calls infrastructure — lives in the application layer).

## Aggregate Design Rules

These rules from Vaughn Vernon's *Implementing Domain-Driven Design* are frequent interview material.

1. **Protect invariants inside one aggregate boundary.** An aggregate is a consistency boundary. Anything that must be transactionally consistent belongs together.
2. **Design small aggregates.** Large aggregates cause contention and slow loads. Favor many small aggregates over a few sprawling ones.
3. **Reference other aggregates by identity, not by object reference.** Store `CustomerId`, not a `Customer` object, to keep boundaries clean and aggregates loadable independently.
4. **Update one aggregate per transaction.** Changes to other aggregates happen in separate transactions, coordinated via domain events and **eventual consistency**.

```csharp
// RULE 3: reference by id, not by navigation, across aggregate boundaries
public class Order : AggregateRoot
{
    public Guid CustomerId { get; private set; }   // GOOD - reference by identity
    // public Customer Customer { get; private set; }  // AVOID - couples two aggregates
}
```

> **Interview tip:** "How big should an aggregate be?" is a classic. The answer: as small as possible while still enclosing the invariants that must be transactionally consistent. If two pieces of data don't need to change together atomically, they probably belong in different aggregates.

## Anemic vs Rich Domain Model

This is one of the most-asked DDD interview topics.

An **anemic domain model** has entities that are bags of public getters/setters with no behavior; all logic lives in services. Martin Fowler calls it an anti-pattern because it discards the core benefit of OOP — co-locating data with the rules that govern it.

```csharp
// ANEMIC - just a data bag; rules live elsewhere and can be bypassed
public class Order
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }   // anyone can set any status, any time
    public List<OrderLine> Lines { get; set; } = new();
}

public class OrderService   // all the logic, all the temptation to skip a check
{
    public void Submit(Order order)
    {
        order.Status = OrderStatus.Submitted;   // no invariant enforced
    }
}
```

```csharp
// RICH - the aggregate owns its rules; invalid states are unrepresentable
public class Order : AggregateRoot
{
    public OrderStatus Status { get; private set; }   // private setter
    private readonly List<OrderLine> _lines = new();

    public Result Submit()
    {
        if (_lines.Count == 0) return Result.Failure("Cannot submit an empty order");
        if (Status != OrderStatus.Draft) return Result.Failure("Only draft orders can be submitted");
        Status = OrderStatus.Submitted;
        return Result.Success();
    }
}
```

| Aspect | Anemic Model | Rich Model |
|--------|--------------|------------|
| Where business logic lives | Services | Entities / aggregates |
| Invariant enforcement | Easily bypassed | Guaranteed by the aggregate |
| Encapsulation | Weak (public setters) | Strong (private setters, factories) |
| Fit for DDD | Anti-pattern | The DDD ideal |
| Fit for simple CRUD | Often acceptable | May be over-engineering |

> **Interview tip:** Don't dogmatically condemn the anemic model. For a thin CRUD app it can be perfectly fine. The point is that *DDD* requires a rich model — the value of DDD evaporates if entities are anemic, because there's no domain behavior to protect.

## DDD and Clean Architecture

DDD and Clean Architecture are complementary, not competing.

- **Clean Architecture** provides the layering and dependency rule (dependencies point inward).
- **DDD** provides what fills the inner layers — the domain model.

```
Domain Layer        →  DDD tactical patterns: entities, value objects, aggregates,
                       domain events, domain services. No framework/EF dependency.
Application Layer   →  Application services / CQRS handlers orchestrating aggregates,
                       transactions (Unit of Work), and infrastructure abstractions.
Infrastructure      →  EF Core DbContext, repository implementations, message bus.
Presentation        →  API/UI translating requests into application use cases.
```

The Dependency Rule keeps the DDD domain pure: aggregates know nothing about EF Core or HTTP, so the ubiquitous language stays uncontaminated by infrastructure concerns. CQRS (see [the CQRS guide](./16-cqrs-pattern.md)) often sits in the application layer, with commands driving the rich write model and queries bypassing aggregates to project read models.

## When DDD Is Overkill

DDD has real costs: more types, more ceremony, and a learning curve. It is the wrong default for many projects.

**DDD is overkill when:**
- The application is essentially CRUD with little business logic (admin tools, simple catalogs, form-over-data apps).
- There is no access to domain experts and the rules are trivial or well-understood.
- The team is small/junior and time-to-market dominates; the tactical ceremony will slow you down without payback.
- The domain is generic/solved (authentication, basic reporting) — buy or use a library instead of modeling it.

**A pragmatic middle ground:** apply DDD's *strategic* ideas (bounded contexts, ubiquitous language) broadly — they're cheap and valuable — while reserving the heavy *tactical* patterns (rich aggregates, domain events) for the genuinely complex contexts (the "core domain"). You don't need full aggregates in a supporting subdomain that's just CRUD.

> **Interview tip:** A senior answer never says "always do DDD." It says: "I apply DDD to the **core domain** where complexity and competitive value live, and keep supporting/generic subdomains simple. Strategic DDD is almost always worth it; tactical DDD is worth it where the rules are rich."

## Interview Questions

### Conceptual Questions

1. **What is Domain-Driven Design?**
   - An approach where the business domain and its rules drive software design, centered on a ubiquitous language shared by developers and domain experts, split into strategic (bounded contexts, context mapping) and tactical (entities, value objects, aggregates, events) design.

2. **What is a bounded context and why does it matter?**
   - An explicit boundary within which one model and language are consistent. It matters because the same term (e.g., "Product") means different things to different parts of the business; bounded contexts let each have its own model instead of forcing one bloated universal model.

3. **Explain the difference between an entity and a value object.**
   - An entity has identity that persists through change (a `Customer` with id). A value object has no identity and is equal by its attributes (a `Money` of 5 USD). Prefer value objects to combat primitive obsession.

4. **What is an aggregate and an aggregate root?**
   - An aggregate is a cluster of objects treated as one consistency unit; the aggregate root is its single entry point. Outside objects reference only the root, and all invariants are enforced through it.

### Technical Questions

5. **What are the rules for designing aggregates?**
   - Keep them small; enforce invariants within one boundary; reference other aggregates by identity, not object reference; and update only one aggregate per transaction, using domain events for cross-aggregate consistency.

6. **What's the difference between a domain service and an application service?**
   - A domain service holds business logic that spans aggregates and lives in the domain layer (pure, no infrastructure). An application service orchestrates use cases — transactions, calling repositories and infrastructure — and lives in the application layer.

7. **How do you map a value object in EF Core 9?**
   ```csharp
   modelBuilder.Entity<Order>().ComplexProperty(o => o.Total, b => { /* ... */ });
   ```
   - Use complex types (EF Core 8+) or owned types so the value object has no identity/table of its own. `record` types give the structural equality value objects require.

### Scenario Questions

8. **What is the anemic domain model and is it always wrong?**
   - It's an entity with only getters/setters and no behavior, with logic in services — an anti-pattern *for DDD* because invariants can be bypassed. It's not always wrong: for thin CRUD apps it's acceptable. But it defeats the purpose of DDD.

9. **A legacy system exposes a messy model you must integrate with. How do you protect your new domain?**
   - Introduce an Anti-Corruption Layer that translates the legacy model into your ubiquitous language at the boundary, so the legacy schema never leaks into your domain.

10. **When would you decide NOT to use DDD?**
    - For CRUD-heavy or generic subdomains, with trivial rules, no domain-expert access, or tight deadlines and a junior team. Apply strategic DDD broadly but reserve rich tactical modeling for the core domain where complexity and business value concentrate.

## See Also

- [Clean Architecture](./11-clean-architecture.md)
- [SOLID Principles](./12-solid-principles.md)
- [Dependency Injection](./13-dependency-injection.md)
- [Repository Pattern](./15-repository-pattern.md)
- [CQRS Pattern](./16-cqrs-pattern.md)

## Additional Resources

- [Eric Evans: Domain-Driven Design (the "Blue Book")](https://www.domainlanguage.com/ddd/)
- [Vaughn Vernon: Implementing Domain-Driven Design](https://kalele.io/books/)
- [Microsoft: Design a DDD-oriented microservice](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/ddd-oriented-microservice)
- [Martin Fowler: Anemic Domain Model](https://martinfowler.com/bliki/AnemicDomainModel.html)
