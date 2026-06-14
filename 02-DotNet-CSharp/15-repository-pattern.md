# Repository Pattern in .NET 9 - Production Guide

## Table of Contents
1. [Introduction](#introduction)
2. [What and Why](#what-and-why)
3. [Repository and Unit of Work](#repository-and-unit-of-work)
4. [Generic vs Specific Repositories](#generic-vs-specific-repositories)
5. ["EF Core Is Already a Repository/UoW" Debate](#ef-core-is-already-a-repositoryuow-debate)
6. [The Specification Pattern](#the-specification-pattern)
7. [Testability](#testability)
8. [Pitfalls and Anti-Patterns](#pitfalls-and-anti-patterns)
9. [Best Practices](#best-practices)
10. [Interview Questions](#interview-questions)

## Introduction

The Repository pattern mediates between the domain and the data-mapping layer, acting like an in-memory collection of domain objects. Clients add, remove, and query objects against the repository without knowing whether the data lives in SQL Server, Cosmos DB, or a JSON file.

The pattern was popularized by Martin Fowler (PoEAA) and Eric Evans (DDD). In modern .NET it is one of the most over-used and most misunderstood patterns — applying it blindly on top of EF Core often adds layers without adding value.

> **Interview tip:** Senior interviewers are rarely asking "can you write a repository?" They are asking "do you know *when not to*?" Lead with the trade-offs, not the boilerplate.

### Key Benefits (When Justified)
- **Persistence ignorance**: The domain and application layers don't reference EF Core, `DbContext`, or `IQueryable`.
- **Testability**: Application services can be tested against an in-memory fake instead of a database.
- **Centralized query logic**: Complex, reused queries live in one place rather than being copy-pasted across handlers.
- **Swappable data store**: In theory you can replace SQL Server with Cosmos DB. In practice this rarely happens, so don't sell the pattern on this alone.

## What and Why

A repository exposes a **collection-like, domain-centric** API. Note that there is no `IQueryable`, no `Include`, and no EF Core types in the signature — that is the whole point.

```csharp
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetPendingForCustomerAsync(Guid customerId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
    void Remove(Order order);
}
```

A common mistake is to design a repository that simply forwards to EF Core. If your repository looks like `_context.Orders.Where(...)` with the EF types leaking out, you have built a thin, leaky wrapper that adds indirection without adding abstraction.

### Why Use It At All?

| Reason | Strength | Notes |
|--------|----------|-------|
| Persistence ignorance for the domain | Strong | Keeps DDD aggregates clean of infrastructure |
| Unit testability without a DB | Strong | But EF Core's in-memory/SQLite providers also help |
| Encapsulating complex queries | Strong | Especially with the Specification pattern |
| Swapping the database engine | Weak | Almost never happens; over-cited justification |
| "Best practice / everyone does it" | None | Cargo-cult reasoning — reject this |

## Repository and Unit of Work

The Repository handles a single aggregate type. The **Unit of Work** coordinates writing changes across multiple repositories within a single transaction and is responsible for committing them atomically.

```csharp
// Application layer abstraction
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// Infrastructure layer - EF Core implements both
public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context) => _context = context;

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<Order>> GetPendingForCustomerAsync(
        Guid customerId, CancellationToken ct = default) =>
        await _context.Orders
            .Where(o => o.CustomerId == customerId && o.Status == OrderStatus.Pending)
            .ToListAsync(ct);

    public async Task AddAsync(Order order, CancellationToken ct = default) =>
        await _context.Orders.AddAsync(order, ct);

    public void Update(Order order) => _context.Orders.Update(order);
    public void Remove(Order order) => _context.Orders.Remove(order);
}
```

Notice that the repository does **not** call `SaveChangesAsync`. Saving is the job of the Unit of Work, so a handler can mutate two aggregates and commit them together.

```csharp
public class TransferStockHandler
{
    private readonly IWarehouseRepository _warehouses;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result> Handle(TransferStockCommand cmd, CancellationToken ct)
    {
        var source = await _warehouses.GetByIdAsync(cmd.SourceId, ct);
        var target = await _warehouses.GetByIdAsync(cmd.TargetId, ct);

        var result = source!.TransferTo(target!, cmd.Sku, cmd.Quantity);
        if (result.IsFailure) return result;

        // One transaction, both aggregates committed atomically
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
```

```csharp
// DI registration - the DbContext IS the unit of work
services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IWarehouseRepository, WarehouseRepository>();
```

> **Interview tip:** A red flag answer is "each repository calls `SaveChanges` after every write." That defeats the Unit of Work, breaks transactional consistency across aggregates, and produces chatty database round-trips.

## Generic vs Specific Repositories

A **generic repository** `IRepository<T>` promises CRUD for any entity. A **specific repository** `IOrderRepository` exposes intent-revealing methods tailored to one aggregate.

```csharp
// Generic - convenient, but encourages a fat, leaky interface
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
```

| Aspect | Generic Repository | Specific Repository |
|--------|-------------------|---------------------|
| Boilerplate | Low (one implementation) | Higher (one per aggregate) |
| Intent / readability | Poor (`GetById` everywhere) | Strong (`GetPendingForCustomer`) |
| Query encapsulation | Weak (callers compose queries) | Strong (queries owned by repo) |
| Risk of leaking `IQueryable` | High | Low |
| DDD aggregate boundaries | Easily violated | Naturally enforced |

The pragmatic answer many senior engineers land on: a **generic base for trivial CRUD plumbing** combined with **specific repositories that add the domain-meaningful queries** and own the aggregate boundary.

```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetPendingForCustomerAsync(Guid customerId, CancellationToken ct = default);
    Task<Order?> GetWithInvoiceAsync(Guid id, CancellationToken ct = default);
}
```

> **Interview tip:** "A pure generic repository over EF Core is widely regarded as an anti-pattern." Be ready to explain why (next two sections). Demonstrating that nuance scores far higher than reciting a generic `IRepository<T>`.

## "EF Core Is Already a Repository/UoW" Debate

This is the single most important talking point for a senior interview.

- `DbSet<T>` already behaves like a **repository** — it is a queryable, addable, removable collection of entities.
- `DbContext` already behaves like a **Unit of Work** — it tracks changes and commits them atomically in `SaveChangesAsync`.

So wrapping EF Core in your own `IRepository<T>`/`IUnitOfWork` can be redundant. **My opinionated position:**

**The pattern adds value when:**
- You practice DDD and want aggregates to be persistence-ignorant. The domain/application layers should never see EF Core types.
- You have **complex, reused queries** worth centralizing (use the Specification pattern).
- You want to enforce **aggregate boundaries** — e.g., you can only load an `Order` with its `Items`, never an orphaned `OrderItem`.
- You genuinely need to swap or mix persistence (e.g., write to SQL, read from a cache or search index).

**The pattern is over-engineering when:**
- It is a thin pass-through that just calls `_context.Set<T>()` and leaks `IQueryable` straight back out.
- The app is mostly CRUD with simple queries. Injecting `DbContext` directly into handlers is simpler and perfectly testable with the SQLite or in-memory provider.
- You add an `IUnitOfWork` that wraps a `DbContext` that is itself already a unit of work, gaining nothing.

```csharp
// Often perfectly fine for CRUD-heavy apps - no repository needed
public class GetProductsHandler(ApplicationDbContext db)
{
    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery q, CancellationToken ct) =>
        await db.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price))
            .ToListAsync(ct);
}
```

> **Interview tip:** If you say "always use a repository" or "never use a repository," you've failed the question. The senior answer is: "It depends on whether you need persistence ignorance and query encapsulation. EF Core already gives you a UoW and a repository-like `DbSet`, so I only add the abstraction when DDD or complex query reuse justifies it."

## The Specification Pattern

The Specification pattern encapsulates query criteria (filtering, includes, ordering, paging) as reusable, composable objects. It is the cleanest way to keep query logic out of handlers while still keeping the repository interface small.

```csharp
public abstract class Specification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public int? Skip { get; protected set; }
    public int? Take { get; protected set; }

    protected void AddInclude(Expression<Func<T, object>> include) => Includes.Add(include);
    protected void ApplyPaging(int skip, int take) { Skip = skip; Take = take; }
}

public class PendingOrdersForCustomerSpec : Specification<Order>
{
    public PendingOrdersForCustomerSpec(Guid customerId, int page, int pageSize)
    {
        Criteria = o => o.CustomerId == customerId && o.Status == OrderStatus.Pending;
        AddInclude(o => o.Items);
        OrderBy = o => o.CreatedAt;
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
```

```csharp
// A single evaluator turns a spec into an IQueryable - kept inside Infrastructure
public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> input, Specification<T> spec) where T : class
    {
        var query = input;
        if (spec.Criteria is not null) query = query.Where(spec.Criteria);
        query = spec.Includes.Aggregate(query, (current, inc) => current.Include(inc));
        if (spec.OrderBy is not null) query = query.OrderBy(spec.OrderBy);
        if (spec.Skip is not null) query = query.Skip(spec.Skip.Value);
        if (spec.Take is not null) query = query.Take(spec.Take.Value);
        return query;
    }
}

public class OrderRepository(ApplicationDbContext context) : IOrderRepository
{
    public async Task<IReadOnlyList<Order>> ListAsync(Specification<Order> spec, CancellationToken ct = default) =>
        await SpecificationEvaluator.GetQuery(context.Orders.AsQueryable(), spec).ToListAsync(ct);
}
```

Now handlers express *intent* (`new PendingOrdersForCustomerSpec(...)`) while the repository interface stays small and the `IQueryable` never leaks. Libraries like **Ardalis.Specification** provide a production-grade version of this.

> **Interview tip:** Mentioning the Specification pattern as the answer to "how do you avoid a leaky generic repository or a method explosion" signals real architectural maturity.

## Testability

The classic argument for repositories is unit testability. With an interface, you fake the repository instead of touching a database.

```csharp
public class FakeOrderRepository : IOrderRepository
{
    private readonly Dictionary<Guid, Order> _store = new();

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_store.GetValueOrDefault(id));

    public Task AddAsync(Order order, CancellationToken ct = default)
    {
        _store[order.Id] = order;
        return Task.CompletedTask;
    }

    public void Update(Order order) => _store[order.Id] = order;
    public void Remove(Order order) => _store.Remove(order.Id);
    public IReadOnlyList<Order> All => _store.Values.ToList();
}
```

But be honest about the trade-off: EF Core ships **SQLite in-memory** and the **in-memory provider**, which let you integration-test against the real `DbContext`. For LINQ-heavy queries, testing against SQLite catches translation bugs that a hand-rolled fake never would.

```csharp
// Integration-style test against a real DbContext over SQLite in-memory
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlite("DataSource=:memory:")
    .Options;

await using var ctx = new ApplicationDbContext(options);
await ctx.Database.OpenConnectionAsync();
await ctx.Database.EnsureCreatedAsync();
// ... arrange, act against real EF query translation, assert
```

> **Interview tip:** "I can mock the repository" is a fine answer, but the stronger one acknowledges that mocking `IQueryable` is brittle, and that SQLite-backed integration tests often give more confidence than a faked repository.

## Pitfalls and Anti-Patterns

### 1. Leaking IQueryable

```csharp
// ANTI-PATTERN: the abstraction leaks; callers now depend on EF Core semantics,
// deferred execution, and the open DbContext lifetime.
public interface IOrderRepository
{
    IQueryable<Order> Query(); // DON'T DO THIS
}
```

If you return `IQueryable<T>`, you have not abstracted persistence at all. Callers build queries that only execute against EF Core, the `DbContext` lifetime becomes ambiguous, and you cannot truly swap the store. Return materialized `IReadOnlyList<T>` or accept a Specification.

### 2. The Generic-Repository-Over-EF Anti-Pattern

A pure `IRepository<T>` on top of EF Core hides EF's most powerful features (projections, `Include`, split queries, compiled queries, bulk operations) behind a lowest-common-denominator interface — then teams add escape hatches that re-leak `IQueryable`, defeating the purpose.

### 3. Repository Method Explosion

Adding `GetByXAndY`, `GetByXOrderedByZ`, `GetByXWithIncludeW` leads to dozens of near-duplicate methods. The Specification pattern is the cure.

### 4. Repositories That Save

Calling `SaveChangesAsync` inside each repository method breaks the Unit of Work, prevents multi-aggregate transactions, and causes excessive round-trips.

### 5. Repositories Crossing Aggregate Boundaries

A repository should map to one **aggregate root** (DDD). Don't expose `OrderItemRepository` — items are loaded and saved through the `Order` aggregate.

## Best Practices

1. Map one repository to one **aggregate root**, not one per table.
2. Keep EF Core types out of the interface — no `IQueryable`, no `DbSet`, no `Include` in signatures.
3. Let the **Unit of Work** (the `DbContext`) own `SaveChanges`; repositories only stage changes.
4. Use the **Specification pattern** to avoid method explosion and `IQueryable` leaks.
5. Skip the repository for simple CRUD/read paths — inject `DbContext` directly or use a CQRS query that projects straight to a DTO.
6. Prefer **integration tests over SQLite** for query correctness; reserve fakes for orchestration logic.

## Interview Questions

### Conceptual Questions

1. **What problem does the Repository pattern solve?**
   - It provides a collection-like, persistence-ignorant abstraction over data access, decoupling domain/application logic from the storage technology and centralizing query logic.

2. **Is the Repository pattern necessary with EF Core?**
   - Not always. `DbSet<T>` is repository-like and `DbContext` is a Unit of Work. Add an explicit repository only when you need persistence ignorance (DDD), query encapsulation, or aggregate-boundary enforcement. For CRUD-heavy apps it is often over-engineering.

3. **What is the Unit of Work and how does it relate to the repository?**
   - The Unit of Work tracks changes across one or more repositories and commits them in a single atomic transaction. In EF Core the `DbContext` already fills this role, so repositories should not call `SaveChanges` themselves.

4. **Generic vs specific repository — which do you prefer?**
   - Specific (or a generic base plus specific extensions). A pure generic repository over EF Core is an anti-pattern: it hides EF's strengths and tempts teams to leak `IQueryable`.

### Technical Questions

5. **Why is returning `IQueryable<T>` from a repository considered bad?**
   ```csharp
   // Leaks EF Core deferred execution + DbContext lifetime to callers
   IQueryable<Order> Query();
   ```
   - It defeats the abstraction, ties callers to EF semantics, makes the `DbContext` lifetime ambiguous, and breaks the promise of swappable persistence.

6. **How do you avoid a repository method explosion?**
   - Use the Specification pattern to encapsulate criteria, includes, ordering, and paging as composable objects, keeping the repository interface small.

7. **How do you test code that uses a repository?**
   ```csharp
   // Option A: fake the interface for orchestration logic
   // Option B: integration test the real repo over SQLite in-memory for query correctness
   ```
   - Use fakes for service orchestration, but prefer SQLite-backed integration tests for anything with real LINQ-to-SQL translation, since mocking `IQueryable` is brittle.

### Scenario Questions

8. **A team wrapped every EF entity in `IRepository<T>` and you see `IQueryable` leaking everywhere. What do you recommend?**
   - Replace the leaky generic repository with either direct `DbContext` access for simple paths or aggregate-specific repositories using the Specification pattern. Stop returning `IQueryable`; return materialized results or accept specs.

9. **You need to update two aggregates atomically. How do you structure it?**
   - Load both via their repositories, mutate them, and call a single `IUnitOfWork.SaveChangesAsync` so EF commits both in one transaction. Never save inside individual repository methods.

10. **When would you deliberately NOT use the Repository pattern?**
    - For CRUD-heavy applications with simple queries, where the repository would be a thin pass-through. Inject the `DbContext` directly (often through a CQRS query handler that projects to a DTO) — it is simpler and still testable.

## See Also

- [Clean Architecture](./11-clean-architecture.md)
- [SOLID Principles](./12-solid-principles.md)
- [Dependency Injection](./13-dependency-injection.md)
- [CQRS Pattern](./16-cqrs-pattern.md)
- [Domain-Driven Design](./17-domain-driven-design.md)

## Additional Resources

- [Martin Fowler: Repository](https://martinfowler.com/eaaCatalog/repository.html)
- [Microsoft: Infrastructure persistence layer design](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Ardalis.Specification](https://github.com/ardalis/Specification)
