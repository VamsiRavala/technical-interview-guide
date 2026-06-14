# Entity Framework Core — Deep Dive

A practical reference for EF Core: migrations and core APIs, feature set, change tracking, loading strategies (lazy / eager / explicit), LINQ-to-SQL translation and performance pitfalls, and optimistic concurrency handling.

> Note: the newest EF Core 9 features are covered separately in `05-ef-core-9.md`. This document focuses on the deep, evergreen concepts.

---

## Migration Commands (CLI)

- `dotnet ef migrations add <Name>` — Add a new migration
- `dotnet ef migrations remove` — Remove last migration
- `dotnet ef migrations list` — List migrations
- `dotnet ef database update` — Apply all pending migrations
- `dotnet ef database update <Migration>` — Roll forward/backward to a specific migration
- `dotnet ef database update 0` — Roll back all migrations
- `dotnet ef migrations script` — Generate SQL script for migrations

---

## Key Classes

- `DbContext` — Main database context for entities and queries
- `DbSet<TEntity>` — Represents a table
- `Migration` — Base class for schema changes (`Up`/`Down`)
- `MigrationBuilder` — Helper to build migration commands
- `ModelBuilder` — Configures entities before migrations

### Useful DbContext Methods
- `context.SaveChanges()` — Save all changes synchronously
- `await context.SaveChangesAsync()` — Save all changes asynchronously
- `context.Database.Migrate()` — Apply all migrations at runtime
- `context.Database.EnsureCreated()` — Create DB if not exists
- `context.Database.EnsureDeleted()` — Delete DB
- `context.Database.GetPendingMigrations()` — List pending migrations
- `context.Database.GetAppliedMigrations()` — List applied migrations

### Common MigrationBuilder Methods
- `CreateTable`, `DropTable`, `AddColumn<T>`, `DropColumn`, `AlterColumn<T>`, `RenameColumn`, `RenameTable`
- `CreateIndex`, `DropIndex`, `AddForeignKey`, `DropForeignKey`
- `Sql("RAW SQL")` — Execute raw SQL

---

## EF Core Features

### LINQ Improvements
EF Core translates complex LINQ queries efficiently to SQL.
```csharp
var orders = context.Orders.Where(o => o.Total > 100).ToList();
```

### Global Query Filters
Automatically filter entities based on conditions (soft delete, tenant filters).
```csharp
modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
```

### Shadow Properties
Properties defined in the model but not in the entity class.
```csharp
modelBuilder.Entity<Order>().Property<DateTime>("CreatedDate");
```

### Batch Updates/Deletes (EF Core 7+)
Update or delete multiple rows without loading them.
```csharp
context.Products.Where(p => p.Stock == 0).ExecuteDelete();
context.Products.Where(p => p.Price < 10)
       .ExecuteUpdate(p => p.SetProperty(x => x.Price, x => x.Price * 1.1));
```

### No-Tracking Queries
Fast read-only queries that do not track changes.
```csharp
var products = context.Products.AsNoTracking().ToList();
```

### Better Async Performance
Use async methods to avoid blocking threads.
```csharp
var orders = await context.Orders.ToListAsync();
```

### Owned Entity Types
Value objects stored in the same table as the owner entity.
```csharp
modelBuilder.Entity<Order>().OwnsOne(o => o.ShippingAddress);
```

### Keyless Entities
Entities without a primary key, useful for views or raw SQL queries.
```csharp
modelBuilder.Entity<SalesReport>().HasNoKey();
```

### Compiled Queries
Precompile LINQ queries for performance on repeated execution.
```csharp
static readonly var query = EF.CompileQuery((AppDbContext ctx, decimal minTotal) =>
    ctx.Orders.Where(o => o.Total > minTotal));
```

---

## Working with an Existing Database (Database-First)

### 1. Scaffold the Existing Database
Generate EF Core models from the existing database:
```bash
dotnet ef dbcontext scaffold "Your_Connection_String" Microsoft.EntityFrameworkCore.SqlServer -o Models -f
```
- `-o Models` → output folder for entities
- `-f` → force overwrite if the folder already exists

> Generates `DbContext` and entity classes for all tables in the DB.

### 2. Make Your Changes in the Database
Suppose your old DB has a new column you want to bring in:
```sql
ALTER TABLE Products ADD Description NVARCHAR(500) NULL;
```

