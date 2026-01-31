# Lazy Loading in EF Core

## 🔹 What is Lazy Loading?
- **Lazy Loading** is a pattern where related entities are **loaded from the database automatically when they are accessed for the first time**.
- Instead of fetching related data eagerly with `.Include()`, EF Core creates proxies that defer loading until navigation properties are accessed.

---

## 🔹 How to Enable Lazy Loading in EF Core

### 1. Install Package
```bash
dotnet add package Microsoft.EntityFrameworkCore.Proxies
```

### 2. Configure in `DbContext`
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseLazyLoadingProxies()
        .UseSqlServer("YourConnectionString");
}
```

### 3. Mark Navigation Properties as `virtual`
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

### 4. Usage Example
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

---

## 🔹 Pros of Lazy Loading
| Benefit | Explanation |
|---------|-------------|
| Simple access | Just use navigation property; EF loads data automatically |
| Cleaner code | No need to `.Include()` everywhere |
| Pay-as-you-go | Loads related data only when needed |
| Useful for small apps | Quick prototyping and small object graphs |

---

## 🔹 Cons of Lazy Loading
| Drawback | Explanation |
|----------|-------------|
| **N+1 Query Problem** | Accessing children in a loop causes multiple queries (bad for perf) |
| **Hidden DB calls** | SQL runs implicitly; can surprise developers |
| **Harder to optimize** | You lose control over query shape/performance |
| **Not supported in some scenarios** | Requires proxies and `virtual` properties |
| **Serialization issues** | Lazy-loaded proxies can cause circular references in JSON serialization |

---

## 🔹 When to Use (and Avoid)

✅ Use Lazy Loading:
- Small apps or prototypes
- When related data is rarely needed
- When simplicity > performance

❌ Avoid Lazy Loading:
- High-performance APIs
- Large graphs with many children (N+1 risk)
- When you need explicit control (`Include`, projections, DTOs)

---

## 🔹 Alternatives
- **Eager Loading**: `.Include()` and `.ThenInclude()` to load related entities up-front.
- **Explicit Loading**: `context.Entry(blog).Collection(b => b.Posts).Load();` to load when you decide.
