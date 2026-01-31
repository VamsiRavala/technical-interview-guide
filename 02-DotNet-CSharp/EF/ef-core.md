# EF Core Cheat Sheet

## 📦 Migration Commands (CLI)
- `dotnet ef migrations add <Name>` : Add a new migration
- `dotnet ef migrations remove` : Remove last migration
- `dotnet ef migrations list` : List migrations
- `dotnet ef database update` : Apply all pending migrations
- `dotnet ef database update <Migration>` : Roll forward/backward to specific migration
- `dotnet ef database update 0` : Roll back all migrations
- `dotnet ef migrations script` : Generate SQL script for migrations

## 🏗️ Key Classes
- `DbContext` : Main database context for entities and queries
- `DbSet<TEntity>` : Represents a table
- `Migration` : Base class for schema changes (`Up`/`Down`)
- `MigrationBuilder` : Helper to build migration commands
- `ModelBuilder` : Configures entities before migrations

## 💡 Useful DbContext Methods
- `context.SaveChanges()` : Save all changes synchronously
- `await context.SaveChangesAsync()` : Save all changes asynchronously
- `context.Database.Migrate()` : Apply all migrations at runtime
- `context.Database.EnsureCreated()` : Create DB if not exists
- `context.Database.EnsureDeleted()` : Delete DB
- `context.Database.GetPendingMigrations()` : List pending migrations
- `context.Database.GetAppliedMigrations()` : List applied migrations

## 🔹 Common MigrationBuilder Methods
- `CreateTable`, `DropTable`, `AddColumn<T>`, `DropColumn`, `AlterColumn<T>`, `RenameColumn`, `RenameTable`
- `CreateIndex`, `DropIndex`, `AddForeignKey`, `DropForeignKey`
- `Sql("RAW SQL")` : Execute raw SQL

# EF Core Features Explained

## LINQ Improvements
EF Core translates complex LINQ queries efficiently to SQL.
```csharp
var orders = context.Orders.Where(o => o.Total > 100).ToList();
```

## Global Query Filters
Automatically filter entities based on conditions (soft delete, tenant filters).
```csharp
modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
```

## Shadow Properties
Properties defined in the model but not in the entity class.
```csharp
modelBuilder.Entity<Order>().Property<DateTime>("CreatedDate");
```

## Batch Updates/Deletes (EF Core 7+)
Update or delete multiple rows without loading them.
```csharp
context.Products.Where(p => p.Stock == 0).ExecuteDelete();
context.Products.Where(p => p.Price < 10)
       .ExecuteUpdate(p => p.SetProperty(x => x.Price, x => x.Price * 1.1));
```

## No-Tracking Queries
Fast read-only queries that do not track changes.
```csharp
var products = context.Products.AsNoTracking().ToList();
```

## Better Async Performance
Use async methods to avoid blocking threads.
```csharp
var orders = await context.Orders.ToListAsync();
```

## Owned Entity Types
Value objects that are stored in the same table as the owner entity.
```csharp
modelBuilder.Entity<Order>().OwnsOne(o => o.ShippingAddress);
```

## Keyless Entities
Entities without a primary key, useful for views or raw SQL queries.
```csharp
modelBuilder.Entity<SalesReport>().HasNoKey();
```

## Compiled Queries
Precompile LINQ queries for performance on repeated execution.
```csharp
static readonly var query = EF.CompileQuery((AppDbContext ctx, decimal minTotal) =>
    ctx.Orders.Where(o => o.Total > minTotal));
```

# EF Core: Working with Existing Database and Bringing Changes

## 1️⃣ Scaffold the Existing Database
If you haven’t already, generate your EF Core models from the existing database:

```bash
dotnet ef dbcontext scaffold "Your_Connection_String" Microsoft.EntityFrameworkCore.SqlServer -o Models -f
```
- `-o Models` → output folder for entities
- `-f` → force overwrite if the folder already exists

> Generates `DbContext` and entity classes for all tables in the DB.

---

## 2️⃣ Make Your Changes in the Database
Suppose your old DB has a new column in a table that you want to bring in:

```sql
ALTER TABLE Products ADD Description NVARCHAR(500) NULL;
```

---

## 3️⃣ Update Your EF Core Model
### Option A: Re-scaffold Entire DB (Overwrite)
```bash
dotnet ef dbcontext scaffold "Your_Connection_String" Microsoft.EntityFrameworkCore.SqlServer -o Models -f
```
- Refreshes all models including the new column
- Downside: overwrites any manual changes in entity classes