### 3. Update Your EF Core Model

**Option A: Re-scaffold Entire DB (Overwrite)**
```bash
dotnet ef dbcontext scaffold "Your_Connection_String" Microsoft.EntityFrameworkCore.SqlServer -o Models -f
```
- Refreshes all models including the new column.
- Downside: overwrites any manual changes in entity classes.

**Option B: Scaffold Only the Affected Table**
```bash
dotnet ef dbcontext scaffold "Your_Connection_String" Microsoft.EntityFrameworkCore.SqlServer -o Models -t Products -f
```
- `-t Products` → only scaffold the `Products` table.
- Brings in only the changes for that table, keeping other classes intact.

### 4. Verify Changes
Check the entity class for the new column:
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }  // new column
}
```

### 5. Optional: Add Migration for Further Changes
```bash
dotnet ef migrations add AddProductDescription
dotnet ef database update
```
- In Database-First, migrations are optional; scaffolding reflects the DB schema.
- Be careful: DB and models should match exactly if using migrations.

**Summary**
- Use `dotnet ef dbcontext scaffold` for an existing DB.
- Bring in **one table or change** using `-t TableName`.
- Manual changes in other entities are safe if you scaffold only the affected table.
- Optionally, use migrations for future schema changes.

---

## EF Core vs ADO.NET: When to Use What

| Scenario / Requirement                     | EF Core Ideal?               | ADO.NET Ideal?                         | Notes / Comments                                                                 |
|-------------------------------------------|-----------------------------|---------------------------------------|---------------------------------------------------------------------------------|
| Rapid development / productivity          | Yes                         | No                                    | EF Core generates entities, supports LINQ, change tracking, and relationships.  |
| Medium-sized database                      | Yes                         | Possible, but manual work required    | EF Core handles CRUD and relationships efficiently.                              |
| Very large database (millions of rows)    | Careful                     | Yes                                   | EF Core tracks entities in memory; ADO.NET streaming (`SqlDataReader`) is better.|
| Complex business logic with relationships | Yes                         | Harder                                | EF Core supports navigation properties, FK constraints, and lazy loading.       |
| Performance-critical queries               | Benchmark first             | Yes                                   | EF Core has translation and tracking overhead; raw SQL in ADO.NET is faster.     |
| Migrations / schema evolution             | Yes                         | No                                    | EF Core migrations automate schema changes; ADO.NET requires manual scripts.    |
| Cross-platform .NET Core                   | Yes                         | Yes                                   | Both EF Core and ADO.NET support .NET Core/.NET 5+.                             |
| Async queries / scalable apps              | Yes                         | Yes (with async ADO.NET methods)      | EF Core async methods are easier; ADO.NET also supports `ExecuteReaderAsync()`. |
| Working with stored procedures / views     | Yes                         | Yes                                   | EF Core allows raw SQL and keyless entities; ADO.NET works natively.            |
| Legacy systems / existing SQL code        | Careful                     | Yes                                   | ADO.NET gives full control and integrates easily with legacy code.              |

### Equivalent of ADO.NET in .NET Core
- **Namespace:** `System.Data`
- **SQL Server provider:** `Microsoft.Data.SqlClient`
- **Key Classes:** `SqlConnection`, `SqlCommand`, `SqlDataReader`, `SqlDataAdapter`

---

## Change Tracking

EF Core's `DbContext` keeps snapshots of entity states (**Added / Modified / Deleted / Unchanged / Detached**).
- On `SaveChanges()`, EF generates SQL based on tracked state → automatic INSERT/UPDATE/DELETE.
- Enables **relationship fix-up** (navigation properties sync) and **lazy loading** (if enabled).
- Costs memory/CPU; for read-only queries use `AsNoTracking()` to speed up and reduce memory.
- For long-running contexts, prefer **short-lived DbContexts** (scoped per request/unit of work) to avoid stale tracking graphs.

**When to disable tracking:** read-heavy or reporting endpoints, list pages, caching layers. Use `AsNoTrackingWithIdentityResolution()` when you need deduped instances without full tracking.

### Tracking & Change Detection modes

| Mode | Call | When |
|---|---|---|
| No tracking | `AsNoTracking()` | Read-only lists |
| No tracking + fixups | `AsNoTrackingWithIdentityResolution()` | Read-only graphs with shared refs |
| Disable change detection | `context.ChangeTracker.AutoDetectChangesEnabled = false` | Bulk ops |

---

## Loading Strategies (Lazy / Eager / Explicit)

| Strategy | How | Pros | Cons | Use When |
|---|---|---|---|---|
| **Eager** | `.Include` / `.ThenInclude` | One or few roundtrips, simple | Can over-fetch; cartesian risk | Small graphs needed immediately |
| **Explicit** | `context.Entry(parent).Collection(p=>p.Children).Load()` | Controlled fetch | Extra code/roundtrips | Conditional/partial graphs |
| **Lazy** | Proxies + `virtual` navigation props | Minimal upfront code | N+1 trap, hidden IO | Rarely; only for simple/low-volume |

### Lazy Loading in Depth

**What it is:** Related entities are loaded from the database automatically when they are accessed for the first time. Instead of fetching related data eagerly with `.Include()`, EF Core creates proxies that defer loading until navigation properties are accessed.

**How to enable lazy loading**

1. Install package:
```bash
dotnet add package Microsoft.EntityFrameworkCore.Proxies
```

2. Configure in `DbContext`:
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseLazyLoadingProxies()
        .UseSqlServer("YourConnectionString");
}
```

