# Redis Caching in .NET — Implementation & Expiration Strategies

A practical, copy‑pasteable guide for using **Redis** with .NET: how to wire it up, apply **absolute/sliding** expiration correctly, avoid stampedes, and get the most effectiveness out of your cache. Includes both **`IDistributedCache`** and **StackExchange.Redis** patterns, plus notes on **Output Caching** with a Redis store.

---

## 1) When to use Redis
- **Multi‑instance / load‑balanced** apps need a **shared cache** → Redis is ideal.
- Use for **expensive, read‑heavy** data (computed results, reference data, rendered fragments).
- Avoid for **secrets** or **strong consistency** needs (cache is an optimization; always have a source of truth).

---

## 2) Wiring Redis with `IDistributedCache`

`IDistributedCache` provides a framework‑level abstraction. Use the Redis implementation for shared cache.

```csharp
// Program.cs (.NET 7/8)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis"); // e.g. "localhost:6379"
    options.InstanceName = "MyApp:"; // optional key prefix/namespace
});
```

### Cache‑aside usage with JSON
```csharp
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

public sealed class ProductService
{
    private readonly IDistributedCache _cache;
    private readonly IProductRepository _repo;

    public ProductService(IDistributedCache cache, IProductRepository repo)
        => (_cache, _repo) = (cache, repo);

    public async Task<Product?> GetProductAsync(int id, CancellationToken ct)
    {
        var key = $"product:{id}";
        var cached = await _cache.GetStringAsync(key, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<Product>(cached);

        var product = await _repo.LoadAsync(id, ct);
        if (product is null) return null;

        var json = JsonSerializer.Serialize(product);

        // Absolute expiration preferred for cross‑instance consistency
        var opts = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            // Or use SlidingExpiration (see section 4)
            // SlidingExpiration = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(key, json, opts, ct);
        return product;
    }

    public Task InvalidateProductAsync(int id, CancellationToken ct)
        => _cache.RemoveAsync($"product:{id}", ct);
}
```

> **Note:** `IDistributedCache.Get*` **refreshes sliding expiration** automatically when SlidingExpiration is used. You can also call `RefreshAsync` explicitly.

---

## 3) Using StackExchange.Redis directly (advanced scenarios)

For fine control (data structures, Lua, pub/sub), use the lower‑level client.

```csharp
using StackExchange.Redis;
using System.Text.Json;

public static class RedisModule
{
    public static IServiceCollection AddRedis(this IServiceCollection services, string conn)
    {
        var muxer = ConnectionMultiplexer.Connect(conn);
        services.AddSingleton<IConnectionMultiplexer>(muxer);
        services.AddSingleton(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
        return services;
    }
}
```

```csharp
public sealed class CatalogCache
{
    private readonly IDatabase _db;
    public CatalogCache(IDatabase db) => _db = db;

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        var payload = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, payload, ttl);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var val = await _db.StringGetAsync(key);
        return val.HasValue ? JsonSerializer.Deserialize<T>(val!) : default;
    }

    public Task RemoveAsync(string key) => _db.KeyDeleteAsync(key);
}
```

**Sliding expiration with Redis (manual):** Redis **does not** natively slide TTL on read; you must **reset TTL** when you read:
```csharp
public async Task<T?> GetSlidingAsync<T>(string key, TimeSpan slideBy)
{
    var tran = _db.CreateTransaction();
    var getTask = tran.StringGetAsync(key);
    // Only extend TTL if key exists after GET
    _ = tran.KeyExpireAsync(key, slideBy, CommandFlags.FireAndForget);
    var committed = await tran.ExecuteAsync();
    var val = await getTask;
    return val.HasValue ? JsonSerializer.Deserialize<T>(val!) : default;
}
```

---

## 4) Expiration Models — What’s the difference?

| Expiration | Where configured | Behavior | Notes |
|---|---|---|---|
| **Absolute** | `DistributedCacheEntryOptions.AbsoluteExpiration` or `...RelativeToNow`; Redis `EX` | Key expires at a fixed time/TTL regardless of access | **Most predictable** across instances; recommended default |
| **Sliding** | `DistributedCacheEntryOptions.SlidingExpiration`; custom with Redis | TTL **resets on access** | Good for hot items; risk of keys living forever (add a **cap**: sliding + absolute) |
| **No expiration** | Avoid | Keys stay until evicted/removed | Risk of unbounded growth |
| **Size/Memory eviction** | Redis server policy | Redis evicts keys under pressure (LRU, LFU) | Set `maxmemory` and **policy** carefully; treat as last‑resort eviction |

**Best practice:** If using Sliding, **combine** with an **Absolute cap** (a.k.a. “sliding with absolute max”) so items don’t survive indefinitely.

```csharp
var opts = new DistributedCacheEntryOptions
{
    SlidingExpiration = TimeSpan.FromMinutes(10),
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) // hard cap
};
```

---

## 5) Avoiding Cache Stampedes (thundering herds)

**Problem:** Many concurrent misses trigger simultaneous expensive recomputation.

**Patterns:**

