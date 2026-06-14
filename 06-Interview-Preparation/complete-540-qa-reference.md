> Multi-topic Q&A bank (C#, .NET, SQL, EF Core, LINQ, Microservices, React, Azure, Design Patterns, ML.NET), basics → advanced.

# Complete Q&A Reference — 540+ Questions & Answers

**540+ Questions & Answers**

*C# | .NET | SQL | EF Core | LINQ | Microservices | React | Azure | Design Patterns | ML.NET*

**Basics → Intermediate → Advanced**

Real-time, practical examples for interview and reference

## How to Use This Document

This document contains 540+ technical Q&As organized by topic, basics-to-advanced. Use it three ways:

- **Daily reference:** Look up specific concepts when you need them.
- **Interview prep:** Read each section in order. Try to answer before reading.
- **Learning path:** Cover one section per week. Practice the code examples.

## Table of Contents

- **[Section 1: C# Fundamentals](#section-1--c-fundamentals-q1-100)** (Q1-100)
  - *Basics, OOP, generics, async, LINQ, modern C# features*
- **[Section 2: .NET / ASP.NET Core](#section-2--net--aspnet-core-q101-180)** (Q101-180)
  - *DI, middleware, hosting, configuration, minimal APIs, modern .NET 8/9*
- **[Section 3: SQL & Entity Framework Core](#section-3--sql--entity-framework-core-q181-280)** (Q181-280)
  - *Queries, joins, indexes, transactions, EF Core, migrations, performance*
- **[Section 4: Microservices & React](#section-4--microservices-q281-340)** (Q281-380)
  - *Bounded contexts, sagas, idempotency, communication; React hooks, state, performance*
- **[Section 5: Azure](#section-5--azure-q381-450)** (Q381-450)
  - *Compute, storage, databases, identity, networking, messaging*
- **[Section 6: Design Patterns](#section-6--design-patterns-q451-510)** (Q451-510)
  - *Creational, structural, behavioral, enterprise, SOLID*
- **[Section 7: ML.NET](#section-7--mlnet-q511-540)** (Q511-540)
  - *Training, evaluation, deployment, MLOps*

## Legend

- **Q##.** = Question with number. Try to answer before reading.
- **A.** = Answer.
- Code blocks = practical, copy-pasteable examples.

## SECTION 1 — C# FUNDAMENTALS (Q1-100)

### 1.1 Basics

**Q1.** What is C#?

C# is a strongly-typed, object-oriented programming language developed by Microsoft. Runs on the .NET runtime. Used for web apps, desktop apps, mobile, games (Unity), cloud services.

**Q2.** What is .NET?

A free, cross-platform, open-source developer platform for building many kinds of applications. Includes the runtime (CLR), libraries (BCL), and languages (C#, F#, VB.NET). Latest is .NET 9 (2024).

**Q3.** What's the difference between .NET Framework and .NET (Core/5+)?

.NET Framework: Windows-only, Mature (2002), maintenance only. .NET Core/5+: Cross-platform (Windows, Linux, macOS), open source, modern, actively developed. Always pick .NET 8+ for new projects.

**Q4.** What is CLR?

Common Language Runtime. The execution engine that runs .NET code. Provides JIT compilation, garbage collection, type safety, exception handling, security.

**Q5.** What is IL (Intermediate Language)?

When you compile C#, it doesn't produce machine code directly. It produces IL (also called MSIL or CIL) — a CPU-independent instruction set. CLR's JIT compiler converts IL to machine code at runtime.

**Q6.** What is JIT compilation?

Just-In-Time. CLR compiles IL to machine code as methods are first called. Result: fast startup vs ahead-of-time, but first call to a method is slower than subsequent calls. AOT (Ahead-Of-Time) compilation is also supported in .NET 7+.

**Q7.** What's the difference between value types and reference types?

Value types (struct, int, bool, DateTime, custom struct): stored on stack, copied by value. Fast.

Reference types (class, string, arrays, interfaces): stored on heap, variables hold a reference. Garbage-collected.

Performance: value types avoid GC pressure but cost CPU when copied if large. Reference types: cheap copy (just reference), but heap allocations cost.

**Q8.** Show the difference between value type and reference type.

When you assign a value type, you get a copy. When you assign a reference type, you get another reference to the same object.

```csharp
// Value type
int a = 5;
int b = a; // b is a copy
b = 10; // a is still 5
// Reference type
var list1 = new List<int> { 1, 2, 3 };
var list2 = list1; // list2 references same object
list2.Add(4); // list1 also sees 4 now
```

**Q9.** What is boxing and unboxing?

Boxing: converting a value type to object (or interface) — wraps the value on the heap. Unboxing: extracting the value back. Both have performance costs. Avoid in hot paths. Generics largely eliminate the need.

**Q10.** What's the difference between == and .Equals()?

For value types: both compare values.

For reference types by default: == compares references, .Equals() also compares references unless overridden.

For string: both compare values (string overrides both).

For your classes: override .Equals() and == for value-style comparison.

**Q11.** What's the difference between string and StringBuilder?

string is immutable — every modification creates a new instance. StringBuilder is mutable — modifies in place. Use StringBuilder for many concatenations (loops, building large strings). For 2-3 concatenations, regular string is clearer.

**Q12.** Show when to use StringBuilder.

When concatenating in a loop or 5+ times, StringBuilder avoids allocating intermediate strings.

```csharp
// BAD - allocates 1000 intermediate strings
string s = "";
for (int i = 0; i < 1000; i++) s += i;
// GOOD - single mutable buffer
var sb = new StringBuilder();
for (int i = 0; i < 1000; i++) sb.Append(i);
string result = sb.ToString();
```

**Q13.** What is an interface in C#?

A contract — defines methods/properties that implementing classes MUST provide. No implementation in interfaces (until C# 8 default interface methods). A class can implement multiple interfaces.

**Q14.** What's the difference between interface and abstract class?

Abstract class: can have implementation, fields, constructors. Single inheritance. For 'is-a' with shared state.

Interface: contract only (mostly). Multiple implementation. For 'can-do' capabilities.

Rule: prefer interfaces unless you genuinely need shared state or implementation.

**Q15.** What is polymorphism?

Same interface, different implementations. virtual methods overridden in derived classes. Same call, different behavior at runtime. Foundation of OOP.

**Q16.** Show polymorphism.

Animal is the base. Each animal speaks differently.

```csharp
public abstract class Animal { public abstract string Speak(); }
public class Dog : Animal { public override string Speak() => "Woof"; }
public class Cat : Animal { public override string Speak() => "Meow"; }
Animal a = new Dog();
Console.WriteLine(a.Speak()); // "Woof" - resolved at runtime
```

**Q17.** What is inheritance?

A class derives from a base class, gaining its members. Subclass extends or modifies parent behavior. C# supports single inheritance for classes (multiple for interfaces).

**Q18.** What is encapsulation?

Hiding internal state and exposing controlled access. Use private fields with public properties. The class controls how its data is read/modified.

**Q19.** What is abstraction?

Showing only essential features, hiding complexity. Interfaces and abstract classes enable abstraction — clients depend on the contract, not the implementation.

**Q20.** What's the difference between virtual and abstract methods?

virtual: has implementation in base class, can be overridden. abstract: NO implementation, MUST be overridden by non-abstract derived classes.

**Q21.** What is the 'sealed' keyword?

Prevents inheritance. sealed class can't be derived from. sealed method (only on override) prevents further overriding. Performance benefit: JIT can devirtualize.

**Q22.** What's the difference between override and new keyword?

override: replaces virtual base method using polymorphism. new: hides base method, NOT polymorphic — base reference calls base method, derived reference calls derived method.

**Q23.** What is method overloading vs overriding?

Overloading: same method name, different parameters in same class. Compile-time. Overriding: derived class replaces virtual base method. Runtime polymorphism.

**Q24.** What are access modifiers?

public: any code anywhere.

private: only within the same class.

protected: same class + derived classes.

internal: same assembly only.

protected internal: same assembly OR derived class.

private protected: same assembly AND derived class.

file (C# 11+): same source file.

**Q25.** What's a constructor?

Special method called when an object is created. Same name as the class, no return type. Initializes the object's state. C# generates a default parameterless constructor if none defined.

**Q26.** What's a static constructor?

Initializes static members. Runs once, before first use of the class. No parameters, no access modifier. Cannot be called explicitly. Good for one-time setup.

**Q27.** What's the 'this' keyword?

Refers to the current instance. Used to disambiguate field vs parameter, pass current instance to other methods, or chain to another constructor: this(args).

**Q28.** What's the 'base' keyword?

Refers to the parent class. Used to call base constructor: base(args), or call base method: base.Method().

**Q29.** What's a static class?

Class with only static members. Cannot be instantiated. Good for utility classes (Math, Console). Loaded once per AppDomain.

**Q30.** What's a static member?

Belongs to the class itself, not an instance. Shared across all instances. Accessed via ClassName.Member. Use sparingly — global state is hostile to tests.

**Q31.** What are properties in C#?

Methods that look like fields. Get/set syntax for reading/writing. Allow validation, lazy loading, computed values without changing API.

**Q32.** Show different property styles.

Auto-property, computed, init-only.

```csharp
// Auto-property (compiler creates backing field)
public string Name { get; set; }
// Computed property (no setter)
public string FullName => $"{First} {Last}";
// Init-only (settable only during initialization)
public string Id { get; init; }
// Required (must be set during init)
public required string Email { get; init; }
// With validation
private int _age;
public int Age
{
get => _age;
set => _age = value >= 0 ? value : throw new ArgumentException();
}
```

**Q33.** What's a record in C#?

Reference type designed for immutable data. Auto-generates Equals, GetHashCode, ToString based on properties. Records compare by value. Added in C# 9.

**Q34.** Show a record.

Records are concise immutable data types.

```csharp
public record Customer(Guid Id, string Name, string Email);
var c1 = new Customer(Guid.NewGuid(), "Alice", "a@x.com");
var c2 = c1 with { Name = "Alice Smith" }; // immutable update
// Records compare by value
var c3 = c1 with { };
bool same = c1 == c3; // true (same property values)
```

**Q35.** What's the difference between class, record, and struct?

class: reference type, mutable by default, single inheritance, default reference equality.

record: reference type (record class) or value type (record struct), value equality, immutable patterns, with-expressions.

struct: value type, no inheritance (sealed), value equality (auto), best for small (<16 byte) immutable data.

**Q36.** What's a null reference?

A reference that doesn't point to any object. Calling methods on null throws NullReferenceException. Use null checks, null-conditional operator (?.), or nullable reference types to avoid.

**Q37.** What are nullable reference types?

Compile-time feature (C# 8+, default in .NET 6+). string is non-nullable; string? allows null. Compiler warns about potential null derefs. Doesn't prevent at runtime — it's a static analysis tool.

**Q38.** Show null-conditional and null-coalescing operators.

Both help work with potential nulls cleanly.

```csharp
string name = customer?.Name; // null-conditional: null if customer is null
string display = name ?? "Unknown"; // null-coalescing: default if null
string greeting = customer?.Name ?? "Guest"; // chained
// Null-conditional with method call
int? count = list?.Count; // safe even if list is null
// Null assignment
customer.Email ??= "unknown@x.com"; // assign only if currently null
```

**Q39.** What's the difference between throw and throw ex?

throw: rethrows the original exception, preserving stack trace.

throw ex: rethrows but RESETS the stack trace from the rethrow point. Loses original origin. Almost always wrong.

**Q40.** Show proper exception handling.

Catch what you can handle. Re-throw with throw (not throw ex).

```csharp
try
{
await ProcessOrder(order);
}
catch (ValidationException ex)
{
_logger.LogWarning(ex, "Validation failed for order {OrderId}", order.Id);
return BadRequest(ex.Message);
}
catch (Exception ex) when (!(ex is OperationCanceledException))
{
_logger.LogError(ex, "Order processing failed");
throw; // preserves stack trace
}
```

**Q41.** What's a custom exception and when do you create one?

Inherit from Exception. Create when you need to represent a specific business error (OrderNotFoundException, InvalidPaymentException). Better than throwing generic Exception or string error codes.

**Q42.** What's the using statement?

Ensures Dispose() is called on IDisposable. C# 8+ has 'using declaration' (no braces). Critical for releasing resources: file handles, DB connections, HTTP clients.

**Q43.** Show using statement variants.

Both old and new syntax.

```csharp
// Classic using statement (with braces)
using (var stream = File.OpenRead(path))
{
var bytes = stream.ReadByte();
} // Dispose called here
// Using declaration (C# 8+) - disposed at end of scope
using var stream = File.OpenRead(path);
var bytes = stream.ReadByte();
// Dispose called when stream goes out of scope
```

**Q44.** What's IDisposable?

Interface with one method: Dispose(). Implement when your class holds unmanaged resources or other IDisposables. Caller calls Dispose to release deterministically (don't wait for GC).

**Q45.** What's the difference between Dispose and finalizer?

Dispose: deterministic cleanup, called explicitly or by 'using'. Fast.

Finalizer (~ClassName): non-deterministic, called by GC. Slow. Only use as a safety net for unmanaged resources.

Modern .NET rarely needs finalizers — use SafeHandle for unmanaged resources.

### 1.2 Collections

**Q46.** What's the difference between Array and List<T>?

Array: fixed size, faster in some scenarios. List<T>: dynamic, growable, internally backed by an array. Use List<T> by default.

**Q47.** What's the difference between List<T>, IList<T>, IEnumerable<T>?

IEnumerable<T>: read-only forward iteration. The most general.

ICollection<T>: adds Count, Add, Remove.

IList<T>: adds index access [].

List<T>: concrete implementation. Use specific interface in method signatures (depend on the abstraction).

**Q48.** When do I use Dictionary<TKey, TValue>?

When you need O(1) lookup by key. Hash-based. Use for caches, lookups, indexing collections. ConcurrentDictionary for thread-safe access.

**Q49.** What's HashSet<T>?

Set of unique values. O(1) Contains, Add, Remove. Use for: deduplication, fast 'is this in the set' checks. No order guarantees.

**Q50.** What's the difference between Queue<T> and Stack<T>?

Queue: FIFO (first-in-first-out). Enqueue/Dequeue. Stack: LIFO (last-in-first-out). Push/Pop. Both O(1) for adds and removes.

**Q51.** What's a SortedDictionary vs SortedList?

Both sorted by key. SortedDictionary: red-black tree, O(log n) add/remove. SortedList: backed by array, O(n) insert. Pick SortedDictionary unless you have many lookups vs few inserts.

**Q52.** What are concurrent collections?

Thread-safe collections in System.Collections.Concurrent: ConcurrentDictionary, ConcurrentQueue, ConcurrentStack, ConcurrentBag, BlockingCollection. Use when multiple threads access the same collection.

**Q53.** When should I use ImmutableList vs List?

ImmutableList: every modification returns a new instance. Use for: shared state across threads, snapshots, undo/redo. Higher cost but lock-free safety.

**Q54.** What's IEnumerable vs IEnumerator?

IEnumerable<T>: 'I can be enumerated.' Has GetEnumerator(). IEnumerator<T>: the actual iterator with MoveNext() and Current. You usually work with IEnumerable; IEnumerator is internal.

**Q55.** What's deferred execution in LINQ?

LINQ queries don't execute until you iterate. Where, Select, OrderBy etc. return IEnumerable that runs when enumerated. ToList(), ToArray(), Count(), First() force execution.

### 1.3 Memory & Performance

**Q56.** What is garbage collection?

Automatic memory management. GC reclaims memory used by unreferenced objects. .NET uses generational GC (Gen 0, 1, 2 + Large Object Heap) for performance.

**Q57.** What are GC generations?

Gen 0: new objects. Collected frequently, cheap.

Gen 1: survived Gen 0. Less frequent.

Gen 2: long-lived. Most expensive collection.

LOH: objects > 85KB. Collected with Gen 2.

Goal: most objects die in Gen 0.

**Q58.** What's a memory leak in .NET?

Holding references to objects you no longer need, preventing GC from reclaiming. Common causes: static collections growing without bound, event handlers not unsubscribed, captured lambdas extending lifetimes, long-lived DbContext.

**Q59.** What's the 'IDisposable' pattern?

Implement IDisposable when class owns resources to release. Call Dispose explicitly or via using. Implement IAsyncDisposable for async cleanup (network, DB).

**Q60.** What's Span<T>?

Stack-allocated view over contiguous memory (array, string, native). Zero-allocation slicing. Use for parsing, buffer manipulation, high-perf scenarios. Cannot be field of a class or used in async methods.

**Q61.** Show Span<T> for parsing.

Slice strings without allocating substrings.

```csharp
// Old way - allocates substrings
string fullName = "John Smith";
string firstName = fullName.Substring(0, 4);
// Span - no allocation
ReadOnlySpan<char> firstName = fullName.AsSpan(0, 4);
// Process firstName as if it were a string
```

**Q62.** What's stackalloc?

Allocate memory on the stack. C# 7.2+ supports stackalloc with Span<T>. Fast, no GC pressure. Limited to small sizes (a few KB). Use for short-lived buffers.

### 1.4 Async / Await

**Q63.** What is async/await?

Syntax for asynchronous code. await suspends the method without blocking the thread. Thread is returned to pool. When awaited operation completes, execution resumes (possibly on different thread).

**Q64.** Why use async/await?

Non-blocking I/O. Web servers handle far more concurrent requests with same threads. DB calls, HTTP, file I/O — all should be async. Without async, threads block waiting, exhausting the pool under load.

**Q65.** What's Task vs Task<T> vs ValueTask?

Task: async operation, no return value.

Task<T>: async operation returning T.

ValueTask<T>: avoids allocation when sync path is common (e.g., cache hit). Use for hot paths.

**Q66.** What's the most common async mistake?

Mixing sync and async with .Result or .Wait(). Causes deadlocks in ASP.NET legacy contexts and starves thread pool. Always 'async all the way down.'

**Q67.** Show common async pitfalls.

Three things to avoid.

```csharp
// PITFALL 1: .Result / .Wait() - can deadlock
var data = SomeAsyncMethod().Result; // BAD
// FIX:
var data = await SomeAsyncMethod();
// PITFALL 2: fire-and-forget without error handling
_ = SomeAsyncMethod(); // exceptions swallowed silently!
// FIX:
_ = Task.Run(async () => { try { await SomeAsyncMethod(); } catch (Exception ex) { _log.LogError(ex, ...); } });
// PITFALL 3: not propagating CancellationToken
public async Task<List<X>> GetAllAsync() { ... } // BAD
// FIX:
public async Task<List<X>> GetAllAsync(CancellationToken ct) { ... }
```

**Q68.** What is ConfigureAwait(false)?

Tells await NOT to capture sync context. Use in libraries to avoid deadlocks and improve perf. In ASP.NET Core there's no sync context, so ConfigureAwait(false) is mostly unnecessary in app code.

**Q69.** What's a CancellationToken?

Cooperative cancellation mechanism. Long operations check token; cancel if requested. ASP.NET Core injects one per HTTP request that fires when client disconnects. Always accept CancellationToken in async methods.

**Q70.** Show CancellationToken usage.

Always accept and propagate.

```csharp
public async Task<List<Order>> SearchAsync(string q, CancellationToken ct)
{
return await _db.Orders
.Where(o => o.Description.Contains(q))
.ToListAsync(ct); // EF respects token
}
[HttpGet]
public async Task<IActionResult> Search(string q, CancellationToken ct)
{
return Ok(await _service.SearchAsync(q, ct));
}
```

**Q71.** What's Task.WhenAll vs Task.WhenAny?

WhenAll: waits for all tasks. Returns when ALL complete (or any throws).

WhenAny: returns the first task that completes. Useful for timeouts, race conditions.

**Q72.** Show parallel async with WhenAll.

Run independent operations in parallel.

```csharp
// Sequential - 3x time
var customer = await GetCustomerAsync(id);
var orders = await GetOrdersAsync(id);
var prefs = await GetPreferencesAsync(id);
// Parallel - 1x time
var customerTask = GetCustomerAsync(id);
var ordersTask = GetOrdersAsync(id);
var prefsTask = GetPreferencesAsync(id);
await Task.WhenAll(customerTask, ordersTask, prefsTask);
var customer = customerTask.Result;
var orders = ordersTask.Result;
```

**Q73.** What's IAsyncEnumerable<T>?

Async iteration. await foreach lets you stream items as they're produced. Useful for large data sets, paginated APIs, real-time streams.

**Q74.** What's a thread vs a task?

Thread: OS-level execution unit. Heavy. Task: lightweight unit of work scheduled on the thread pool. Tasks reuse threads. Always prefer Task over Thread for async code.

**Q75.** What's the thread pool?

.NET maintains a pool of worker threads. Tasks are scheduled to run on pool threads. Avoids the cost of creating threads. async/await uses the pool transparently.

**Q76.** What's a deadlock and how do you avoid in async?

Two threads each waiting for the other's resource. In async: caused by .Result/.Wait() in sync context. Avoid by using await all the way down. Never block on async code.

**Q77.** When do I use Parallel.For vs Task.WhenAll?

Parallel.For: CPU-bound parallel work. Spawns multiple threads for parallel computation.

Task.WhenAll: I/O-bound async work. Awaits multiple async operations concurrently on a single thread.

**Q78.** What's Channel<T>?

Thread-safe producer/consumer pipeline. Modern alternative to BlockingCollection. Async-friendly. Bounded or unbounded. Use for inter-task communication.

### 1.5 LINQ

**Q79.** What is LINQ?

Language Integrated Query. Query syntax for collections, databases, XML, anywhere. Two flavors: method syntax (.Where().Select()) and query syntax (from x in xs select x). Method syntax is more common.

**Q80.** Show LINQ basics.

The most common operators.

```csharp
var orders = new List<Order> { ... };
// Filter
var pending = orders.Where(o => o.Status == "pending");
// Project (transform)
var totals = orders.Select(o => o.Total);
// Aggregate
decimal sum = orders.Sum(o => o.Total);
int count = orders.Count(o => o.Status == "pending");
// Sort
var sorted = orders.OrderBy(o => o.Date).ThenBy(o => o.Total);
// Group
var byCustomer = orders.GroupBy(o => o.CustomerId);
// Materialize (force execution)
List<Order> list = orders.Where(o => o.Total > 100).ToList();
```

**Q81.** What's Where vs Select?

Where: filters. Returns items matching predicate. Same type. Select: projects/transforms. Returns possibly different type. Both deferred.

**Q82.** What's First, FirstOrDefault, Single, SingleOrDefault?

First: returns first matching, throws if none.

FirstOrDefault: returns first matching or default (null/0).

Single: returns the only matching, throws if zero or more than one.

SingleOrDefault: returns the only matching or default, throws if more than one.

Use Single when you expect exactly one (e.g., FindById).

**Q83.** What's Any vs All vs Contains?

Any(): is the collection non-empty? Any(predicate): does any match?

All(predicate): do all match?

Contains(item): does the collection contain this item?

**Q84.** What's the difference between Where().Count() and Count(predicate)?

Functionally equivalent. Count(predicate) is slightly more concise. Both are O(n).

**Q85.** What's Skip and Take?

Pagination. Skip(N): bypass first N items. Take(N): take only first N items. Together: .Skip(page*size).Take(size). Use SkipLast/TakeLast for end-pagination.

**Q86.** Show pagination with Skip/Take.

Standard pattern.

```csharp
int page = 2; // 0-indexed
int pageSize = 20;
var items = await _db.Orders
.OrderBy(o => o.Date)
.Skip(page * pageSize)
.Take(pageSize)
.ToListAsync();
int totalCount = await _db.Orders.CountAsync();
```

**Q87.** What's GroupBy?

Groups elements by key. Returns IEnumerable<IGrouping<TKey, TElement>>. Each group has Key and is enumerable of items.

**Q88.** Show GroupBy.

Group orders by customer, count per customer.

```csharp
var summary = orders
.GroupBy(o => o.CustomerId)
.Select(g => new
{
CustomerId = g.Key,
OrderCount = g.Count(),
Total = g.Sum(o => o.Total)
});
```

**Q89.** What's Join?

SQL-like join. Combines two collections by matching keys. Less common in code (we usually use navigation properties or just iterate); useful for in-memory joins of disparate sources.

**Q90.** Show LINQ Join.

Inner join syntax.

```csharp
var results = customers.Join(
orders,
c => c.Id, // outer key
o => o.CustomerId, // inner key
(c, o) => new { c.Name, o.Total }
);
```

**Q91.** What's the difference between IEnumerable and IQueryable?

IEnumerable: in-memory iteration. LINQ to Objects. Filters/projects in memory.

IQueryable: query expression that hasn't executed. LINQ to SQL/EF. Translates to SQL, push down to database.

Critical distinction with EF — keep IQueryable as long as possible.

**Q92.** Show why IQueryable matters.

Materializing too early loads everything into memory.

```csharp
// BAD - loads ENTIRE Orders table into memory, then filters
var orders = _db.Orders.AsEnumerable().Where(o => o.Status == "pending");
// GOOD - filter happens at the database (SELECT WHERE)
var orders = await _db.Orders.Where(o => o.Status == "pending").ToListAsync();
```

**Q93.** What's Aggregate?

Reduces a sequence to a single value. Like fold/reduce. Less common — Sum, Count, Min, Max cover most cases.

**Q94.** What's Distinct?

Removes duplicates. Uses default equality. For custom equality, pass IEqualityComparer<T> or use DistinctBy (LINQ in .NET 6+) for property-based dedup.

**Q95.** What's Zip in LINQ?

Pairs elements from two sequences by position. Stops at the shorter one. Uses cases: combining parallel arrays, matching a list with its index.

**Q96.** What's the LINQ method that I should know exists but haven't used?

Chunk (.NET 6+). Splits sequence into chunks of N. Great for batching operations. orders.Chunk(100) gives groups of 100 to process in batches.

**Q97.** How do I write a LINQ query that's both readable and efficient?

Filter early (Where before Select).

Project to anonymous types or DTOs to reduce data transferred.

Avoid materializing (.ToList()) until you need the data.

For complex queries, break into named LINQ method calls or use query syntax.

**Q98.** What's the performance cost of LINQ?

Some overhead per call (allocations, virtual calls). For hot paths over large arrays, hand-written loops can be faster. For typical app code, LINQ is fine and more readable.

**Q99.** How do I debug a LINQ query?

Break it into pieces and ToList() each step to inspect.

Use .Dump() in LINQPad.

For EF, log generated SQL.

For complex projections, project to anonymous types and inspect.

**Q100.** What's a LINQ pitfall I should know?

Multiple enumeration. var x = list.Where(...) — every time you iterate x, the Where runs again. If expensive (DB call), cache with ToList(). For in-memory, multiple enumeration is usually fine.

## SECTION 2 — .NET / ASP.NET CORE (Q101-180)

### 2.1 ASP.NET Core Basics

**Q101.** What is ASP.NET Core?

Microsoft's cross-platform web framework. Successor to ASP.NET. Used to build APIs, web apps, real-time apps. Runs on Windows, Linux, macOS. Open source.

**Q102.** What's the difference between ASP.NET Web API and ASP.NET Core?

ASP.NET Web API (legacy): part of .NET Framework, Windows-only. ASP.NET Core: cross-platform, modern, faster, unified MVC + Web API. Use ASP.NET Core for everything new.

**Q103.** What is the typical structure of a .NET 8 Web API?

Program.cs (entry point + DI + middleware), Controllers folder (or Minimal APIs), Models, Services, appsettings.json. Older versions used Startup.cs separately; modern .NET 6+ uses minimal hosting in Program.cs.

**Q104.** Show a minimal Program.cs.

Modern .NET 8 web API entry point.

```csharp
var builder = WebApplication.CreateBuilder(args);
// Add services to DI
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(opts =>
opts.UseSqlServer(builder.Configuration.GetConnectionString("Db")));
builder.Services.AddScoped<IOrderService, OrderService>();
var app = builder.Build();
// Configure middleware pipeline
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

**Q105.** What's middleware in ASP.NET Core?

Components that handle HTTP requests in a pipeline. Each middleware can: do work before next, call next, do work after. Common: authentication, authorization, logging, exception handling, CORS, static files.

**Q106.** What's the order of middleware important?

Critical. UseAuthentication must come before UseAuthorization. UseExceptionHandler must come early to catch exceptions from later middleware. UseCors before any middleware that needs CORS.

**Q107.** Show custom middleware.

Logs request duration.

```csharp
public class TimingMiddleware
{
private readonly RequestDelegate _next;
private readonly ILogger<TimingMiddleware> _logger;
public TimingMiddleware(RequestDelegate next, ILogger<TimingMiddleware> logger)
{ _next = next; _logger = logger; }
public async Task InvokeAsync(HttpContext context)
{
var sw = Stopwatch.StartNew();
await _next(context);
_logger.LogInformation("{Method} {Path} took {Ms}ms",
context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds);
}
}
// Register: app.UseMiddleware<TimingMiddleware>();
```

**Q108.** What's Dependency Injection?

Pattern: provide dependencies from outside instead of creating inside. .NET has built-in DI container. Configure in builder.Services. Inject via constructor.

**Q109.** What are the three DI lifetimes?

Singleton: one instance for app lifetime. For: config, caches, stateless services.

Scoped: one instance per HTTP request. For: DbContext, business services. Most common.

Transient: new instance every time. For: lightweight stateless services.

**Q110.** Show DI registration and consumption.

Standard pattern.

```csharp
// Registration in Program.cs
builder.Services.AddSingleton<IConfig, AppConfig>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
// Consumption via constructor injection
public class OrderController(IOrderService orders, ILogger<OrderController> log)
: ControllerBase
{
[HttpPost]
public async Task<IActionResult> Create(OrderDto dto)
{
log.LogInformation("Creating order");
var result = await orders.CreateAsync(dto);
return Ok(result);
}
}
```

**Q111.** What's a captive dependency?

Singleton holding reference to Scoped/Transient. The Scoped becomes effectively Singleton (lives as long as Singleton). Compile-time check via builder.Services.BuildServiceProvider(validateScopes: true).

**Q112.** What's IOptions<T>, IOptionsSnapshot<T>, IOptionsMonitor<T>?

IOptions<T>: singleton, value at app start. Use for config that doesn't change.

IOptionsSnapshot<T>: scoped, fresh per request. Picks up reload during config reload.

IOptionsMonitor<T>: singleton with change notification. For: long-running services that need config refresh.

### 2.2 Controllers and Endpoints

**Q113.** What's a Controller in ASP.NET Core?

Class that handles HTTP requests for related endpoints. Inherits ControllerBase (API) or Controller (MVC with views). Methods are actions. Routing maps URLs to actions.

**Q114.** Show a basic API controller.

Standard CRUD pattern.

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService service) : ControllerBase
{
[HttpGet]
public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());
[HttpGet("{id:guid}")]
public async Task<IActionResult> Get(Guid id)
{
var order = await service.GetAsync(id);
return order is null ? NotFound() : Ok(order);
}
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateOrderRequest req)
{
var created = await service.CreateAsync(req);
return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
}
}
```

**Q115.** What does [ApiController] do?

Enables: automatic 400 on invalid model state, automatic [FromBody] for complex types, attribute routing required, problem details for errors. Use it on all API controllers.

**Q116.** What are the binding source attributes?

[FromRoute]: bind from URL route segment.

[FromQuery]: bind from query string.

[FromBody]: bind from request body (JSON).

[FromHeader]: bind from header.

[FromForm]: bind from form data.

[FromServices]: inject from DI directly into action.

**Q117.** What's the difference between Controllers and Minimal APIs?

Controllers: full MVC features, filters, model binding. More boilerplate. Better for large APIs.

Minimal APIs (.NET 6+): less ceremony, faster startup. Best for: small services, simple endpoints. Recently gained feature parity with Controllers.

**Q118.** Show Minimal API.

Equivalent to a controller but more concise.

```csharp
app.MapGet("/orders/{id:guid}", async (Guid id, IOrderService svc) =>
{
var order = await svc.GetAsync(id);
return order is null ? Results.NotFound() : Results.Ok(order);
});
app.MapPost("/orders", async (CreateOrderRequest req, IOrderService svc) =>
{
var created = await svc.CreateAsync(req);
return Results.Created($"/orders/{created.Id}", created);
});
```

**Q119.** How do I version an API?

Use Microsoft.AspNetCore.Mvc.Versioning. URL versioning (/api/v1/, /api/v2/) is most explicit and easiest to debug. Header versioning is cleaner URLs but harder to test.

**Q120.** How do I return different status codes?

Ok(data): 200

Created(uri, data): 201

NoContent(): 204

BadRequest(): 400

Unauthorized(): 401

Forbid(): 403

NotFound(): 404

Conflict(): 409

Problem(...): 500 with details

StatusCode(N, data): custom

**Q121.** What is action filter?

Code that runs before/after action methods. Types: AuthorizationFilter, ResourceFilter, ActionFilter, ExceptionFilter, ResultFilter. Apply via [Attribute] or globally. Use for cross-cutting: logging, auth, validation.

### 2.3 Configuration and Logging

**Q122.** How does configuration work?

IConfiguration is layered: appsettings.json + appsettings.{env}.json + env variables + command line + Azure App Configuration + Key Vault. Later sources override earlier.

**Q123.** Show configuration setup.

Standard layered config.

```csharp
// appsettings.json
{
"ConnectionStrings": {
"Database": "Server=..."
},
"OrderSettings": {
"MaxItems": 100,
"EmailFrom": "orders@x.com"
}
}
// Strongly typed config in Program.cs
builder.Services.Configure<OrderSettings>(
builder.Configuration.GetSection("OrderSettings"));
// In service
public class OrderService(IOptions<OrderSettings> opts)
{
private readonly OrderSettings _settings = opts.Value;
}
```

**Q124.** How do I store secrets in development?

User Secrets: dotnet user-secrets set Key Value. Stored outside source control in user profile. For production: environment variables, Azure Key Vault, AWS Secrets Manager.

**Q125.** What's logging in ASP.NET Core?

ILogger<T> injected via DI. Built-in providers: console, debug, EventSource, Application Insights. Structured logging supported with placeholders.

**Q126.** Show structured logging.

Use placeholders, not string interpolation.

```csharp
// BAD - string interpolation, can't be queried
_logger.LogInformation($"Order {orderId} processed for {customerId}");
// GOOD - structured, can be queried by OrderId or CustomerId in App Insights
_logger.LogInformation(
"Order {OrderId} processed for {CustomerId}",
orderId, customerId);
```

**Q127.** What are log levels?

Trace: most verbose, never in production.

Debug: dev/troubleshooting.

Information: normal flow.

Warning: unexpected but recoverable.

Error: failure of operation.

Critical: system unusable.

Configure per provider in appsettings.json.

**Q128.** What's Serilog and why might I use it?

Third-party structured logging library. Better sinks (file, Elasticsearch, Seq), enrichers, more flexibility than built-in. Common in production .NET apps.

### 2.4 Authentication & Authorization

**Q129.** What's the difference between authentication and authorization?

Authentication: who are you? (login)

Authorization: what can you do? (permissions)

**Q130.** How do I add JWT authentication?

Add Microsoft.AspNetCore.Authentication.JwtBearer. Configure with authority and audience. Add app.UseAuthentication(). For Entra ID: use Microsoft.Identity.Web.

**Q131.** Show JWT bearer setup.

Standard config for Entra ID-protected API.

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(opts =>
{
opts.Authority = "https://login.microsoftonline.com/{tenant}/v2.0";
opts.Audience = "api://orders";
opts.TokenValidationParameters = new TokenValidationParameters
{
ValidateIssuer = true,
ValidateAudience = true,
ValidateLifetime = true,
ValidateIssuerSigningKey = true
};
});
// Then in pipeline:
app.UseAuthentication();
app.UseAuthorization();
```

**Q132.** What's [Authorize] attribute?

Requires authentication on action/controller. [Authorize(Roles="Admin")] requires role. [Authorize(Policy="...")] uses policy. Apply globally to all controllers via filter.

**Q133.** What are policies?

Named authorization rules. Define once, reuse via [Authorize(Policy="...")]. Can require claims, roles, custom requirements. More flexible than role-based auth.

**Q134.** Show a custom policy.

Policy requiring specific claim.

```csharp
// Register
builder.Services.AddAuthorization(opts =>
{
opts.AddPolicy("ManagerOnly", p => p.RequireClaim("Role", "Manager"));
opts.AddPolicy("OrdersRead", p => p.RequireClaim("scope", "orders.read"));
});
// Use
[Authorize(Policy = "ManagerOnly")]
[HttpGet("sensitive")]
public IActionResult GetSensitive() => Ok();
```

**Q135.** What's CORS?

Cross-Origin Resource Sharing. Browser security blocking JS from calling APIs on different origin. Configure in API to allow specific origins.

**Q136.** Show CORS setup.

Lock down to specific origins in production.

```csharp
builder.Services.AddCors(opts =>
{
opts.AddPolicy("AllowApp", p => p
.WithOrigins("https://app.steris.com")
.AllowAnyMethod()
.AllowAnyHeader()
.AllowCredentials());
});
// In pipeline (BEFORE other middleware that uses CORS)
app.UseCors("AllowApp");
```

### 2.5 Error Handling

**Q137.** How should I handle errors in ASP.NET Core?

Use exception handling middleware. Convert exceptions to ProblemDetails (RFC 7807). Log with context. Don't leak sensitive info to clients.

**Q138.** Show global exception handler.

Modern .NET 8+ pattern using IExceptionHandler.

```csharp
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
: IExceptionHandler
{
public async ValueTask<bool> TryHandleAsync(
HttpContext ctx, Exception ex, CancellationToken ct)
{
logger.LogError(ex, "Unhandled exception");
var problem = new ProblemDetails
{
Status = StatusCodes.Status500InternalServerError,
Title = "An error occurred",
Detail = ex.Message,
Instance = ctx.Request.Path
};
ctx.Response.StatusCode = problem.Status.Value;
await ctx.Response.WriteAsJsonAsync(problem, ct);
return true;
}
}
// Register
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// In pipeline:
app.UseExceptionHandler();
```

**Q139.** What's ProblemDetails?

RFC 7807 standard for error responses. Has type, title, status, detail, instance. Consistent format across APIs. Tools (Swagger, clients) understand it.

### 2.6 Validation

**Q140.** How do I validate inputs?

Data Annotations on DTOs ([Required], [Range], [EmailAddress]) for simple cases. FluentValidation library for complex cross-field rules. [ApiController] auto-returns 400 on invalid model state.

**Q141.** Show validation with data annotations.

Built-in attributes for common rules.

```csharp
public class CreateOrderRequest
{
[Required]
[StringLength(200, MinimumLength = 1)]
public string CustomerName { get; set; }
[Range(1, 1000)]
public int Quantity { get; set; }
[Required]
[EmailAddress]
public string Email { get; set; }
[Url]
public string? CallbackUrl { get; set; } // optional
[RegularExpression(@"^\\d{5}$")]
public string ZipCode { get; set; }
}
```

**Q142.** What's FluentValidation?

Library for fluent, code-based validation rules. More expressive than data annotations. Better for complex/cross-field validation, conditional rules, async validators.

**Q143.** Show FluentValidation.

Validator class for the request.

```csharp
public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
public CreateOrderValidator()
{
RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(1000);
RuleFor(x => x.Email).NotEmpty().EmailAddress();
RuleFor(x => x.CallbackUrl).MustAsync(BeReachable)
.When(x => !string.IsNullOrEmpty(x.CallbackUrl));
}
private async Task<bool> BeReachable(string url, CancellationToken ct) => /* check */;
}
```

### 2.7 Health Checks and Background Jobs

**Q144.** What are health checks?

Endpoints that report app health. Used by load balancers, K8s, Azure to check if app is alive and ready. ASP.NET Core has built-in health check API.

**Q145.** Show health checks.

Liveness vs readiness.

```csharp
builder.Services.AddHealthChecks()
.AddSqlServer(builder.Configuration.GetConnectionString("Db"))
.AddAzureServiceBusQueue(connStr, "orders")
.AddCheck<MyCustomCheck>("custom");
// Liveness: process alive? Cheap, no external deps.
app.MapHealthChecks("/health/live",
new HealthCheckOptions { Predicate = _ => false });
// Readiness: ready to serve? Checks dependencies.
app.MapHealthChecks("/health/ready",
new HealthCheckOptions { Predicate = _ => true });
```

**Q146.** What's a hosted service / background service?

Long-running task that starts with the app. IHostedService interface, BackgroundService base class. Use for: queue consumers, scheduled jobs, cleanup tasks.

**Q147.** Show a BackgroundService.

Runs continuously, picks up work.

```csharp
public class OrderProcessorService : BackgroundService
{
private readonly IServiceProvider _sp;
public OrderProcessorService(IServiceProvider sp) => _sp = sp;
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
while (!stoppingToken.IsCancellationRequested)
{
using var scope = _sp.CreateScope();
var svc = scope.ServiceProvider.GetRequiredService<IOrderService>();
await svc.ProcessPendingAsync(stoppingToken);
await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
}
}
}
// Register: builder.Services.AddHostedService<OrderProcessorService>();
```

**Q148.** What's Hangfire / Quartz.NET?

Libraries for scheduled and recurring background jobs. Hangfire: persistent (survives restart), built-in dashboard. Quartz: full-featured but heavier. Use when you need persistence beyond a process restart.

### 2.8 HttpClient

**Q149.** How do I make HTTP calls in .NET?

Use IHttpClientFactory injected via DI. Don't 'new HttpClient()' — leaks sockets. Factory manages lifetime, supports Polly resilience, handler chains.

**Q150.** Show HttpClient setup and usage.

Typed client pattern.

```csharp
// Register
builder.Services.AddHttpClient<IPaymentClient, PaymentClient>(client =>
{
client.BaseAddress = new Uri("https://api.payment.com/");
client.DefaultRequestHeaders.Accept.Add(
new MediaTypeWithQualityHeaderValue("application/json"));
});
// Implementation
public class PaymentClient : IPaymentClient
{
private readonly HttpClient _http;
public PaymentClient(HttpClient http) => _http = http;
public async Task<PaymentResult> ChargeAsync(Money amount, CancellationToken ct)
{
var response = await _http.PostAsJsonAsync("charge", amount, ct);
response.EnsureSuccessStatusCode();
return await response.Content.ReadFromJsonAsync<PaymentResult>(ct)
?? throw new Exception("Empty response");
}
}
```

**Q151.** Why shouldn't I use 'new HttpClient()'?

HttpClient holds a socket per instance. Creating new ones leaks sockets (TIME_WAIT). Reusing one leaks DNS changes. IHttpClientFactory solves both.

**Q152.** What's Polly?

Resilience library: retry, circuit breaker, timeout, bulkhead, fallback. .NET 8+ has built-in resilience via Microsoft.Extensions.Http.Resilience (Polly under the hood).

**Q153.** Show Polly resilience pipeline.

.NET 8+ standard resilience.

```csharp
builder.Services.AddHttpClient<IPaymentClient, PaymentClient>(...)
.AddStandardResilienceHandler(opts =>
{
opts.Retry.MaxRetryAttempts = 3;
opts.Retry.UseJitter = true;
opts.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
opts.CircuitBreaker.MinimumThroughput = 5;
});
// Now every call through this client has retries, circuit breaker, timeout
// without changing PaymentClient code.
```

### 2.9 SignalR

**Q154.** What's SignalR?

Real-time bidirectional communication between server and clients. Uses WebSockets (with fallbacks). Use for: chat, live dashboards, real-time notifications, collaborative editing.

**Q155.** When do I use SignalR vs polling?

SignalR: many concurrent users, true real-time needs, server pushes data. Polling: simpler, less infra. For 'check every 30s,' polling is fine. For 'show notification immediately,' SignalR.

### 2.10 Caching

**Q156.** What caching options does ASP.NET Core have?

MemoryCache: in-process, simple, single-server only.

Distributed Cache (IDistributedCache): cross-process. Implementations: Redis, SQL Server, MemoryDistributedCache.

Output Caching: caches entire HTTP responses. .NET 7+.

Response Caching: HTTP cache headers.

**Q157.** Show MemoryCache usage.

In-process cache with TTL.

```csharp
builder.Services.AddMemoryCache();
public class ProductService(IMemoryCache cache, IProductRepository repo)
{
public async Task<Product?> GetAsync(Guid id)
{
return await cache.GetOrCreateAsync($"product:{id}", async entry =>
{
entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
return await repo.FindAsync(id);
});
}
}
```

**Q158.** What's distributed cache?

Cache shared across multiple servers. Use when you have >1 instance. Redis is the standard. Use IDistributedCache abstraction.

**Q159.** Show distributed cache with Redis.

Redis-backed cache.

```csharp
builder.Services.AddStackExchangeRedisCache(opts =>
{
opts.Configuration = builder.Configuration.GetConnectionString("Redis");
opts.InstanceName = "myapp:";
});
public class ProductService(IDistributedCache cache, IProductRepository repo)
{
public async Task<Product?> GetAsync(Guid id)
{
var key = $"product:{id}";
var cached = await cache.GetStringAsync(key);
if (cached != null) return JsonSerializer.Deserialize<Product>(cached);
var product = await repo.FindAsync(id);
if (product != null)
{
await cache.SetStringAsync(key, JsonSerializer.Serialize(product),
new DistributedCacheEntryOptions
{
AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
});
}
return product;
}
}
```

### 2.11 Performance

**Q160.** How do I profile a slow .NET app?

dotnet-counters: real-time metrics.

dotnet-trace: collect ETW traces, view in PerfView.

dotnet-dump: process dumps for offline analysis.

Application Insights Profiler: production-safe sampling.

BenchmarkDotNet: micro-benchmarks for specific code.

Visual Studio Diagnostic Tools: integrated.

**Q161.** What's the difference between Debug and Release build?

Debug: optimizations off, debug symbols included, slower. Release: optimizations on, no debug info, faster. Always benchmark in Release. Always run in production in Release.

**Q162.** What's AOT compilation?

Ahead-of-Time. Compiles IL to native code at build time, not runtime. Faster startup, smaller memory. Limitations: no runtime code generation, smaller library set. Used in mobile (Xamarin), Native AOT in .NET 7+.

**Q163.** What are Source Generators?

Compile-time code generation. Roslyn extensions that generate C# files based on attributes/syntax. Used for: serializer optimization (System.Text.Json), DI, mappers. Faster than reflection at runtime.

**Q164.** What's the difference between Task.Run and just calling async method?

Calling async directly: runs on current thread until first await.

Task.Run: queues work to thread pool, returns immediately. Use for: CPU-bound work, offloading from UI thread.

DON'T use Task.Run to make async I/O 'more async' — it just wastes a thread.

### 2.12 Testing

**Q165.** What testing frameworks are common?

xUnit: most popular for new projects.

NUnit: older, full-featured.

MSTest: Microsoft's, used in legacy.

All work fine. Pick xUnit for new code.

**Q166.** Show a basic xUnit test.

Arrange-Act-Assert pattern.

```csharp
public class OrderServiceTests
{
[Fact]
public async Task PlaceOrder_WithValidData_ReturnsConfirmation()
{
// Arrange
var mockRepo = new Mock<IOrderRepository>();
var order = new Order { Id = Guid.NewGuid(), Total = 100m };
mockRepo.Setup(r => r.SaveAsync(It.IsAny<Order>())).ReturnsAsync(order);
var service = new OrderService(mockRepo.Object);
// Act
var result = await service.PlaceOrderAsync(order);
// Assert
Assert.Equal(OrderStatus.Confirmed, result.Status);
mockRepo.Verify(r => r.SaveAsync(order), Times.Once);
}
}
```

**Q167.** What's [Fact] vs [Theory]?

[Fact]: single test case. [Theory]: parameterized test with multiple data sets via [InlineData], [MemberData], [ClassData].

**Q168.** Show a [Theory] test.

Same logic, multiple inputs.

```csharp
[Theory]
[InlineData(0, 0, 0)]
[InlineData(1, 2, 3)]
[InlineData(-1, -2, -3)]
public void Add_TwoNumbers_ReturnsSum(int a, int b, int expected)
{
Assert.Equal(expected, Calculator.Add(a, b));
}
```

**Q169.** What's mocking and why?

Replacing real dependencies with controllable fakes for unit tests. Lets you test the unit in isolation. Moq is most common in .NET; NSubstitute is a popular alternative.

**Q170.** What's the difference between Mock, Stub, Fake?

Stub: returns canned data. No assertions on calls.

Mock: stub + verifies calls were made.

Fake: simplified working implementation (in-memory DB).

**Q171.** What's WebApplicationFactory?

ASP.NET Core test infrastructure for integration tests. Spins up the app in memory. Lets you make real HTTP requests against your code. Replace specific services for testing (e.g., real DB with InMemory).

**Q172.** Show an integration test with WebApplicationFactory.

End-to-end test of the API.

```csharp
public class OrdersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
private readonly WebApplicationFactory<Program> _factory;
public OrdersApiTests(WebApplicationFactory<Program> factory) => _factory = factory;
[Fact]
public async Task GetOrder_NotFound_Returns404()
{
var client = _factory.CreateClient();
var response = await client.GetAsync($"/api/orders/{Guid.NewGuid()}");
Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
}
```

**Q173.** When do I use unit tests vs integration tests?

Unit tests: most of your tests. Test single class with mocks. Fast.

Integration tests: a few key flows. Test multiple components together. Slower.

End-to-end tests: only critical paths. Real DB, real HTTP. Slowest.

### 2.13 Modern .NET Features

**Q174.** What's a top-level statement?

C# 9+: code outside a class/method. Used in Program.cs of new projects. Cleaner for entry points.

**Q175.** What are file-scoped namespaces?

C# 10+: namespace MyApp; (no braces, no extra indent). Same scope as the file. Cleaner.

**Q176.** What's pattern matching?

Test value's type and shape. Switch expression, is patterns, property patterns. Powerful for clean conditional logic.

**Q177.** Show pattern matching examples.

Modern C# pattern matching.

```csharp
// Type pattern with property
if (obj is Order { Status: "pending", Total: > 100 } o)
{
Console.WriteLine($"Big pending order: {o.Id}");
}
// Switch expression
string description = order switch
{
{ Status: "shipped" } => "Already shipped",
{ Status: "pending", Total: > 1000 } => "Big pending",
{ Status: "pending" } => "Pending",
_ => "Unknown"
};
// Tuple patterns
var result = (status, total) switch
{
("shipped", _) => "Done",
("pending", > 1000) => "Big pending",
_ => "Other"
};
```

**Q178.** What's the 'with' expression?

C# 9+: creates a copy of a record with some properties changed. Immutable update. record c2 = c1 with { Name = "New" }.

**Q179.** What are required properties?

C# 11+: required string Email { get; init; }. Compiler ensures property is set during initialization. No need to make constructor.

**Q180.** What's a primary constructor?

C# 12+: parameters in class declaration become available everywhere in the class. public class OrderService(IOrderRepo repo, ILogger<OrderService> log) { ... }. Reduces boilerplate.

## SECTION 3 — SQL & ENTITY FRAMEWORK CORE (Q181-280)

### 3.1 SQL Basics

**Q181.** What is SQL?

Structured Query Language. Standard language for relational databases. Used to query, insert, update, delete data and define schemas.

**Q182.** What are the main SQL statement types?

DDL (Data Definition): CREATE, ALTER, DROP. Schema changes.

DML (Data Manipulation): SELECT, INSERT, UPDATE, DELETE. Data changes.

DCL (Data Control): GRANT, REVOKE. Permissions.

TCL (Transaction Control): COMMIT, ROLLBACK, SAVEPOINT.

**Q183.** Show basic SELECT.

Most fundamental SQL.

```sql
-- Select all columns
SELECT * FROM Orders;
-- Select specific columns
SELECT Id, Total, CustomerId FROM Orders;
-- Filter with WHERE
SELECT * FROM Orders WHERE Status = 'pending';
-- Sort
SELECT * FROM Orders ORDER BY OrderDate DESC;
-- Limit (SQL Server)
SELECT TOP 10 * FROM Orders ORDER BY Total DESC;
-- Limit (Postgres / MySQL)
SELECT * FROM Orders ORDER BY Total DESC LIMIT 10;
```

**Q184.** What's the difference between WHERE and HAVING?

WHERE: filters rows BEFORE aggregation. HAVING: filters AFTER aggregation. Use WHERE for column conditions, HAVING for aggregate conditions (SUM, COUNT, AVG).

**Q185.** Show WHERE vs HAVING.

Both filter, different points.

```sql
-- Customers with >5 large orders
SELECT CustomerId, COUNT(*) AS OrderCount, SUM(Total) AS GrandTotal
FROM Orders
WHERE Total > 1000 -- per-row filter (before grouping)
GROUP BY CustomerId
HAVING COUNT(*) > 5 -- aggregate filter (after grouping)
```

**Q186.** What are JOINs?

INNER JOIN: only matching rows in BOTH tables.

LEFT JOIN: all from left + matched from right (NULL if no match).

RIGHT JOIN: all from right + matched from left.

FULL OUTER JOIN: all from both.

CROSS JOIN: every row × every row (cartesian).

**Q187.** Show JOIN examples.

Most common patterns.

```sql
-- INNER: orders with customer info; orphan orders excluded
SELECT o.Id, o.Total, c.Name
FROM Orders o
INNER JOIN Customers c ON o.CustomerId = c.Id;
-- LEFT: all customers, even those with no orders
SELECT c.Name, COUNT(o.Id) AS OrderCount
FROM Customers c
LEFT JOIN Orders o ON o.CustomerId = c.Id
GROUP BY c.Name;
-- Multiple joins
SELECT o.Id, c.Name, p.Title, ol.Quantity
FROM Orders o
INNER JOIN Customers c ON o.CustomerId = c.Id
INNER JOIN OrderLines ol ON ol.OrderId = o.Id
INNER JOIN Products p ON p.Id = ol.ProductId;
```

**Q188.** What is GROUP BY?

Groups rows with same value in specified column(s). Used with aggregate functions (COUNT, SUM, AVG, MIN, MAX). Every column in SELECT must be in GROUP BY or be an aggregate.

**Q189.** Show GROUP BY with aggregates.

Summary statistics.

```sql
-- Order count and total per customer
SELECT
CustomerId,
COUNT(*) AS OrderCount,
SUM(Total) AS GrandTotal,
AVG(Total) AS AvgOrder,
MIN(Total) AS MinOrder,
MAX(Total) AS MaxOrder
FROM Orders
GROUP BY CustomerId
ORDER BY GrandTotal DESC;
```

**Q190.** What's the difference between UNION and UNION ALL?

UNION: combines results, removes duplicates. Slower (extra sort/distinct).

UNION ALL: combines results, keeps duplicates. Faster.

Use UNION ALL unless you specifically need dedup.

**Q191.** What's a subquery?

Query inside another query. Used in WHERE, FROM, SELECT. Two types: correlated (references outer) and non-correlated (independent).

**Q192.** Show subquery examples.

Different positions.

```sql
-- In WHERE
SELECT * FROM Customers
WHERE Id IN (SELECT CustomerId FROM Orders WHERE Total > 1000);
-- In FROM (derived table)
SELECT subq.CustomerId, subq.OrderCount
FROM (
SELECT CustomerId, COUNT(*) AS OrderCount
FROM Orders
GROUP BY CustomerId
) subq
WHERE subq.OrderCount > 5;
-- In SELECT (correlated)
SELECT c.Name,
(SELECT COUNT(*) FROM Orders WHERE CustomerId = c.Id) AS OrderCount
FROM Customers c;
```

**Q193.** What's a CTE (Common Table Expression)?

Named temporary result set defined in WITH clause. Improves readability for complex queries. Can be recursive (e.g., for hierarchies).

**Q194.** Show a CTE.

Cleaner than nested subqueries.

```sql
WITH BigSpenders AS (
SELECT CustomerId, SUM(Total) AS Total
FROM Orders
GROUP BY CustomerId
HAVING SUM(Total) > 10000
)
SELECT c.Name, bs.Total
FROM Customers c
INNER JOIN BigSpenders bs ON c.Id = bs.CustomerId
ORDER BY bs.Total DESC;
```

**Q195.** What's a recursive CTE?

CTE that references itself. Used for hierarchies (org charts, categories with parents), graph traversals.

**Q196.** Show a recursive CTE for hierarchy.

Walk an employee tree.

```sql
WITH EmployeeTree AS (
-- Anchor: top-level (no manager)
SELECT Id, Name, ManagerId, 1 AS Level
FROM Employees
WHERE ManagerId IS NULL
UNION ALL
-- Recursive: join to find subordinates
SELECT e.Id, e.Name, e.ManagerId, t.Level + 1
FROM Employees e
INNER JOIN EmployeeTree t ON e.ManagerId = t.Id
)
SELECT * FROM EmployeeTree ORDER BY Level, Name;
```

### 3.2 SQL Modify Statements

**Q197.** Show INSERT.

Insert single and multiple rows.

```sql
-- Single row
INSERT INTO Orders (CustomerId, Total, Status, OrderDate)
VALUES (123, 99.99, 'pending', GETUTCDATE());
-- Multiple rows
INSERT INTO Orders (CustomerId, Total, Status)
VALUES
(123, 100, 'pending'),
(456, 200, 'pending'),
(789, 300, 'shipped');
-- Insert from another table
INSERT INTO ArchivedOrders
SELECT * FROM Orders WHERE OrderDate < '2020-01-01';
```

**Q198.** Show UPDATE.

With and without joins.

```sql
-- Simple update
UPDATE Orders SET Status = 'shipped' WHERE Id = 123;
-- Multiple columns
UPDATE Orders
SET Status = 'shipped', ShippedDate = GETUTCDATE()
WHERE Status = 'pending' AND CreatedDate < DATEADD(day, -7, GETUTCDATE());
-- Update with join (SQL Server)
UPDATE o
SET o.Status = 'vip'
FROM Orders o
INNER JOIN Customers c ON o.CustomerId = c.Id
WHERE c.Tier = 'premium';
```

**Q199.** Show DELETE.

Always with WHERE in production.

```sql
-- Single
DELETE FROM Orders WHERE Id = 123;
-- With condition
DELETE FROM Orders WHERE Status = 'cancelled' AND CreatedDate < DATEADD(year, -2, GETUTCDATE());
-- ALWAYS test with SELECT first!
-- SELECT * FROM Orders WHERE Status = 'cancelled' AND CreatedDate < DATEADD(year, -2, GETUTCDATE());
```

**Q200.** What's the difference between TRUNCATE, DELETE, DROP?

DELETE: removes rows. Logged. Can be filtered. Triggers fire. Rollback-able.

TRUNCATE: removes ALL rows. Minimally logged. Faster. Resets identity. No WHERE.

DROP: removes table itself.

**Q201.** What's MERGE / UPSERT?

Atomic insert-or-update. SQL Server has MERGE statement; Postgres has INSERT ... ON CONFLICT. Useful for sync scenarios.

**Q202.** Show MERGE (SQL Server).

Update if exists, insert if not.

```sql
MERGE INTO Customers AS target
USING (SELECT @Id AS Id, @Name AS Name, @Email AS Email) AS source
ON target.Id = source.Id
WHEN MATCHED THEN
UPDATE SET Name = source.Name, Email = source.Email
WHEN NOT MATCHED THEN
INSERT (Id, Name, Email) VALUES (source.Id, source.Name, source.Email);
```

### 3.3 Indexes

**Q203.** What's an index?

Data structure (typically B-tree) speeding up reads at cost of writes and storage. Without index, queries scan the entire table. With index, database walks the tree directly to matching rows.

**Q204.** What's the difference between clustered and non-clustered indexes?

Clustered: data IS the index. One per table. Determines physical row order. Typically the primary key.

Non-clustered: separate structure pointing to rows. Many per table.

**Q205.** Show creating indexes.

Common index types.

```sql
-- Single column
CREATE INDEX IX_Orders_CustomerId ON Orders (CustomerId);
-- Composite (multiple columns)
CREATE INDEX IX_Orders_Status_Date ON Orders (Status, OrderDate);
-- Unique index
CREATE UNIQUE INDEX IX_Customers_Email ON Customers (Email);
-- Filtered index (only some rows)
CREATE INDEX IX_Orders_Pending ON Orders (CustomerId)
WHERE Status = 'pending';
-- Covering index (INCLUDE more columns to avoid lookup)
CREATE INDEX IX_Orders_Status ON Orders (Status)
INCLUDE (Total, OrderDate);
```

**Q206.** How do I know if I need an index?

Look at execution plans. SET STATISTICS IO ON or graphical plan.

Signs: Table Scan or Clustered Index Scan on selective queries.

High logical reads on small result sets.

Slow query log shows same WHERE pattern.

Query missing in sys.dm_db_missing_index_details.

**Q207.** What's the cost of indexes?

Reads on indexed columns: orders of magnitude faster. Writes: slower (each index updated). Storage: each index takes disk. Don't over-index — every index is a tax on writes.

**Q208.** What's a covering index?

Includes ALL columns needed by a query, so the database doesn't need a key lookup. Use INCLUDE clause to add columns not in the key. Massive speedup for hot queries.

**Q209.** What's the order of columns in a composite index?

Critical. Index on (A, B) helps queries filtering on A or A+B, but NOT on B alone. Put most-selective column first, or column most often filtered.

### 3.4 Performance

**Q210.** How do I make a query sargable (index-friendly)?

Don't apply functions to indexed columns: WHERE YEAR(Date) = 2026 - bad.

Use range instead: WHERE Date >= '2026-01-01' AND Date < '2027-01-01' - good.

Avoid leading wildcards in LIKE: LIKE '%abc' can't use index.

Avoid implicit conversions (e.g., comparing nvarchar column to varchar literal).

Avoid OR if possible; UNION can be faster.

**Q211.** Show sargable vs non-sargable.

Same intent, very different performance.

```sql
-- BAD: function on column - can't use index on OrderDate
SELECT * FROM Orders WHERE YEAR(OrderDate) = 2026;
-- GOOD: range search - uses index
SELECT * FROM Orders
WHERE OrderDate >= '2026-01-01' AND OrderDate < '2027-01-01';
```

**Q212.** What's an execution plan?

Database's strategy for executing a query. Shows operations: scans, seeks, joins, sorts, with estimated cost. View in SSMS via 'Include Actual Execution Plan' or SET STATISTICS IO ON.

**Q213.** What's the difference between Index Seek and Index Scan?

Index Seek: jumps to specific rows using the index. Fast.

Index Scan: reads the entire index. Slower.

Both use the index — but Seek is far better. Scan often means your WHERE isn't selective enough or doesn't use index.

**Q214.** How do I optimize a slow query?

1. Get the execution plan.

2. Look for: Table Scans, Key Lookups, missing indexes, expensive Sorts.

3. Check WHERE: is it sargable?

4. Check SELECT: only return columns you need.

5. Add indexes where missing.

6. Consider: query rewrite, denormalization, caching.

**Q215.** What's pagination and how do I do it efficiently?

OFFSET-based: OFFSET N ROWS FETCH NEXT M ROWS ONLY. Easy but slow for deep pages.

Keyset / cursor-based: WHERE Id > @LastId. Fast even at deep pages. Required for very large tables.

**Q216.** Show keyset pagination.

Better than OFFSET for large tables.

```sql
-- Page 1: get first 20
SELECT TOP 20 * FROM Orders ORDER BY Id;
-- Page 2+: use last Id from previous page
SELECT TOP 20 * FROM Orders WHERE Id > @LastId ORDER BY Id;
```

### 3.5 Transactions

**Q217.** What's a transaction and what are ACID properties?

Transaction: unit of work, all-or-nothing.

Atomicity: all operations succeed or none do.

Consistency: DB moves valid state to valid state. Constraints enforced.

Isolation: concurrent transactions don't interfere.

Durability: committed transactions survive crashes.

**Q218.** Show explicit transaction.

Multi-statement transaction.

```sql
BEGIN TRANSACTION;
BEGIN TRY
UPDATE Accounts SET Balance = Balance - 100 WHERE Id = 1;
UPDATE Accounts SET Balance = Balance + 100 WHERE Id = 2;
INSERT INTO Transfers (FromId, ToId, Amount) VALUES (1, 2, 100);
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
ROLLBACK TRANSACTION;
THROW;
END CATCH;
```

**Q219.** What are isolation levels?

Read Uncommitted: dirty reads allowed. Fastest, rare in practice.

Read Committed (default): no dirty reads. Allows non-repeatable reads.

Repeatable Read: same query returns same rows.

Serializable: strongest, slowest. No anomalies.

Snapshot (RCSI): MVCC. Readers don't block writers. Best balance.

**Q220.** What's a deadlock?

Two transactions hold locks the other needs. SQL detects and rolls back one (the 'victim'). Common cause: queries access tables in different orders.

**Q221.** How do I avoid deadlocks?

Always access tables in same order across transactions.

Keep transactions short.

Use lower isolation level when safe (RCSI).

Add indexes to make queries faster (less lock time).

Application: catch deadlock errors (1205) and retry.

**Q222.** What's row lock vs page lock vs table lock?

Row: only the affected row.

Page: 8KB containing multiple rows.

Table: whole table.

SQL Server escalates locks: many row locks → page → table. Reduces overhead, increases contention.

### 3.6 Schema Design

**Q223.** What's a primary key?

Column(s) uniquely identifying each row. NOT NULL, unique. Usually clustered index. Often surrogate (auto-increment integer or GUID).

**Q224.** What's a foreign key?

Column referencing primary key of another table. Enforces referential integrity. Prevents orphan records. Optional in some scenarios (microservices, sharding).

**Q225.** What's normalization?

Process of organizing data to reduce redundancy.

1NF: atomic values, no repeating groups.

2NF: 1NF + all non-key columns depend on entire primary key.

3NF: 2NF + no transitive dependencies.

BCNF: stricter 3NF. Most designs go to 3NF and stop.

**Q226.** When should I denormalize?

When read performance > write convenience. Reporting tables, analytics, hot dashboards. Always document the trade-off. Keep source-of-truth normalized; denormalize for specific queries.

**Q227.** What datatype should I use for money?

DECIMAL(18, 2) or DECIMAL(19, 4) for currency. NEVER float/real — binary floating point is inexact. SQL Server has 'money' type but DECIMAL is more portable.

**Q228.** What datatype for dates?

DATETIME2 (SQL Server): higher precision, recommended over DATETIME.

DATE: just date, no time.

DATETIMEOFFSET: with timezone offset. Best for events.

Always store UTC. Convert to user's timezone in app.

**Q229.** What's NVARCHAR vs VARCHAR?

VARCHAR: 1 byte per ASCII char. NVARCHAR: 2 bytes (or 4 for UTF-16 surrogate pairs), supports any Unicode. NVARCHAR is safer. Use VARCHAR only if you're sure data is ASCII and storage matters.

**Q230.** What's a surrogate key vs natural key?

Natural: meaningful business key (Email, SSN). Risk: changes over time.

Surrogate: meaningless (auto-increment, GUID). Stable, joinable, no business meaning.

Use surrogate as PK; constrain natural keys with UNIQUE.

**Q231.** When do I use Guid vs int as primary key?

Int (or BigInt): smaller, faster joins, sequential.

Guid: globally unique. Generate without DB roundtrip. Better for distributed systems.

Sequential GUIDs (newsequentialid in SQL Server) avoid index fragmentation.

### 3.7 Stored Procedures and Functions

**Q232.** What's a stored procedure?

Pre-compiled SQL routine stored in DB, called by name. Can have parameters, control flow, transactions. Faster (cached plan), more secure (grant EXEC without table access).

**Q233.** Show a stored procedure.

Standard pattern.

```sql
CREATE PROCEDURE GetOrdersByCustomer
@CustomerId INT,
@Status VARCHAR(20) = NULL
AS
BEGIN
SET NOCOUNT ON;
SELECT * FROM Orders
WHERE CustomerId = @CustomerId
AND (@Status IS NULL OR Status = @Status);
END;
-- Call:
EXEC GetOrdersByCustomer @CustomerId = 123;
EXEC GetOrdersByCustomer @CustomerId = 123, @Status = 'pending';
```

**Q234.** What's a UDF (user-defined function)?

Function callable in SQL expressions. Three types: scalar (returns single value), inline table-valued (returns table), multi-statement table-valued (returns table). Scalar UDFs are infamously slow in older SQL Server.

**Q235.** When should I use stored procs vs in-app SQL/ORM?

Use procs for: complex performance-critical operations, ETL, batch jobs.

Use ORM/in-app SQL for: most CRUD, anything that changes with code.

Modern preference: keep logic in app code. Procs split logic between code and DB, harder to version control and test.

### 3.8 Entity Framework Core Basics

**Q236.** What is Entity Framework Core?

Microsoft's ORM. Maps C# classes to database tables. Lets you query with LINQ, modify entities, persist changes. Cross-platform, open source. Replaces older Entity Framework 6.

**Q237.** What's a DbContext?

Represents a session with the database. Has DbSets for tables. Tracks changes to entities. Persists changes via SaveChangesAsync. Should be Scoped lifetime in DI.

**Q238.** Show DbContext setup.

Modern .NET 8 pattern.

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
public DbSet<Customer> Customers => Set<Customer>();
public DbSet<Order> Orders => Set<Order>();
protected override void OnModelCreating(ModelBuilder mb)
{
mb.Entity<Customer>(e =>
{
e.HasKey(c => c.Id);
e.Property(c => c.Email).IsRequired().HasMaxLength(200);
e.HasIndex(c => c.Email).IsUnique();
e.HasMany(c => c.Orders).WithOne(o => o.Customer);
});
}
}
// Register
builder.Services.AddDbContext<AppDbContext>(opts =>
opts.UseSqlServer(builder.Configuration.GetConnectionString("Db")));
```

**Q239.** What's Code-First vs Database-First?

Code-First: define entities in C#, generate schema. Most common in modern .NET.

Database-First: existing DB, scaffold C# from it. Use for legacy databases.

Modern guidance: Code-First with migrations.

**Q240.** What's a Migration?

Versioned schema change. dotnet ef migrations add Name generates code. dotnet ef database update applies. Each migration has Up (apply) and Down (rollback).

**Q241.** Show migration commands.

Standard workflow.

```bash
# Add a new migration after model changes
dotnet ef migrations add AddCustomerEmailIndex
# Apply migrations to current database
dotnet ef database update
# Apply to a specific migration (rollback)
dotnet ef database update PreviousMigrationName
# Generate SQL script (don't apply)
dotnet ef migrations script > migration.sql
# Remove last migration (if not applied)
dotnet ef migrations remove
```

**Q242.** Show basic CRUD with EF.

Create, Read, Update, Delete.

```csharp
// CREATE
var customer = new Customer { Name = "Alice", Email = "a@x.com" };
_db.Customers.Add(customer);
await _db.SaveChangesAsync();
// READ - by id
var c = await _db.Customers.FindAsync(id);
// READ - filtered
var active = await _db.Customers
.Where(c => c.IsActive)
.OrderBy(c => c.Name)
.ToListAsync();
// UPDATE
customer.Email = "new@x.com";
await _db.SaveChangesAsync(); // EF detects changes
// DELETE
_db.Customers.Remove(customer);
await _db.SaveChangesAsync();
```

### 3.9 EF Core Advanced

**Q243.** What's the difference between Eager, Lazy, and Explicit loading?

Eager: .Include() — load related entities in same query (JOIN).

Lazy: auto-load when accessed. Requires proxies. RISK: N+1.

Explicit: manually trigger load with context.Entry(e).Reference(...).LoadAsync().

Default to Eager. Lazy is dangerous in apps.

**Q244.** Show eager loading with Include.

Loads related data in single SQL query.

```csharp
var orders = await _db.Orders
.Include(o => o.Customer) // load customer
.Include(o => o.Lines) // load lines
.ThenInclude(l => l.Product) // load product for each line
.Where(o => o.Status == "pending")
.ToListAsync();
// Single SQL query with joins.
```

**Q245.** What's the N+1 problem?

Execute one query for list + one query per item for related data. 1+N total. Fix with .Include() or projection.

**Q246.** Show N+1 problem.

What to avoid.

```csharp
// 1 query for orders
var orders = await _db.Orders.ToListAsync();
// N queries (one per order) - this is N+1!
foreach (var o in orders)
{
var lines = await _db.OrderLines
.Where(l => l.OrderId == o.Id).ToListAsync();
}
// FIX: Include
var orders = await _db.Orders.Include(o => o.Lines).ToListAsync();
```

**Q247.** What's tracking vs no-tracking?

Tracking (default): EF tracks every entity. Modifications are persisted on SaveChanges.

No-tracking (.AsNoTracking()): faster, lower memory. Use for read-only queries.

Read-only API endpoints should always use AsNoTracking().

**Q248.** Show projection to DTO.

Projecting fetches only needed columns.

```csharp
// Loads all columns
var orders = await _db.Orders.ToListAsync();
// Loads only what we need - faster, less memory
var summary = await _db.Orders
.Where(o => o.Status == "pending")
.Select(o => new OrderSummary
{
Id = o.Id,
CustomerName = o.Customer.Name,
Total = o.Total
})
.ToListAsync();
```

**Q249.** What's AsSplitQuery?

Splits a query with multiple Includes into multiple SQL queries instead of one giant join. Use when single-query joins create cartesian explosion (lots of duplicate rows).

**Q250.** What's a transaction in EF Core?

EF wraps SaveChangesAsync in implicit transaction. For multi-SaveChanges atomic operations, use BeginTransactionAsync.

**Q251.** Show explicit EF transaction.

When you need multiple SaveChanges to be atomic.

```csharp
using var tx = await _db.Database.BeginTransactionAsync();
try
{
_db.Orders.Add(order);
await _db.SaveChangesAsync();
_db.AuditLog.Add(new Audit { Action = "OrderCreated", OrderId = order.Id });
await _db.SaveChangesAsync();
await tx.CommitAsync();
}
catch
{
await tx.RollbackAsync();
throw;
}
```

**Q252.** What's bulk insert in EF Core?

Built-in: AddRange + SaveChanges (slower, one INSERT per entity in older EF). EF Core 7+ has ExecuteUpdate / ExecuteDelete for bulk operations. For massive data: use SqlBulkCopy or libraries like EFCore.BulkExtensions.

**Q253.** Show ExecuteUpdate / ExecuteDelete.

EF Core 7+ bulk operations.

```csharp
// Bulk update (single SQL, no loading entities)
await _db.Orders
.Where(o => o.Status == "pending" && o.CreatedDate < cutoff)
.ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, "expired"));
// Bulk delete (single SQL DELETE)
await _db.Orders
.Where(o => o.Status == "cancelled")
.ExecuteDeleteAsync();
```

**Q254.** How do I see the SQL EF generates?

Log to console: optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information).

Log to ILogger: configured automatically with DI.

EFCore Toolkit / EF Profiler for advanced inspection.

Always log SQL in development to catch bad queries early.

**Q255.** Show enabling EF logging.

See the actual SQL being run.

```csharp
builder.Services.AddDbContext<AppDbContext>(opts =>
{
opts.UseSqlServer(connectionString);
if (builder.Environment.IsDevelopment())
{
opts.LogTo(Console.WriteLine, LogLevel.Information);
opts.EnableSensitiveDataLogging(); // shows parameter values - DEV ONLY
}
});
```

**Q256.** What's a value converter in EF Core?

Converts a property between database type and CLR type. Used for: enums to strings, JSON columns, custom types like Money.

**Q257.** Show value converter.

Store enum as string instead of int.

```csharp
modelBuilder.Entity<Order>()
.Property(o => o.Status)
.HasConversion<string>(); // store as string, not int
// Custom converter
modelBuilder.Entity<Order>()
.Property(o => o.Total)
.HasConversion(
money => money.Amount, // to DB
amount => new Money(amount)); // from DB
```

**Q258.** How do I handle EF concurrency?

Optimistic concurrency: detect conflicts on save.

Add a [Timestamp] / RowVersion column. EF checks it on UPDATE.

If row changed since load, EF throws DbUpdateConcurrencyException.

Handle by: re-loading and retrying, or asking user.

**Q259.** Show concurrency handling.

Optimistic concurrency with rowversion.

```csharp
public class Order
{
public Guid Id { get; set; }
public string Status { get; set; }
[Timestamp] public byte[] RowVersion { get; set; }
}
try { await _db.SaveChangesAsync(); }
catch (DbUpdateConcurrencyException ex)
{
foreach (var entry in ex.Entries)
{
var dbValues = await entry.GetDatabaseValuesAsync();
if (dbValues == null) { /* deleted by other */ }
else { /* re-load and retry */ }
}
}
```

**Q260.** What are EF Core conventions?

Default behaviors: Id property is PK, [Required] columns are NOT NULL, navigation properties create FKs. Override with attributes or fluent API in OnModelCreating.

**Q261.** What's Fluent API?

Configure entities in OnModelCreating using builder methods. More expressive than attributes. Keeps domain models clean.

**Q262.** Show Fluent API configuration.

Configure entities programmatically.

```csharp
protected override void OnModelCreating(ModelBuilder mb)
{
mb.Entity<Customer>(e =>
{
e.ToTable("Customers", "sales");
e.HasKey(c => c.Id);
e.Property(c => c.Email).IsRequired().HasMaxLength(200);
e.HasIndex(c => c.Email).IsUnique();
e.Property(c => c.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
e.HasMany(c => c.Orders).WithOne(o => o.Customer).OnDelete(DeleteBehavior.Cascade);
});
}
```

**Q263.** What's a IEntityTypeConfiguration?

Cleaner way to organize Fluent API. One configuration class per entity. Apply with mb.ApplyConfiguration() or mb.ApplyConfigurationsFromAssembly().

**Q264.** When should I use raw SQL with EF Core?

Complex queries that don't translate well to LINQ.

Bulk operations.

Performance-critical hot paths.

Database-specific features.

Use FromSqlRaw/FromSqlInterpolated for SELECT, ExecuteSqlRaw for non-query.

**Q265.** Show raw SQL with EF.

Mix LINQ and raw SQL.

```csharp
// Query with raw SQL
var orders = await _db.Orders
.FromSqlInterpolated($"SELECT * FROM Orders WHERE CustomerId = {customerId}")
.Where(o => o.Total > 100) // can still chain LINQ
.ToListAsync();
// Non-query
await _db.Database.ExecuteSqlInterpolatedAsync(
$"UPDATE Orders SET Status = 'archived' WHERE OrderDate < {cutoff}");
```

**Q266.** What's pooling in EF Core?

DbContext pooling (AddDbContextPool) reuses contexts across requests. Avoids allocation overhead. Use when: high request volume, no per-request DbContext customization.

**Q267.** How do I seed initial data?

Two ways: data seeding via HasData() in OnModelCreating (becomes part of migration) or seeding via code at app startup.

**Q268.** What's a global query filter?

Automatic WHERE applied to every query for an entity. Used for: soft delete, multi-tenancy.

**Q269.** Show soft delete with global filter.

Auto-filter out deleted rows.

```csharp
public class Order
{
public Guid Id { get; set; }
public bool IsDeleted { get; set; }
}
// In OnModelCreating
modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
// Now this only returns non-deleted
var orders = await _db.Orders.ToListAsync();
// To include deleted explicitly
var all = await _db.Orders.IgnoreQueryFilters().ToListAsync();
```

**Q270.** What's owned types in EF Core?

Entity belongs to another entity, has no own identity. Used for value objects (Address, Money). Stored in same table or separate.

**Q271.** Show owned type.

Customer has an Address that's not its own table.

```csharp
public class Customer
{
public Guid Id { get; set; }
public string Name { get; set; }
public Address Address { get; set; } // owned
}
public class Address // no Id
{
public string Street { get; set; }
public string City { get; set; }
public string Zip { get; set; }
}
modelBuilder.Entity<Customer>().OwnsOne(c => c.Address);
// Address columns are added to Customers table:
// Address_Street, Address_City, Address_Zip
```

**Q272.** What's a shadow property?

Property in EF model that doesn't exist on C# class — only in DB and metadata. Useful for: timestamps, audit columns, soft delete flags.

**Q273.** What's the difference between SaveChanges and SaveChangesAsync?

SaveChanges blocks the thread. SaveChangesAsync is non-blocking. ALWAYS use async in web apps.

**Q274.** What's a compiled query?

EF.CompileAsyncQuery() pre-compiles a query for faster execution. Used for hot paths run thousands of times. Manual optimization, not usually needed.

**Q275.** How do I handle EF Core in ASP.NET production?

Register as Scoped via AddDbContext().

Use AddDbContextPool for high-volume APIs.

Use AsNoTracking() for read-only queries.

Project to DTOs to reduce data loaded.

Use Include() correctly to avoid N+1.

Log SQL in dev to catch bad queries.

Apply migrations as separate deploy step in production (not at app startup).

### 3.10 Advanced SQL

**Q276.** What's a window function?

Function that operates over a set of rows related to current row, without grouping. Examples: ROW_NUMBER, RANK, LAG, LEAD, SUM OVER. Powerful for analytics.

**Q277.** Show window function.

Rank orders within each customer.

```sql
SELECT
o.Id,
o.CustomerId,
o.Total,
ROW_NUMBER() OVER (PARTITION BY o.CustomerId ORDER BY o.Total DESC)
AS RankWithinCustomer,
SUM(o.Total) OVER (PARTITION BY o.CustomerId) AS CustomerTotal
FROM Orders o;
```

**Q278.** What's a pivot?

Transform rows into columns. Useful for reports. SQL Server has PIVOT operator; alternative is conditional aggregation (SUM(CASE WHEN ...)).

**Q279.** What's the difference between EXISTS and IN?

IN: 'is value in list?' Best for small fixed lists.

EXISTS: 'does any row match?' Often faster for subqueries — short-circuits on first match.

Modern optimizers often produce identical plans for both.

**Q280.** What's the difference between OLTP and OLAP?

OLTP (transactional): many small concurrent operations. Normalized schema. Examples: order entry, banking. Tools: Azure SQL, Postgres.

OLAP (analytical): few large scans/aggregations. Denormalized (star/snowflake). Examples: BI dashboards, executive reports. Tools: Synapse, Snowflake.

Don't run heavy OLAP on OLTP — separate systems with ETL/CDC between.

## SECTION 4 — MICROSERVICES (Q281-340)

### 4.1 Fundamentals

**Q281.** What's a microservice?

Small, independently deployable service owning a bounded business capability. Three properties: independently deployable, owns its data, loosely coupled communication. Size doesn't define a microservice — independence does.

**Q282.** What's a monolith vs microservices?

Monolith: one deployable unit with all code. Simple, fast for small teams. Microservices: many independently deployable services. Complex, scales orgs and tech.

**Q283.** When should you NOT use microservices?

Small team (under 10 engineers) — coordination overhead exceeds benefit.

Immature CI/CD, observability, IaC — you'll regret it within months.

Domain isn't clear — boundaries drawn early often need to move.

Cross-service transactions are common — sagas are hard.

No on-call rotations or production support culture.

**Q284.** What's a distributed monolith?

Worst of both worlds: services that LOOK independent but require coordinated deploys, share databases, or have synchronous chains. Pay all the cost of microservices, get none of the benefit.

**Q285.** Smells of a distributed monolith?

Service A and B must be deployed together.

Multiple services share a database.

Long synchronous chains (A calls B calls C calls D).

Failure of one service breaks the whole flow.

Coordinated releases across teams.

**Q286.** What are the 8 fallacies of distributed computing?

1. The network is reliable. (It isn't.)

2. Latency is zero. (It's not.)

3. Bandwidth is infinite. (It's metered.)

4. The network is secure. (Assume hostile.)

5. Topology doesn't change. (It does — pods restart, IPs rotate.)

6. There is one administrator. (There isn't.)

7. Transport cost is zero. (Serialization/encryption all cost.)

8. The network is homogeneous. (Different protocols, vendors.)

**Q287.** What's a Bounded Context?

DDD concept. A boundary inside which a model is internally consistent. Same word can mean different things in different contexts (Customer in Sales vs Support vs Billing). One microservice = one bounded context (usually).

**Q288.** What's an Aggregate in DDD?

Cluster of domain objects treated as a single unit for changes. Has one root entity (Aggregate Root). Other code references it by ID. Transactions never span aggregates.

**Q289.** How do you decide microservice boundaries?

Bounded contexts (DDD) — primary heuristic.

Change rate — things that change together belong together.

Data ownership — each service owns its data.

Anti-pattern: splitting by technical layer (UI/business/data).

Anti-pattern: splitting too finely (one service per entity).

### 4.2 Communication

**Q290.** Sync vs async — when each?

Sync (REST, gRPC): caller waits for response. Use when caller needs answer to proceed and latency budget allows.

Async (queue, event): fire-and-forget. Use when work can complete in background, consumer might be down, multiple consumers want event.

Default to async. Sync only when you have a specific reason.

**Q291.** Why is sync risky in microservices?

Cascading failures: A calls B calls C, C is slow → all upstream tied up.

Latency compounds: 5 hops at 50ms each = 250ms minimum.

Availability multiplies: 0.999^5 = 99.5%, less than any single service.

**Q292.** REST vs gRPC?

REST: HTTP + JSON. Loose contract. Universal client support. Slower.

gRPC: HTTP/2 + Protobuf. Strong contracts (.proto). Faster, binary. Streaming.

Pick REST for: public APIs, browser clients. Pick gRPC for: internal service-to-service.

**Q293.** What's an API Gateway?

Single entry point for client traffic. Provides cross-cutting concerns: authentication, rate limiting, versioning, transformation, logging. Implementations: Azure APIM, Kong, AWS API Gateway.

**Q294.** What's a BFF (Backend for Frontend)?

Per-client API layer aggregating downstream calls and shaping responses for one client (mobile, web, partner). Reduces client-side complexity, network round-trips.

**Q295.** What's a Service Mesh?

Sidecar proxies (Envoy) next to every service. Handles mTLS, retries, traffic shifting, observability — without app code. Implementations: Istio, Linkerd, Consul Connect. Use only at 15+ services.

### 4.3 Distributed Data

**Q296.** Why don't we use 2PC (Two-Phase Commit)?

Blocking: participants hold locks during PREPARE.

Coordinator failure leaves participants in doubt.

Doesn't fit cloud-native (HTTP, queues).

Latency: every transaction = two roundtrips.

Modern: sagas + outbox pattern.

**Q297.** What's a Saga?

Sequence of local transactions, each in one service. If a step fails, previously-completed steps are undone via compensating transactions (semantic undo).

**Q298.** Show saga steps.

Order placement saga.

```text
Step 1: OrderService creates order (status='pending')
Step 2: PaymentService charges card
Step 3: InventoryService reserves stock
Step 4: ShippingService schedules pickup
Step 5: OrderService marks order 'confirmed'
If step 3 fails:
-> compensate step 2: refund the charge
-> compensate step 1: mark order 'failed'
```

**Q299.** Saga orchestration vs choreography?

Orchestration: central orchestrator drives saga step by step.

Choreography: each service emits events; others subscribe and react. No central coordinator.

Pick orchestration for ≤4 steps + need visibility. Choreography for >4 steps + loose coupling.

**Q300.** What's the Outbox Pattern?

Solves the dual-write problem: 'I need to update DB AND publish a message atomically.' Write business state and outbox row in same DB transaction. Background job publishes outbox messages to broker.

**Q301.** Show outbox pattern.

Reliable event publishing.

```sql
BEGIN TRANSACTION
INSERT INTO Orders (...);
INSERT INTO Outbox (id, type, payload, status='pending');
COMMIT
-- Background poller
rows = SELECT TOP 100 * FROM Outbox WHERE status='pending';
foreach row:
publish to message bus
UPDATE Outbox SET status='sent' WHERE id = row.id
```

**Q302.** Why is exactly-once delivery a myth?

In distributed systems you can have at-most-once or at-least-once. 'Exactly-once' wire-level is impossible — network can drop ack. Solution: at-least-once + idempotent receivers = 'effectively-once.'

**Q303.** How do you make a handler idempotent?

Dedup table: store processed message IDs, insert atomically with work.

Conditional updates: 'UPDATE WHERE status='pending'' — second run is no-op.

Set semantics: 'mark as shipped' twice is a no-op.

**Q304.** What's CAP theorem?

In distributed system with network partition, can have:

CP: Consistency + Partition tolerance (sacrifice availability).

AP: Availability + Partition tolerance (sacrifice immediate consistency).

Cannot have all three during partition. Practical choice: C vs A.

Examples: SQL = CP. Cassandra/DynamoDB = AP.

**Q305.** What's eventual consistency?

All replicas converge eventually, but reads may temporarily return stale values. Trade-off for availability and scale.

### 4.4 Resilience

**Q306.** What's Polly?

Standard .NET resilience library. Composable policies: Retry, Circuit Breaker, Timeout, Bulkhead, Fallback.

**Q307.** What's a Circuit Breaker?

State machine in front of a remote call. Three states: Closed (normal), Open (fail fast after N failures), Half-Open (after cooldown, probe). Stops cascading failures.

**Q308.** Why exponential backoff with jitter?

Backoff: 100ms, 200ms, 400ms... — prevents hammering struggling service.

Jitter: random variance — prevents synchronized 'thundering herd' retries.

Without both, retries amplify outages.

**Q309.** Polly composition order?

Outside-in: Bulkhead → Retry → CircuitBreaker → Timeout → call.

Translation: limit concurrency, retry on failure, but stop if breaker open, don't let any call hang.

**Q310.** Show .NET 8 resilience pipeline.

Modern pattern.

```csharp
builder.Services.AddHttpClient<IPaymentClient, PaymentClient>()
.AddStandardResilienceHandler(opts =>
{
opts.Retry.MaxRetryAttempts = 3;
opts.Retry.UseJitter = true;
opts.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
opts.CircuitBreaker.MinimumThroughput = 5;
});
```

**Q311.** What's a bulkhead?

Isolate resources so one failing dependency doesn't exhaust shared pools. Named after ship bulkheads. Implement with separate thread pools, connection pools, or HttpClient instances per dependency.

**Q312.** What's the difference between liveness and readiness probes?

Liveness: 'are you alive?' Fail → restart pod.

Readiness: 'can you serve traffic?' Fail → remove from load balancer (no restart).

Don't conflate. They have different consequences.

### 4.5 Cross-Cutting

**Q313.** What's the 12-Factor App?

Heroku's principles for cloud-native apps.

Codebase, Dependencies, Config, Backing services, Build/Release/Run.

Processes (stateless), Port binding, Concurrency, Disposability.

Dev/Prod parity, Logs, Admin processes.

**Q314.** How do you handle config in microservices?

Externalize all env-specific values.

Source: Azure App Configuration or K8s ConfigMaps.

Secrets: Key Vault, referenced via App Config or Managed Identity.

Hot-reload where possible.

**Q315.** How do services find each other (service discovery)?

DNS-based: in K8s/AKS, automatic via Service objects.

Client-side: client queries registry (Eureka, Consul) and load-balances.

Server-side: API gateway hides discovery.

AKS: do nothing — DNS is automatic.

**Q316.** How do you handle distributed tracing?

W3C Trace Context standard via traceparent header. .NET 8 propagates automatically. App Insights stitches into end-to-end view. Critical for debugging cross-service issues.

**Q317.** How do you handle authentication across services?

API Gateway validates JWT once at the edge.

Gateway adds user identity as headers when forwarding.

Internal services trust gateway, read identity from headers.

Service-to-service: Managed Identity (in Azure) or Client Credentials flow.

**Q318.** What's schema evolution for events?

Additive only: add fields, never remove or rename.

Default values for new fields.

Consumer tolerance: ignore unknown fields.

Breaking changes: new event type (OrderPlacedV2). Producer publishes both during migration.

**Q319.** What's an Anti-Corruption Layer?

Translation boundary between your bounded context and external/legacy model. Adapter classes convert vendor types to your domain types. Prevents foreign concepts from leaking in.

**Q320.** What's the Strangler Fig pattern?

Coined by Martin Fowler.

Migrate legacy by routing traffic through a facade that progressively delegates more to new code.

Steps: facade → migrate first leaf → repeat → decommission legacy.

Zero-big-bang risk.

### 4.6 Operations

**Q321.** What's the difference between Service Bus and Event Grid?

Service Bus: transactional messaging between services. FIFO via sessions, DLQ. Pull-based.

Event Grid: reactive events from Azure resources or apps. Push-based. Massive fan-out.

Pick Service Bus for reliable inter-service. Event Grid for fire-and-forget reactions.

**Q322.** What's a Dead-Letter Queue (DLQ)?

Sub-queue for messages that can't be processed (max retries, expired, or explicitly dead-lettered). Prevents poison messages blocking the queue. Alert on DLQ depth.

**Q323.** What's blue/green vs canary deployment?

Blue/green: two full environments. Switch traffic 100% at once. Instant rollback. Higher cost.

Canary: small % of traffic to new version, ramp gradually. Catches issues that only appear under real load.

**Q324.** What's a feature flag?

Code that's deployed but disabled until flag flipped. Decouples deploy from release. Enables: gradual rollout, A/B testing, instant disable without redeploy.

**Q325.** What's the difference between containers and VMs?

VM: full OS + your app. GBs. Slow to start.

Container: shared OS kernel + your app + libraries. MBs. Starts in seconds.

Containers package your app with its dependencies.

**Q326.** What's Docker?

Container runtime. Build images with Dockerfile, run containers from images. Standard for packaging apps for deployment.

**Q327.** What's Kubernetes?

Container orchestrator. Manages many containers across many machines. Handles scheduling, scaling, healing, networking. Azure version: AKS. Use only when you have many services.

**Q328.** What's a Pod in Kubernetes?

Smallest deployable unit. One or more containers sharing network and storage. Pods are ephemeral — they die and get replaced.

**Q329.** What's a Deployment in K8s?

Declarative spec for Pods + ReplicaSet. Manages rolling updates. Says 'I want N replicas of this image.'

**Q330.** What's a Service in K8s?

Stable network endpoint for a set of Pods. Pods come and go; Service provides a constant DNS name and IP.

**Q331.** What's an Ingress?

HTTP/HTTPS routing into the cluster. Routes by hostname/path to internal Services. Implementations: NGINX, Application Gateway Ingress Controller.

**Q332.** What's HPA (Horizontal Pod Autoscaler)?

Scales pod count based on CPU/memory or custom metrics. Replaces manual replica adjustment.

**Q333.** What's KEDA?

Kubernetes Event-Driven Autoscaling. Scale by external metrics (queue depth, Cosmos RU, Service Bus messages). Built into Container Apps.

**Q334.** What's a ConfigMap and Secret in K8s?

ConfigMap: non-sensitive config (env vars, config files).

Secret: sensitive data (passwords, keys). Base64 encoded — not encryption!

For real secrets, use Key Vault + CSI driver or Workload Identity.

**Q335.** What's a CI/CD pipeline?

Build → Test → Scan → Push → Deploy.

Triggered on code commit.

Tools: GitHub Actions, Azure DevOps, GitLab CI.

Goal: every commit is automatically tested and deployable.

**Q336.** What's Infrastructure as Code (IaC)?

Define infrastructure (VMs, networks, DBs) in declarative code in version control. Apply via tooling. Same code = same infra everywhere. Tools: Bicep, Terraform, ARM.

**Q337.** What's GitOps?

Git is source of truth for desired state. Agent in cluster (Flux, ArgoCD) reconciles actual state to match. No kubectl from laptops. Auditable, drift-corrected.

**Q338.** What are DORA metrics?

DevOps Research and Assessment metrics:

Deployment Frequency: how often you deploy.

Lead Time for Changes: commit to prod.

Change Failure Rate: % deploys causing issues.

MTTR: mean time to recovery.

Elite teams: deploy multiple times/day, <1hr lead time, <15% CFR, <1hr MTTR.

**Q339.** What's an SLO and error budget?

SLO: Service Level Objective. e.g., 99.9% availability over 30 days.

Error budget: 0.1% = ~43 minutes/month allowed downtime.

Used to balance reliability vs feature velocity. Budget remaining → ship features. Exhausted → freeze risky changes.

**Q340.** What's the difference between containers and serverless?

Container (AKS, Container Apps): you control runtime, longer-running, more flexibility.

Serverless (Functions): per-execution, scales to zero, no infra management, but cold starts.

Pick serverless for: event-driven, bursty, short jobs. Container for: long-running, predictable load.

## SECTION 5 — REACT (Q341-380)

### 5.1 React Fundamentals

**Q341.** What's React?

JavaScript library for building UIs using composable components and a virtual DOM that efficiently updates the real DOM when state changes.

**Q342.** What's a component?

Reusable, self-contained piece of UI. Modern React uses functional components with hooks. Class components are legacy.

**Q343.** What's JSX?

Syntax extension for JavaScript. Looks like HTML, compiles to React.createElement() calls. Lets you write UI declaratively in JS.

**Q344.** What's the virtual DOM?

In-memory representation of the UI. React diffs old vs new virtual DOM, computes minimal real DOM changes. Faster than direct DOM manipulation for most use cases.

**Q345.** What are props?

Data passed from parent to child component. Read-only — child cannot modify. Like function arguments.

**Q346.** What's state?

Component-local data that triggers re-render when changed. useState hook in functional components. Belongs to the component that needs to change it.

**Q347.** Show useState basics.

Local component state.

```jsx
function Counter() {
const [count, setCount] = useState(0);
return (
<>
<p>Count: {count}</p>
<button onClick={() => setCount(count + 1)}>+</button>
</>
);
}
```

**Q348.** What's useEffect?

Hook for side effects: data fetching, subscriptions, manual DOM updates. Runs after render. Cleanup function on unmount or before re-running.

**Q349.** Show useEffect for data fetching.

Common pattern.

```jsx
function OrderList({ customerId }) {
const [orders, setOrders] = useState([]);
const [loading, setLoading] = useState(true);
useEffect(() => {
let cancelled = false;
fetch(`/api/orders?customerId=${customerId}`)
.then(r => r.json())
.then(data => { if (!cancelled) { setOrders(data); setLoading(false); } });
return () => { cancelled = true; }; // cleanup
}, [customerId]);
if (loading) return <Spinner />;
return <ul>{orders.map(o => <li key={o.id}>{o.total}</li>)}</ul>;
}
```

**Q350.** What's the dependency array in useEffect?

Array tells React when to re-run effect. [] = once on mount. [x, y] = when x or y changes. Missing = every render. The most common bug: missing dependencies cause stale closures.

### 5.2 React Hooks

**Q351.** What are the most common hooks?

useState: local state.

useEffect: side effects.

useRef: mutable reference, often DOM access.

useMemo: memoize expensive calculations.

useCallback: memoize functions.

useContext: access React Context.

useReducer: complex state with actions.

**Q352.** useState vs useReducer?

useState: simple. Best for primitive or small object.

useReducer: complex state with multiple sub-values. Same pattern as Redux. Use for forms, finite state machines.

**Q353.** useMemo vs useCallback?

useMemo: memoize a value. Recompute only when deps change.

useCallback: memoize a function. Returns same function reference unless deps change.

Don't overuse — they have memory cost. Use only when there's a measurable problem.

**Q354.** What's useRef?

Mutable reference that persists across renders. Doesn't trigger re-render on change. Two uses: access DOM nodes, store mutable values.

**Q355.** Show useRef.

DOM access and mutable storage.

```jsx
// DOM access
function FocusInput() {
const inputRef = useRef(null);
return (
<>
<input ref={inputRef} />
<button onClick={() => inputRef.current.focus()}>Focus</button>
</>
);
}
// Mutable storage (without re-render)
function Timer() {
const intervalRef = useRef(null);
useEffect(() => {
intervalRef.current = setInterval(() => console.log('tick'), 1000);
return () => clearInterval(intervalRef.current);
}, []);
return <div>Running</div>;
}
```

**Q356.** What's a custom hook?

Function whose name starts with 'use' that calls other hooks. Reusable logic without HOCs.

**Q357.** Show a custom hook.

Reusable data fetching.

```jsx
function useFetch(url) {
const [data, setData] = useState(null);
const [loading, setLoading] = useState(true);
const [error, setError] = useState(null);
useEffect(() => {
fetch(url)
.then(r => r.json())
.then(setData)
.catch(setError)
.finally(() => setLoading(false));
}, [url]);
return { data, loading, error };
}
// Use:
function Orders() {
const { data, loading, error } = useFetch('/api/orders');
if (loading) return <Spinner />;
if (error) return <p>Error</p>;
return <ul>{data.map(o => <li key={o.id}>{o.total}</li>)}</ul>;
}
```

### 5.3 State Management

**Q358.** What state management options are there?

Local: useState/useReducer.

Lifted: shared state in common parent.

Context: app-wide (theme, user). Few updates, broad reads.

Server state: TanStack Query/RTK Query/SWR. ALWAYS for server data.

Client state library: Redux Toolkit, Zustand, Jotai. For complex client state.

URL: routing params for shareable state.

**Q359.** What's TanStack Query?

Library for server state. Handles caching, refetching, loading/error states, optimistic updates. Replaces ad-hoc useEffect+fetch. The biggest React improvement of 2020s.

**Q360.** Show TanStack Query.

Cleaner than manual fetch + useState.

```jsx
function OrderList({ customerId }) {
const { data, isLoading, error } = useQuery({
queryKey: ['orders', customerId],
queryFn: () => fetch(`/api/orders?customerId=${customerId}`).then(r => r.json())
});
if (isLoading) return <Spinner />;
if (error) return <Error msg={error.message} />;
return <ul>{data.map(o => <li key={o.id}>{o.total}</li>)}</ul>;
}
```

**Q361.** Context vs Redux?

Context: built-in. Best for app-wide read-mostly data (theme, user, locale).

Redux: external library. Best for complex client state with many updates, time-travel debugging needs.

In 2026: many React apps don't need Redux. TanStack Query for server, Context for app-wide, useState for local.

**Q362.** What's the biggest React state mistake?

Putting server data in Redux/Context. Server state has cache lifetimes, refetch needs, optimistic updates — TanStack Query handles all of that. Redux for server data is years of self-inflicted pain.

### 5.4 Performance

**Q363.** How do I optimize React performance?

Code splitting: route-based with React.lazy + Suspense.

Memoize expensive components with React.memo.

useMemo for expensive calculations.

useCallback for callbacks passed to memoized children.

Virtualize large lists (react-window).

Defer non-critical work with useTransition (React 18).

Profile with React DevTools — measure before optimizing.

**Q364.** What's React.memo?

Higher-order component that memoizes a functional component. Skips re-render if props haven't changed (shallow comparison).

**Q365.** What causes a re-render in React?

State change (useState, useReducer).

Props change.

Parent re-renders.

Context value changes.

useMemo/useCallback don't prevent re-render — they prevent recomputation/recreation.

**Q366.** What's React 18 concurrent mode?

Renders can be interrupted, prioritized, or thrown away. Enables: useTransition (mark updates as non-urgent), Suspense for data fetching, automatic batching.

**Q367.** What are Web Vitals?

LCP (Largest Contentful Paint): under 2.5s.

INP (Interaction to Next Paint): under 200ms.

CLS (Cumulative Layout Shift): under 0.1.

TTFB (Time To First Byte): under 800ms.

Track with web-vitals library, send to App Insights.

### 5.5 Modern React

**Q368.** What are React Server Components?

Components that run on the server, output HTML directly. Don't ship JS for those components. Smaller bundles, direct data access. Need framework support (Next.js App Router).

**Q369.** What's CSR vs SSR vs SSG vs ISR?

CSR: Client-Side Rendering. Default React. Fast iteration, no SEO.

SSR: Server-Side Rendering. Per-request HTML. SEO + fast first paint.

SSG: Static Site Generation. Build-time HTML. Fastest, no dynamic data.

ISR: Incremental Static Regeneration. Mostly static, occasional updates. Next.js feature.

**Q370.** What's Next.js?

De-facto React framework. Routing, SSR/SSG, code splitting, API routes, RSC. Owned by Vercel. The standard React framework for production apps.

**Q371.** What's Vite?

Modern frontend build tool. Fast dev server (no bundling), fast builds (esbuild). Replaces Create React App for new projects.

**Q372.** What's the difference between controlled and uncontrolled components?

Controlled: value comes from state, updates via onChange.

Uncontrolled: input manages own value, read via ref.

**Q373.** How do I handle forms in React?

Simple: useState per field, controlled inputs.

Complex: React Hook Form library — great DX, built-in validation, low re-render.

Old: Formik (still works, less recommended for new).

**Q374.** What's react-router?

Standard routing library for React. Maps URLs to components. Supports nested routes, dynamic params, loaders (data fetching), actions (mutations).

**Q375.** How do I authenticate in React?

Use library: MSAL.js for Entra, Auth0 SDK, etc.

Don't roll your own OAuth.

Store access token in memory (NOT localStorage — XSS vulnerable).

Refresh token in HttpOnly cookie.

**Q376.** What's the difference between styled-components, CSS Modules, Tailwind?

CSS Modules: scoped CSS files. Standard webpack/Vite feature.

styled-components: CSS-in-JS. Components encapsulate styles.

Tailwind: utility-first CSS. Class names like p-4 bg-blue-500. Most popular for new projects.

**Q377.** What's a key prop and why is it required in lists?

React uses key to identify list items across renders. Required for array-rendered children. Use stable unique IDs (record id), not array index for dynamic lists.

**Q378.** What's React.lazy and Suspense?

lazy: dynamically import a component (code splitting). Suspense: shows fallback UI while async work loads. Combined for route-based splitting.

**Q379.** Show lazy loading.

Code splitting per route.

```jsx
import { lazy, Suspense } from 'react';
const AdminPanel = lazy(() => import('./AdminPanel'));
function App() {
return (
<Suspense fallback={<Spinner />}>
<Routes>
<Route path="/admin" element={<AdminPanel />} />
</Routes>
</Suspense>
);
}
// AdminPanel is in a separate JS bundle, loaded only when route hit
```

**Q380.** How do I debug React performance?

React DevTools Profiler tab.

Click Record, reproduce slow interaction.

Check flame chart for long renders.

Use 'Why did this render?' to find unnecessary re-renders.

Common fixes: memoize expensive components, virtualize lists, code split.

## SECTION 5 — AZURE (Q381-450)

### 5.1 Cloud Computing Basics

**Q381.** What is cloud computing in simple terms?

Renting computing resources (servers, storage, databases, networking) over the internet, on-demand, paying only for what you use. Instead of buying servers, you rent capacity from Microsoft (Azure), Amazon (AWS), or Google (GCP).

**Q382.** What's IaaS, PaaS, and SaaS?

IaaS (Infrastructure as a Service): rent VMs, networks, storage. You manage OS, runtime, app. Example: Azure VMs.

PaaS (Platform as a Service): cloud manages OS, runtime, scaling. You deploy app code. Example: Azure App Service, Functions, Cosmos DB.

SaaS (Software as a Service): cloud manages everything. You use the app. Example: Microsoft 365, Outlook.com.

Architect rule: prefer PaaS over IaaS when possible.

**Q383.** What's a region in Azure?

Geographic area with multiple data centers. Examples: East US, West Europe, Central India. Microsoft has 60+ regions globally.

**Q384.** What's an Availability Zone?

Physically separate data centers within a region, each with independent power, cooling, networking. Most major regions have 3 zones. Multi-AZ deployment survives data-center-level failures.

**Q385.** What's a Resource Group?

Logical container for related Azure resources (VMs, DBs, storage, networks). Resources share lifecycle. Apply RBAC at group level. Track costs by group via tags. Delete group = delete everything in it.

**Q386.** What's a Subscription?

Billing and management container. Has its own bill, limits, and RBAC scope. Common pattern: separate subscriptions per environment (dev/test/prod) or per business unit.

**Q387.** What's the Azure hierarchy?

Tenant (Entra ID directory)

Management Group (optional)

Subscription

Resource Group

Resources (VM, DB, Storage)

RBAC and policies cascade down from any level.

**Q388.** What's Azure Resource Manager (ARM)?

The deployment and management layer for Azure. Every Azure operation goes through ARM via REST APIs. Tools that talk to ARM: Portal, Azure CLI, PowerShell, ARM templates, Bicep, Terraform, SDKs.

**Q389.** What's Bicep?

Microsoft's modern domain-specific language for Azure IaC. Compiles to ARM JSON. Cleaner syntax, modules, type checking. Always pick Bicep over raw ARM for new projects.

**Q390.** What's Terraform?

Multi-cloud IaC tool by HashiCorp. Supports AWS, Azure, GCP. Use over Bicep when you need multi-cloud or your team already uses it.

### 5.2 Azure Compute

**Q391.** What are the main Azure compute options?

App Service: PaaS for web apps. Default for traditional .NET / Node web apps.

Azure Functions: Serverless. Event-driven, short jobs.

Container Apps: Containers without K8s management. 5-20 services.

AKS: Full Kubernetes. 15+ services with team experience.

Azure VMs: IaaS. Lift-and-shift legacy.

Logic Apps: Low-code workflow integration.

**Q392.** What's Azure App Service?

PaaS for hosting web apps and APIs. Supports .NET, Node, Python, Java, PHP, Ruby, custom containers. Auto-patching, load balancing, auto-scale, deployment slots, managed SSL, App Insights integration.

**Q393.** What are App Service tiers?

Free / Shared: dev only.

Basic: small production. No auto-scale, no slots.

Standard: production. Auto-scale, slots, daily backups.

Premium V3: better perf, VNet integration, zone redundancy.

Isolated V2: dedicated tier, single-tenant, in your VNet.

**Q394.** What's a Deployment Slot?

Separate instance of your app under same App Service Plan. Deploy to staging slot, test, then SWAP — staging becomes production with zero downtime. Easy rollback. Standard: 5 slots. Premium: 20.

**Q395.** What's an App Service Plan?

The compute (VMs) underlying your apps. Multiple apps can share one plan. You pay for the plan, not per app. Don't put high-traffic apps on same plan as low-traffic ones.

**Q396.** What's Azure Functions?

Microsoft's serverless compute. Triggered by events (HTTP, queue, timer, Cosmos changes, etc). You write a function, it runs when triggered. Pay-per-execution on Consumption plan.

**Q397.** What are Azure Functions hosting plans?

Consumption: pay-per-execution. Cold starts. 10-min timeout. Cheapest.

Premium: pre-warmed instances, VNet, no cold start, longer timeouts.

Dedicated (App Service Plan): run on existing App Service.

Flex Consumption (newer): per-instance scale, VNet, faster cold start.

**Q398.** What's the cold start problem?

On Consumption plan, idle functions are de-provisioned. First request after idle takes 1-10s for .NET. Mitigations: Premium plan, keep functions warm with timer triggers, use App Service for latency-critical APIs.

**Q399.** What's a Durable Function?

Stateful workflow built on Azure Functions. State persists automatically; orchestrator survives crashes. Patterns: function chaining, fan-out/fan-in, async HTTP API, monitoring.

**Q400.** What's Azure Container Apps?

Serverless containers. K8s under the hood but you don't see it. KEDA-based auto-scale, scale-to-zero, built-in HTTPS. Best for 5-20 microservices without K8s overhead.

**Q401.** What's AKS (Azure Kubernetes Service)?

Managed Kubernetes. Microsoft runs control plane (free), you manage worker nodes. Use for 15+ services with K8s expertise. Has node pools, ingress, HPA, Workload Identity.

**Q402.** What's a Pod in Kubernetes?

Smallest deployable unit. 1+ containers sharing network and storage. Usually one container per pod, but you can have sidecars (logging, mTLS proxies).

**Q403.** What's a Deployment in K8s?

Declarative spec for Pods + ReplicaSet. Manages rolling updates. Specify replicas, image, resource limits.

**Q404.** What's a Service in K8s?

Stable network endpoint for Pods. Types: ClusterIP (internal), NodePort, LoadBalancer (external).

**Q405.** What's HPA?

Horizontal Pod Autoscaler. Scales pod count by CPU/memory/custom metrics. KEDA extends this with event-driven scaling (queue depth, etc).

### 5.3 Azure Storage & Databases

**Q406.** What's an Azure Storage Account?

Namespace for storage services: Blobs (objects/files), Files (SMB shares), Queues (simple FIFO), Tables (NoSQL key-value), Disks (for VMs).

**Q407.** What are Azure Storage replication options?

LRS: 3 copies in one data center. Cheapest. No DC failure tolerance.

ZRS: 3 copies across 3 AZs in one region. Survives DC failure.

GRS: LRS in primary + LRS in paired region. Survives region failure.

RA-GRS: GRS + read access on secondary.

GZRS / RA-GZRS: ZRS in primary + LRS / read in paired region. Highest durability.

**Q408.** What are Blob access tiers?

Hot: most expensive storage, cheapest access. Frequent reads/writes.

Cool: cheaper storage, more expensive access. Infrequent (>30 days).

Cold: even cheaper, 90-day minimum.

Archive: cheapest storage, very expensive access. Rehydration takes hours. 180-day minimum.

**Q409.** What's a SAS token?

Shared Access Signature. Signed URL granting temporary, scoped access to a storage resource. Use for direct upload/download without proxying through your API. User Delegation SAS (signed by Entra) is most secure.

**Q410.** What's an Immutable Blob policy?

Lock blobs against modification or deletion for specified period. Required for FDA 21 CFR Part 11, HIPAA retention, SEC Rule 17a-4. Time-based or legal-hold mode.

**Q411.** What's Azure SQL Database?

Microsoft's managed SQL Server PaaS. Greenfield default. Single-database. Auto-patching, backups, HA. Tiers: Basic, General Purpose, Business Critical, Hyperscale.

**Q412.** What's Azure SQL Managed Instance?

Lift-and-shift SQL Server with VNet, Agent, cross-DB queries. Near-100% feature parity with on-prem SQL Server. Pick for migrations from on-prem.

**Q413.** What's the difference between DTU and vCore pricing?

DTU: blended metric (CPU + memory + IO). Simple. vCore: separately specified resources, more flexible. Modern default is vCore.

**Q414.** What's Cosmos DB?

Globally distributed NoSQL with multi-model support (SQL, MongoDB, Cassandra APIs). Single-region or multi-region writes. Single-digit ms p99 reads. 99.999% SLA.

**Q415.** What's a Request Unit (RU) in Cosmos?

Cosmos's throughput currency. 1KB read with session consistency = ~1 RU. 1KB write = ~5 RU. Cross-partition queries cost more.

**Q416.** What's a partition key in Cosmos DB?

Field Cosmos uses to physically distribute data. Hash(key) = partition. CRITICAL: cannot change after container creation. Pick high cardinality, evenly accessed values, used in queries.

**Q417.** What's a hot partition?

Partition getting disproportionate traffic. Cosmos throttles (429s) even if container has spare RU. Fix: better partition key, randomize for hot writes, cache hot reads.

**Q418.** What are Cosmos consistency levels?

Strong: linearizable. Highest cost.

Bounded Staleness: lag bounded by K versions or T seconds.

Session (default): see your own writes within session.

Consistent Prefix: never out-of-order.

Eventual: cheapest, weakest.

**Q419.** What's Cosmos Change Feed?

Built-in event stream of all writes. Subscribe via Functions Cosmos trigger. Use for materialized views, event sourcing, cross-system sync.

**Q420.** When NOT to use Cosmos DB?

Need rich SQL queries with JOINs across entities.

Need ACID across multiple partitions.

Cost-sensitive low-volume single-region.

Heavy reporting/analytics — use Synapse.

Schema-strict relational data.

**Q421.** What's Azure Cache for Redis?

Managed Redis. Distributed in-memory cache. Use for: cache-aside, session state, leaderboards, pub/sub, distributed locks. Tiers: Basic, Standard, Premium, Enterprise.

**Q422.** What's Azure AI Search?

Managed search-as-a-service. Full-text, faceted, vector (for RAG), hybrid, semantic ranker. Standard backbone for enterprise search and RAG.

### 5.4 Identity & Security

**Q423.** What's Microsoft Entra ID?

Microsoft's cloud identity service (formerly Azure AD). Implements OAuth 2.0, OIDC, SAML. Foundation of Azure security. Manages users, groups, service principals, managed identities.

**Q424.** What's a Tenant?

Your organization's Entra directory. Has unique Tenant ID, default domain (yourcompany.onmicrosoft.com), custom domains, users, app registrations.

**Q425.** What's an App Registration?

Defines your app to Entra. Configures client ID, redirect URIs, secrets/certificates, API permissions, exposed API, roles, token claims.

**Q426.** What's the OAuth Authorization Code + PKCE flow?

1. SPA generates code_verifier, computes code_challenge = SHA256(verifier).

2. Redirect to Entra with code_challenge.

3. User logs in, Entra redirects back with auth code.

4. SPA POSTs code + code_verifier to token endpoint.

5. Entra returns access_token + id_token + refresh_token.

6. SPA calls API with Bearer access_token.

**Q427.** What's Client Credentials flow?

Service-to-service auth. No user. App authenticates as itself with client secret or certificate. Used for: backend daemons, scheduled jobs, microservice-to-microservice.

**Q428.** What's a JWT?

JSON Web Token. Three Base64URL parts: Header.Payload.Signature. Signed (not encrypted). Anyone can read payload. Verify signature against public key from JWKS endpoint.

**Q429.** What must I validate in a JWT?

Signature: against AS's public key.

iss (issuer): match expected AS.

aud (audience): include your API's identifier.

exp (expiration): must be future.

nbf (not before): must be past.

scopes/roles: include required permissions.

**Q430.** What's Managed Identity?

Service principal automatically managed by Azure for an Azure resource. Resource gets token from local IMDS endpoint. No secrets on disk, no rotation needed. System-assigned (tied to resource) or User-assigned (standalone).

**Q431.** Show Managed Identity usage in .NET.

DefaultAzureCredential automatically picks up MI in Azure or your IDE login locally.

```csharp
var credential = new DefaultAzureCredential();
// Blob access
var blob = new BlobClient(uri, credential);
// Azure SQL connection string
// Server=tcp:myserver.database.windows.net,1433;
// Database=mydb;Authentication=Active Directory Default;
```

**Q432.** What's Azure Key Vault?

Secret/key/certificate store. Three object types: Secrets (strings), Keys (crypto), Certificates (TLS). Always enable soft-delete + purge protection in production.

**Q433.** What's RBAC?

Role-Based Access Control. Grant a security principal a role at a scope. Built-in roles: Owner, Contributor, Reader, plus service-specific. Apply at lowest scope necessary.

**Q434.** What's PIM?

Privileged Identity Management. Just-in-time elevation. Instead of permanent admin, user activates role for 1-8 hours with approval. Eliminates standing admin permissions.

**Q435.** What's MFA?

Multi-Factor Authentication. Require 2+ factors: something you know (password) + have (phone, FIDO2) + are (biometric). FIDO2 keys are most secure.

**Q436.** What's Conditional Access?

Entra policy-based access control. Examples: require MFA for admins, block from outside US, require compliant device for sensitive apps.

### 5.5 Networking

**Q437.** What's a VNet?

Virtual Network. Isolated network in Azure with your own private IP space. Resources deployed into VNet communicate privately. Subdivided into subnets.

**Q438.** What's a Subnet?

IP range subdivision of a VNet. Each subnet gets its own NSG. Some Azure services require dedicated subnets (App Service VNet integration, Application Gateway, Bastion).

**Q439.** What's an NSG?

Network Security Group. Stateful L4 firewall. Allow/deny by IP/port. Apply to subnet (recommended) or NIC. Stateful = response automatically allowed.

**Q440.** What's a Private Endpoint?

PaaS resource gets a private IP in YOUR VNet. Disable public access entirely. Modern best practice for production data services. Always pair with Private DNS Zone.

**Q441.** What's the difference between Service Endpoint and Private Endpoint?

Service Endpoint (legacy): allowlist subnet on PaaS. PaaS still has public IP. Private Endpoint (modern): PaaS gets private IP in your VNet, public can be disabled. Always prefer Private Endpoints.

**Q442.** What's hub-and-spoke topology?

Reference enterprise topology. Hub VNet has shared services (firewall, DNS, ExpressRoute). Spoke VNets are workloads, peered to hub. Inter-spoke traffic transits hub firewall.

**Q443.** What's Front Door?

Global L7 traffic manager + WAF + CDN. Anycast — closest PoP serves clients. Use for global apps, multi-region routing, edge security.

**Q444.** What's Application Gateway?

Regional L7 load balancer with optional WAF. Inside a VNet. Common for in-region L7 routing, AKS Ingress (AGIC).

**Q445.** What's the difference between Front Door and App Gateway?

Front Door: global, edge-based, L7 + WAF + CDN. Use for global routing.

App Gateway: regional, in-VNet, L7 + WAF. Use inside a region.

Common stack: Front Door (global edge) -> APIM (governance) -> App Gateway / AKS (workload).

**Q446.** What's a WAF?

Web Application Firewall. L7 protection against OWASP Top 10 (SQL injection, XSS, etc). Available on Front Door (global) and Application Gateway (regional). Use OWASP Core Rule Set + custom rules.

**Q447.** What's APIM?

Azure API Management. Single entry point for client traffic. Auth (JWT validation), rate limiting, transformation, versioning, response caching, dev portal. Tiers: Consumption, Developer, Basic, Standard, Premium.

### 5.6 Messaging

**Q448.** What are Azure messaging services?

Service Bus: transactional messaging between services. FIFO via sessions. DLQ. Default for inter-service messaging.

Event Grid: reactive events from Azure resources or your apps. Push-based. Fire-and-forget.

Event Hubs: high-throughput streaming (millions/sec). Pull-by-offset. Replayable. Has Kafka API.

Storage Queue: cheap, simple, large messages. No topics or sessions.

**Q449.** What's a Service Bus session?

Groups messages by sessionId for FIFO-per-key. Session locked to one consumer. Different sessions process in parallel. Use for per-customer ordering.

**Q450.** What's a Dead-Letter Queue (DLQ)?

Sub-queue for messages that can't be processed (max retries, expired, filter failed, explicitly DLQ'd). Alert on DLQ depth. Triage and reprocess after fixing.

## SECTION 6 — DESIGN PATTERNS (Q451-510)

Pattern questions are common in tech lead interviews. For each: know the trigger sentence (when to use it) and a real .NET example.

### 6.1 Creational Patterns

**Q451.** What is the Singleton pattern?

Trigger: 'There must be exactly one of this thing in the process and global access is acceptable.' Use cases: configuration, in-memory cache, logger. In modern .NET, prefer DI's AddSingleton<T> — same lifetime, no global state, fully testable.

**Q452.** Show a thread-safe Singleton in C#.

Use Lazy<T> for thread-safe lazy initialization:

```csharp
public sealed class Config
{
private static readonly Lazy<Config> _instance = new(() => new Config());
public static Config Instance => _instance.Value;
private Config() { /* load */ }
}
Trade-off: hidden global state, hostile to unit tests. Prefer DI.
```

**Q453.** What is the Factory Method pattern?

Trigger: 'I need to create objects of varying types, choice depends on input or context.' Method on a base class returns the right subtype.

**Q454.** What is Abstract Factory?

Interface that creates families of related objects. Use when you need to produce sets of objects that work together (e.g., Windows-style buttons + scrollbars vs Mac-style buttons + scrollbars).

**Q455.** Show a Factory pattern for payment processors.

Different processor based on payment method:

```csharp
public interface IPaymentProcessor {
Task<bool> ProcessAsync(decimal amount);
}
public class PaymentProcessorFactory {
public IPaymentProcessor Create(PaymentMethod method) => method switch {
PaymentMethod.Card => new StripeProcessor(),
PaymentMethod.PayPal => new PayPalProcessor(),
PaymentMethod.Bank => new BankTransferProcessor(),
_ => throw new NotSupportedException()
};
}
```

**Q456.** What is the Builder pattern?

Trigger: 'Constructing object with many optional parts; want readable, immutable result.' Common for query builders, configuration objects, fluent APIs.

**Q457.** Show a Builder pattern.

Fluent builder:

```csharp
var promo = new PromotionBuilder()
.WithName("BlackFriday")
.WithDiscount(0.20m)
.ValidFrom(start)
.ValidUntil(end)
.ForChannels("web", "mobile")
.Build();
Modern alternative: object initializers + record 'with' expressions.
```

**Q458.** What is Prototype pattern?

Create new objects by copying an existing prototype. In .NET: ICloneable interface, MemberwiseClone, or record types with 'with' expressions. Modern apps rarely need it explicitly.

### 6.2 Structural Patterns

**Q459.** What is the Adapter pattern?

Trigger: 'Class with wrong interface for my consumer.' Wrap incompatible API in your domain interface. Common when integrating legacy code or third-party libraries.

**Q460.** Show Adapter pattern.

Wrap legacy COM API in clean .NET interface:

```csharp
public interface IPromotionLegacyClient {
Task<Promotion> GetByIdAsync(string id);
}
public class LegacyComAdapter : IPromotionLegacyClient {
private readonly LegacyComObject _com;
public LegacyComAdapter(LegacyComObject com) => _com = com;
public Task<Promotion> GetByIdAsync(string id) {
var raw = _com.GetPromo(id); // ugly COM call
return Task.FromResult(MapToDomain(raw));
}
}
```

**Q461.** What is the Decorator pattern?

Trigger: 'Add cross-cutting behavior (logging, caching, retry) without modifying core class.' Wrap one implementation in another that adds behavior.

**Q462.** Show Decorator chaining.

Stack decorators for layered behavior:

```csharp
public class CachedRepo<T> : IRepo<T> {
private readonly IRepo<T> _inner;
private readonly IMemoryCache _cache;
public CachedRepo(IRepo<T> inner, IMemoryCache cache) {
_inner = inner;
_cache = cache;
}
public async Task<T> GetAsync(string id) =>
await _cache.GetOrCreateAsync(id, _ => _inner.GetAsync(id));
}
// Stack: Logged -> Cached -> Retry -> SqlRepo
var repo = new LoggedRepo<T>(
new CachedRepo<T>(
new RetryRepo<T>(
new SqlRepo<T>(db))));
```

**Q463.** What is the Facade pattern?

Trigger: 'Expose simpler API over complex subsystem.' OrderService.PlaceOrder() hides 5 internal services. Common in service layers.

**Q464.** What is the Proxy pattern?

Control access to an object. Variants: Lazy proxy (deferred initialization), Remote proxy (network call hidden), Protection proxy (security checks), Logging proxy. EF Core uses lazy proxies for navigation properties.

**Q465.** What is the Composite pattern?

Trigger: 'Part-whole hierarchies treated uniformly.' Examples: file system (folders contain files AND folders), UI tree (containers contain widgets AND containers). Same interface for both single and composite.

**Q466.** What is the Bridge pattern?

Decouple abstraction from implementation. Both can vary independently. Example: Shape (abstraction) + Renderer (implementation). 5 shapes × 3 renderers = 15 combinations from 8 classes instead of 15.

**Q467.** What is the Flyweight pattern?

Trigger: 'Many objects with shared state — pool the shared part.' String interning is a built-in flyweight in .NET. Use for: many similar small objects with shared immutable state.

### 6.3 Behavioral Patterns

**Q468.** What is the Strategy pattern?

Trigger: 'Many algorithms for same task, selectable at runtime.' Most-cited GoF pattern in design reviews. Common for: discount calculation, sorting, validation rules, payment processing.

**Q469.** Show Strategy pattern with DI.

Discount strategy in a checkout service:

```csharp
public interface IDiscountStrategy {
decimal Apply(Cart cart);
}
public class PercentageDiscount : IDiscountStrategy { ... }
public class BogoDiscount : IDiscountStrategy { ... }
public class FlatAmountDiscount : IDiscountStrategy { ... }
// Register all strategies
services.AddScoped<IDiscountStrategy, PercentageDiscount>();
services.AddScoped<IDiscountStrategy, BogoDiscount>();
// Inject all and apply each
public class CheckoutService(IEnumerable<IDiscountStrategy> strategies) {
public Money Total(Cart cart) =>
strategies.Aggregate(cart.Subtotal,
(acc, s) => acc - s.Apply(cart));
}
```

**Q470.** What is the Observer pattern?

Trigger: 'When this changes, multiple unrelated things need to react.' .NET implementations: events, IObservable<T>, MediatR domain events. In distributed systems: pub/sub via Service Bus topic.

**Q471.** What is the Mediator pattern?

Trigger: 'I want N components to talk through one hub instead of N×N.' Reduces coupling. MediatR library is the standard .NET implementation for in-process command/query handling.

**Q472.** Show MediatR command/query.

Decoupled command handler:

```csharp
// Command
public record CreateOrderCommand(string CustomerName, List<Item> Items)
: IRequest<Guid>;
// Handler (auto-discovered)
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid> {
private readonly AppDbContext _db;
public CreateOrderHandler(AppDbContext db) => _db = db;
public async Task<Guid> Handle(CreateOrderCommand cmd, CancellationToken ct) {
var order = new Order(cmd.CustomerName, cmd.Items);
_db.Orders.Add(order);
await _db.SaveChangesAsync(ct);
return order.Id;
}
}
// In controller
var orderId = await _mediator.Send(new CreateOrderCommand(name, items));
```

**Q473.** What is the Chain of Responsibility?

Pass request through a chain until something handles it. ASP.NET Core middleware is exactly this pattern. Each middleware decides to handle or pass to the next.

**Q474.** What is the Command pattern?

Encapsulate a request as an object. Enables: queueing, logging, undo, audit. Classic example: GUI menu items (each is a Command object). MediatR commands are this pattern.

**Q475.** What is the State pattern?

Behavior changes based on internal state. Replaces if (state == X) chains with state objects. Common for: order workflow, document approval, game characters. Each state knows its valid transitions.

**Q476.** What is the Template Method pattern?

Algorithm skeleton with subclass-supplied steps. Base class has the structure; subclasses fill in specific steps. Common in framework code (e.g., abstract Process method with template Run method calling it).

**Q477.** What is the Iterator pattern?

Sequential access to a collection without exposing structure. .NET's IEnumerable<T> + IEnumerator<T> ARE the iterator pattern. yield return generates iterators automatically.

**Q478.** What is the Visitor pattern?

Add operations to a class hierarchy without modifying the classes. Example: serializing different shapes to different formats (JSON, XML) — the shapes don't know about formats. Use sparingly; pattern matching often replaces it now.

**Q479.** What is the Memento pattern?

Capture and restore an object's state. Used for: undo/redo, save points, snapshots. Stores state externally, restoring later without exposing internals.

### 6.4 Enterprise Patterns

**Q480.** What is the Repository pattern?

Trigger: 'I want my domain to think in collections, not SQL.' Hides persistence behind a collection-like interface (Get/Find/Add/Remove). Aggregate roots have repos.

**Q481.** Show Repository pattern.

Repository for Order aggregate:

```csharp
public interface IOrderRepository {
Task<Order?> GetByIdAsync(Guid id);
Task<IReadOnlyList<Order>> FindByStatusAsync(OrderStatus status);
Task AddAsync(Order order);
Task DeleteAsync(Order order);
}
public class EfOrderRepository : IOrderRepository {
private readonly AppDbContext _db;
public EfOrderRepository(AppDbContext db) => _db = db;
public Task<Order?> GetByIdAsync(Guid id) =>
_db.Orders.FirstOrDefaultAsync(o => o.Id == id);
// ... etc
}
Trade-off: EF Core's DbContext IS already a Repository + Unit of Work. Wrapping adds a layer. Build only if you'll swap data stores or need clean DDD boundaries.
```

**Q482.** What is Unit of Work?

Tracks changes across multiple repositories within a single transaction. Commits all atomically. EF Core's DbContext implements UoW by default — SaveChangesAsync is the commit.

**Q483.** What is CQRS?

Command Query Responsibility Segregation. Split write model (commands) from read model (queries). Different schemas, different stores, different scaling. Common with MediatR. Don't apply everywhere — only when read shape differs significantly from write.

**Q484.** What is the Saga pattern?

Long-running business transaction across multiple services using local transactions and compensating actions. Two flavors: Orchestration (central coordinator) and Choreography (services emit/react to events).

**Q485.** What is the Outbox pattern?

Solves dual-write: 'update DB AND publish message atomically.' Insert into outbox table in same DB transaction. Background poller reads outbox, publishes to bus, marks sent. Inbox table on consumer side for dedup.

**Q486.** Show Outbox pattern.

Atomic write of business state + outbox message:

```csharp
// Same DB transaction
BEGIN TRAN
INSERT INTO Orders (...);
INSERT INTO Outbox (Id, Type, Payload, Status='pending');
COMMIT
// Background worker
while (true) {
var msgs = SELECT TOP 100 * FROM Outbox WHERE Status='pending';
foreach (var m in msgs) {
await _bus.PublishAsync(m);
UPDATE Outbox SET Status='sent' WHERE Id = m.Id;
}
await Task.Delay(1000);
}
```

**Q487.** What is the Strangler Fig pattern?

Migrate legacy system incrementally by routing traffic through a facade that progressively delegates to new code. Old system shrinks, new system grows. Used in your VB6→.NET migration.

**Q488.** What is the Anti-Corruption Layer (ACL)?

Translation boundary between your bounded context and someone else's model (legacy, third-party). Adapter classes convert vendor types to your domain types. Prevents foreign concepts from leaking.

**Q489.** What is the Backend for Frontend (BFF)?

Per-client API layer that aggregates downstream calls and shapes responses for one specific client (mobile, web, partner). Use for 2+ distinct client types with different needs.

**Q490.** What is the Sidecar pattern?

Helper container deployed alongside main app, sharing pod/process. Provides cross-cutting features (logging, mTLS, config) without modifying app. Common in K8s service meshes.

**Q491.** What is the Cache-Aside pattern?

App checks cache first; on miss, reads DB, populates cache. On data change, invalidate. Most common caching pattern. Use Redis.

**Q492.** What is the Circuit Breaker pattern?

State machine: Closed (calls flow), Open (fail fast), Half-Open (probe to check recovery). Prevents cascade when downstream is failing. Without it, retries make outages worse.

**Q493.** What is the Bulkhead pattern?

Isolate resources so one failing dependency doesn't exhaust shared pools. Like ship bulkheads — flooded compartment doesn't sink the ship. In .NET: separate HttpClient or thread pool per dependency.

**Q494.** What is the Materialized View pattern?

Pre-compute query-optimized projection. Updated via events. Trades storage and complexity for read speed. Common in CQRS.

**Q495.** What is Event Sourcing?

Store events instead of current state. State = fold(events). Provides full audit, time travel, ability to replay. Heavy — apply only when audit/replay is genuinely needed. Most CRUD apps don't need it.

### 6.5 SOLID Principles

**Q496.** What is SRP (Single Responsibility Principle)?

A class should have one reason to change. 'Reason' means actor/stakeholder driving change. Class modified by two unrelated tickets in same sprint = should be split.

**Q497.** What is OCP (Open/Closed Principle)?

Software entities should be open for extension, closed for modification. New behavior arrives by adding new code, not editing tested code. Mechanism: polymorphism + DI.

**Q498.** What is LSP (Liskov Substitution)?

Subtypes must be substitutable for base types without breaking callers. Classic violation: Square inherits from Rectangle, but setting width also sets height. Code expecting Rectangle behavior breaks.

**Q499.** What is ISP (Interface Segregation)?

Clients shouldn't depend on methods they don't use. Big god-interfaces force every consumer to mock unused methods. Refactor: IUserService with 30 methods → IUserAuthenticator + IUserProfile + IUserAdmin.

**Q500.** What is DIP (Dependency Inversion)?

High-level policy depends on abstractions, not concrete details. Business code accepts IRepo<T>, not EfRepo<T>. Most important SOLID principle for architects — enables testing, swappable infrastructure, clean architecture.

**Q501.** Show DIP in .NET DI.

Constructor injection of interface, not concrete:

```csharp
// BAD: depends on concrete EF
public class OrderService {
private readonly EfOrderRepository _repo;
public OrderService() { _repo = new EfOrderRepository(); }
}
// GOOD: depends on abstraction
public class OrderService(IOrderRepository repo) {
public async Task<Order> GetAsync(Guid id) =>
await repo.GetByIdAsync(id);
}
// In Program.cs
services.AddScoped<IOrderRepository, EfOrderRepository>();
```

**Q502.** What's DRY?

Don't Repeat Yourself. Each piece of knowledge has one authoritative representation. Caveat: don't over-DRY. Three similar things may have one underlying concept (DRY) or three independent concepts that look similar today (don't DRY).

**Q503.** What's KISS?

Keep It Simple, Stupid. Most apps don't need 7 layers of abstraction. Start simple. Add complexity only when proven necessary. The smartest engineers I know write the simplest code.

**Q504.** What's YAGNI?

You Aren't Gonna Need It. Don't build features 'just in case.' Build what's needed now. Future requirements often differ from what you imagined. Premature flexibility = waste + complexity.

**Q505.** What's the Law of Demeter?

'Don't talk to strangers.' An object should call methods on: itself, its parameters, objects it created, its direct fields. Avoid order.Customer.Address.City. Reduces coupling.

**Q506.** What's separation of concerns?

Each part of code addresses one concern (data access, business logic, presentation). Clean Architecture / Hexagonal apply this rigorously. Result: easy to change one without affecting others.

**Q507.** What's a code smell?

Surface indication of deeper problem. Examples: long methods, large classes, primitive obsession, feature envy, shotgun surgery (one change requires many file edits), data clumps. Each smell suggests specific refactorings.

**Q508.** What's technical debt?

Cost of choosing easy now over hard right. Like financial debt: interest accrues. Some debt is intentional (ship MVP, fix later). Some accidental (didn't know better). Track it, prioritize paydown by impact.

**Q509.** What's an Anti-Pattern?

Common solution that looks reasonable but causes problems. Examples: God Class (one class does everything), Spaghetti Code, Magic Numbers (hardcoded values), Premature Optimization, Reinventing the Wheel.

**Q510.** When do you use a pattern vs simpler code?

Patterns add structure but cost complexity. Use a pattern when: (1) you have a real recurring problem, (2) the pattern's complexity is justified by the benefit, (3) future changes are likely. Don't pattern-everything just to look clever.

## SECTION 7 — ML.NET (Q511-540)

ML.NET is Microsoft's machine learning framework for .NET developers. Lets you train, deploy, and use ML models in C#/F# without learning Python.

### 7.1 ML.NET Basics

**Q511.** What is ML.NET?

Open-source, cross-platform ML framework for .NET. Lets C#/F# developers add ML to apps without Python. Covers: classification, regression, clustering, anomaly detection, recommendation, image classification, time series.

**Q512.** What can ML.NET do?

Binary classification: spam/not-spam, fraud/legit.

Multi-class classification: categorize into N classes (sentiment, topic).

Regression: predict numbers (price, sales).

Clustering: group similar items (customer segments).

Anomaly detection: spot outliers (security, equipment failure).

Recommendation: collaborative filtering.

Forecasting: time series predictions.

Image classification: via TensorFlow/ONNX integration.

**Q513.** When should I use ML.NET vs Python?

ML.NET: existing .NET stack, want type safety, deploy in same app process, simpler scenarios.

Python: cutting-edge research, complex deep learning, large team of data scientists, latest models.

Pragmatic: train in Python with PyTorch/TensorFlow, export to ONNX, use ML.NET to load and run inference. Best of both.

**Q514.** What's a Model in ML.NET?

Trained algorithm that makes predictions. You train once on historical data, save (.zip), load in your app, call Predict() per request. The trained model is small (KBs to MBs typically).

**Q515.** What's a Pipeline in ML.NET?

Sequence of data transformations + a learner (algorithm). Defines how raw data becomes predictions. Classic pipeline: load → featurize → normalize → train → evaluate.

**Q516.** Show a basic ML.NET sentiment classification.

Train a binary classifier:

```csharp
// Define data classes
public class SentimentData {
[LoadColumn(0)] public string Text { get; set; }
[LoadColumn(1)] public bool Label { get; set; } // true = positive
}
public class SentimentPrediction {
[ColumnName("PredictedLabel")] public bool Prediction { get; set; }
public float Probability { get; set; }
public float Score { get; set; }
}
// Train
var ml = new MLContext();
var data = ml.Data.LoadFromTextFile<SentimentData>("reviews.tsv",
hasHeader: true);
var pipeline = ml.Transforms.Text
.FeaturizeText("Features", "Text")
.Append(ml.BinaryClassification.Trainers
.SdcaLogisticRegression());
var model = pipeline.Fit(data);
ml.Model.Save(model, data.Schema, "sentiment.zip");
// Use
var engine = ml.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
var pred = engine.Predict(new SentimentData { Text = "This is great!" });
Console.WriteLine($"Positive: {pred.Prediction}, Probability: {pred.Probability}");
```

**Q517.** What's MLContext?

The starting point for all ML.NET operations. Like DbContext for EF Core. Holds: Data (loading), Transforms (feature engineering), Trainers (algorithms), Model (save/load), evaluation.

**Q518.** How do I evaluate model accuracy?

Split data into train/test (typically 80/20). Train on train set. Predict on test set. Measure: Accuracy, Precision, Recall, F1 (classification); RMSE, R² (regression).

**Q519.** Show train/test split and evaluation.

Standard pattern:

```csharp
var split = ml.Data.TrainTestSplit(data, testFraction: 0.2);
var trainData = split.TrainSet;
var testData = split.TestSet;
var model = pipeline.Fit(trainData);
var predictions = model.Transform(testData);
var metrics = ml.BinaryClassification.Evaluate(predictions);
Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
Console.WriteLine($"F1 Score: {metrics.F1Score:P2}");
Console.WriteLine($"AUC: {metrics.AreaUnderRocCurve:P2}");
```

**Q520.** What's overfitting?

Model learns training data too well, including its noise. Performs great on train, bad on new data. Mitigations: more training data, regularization, simpler model, cross-validation, early stopping.

**Q521.** What's underfitting?

Model is too simple to capture the patterns. Bad on both train and test. Mitigations: more features, more complex model, train longer, less regularization.

**Q522.** What's cross-validation?

Train+test split is one sample. Cross-validation does it K times with different splits, averaging results. More reliable estimate of model performance. ML.NET: ml.BinaryClassification.CrossValidate(data, pipeline, numberOfFolds: 5).

**Q523.** What's feature engineering?

Transforming raw data into features the model can use. Examples: text → TF-IDF vectors, dates → day-of-week + month, categorical → one-hot encoding, numbers → normalized.

**Q524.** What ML.NET data transformations are common?

FeaturizeText: text → numeric vector.

OneHotEncoding: categorical → binary columns.

NormalizeMinMax / NormalizeMeanVariance: scale numbers.

Concatenate: combine multiple columns into one Features vector.

MissingValueReplace: fill NaN with mean/min/max.

ConvertType: change column type.

**Q525.** What's AutoML in ML.NET?

Automatically tries many algorithms and feature engineering combinations to find the best for your data. Run with Microsoft.ML.AutoML package. Saves you from manual algorithm selection. Best for scenarios where you don't know what works.

**Q526.** Show AutoML basic usage.

Auto-pick best classification model:

```csharp
var experiment = ml.Auto()
.CreateBinaryClassificationExperiment(maxExperimentTimeInSeconds: 300);
var result = experiment.Execute(trainData, labelColumnName: "Label");
Console.WriteLine($"Best model: {result.BestRun.TrainerName}");
Console.WriteLine($"Accuracy: {result.BestRun.ValidationMetrics.Accuracy:P2}");
ml.Model.Save(result.BestRun.Model, trainData.Schema, "model.zip");
```

**Q527.** What's PredictionEngine?

Object that runs predictions one row at a time. Simple to use but not thread-safe. For multi-threaded scenarios, use PredictionEnginePool (registered in DI as singleton).

**Q528.** Show PredictionEnginePool usage in ASP.NET Core.

Thread-safe predictions in a web app:

```csharp
// In Program.cs
builder.Services.AddPredictionEnginePool<SentimentData, SentimentPrediction>()
.FromFile(modelName: "sentiment",
filePath: "model.zip",
watchForChanges: true);
// In controller
[ApiController, Route("sentiment")]
public class SentimentController(
PredictionEnginePool<SentimentData, SentimentPrediction> pool) : ControllerBase
{
[HttpPost]
public IActionResult Analyze([FromBody] string text)
{
var pred = pool.Predict("sentiment", new SentimentData { Text = text });
return Ok(new { positive = pred.Prediction, probability = pred.Probability });
}
}
```

**Q529.** Can ML.NET use deep learning models?

Yes, via ONNX integration. Train in PyTorch / TensorFlow → export to ONNX → load in ML.NET → run inference. Common for: image classification, NLP. ML.NET also has built-in ImageClassification trainer using TensorFlow.

**Q530.** What's ONNX?

Open Neural Network Exchange. Standard format for ML models. Train in any framework (PyTorch, TensorFlow, scikit-learn), export to ONNX, run anywhere (ML.NET, ONNX Runtime, mobile). Decouples training from deployment.

**Q531.** How do I deploy an ML.NET model?

Save model: ml.Model.Save(model, schema, "model.zip").

Include zip in your app deployment.

Load at startup with PredictionEnginePool.

Call Predict() per request.

For updates: replace zip file, watchForChanges=true reloads.

**Q532.** How big are ML.NET models?

Small. Linear models: KBs. Tree models: usually under 50MB. Deep learning models (via ONNX): can be 100MB+. Practical concern: load time and memory during prediction.

**Q533.** What's Model Builder?

Visual Studio extension. Wizard-style UI for training ML.NET models. Lets non-experts train classification, regression, image classification, object detection. Generates ML.NET code you can extend.

**Q534.** What real STERIS-like uses fit ML.NET?

Predictive maintenance: predict device failures from telemetry.

Anomaly detection: flag unusual sterilization cycles.

Demand forecasting: predict consumable usage by hospital.

Quality classification: pass/fail from sensor data.

Customer segmentation: group hospitals by usage patterns.

**Q535.** What are the limits of ML.NET?

Smaller community than Python ecosystem.

Fewer cutting-edge algorithms (e.g., latest transformer models often Python-first).

Less ecosystem for deep learning research.

For simple-to-mid complexity ML in production: ML.NET is excellent.

For research or cutting-edge: Python with PyTorch.

**Q536.** How do I version ML models?

Treat models like code: store in Git or Azure ML Model Registry. Version with semver. Track training data version too. CI/CD pipeline trains, evaluates, promotes if metrics meet threshold.

**Q537.** What's MLOps?

DevOps for ML. CI/CD pipelines for model training, evaluation, deployment, monitoring. Tools: Azure ML, MLflow, GitHub Actions. Important: monitor model drift in production (predictions degrading over time).

**Q538.** What's model drift?

Model accuracy degrades over time as real-world data shifts. Example: fraud patterns evolve, sentiment language changes. Solution: continuous monitoring + retraining schedule (weekly/monthly).

**Q539.** How do I explain a model's predictions?

Feature Importance: which features drive predictions. ML.NET provides Permutation Feature Importance. For deep models: SHAP values (via ONNX Runtime). Important for: regulated industries, debugging, trust.

**Q540.** Should I retrain every time?

No. Retrain when: accuracy drops below threshold (drift), new significant data available, business rules change. Most production models retrain weekly or monthly. Continuous training is overkill for most cases.