3. Mark navigation properties as `virtual`:
```csharp
public class Blog
{
    public int BlogId { get; set; }
    public string Url { get; set; }

    // Navigation property - must be virtual
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public int BlogId { get; set; }
    public virtual Blog Blog { get; set; }
}
```

4. Usage example:
```csharp
using var context = new AppDbContext();

// Query only Blog (no Posts yet)
var blog = context.Blogs.First();

// Access navigation property → triggers SQL query for Posts
foreach (var post in blog.Posts)
{
    Console.WriteLine(post.Title);
}
```

Generated SQL:
```sql
-- First access: get Blog
SELECT TOP(1) [b].[BlogId], [b].[Url]
FROM [Blogs] AS [b];

-- Access Posts: triggers lazy load
SELECT [p].[PostId], [p].[Title], [p].[Content], [p].[BlogId]
FROM [Posts] AS [p]
WHERE [p].[BlogId] = @__blogId_0;
```

**Pros of Lazy Loading**

| Benefit | Explanation |
|---------|-------------|
| Simple access | Just use navigation property; EF loads data automatically |
| Cleaner code | No need to `.Include()` everywhere |
| Pay-as-you-go | Loads related data only when needed |
| Useful for small apps | Quick prototyping and small object graphs |

**Cons of Lazy Loading**

| Drawback | Explanation |
|----------|-------------|
| **N+1 Query Problem** | Accessing children in a loop causes multiple queries (bad for perf) |
| **Hidden DB calls** | SQL runs implicitly; can surprise developers |
| **Harder to optimize** | You lose control over query shape/performance |
| **Not supported in some scenarios** | Requires proxies and `virtual` properties |
| **Serialization issues** | Lazy-loaded proxies can cause circular references in JSON serialization |

**When to use (and avoid)**

Use Lazy Loading for small apps/prototypes, when related data is rarely needed, and when simplicity outweighs performance.

Avoid Lazy Loading for high-performance APIs, large graphs with many children (N+1 risk), and when you need explicit control (`Include`, projections, DTOs).

**Alternatives**
- **Eager Loading:** `.Include()` and `.ThenInclude()` to load related entities up-front.
- **Explicit Loading:** `context.Entry(blog).Collection(b => b.Posts).Load();` to load when you decide.

---

## How LINQ Becomes SQL — Performance Pitfalls

> Focus: what EF Core translates, where it can't, and how to avoid slow queries.

### Translation Basics (Mental Model)

| Stage | What EF Does | Why it matters |
|---|---|---|
| Build Expression | You write LINQ; EF builds an expression tree | Shapes what EF *can* translate |
| Translate | EF maps expression to SQL (provider-specific) | Unsupported nodes → client eval/materialization |
| Execute | SQL sent to DB; results materialized into CLR | Data volume + shape drive perf |
| Track | (Default) entities tracked for changes | Tracking adds CPU/memory; use `AsNoTracking` if read-only |