### Option B: Scaffold Only the Affected Table
```bash
dotnet ef dbcontext scaffold "Your_Connection_String" Microsoft.EntityFrameworkCore.SqlServer -o Models -t Products -f
```
- `-t Products` → only scaffold the `Products` table
- Brings in only the changes for that table, keeping other classes intact

---

## 4️⃣ Verify Changes
Check the entity class for the new column:
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }  // new column
}
```

---

## 5️⃣ Optional: Add Migration for Further Changes
```bash
dotnet ef migrations add AddProductDescription
dotnet ef database update
```
- In Database-First, migrations are optional; scaffolding reflects the DB schema.
- Be careful: DB and models should match exactly if using migrations.

---

## ✅ Summary
- Use `dotnet ef dbcontext scaffold` for existing DB.
- Bring in **one table or change** using `-t TableName`.
- Manual changes in other entities are safe if you scaffold only the affected table.
- Optionally, use migrations for future schema changes.

# EF Core vs ADO.NET: When to Use What

| Scenario / Requirement                     | EF Core Ideal?               | ADO.NET Ideal?                         | Notes / Comments                                                                 |
|-------------------------------------------|-----------------------------|---------------------------------------|---------------------------------------------------------------------------------|
| Rapid development / productivity          | ✅ Yes                      | ❌ No                                  | EF Core generates entities, supports LINQ, change tracking, and relationships.  |
| Medium-sized database                      | ✅ Yes                      | Possible, but manual work required    | EF Core handles CRUD and relationships efficiently.                              |
| Very large database (millions of rows)    | ⚠ Careful                  | ✅ Yes                                 | EF Core tracks entities in memory; ADO.NET streaming (`SqlDataReader`) is better.|
| Complex business logic with relationships | ✅ Yes                      | ⚠ Harder                               | EF Core supports navigation properties, FK constraints, and lazy loading.       |
| Performance-critical queries               | ⚠ Benchmark first           | ✅ Yes                                 | EF Core has translation and tracking overhead; raw SQL in ADO.NET is faster.     |
| Migrations / schema evolution             | ✅ Yes                      | ❌ No                                  | EF Core migrations automate schema changes; ADO.NET requires manual scripts.    |
| Cross-platform .NET Core                   | ✅ Yes                      | ✅ Yes                                 | Both EF Core and ADO.NET support .NET Core/.NET 5+.                             |
| Async queries / scalable apps              | ✅ Yes                      | ✅ Yes (with async ADO.NET methods)   | EF Core async methods are easier; ADO.NET also supports `ExecuteReaderAsync()`. |
| Working with stored procedures / views     | ✅ Yes                      | ✅ Yes                                 | EF Core allows raw SQL and keyless entities; ADO.NET works natively.            |
| Legacy systems / existing SQL code        | ⚠ Careful                   | ✅ Yes                                 | ADO.NET gives full control and integrates easily with legacy code.              |

## Equivalent of ADO.NET in .NET Core

- **Namespace:** `System.Data`
- **SQL Server provider:** `Microsoft.Data.SqlClient`
- **Key Classes:** `SqlConnection`, `SqlCommand`, `SqlDataReader`, `SqlDataAdapter`

# EF Core: How LINQ Becomes SQL — Performance Pitfalls Cheatsheet

> Focus: **what EF Core translates**, **where it can’t**, and **how to avoid slow queries**.

---

## 1) Translation Basics (Mental Model)

| Stage | What EF Does | Why it matters |
|---|---|---|
| Build Expression | You write LINQ; EF builds an expression tree | Shapes what EF *can* translate |
| Translate | EF maps expression to SQL (provider-specific) | Unsupported nodes → client eval/materialization |
| Execute | SQL sent to DB; results materialized into CLR | Data volume + shape drive perf |
| Track | (Default) entities tracked for changes | Tracking adds CPU/memory; use `AsNoTracking` if read-only |

---

## 2) Common LINQ → SQL Patterns (and Gotchas)

| LINQ | Typical SQL | Notes / Pitfalls |
|---|---|---|
| `Where` | `WHERE` | Fine; prefer server-side predicates |
| `Select` | `SELECT <projection>` | Project **only needed columns** |
| `OrderBy`/`ThenBy` | `ORDER BY` | Must exist **before** `Skip/Take` |
| `Skip/Take` | `OFFSET … FETCH` | Requires deterministic order |
| `Any(predicate)` | `EXISTS (SELECT 1 …)` | Prefer to `Count() > 0` |
| `Count()` | `COUNT(*)` | Avoid materializing then counting |
| `Contains(list)` | `IN (…)` or join | Watch very large lists (parameter explosion) |
| `GroupBy` | `GROUP BY` **or** client-side | Many post-group projections **fall back to client** unless aggregate-only |
| `Join` | `INNER/LEFT JOIN` | Navigation filtering + `Include` can cause *cartesian blowups* |
| `SelectMany` | `CROSS APPLY`/`JOIN` | OK; but watch multiplicative rows |
| String ops (`StartsWith`) | `LIKE 'abc%'` | `ToLower()` may block indexes; use `EF.Functions.ILike`/collations where supported |
| Date ops (`Date`, `AddDays`) | Provider function (`CAST`, `DATEADD`) | Some members aren’t translatable → client |
| `Distinct()` | `DISTINCT` | Remove *after* projecting required columns |

---

## 3) The Big Pitfalls (Symptoms → Fix)

| Pitfall | Symptom | Root Cause | Fix |
|---|---|---|---|
| **N+1 due to lazy loading** | Many tiny SQL calls per loop | Accessing nav props per row | Use **eager loading** (`Include`/`ThenInclude`), or **explicit loading**, or **disable lazy loading** |
| **Cartesian explosion with `Include`** | Huge result set; duplicate parent rows | Multiple `Include` on collections in one SQL | Use `AsSplitQuery()` or project DTOs; avoid over-including |
| **Client evaluation** | Results fetched, then filtered in memory | Non-translatable methods/expressions | Keep predicates **translatable**; move logic to DB functions or computed columns |
| **Projecting entire entities** | Wide `SELECT *` + slow materialization | Selecting more than needed | Project **DTOs/anonymous** with only used fields |
| **`Skip/Take` without order** | Unstable pagination | Missing ORDER BY | Always `OrderBy` before paging |
| **`Contains` on big lists** | Slow, huge SQL/params | `IN` with long parameter lists | Use **temp table**, **table-valued params**, or chunked queries |
| **String case transforms in predicates** | Indexes not used | `LOWER(Column)` prevents index seek | Use case-insensitive collation/index; avoid wrapping columns |
| **Tracking overhead** | High CPU/memory for read-only pages | Default tracking | Use `AsNoTracking()` or `AsNoTrackingWithIdentityResolution()` |
| **`GroupBy` unexpected client work** | Large memory, slow | Post-aggregate projection not translatable | Do **aggregates in SQL**; project grouped aggregates only |
| **Multiple roundtrips in loops** | Many `await context.Set…` inside loops | Per-item queries | Batch with `Where(x in ids)`, joins, or **set-based** operations |
| **`Include` + `Where` on child** | Filter ignored/partial | `Include` is for shape, not filter | Filter **parent** by child with `Where`, then `Select` child projection; or use filtered include if supported |
| **JSON/complex conversions** | Client eval or heavy cast | Provider lacks translation | Map columns properly; use raw SQL if needed |
| **Parameter sniffing surprises** | Plan mismatch for outliers | Parameterization | Add reasonable filters; sometimes `FromSql`/hints, or split cases |

---

## 4) Loading Strategies (choose wisely)

| Strategy | How | Pros | Cons | Use When |
|---|---|---|---|---|
| **Eager** | `.Include/ThenInclude` | One or few roundtrips, simple | Can over-fetch; cartesian risk | Small graphs needed immediately |
| **Explicit** | `context.Entry(parent).Collection(p=>p.Children).Load()` | Controlled fetch | Extra code/roundtrips | Conditional/partial graphs |
| **Lazy** | Proxies/virtual + lazy loading | Minimal upfront code | N+1 trap, hidden IO | Rarely; only for simple/low-volume |

---

## 5) Projection Patterns (fast paths)

**Bad (wide & tracked):**
```csharp
var orders = await ctx.Orders
    .Include(o => o.Customer)
    .Include(o => o.Items)
    .ToListAsync();
