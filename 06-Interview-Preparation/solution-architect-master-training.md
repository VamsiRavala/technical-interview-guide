> Reference document — Solution Architect interview track.

# Solution Architect Master Training Document

*Architect-level reference covering Architecture, Patterns, System Design, Microservices, Azure, React, Security & DevOps*

**Prepared for: Vamsi Raavala**

Companion to the 8-Week Daily Practice Plan

## How to Use This Document

This document is your reference for the 8-week prep plan. Each section maps to specific days in the practice plan. Use it three ways:

- **As a daily reader:** Read the relevant section during the 1-hour study block.
- **As an interview cheat sheet:** Skim the 'Interview Tip' callouts before any architect interview.
- **As a teaching script:** If you can read a section aloud and explain every concept in your own words, you've internalized it.

Conventions used throughout:

- **Architect Insight (blue) —** the perspective shift senior interviewers want to hear.
- **Trade-off (red/orange) —** the honest cost of the choice. Always have one ready.
- **Interview Tip (yellow) —** exactly how to phrase or structure an answer.
- **Watch Out —** common production gotchas worth memorizing.

## 1. Architecture Foundations

### 1.1 The Architect Mindset

A developer optimizes for code quality. A lead optimizes for team output. An architect optimizes for system fitness over time across functional and non-functional dimensions, business constraints, and technical debt curves.

Solution Architect role definition (the version that wins interviews):

- Owns end-to-end design of one or more systems for a specific business domain.
- Translates business goals into technology decisions, with trade-offs articulated.
- Defines non-functional requirements (NFRs) — scale, latency, availability, security, cost — and ensures the design meets them.
- Picks technologies and patterns; writes and maintains Architecture Decision Records (ADRs).
- Influences across teams; reviews other architects' designs; mentors senior engineers.
- Carries accountability for production outcomes even when not coding daily.

> **Architect Insight:** In every interview, frame answers around trade-offs, not features. 'We picked X' is junior. 'We picked X over Y because the cost of Y's eventual consistency exceeded the latency win for our hot path' is architect.

#### Functional vs Non-Functional Requirements (FR vs NFR)

FRs are what the system does. NFRs are how well it does it. NFRs win or lose architect interviews.

| NFR Category | Example metric you should be ready to quote |
| --- | --- |
| **Performance** | p99 latency under 200ms; throughput \>= 10K req/sec. |
| **Scalability** | Horizontal scale to 100K concurrent users; data growth 1TB/month. |
| **Availability** | 99.95% uptime; RTO under 15 min; RPO under 5 min. |
| **Reliability** | MTTR under 30 min; error rate under 0.1%. |
| **Security** | Data at rest + in transit encrypted; SOC2; OWASP Top 10 hardened. |
| **Maintainability** | New feature lead time under 2 weeks; deploy frequency daily. |
| **Cost** | Cloud bill within target $X/month; cost per transaction \< $Y. |
| **Observability** | Distributed tracing for 100% of requests; alert on SLO burn rate. |

> **Interview Tip:** When a system design question begins, your FIRST move is to clarify FRs and NFRs. Skipping this is the #1 reason senior engineers fail architect interviews. Even if the interviewer says 'just go,' say 'one minute, let me capture requirements before designing.'

### 1.2 SOLID — Single Responsibility & Open/Closed

#### Single Responsibility Principle (SRP)

A class should have one, and only one, reason to change. The classic mistake is interpreting 'responsibility' as 'thing it does.' The correct interpretation is 'reason it changes' — i.e. the actor or stakeholder driving change.

Smell: a class is modified by two unrelated tickets in the same sprint. Different reasons to change → split.

Real-world (your Kroger Sales Promotion): a PromotionService that calculated discounts AND emailed marketing AND wrote audit logs has three actors: pricing team, marketing team, compliance. Split into PromotionCalculator, MarketingNotifier, AuditLogger.

#### Open/Closed Principle (OCP)

Software entities should be open for extension, closed for modification. New behavior should arrive by adding new code, not editing tested code.

Mechanism: polymorphism + dependency injection. Define a stable interface (closed). New strategies plug in as new classes (open).

Example: a discount engine that started with a hardcoded if/else chain (Christmas, BOGO, ClearanceFlat) becomes a Strategy pattern. Each rule implements IDiscountStrategy, registered in DI. New rules ship without touching the engine.

> **Trade-off:** OCP overuse leads to abstraction soup. Don't abstract until you have at least 2-3 concrete cases. Premature interfaces are as bad as none.

### 1.3 SOLID — Liskov, Interface Segregation, Dependency Inversion

#### Liskov Substitution (LSP)

Subtypes must be substitutable for their base types without breaking callers. Violations show up as 'is-a' relationships that aren't truly behavioral 'is-a's.

Classic violation: Square inherits from Rectangle, but setting width on a Square also sets height. Code expecting 'change width, height stays' breaks. Lesson: model behavior, not vocabulary.

#### Interface Segregation (ISP)

Clients shouldn't depend on methods they don't use. Big god-interfaces force every consumer to mock unused methods and slow change.

Refactor IUserService with 30 methods into IUserAuthenticator, IUserProfile, IUserAdmin. Each consumer depends on the slice it needs.

#### Dependency Inversion (DIP)

High-level policy depends on abstractions, not low-level details. Low-level details depend on abstractions too. The result: business logic is testable and swappable from infrastructure.

This is what every DI container in .NET implements. Your business code accepts IPromotionRepository in its constructor, not a concrete EfCorePromotionRepository. Tests inject a fake; production injects EF Core; tomorrow you swap to Cosmos without touching domain.

> **Architect Insight:** DIP is the single most important SOLID principle for architects. It enables hexagonal architecture, swappable infrastructure, and unit testing. Most 'unfixable' legacy code is unfixable because DIP was violated.

### 1.4 Layered Architecture

The classic 3-tier or n-tier layout: Presentation -> Business -> Data. Each layer talks only to the one directly below it (closed layers) or sometimes skips (open layers).

Strengths: simple, well-understood, fits monolithic and CRUD-heavy systems. Most internal enterprise apps are layered, including most of your Kraft Heinz work.

Weaknesses: business logic tends to leak into the data layer (stored procs) or presentation (controllers with logic). Hard to swap data store. Tight coupling between layers via DTOs.

### 1.5 Clean / Onion / Hexagonal Architecture

These three are siblings. They all answer the same question: 'How do I keep business logic independent of frameworks, databases, and UIs?' Differences are mostly vocabulary.

#### Clean Architecture (Uncle Bob)

Concentric circles. Inside-out: Entities -> Use Cases -> Interface Adapters -> Frameworks & Drivers. The dependency rule: source code dependencies point inward only.

Layers in .NET solution form:

- Domain — entities, value objects, domain events. No framework references.
- Application — use cases, commands, queries, handlers, port interfaces. Depends only on Domain.
- Infrastructure — EF Core, Service Bus, file system, third-party SDKs. Implements ports defined in Application.
- WebApi (or other entry) — controllers, DI registration, middleware. Depends on Application and Infrastructure for composition root only.

#### Onion Architecture

Same idea, different drawing. Domain at the center, then Domain Services, then Application Services, then Infrastructure as the outer ring. Dependencies flow inward.

#### Hexagonal (Ports & Adapters) — Alistair Cockburn

Domain core surrounded by ports (interfaces). Adapters connect ports to the outside world. Inbound adapters drive the application (HTTP, CLI, message handlers); outbound adapters are driven by the application (database, queue publishers, email).

> **Architect Insight:** Pick Hexagonal vocabulary in interviews. 'Ports' and 'adapters' lands as more architect than 'layers.' The concept is identical; the framing signals seniority.

> **Trade-off:** These architectures cost up-front complexity. For a small CRUD service with one consumer and one DB, Clean Architecture is overkill. Use it when (a) team is large, (b) life of system is long, (c) infrastructure may swap, or (d) testability matters.

### 1.6 Architecture Decision Records (ADRs)

An ADR is a short document capturing one architectural decision: context, options considered, decision, and consequences. The format Michael Nygard popularized:

```text
# ADR-001: Use Azure Service Bus for inter-service messaging

## Status

Accepted, 2026-01-15

## Context

We have 5 microservices needing async communication.

Volume: ~100K msg/day, ordered per customer required for billing.

## Options Considered

1. Azure Service Bus — sessions for ordering, DLQ, mature.

2. Event Grid — push, simple, but no per-key ordering.

3. Event Hubs — high throughput, but partition-key ordering only.

## Decision

Azure Service Bus with sessions keyed on customerId.

## Consequences

+ Per-customer ordering guaranteed.

+ Built-in DLQ + retry.

- Higher cost than Storage Queues.

- Single-region; cross-region adds geo-DR cost.

- Requires session-aware consumers.
```

Keep ADRs in version control next to code: docs/adr/0001-use-service-bus.md. Number sequentially. Link from PRs.

> **Interview Tip:** In interviews, mention 'we documented this in an ADR' when describing past decisions. Signals discipline and traceability — a hallmark of architect maturity.

### 1.7 Architectural Trade-off Analysis

Every architectural decision is a trade-off across competing forces. The table below is your mental model. Every interview answer should reference at least 2-3 of these axes.

| Axis | Trade against |
| --- | --- |
| **Consistency** | Availability + latency (CAP). |
| **Scalability** | Cost + complexity. |
| **Performance** | Cost + simplicity (caching, replicas). |
| **Security** | Latency + developer velocity (extra hops, more reviews). |
| **Cost** | Resilience + performance (cheap = single AZ = brittle). |
| **Time-to-market** | Long-term maintainability (shortcuts compound). |
| **Flexibility** | Simplicity (every config knob is a future bug). |
| **Observability** | Cost + storage + privacy. |

> **Architect Insight:** Architects don't pick winners. They make trade-offs explicit and let business priorities choose. 'Given that latency is our #1 NFR, we pay the consistency cost' is the architect voice.

## 2. Design Patterns (GoF + Enterprise Essentials)

This is not a 23-pattern textbook. It's the patterns that come up in interviews and design reviews, with code-style sketches and the trigger sentence (when to reach for it).

### 2.1 Singleton

Trigger: 'There must be exactly one of this thing across the process and global access is acceptable.'

Use cases: configuration, in-memory cache, logger (rarely needed; DI usually solves it via lifetime: Singleton).

```csharp
public sealed class Config {
    private static readonly Lazy<Config> _instance = new(() => new Config());
    public static Config Instance => _instance.Value;
    private Config() { /* load */ }
}
```

> **Trade-off:** Hidden global state. Hostile to unit tests. In modern .NET, prefer 'AddSingleton\<T\>' in DI — same lifetime, no global access, fully testable.

### 2.2 Factory Method & Abstract Factory

Trigger: 'I need to create objects of varying types, and the choice depends on input or context.'

Factory Method: a method on a base class returns the right subtype. Abstract Factory: an interface that creates families of related objects.

```csharp
public interface IPaymentProcessorFactory {
    IPaymentProcessor Create(PaymentMethod method);
}

public class PaymentProcessorFactory : IPaymentProcessorFactory {
    public IPaymentProcessor Create(PaymentMethod m) => m switch {
        PaymentMethod.Card => new StripeProcessor(),
        PaymentMethod.PayPal => new PayPalProcessor(),
        _ => throw new NotSupportedException()
    };
}
```

> **Architect Insight:** In real .NET, you rarely write factories by hand — DI does it. Reach for an explicit factory when (a) the choice depends on runtime data DI doesn't have, or (b) you create many short-lived instances per request.

### 2.3 Builder

Trigger: 'I'm constructing an object with many optional parts and want a readable, immutable result.'

```csharp
var promo = new PromotionBuilder()
    .WithName("BlackFriday")
    .WithDiscount(0.20m)
    .ValidFrom(start).ValidUntil(end)
    .ForChannels("web", "mobile")
    .Build();
```

In C#, object initializers and 'with' expressions for records replace Builder for simple cases. Reach for Builder when validation must occur at Build() time, or construction has a non-trivial state machine.

### 2.4 Strategy

Trigger: 'I have many algorithms or rules for the same task, selectable at runtime.'

```csharp
public interface IDiscountStrategy {
    decimal Apply(Cart cart);
}

public class PercentageDiscount : IDiscountStrategy { ... }
public class BogoDiscount : IDiscountStrategy { ... }

public class CheckoutService {
    private readonly IEnumerable<IDiscountStrategy> _strategies;
    public Money Total(Cart cart) =>
        _strategies.Aggregate(cart.Subtotal,
            (acc, s) => acc - s.Apply(cart));
}
```