### Common LINQ → SQL Patterns (and Gotchas)

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
| Date ops (`Date`, `AddDays`) | Provider function (`CAST`, `DATEADD`) | Some members aren't translatable → client |
| `Distinct()` | `DISTINCT` | Remove *after* projecting required columns |

### The Big Pitfalls (Symptoms → Fix)

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

### Projection Patterns (fast paths)

Bad (wide & tracked):
```csharp
var orders = await ctx.Orders
    .Include(o => o.Customer)
    .Include(o => o.Items)
    .ToListAsync();
```

Better (narrow DTO, no tracking):
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

### Pagination (stable & efficient)
```csharp
var page = await ctx.Products.AsNoTracking()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Id)           // Must be deterministic
    .Skip((pageIndex - 1) * pageSize)
    .Take(pageSize)
    .Select(p => new { p.Id, p.Name, p.Price })
    .ToListAsync();
```

### Aggregates & Grouping (server-side)
```csharp
var totals = await ctx.OrderItems
    .Where(i => i.OrderDate >= from && i.OrderDate < to)
    .GroupBy(i => i.ProductId)
    .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Qty) })
    .ToListAsync();
```

### Searching & "IN" Lists
```csharp
// OK for small lists
var hits = await ctx.Users
    .Where(u => ids.Contains(u.Id))
    .ToListAsync();
```
For large lists → use **temp tables**, **table-valued params**, or **chunking**.

### Strings, Dates, Nulls

| Pattern | Prefer | Avoid | Why |
|---|---|---|---|
| Case-insensitive search | Collation/index, `EF.Functions.ILike` (PG) | `ToLower(Column)` | Keeps index usage |
| StartsWith/EndsWith | `StartsWith("abc")` | `Contains("^abc")` hacks | Translates to `LIKE 'abc%'` |
| Date filters | Range on full `DateTime` | `where x.Date == targetDate.Date` | Casting column kills indexes |
| Null checks | `== null` / `!= null` | Complex coalescing | Simple translates best |

### Split vs Single Query
```csharp
var data = await ctx.Parents
    .Include(p => p.Children)
    .Include(p => p.Tags)
    .AsSplitQuery() // avoid cartesian explosion
    .ToListAsync();
```

### Compiled Queries
```csharp
static readonly Func<AppDbContext,int,Task<Product?>> GetProduct =
    EF.CompileAsyncQuery((AppDbContext db, int id) =>
        db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id));
```

### Raw SQL
```csharp
var items = await ctx.Items
    .FromSqlInterpolated($"SELECT TOP {take} * FROM Items WHERE CreatedAt >= {from} AND CreatedAt < {to}")
    .AsNoTracking()
    .ToListAsync();
```

### Set-based Updates/Deletes
```csharp
await ctx.Orders
    .Where(o => o.Status == Status.Stale)
    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, Status.Archived));

await ctx.Logs
    .Where(l => l.CreatedAt < cutoff)
    .ExecuteDeleteAsync();
```

### Diagnostics

| Tool | What | Why |
|---|---|---|
| `ToQueryString()` | See SQL from LINQ | Debug translation |
| Logging (`EnableSensitiveDataLogging`) | Show SQL + params | Perf debugging |
| `TagWith("hint")` | Annotate SQL | Easier tracing |
| EF Analyzers | Compile-time hints | Catch bad LINQ |
| DB plans/statistics | Query plan | Confirm indexes |

### Smell Checklist
- [ ] Only needed columns selected?
- [ ] `OrderBy` before `Skip/Take`?
- [ ] Includes minimized?
- [ ] Any client eval warnings?
- [ ] Loops issuing queries?
- [ ] NoTracking used for reads?
- [ ] SplitQuery for multi-collections?
- [ ] Set-based updates/deletes?

### Bad → Good Examples

N+1 (bad):
```csharp
var customers = await ctx.Customers.ToListAsync();
foreach (var c in customers)
    Console.WriteLine(c.Orders.Count); // 1 query per customer
```

Eager load (good):
```csharp
var counts = await ctx.Customers
    .Select(c => new { c.Id, Orders = c.Orders.Count })
    .ToListAsync();
```

Fat projection (bad):
```csharp
var products = await ctx.Products.ToListAsync();
```