```

**Better (narrow DTO, no tracking):**
```csharp
var orders = await ctx.Orders.AsNoTracking()
    .Where(o => o.Status == OrderStatus.Ready)
    .OrderBy(o => o.CreatedAt)
    .Select(o => new OrderDto {
        Id = o.Id,
        CustomerName = o.Customer.Name,
        Total = o.Items.Sum(i => i.Price * i.Qty)
    })
    .ToListAsync();
```

---

## 6) Pagination (stable & efficient)

```csharp
var page = await ctx.Products.AsNoTracking()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Id)           // Must be deterministic
    .Skip((pageIndex - 1) * pageSize)
    .Take(pageSize)
    .Select(p => new { p.Id, p.Name, p.Price })
    .ToListAsync();
```

---

## 7) Aggregates & Grouping (server-side)

```csharp
var totals = await ctx.OrderItems
    .Where(i => i.OrderDate >= from && i.OrderDate < to)
    .GroupBy(i => i.ProductId)
    .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Qty) })
    .ToListAsync();
```

---

## 8) Searching & “IN” Lists

```csharp
// OK for small lists
var hits = await ctx.Users
    .Where(u => ids.Contains(u.Id))
    .ToListAsync();
```

For large lists → use **temp tables**, **table-valued params**, or **chunking**.

---

## 9) Strings, Dates, Nulls

| Pattern | Prefer | Avoid | Why |
|---|---|---|---|
| Case-insensitive search | Collation/index, `EF.Functions.ILike` (PG) | `ToLower(Column)` | Keeps index usage |
| StartsWith/EndsWith | `StartsWith("abc")` | `Contains("^abc")` hacks | Translates to `LIKE 'abc%'` |
| Date filters | Range on full `DateTime` | `where x.Date == targetDate.Date` | Casting column kills indexes |
| Null checks | `== null` / `!= null` | Complex coalescing | Simple translates best |

---

## 10) Tracking & Change Detection

| Mode | Call | When |
|---|---|---|
| No tracking | `AsNoTracking()` | Read-only lists |
| No tracking + fixups | `AsNoTrackingWithIdentityResolution()` | Read-only graphs with shared refs |
| Disable change detection | `context.ChangeTracker.AutoDetectChangesEnabled = false` | Bulk ops |

---

## 11) Split vs Single Query

```csharp
var data = await ctx.Parents
    .Include(p => p.Children)
    .Include(p => p.Tags)
    .AsSplitQuery() // avoid cartesian explosion
    .ToListAsync();