> **Interview Tip:** Strategy is the most-cited GoF pattern in design reviews. If asked 'how would you support adding new discount types without redeploying core code?' — Strategy + plugin loading is the answer.

### 2.5 Decorator

Trigger: 'I need to add cross-cutting behavior (logging, caching, retry, metrics) without modifying the core class.'

```csharp
public class CachedRepo<T> : IRepo<T> {
    private readonly IRepo<T> _inner;
    private readonly IMemoryCache _cache;
    public T Get(string id) => _cache.GetOrCreate(id, _ => _inner.Get(id));
    public void Save(T item) { _inner.Save(item); _cache.Remove(item.Id); }
}
```

Stack decorators: LoggedRepo(CachedRepo(RetryRepo(SqlRepo))). Each is independent and reusable. This is the pattern behind ASP.NET Core middleware, action filters, and HttpClient delegating handlers.

### 2.6 Observer

Trigger: 'When this thing changes, multiple unrelated things need to react.'

In modern .NET: events, IObservable\<T\>, or domain events through MediatR. In distributed systems, observer becomes pub/sub via Service Bus topic.

### 2.7 Adapter

Trigger: 'I have a class with the wrong interface for my consumer.'

Real example from your VB6 -> .NET migration: the old VB6 system exposed a COM interface. You wrote a .NET adapter implementing IPromotionLegacyClient that wraps the COM calls and exposes idiomatic C# methods. New code only sees the adapter; the COM ugliness is contained.

### 2.8 Repository (Enterprise)

Trigger: 'I want my domain to think in collections, not SQL.'

Repository hides persistence behind a collection-like interface (Get/Find/Add/Remove). Aggregate roots are the only entities with repos.

```csharp
public interface IPromotionRepository {
    Task<Promotion?> GetByIdAsync(PromotionId id);
    Task<IReadOnlyList<Promotion>> FindActiveAsync(DateTime onDate);
    Task AddAsync(Promotion promo);
}
```

> **Trade-off:** EF Core's DbContext IS already a Unit of Work + Repository. Wrapping it in your own IRepository adds a layer with little benefit unless (a) you'll swap data stores, (b) your domain is rich enough for a DDD boundary, or (c) you need to mock for fast tests. Otherwise, inject DbContext directly.

### 2.9 Other Patterns Worth Knowing (Briefly)

| Pattern | Trigger sentence |
| --- | --- |
| **Facade** | I want to expose a simpler API over a complex subsystem. |
| **Proxy** | I want to control access to an object (lazy load, security, remote). |
| **Composite** | I have part-whole hierarchies that should be treated uniformly. |
| **Bridge** | I want to vary abstraction and implementation independently. |
| **Flyweight** | I have many small objects with shared state — pool the shared part. |
| **Chain of Responsibility** | I want a request to walk a chain until something handles it. |
| **Mediator** | I want N components to talk through one hub instead of N x N. |
| **Command** | I want to encapsulate a request as an object (queue, undo, audit). |
| **State** | Behavior changes based on internal state; lots of if(state) chains. |
| **Template Method** | Algorithm skeleton with subclass-supplied steps. |
| **Iterator** | Sequential access to a collection without exposing structure. |
| **Visitor** | Add operations to a class hierarchy without modifying classes. |
| **Memento** | Capture and restore an object's state (undo). |

> **Interview Tip:** Don't memorize all 23. In interviews, when describing a solution, name the pattern explicitly: 'I'd model that as a Strategy, with rules pluggable at runtime.' Signals fluency without lecturing.

## 3. Enterprise & Cloud Patterns

These patterns sit above GoF. They describe how distributed and cloud systems coordinate, integrate, and evolve. Architect interviews lean heavily on these.

### 3.1 Repository + Unit of Work (and when not to use them)

Unit of Work tracks changes to multiple repositories within a single transactional scope and commits them atomically.

```csharp
public interface IUnitOfWork : IDisposable {
    IPromotionRepository Promotions { get; }
    IOrderRepository Orders { get; }
    Task<int> SaveChangesAsync();
}
```

> **Trade-off:** EF Core's DbContext implements UoW + repositories already. Building these abstractions on top is the most common over-engineering in .NET. Build them only when your domain is rich and you genuinely want infrastructure independence.

### 3.2 CQRS — Command Query Responsibility Segregation

Split write models (commands) from read models (queries). The two can use different schemas, different stores, and scale independently.

Why it matters: read traffic is usually 10-100x write traffic. Optimizing both sides for different shapes lets you scale them independently.

Implementation in .NET typically uses MediatR:

```csharp
public record CreatePromotionCommand(string Name, decimal Discount) : IRequest<Guid>;

public class CreatePromotionHandler : IRequestHandler<CreatePromotionCommand, Guid> {
    public async Task<Guid> Handle(CreatePromotionCommand cmd, CancellationToken ct) {
        var promo = Promotion.Create(cmd.Name, cmd.Discount);
        await _repo.AddAsync(promo);
        await _publisher.PublishAsync(new PromotionCreated(promo.Id));
        return promo.Id;
    }
}
```

The read side can be a denormalized projection in Cosmos, Redis, or Elasticsearch — populated by event handlers.

> **Trade-off:** CQRS adds eventual consistency (read model lags writes). Don't apply it everywhere. Use it when: (a) read shape differs significantly from write shape, (b) read scale dwarfs write, or (c) domain logic is complex enough to benefit from clean command boundaries.

### 3.3 Saga Pattern — Orchestration vs Choreography

A saga manages a long-running business transaction across multiple services using local transactions and compensating actions.

#### Orchestration

A central orchestrator service drives the saga step by step. It tells each service what to do and listens for results. Easy to monitor, easy to reason about, but creates a coordinator that can become a bottleneck or single source of complexity.

#### Choreography

Each service emits events; other services subscribe and react. No central coordinator. Loose coupling, but the workflow is implicit — distributed across services and harder to trace.

Example: Order placement saga

Choreography flow:

```text
OrderService: OrderPlaced event ->
PaymentService: charges card -> PaymentCompleted ->
InventoryService: reserves stock -> StockReserved ->
ShippingService: schedules pickup -> OrderShipped
```

On failure, compensating events flow back:

```text
StockReservationFailed -> PaymentService refunds -> OrderCancelled
```

> **Architect Insight:** Rule of thumb: \<= 4 steps and need clear status visibility -> orchestration. \> 4 steps with mostly independent services -> choreography. Many real systems mix both.

### 3.4 Outbox Pattern (and Inbox)

Solves the dual-write problem: 'I need to update the database AND publish a message, atomically.' You can't 2PC across DB and broker, so you use the database itself as the queue.

Mechanism:

- In one local DB transaction, write the business state AND insert a row into an Outbox table.
- A separate process polls the Outbox, publishes messages to the broker, marks rows sent.
- On the consuming side, an Inbox table dedups by message id (idempotent receiver).

```sql
BEGIN TRAN
INSERT INTO Promotion (...);
INSERT INTO Outbox (Id, Type, Payload, Status) VALUES (...);
COMMIT
```

Background job:

```text
rows = SELECT TOP 100 * FROM Outbox WHERE Status='pending';
foreach (row in rows): publish to bus; update row Status='sent';
```

> **Interview Tip:** Outbox is THE answer to any 'how do you guarantee reliable event publishing' question. Master this — it's asked in nearly every architect interview involving messaging.

### 3.5 Strangler Fig

Coined by Martin Fowler. Migrate a legacy system incrementally by routing traffic through a facade that progressively delegates more functionality to new code, until the old system can be deleted.

This IS the story of your VB6 -> React/.NET migration at Kroger. Steps:

- Place a facade (API Gateway / reverse proxy) in front of the legacy.
- Identify the first 'leaf' to migrate (low coupling, high value).
- Build new service, route just that endpoint through the facade to the new code.
- Repeat. Old system shrinks, new system grows.
- When old has no users, decommission.

> **Architect Insight:** In interviews: 'We migrated using the Strangler Fig pattern' is a much stronger phrase than 'We rewrote it.' Use the term.

### 3.6 Anti-Corruption Layer (ACL)

A translation boundary between your bounded context and someone else's model — usually a legacy system, third-party API, or partner. The ACL prevents foreign concepts from leaking into your clean domain.

Example: integrating with a vendor whose API uses 'CustomerProfile' with 47 fields. Your ACL converts to your domain's Customer (5 fields) on the way in, and back on the way out. Your domain stays pure.

### 3.7 BFF — Backend For Frontend

Each client type (web, mobile, partner) gets its own API tailored to that client's needs. Aggregates across multiple downstream services so the client makes one call instead of many.

Why it exists: a mobile app over poor networks suffers if it makes 7 API calls per screen. A BFF makes 7 server-side calls and returns one shape.

> **Trade-off:** BFFs add a service to maintain. Good for \>= 2 distinct clients with different needs. Overkill if clients are similar.

### 3.8 Sidecar & Ambassador

Sidecar: a helper container deployed alongside your main app in the same pod, providing cross-cutting features (logging, mTLS, config) without modifying the app.

Ambassador: a sidecar variant that proxies outbound traffic. Common for service mesh (Envoy in Istio).

Why architects care: it's how service mesh, observability, and zero-trust are added to existing apps without code changes.

### 3.9 Other Cloud Patterns to Know

| Pattern | Use case |
| --- | --- |
| **Cache-Aside** | App reads cache; on miss, reads DB and populates cache. |
| **Materialized View** | Pre-compute a query-optimized projection. |
| **Compensating Transaction** | Undo a previously completed step in a saga. |
| **Bulkhead** | Isolate failures by partitioning resources (thread pools, connections). |
| **Throttling / Rate Limit** | Protect downstream by capping request rate. |
| **Health Endpoint Monitoring** | Expose /health for orchestrators (K8s, Service Fabric). |
| **Leader Election** | One node owns a task at a time across replicas. |
| **Gateway Aggregation** | API Gateway calls multiple services and composes one response. |
| **Gateway Offloading** | Move auth/SSL/logging from app to gateway. |
| **Pipes and Filters** | Decompose long processing into reusable filter stages. |

## 4. System Design Fundamentals

This section covers the vocabulary and mental models every system design interview tests: CAP, partitioning, replication, caching, load balancing, queuing. Be able to whiteboard each of these on demand.

### 4.1 CAP & PACELC

CAP theorem (Brewer): in the presence of a network partition, a distributed system can deliver either Consistency or Availability, not both. The 'P' is not optional — partitions happen — so the real choice is C vs A during a partition.

| Choice | Behavior during partition |
| --- | --- |
| **CP (consistency)** | Reject reads/writes that can't be consistent. Examples: traditional RDBMS clusters, MongoDB (with majority writes), HBase, ZooKeeper. |
| **AP (availability)** | Continue serving, accepting eventual consistency. Examples: Cassandra, DynamoDB, Cosmos DB (default modes), Riak. |

PACELC extends CAP for the normal (non-partitioned) case: if Partition then C-or-A, Else Latency-or-Consistency. Real systems trade latency for consistency even when healthy.

| System | PACELC profile |
| --- | --- |
| **DynamoDB** | PA / EL — favors availability and low latency. |
| **Cosmos DB (Strong)** | PC / EC — favors consistency in both modes. |
| **MongoDB (default)** | PA / EC — A on partition, C otherwise. |
| **Cassandra** | PA / EL — fast and available, eventually consistent. |

#### Consistency Models — Beyond Strong vs Eventual

| Model | Guarantee |
| --- | --- |
| **Strong (linearizable)** | Reads see the latest write across the system. Highest cost. |
| **Sequential** | All clients see operations in the same order, but not real-time. |
| **Causal** | Operations causally related are seen in order; unrelated ones may differ. |
| **Read-your-writes** | A client sees its own writes immediately on subsequent reads. |
| **Monotonic reads** | A client never sees an older value after a newer one. |
| **Eventual** | All replicas converge eventually. No timing guarantee. |

> **Architect Insight:** Don't say 'eventually consistent' as if it's a single thing. Cosmos DB, for example, offers 5 distinct consistency levels (Strong, Bounded Staleness, Session, Consistent Prefix, Eventual). Architect-level answers name a specific level and defend it.

### 4.2 Partitioning (Sharding)

Splitting data across nodes so each node holds a subset. Necessary when data exceeds one node's storage or write throughput.

#### Partitioning Strategies