Slim DTO (good):
```csharp
var products = await ctx.Products
   .Select(p => new { p.Id, p.Name, p.Price })
   .AsNoTracking()
   .ToListAsync();
```

### Bottom Line
- LINQ **translates to SQL**, but only what EF can express.
- Beware **N+1, Includes, GroupBy, and client evaluation**.
- Always check `ToQueryString()` and DB query plans.
- Think **set-based, narrow, ordered, no surprises**.

---

## Concurrency — Handling Conflicts

> Goal: prevent **lost updates** when multiple users edit the same row at the same time, and resolve conflicts predictably.

### Concurrency Models (Mental Model)

| Model | How it works | Notes |
|---|---|---|
| **Optimistic** (default in EF Core) | No locks during reads; compare a **concurrency token** at update. If it changed, throw `DbUpdateConcurrencyException`. | Most common; great for web apps. |
| **Pessimistic** | Lock rows while reading/updating so others wait. | Requires database locks/`FOR UPDATE`; EF uses raw SQL or provider-specific APIs. Use sparingly. |

**EF Core focuses on optimistic concurrency.**

### Marking Concurrency Tokens

Use a column that changes whenever the row changes.

**a) RowVersion / Timestamp (recommended)**
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }

    // Concurrency token
    [Timestamp]                    // DataAnnotation
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

// Or via Fluent API
protected override void OnModelCreating(ModelBuilder mb)
{
    mb.Entity<Product>()
      .Property(p => p.RowVersion)
      .IsRowVersion();            // sets IsConcurrencyToken + value generated on update
}
```
- SQL Server type is `rowversion`/`timestamp` (auto-updates on each write).
- Other providers: use a `byte[]` or computed column that changes on update.

**b) Property-based token**
```csharp
public class Blog
{
    public int Id { get; set; }

    [ConcurrencyCheck]            // Any change to this column is checked
    public string Slug { get; set; } = "";
}
```
```csharp
mb.Entity<Blog>().Property(b => b.Slug).IsConcurrencyToken();
```

### What EF Sends to the DB (Update WHERE clause)

With a concurrency token, EF generates SQL like:
```sql
UPDATE [Products]
SET [Name] = @p0, [Price] = @p1
WHERE [Id] = @id AND [RowVersion] = @original_rowversion;
-- rowsAffected == 0 -> DbUpdateConcurrencyException
```
If another user updated the row (changing `RowVersion`), the `WHERE` fails and EF throws `DbUpdateConcurrencyException` on `SaveChanges()`.

### Handling `DbUpdateConcurrencyException` — Three Strategies

| Strategy | Nickname | What you do | Pros | Cons |
|---|---|---|---|---|
| **Database wins** | Discard mine | Reload entity from DB and ignore my changes. | Simple, safe | User loses edits |
| **Client wins** | Overwrite theirs | Write my values again using the **current** database token. | Keeps my edits | Might clobber others' changes |
| **Merge** | Manual merge | Compare current DB values vs my values; decide per field; save. | Preserves intent | More code/UI work |

**a) Database Wins (reload)**
```csharp
try
{
    await context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    foreach (var entry in ex.Entries)
    {
        // Throw away local changes and reload from DB
        await entry.ReloadAsync();
    }
}
```

**b) Client Wins (force overwrite)**
```csharp
try
{
    await context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    foreach (var entry in ex.Entries)
    {
        var databaseValues = await entry.GetDatabaseValuesAsync();
        if (databaseValues is null) throw;   // deleted by another user

        // Set original values to database values so the next Save uses the new token
        entry.OriginalValues.SetValues(databaseValues);
    }
    await context.SaveChangesAsync();        // retried with fresh token (overwrites)
}
```

**c) Merge (field-level resolution)**
```csharp
catch (DbUpdateConcurrencyException ex)
{
    foreach (var entry in ex.Entries)
    {
        var dbValues = await entry.GetDatabaseValuesAsync();
        if (dbValues is null) throw; // deleted; handle separately

        var proposedValues = entry.CurrentValues;   // my edits
        var originalValues = entry.OriginalValues;  // what I read

        // Example policy: if I changed it and DB changed it too → prefer DB; else keep mine.
        foreach (var prop in entry.Metadata.GetProperties())
        {
            var original = originalValues[prop];
            var current  = proposedValues[prop];
            var database = dbValues[prop];

            bool iChanged    = !Equals(original, current);
            bool dbChanged   = !Equals(original, database);

            if (iChanged && dbChanged)
            {
                // conflict: choose database value (or prompt user)
                proposedValues[prop] = database;
            }
            // else keep my value (no conflict or only one side changed)
        }

        // Update the original values so we can save
        entry.OriginalValues.SetValues(dbValues);
    }
    await context.SaveChangesAsync();
}
```

> In UI scenarios, surface both versions to the user for a human decision.

### Web API Pattern with ETags (RowVersion → ETag)

Expose `RowVersion` as a base64 ETag and require `If-Match` on updates.
```csharp
// GET
public async Task<ActionResult<ProductDto>> Get(int id)
{
    var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    if (product is null) return NotFound();

    var etag = Convert.ToBase64String(product.RowVersion);
    Response.Headers.ETag = $"W/\"{etag}\"";
    return new ProductDto(product);
}