1) **Single‑flight (mutex) in‑process**  
```csharp
private static readonly SemaphoreSlim _mutex = new(1,1);

public async Task<Product?> GetProductSafeAsync(int id, CancellationToken ct)
{
    var key = $"product:{id}";
    var cached = await _cache.GetStringAsync(key, ct);
    if (cached is not null) return JsonSerializer.Deserialize<Product>(cached);

    await _mutex.WaitAsync(ct);
    try
    {
        // Double‑check after entering the lock
        cached = await _cache.GetStringAsync(key, ct);
        if (cached is not null) return JsonSerializer.Deserialize<Product>(cached);

        var product = await _repo.LoadAsync(id, ct);
        if (product is null) return null;
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(product),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
        return product;
    }
    finally { _mutex.Release(); }
}
```

2) **Jittered TTLs** to avoid synchronized expiry
```csharp
var baseTtl = TimeSpan.FromMinutes(10);
var jitter = TimeSpan.FromSeconds(Random.Shared.Next(0, 60));
await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = baseTtl + jitter
});
```

3) **Refresh‑ahead (background)**  
Refresh popular keys **before** they expire (via HostedService) to keep hit ratio high.

4) **Distributed lock** (advanced)  
Use a library (e.g., Redlock) if you must coordinate **across instances**. Prefer single‑flight + short TTL + jitter first.

---

## 6) Key Design & Namespacing

- Use stable, clear keys: `entity:{type}:{id}`, e.g., `product:42`.
- For query results: normalized, sorted keys: `search:category=shoes&color=red&page=1`.
- For bulk invalidation, **prefix** or **tag** keys and publish invalidations:
  - Keep a list/set of keys by tag (e.g., `tag:product:42` → Set of keys to purge).

---

## 7) Measuring Effectiveness

- Track **hit ratio** = hits / (hits + misses). Aim for > 0.8 on hot paths.
- Monitor **tail latency**; stampedes show up as spikes near expiry.
- Watch Redis metrics: CPU, memory used, eviction count, keyspace hits/misses.
- Add **tracing** around cache get/set/refresh to see impact.

---

## 8) Output Caching backed by Redis (server‑side HTML/API fragment caching)

`OutputCache` is pluggable via `IOutputCacheStore`. You can back it with Redis to **share** output cache across instances.

```csharp
builder.Services.AddOutputCache();
builder.Services.AddSingleton<IOutputCacheStore>(sp =>
    new RedisOutputCacheStore(/* inject IConnectionMultiplexer, db index, key prefix, serializer */));

var app = builder.Build();
app.UseOutputCache();

app.MapGet("/products", () => GetProducts())
   .CacheOutput(p => p.Expire(TimeSpan.FromMinutes(5))
                      .SetVaryByQuery("category", "page")
                      .Tag("products-list"));
```

> Implement `IOutputCacheStore` with `GetAsync`, `SetAsync`, `EvictByTagAsync` using Redis keys and TTLs. (Or use a community package if available.)

---

## 9) Quick Do/Don’t

**Do**
- ✅ Prefer **absolute TTL** (with optional sliding cap).  
- ✅ Add **jitter** to TTLs.  
- ✅ Use **cache‑aside** with clear invalidation on writes.  
- ✅ Compress large payloads if bandwidth matters (mind CPU cost).  
- ✅ Limit value sizes; serialize with `System.Text.Json` for speed.

**Don’t**
- ❌ Cache unbounded collections without paging.  
- ❌ Use sliding‑only TTLs in shared caches without a cap.  
- ❌ Treat cache as the source of truth.  
- ❌ Forget to monitor hit ratio and memory/evictions.

---

## 10) End‑to‑End Example (Put it together)

```csharp
public sealed class CachedCatalogService
{
    private readonly IDistributedCache _cache;
    private readonly CatalogRepository _repo;

    public CachedCatalogService(IDistributedCache cache, CatalogRepository repo)
        => (_cache, _repo) = (cache, repo);

    public async Task<CatalogItem?> GetAsync(int id, CancellationToken ct)
    {
        var key = $"catalog:item:{id}";
        var hit = await _cache.GetStringAsync(key, ct);
        if (hit is not null) return JsonSerializer.Deserialize<CatalogItem>(hit);

        // cache miss
        var item = await _repo.GetAsync(id, ct);
        if (item is null) return null;

        var json = JsonSerializer.Serialize(item);
        var ttl = TimeSpan.FromMinutes(15) + TimeSpan.FromSeconds(Random.Shared.Next(0, 60));
        await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        }, ct);

        return item;
    }

    public Task InvalidateAsync(int id, CancellationToken ct)
        => _cache.RemoveAsync($"catalog:item:{id}", ct);
}
```

---

### Checklist
- [ ] Add Redis via `AddStackExchangeRedisCache` (or `StackExchange.Redis` directly).
- [ ] Choose **absolute TTL**; consider sliding + cap for hot items.
- [ ] Add **jitter**; prevent stampedes with single‑flight/refresh‑ahead.
- [ ] Define **key scheme** & invalidation on writes.
- [ ] Monitor hit ratio, latency, Redis memory/evictions.

---