| Strategy | How it works / when to use |
| --- | --- |
| **Hash partitioning** | hash(key) mod N -> node. Even distribution. Bad for range queries. Cosmos DB default. |
| **Range partitioning** | Keys in ranges per node (e.g., A-F, G-L). Good for range scans. Risk: hot ranges. |
| **Geo / directory** | By region or tenant. Aligns with regulatory needs (data residency). |
| **Composite** | Tenant+key, where tenant determines coarse partition and key fine-grains. |

#### Hot Partitions — The Killer Problem

When one key gets disproportionate traffic — a celebrity user on social media, a viral product on Black Friday — that partition burns while others idle. Mitigations:

- Choose high-cardinality partition keys. UserId is usually better than CountryCode.
- Add randomness for write hot spots: append random suffix to key, fan-out to N sub-keys (write 1-of-N, read all-N and merge).
- Cache hot reads aggressively at the edge.
- Move very hot tenants to dedicated capacity (VIP partitions).

#### Resharding

When you outgrow your partition scheme. Painful, often requires double-write + backfill + cutover. Modern systems (Cosmos, DynamoDB) handle this automatically; classic shard-by-modulo schemes do not — picking modulo upfront is a known anti-pattern. Consistent hashing avoids most reshuffling.

> **Interview Tip:** If asked 'how do you partition this?' your answer should include: (1) chosen key, (2) why high cardinality, (3) how you'd detect a hot partition, (4) what you'd do about it. That's the architect framing.

### 4.3 Replication

Multiple copies of data across nodes for availability and read scaling.

#### Replication Topologies

| Topology | Description |
| --- | --- |
| **Single leader (primary-replica)** | All writes go to leader; replicas follow. Simple. Failure = failover. Used by SQL Server, Postgres, MySQL. |
| **Multi-leader** | Writes accepted at multiple leaders, replicated bidirectionally. Conflict resolution required (LWW, CRDTs). Used in geo-distributed systems. |
| **Leaderless (quorum)** | All replicas equal. Reads/writes use quorums (R+W\>N for consistency). Cassandra, DynamoDB. |

#### Sync vs Async Replication

| Mode | Trade-off |
| --- | --- |
| **Synchronous** | Write returns only after replicas ack. Strong consistency, higher latency. Risk: slow replica blocks writes. |
| **Asynchronous** | Write returns once leader has it; replicas catch up. Low latency. Risk: data loss on leader failure (RPO \> 0). |
| **Semi-sync (typical)** | Wait for at least one replica to ack. Compromise. |

#### Quorum Math (Leaderless Systems)

Given N replicas, with W writes acked and R reads from replicas, you get strong consistency if R + W \> N.

Example: N=5. Choose W=3, R=3 -> R+W=6 \> 5, consistent. Or W=5, R=1 -> 'write to all, read from one' (consistent reads, slow writes). Or W=1, R=1 -> fast both, eventually consistent.

```text
N=5, R=3, W=3 -> tolerates 2 node failures, consistent.
N=3, R=2, W=2 -> tolerates 1 failure, consistent (typical).
N=3, R=1, W=1 -> tolerates 2 failures, eventually consistent.
```

### 4.4 Caching

The four standard caching strategies:

| Strategy | How it works / trade-offs |
| --- | --- |
| **Cache-Aside (Lazy)** | App checks cache first; on miss, reads DB, populates cache. Simple, common. Risk: stale on write if not invalidated. |
| **Read-Through** | Cache library fetches from DB on miss. App talks only to cache. Cleaner separation. |
| **Write-Through** | Writes go to cache, which writes synchronously to DB. Cache always fresh. Higher write latency. |
| **Write-Behind (Write-Back)** | Writes go to cache, async-flushed to DB. Lowest latency. Risk: data loss if cache fails before flush. |

#### Cache Invalidation

Phil Karlton: 'There are only two hard things in computer science: cache invalidation and naming things.' Strategies:

- TTL (Time-to-live): simple, may serve stale. Pick TTL based on tolerance for staleness.
- Explicit invalidation: on write, delete the key. Hard at scale (which keys?).
- Versioned keys: include version in key (user:42:v17). Old versions just expire by TTL. Avoids precise invalidation.
- Stale-while-revalidate: serve stale, refresh in background. Latency-friendly.

#### Cache Stampede / Thundering Herd

When a hot key expires, thousands of requests miss simultaneously and pile onto the DB. Mitigations:

- Probabilistic early expiration (recompute slightly before TTL).
- Mutex / single-flight: only one request rebuilds; others wait.
- Background refresh by a dedicated job for hot keys.

### 4.5 Load Balancing

| Layer | What it does / examples |
| --- | --- |
| **L4 (transport)** | Routes by IP/port. Fast, simple. AWS NLB, Azure Load Balancer. |
| **L7 (application)** | Routes by URL/header/cookie. Can do TLS termination, sticky sessions. AWS ALB, Azure App Gateway, Front Door, Nginx, Envoy. |

#### Algorithms

- Round-robin: simple, ignores load.
- Least connections: routes to least-busy backend.
- Weighted: by backend capacity.
- Hash-based (consistent hashing): same key always to same backend. Good for cache affinity.
- IP hash / sticky session: client always to same backend. Required when state is local. Limits scale.

> **Architect Insight:** Aim for stateless services. Sticky sessions are an architecture smell — they prevent free horizontal scaling and complicate failure recovery. If you need session state, externalize it (Redis, sticky-by-token, JWT).

### 4.6 Queues, Streams & Backpressure

| Concept | Definition |
| --- | --- |
| **Queue** | Point-to-point: each message consumed once. Service Bus queue, SQS, RabbitMQ classic. |
| **Topic / pub-sub** | One message fans out to N subscribers. Service Bus topic, Kafka, Event Grid. |
| **Stream** | Ordered, replayable log. Consumers track offsets. Kafka, Event Hubs, Kinesis. |
| **Dead Letter Queue (DLQ)** | Messages that failed processing repeatedly are routed here for inspection. |
| **Backpressure** | Mechanism to slow producers when consumers can't keep up (queue depth thresholds, 429s, reactive streams). |

#### Queue vs Stream — Practical Picker

| Need | Use Queue | Use Stream |
| --- | --- | --- |
| **One consumer per message** | Yes | Use group-of-1 consumer pattern |
| **Replay history** | No (consumed = gone) | Yes (by offset) |
| **Order guarantee per key** | Sessions (Service Bus) | Native (partition) |
| **Multiple independent consumers** | Use topic | Native (consumer groups) |
| **Throughput \>= 100K msg/sec** | Possible but expensive | Native fit |

> **Interview Tip:** When asked 'queue or stream?' say: 'Stream if I need replay or fan-out at high throughput; queue if I need transactional once-only delivery.' That sentence handles 80% of the question.

## 5. Microservices Architecture (Deep)

### 5.1 When Microservices Are Right — and When They're Not

Microservices are a solution to organizational and scale problems, not a default architecture. Many companies that adopted microservices early regret it; many that stayed monolithic accelerated.

#### When Microservices Help