```

---

## 12) Compiled Queries

```csharp
static readonly Func<AppDbContext,int,Task<Product?>> GetProduct =
    EF.CompileAsyncQuery((AppDbContext db, int id) =>
        db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id));
```

---

## 13) Raw SQL

```csharp
var items = await ctx.Items
    .FromSqlInterpolated($"SELECT TOP {take} * FROM Items WHERE CreatedAt >= {from} AND CreatedAt < {to}")
    .AsNoTracking()
    .ToListAsync();
```

---

## 14) Set-based Updates/Deletes

```csharp
await ctx.Orders
    .Where(o => o.Status == Status.Stale)
    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, Status.Archived));

await ctx.Logs
    .Where(l => l.CreatedAt < cutoff)
    .ExecuteDeleteAsync();
```

---

## 15) Diagnostics

| Tool | What | Why |
|---|---|---|
| `ToQueryString()` | See SQL from LINQ | Debug translation |
| Logging (`EnableSensitiveDataLogging`) | Show SQL + params | Perf debugging |
| `TagWith("hint")` | Annotate SQL | Easier tracing |
| EF Analyzers | Compile-time hints | Catch bad LINQ |
| DB plans/statistics | Query plan | Confirm indexes |

---

## 16) Smell Checklist

- [ ] Only needed columns selected?  
- [ ] `OrderBy` before `Skip/Take`?  
- [ ] Includes minimized?  
- [ ] Any client eval warnings?  
- [ ] Loops issuing queries?  
- [ ] NoTracking used for reads?  
- [ ] SplitQuery for multi-collections?  
- [ ] Set-based updates/deletes?  

---

## 17) Bad → Good Examples

**N+1 (bad)**
```csharp
var customers = await ctx.Customers.ToListAsync();
foreach (var c in customers)
    Console.WriteLine(c.Orders.Count); // 1 query per customer
```

**Eager load (good)**
```csharp
var counts = await ctx.Customers
    .Select(c => new { c.Id, Orders = c.Orders.Count })
    .ToListAsync();
```

**Fat projection (bad)**
```csharp
var products = await ctx.Products.ToListAsync();
```

**Slim DTO (good)**
```csharp
var products = await ctx.Products
   .Select(p => new { p.Id, p.Name, p.Price })
   .AsNoTracking()
   .ToListAsync();
```

---

### ✅ Bottom Line
- LINQ **translates to SQL**, but only what EF can express.  
- Beware **N+1, Includes, GroupBy, and client evaluation**.  
- Always check `ToQueryString()` and DB query plans.  
- Think **set-based, narrow, ordered, no surprises**.  




