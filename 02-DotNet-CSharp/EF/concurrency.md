# Handling Concurrency Conflicts in Entity Framework Core — Cheatsheet

> Goal: prevent **lost updates** when multiple users edit the same row at the same time, and resolve conflicts predictably.

---

## 1) Concurrency Models (Mental Model)

| Model | How it works | Notes |
|---|---|---|
| **Optimistic** (default in EF Core) | No locks during reads; compare a **concurrency token** at update. If it changed, throw `DbUpdateConcurrencyException`. | Most common; great for web apps. |
| **Pessimistic** | Lock rows while reading/updating so others wait. | Requires database locks/`FOR UPDATE`; EF uses raw SQL or provider-specific APIs. Use sparingly. |

**EF Core focuses on optimistic concurrency.**

---

## 2) Marking Concurrency Tokens

Use a column that changes whenever the row changes.

### a) RowVersion / Timestamp (recommended)

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
- Other providers: use a byte[] or computed column that changes on update.

### b) Property-based token

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

---

## 3) What EF Sends to the DB (Update WHERE clause)

With a concurrency token, EF generates SQL like:

```sql
UPDATE [Products]
SET [Name] = @p0, [Price] = @p1
WHERE [Id] = @id AND [RowVersion] = @original_rowversion;
-- rowsAffected == 0 -> DbUpdateConcurrencyException
```

If another user updated the row (changing `RowVersion`), the `WHERE` fails and EF throws `DbUpdateConcurrencyException` on `SaveChanges()`.

---

## 4) Handling `DbUpdateConcurrencyException` — Three Strategies

| Strategy | Nickname | What you do | Pros | Cons |
|---|---|---|---|---|
| **Database wins** | Discard mine | Reload entity from DB and ignore my changes. | Simple, safe | User loses edits |
| **Client wins** | Overwrite theirs | Write my values again using the **current** database token. | Keeps my edits | Might clobber others' changes |
| **Merge** | Manual merge | Compare current DB values vs my values; decide per field; save. | Preserves intent | More code/UI work |

### a) Database Wins (reload)

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

### b) Client Wins (force overwrite)

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

### c) Merge (field-level resolution)

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

---

## 5) Web API Pattern with ETags (RowVersion → ETag)

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

---

## 6) Razor Pages / MVC UI Example

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

---

## 7) Deletes & Concurrency

- Include the token in the `WHERE` clause for deletes as well:

```sql
DELETE FROM [Products] WHERE [Id] = @id AND [RowVersion] = @original_rowversion;
-- rowsAffected == 0 => someone else changed/deleted it
```

EF does this automatically when tokens are configured.

---

## 8) Bulk/Set-Based Operations

- If you use raw SQL or set-based updates, **include the token** in the predicate to preserve concurrency semantics.
- EF Core's `ExecuteUpdateAsync/ExecuteDeleteAsync` (per-provider) will respect predicates that include the token. Example:

```csharp
await context.Products
    .Where(p => p.Id == id && p.RowVersion == originalToken)
    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Price, p => p.Price * 1.05m));
```

---

## 9) Testing Concurrency

- Create two contexts; load the same entity in both; modify & save in A; then save in B and expect `DbUpdateConcurrencyException`.
- Verify your UI/API surfaces conflicts clearly and lets users retry or merge.

---

## 10) Quick Checklist

- [ ] Use **RowVersion**/`[Timestamp]` wherever possible.  
- [ ] Ensure tokens **round-trip** (hidden fields / ETags).  
- [ ] Catch **`DbUpdateConcurrencyException`**; decide **database wins**, **client wins**, or **merge**.  
- [ ] For APIs, use **ETags + If-Match**.  
- [ ] Include tokens in **bulk updates/deletes**.  
- [ ] Log conflicts to monitor contention hotspots.  

---

### Bottom Line
Optimistic concurrency in EF Core is simple and powerful **if you surface the token and handle conflicts intentionally**. Choose a strategy (database wins, client wins, or merge), make it consistent in your UI/API, and test it.