- Multiple teams (15+) need to ship independently and step on each other in a monolith.
- Different parts of the system have very different scaling needs (recommendation engine vs login).
- Different parts have different reliability needs (payment can't go down; analytics can).
- Different parts have different tech needs (ML in Python, transactional in .NET).
- You have the operational maturity: CI/CD, observability, service mesh, on-call rotations.

#### When Microservices Hurt

- Small team (under 10 engineers). The coordination overhead exceeds the benefit.
- You haven't yet built CI/CD, IaC, distributed tracing. You'll regret it within months.
- The domain isn't clear. Boundaries drawn early often need to move; in microservices that's expensive (database splits, contract changes).
- Cross-service transactions are common in your domain. Sagas are hard.

> **Architect Insight:** The dominant 'microservices' anti-pattern is the distributed monolith: services that must be deployed together, share databases, or have synchronous chains. You get all the cost of microservices and none of the benefits. If your services can't deploy independently, you don't have microservices.

#### The 8 Fallacies of Distributed Computing (Deutsch / Gosling)

Memorize these. Interviewers love hearing them cited:

- The network is reliable. (It isn't.)
- Latency is zero. (It's not.)
- Bandwidth is infinite. (It's metered.)
- The network is secure. (Assume hostile.)
- Topology doesn't change. (Pods restart, IPs rotate.)
- There is one administrator. (There isn't.)
- Transport cost is zero. (Serialization, encryption, routing all cost.)
- The network is homogeneous. (It isn't.)

### 5.2 Domain-Driven Design (DDD) Essentials

DDD is the discipline of building software around the business domain, with a shared language between domain experts and engineers. For microservices, DDD provides the rigor for picking service boundaries.

#### Core Concepts

| Concept | Definition |
| --- | --- |
| **Ubiquitous Language** | Domain experts and engineers use the same words for the same things. 'Order' means one thing only. |
| **Bounded Context** | An explicit boundary inside which a model has a single, consistent meaning. The same term may mean different things in different contexts. |
| **Aggregate** | A cluster of domain objects treated as a unit for data changes. Has a single root entity. Transactions never span aggregates. |
| **Aggregate Root** | The only entity outside code can hold a reference to. Enforces invariants. |
| **Entity** | Object with identity that persists over time (Customer, Order). |
| **Value Object** | Object defined entirely by its attributes; immutable (Money, Address, DateRange). |
| **Domain Event** | Something that happened that domain experts care about (OrderPlaced, InventoryReserved). |
| **Repository** | Persistence boundary at the aggregate level. |
| **Domain Service** | Operation that doesn't naturally belong to a single entity (e.g., transferring funds between accounts). |

#### Bounded Contexts -> Service Boundaries

The strongest heuristic: one bounded context per microservice. The bounded context is the smallest unit where a model is internally consistent.

Example for your Kroger Sales Promotion: bounded contexts might be Promotion Authoring, Promotion Eligibility, Pricing, Order Application. Each is a microservice candidate. Each owns its data; integrations happen via events or anti-corruption layers.

> **Interview Tip:** In design interviews, when picking service boundaries, name the bounded contexts. 'I'd start with three bounded contexts: Catalog, Pricing, and Promotion. Each becomes a service that owns its data.' That's the architect framing.

### 5.3 Sync vs Async Communication

Every cross-service call is one of these. Picking wrong is the most common cause of distributed monoliths.

| Style | When to use | Risks |
| --- | --- | --- |
| **REST (sync)** | Query-style, request-response, simple read. | Tight coupling, cascading failures, latency adds up. |
| **gRPC (sync)** | Internal service-to-service, low latency, typed contracts. | Same coupling risks; harder for cross-stack consumers. |
| **Message queue (async)** | Commands/events between services. Producer doesn't wait. | Eventual consistency, ordering issues, debugging harder. |
| **Event stream (async)** | High-volume events, multiple consumers, replay needed. | Schema evolution; consumer-side state matters. |
| **Webhooks** | External integrations. | Receiver-side reliability, retries on your shoulders. |

> **Architect Insight:** Default to async unless there's a specific reason to be sync. Sync calls turn N services into a chain whose availability is the product (99.9% ^ 5 = 99.5%). Async via events lets each service be independently available.

#### Common Sync Antipatterns

- Chained synchronous calls: A -> B -> C -> D. Latency adds up; one failure cascades.
- Synchronous call to a service inside a transaction. Now you hold a DB transaction over the network.
- Sync call for something the caller doesn't actually need to wait for (logging, metrics, notifications).

### 5.4 API Gateway, BFF, Service Mesh

#### API Gateway

Single entry point for client traffic. Provides cross-cutting concerns once instead of per-service:

- Authentication and authorization (JWT validation).
- Rate limiting and quotas.
- Request transformation, versioning, deprecation routing.
- Response caching.
- Logging, metrics, tracing inject points.
- TLS termination.

Implementations: Azure API Management (APIM), AWS API Gateway, Kong, Apigee.

#### BFF (Backend For Frontend) — Recap

Per-client API layer (mobile-bff, web-bff, partner-bff) that aggregates downstream calls and shapes responses for the specific client.

#### Service Mesh

Sidecar proxies (Envoy) injected next to every service. The mesh handles service-to-service concerns:

- mTLS (mutual TLS) between every service automatically.
- Retries, timeouts, circuit breakers — without the app knowing.
- Traffic shifting for canary / blue-green.
- L7 observability — every call automatically traced.

Implementations: Istio, Linkerd, Consul Connect, AWS App Mesh.

> **Trade-off:** Service mesh is heavy. Don't deploy until you have at least 15-20 services AND need policy/security/observability beyond what your platform gives you. For small fleets, Polly + APIM + App Insights covers most of the same ground at a fraction of the operational cost.

### 5.5 Service Discovery, Configuration & Secrets

#### Service Discovery

How service A finds an instance of service B without hardcoded addresses.

| Mode | How it works |
| --- | --- |
| **DNS-based** | Service B's name resolves to current IPs (Kubernetes Services, ECS Service Discovery). |
| **Client-side** | Client queries a registry (Eureka, Consul) and load-balances. |
| **Server-side** | Load balancer / API gateway hides discovery from clients. |

In Kubernetes (and AKS), you usually do nothing — DNS-based discovery is automatic via Service objects.

#### Configuration

Externalize configuration. Never compile or hardcode env-specific values. Use Azure App Configuration or Kubernetes ConfigMaps. Hot-reload where possible.

#### Secrets

Never put secrets in: source control, environment variables checked into IaC, ConfigMaps, plain DB. Use Azure Key Vault. Use Managed Identity to access Key Vault — no credentials at all on disk.

### 5.6 The Twelve-Factor App

Heroku's principles for building cloud-native apps. Architects reference these constantly.

| # | Principle |
| --- | --- |
| **1. Codebase** | One codebase tracked in version control, many deploys. |
| **2. Dependencies** | Explicitly declare and isolate dependencies. |
| **3. Config** | Store config in the environment, never in code. |
| **4. Backing services** | Treat backing services (DB, cache, queue) as attached resources. |
| **5. Build/release/run** | Strictly separate build and run stages. |
| **6. Processes** | Execute as one or more stateless processes. |
| **7. Port binding** | Export services via port binding (self-contained). |
| **8. Concurrency** | Scale out via the process model. |
| **9. Disposability** | Fast startup and graceful shutdown. |
| **10. Dev/prod parity** | Keep dev, staging, and prod as similar as possible. |
| **11. Logs** | Treat logs as event streams (stdout, never files). |
| **12. Admin processes** | Run admin tasks as one-off processes (same release). |

> **Interview Tip:** Memorize at least 1-3-6-11-12. Cite them naturally: 'Per twelve-factor, we kept the service stateless and externalized config to App Configuration.' Architect signal.

## 6. Distributed Data, Messaging & Resilience

### 6.1 Why Two-Phase Commit Doesn't Scale

In a single database, transactions are atomic via locks and a transaction log. Across services with separate databases, you'd theoretically use two-phase commit (2PC): a coordinator asks all participants to prepare, then commit if all agree.

Why 2PC fails at scale:

- Blocking: participants hold locks during the prepare phase. Slow or failed coordinator -> long lock holds.
- Coordinator failure leaves participants 'in doubt' — manual recovery or timeout cascade.
- Bad fit for cloud-native: HTTP, queues, and microservices don't easily 2PC.
- Latency: every transaction crosses the network twice.

Modern alternative: sagas with compensating transactions, plus the outbox pattern for reliable event publishing.

### 6.2 Sagas — Detailed

A saga is a sequence of local transactions, each in one service. If a step fails, previously-completed steps are undone via compensating transactions (semantic undo, not rollback).

#### Orchestration Saga (.NET sketch)

```csharp
public class OrderSagaOrchestrator {
    public async Task RunAsync(OrderPlaced cmd) {
        try {
            var payment = await _payments.ChargeAsync(cmd.OrderId, cmd.Amount);
            try {
                var stock = await _inventory.ReserveAsync(cmd.Items);
                try {
                    await _shipping.ScheduleAsync(cmd.OrderId, cmd.Address);
                    await _orders.MarkConfirmedAsync(cmd.OrderId);
                } catch {
                    await _inventory.ReleaseAsync(stock.ReservationId);
                    throw;
                }
            } catch {
                await _payments.RefundAsync(payment.ChargeId);
                throw;
            }
        } catch (Exception ex) {
            await _orders.MarkFailedAsync(cmd.OrderId, ex.Message);
        }
    }
}
```

In production, replace try/catch chains with a state machine library (e.g., MassTransit Saga, NServiceBus, Durable Functions). The state is persisted, so the orchestrator survives restarts.

#### Compensating Transactions — Important Properties

- Must be idempotent — they may run more than once.
- Must always succeed (eventually) — no compensation can be 'rejected' by business logic. Refunds, releases, and cancellations are designed to never fail.
- Are semantic, not physical undo — refunding a charge is not deleting the charge record.

### 6.3 Event Sourcing

Instead of storing the current state, store the sequence of events that led to it. Current state is a fold (left-reduce) over the event stream.

```text
Order events:
OrderPlaced(orderId, items, total)
PaymentReceived(orderId, amount)
ItemsShipped(orderId, trackingId)
OrderDelivered(orderId)

Current state = fold(events) -> Order { status: Delivered }
```

#### Why Event Sourcing

- Full audit log for free.
- Ability to replay events to rebuild any view (great with CQRS).
- Ability to project new views by replaying historical events into a new shape.
- Time travel debugging.

#### Why Event Sourcing Hurts

- Schema evolution: you can't change past events. Need versioning + upcasters.
- Snapshots: replaying millions of events is slow; need periodic snapshots.
- Querying current state is via projections, not direct queries.
- Steep learning curve for the team.

> **Trade-off:** Event sourcing is powerful but heavy. Apply only where audit, replay, or time-travel is genuinely required. For most CRUD apps, it's over-engineering.

### 6.4 Idempotency & The Exactly-Once Myth

In distributed systems, exactly-once delivery does not exist. You can have:

- At-most-once: send and pray. Messages may be lost.
- At-least-once: retry until ack. Messages may be duplicated. (Default of most brokers including Service Bus, Kafka.)

'Exactly-once processing' (effectively-once) is achieved by combining at-least-once delivery with idempotent receivers.

#### Idempotent Receivers

A handler that produces the same effect whether run once or N times. Mechanisms:

- Dedup table: store processed message ids; check before processing.
- Idempotency keys passed by caller (Stripe-style 'Idempotency-Key' header).
- Conditional updates: 'UPDATE ... WHERE Status='pending'' (the second one is a no-op).
- Set semantics: an operation that's a no-op when applied to already-applied state ('mark as shipped').

```csharp
public async Task HandleAsync(PaymentMessage msg) {
    var processed = await _db.IdempotencyKeys
        .FirstOrDefaultAsync(k => k.Key == msg.MessageId);
    if (processed != null) return; // already done

    using var tx = await _db.Database.BeginTransactionAsync();
    await ChargeCardAsync(msg.OrderId, msg.Amount);
    _db.IdempotencyKeys.Add(new IdempotencyKey { Key = msg.MessageId });
    await _db.SaveChangesAsync();
    await tx.CommitAsync();
}
```

> **Interview Tip:** 'Exactly-once is a myth; we get effectively-once via at-least-once delivery plus idempotent receivers' is a sentence that lands as architect-level immediately.

### 6.5 Azure Service Bus — Deep Dive

Service Bus is Azure's enterprise messaging service. Deeper guarantees than Storage Queues, lower throughput than Event Hubs.

#### Core Concepts

| Concept | Meaning |
| --- | --- |
| **Queue** | Point-to-point: one consumer per message. |
| **Topic + Subscription** | Pub-sub: each subscription gets its own copy. SQL filters supported. |
| **Sessions** | Group messages by session id (e.g., customerId) for FIFO-per-key processing. |
| **Peek-lock** | Receive without removing; commit (Complete) or release (Abandon) after processing. Default mode. |
| **Receive-and-delete** | Atomic receive + delete. Faster, but lose-at-most-once semantics. |
| **Dead-Letter Queue (DLQ)** | Messages that fail repeatedly or expire are routed here. Each queue/sub has one. |
| **Auto-forwarding** | Chain queues / topics. |
| **Scheduled / deferred** | Schedule for future, or set aside for later processing. |
| **Duplicate detection** | Optional, by MessageId, within a configurable window. |
| **Transactions** | Atomic send/receive across multiple entities in same namespace. |

#### Tiers

| Tier | Use case |
| --- | --- |
| **Basic** | Queues only. Cheapest. No topics, no sessions. Rarely used in production architectures. |
| **Standard** | Queues + topics + sessions. Shared-tenant. Good default for most workloads. |
| **Premium** | Dedicated capacity. Predictable latency. Required for VNet integration. Higher minimum cost. |

#### Common Patterns

- FIFO per customer: queue with sessions, sessionId = customerId. Single-threaded per session, parallel across sessions.
- Retry with backoff: Service Bus auto-retries on Abandon. Combine with delivery count + DLQ at threshold (default 10).
- Outbox publisher: poll Outbox table, send to Service Bus, mark sent. Use duplicate detection if your worker is at-least-once.
- Topic with multiple subscriptions: one OrderPlaced topic, subscriptions for billing, inventory, notifications, analytics. Each evolves independently.

> **Watch Out:** Service Bus is NOT a stream. You cannot replay messages once consumed. If you need replay, use Event Hubs or Kafka.

### 6.6 Event Grid, Event Hubs, Kafka — Picker

| Service | Best for | Avoid when |
| --- | --- | --- |
| **Service Bus** | Transactional messaging between services; FIFO per key; DLQ; \<= ~thousands msg/sec. | You need replay or \>100K msg/sec. |
| **Event Grid** | Reactive events from Azure resources or your apps. Push-based. Massive fan-out, simple. | You need ordering, transactions, or large message bodies (\>64KB). |
| **Event Hubs** | High-throughput telemetry and event streaming on Azure. Kafka-protocol option. | You need per-message ack semantics (it's pull-by-offset). |
| **Apache Kafka (self/managed)** | Event streaming with broad ecosystem (Connect, Streams, Schema Registry). | You don't need a stream and don't want operational complexity. |
| **Storage Queue** | Cheap, simple, large queues. Up to 64KB messages. | You need topics, sessions, or transactional features. |

### 6.7 Resilience Patterns (with Polly)

Polly is the standard .NET resilience library. The patterns you compose:

#### Retry

```csharp
var retry = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt =>
            TimeSpan.FromMilliseconds(
                Math.Pow(2, attempt) * 100 +
                Random.Shared.Next(0, 100))); // jitter
```

Use exponential backoff with jitter to avoid synchronized retries (the 'thundering herd' problem).

#### Circuit Breaker

After N consecutive failures, open the circuit and fail fast for a cooldown period. Then a half-open probe checks recovery.

```csharp
var breaker = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30));
```

> **Architect Insight:** Without a circuit breaker, retries make outages worse. The downstream service is already struggling; piling on retries amplifies the problem. Circuit breaker = 'stop hitting the dying service.'

#### Timeout

Cap how long you wait. Pessimistic timeouts (the operation may still run on the server) are common in HTTP. Combine with retry to avoid unbounded latency.

```csharp
var timeout = Policy.TimeoutAsync(
    TimeSpan.FromSeconds(2),
    TimeoutStrategy.Pessimistic);
```

#### Bulkhead

Isolate resources so that one failing dependency doesn't exhaust threads/connections used by others.

```csharp
var bulkhead = Policy.BulkheadAsync(
    maxParallelization: 20,
    maxQueuingActions: 100);
```

#### Composition (the Polly Order That Actually Works)

Compose outside-in. Standard order: Bulkhead -> Retry -> CircuitBreaker -> Timeout (-> actual call). This means: 'limit concurrency, retry on failure, but stop if the breaker is open, and don't let any single call hang forever.'

```csharp
var policy = Policy.WrapAsync(bulkhead, retry, breaker, timeout);
await policy.ExecuteAsync(() => _httpClient.GetAsync(url));
```

#### Fallback

Provide a graceful default response when all else fails. Cached value, simplified response, or a 'service degraded' message. Better than a 5xx.

#### Health Checks & Readiness Probes

Distinguish liveness (am I alive?) from readiness (am I ready for traffic?). In Kubernetes, this matters: failing readiness removes pod from load balancer; failing liveness restarts it.

```csharp
// .NET 8
builder.Services.AddHealthChecks()
    .AddSqlServer(connStr)
    .AddAzureServiceBusQueue(sbConn, "orders")
    .AddCheck<MyDeepCheck>("deep");

app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = _ => true });
```

## 7. Azure Deep Dive — Compute, Data, Identity, Network

### 7.1 The Azure Compute Landscape

Picking the right compute is the first move in any Azure architecture. Use this decision flow:

| Service | Best fit | Avoid when |
| --- | --- | --- |
| **Azure Functions** | Event-driven, short-lived, bursty workloads. HTTP, queue, timer triggers. | Long-running (\>10 min on Consumption); cold-start sensitive low-latency APIs. |
| **App Service** | Stateless web APIs, simple deployments, no container expertise needed. | You need full networking control, custom OS, or sidecars. |
| **Container Apps** | Containerized workloads with autoscale (KEDA), without K8s ops. | You need full K8s control plane (DaemonSets, custom CRDs, complex networking). |
| **AKS** | Microservices fleets, complex orchestration, multi-tenant platforms. | Small fleets (\<5 services); team lacks K8s ops maturity. |
| **Azure VMs** | Lift-and-shift legacy, custom OS, fully self-managed. | Anything cloud-native — VMs are the highest TCO. |
| **Service Fabric** | Stateful microservices, actor model. (Mostly legacy now.) | Greenfield projects — pick AKS + state in Cosmos/Redis. |
| **Logic Apps** | Low-code workflow integration with SaaS connectors. | Code-heavy logic; high-volume processing. |

### 7.2 Azure Kubernetes Service (AKS) — Deep

#### Core Kubernetes Concepts

| Concept | Role |
| --- | --- |
| **Pod** | Smallest deployable unit; 1+ containers sharing network and storage. |
| **Deployment** | Declarative spec for pods + ReplicaSet; rolling updates. |
| **Service** | Stable network endpoint for a set of pods. Types: ClusterIP, NodePort, LoadBalancer. |
| **Ingress** | L7 routing into the cluster. Implementations: NGINX Ingress, Application Gateway Ingress Controller (AGIC). |
| **ConfigMap / Secret** | Externalized config / sensitive data. |
| **HPA** | Horizontal Pod Autoscaler — scales replicas by CPU/memory or custom metrics. |
| **VPA** | Vertical Pod Autoscaler — adjusts resource requests. |
| **KEDA** | Kubernetes Event-Driven Autoscaling — scale by queue depth, etc. |
| **DaemonSet** | One pod per node (logging agent, security agent). |
| **StatefulSet** | Stable identity + storage per replica (databases, leader election). |
| **Job / CronJob** | One-off / scheduled batch tasks. |

#### AKS-Specific Features

- Node pools: separate pools for system pods, user workloads, GPU, spot. Pin workloads via node selectors / taints.
- Networking modes: kubenet (overlay, simpler) vs Azure CNI (each pod gets a VNet IP, more peering-friendly).
- Workload Identity: pods authenticate to Azure as a Managed Identity via federated credentials. The modern replacement for AAD Pod Identity.
- Cluster Autoscaler: scale nodes when pods are unschedulable.
- Azure Policy add-on: enforce K8s policies (no privileged pods, image registries, etc).

#### AKS Production Checklist

- Multiple node pools (system / user / spot).
- Azure CNI for Private Endpoint integration.
- Workload Identity (no service principals on disk).
- Azure Container Registry with image scanning.
- Network policies enforced.
- Pod-level resource requests/limits set on every workload.
- Health probes (liveness, readiness, startup).
- PodDisruptionBudgets for availability during node upgrades.
- Container Insights + Diagnostic settings -> Log Analytics.
- Automatic upgrades for control plane; planned maintenance windows.

> **Interview Tip:** When asked about your AKS setup, name your node pool strategy first. 'Three pools — system, user, spot. System pool taints keep user workloads off it. Spot pool runs batch jobs at ~80% cost savings.' That's architect detail.

### 7.3 Azure Data Stores

#### Picker Table

| Store | Best fit | Avoid when |
| --- | --- | --- |
| **Azure SQL DB** | Single-region, relational, transactional. PaaS Postgres/MySQL similar. | You need geo-distributed writes. |
| **Azure SQL Managed Instance** | Lift-and-shift SQL Server with VNet, Agent, cross-DB queries. | You're starting greenfield (Azure SQL DB is leaner). |
| **Cosmos DB** | Geo-distributed, multi-model, low-latency NoSQL with SLAs. | You need rich SQL queries, joins, complex transactions. |
| **Azure Storage Tables** | Massive scale, simple key-value, very cheap. | You need anything richer than partition+row key lookups. |
| **Azure Storage Blobs** | Files, images, video, big binary objects. | Anything transactional. |
| **Azure Cache for Redis** | Distributed cache, leaderboards, ephemeral state. | Durable storage. |
| **Azure Database for PostgreSQL / MySQL** | Open-source SQL with managed service. | You need SQL Server-specific features. |
| **Azure Synapse / Fabric** | Analytics, data warehouse, OLAP queries on TB-PB scale. | OLTP workloads. |

#### Cosmos DB Architect-Level Knowledge

- APIs: SQL (Core), MongoDB, Cassandra, Gremlin, Table. Pick based on existing skills/migration.
- Partition key choice: high cardinality, evenly accessed. Cardinality near unbounded. Common errors: tenantId for highly skewed tenants, dateOnly for time-series writes.
- RU/s (Request Units): the throughput currency. Provisioned (predictable) vs Serverless (small workloads) vs Autoscale (bursty).
- Consistency levels: Strong, Bounded Staleness, Session (default), Consistent Prefix, Eventual. Pick per-workload, not per-account.
- Change Feed: built-in event stream of all writes. Use for downstream projections, materialized views.
- Multi-region writes: enable for active-active deployments. Conflict resolution policies (LWW or custom).

> **Architect Insight:** Cosmos DB partition key is a one-way decision — you cannot change it on an existing container. Picking it well is one of the highest-leverage architectural decisions in any Cosmos design. Always defend your choice with read/write traffic shape and cardinality.

#### When NOT to Pick Cosmos DB

- Heavy joins or complex aggregations across data — go SQL DB or Synapse.
- Strict ACID across multiple partitions — Cosmos transactions are partition-scoped.
- Cost-sensitive low-volume single-region apps — RU pricing can be expensive vs SQL Basic tier.
- You don't need geo-distribution or its other distinctive features.

### 7.4 Identity — Entra ID, Managed Identity, Key Vault

#### Microsoft Entra ID (formerly Azure AD)

Azure's identity platform. Implements OAuth2 and OIDC. Foundation of Azure security.

- Tenants: each Entra directory.
- Users, Groups, Service Principals (apps), Managed Identities (Azure-resource-tied SPs).
- Conditional Access: policy-based access control (require MFA, compliant device, location).
- App Registrations: define applications, their redirect URIs, scopes, certificates.
- Roles: built-in (Owner, Contributor, Reader) and custom Azure RBAC. Apply at subscription, resource group, or resource scope.

#### Managed Identity

An identity in Entra automatically managed for an Azure resource. Two flavors:

- System-assigned: created and tied to one resource. Deleted when resource deletes.
- User-assigned: standalone, can be attached to multiple resources. Survives independently.

Why architects love Managed Identity: zero secrets on disk. The resource gets a token from the IMDS endpoint and uses it to call Azure APIs. Rotation and revocation are automatic.

```csharp
// .NET — connect to Azure SQL with Managed Identity
var conn = new SqlConnection(
    "Server=tcp:myserver.database.windows.net,1433;" +
    "Database=mydb;Authentication=Active Directory Default;");
// No password. The DefaultAzureCredential picks up the MI.
```

> **Interview Tip:** In any 'how do you handle secrets' question, Managed Identity should be your first sentence. 'We don't have connection strings. App talks to Azure SQL and Service Bus via Managed Identity; no rotation, no secrets in Key Vault for those.'

#### Azure Key Vault

Secret, key, and certificate store. Hardware Security Module (HSM) options for FIPS 140-2 compliance.

- Use for things Managed Identity can't help with: external API keys, third-party DB passwords, certificates.
- Reference secrets by URI in App Service, Functions, Container Apps.
- Soft-delete and purge protection: enable both for production.
- Access via RBAC (modern) or Access Policies (legacy).
- Rotate secrets via Event Grid notifications + automation.

### 7.5 Networking

#### Virtual Network (VNet) Basics

- VNet: an isolated network in Azure with your own IP space.
- Subnets: subdivide a VNet. Resources are deployed into subnets.
- NSGs (Network Security Groups): stateful L4 firewall rules. Attach to subnet or NIC.
- Route tables: control egress paths (to firewall, to on-prem).
- Service Endpoints (legacy): grant a subnet access to a PaaS service over the Azure backbone.
- Private Endpoints (modern): give a PaaS resource a private IP inside your VNet. Traffic never leaves the Azure backbone.

#### Hub-and-Spoke Topology

The reference topology for enterprise Azure landing zones.

- Hub VNet: shared services — Azure Firewall, ExpressRoute / VPN gateway, DNS Private Resolver, jump hosts.
- Spoke VNets: each workload (or workload group) gets a spoke. Peered to hub.
- All inter-spoke traffic transits through the hub firewall (no direct spoke-to-spoke peering).
- On-prem connects to hub via ExpressRoute or Site-to-Site VPN.

#### Private Endpoints Done Right

Place Private Endpoints for Azure SQL, Storage, Service Bus, Key Vault. Then disable public network access on the resource. Combined with Private DNS Zones, your apps resolve and reach data services entirely over private IPs.

> **Watch Out:** DNS is the #1 gotcha. Without a Private DNS Zone (or your own DNS resolution), the FQDN resolves to the public IP, and your traffic goes out the internet — defeating the purpose of the Private Endpoint.

#### Egress Control

By default, AKS pods can reach the internet. For compliance, force all egress through Azure Firewall or a third-party NVA. Required for regulated industries.

> **Architect Insight:** Network design is where many architect candidates get exposed. Be ready to whiteboard hub-spoke, place private endpoints, draw the DNS resolution path, and explain why the firewall is in the hub. Practice this — it's hard to wing.

## 8. Azure Deep Dive — Messaging, APIM, Observability, Cost, WAF

### 8.1 Azure Functions & Durable Functions

#### Hosting Plans

| Plan | Use case |
| --- | --- |
| **Consumption** | Pay-per-execution. True serverless. Cold starts. 10-min timeout. |
| **Premium** | Pre-warmed instances, VNet integration, longer timeouts, no cold start. |
| **Dedicated (App Service Plan)** | Run alongside web apps; predictable cost. No autoscale-to-zero. |
| **Flex Consumption** | Newer model: per-instance scaling, VNet, faster cold start. Default for greenfield serverless workloads in 2026. |

#### Triggers and Bindings

Triggers start a function. Bindings provide declarative input/output.

- Trigger types: HTTP, Timer, Queue, Service Bus, Event Hub, Event Grid, Cosmos Change Feed, Blob, Kafka.
- Bindings reduce boilerplate: input from blob, output to queue, all wired by attributes.

```csharp
[Function("ProcessOrder")]
[ServiceBusOutput("notifications", Connection = "Sb")]
public async Task<string> Run(
    [ServiceBusTrigger("orders", Connection = "Sb")] OrderMsg msg)
{
    var enriched = await _enricher.EnrichAsync(msg);
    return JsonSerializer.Serialize(enriched);
}
```

#### Durable Functions

Stateful workflows on top of Functions. Three patterns:

- Function chaining: F1 -> F2 -> F3, awaiting each.
- Fan-out / fan-in: parallel execution + aggregation.
- Async HTTP API: long-running work with status polling endpoint.
- Monitoring (recurring polling) and Human Interaction (waiting for external event).

Durable orchestrators are deterministic — they replay history. Side effects must be in activities, not the orchestrator.

> **Architect Insight:** Durable Functions is one of the cleanest ways to do orchestration sagas in Azure. State persists in storage; orchestrator survives crashes; replay is automatic. Mention it when discussing saga implementations.

### 8.2 API Management (APIM)

Azure's API gateway. Sits in front of any backend (Functions, App Service, AKS, on-prem) and provides a single, governed entry point.

#### Building Blocks

| Concept | Meaning |
| --- | --- |
| **Backend** | The API server APIM forwards to. |
| **API** | Imported from OpenAPI, WSDL, or defined manually. Versions and revisions supported. |
| **Operation** | Individual endpoint within an API. |
| **Product** | A bundle of APIs that consumers subscribe to (with a subscription key). |
| **Policy** | XML rules executed at inbound, backend, outbound, on-error stages. |
| **Developer Portal** | Auto-generated docs and sign-up site for API consumers. |

#### Common Policies

```text
<policies>
  <inbound>
    <validate-jwt header-name="Authorization"
        failed-validation-httpcode="401">
      <openid-config url="https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid-configuration" />
      <required-claims>
        <claim name="aud"><value>api://orders</value></claim>
      </required-claims>
    </validate-jwt>
    <rate-limit-by-key calls="100" renewal-period="60"
        counter-key="@(context.Subscription.Id)" />
    <set-header name="X-Correlation-Id" exists-action="skip">
      <value>@(Guid.NewGuid().ToString())</value>
    </set-header>
  </inbound>
  <backend><forward-request /></backend>
  <outbound>
    <set-header name="X-Powered-By" exists-action="delete" />
  </outbound>
  <on-error>
    <log-to-eventhub logger-id="audit-logger">
      @(context.Response?.StatusCode ?? 500)
    </log-to-eventhub>
  </on-error>
</policies>
```

#### Tiers

- Consumption: pay-per-call, lightweight, cheapest. No VNet integration in the Premium-style sense.
- Developer / Basic / Standard: shared dev/test environments.
- Premium: VNet integration (internal mode), multi-region active-active, AZ redundancy. Production-grade.
- Standard v2 / Premium v2: newer SKUs with simplified VNet, faster cold starts.

### 8.3 Front Door, App Gateway, Traffic Manager

| Service | Layer / scope | Use when |
| --- | --- | --- |
| **Azure Front Door** | Global L7 (anycast). | Global edge, multi-region routing, WAF, CDN, SSL offload, instant failover. |
| **Application Gateway** | Regional L7. | Inside a region, in-VNet, with WAF. Backend can be VMs/AKS/App Service. |
| **Traffic Manager** | Global DNS-based. | Routing across regions where you don't need an L7 proxy. DNS-only, slower failover. |
| **Azure Load Balancer** | Regional L4. | Internal LBs for VNet, basic public LBs. AKS uses one under the hood. |

Common production stack: Front Door (global edge + WAF) -> APIM (governance + auth) -> AKS (workloads), with App Gateway only when needed in front of an internal AKS Ingress.

### 8.4 Observability — Monitor, App Insights, KQL

#### The Three Pillars

| Pillar | Meaning |
| --- | --- |
| **Metrics** | Numeric time series (CPU, RPS, error rate). Cheap, real-time, alerting-friendly. |
| **Logs** | Discrete events with structure (request, exception, custom). Costlier, queryable. |
| **Traces** | Distributed request paths across services. Most expensive but invaluable. |

#### Azure Monitor Stack

- Azure Monitor: umbrella product.
- Application Insights: app-level telemetry (requests, dependencies, exceptions, custom events). Distributed tracing built-in.
- Log Analytics: backend store for App Insights and any Diagnostic Settings. Queries via Kusto Query Language (KQL).
- Alerts: metric alerts (cheap), log query alerts (richer).
- Dashboards: workbook reports.

#### KQL Examples

```text
// Top 10 slow endpoints (last hour)
requests
| where timestamp > ago(1h)
| summarize p95 = percentile(duration, 95) by name
| top 10 by p95 desc

// Error rate by operation, 5-min buckets
requests
| where timestamp > ago(24h)
| summarize errors = countif(success == false), total = count()
    by bin(timestamp, 5m), name
| extend errorRate = todouble(errors) / total
| where errorRate > 0.01
| order by timestamp desc

// Find slow dependency calls
dependencies
| where timestamp > ago(1h)
| where duration > 1000 // > 1 sec
| summarize count() by target, name
| order by count_ desc
```

#### Distributed Tracing

.NET 8 emits W3C Trace Context headers automatically (Activity API). App Insights stitches them into end-to-end traces. Verify by checking the 'End-to-end transaction' view for a request crossing 3+ services.

> **Interview Tip:** 'We instrumented every service with App Insights, used W3C trace context, and got distributed traces across our 5 services for free. Then KQL queries on dependency duration drove our optimization roadmap.' This is architect-level observability talk.

### 8.5 Security at the Edge — WAF, DDoS, Defender

#### Web Application Firewall (WAF)

Layer 7 protection against OWASP Top 10. Available on Front Door (global) and Application Gateway (regional).

- Managed rule sets (OWASP Core Rule Set, Microsoft Bot Manager).
- Custom rules by geo, IP, header.
- Modes: Detection (log only) vs Prevention (block).
- Always start in Detection mode in production for 1-2 weeks; review false positives; switch to Prevention.

#### DDoS Protection

- Basic: included free at the platform level.
- Standard / Network Protection: VNet-level, attack alerts, traffic analytics, DDoS Rapid Response (24/7 expert team during attack).

#### Microsoft Defender for Cloud

Cloud Security Posture Management (CSPM) + Cloud Workload Protection Platform (CWPP).

- Secure Score: a percentage rating of your security posture with actionable recommendations.
- Defender plans for SQL, Storage, AKS, App Service, Containers, etc.
- Regulatory compliance dashboard (PCI, HIPAA, ISO, NIST, SOC2 mappings).

### 8.6 Azure Well-Architected Framework

Microsoft's framework of 5 pillars. Architect interviews reference these by name.

| Pillar | Core question |
| --- | --- |
| **Cost Optimization** | Are we getting value for the spend? |
| **Operational Excellence** | Can we deploy, monitor, and recover effectively? |
| **Performance Efficiency** | Are we using the right resources for the load? |
| **Reliability** | Can we meet our SLOs even when things fail? |
| **Security** | Are we protected against threats? |

#### Each Pillar — Practical Architect Levers

##### Cost Optimization

- Right-size compute. Most VMs are over-provisioned by 30-50%. Use Azure Advisor.
- Reservations and Savings Plans for predictable workloads (1-year or 3-year).
- Spot VMs for interruptible workloads (batch, dev/test, ML training): up to 90% savings.
- Auto-scale in. Most teams set scale-out rules but forget scale-in.
- Storage tier (Hot/Cool/Archive) matched to access pattern.
- Tag everything; track cost per service/feature/team.
- Set budgets with alerts at 50%/75%/90%/100%.

##### Operational Excellence

- Everything as code: IaC (Bicep/Terraform), GitOps for K8s.
- CI/CD with environment promotion gates.
- Blue/green or canary deploys.
- Runbooks: documented procedures for common operations.
- Game days / chaos engineering.

##### Performance Efficiency

- Right SKU, right scaling rules, right region (proximity to users).
- Caching at every appropriate layer (browser, CDN, app, DB).
- Async over sync for non-critical paths.
- Load testing in production-like environments before launch.

##### Reliability

- Define SLOs explicitly (e.g., 99.9% availability, p99 \< 300ms).
- Multi-AZ for any production workload.
- Multi-region for tier-1 (active-active or active-passive).
- Backup + tested restore (an untested backup is a guess).
- Chaos testing failure modes.
- Health probes + automatic remediation (K8s, App Service health check).

##### Security

- Defense in depth: WAF + APIM + auth + network + RBAC.
- Zero trust: never trust the network. Authenticate every call.
- Managed Identity end-to-end.
- Private Endpoints for all PaaS data services in production.
- RBAC at the lowest scope necessary; eliminate Owner roles outside emergency break-glass.
- Encryption at rest (default in Azure) and in transit (TLS 1.2+).
- Secrets in Key Vault, never in code or config files.

> **Interview Tip:** Memorize the 5 pillars in order: 'CORPS — Cost, Operational excellence, Reliability, Performance, Security.' (Microsoft's order varies; CORPS is mnemonic-friendly.) Reference them when reviewing any architecture: 'Walking the 5 pillars: cost is fine because we're using reservations; reliability needs work — we're single-region today...'

#### FinOps Loop

Inform -> Optimize -> Operate. Continuous discipline:

- Inform: tag, allocate, show-back/charge-back per team.
- Optimize: rightsize, reserve, eliminate orphaned resources.
- Operate: budgets, alerts, monthly cost reviews per team.

## 9. React — The Architect View

React for a Solution Architect is not about hooks syntax. It's about the choices a senior engineer brings to you for sign-off: which state library, when to render server-side, how to scale a frontend across many teams. This section gives you defensible answers.

### 9.1 React Component & Rendering Model

#### Functional Components & Hooks

- Functional components are now the norm; class components are legacy.
- Core hooks you must know: useState, useEffect, useRef, useMemo, useCallback, useContext, useReducer.
- React 18+ adds: useTransition, useDeferredValue, useId, useSyncExternalStore.
- Custom hooks: any function whose name starts with 'use' and that calls other hooks. Reusable logic without HOCs.

#### Rendering & Reconciliation

- React maintains a virtual DOM tree; diffs against the previous tree; commits minimal real DOM mutations.
- React 18 enables concurrent rendering: renders can be interrupted, prioritized, or thrown away.
- Re-renders happen when state changes or parent re-renders. Children re-render unless memoized.

#### Common Hook Pitfalls

- Missing dependencies in useEffect / useCallback. Use the ESLint rule react-hooks/exhaustive-deps.
- Using useEffect for derived state. Most 'derived state' is just a calculation in render — no useEffect needed.
- Using useMemo / useCallback prematurely. They're not free — they cost memory and complexity.
- Closures over stale state. useRef or functional setState fix this.

### 9.2 State Management — The Picker

The most over-discussed topic in React. Architect-level answer: scope the state correctly.

| State type | Tool | When |
| --- | --- | --- |
| **Local UI state** | useState / useReducer | Forms, toggles, modals. Default. |
| **Shared between siblings** | Lift to common parent | When a couple of components need it. |
| **App-wide UI / theme / auth user** | Context + useReducer | Few updates; broad reads. |
| **Server cache (data fetched from API)** | TanStack Query / RTK Query / SWR | ALWAYS for server state. This is the biggest trade-off win in modern React. |
| **Complex client state (graph, editor)** | Redux Toolkit / Zustand / Jotai | Genuinely complex flows; many components updating. |
| **Form state** | React Hook Form / Formik | Validation, dirty tracking, large forms. |
| **URL-driven state** | Router params / search params | Anything that should be shareable / bookmarkable. |

> **Architect Insight:** The single biggest architecture mistake in modern React is putting server data in Redux/Context. Server state has cache lifetimes, refetch needs, optimistic updates, retries — TanStack Query handles all of these. Redux for server data is N years of self-inflicted pain. Use Redux only for genuine client state.

### 9.3 Performance Architecture

#### Bundle Size

- Code splitting: route-based with React.lazy + Suspense. Don't ship the admin panel to logged-out users.
- Tree shaking: import only what you use. Avoid 'import * as'.
- Analyze with source-map-explorer or webpack-bundle-analyzer. Set budgets.
- Vendor splitting: separate vendor chunk caches better across deploys.

#### Rendering Performance

- React.memo for expensive components that often re-render with same props.
- useMemo for expensive derivations referenced by many children.
- useCallback only when the callback is a dep of a memoized child or hook.
- Virtualize large lists: react-window, react-virtual.
- Defer non-critical work with useTransition (React 18).
- Profile with React DevTools Profiler — measure before optimizing.

#### Web Vitals — The Metrics Architects Track

| Metric | Goal |
| --- | --- |
| **LCP (Largest Contentful Paint)** | \< 2.5s |
| **INP (Interaction to Next Paint)** | \< 200ms |
| **CLS (Cumulative Layout Shift)** | \< 0.1 |
| **TTFB (Time To First Byte)** | \< 800ms |

Wire up web-vitals library, send to Application Insights or your analytics pipeline. SLOs based on field data, not lab data.

### 9.4 SSR, Next.js, Micro-Frontends

#### Rendering Strategies

| Strategy | When |
| --- | --- |
| **CSR (Client-Side Rendering)** | Internal apps, dashboards. SEO not needed. Fast iteration. |
| **SSR (Server-Side Rendering)** | Public marketing pages, e-commerce. Need SEO and fast first paint. |
| **SSG (Static Site Generation)** | Content rarely changes. Marketing, docs, blog. |
| **ISR (Incremental Static Regeneration, Next.js)** | Mostly static, occasional update. |
| **RSC (React Server Components)** | Mix server-rendered + interactive islands. The new direction. |

#### Next.js

- The de-facto React framework. Handles routing, SSR/SSG, code splitting, API routes.
- App Router (Next 13+) brings React Server Components.
- Architect concerns: deploy target (Vercel vs custom), edge vs node runtime, caching strategy, ISR semantics.

#### Micro-Frontends

Split a large frontend across teams, each deploying independently. Two main approaches:

| Approach | Description |
| --- | --- |
| **Build-time composition** | Each team ships an npm package. Composed at build time. Simple but couples deploy cadence. |
| **Run-time composition (Module Federation)** | Each team deploys a remote bundle. Loaded at runtime. True deploy independence. |
| **iFrame-based** | Strongest isolation, weakest UX. Rare in modern apps. |
| **Server-side composition (BFF / edge include)** | Server stitches HTML fragments from each team. Edge-friendly. |

> **Trade-off:** Micro-frontends solve organizational scaling. They cost real complexity: shared design system, cross-MFE state, performance budgets, dependency drift. Don't adopt with under 50 frontend engineers; the overhead exceeds the benefit.

### 9.5 Frontend Architecture for Large Teams

#### Folder & Layer Structure

For mid-to-large React apps, organize by feature, not file type. Flat src/components/ with 200 components is unmaintainable.

```text
src/
  app/                  // composition root, routing, providers
  features/
    promotions/         // self-contained feature
      api/              // calls to backend
      components/
      hooks/
      types.ts
      index.ts          // barrel: only public API of feature
    pricing/
      ...
  shared/
    ui/                 // design system primitives
    lib/                // utilities
    api/                // shared API client setup
    styles/
  test/
```

Rules: features may NOT import from each other directly. They can both import from shared/. Cross-feature interaction goes through routes or app-level state. Lint rules enforce this (eslint-plugin-boundaries).

#### Design System

- Atomic design: atoms (Button, Input) -> molecules (FormField) -> organisms (LoginForm) -> templates -> pages.
- Headless component libraries (Radix, Headless UI) + your own styling. Best of both: accessibility + brand.
- Tokens: colors, spacing, typography as design tokens, consumed by both Figma and code.
- Storybook for components in isolation. Visual regression with Chromatic / Playwright.

#### API Layer

- Type-safe API clients: generate from OpenAPI (orval, openapi-typescript-codegen) or use tRPC for full-stack TS.
- Wrap in TanStack Query hooks per endpoint.
- Centralize auth, retries, error normalization in a single fetch wrapper.

#### Routing

- React Router or Next.js routing. Route-based code splitting standard.
- Protected routes: a wrapper that checks auth + redirects.
- Loaders / data routers (RR v6.4+) move data fetching out of components.

### 9.6 Frontend Security Essentials

- Never store secrets in frontend code. The browser is hostile.
- Tokens in HttpOnly cookies (CSRF-safe with SameSite=Lax + double-submit) or short-lived in memory; never localStorage if sensitive.
- Content Security Policy (CSP) headers — major XSS mitigation.
- Sanitize anything rendered as HTML (dangerouslySetInnerHTML); prefer DOMPurify.
- Subresource Integrity (SRI) for third-party scripts.
- Lock down CORS server-side.

> **Interview Tip:** When asked 'how do you store JWTs in your SPA?', the architect answer is: 'It depends on threat model. For high-security apps, BFF pattern: HttpOnly cookies, no token in JS at all. For lower-risk SPAs talking to public APIs, in-memory access tokens with refresh via HttpOnly refresh cookie. localStorage is the wrong default — it's XSS-vulnerable.'

## 10. Security & DevOps

### 10.1 OAuth 2.0 & OIDC

OAuth 2.0 is for delegated authorization. OpenID Connect (OIDC) is the identity layer on top — it adds an ID token and standardizes user info. Together they're the basis of nearly all modern auth.

#### Roles in OAuth

| Role | Meaning |
| --- | --- |
| **Resource Owner** | The user. |
| **Client** | The application requesting access (your SPA, mobile app, daemon). |
| **Authorization Server** | Issues tokens after authenticating user (Entra ID, Auth0, Okta). |
| **Resource Server** | The API the client wants to call. |

#### The Flows You Need to Know

| Flow | When | Notes |
| --- | --- | --- |
| **Authorization Code + PKCE** | SPAs, mobile apps, native. | The default for everything user-facing in 2026. |
| **Client Credentials** | Service-to-service, no user. | Daemon auth. Requires client secret OR certificate. |
| **Device Code** | Devices without a browser (TVs, CLI). | User authenticates on a phone, code links them. |
| **Implicit** | Legacy SPA flow. | Deprecated. Don't use. Replaced by Auth Code + PKCE. |
| **Resource Owner Password** | Legacy. | Deprecated. Don't use ever. |

#### Authorization Code + PKCE Step-by-Step

1. SPA generates code_verifier (random) and code_challenge = SHA256(verifier).
2. SPA redirects browser to Authorization Server with code_challenge.
3. User logs in. AS redirects back to SPA with an authorization code.
4. SPA POSTs to AS token endpoint: { code, code_verifier }.
5. AS verifies code_verifier hashes to the original code_challenge.
6. AS returns: access_token (short-lived), id_token (OIDC), refresh_token (optional).
7. SPA calls API with: Authorization: Bearer \<access_token\>.

PKCE protects against authorization code interception (e.g., a malicious app on the same OS catching the redirect). Code without verifier is useless.

#### Tokens

| Token | Purpose |
| --- | --- |
| **Access Token** | Short-lived (mins to ~1 hour). Sent to API. Usually JWT. |
| **ID Token** | OIDC. Contains user info (sub, email, name). Consumed by client only. |
| **Refresh Token** | Long-lived. Used to get a new access token without re-auth. Must be stored securely. |

### 10.2 JWT Internals

JSON Web Token. Three Base64URL parts: Header.Payload.Signature.

```text
Header: { "alg": "RS256", "typ": "JWT", "kid": "abc123" }

Payload: { "sub": "user42", "aud": "api://orders",
           "iss": "https://login.microsoftonline.com/...",
           "exp": 1715200000, "scope": "orders.read" }

Signature: RSA-SHA256(base64(Header) + "." + base64(Payload), privateKey)
```

#### Validation — The Required Steps

- Verify signature using the AS's public key (fetched from JWKS endpoint, cached, refreshed on key rotation).
- Verify 'iss' (issuer): must match expected AS.
- Verify 'aud' (audience): must include your API's identifier.
- Verify 'exp' (expiration): must be future.
- Verify 'nbf' (not before): must be past.
- If sensitive operation, verify scopes/roles claims include what's needed.

> **Watch Out:** 'alg=none' attacks: if you accept the alg from the header without checking it's allowed, attackers can forge unsigned tokens. Always validate alg server-side against an allowlist.

#### Signing — RS256 vs HS256

- HS256 (HMAC + secret): symmetric. Same secret signs and verifies. Don't use across trust boundaries.
- RS256 (RSA + private/public): asymmetric. AS signs with private key; everyone verifies with public key. Standard for distributed systems.
- ES256 (ECDSA): asymmetric, smaller signatures. Increasingly common.

#### Encryption vs Signing

JWTs are signed by default, NOT encrypted. The payload is readable by anyone. For sensitive data: use JWE (encrypted JWT) or just don't put sensitive data in the token.

### 10.3 Threat Modeling — STRIDE

STRIDE is a Microsoft-developed mnemonic for systematic threat enumeration. Walk it for every architecture you design.

| Threat | Property violated |
| --- | --- |
| **Spoofing** | Authentication |
| **Tampering** | Integrity |
| **Repudiation** | Non-repudiation |
| **Information disclosure** | Confidentiality |
| **Denial of Service** | Availability |
| **Elevation of Privilege** | Authorization |

#### Threat Modeling Process

- Diagram the system (data flows, trust boundaries).
- Walk each component; for each STRIDE category, ask: 'how could this happen here?'
- Score by likelihood + impact.
- Mitigate, accept, transfer, or avoid.
- Document in a Threat Model doc, kept with the architecture.

> **Interview Tip:** 'Yes, we did STRIDE on this. The biggest unmitigated risk was repudiation in the audit trail; we addressed it by writing signed audit events to an immutable log.' Architect-grade.

### 10.4 CI/CD Pipelines

#### Pipeline Stages — The Standard Shape

1. Build -> compile, run unit tests, build container image
2. Scan -> static analysis, dependency CVE scan, container scan
3. Push -> push image to ACR with semver + git-sha tag
4. Deploy Dev -> deploy to dev, run smoke tests
5. Deploy QA -> manual or automatic gate; integration tests
6. Deploy UAT -> business approval gate
7. Deploy Prod -> canary -> wait -> full; or blue/green

#### Deployment Strategies

| Strategy | How / when |
| --- | --- |
| **Recreate** | Stop old, start new. Downtime. Only for non-critical. |
| **Rolling** | Replace pods one batch at a time. Default in K8s. Some risk if new version is bad. |
| **Blue/Green** | Two full environments. Switch traffic via LB. Instant rollback. Higher cost (2x infra). |
| **Canary** | Send small % of traffic to new version. Monitor. Gradually ramp. |
| **Feature flags** | Deploy code dark; turn on for selected users/percentages. Decouples deploy from release. |

#### GitHub Actions Sketch

```text
name: Deploy
on:
  push:
    branches: [main]
jobs:
  build:
    runs-on: ubuntu-latest
    permissions: { id-token: write, contents: read } # OIDC auth to Azure
    steps:
      - uses: actions/checkout@v4
      - run: dotnet test --logger trx
      - uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUB_ID }}
      - run: az acr build --registry myacr -t orders:${{ github.sha }} .
      - run: az containerapp update -n orders -g rg --image myacr.azurecr.io/orders:${{ github.sha }}
```

> **Architect Insight:** Use OIDC federated credentials from GitHub/ADO to Azure — no service principal secrets to rotate, full audit, scoped per repo. This is the modern best practice.

### 10.5 Infrastructure as Code (IaC) & GitOps

#### Bicep — Azure-Native IaC

Domain-specific language for Azure Resource Manager. Cleaner than ARM JSON. Compiles to ARM.

```text
param location string = resourceGroup().location
param appName string

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${appName}-plan'
  location: location
  sku: { name: 'P1v3', tier: 'PremiumV3' }
}

resource app 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  identity: { type: 'SystemAssigned' }
  properties: { serverFarmId: plan.id, httpsOnly: true }
}

output appUrl string = 'https://${app.properties.defaultHostName}'
```

Modules: factor reusable units (vnet, aks, sql) into Bicep modules. Compose at the environment level.

#### Terraform

Multi-cloud IaC. Stronger ecosystem; remote state management; provider for almost everything. Pick over Bicep when multi-cloud or your team already uses it.

#### GitOps

Git is the source of truth for desired state. An agent in the cluster (Flux, Argo CD) reconciles actual state to match. No 'kubectl apply' from laptops.

- Pull-based: cluster pulls; firewall-friendly.
- Audit: every change is a git commit.
- Drift correction: out-of-band changes get reverted.
- Standard structure: app repos -> generate manifests -> commit to a config repo -> Flux/Argo applies.

> **Interview Tip:** 'We do GitOps with Flux: app repos build images, Flux reconciles manifests in the config repo to AKS. Out-of-band kubectl edits get reverted within seconds.' This sentence wins points fast.

#### DevOps Maturity Levels (DORA)

DORA's four key metrics — used industry-wide to benchmark teams:

| Metric | Elite teams |
| --- | --- |
| **Deployment Frequency** | On-demand (multiple deploys per day) |
| **Lead Time for Changes** | \< 1 hour from commit to prod |
| **Change Failure Rate** | 0-15% |
| **Mean Time to Recovery (MTTR)** | \< 1 hour |

In interviews, knowing your team's DORA numbers (or a credible estimate) signals operational maturity.

## Appendix A — STAR Story Bank (Tied to Your Resume)

These are templates personalized to your Kroger and Kraft Heinz experience. Fill in specifics, then practice each in 90s and 4-min versions.

### Story 1 — VB6 to React/.NET Migration (Strangler Fig)

Situation: At Kroger, the Sales Promotion system was a 15-year-old VB6 monolith. Pricing team needed faster turnaround on new promo types; the codebase blocked that.

Task: As senior engineer / informal architect on the team, I owned designing the migration approach without disrupting ongoing promotion launches.

Action: I proposed a Strangler Fig migration. Key decisions: (1) introduced an API gateway in front of VB6, routing reads/writes; (2) identified 'promotion eligibility' as the highest-value, lowest-coupling leaf to migrate first; (3) built the new service on .NET / React, with an Anti-Corruption Layer translating between the legacy COM types and our clean domain; (4) migrated bounded contexts incrementally over [N] months.

Result: Cut new-promo development time from [X weeks] to [Y days]. Zero downtime during cutover. Old system fully decommissioned by [date].

Reflection: I'd push harder earlier on the data ownership boundaries. We dual-wrote longer than necessary because the data split lagged the service split.

### Story 2 — Microservices Communication Decision

Situation: Within the new architecture, we had 5 services that needed to coordinate around order/promotion application.

Task: Decide sync vs async communication patterns and defend the choice in design review.

Action: I drew the call graph and found a 4-deep synchronous chain that would compound latency and failure. Proposed switching three of those edges to event-driven via Service Bus topics, leaving only the user-facing read path synchronous. Wrote ADR-002 documenting the trade-off.

Result: p99 dropped from [N]ms to [M]ms. Service availability went from 99.5% (product) to 99.9% per service. Failure of one service no longer cascaded.

Reflection: Should have introduced the outbox pattern from day one rather than a sprint later — we lost a few events during early prod.

### Story 3 — Production Incident Ownership

Situation: [Concrete incident: e.g., cache stampede during a Black Friday flash sale; or a deployment that caused a 30-minute outage].

Task: I was on-call and led the response.

Action: Triaged via App Insights — saw dependency latency spike on the pricing service. Rolled back the latest deploy. Confirmed customer impact resolved. Then ran a blameless RCA: root cause was a missing connection pool config after a refactor.

Result: Customer-facing impact contained to [X] minutes. RCA delivered within 24 hours. Permanent fix shipped within a week including (1) a configuration test, (2) a load test in CI, (3) an SLO alert that would have caught it pre-rollout.

Reflection: We had observability but not the right alerts. I now treat 'what alert would have caught this?' as a mandatory RCA question.

### Story 4 — Influencing Without Authority

Situation: A peer team was about to spin up a new microservice that duplicated capability we already had. Politics made a direct 'no' impractical.

Task: Steer the decision toward reuse without escalating.

Action: Set up a working session, brought a comparison doc with TCO estimates, code, and ADR. Showed how their use case fit ours with a small extension. Offered to pair on the integration.

Result: Avoided ~6 months of duplicate build. Built a stronger relationship with the peer team that paid off twice in the next year.

Reflection: I waited too long to engage. Earlier visibility into their roadmap would have caught this in planning.

### Story 5 — Mentoring a Senior Engineer

Situation: A senior engineer on my team was technically strong but kept getting stuck on architectural choices, deferring to me each time.

Task: Get them comfortable making and defending architectural decisions independently.

Action: Started using ADR templates as a teaching tool. For each decision, asked them to draft the ADR — context, options, trade-offs, decision. Reviewed asynchronously. After ~5 ADRs, they were leading design reviews.

Result: They were promoted to Staff within a year. The team's architecture review throughput doubled because I was no longer the bottleneck.

Reflection: I should have introduced ADRs across the full team, not just one person. Did that as the next step.

### Story 6 — Architectural Mistake / Regret

Situation: Early in [project], I picked Cosmos DB for a use case that turned out to need rich joins and ad-hoc queries.

Task: Recognize the mismatch and own the correction.

Action: Within 3 months, the smell was clear — query patterns were forcing us to denormalize aggressively, and ad-hoc reporting was painful. I wrote an ADR proposing migration of the analytical workload to Azure SQL while keeping operational reads in Cosmos. CQRS-style split.

Result: Reporting velocity returned to normal. Operational latency stayed strong. Total migration cost was ~[N] sprints — recoverable.

Reflection: I'd picked Cosmos for the geo-distribution buzzword without validating against actual query patterns. Now I always interview the BI/reporting needs explicitly before picking a primary store.

### Story 7 — Hard Tech Debate

Situation: Disagreement with another senior engineer on Redux vs TanStack Query for our React state layer.

Task: Reach a defensible team decision without bad blood.

Action: Set up a 30-min spike: I'd build the same feature with each approach. Both of us reviewed the result with the team, weighing maintainability, learning curve, server-state concerns. Picked TanStack Query for server state, Redux Toolkit for the small client-state we actually had.

Result: Team aligned on the split. Reduced state-related bugs notably; engineer onboarding time on the codebase dropped because data fetching followed one consistent pattern.

Reflection: The spike was the unlock. Talking abstractly was getting nowhere; concrete code surfaced real differences.

### Story 8 — Performance Win

Situation: Promotion search endpoint was averaging [N]ms p99 — too slow for the checkout integration.

Task: Reduce p99 below 200ms.

Action: Profiled with App Insights. Top contributor was N+1 queries. Added a query-side projection in Cosmos populated by Change Feed. Added Redis cache-aside on the most-hit keys with TTL aligned to promo update cadence.

Result: p99 dropped to [M]ms. Cosmos RU consumption dropped [X]% during peak. ROI clear within first sale event.

Reflection: Should have set the SLO before designing v1. We rebuilt instead of designing for the requirement up-front.

### Story 9 — Saying No to a Stakeholder

Situation: Product wanted a 2-week 'quick win' that bypassed our authentication on a 'just internal' API.

Task: Push back without becoming the 'no' person.

Action: Acknowledged the urgency. Walked through the risk: 'internal' systems get exposed via accident or audit; we'd carry a SOC2 finding. Proposed a 1-week version with a basic JWT check using existing identity, deferring the richer authz model. Got agreement from product and security in one call.

Result: Shipped on time with auth in place. SOC2 audit had no finding.

Reflection: 'No' alone is a junior move. 'Yes, with this safer alternative on the same timeline' is the architect move.

### Story 10 — Leading Change (Process)

Situation: Team had no consistent design review process; designs slipped to coding without scrutiny, then we paid in rework.

Task: Introduce a lightweight architecture review without bureaucracy.

Action: Proposed: (1) every change above some size needs an ADR; (2) ADRs reviewed in a weekly 30-min architecture review; (3) decisions documented and merged. Started small with my team.

Result: Rework dropped notably. Within 2 quarters, two other teams adopted the format.

Reflection: Started with a too-heavy template. Pruning it to a 1-page ADR was the unlock to adoption.

## Appendix B — Azure Service-by-Use-Case Cheat Sheet

### Compute

| Need | Pick |
| --- | --- |
| **Stateless API, simple deploy** | App Service |
| **Event-driven, short-running** | Functions (Consumption / Flex Consumption) |
| **Containers without K8s** | Container Apps |
| **Microservices fleet (many services)** | AKS |
| **Lift-and-shift legacy** | Azure VMs |
| **Workflow with state (durable)** | Durable Functions / Logic Apps |

### Data

| Need | Pick |
| --- | --- |
| **Single-region OLTP, relational** | Azure SQL DB |
| **Lift-and-shift SQL Server** | Azure SQL Managed Instance |
| **Geo-distributed NoSQL with SLA** | Cosmos DB |
| **High-scale key-value, cheap** | Azure Storage Tables |
| **Files, images, media** | Storage Blobs |
| **Distributed cache, leaderboards** | Cache for Redis |
| **Analytics / data warehouse** | Synapse / Microsoft Fabric |
| **Search** | Azure AI Search (formerly Cognitive Search) |

### Messaging

| Need | Pick |
| --- | --- |
| **Transactional messaging, FIFO per key** | Service Bus (with sessions) |
| **Pub-sub events from Azure resources** | Event Grid |
| **High-volume telemetry / event streaming** | Event Hubs |
| **Cheap simple queue, large messages OK** | Storage Queue |
| **Kafka ecosystem requirement** | Event Hubs (Kafka surface) or HDInsight Kafka / Confluent on Azure |

### Networking & Edge

| Need | Pick |
| --- | --- |
| **Global L7 + WAF + CDN** | Front Door |
| **Regional L7 + WAF in VNet** | Application Gateway |
| **Global DNS-based routing only** | Traffic Manager |
| **Internal regional L4** | Azure Load Balancer |
| **API gateway with policy + dev portal** | API Management |
| **VNet-to-VNet / on-prem connectivity** | VPN Gateway / ExpressRoute |

### Identity & Security

| Need | Pick |
| --- | --- |
| **User identity + OIDC** | Microsoft Entra ID |
| **Customer identity (B2C)** | Entra External ID (formerly Azure AD B2C) |
| **Secrets / keys / certs** | Key Vault |
| **Resource-to-resource auth** | Managed Identity |
| **RBAC for Azure resources** | Azure RBAC |
| **WAF** | Front Door WAF / App Gateway WAF |
| **DDoS Standard** | Network Protection plan |
| **Posture management + workload protection** | Defender for Cloud |

### Observability

| Need | Pick |
| --- | --- |
| **App-level telemetry + traces** | Application Insights |
| **Logs from any Azure resource** | Diagnostic Settings -> Log Analytics |
| **Metrics + alerts** | Azure Monitor Metrics + Alerts |
| **Dashboards** | Azure Monitor Workbooks / Grafana on Azure |
| **Security signals correlation** | Microsoft Sentinel |

## Appendix C — System Design Interview Rubric

Use this to score yourself after every mock. Score 1-5 on each. Anything 3 or below is a focus area.

| Dimension | What 5/5 looks like |
| --- | --- |
| **Requirements clarification** | Asks 5+ clarifying questions covering FRs, NFRs, scale, constraints. Doesn't dive in until done. |
| **Capacity estimation** | Back-of-envelope: users, RPS, storage, bandwidth. Numbers feel grounded. |
| **High-level diagram** | Clean component diagram: client, gateway, services, data stores, queues, caches. Within 5 minutes. |
| **API design** | Defined endpoints with key request/response shapes; versioning; idempotency where it matters. |
| **Data model** | Picked store with justification. Partition strategy for scaled stores. Indexes mentioned. |
| **Scaling strategy** | Where bottlenecks emerge; horizontal scale; caching; sharding; read replicas. |
| **Failure modes** | What happens when each component fails; retries, circuit breaker, fallback, DLQ. |
| **Security** | AuthN/Z addressed; sensitive data encrypted; private network where appropriate. |
| **Cost awareness** | Mentions cost levers; doesn't over-engineer. |
| **Trade-off articulation** | Names alternatives explicitly and why they were rejected. Architect-grade. |
| **Communication** | Clear, structured, doesn't hand-wave. Asks for feedback. Iterates with the interviewer. |

## Appendix D — Glossary of Architect Vocabulary

| Term | Definition |
| --- | --- |
| **ACL (Anti-Corruption Layer)** | Translation boundary protecting your domain from external models. |
| **ADR (Architecture Decision Record)** | Short doc capturing one decision: context, options, decision, consequences. |
| **AKS** | Azure Kubernetes Service. |
| **APIM** | Azure API Management. |
| **Aggregate (DDD)** | Cluster of domain objects treated as a unit; has a root. |
| **BFF** | Backend For Frontend; per-client API layer. |
| **Bounded Context** | Boundary inside which a model is internally consistent (DDD). |
| **CAP** | Consistency-Availability-Partition; choose 2 during a partition. |
| **CQRS** | Command Query Responsibility Segregation. |
| **Canary** | Deployment strategy: send small % of traffic to new version first. |
| **Circuit Breaker** | Fail fast after repeated failures to protect downstream. |
| **Compensating Transaction** | Semantic undo of a previously completed saga step. |
| **Cosmos DB** | Azure's globally distributed NoSQL with SLAs. |
| **DLQ** | Dead-Letter Queue; messages that failed processing repeatedly. |
| **DDD** | Domain-Driven Design. |
| **DIP** | Dependency Inversion Principle. |
| **Event Sourcing** | Storing the sequence of events instead of current state. |
| **Hexagonal Architecture** | Domain core + ports + adapters; same as Ports & Adapters. |
| **IaC** | Infrastructure as Code. |
| **Idempotency** | Operation produces same effect whether run once or N times. |
| **JWT** | JSON Web Token. Header.Payload.Signature. |
| **KQL** | Kusto Query Language; used by Azure Monitor / App Insights. |
| **Managed Identity** | Azure-managed service principal tied to a resource. No secrets. |
| **MTTR** | Mean Time To Recovery. |
| **NFR** | Non-Functional Requirement (perf, availability, security, etc). |
| **OIDC** | OpenID Connect; identity layer over OAuth 2.0. |
| **Outbox Pattern** | Reliably publish messages by writing to a DB outbox in same tx. |
| **PACELC** | CAP extended: Partition then C-or-A, Else Latency-or-Consistency. |
| **PKCE** | Proof Key for Code Exchange; protects authorization code flow. |
| **RPO** | Recovery Point Objective; max data loss tolerated. |
| **RTO** | Recovery Time Objective; max downtime tolerated. |
| **RU/s (Cosmos)** | Request Units per second; throughput currency in Cosmos. |
| **Saga** | Pattern for long-running distributed transactions via local txns + compensations. |
| **SLI / SLO / SLA** | Indicator (measure), Objective (target), Agreement (contract). |
| **SOLID** | 5 OO principles: SRP, OCP, LSP, ISP, DIP. |
| **STRIDE** | Microsoft threat model: Spoofing, Tampering, Repudiation, InfoDisclosure, DoS, ElevationOfPrivilege. |
| **Strangler Fig** | Migration pattern: gradually replace legacy by routing through a facade. |
| **WAF** | Web Application Firewall. |
| **Zero Trust** | Never trust the network; authenticate every call. |
