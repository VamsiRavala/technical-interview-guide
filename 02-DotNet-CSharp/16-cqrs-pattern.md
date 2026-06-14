# CQRS Pattern in .NET 9 - Production Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Command Query Separation vs CQRS](#command-query-separation-vs-cqrs)
3. [When to Use CQRS (and When Not)](#when-to-use-cqrs-and-when-not)
4. [CQRS Without MediatR](#cqrs-without-mediatr)
5. [CQRS With MediatR](#cqrs-with-mediatr)
6. [The Request/Handler Pipeline](#the-requesthandler-pipeline)
7. [Read vs Write Models](#read-vs-write-models)
8. [CQRS and Event Sourcing](#cqrs-and-event-sourcing)
9. [Eventual Consistency](#eventual-consistency)
10. [Folder Structure](#folder-structure)
11. [Interview Questions](#interview-questions)

## Introduction

CQRS (Command Query Responsibility Segregation) splits an application's behavior into two distinct paths: **commands** that change state and **queries** that read state. Greg Young coined the term, building on Bertrand Meyer's Command-Query Separation (CQS).

The core insight: reads and writes have very different requirements. Writes must enforce invariants and consistency; reads must be fast and shaped for the UI. Treating them identically (one model for both) forces compromises on both sides.

> **Interview tip:** The most common misconception is that "CQRS requires two databases / event sourcing / microservices." It does not. The minimal, valuable form of CQRS is simply *separating the command and query models in code*. Lead with that and you'll stand out from candidates who conflate CQRS with its heavyweight cousins.

### Key Benefits
- **Independent optimization**: Read side can use denormalized projections or Dapper; write side enforces domain rules.
- **Scalability**: Read and write workloads can scale independently (read replicas, caches).
- **Clarity**: Each handler does exactly one thing, with one reason to change (SRP).
- **Security and validation**: Cross-cutting concerns attach cleanly to a uniform request pipeline.

## Command Query Separation vs CQRS

These are related but distinct.

| Aspect | CQS (method level) | CQRS (architecture level) |
|--------|--------------------|---------------------------|
| Scope | A single method | The whole request/response flow |
| Rule | A method either changes state OR returns data, never both | Commands and queries use separate models, handlers, and often separate stores |
| Example | `void SetName(...)` vs `string GetName()` | `CreateOrderCommand` handler vs `GetOrderQuery` handler |

CQRS is CQS applied at the message/handler granularity. A **command** expresses intent to change state and ideally returns only an ack (or an id). A **query** returns data and must have no side effects.

```csharp
public record CreateOrderCommand(Guid CustomerId, List<OrderLineDto> Lines) : IRequest<Guid>;
public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDetailsDto?>;
```

## When to Use CQRS (and When Not)

CQRS is a tool with real costs (more types, more indirection). Apply it deliberately.

**Use CQRS when:**
- Read and write models genuinely diverge (the UI needs a flat projection but the domain is a rich aggregate).
- Read and write loads are very different and need independent scaling.
- You have complex business rules on writes that you want isolated from reporting/query code.
- You are already doing DDD with aggregates — CQRS pairs naturally with it.
- You want a uniform pipeline for validation, logging, transactions, and authorization.

**Avoid CQRS when:**
- The application is simple CRUD with a single model that serves both reads and writes well.
- The team is small/junior and the indirection would hurt more than help.
- You'd be adding two databases or eventual consistency "because CQRS" without a real driver — that is over-engineering.

> **Interview tip:** A great answer names a concrete decision boundary: "I reach for CQRS when the read shape and write shape diverge or when I want a behavior pipeline. For a basic admin CRUD screen I would not — a single service and EF Core is simpler."

## CQRS Without MediatR

You do **not** need a library. CQRS is an architectural idea; MediatR is just one way to dispatch.

```csharp
// Define the contracts yourself
public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken ct);
}

public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken ct);
}

public record CreateOrderCommand(Guid CustomerId, List<OrderLineDto> Lines);

public class CreateOrderCommandHandler(IOrderRepository orders, IUnitOfWork uow)
    : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(cmd.CustomerId, cmd.Lines);
        await orders.AddAsync(order, ct);
        await uow.SaveChangesAsync(ct);
        return order.Id;
    }
}
```

```csharp
// Register all handlers by convention with the built-in DI container
services.Scan(scan => scan
    .FromAssemblyOf<CreateOrderCommandHandler>()
    .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
        .AsImplementedInterfaces().WithScopedLifetime()
    .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
        .AsImplementedInterfaces().WithScopedLifetime());
```

Hand-rolling the interfaces avoids a third-party dependency and keeps dispatch explicit. The trade-off is that you implement your own pipeline (validation, logging) instead of getting it for free.

> **Interview tip:** Note that since MediatR moved to a commercial license for larger orgs (v13+), "can you do CQRS without MediatR?" has become a real interview question. The answer is yes — two interfaces and assembly scanning.

## CQRS With MediatR

MediatR provides the in-process mediator, request/handler contracts, and — crucially — a **behavior pipeline** out of the box.

```csharp
// Application layer
public record CreateOrderCommand(Guid CustomerId, List<OrderLineDto> Lines) : IRequest<Result<Guid>>;

public class CreateOrderCommandHandler(IOrderRepository orders, IUnitOfWork uow)
    : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = Order.Create(request.CustomerId, request.Lines);
        await orders.AddAsync(order, ct);
        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(order.Id);
    }
}

public record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<OrderDetailsDto>>;

public class GetOrderByIdQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailsDto>>
{
    public async Task<Result<OrderDetailsDto>> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var dto = await db.Orders
            .Where(o => o.Id == request.OrderId)
            .Select(o => new OrderDetailsDto(o.Id, o.CustomerId, o.Status.ToString(), o.Total))
            .FirstOrDefaultAsync(ct);

        return dto is null
            ? Result<OrderDetailsDto>.Failure("Order not found")
            : Result<OrderDetailsDto>.Success(dto);
    }
}
```

```csharp
// Thin controller - just translates HTTP to messages
[ApiController]
[Route("api/orders")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
                                 : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetOrderByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
```

Note the asymmetry: the **command** goes through the repository/aggregate (write model), while the **query** projects straight to a DTO from `DbContext` (read model). That asymmetry is CQRS in action.

## The Request/Handler Pipeline

The behavior (or middleware) pipeline is where CQRS pays off operationally: cross-cutting concerns wrap every request without polluting handlers.

```csharp
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct))))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }
        return await next();
    }
}

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();
        logger.LogInformation("Handling {Request}", name);
        var response = await next();
        logger.LogInformation("Handled {Request} in {Elapsed}ms", name, sw.ElapsedMilliseconds);
        return response;
    }
}
```

```csharp
// Registration order matters: behaviors run in the order added (outermost first)
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommandHandler).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));       // outermost
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));   // innermost - wraps the handler in a transaction
});
```

```csharp
// FluentValidation validator picked up automatically by the pipeline
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("An order needs at least one line");
        RuleForEach(x => x.Lines).ChildRules(l => l.RuleFor(x => x.Quantity).GreaterThan(0));
    }
}
```

> **Interview tip:** Be able to explain pipeline ordering. Validation typically wraps the transaction so invalid requests never open a transaction, and logging/metrics usually sit outermost so they record the full duration including validation.

## Read vs Write Models

This is the heart of CQRS.

| Concern | Write Model (commands) | Read Model (queries) |
|---------|------------------------|----------------------|
| Goal | Enforce invariants, consistency | Fast, UI-shaped reads |
| Shape | Rich DDD aggregates | Flat DTOs / projections |
| Access | Repository + aggregate, tracked | Direct `DbContext` projection, `AsNoTracking`, or Dapper |
| Normalization | Normalized | Often denormalized |
| Validation | Heavy (business rules) | Minimal (input only) |

```csharp
// WRITE: load the aggregate, mutate through behavior, persist via UoW
public class ConfirmOrderCommandHandler(IOrderRepository orders, IUnitOfWork uow)
    : IRequestHandler<ConfirmOrderCommand, Result>
{
    public async Task<Result> Handle(ConfirmOrderCommand request, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(request.OrderId, ct);
        if (order is null) return Result.Failure("Order not found");

        var result = order.Confirm();        // domain invariant lives in the aggregate
        if (result.IsFailure) return result;

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// READ: no aggregate, no tracking - project straight to the DTO the screen needs
public class GetCustomerOrderSummariesHandler(ApplicationDbContext db)
    : IRequestHandler<GetCustomerOrderSummariesQuery, IReadOnlyList<OrderSummaryDto>>
{
    public async Task<IReadOnlyList<OrderSummaryDto>> Handle(GetCustomerOrderSummariesQuery q, CancellationToken ct) =>
        await db.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == q.CustomerId)
            .Select(o => new OrderSummaryDto(o.Id, o.Status.ToString(), o.Total, o.Items.Count))
            .ToListAsync(ct);
}
```

The two models can share one database (the common, pragmatic case) or live in separate stores (the advanced case, where a denormalized read store is updated from write-side events).

## CQRS and Event Sourcing

These are **separable** — a critical point.

- **CQRS** = separate the read and write paths. Says nothing about how you store state.
- **Event Sourcing** = persist state as an append-only log of events; current state is a fold over events.

You can do CQRS with a plain relational database and no events at all (the majority of CQRS systems). You *can* combine them — event sourcing makes a natural write model, with projections building read models — but neither requires the other.

```csharp
// Event-sourced write side (advanced, optional)
public class Order
{
    private readonly List<IDomainEvent> _changes = new();

    public static Order Create(Guid customerId, IReadOnlyList<OrderLineDto> lines)
    {
        var order = new Order();
        order.Apply(new OrderCreated(Guid.NewGuid(), customerId, lines));  // record the fact
        return order;
    }

    private void Apply(IDomainEvent e)
    {
        // mutate state from the event...
        _changes.Add(e);  // append to the stream on save
    }
}
```

> **Interview tip:** If an interviewer says "CQRS and Event Sourcing are the same thing," gently correct them. They are frequently used together but are independent. Conflating them is one of the most common architectural mistakes, and recognizing the distinction signals senior-level understanding.

## Eventual Consistency

CQRS introduces eventual consistency only when the read and write models are stored separately and updated asynchronously (e.g., the write side publishes an event, a projector updates a denormalized read store).

```csharp
// Write side publishes after commit; a background projector updates the read model later
public class OrderConfirmedProjector(IReadModelStore store) : INotificationHandler<OrderConfirmed>
{
    public async Task Handle(OrderConfirmed notification, CancellationToken ct)
    {
        // The read model becomes consistent shortly after the write commits - not instantly
        await store.MarkOrderConfirmedAsync(notification.OrderId, ct);
    }
}
```

Implications you must design for:
- The UI may briefly show stale data after a write. Mitigate with optimistic UI updates or by reading back from the write model immediately after a command.
- Projectors must be **idempotent** and able to recover/replay.
- If a single relational database backs both models (no async projection), reads are **strongly consistent** — eventual consistency is a choice you opt into, not an inherent cost of CQRS.

> **Interview tip:** Make clear that "CQRS = eventual consistency" is false. Eventual consistency only appears when you physically separate the stores. The simplest CQRS — separate models, one database — is fully consistent.

## Folder Structure

Organize by feature (vertical slices), not by technical layer.

```
src/MyApp.Application/
├── Orders/
│   ├── Commands/
│   │   ├── CreateOrder/
│   │   │   ├── CreateOrderCommand.cs
│   │   │   ├── CreateOrderCommandHandler.cs
│   │   │   └── CreateOrderCommandValidator.cs
│   │   └── ConfirmOrder/
│   │       ├── ConfirmOrderCommand.cs
│   │       └── ConfirmOrderCommandHandler.cs
│   ├── Queries/
│   │   ├── GetOrderById/
│   │   │   ├── GetOrderByIdQuery.cs
│   │   │   ├── GetOrderByIdQueryHandler.cs
│   │   │   └── OrderDetailsDto.cs
│   │   └── GetCustomerOrderSummaries/
│   │       ├── GetCustomerOrderSummariesQuery.cs
│   │       └── OrderSummaryDto.cs
│   └── EventHandlers/
│       └── OrderConfirmedProjector.cs
├── Common/
│   └── Behaviors/
│       ├── ValidationBehavior.cs
│       ├── LoggingBehavior.cs
│       └── TransactionBehavior.cs
└── DependencyInjection.cs
```

Keeping the command, its handler, and its validator in one folder makes a feature easy to find and to delete — a hallmark of the vertical-slice style that pairs naturally with CQRS.

## Interview Questions

### Conceptual Questions

1. **What is CQRS and what problem does it solve?**
   - It separates state-changing commands from data-returning queries so each path can be modeled, validated, and optimized independently, avoiding the compromises of a single shared model.

2. **What's the difference between CQS and CQRS?**
   - CQS is a method-level principle (a method either mutates or returns, not both). CQRS applies that idea at the architecture level, with separate models and handlers for commands and queries.

3. **Does CQRS require two databases or event sourcing?**
   - No. The minimal valuable form is separate models in code over one database. Separate stores, event sourcing, and eventual consistency are optional advanced variants.

4. **When would you NOT use CQRS?**
   - For simple CRUD where one model serves reads and writes well, or where the team is small and the extra indirection costs more than it returns.

### Technical Questions

5. **How do you implement CQRS without MediatR?**
   ```csharp
   public interface ICommandHandler<in TCommand, TResult> { Task<TResult> Handle(TCommand c, CancellationToken ct); }
   public interface IQueryHandler<in TQuery, TResult> { Task<TResult> Handle(TQuery q, CancellationToken ct); }
   ```
   - Define your own command/query handler interfaces, register them via assembly scanning, and dispatch through a thin custom mediator or directly. This avoids the MediatR dependency/licensing.

6. **What is the MediatR behavior pipeline and why is it useful?**
   - It is a chain of `IPipelineBehavior<,>` wrapping every handler, enabling cross-cutting concerns (validation, logging, transactions, authorization) in one place. Ordering matters — validation usually wraps the transaction; logging is outermost.

7. **How do read and write models differ in CQRS?**
   - Write models are rich aggregates that enforce invariants, accessed through repositories with change tracking. Read models are flat DTO projections, typically `AsNoTracking` or via Dapper, shaped for the UI.

### Scenario Questions

8. **A junior says "we must use event sourcing because we're doing CQRS." How do you respond?**
   - They're separable. CQRS only mandates separating read/write paths. Event sourcing is one possible write-model implementation; most CQRS systems use a plain relational store. Only adopt event sourcing if audit/temporal requirements justify it.

9. **After confirming an order the user refreshes and sees the old status. Why, and how do you fix it?**
   - This is eventual consistency from an async projection updating a separate read store. Fix by reading back from the write model right after the command, applying an optimistic UI update, or — if acceptable — collapsing to a single consistent database.

10. **How do you structure a CQRS application's folders?**
    - By feature/vertical slice: each command or query gets a folder containing its message, handler, and validator/DTO, with shared pipeline behaviors in a common folder. This keeps features cohesive and easy to remove.

## See Also

- [Clean Architecture](./11-clean-architecture.md)
- [SOLID Principles](./12-solid-principles.md)
- [Dependency Injection](./13-dependency-injection.md)
- [Repository Pattern](./15-repository-pattern.md)
- [Domain-Driven Design](./17-domain-driven-design.md)

## Additional Resources

- [Martin Fowler: CQRS](https://martinfowler.com/bliki/CQRS.html)
- [Microsoft: Apply simplified CQRS and DDD patterns](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/apply-simplified-microservice-cqrs-ddd-patterns)
- [Greg Young: CQRS Documents](https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf)