// PUT (If-Match required)
[HttpPut("{id}")]
public async Task<IActionResult> Put(int id, ProductUpdateDto dto, [FromHeader(Name = "If-Match")] string ifMatch)
{
    var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
    if (product is null) return NotFound();

    var currentEtag = $"W/\"{Convert.ToBase64String(product.RowVersion)}\"";
    if (ifMatch != currentEtag) return StatusCode(StatusCodes.Status412PreconditionFailed);

    // apply changes
    product.Name = dto.Name;
    product.Price = dto.Price;

    await _db.SaveChangesAsync(); // will bump RowVersion
    return NoContent();
}
```
Benefits: **stateless concurrency** at the HTTP layer; plays nicely with caches.

### Razor Pages / MVC UI Example
- Bind `RowVersion` as a hidden field so the token round-trips.
- On conflict, display both sets of values.
```csharp
[BindProperty] public Product Product { get; set; } = default!;

public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid) return Page();
    _db.Attach(Product).State = EntityState.Modified;

    try
    {
        await _db.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
    catch (DbUpdateConcurrencyException ex)
    {
        var entry = ex.Entries.Single();
        var databaseValues = await entry.GetDatabaseValuesAsync();
        if (databaseValues is null)
        {
            ModelState.AddModelError("", "The product was deleted by another user.");
            return Page();
        }

        var dbProduct = (Product)databaseValues.ToObject();
        ModelState.AddModelError("", "The record was modified by another user. Review the database values and re-save if appropriate.");

        // Example: show database values to user
        // Copy dbProduct fields into a separate view model or ModelState

        // Reset original values to DB's so next post will succeed
        entry.OriginalValues.SetValues(databaseValues);
        return Page();
    }
}
```

### Deletes & Concurrency
Include the token in the `WHERE` clause for deletes as well:
```sql
DELETE FROM [Products] WHERE [Id] = @id AND [RowVersion] = @original_rowversion;
-- rowsAffected == 0 => someone else changed/deleted it
```
EF does this automatically when tokens are configured.

### Bulk / Set-Based Operations
- If you use raw SQL or set-based updates, **include the token** in the predicate to preserve concurrency semantics.
- EF Core's `ExecuteUpdateAsync`/`ExecuteDeleteAsync` (per-provider) will respect predicates that include the token:
```csharp
await context.Products
    .Where(p => p.Id == id && p.RowVersion == originalToken)
    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Price, p => p.Price * 1.05m));
```

### Testing Concurrency
- Create two contexts; load the same entity in both; modify & save in A; then save in B and expect `DbUpdateConcurrencyException`.
- Verify your UI/API surfaces conflicts clearly and lets users retry or merge.

### Quick Checklist
- [ ] Use **RowVersion**/`[Timestamp]` wherever possible.
- [ ] Ensure tokens **round-trip** (hidden fields / ETags).
- [ ] Catch **`DbUpdateConcurrencyException`**; decide **database wins**, **client wins**, or **merge**.
- [ ] For APIs, use **ETags + If-Match**.
- [ ] Include tokens in **bulk updates/deletes**.
- [ ] Log conflicts to monitor contention hotspots.

### Bottom Line
Optimistic concurrency in EF Core is simple and powerful **if you surface the token and handle conflicts intentionally**. Choose a strategy (database wins, client wins, or merge), make it consistent in your UI/API, and test it.
