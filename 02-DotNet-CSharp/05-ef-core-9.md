# Entity Framework Core 9 Features - Complete Guide

## Table of Contents
1. [Introduction](#introduction)
2. [LINQ Improvements](#linq-improvements)
3. [JSON Column Enhancements](#json-column-enhancements)
4. [Complex Types Support](#complex-types-support)
5. [Cosmos DB Provider Updates](#cosmos-db-provider-updates)
6. [Performance Enhancements](#performance-enhancements)
7. [Migration Improvements](#migration-improvements)
8. [Best Practices](#best-practices)
9. [Interview Questions](#interview-questions)

## Introduction

Entity Framework Core 9, released with .NET 9 in November 2024, brings substantial improvements in query translation, JSON support, performance optimizations, and complex type handling. This guide covers all major features with practical examples and production patterns.

## LINQ Improvements

### Enhanced Query Translation

```csharp
// EF Core 9: Improved complex query translation
public class ProductRepository
{
    private readonly ApplicationDbContext _context;
    
    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // .NET 9: GroupBy improvements
    public async Task<List<CategoryStats>> GetCategoryStatsAsync()
    {
        // EF Core 9: Better GroupBy translation
        return await _context.Products
            .GroupBy(p => p.Category)
            .Select(g => new CategoryStats
            {
                Category = g.Key,
                TotalProducts = g.Count(),
                AveragePrice = g.Average(p => p.Price),
                TotalRevenue = g.Sum(p => p.Price * p.Stock),
                MaxPrice = g.Max(p => p.Price),
                MinPrice = g.Min(p => p.Price)
            })
            .ToListAsync();
    }
    
    // EF Core 9: Complex subqueries
    public async Task<List<ProductWithOrderCount>> GetProductsWithOrderCountAsync()
    {
        return await _context.Products
            .Select(p => new ProductWithOrderCount
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                OrderCount = _context.OrderItems
                    .Where(oi => oi.ProductId == p.Id)
                    .Count()
            })
            .Where(p => p.OrderCount > 0)
            .OrderByDescending(p => p.OrderCount)
            .ToListAsync();
    }
    
    // EF Core 9: String operations
    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        return await _context.Products
            .Where(p => 
                EF.Functions.Like(p.Name, $"%{searchTerm}%") ||
                EF.Functions.Like(p.Description, $"%{searchTerm}%"))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
```

### Prune Collections

```csharp
// EF Core 9: Prune empty collections from query results
public class OrderService
{
    private readonly ApplicationDbContext _context;
    
    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Customer>> GetCustomersWithOrdersAsync()
    {
        // EF Core 9: Automatically prunes empty collections
        var customers = await _context.Customers
            .Include(c => c.Orders.Where(o => o.Status == OrderStatus.Completed))
            .Include(c => c.Addresses)
            .AsSplitQuery()
            .ToListAsync();
        
        // In EF Core 9, customers with no completed orders 
        // will have Orders collection as null instead of empty list
        // Better memory efficiency
        
        return customers;
    }
    
    // Control pruning behavior
    public async Task<List<Customer>> GetCustomersNoPruningAsync()
    {
        return await _context.Customers
            .Include(c => c.Orders)
            .AsSplitQuery()
            .Select(c => new Customer
            {
                Id = c.Id,
                Name = c.Name,
                Orders = c.Orders ?? new List<Order>() // Explicit empty list
            })
            .ToListAsync();
    }
}
```

### Improvements to ExecuteUpdate and ExecuteDelete

```csharp
// EF Core 9: Enhanced bulk operations
public class BulkOperations
{
    private readonly ApplicationDbContext _context;
    
    public BulkOperations(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // EF Core 9: More complex ExecuteUpdate scenarios
    public async Task<int> IncreaseProductPricesAsync(string category, decimal percentage)
    {
        return await _context.Products
            .Where(p => p.Category == category)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Price, p => p.Price * (1 + percentage / 100))
                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
    }
    
    // Update with joins
    public async Task<int> UpdateProductStockFromInventoryAsync()
    {
        return await _context.Products
            .Join(_context.InventoryItems,
                p => p.Id,
                i => i.ProductId,
                (p, i) => new { Product = p, Inventory = i })
            .Where(x => x.Product.Stock != x.Inventory.AvailableQuantity)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Product.Stock, x => x.Inventory.AvailableQuantity));
    }
    
    // Conditional delete
    public async Task<int> DeleteExpiredPromotionsAsync()
    {
        return await _context.Promotions
            .Where(p => p.EndDate < DateTime.UtcNow)
            .Where(p => !p.Orders.Any()) // Don't delete if orders exist
            .ExecuteDeleteAsync();
    }
    
    // Complex delete with subquery
    public async Task<int> DeleteInactiveUsersAsync(int inactiveDays)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-inactiveDays);
        
        return await _context.Users
            .Where(u => u.LastLoginDate < cutoffDate)
            .Where(u => !_context.Orders.Any(o => o.UserId == u.Id))
            .Where(u => !_context.Reviews.Any(r => r.UserId == u.Id))
            .ExecuteDeleteAsync();
    }
}
```

### Query Improvements

```csharp
// EF Core 9: Advanced LINQ patterns
public class AdvancedQueries
{
    private readonly ApplicationDbContext _context;
    
    public AdvancedQueries(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Window functions support (database-specific)
    public async Task<List<ProductRanking>> GetProductRankingsByCategoryAsync()
    {
        return await _context.Products
            .Select(p => new ProductRanking
            {
                ProductId = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price,
                Rank = EF.Functions.RowNumber(
                    EF.Functions.OrderBy(p.Price, descending: true),
                    EF.Functions.PartitionBy(p.Category))
            })
            .ToListAsync();
    }
    
    // Common Table Expressions (CTE)
    public async Task<List<CategoryHierarchy>> GetCategoryHierarchyAsync()
    {
        // EF Core 9: Better CTE support
        var query = _context.Categories
            .FromSqlRaw(@"
                WITH RECURSIVE CategoryTree AS (
                    SELECT Id, Name, ParentId, 0 AS Level
                    FROM Categories
                    WHERE ParentId IS NULL
                    
                    UNION ALL
                    
                    SELECT c.Id, c.Name, c.ParentId, ct.Level + 1
                    FROM Categories c
                    INNER JOIN CategoryTree ct ON c.ParentId = ct.Id
                )
                SELECT * FROM CategoryTree
                ORDER BY Level, Name
            ");
        
        return await query.ToListAsync();
    }
    
    // Aggregations with DISTINCT
    public async Task<decimal> GetAverageUniqueProductPricesAsync()
    {
        // EF Core 9: DISTINCT in aggregations
        return await _context.Products
            .Select(p => p.Price)
            .Distinct()
            .AverageAsync();
    }
}
```

## JSON Column Enhancements

### Enhanced JSON Mapping

```csharp
// EF Core 9: Improved JSON column support
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    // JSON column for flexible attributes
    public ProductAttributes Attributes { get; set; } = new();
    
    // JSON collection
    public List<Review> Reviews { get; set; } = new();
}

public class ProductAttributes
{
    public string Color { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public int Weight { get; set; }
    public Dictionary<string, string> CustomFields { get; set; } = new();
}

public class Review
{
    public string Author { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// DbContext configuration
public class ApplicationDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // EF Core 9: Configure JSON columns
        modelBuilder.Entity<Product>(builder =>
        {
            builder.OwnsOne(p => p.Attributes, ownedBuilder =>
            {
                ownedBuilder.ToJson();
                
                // EF Core 9: Query JSON properties
                ownedBuilder.Property(a => a.Color).IsRequired();
                ownedBuilder.Property(a => a.Size).HasMaxLength(10);
            });
            
            builder.OwnsMany(p => p.Reviews, ownedBuilder =>
            {
                ownedBuilder.ToJson();
                ownedBuilder.Property(r => r.Rating).HasMaxLength(5);
            });
        });
    }
}
```

### Querying JSON Columns

```csharp
// EF Core 9: Query JSON data efficiently
public class ProductJsonQueries
{
    private readonly ApplicationDbContext _context;
    
    public ProductJsonQueries(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Query JSON properties
    public async Task<List<Product>> FindByColorAsync(string color)
    {
        return await _context.Products
            .Where(p => p.Attributes.Color == color)
            .ToListAsync();
    }
    
    // Query nested JSON properties
    public async Task<List<Product>> FindByCustomFieldAsync(string key, string value)
    {
        return await _context.Products
            .Where(p => p.Attributes.CustomFields[key] == value)
            .ToListAsync();
    }
    
    // Query JSON collections
    public async Task<List<Product>> FindByMinRatingAsync(int minRating)
    {
        return await _context.Products
            .Where(p => p.Reviews.Any(r => r.Rating >= minRating))
            .ToListAsync();
    }
    
    // Update JSON properties
    public async Task UpdateProductColorAsync(int productId, string newColor)
    {
        await _context.Products
            .Where(p => p.Id == productId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Attributes.Color, newColor));
    }
    
    // Complex JSON queries
    public async Task<List<ProductStats>> GetProductStatsWithReviewsAsync()
    {
        return await _context.Products
            .Select(p => new ProductStats
            {
                ProductId = p.Id,
                Name = p.Name,
                ReviewCount = p.Reviews.Count,
                AverageRating = p.Reviews.Average(r => r.Rating),
                LatestReviewDate = p.Reviews.Max(r => r.CreatedAt)
            })
            .Where(s => s.ReviewCount > 0)
            .ToListAsync();
    }
}
```

### JSON Updates and Modifications

```csharp
// EF Core 9: Modify JSON data
public class JsonModifications
{
    private readonly ApplicationDbContext _context;
    
    public JsonModifications(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Add review to JSON collection
    public async Task AddReviewAsync(int productId, Review review)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);
        
        if (product != null)
        {
            product.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }
    }
    
    // Update specific JSON property
    public async Task UpdateAttributeAsync(int productId, string color, string size)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);
        
        if (product != null)
        {
            product.Attributes.Color = color;
            product.Attributes.Size = size;
            await _context.SaveChangesAsync();
        }
    }
    
    // Bulk update JSON property
    public async Task BulkUpdateWeightAsync(string category, int newWeight)
    {
        await _context.Products
            .Where(p => p.Category == category)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Attributes.Weight, newWeight));
    }
    
    // Delete from JSON collection
    public async Task DeleteLowRatingReviewsAsync(int productId, int minRating)
    {
        var product = await _context.Products
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == productId);
        
        if (product != null)
        {
            product.Reviews.RemoveAll(r => r.Rating < minRating);
            await _context.SaveChangesAsync();
        }
    }
}
```

## Complex Types Support

### Value Objects as Complex Types

```csharp
// EF Core 9: Complex types (value objects)
public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    
    // Complex type - not a separate table
    public Address ShippingAddress { get; set; } = new();
    public Address BillingAddress { get; set; } = new();
    
    // Another complex type
    public Money TotalAmount { get; set; } = new();
    
    public List<OrderItem> Items { get; set; } = new();
}

// Complex type - value object
public record Address
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

// Another complex type
public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
}

// DbContext configuration
public class ApplicationDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // EF Core 9: Configure complex types
        modelBuilder.Entity<Order>(builder =>
        {
            // ShippingAddress as complex type
            builder.ComplexProperty(o => o.ShippingAddress, addressBuilder =>
            {
                addressBuilder.Property(a => a.Street)
                    .HasColumnName("ShippingStreet")
                    .HasMaxLength(200);
                addressBuilder.Property(a => a.City)
                    .HasColumnName("ShippingCity")
                    .HasMaxLength(100);
                addressBuilder.Property(a => a.State)
                    .HasColumnName("ShippingState")
                    .HasMaxLength(50);
                addressBuilder.Property(a => a.PostalCode)
                    .HasColumnName("ShippingPostalCode")
                    .HasMaxLength(20);
                addressBuilder.Property(a => a.Country)
                    .HasColumnName("ShippingCountry")
                    .HasMaxLength(100);
            });
            
            // BillingAddress as complex type
            builder.ComplexProperty(o => o.BillingAddress, addressBuilder =>
            {
                addressBuilder.Property(a => a.Street)
                    .HasColumnName("BillingStreet");
                addressBuilder.Property(a => a.City)
                    .HasColumnName("BillingCity");
                // ... similar configuration
            });
            
            // Money as complex type
            builder.ComplexProperty(o => o.TotalAmount, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                    .HasColumnName("TotalAmount")
                    .HasPrecision(18, 2);
                moneyBuilder.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3);
            });
        });
    }
}
```

### Querying Complex Types

```csharp
// EF Core 9: Query complex types
public class OrderQueries
{
    private readonly ApplicationDbContext _context;
    
    public OrderQueries(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Query by complex type property
    public async Task<List<Order>> FindOrdersByCity(string city)
    {
        return await _context.Orders
            .Where(o => o.ShippingAddress.City == city)
            .ToListAsync();
    }
    
    // Query multiple complex type properties
    public async Task<List<Order>> FindOrdersByRegionAsync(string state, string country)
    {
        return await _context.Orders
            .Where(o => 
                o.ShippingAddress.State == state && 
                o.ShippingAddress.Country == country)
            .ToListAsync();
    }
    
    // Query by money complex type
    public async Task<List<Order>> FindExpensiveOrdersAsync(decimal minAmount)
    {
        return await _context.Orders
            .Where(o => o.TotalAmount.Amount > minAmount)
            .Where(o => o.TotalAmount.Currency == "USD")
            .ToListAsync();
    }
    
    // Update complex type
    public async Task UpdateShippingAddressAsync(int orderId, Address newAddress)
    {
        await _context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(o => o.ShippingAddress, newAddress));
    }
    
    // Group by complex type property
    public async Task<List<CityOrderStats>> GetOrderStatsByCity()
    {
        return await _context.Orders
            .GroupBy(o => o.ShippingAddress.City)
            .Select(g => new CityOrderStats
            {
                City = g.Key,
                OrderCount = g.Count(),
                TotalRevenue = g.Sum(o => o.TotalAmount.Amount)
            })
            .OrderByDescending(s => s.OrderCount)
            .ToListAsync();
    }
}
```

### Complex Types in Hierarchies

```csharp
// EF Core 9: Complex types with inheritance
public abstract class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Complex type in base class
    public ContactInfo Contact { get; set; } = new();
}

public class Employee : Person
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    
    // Additional complex type
    public BankAccount BankAccount { get; set; } = new();
}

public class Customer : Person
{
    public string CustomerNumber { get; set; } = string.Empty;
    
    // Address complex type
    public Address Address { get; set; } = new();
}

public record ContactInfo
{
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
}

public record BankAccount
{
    public string AccountNumber { get; init; } = string.Empty;
    public string RoutingNumber { get; init; } = string.Empty;
}

// Configuration
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Person>(builder =>
    {
        builder.UseTphMappingStrategy(); // Table-Per-Hierarchy
        
        builder.ComplexProperty(p => p.Contact);
    });
    
    modelBuilder.Entity<Employee>(builder =>
    {
        builder.ComplexProperty(e => e.BankAccount);
    });
    
    modelBuilder.Entity<Customer>(builder =>
    {
        builder.ComplexProperty(c => c.Address);
    });
}
```

## Cosmos DB Provider Updates

### Improved Cosmos DB Support

```csharp
// EF Core 9: Enhanced Cosmos DB provider
public class CosmosDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseCosmos(
            accountEndpoint: "https://myaccount.documents.azure.com:443/",
            accountKey: "your-key",
            databaseName: "MyDatabase");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // EF Core 9: Better partition key support
        modelBuilder.Entity<Product>(builder =>
        {
            builder.ToContainer("Products");
            builder.HasPartitionKey(p => p.Category);
            
            // Configure hierarchical partition key
            builder.HasPartitionKey(p => new { p.Category, p.Brand });
        });
        
        modelBuilder.Entity<Order>(builder =>
        {
            builder.ToContainer("Orders");
            builder.HasPartitionKey(o => o.CustomerId);
            
            // EF Core 9: Configure TTL (Time To Live)
            builder.HasDefaultTimeToLive(TimeSpan.FromDays(90));
        });
    }
}

// Querying with partition key
public class CosmosQueries
{
    private readonly CosmosDbContext _context;
    
    public CosmosQueries(CosmosDbContext context)
    {
        _context = context;
    }
    
    // Efficient query with partition key
    public async Task<List<Product>> GetProductsByCategoryAsync(string category)
    {
        // EF Core 9: Optimized for single partition
        return await _context.Products
            .WithPartitionKey(category)
            .Where(p => p.Price > 10)
            .ToListAsync();
    }
    
    // Cross-partition query
    public async Task<List<Product>> GetAllExpensiveProductsAsync()
    {
        // Works across all partitions
        return await _context.Products
            .Where(p => p.Price > 1000)
            .ToListAsync();
    }
    
    // Point read (most efficient)
    public async Task<Product?> GetProductByIdAsync(string id, string category)
    {
        return await _context.Products
            .WithPartitionKey(category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

### Cosmos DB-Specific Features

```csharp
// EF Core 9: Cosmos DB advanced features
public class CosmosAdvanced
{
    private readonly CosmosDbContext _context;
    
    public CosmosAdvanced(CosmosDbContext context)
    {
        _context = context;
    }
    
    // Bulk operations
    public async Task BulkInsertProductsAsync(List<Product> products)
    {
        // EF Core 9: Optimized bulk operations
        foreach (var product in products)
        {
            _context.Products.Add(product);
        }
        
        // Uses Cosmos DB bulk API
        await _context.SaveChangesAsync();
    }
    
    // Raw SQL (Cosmos DB SQL API)
    public async Task<List<Product>> ExecuteRawQueryAsync()
    {
        return await _context.Products
            .FromSqlRaw("SELECT * FROM c WHERE c.Price > 100")
            .ToListAsync();
    }
    
    // Change feed support
    public async Task ProcessChangeFeedAsync()
    {
        // EF Core 9: Change feed integration
        var changes = _context.Products
            .AsNoTracking()
            .Where(p => p.UpdatedAt > DateTime.UtcNow.AddDays(-1));
        
        await foreach (var product in changes.AsAsyncEnumerable())
        {
            // Process changed product
            Console.WriteLine($"Product changed: {product.Name}");
        }
    }
}
```

## Performance Enhancements

### Query Caching and Compilation

```csharp
// EF Core 9: Improved query caching
public class PerformanceOptimizations
{
    private readonly ApplicationDbContext _context;
    
    // Compiled queries for better performance
    private static readonly Func<ApplicationDbContext, int, Task<Product?>> 
        GetProductByIdCompiled = 
        EF.CompileAsyncQuery((ApplicationDbContext context, int id) =>
            context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id));
    
    public PerformanceOptimizations(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Use compiled query
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        // Faster execution - query is pre-compiled
        return await GetProductByIdCompiled(_context, id);
    }
    
    // EF Core 9: Better query plan caching
    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        // Query plan is cached automatically
        return await _context.Products
            .Where(p => EF.Functions.Like(p.Name, $"%{searchTerm}%"))
            .OrderBy(p => p.Name)
            .Take(100)
            .ToListAsync();
    }
}
```

### Batch Operations

```csharp
// EF Core 9: Optimized batching
public class BatchOperations
{
    private readonly ApplicationDbContext _context;
    
    public BatchOperations(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Batch inserts
    public async Task BulkInsertAsync(List<Product> products)
    {
        // EF Core 9: Automatically batches into optimal sizes
        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();
    }
    
    // Batch updates
    public async Task BulkUpdatePricesAsync(Dictionary<int, decimal> priceUpdates)
    {
        var productIds = priceUpdates.Keys.ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();
        
        foreach (var product in products)
        {
            product.Price = priceUpdates[product.Id];
        }
        
        // EF Core 9: Batches updates efficiently
        await _context.SaveChangesAsync();
    }
    
    // Configure batch size
    public void ConfigureBatching()
    {
        _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
        
        // Set max batch size (if supported by provider)
        // Default is optimized automatically in EF Core 9
    }
}
```

### Connection Resilience

```csharp
// EF Core 9: Enhanced connection resilience
public class ResilientDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "connection-string",
            sqlOptions =>
            {
                // EF Core 9: Improved retry logic
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                
                // Command timeout
                sqlOptions.CommandTimeout(60);
                
                // Connection resilience
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
    }
}

// Custom retry strategy
public class CustomRetryStrategy : IExecutionStrategy
{
    private readonly SqlServerRetryingExecutionStrategy _strategy;
    
    public CustomRetryStrategy(DbContext context)
    {
        _strategy = new SqlServerRetryingExecutionStrategy(
            context,
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }
    
    public bool RetriesOnFailure => _strategy.RetriesOnFailure;
    
    public TResult Execute<TState, TResult>(
        TState state,
        Func<DbContext, TState, TResult> operation,
        Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
    {
        return _strategy.Execute(state, operation, verifySucceeded);
    }
    
    public Task<TResult> ExecuteAsync<TState, TResult>(
        TState state,
        Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
        Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
        CancellationToken cancellationToken = default)
    {
        return _strategy.ExecuteAsync(state, operation, verifySucceeded, cancellationToken);
    }
}
```

## Migration Improvements

### Enhanced Migrations

```csharp
// EF Core 9: Better migration support
public class MigrationOperations
{
    private readonly ApplicationDbContext _context;
    
    public MigrationOperations(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Check pending migrations
    public async Task<bool> HasPendingMigrationsAsync()
    {
        var pending = await _context.Database
            .GetPendingMigrationsAsync();
        return pending.Any();
    }
    
    // Apply migrations programmatically
    public async Task MigrateDatabaseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
            Console.WriteLine("Database migrated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Migration failed: {ex.Message}");
            throw;
        }
    }
    
    // Get applied migrations
    public async Task<List<string>> GetAppliedMigrationsAsync()
    {
        var applied = await _context.Database
            .GetAppliedMigrationsAsync();
        return applied.ToList();
    }
    
    // Create database if not exists
    public async Task EnsureDatabaseAsync()
    {
        var created = await _context.Database.EnsureCreatedAsync();
        if (created)
        {
            Console.WriteLine("Database created");
        }
    }
}
```

### Custom Migration Operations

```csharp
// EF Core 9: Custom migration with data seeding
public partial class AddProductsWithSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Category = table.Column<string>(maxLength: 100, nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });
        
        // Create index
        migrationBuilder.CreateIndex(
            name: "IX_Products_Category",
            table: "Products",
            column: "Category");
        
        // Seed initial data
        migrationBuilder.InsertData(
            table: "Products",
            columns: new[] { "Name", "Price", "Category", "CreatedAt" },
            values: new object[,]
            {
                { "Laptop", 999.99m, "Electronics", DateTime.UtcNow },
                { "Mouse", 29.99m, "Electronics", DateTime.UtcNow },
                { "Desk", 299.99m, "Furniture", DateTime.UtcNow }
            });
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Products");
    }
}

// Migration with raw SQL
public partial class AddStoredProcedure : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE PROCEDURE GetProductsByCategory
                @Category NVARCHAR(100)
            AS
            BEGIN
                SELECT * FROM Products
                WHERE Category = @Category
                ORDER BY Name
            END
        ");
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP PROCEDURE GetProductsByCategory");
    }
}
```

## Best Practices

### 1. LINQ Query Optimization
- Use compiled queries for frequently executed queries
- Leverage ExecuteUpdate/ExecuteDelete for bulk operations
- Use AsNoTracking() for read-only queries
- Project only needed columns with Select()
- Use appropriate query splitting strategy

### 2. JSON Columns
- Use JSON columns for semi-structured data
- Index frequently queried JSON properties
- Keep JSON documents reasonably sized (<100KB)
- Use appropriate column types (jsonb in PostgreSQL)
- Consider querying performance vs. flexibility tradeoff

### 3. Complex Types
- Use complex types for value objects without identity
- Keep complex types focused and cohesive
- Configure appropriate column naming
- Use records for immutable complex types
- Avoid deep nesting (max 2-3 levels)

### 4. Cosmos DB
- Always specify partition key in queries when possible
- Use hierarchical partition keys for better data distribution
- Implement point reads for best performance
- Configure appropriate TTL for time-based data
- Use bulk operations for large inserts

### 5. Performance
- Enable query caching for production
- Configure connection pooling appropriately
- Use split queries for multiple includes
- Implement retry logic for transient failures
- Monitor and log slow queries

### 6. Migrations
- Keep migrations small and focused
- Test migrations on production-like data
- Have rollback strategy ready
- Use data seeding sparingly in migrations
- Version control all migration files

## Interview Questions

### Question 1: ExecuteUpdate vs SaveChanges
**Q:** When should you use ExecuteUpdate instead of loading entities and calling SaveChanges?

**A:** Use ExecuteUpdate when:
1. **Bulk updates**: Updating many records (100s-1000s)
2. **Simple updates**: Changing one or few properties
3. **Performance critical**: Need fastest possible update
4. **No tracking needed**: Don't need change tracking

Use SaveChanges when:
- Need to execute business logic before update
- Updating complex object graphs
- Need change tracking and auditing
- Updating few entities (<10-20)

Performance comparison:
```csharp
// SaveChanges: Load → Modify → Save
// 10,000 products: ~3000ms, high memory
var products = await context.Products.Where(p => p.Category == "Electronics").ToListAsync();
products.ForEach(p => p.Price *= 1.1m);
await context.SaveChangesAsync();

// ExecuteUpdate: Direct SQL update
// 10,000 products: ~50ms, minimal memory
await context.Products
    .Where(p => p.Category == "Electronics")
    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Price, p => p.Price * 1.1m));
```

### Question 2: JSON Columns Use Cases
**Q:** When should you use JSON columns instead of normalized tables in EF Core 9?

**A:** Use JSON columns for:
1. **Semi-structured data**: Varying attributes per product type
2. **Flexible schemas**: Frequently changing structure
3. **Document storage**: Complete documents as single unit
4. **Read-heavy data**: Mostly read, rarely updated
5. **Nested collections**: Small collections embedded in entity

Use normalized tables for:
- Frequently queried/filtered data
- Data requiring referential integrity
- Large collections (>100 items)
- Data updated independently
- Need for complex joins

Example decision:
```csharp
// JSON: Product attributes vary by type
public class Product {
    public JsonAttributes Attributes { get; set; } // Flexible
}

// Normalized: Order items need referential integrity
public class Order {
    public List<OrderItem> Items { get; set; } // Separate table
}
```

### Question 3: Complex Types vs Owned Types
**Q:** What's the difference between complex types and owned types in EF Core 9?

**A:** Key differences:

**Complex Types (EF Core 9)**:
- Value objects without identity
- Always part of parent entity
- Stored in same table as parent
- Cannot be shared between entities
- Better for simple value objects

```csharp
builder.ComplexProperty(o => o.Address);
// Result: ShippingStreet, ShippingCity columns in Orders table
```

**Owned Types (existing)**:
- Can have separate table
- Can be shared via owned type collection
- More configuration options
- Can have navigation properties

```csharp
builder.OwnsOne(o => o.Address);
// Can be: Same table OR separate table
```

Use complex types for: Money, Address, Coordinates (simple value objects)
Use owned types for: More complex scenarios with navigations

### Question 4: Cosmos DB Partition Strategy
**Q:** How do you design an effective partition key strategy for Cosmos DB in EF Core 9?

**A:** Effective partition key design:

**Principles**:
1. **High cardinality**: Many distinct values
2. **Even distribution**: Avoid hot partitions
3. **Query pattern**: Match common query filters
4. **Scalability**: Consider future growth

**Examples**:
```csharp
// Good: UserId for user-centric app
builder.HasPartitionKey(o => o.UserId);
// Queries: SELECT * FROM Orders WHERE UserId = 'user123'

// Better: Hierarchical for multi-tenant
builder.HasPartitionKey(o => new { o.TenantId, o.Category });
// Better distribution + tenant isolation

// Bad: Status field (low cardinality)
builder.HasPartitionKey(o => o.Status); // Only 3-4 values!

// Bad: Timestamp (time-based hot spots)
builder.HasPartitionKey(o => o.CreatedDate); // Recent = hot
```

**Best practice**: Partition by entity that query filters use most (UserId, TenantId, etc.)

### Question 5: Query Splitting
**Q:** Explain AsSplitQuery() vs AsSingleQuery() in EF Core 9 and when to use each.

**A:** Comparison:

**AsSingleQuery()** (default for includes):
```csharp
// Single SQL query with JOINs
var orders = await context.Orders
    .Include(o => o.Items)
    .Include(o => o.Customer)
    .AsSingleQuery()
    .ToListAsync();

// SQL: One query with LEFT JOINs
// Result: Cartesian product (potential duplication)
```

Pros: Single roundtrip, good for small collections
Cons: Data duplication (items × customer), memory intensive

**AsSplitQuery()**:
```csharp
// Multiple SQL queries
var orders = await context.Orders
    .Include(o => o.Items)
    .Include(o => o.Customer)
    .AsSplitQuery()
    .ToListAsync();

// SQL: Three queries
// 1. SELECT Orders
// 2. SELECT Items WHERE OrderId IN (...)
// 3. SELECT Customers WHERE Id IN (...)
```

Pros: No duplication, less memory, better for large collections
Cons: Multiple roundtrips, potential consistency issues

**Use AsSplitQuery when**:
- Including collections (one-to-many)
- Large result sets
- Memory constrained

**Use AsSingleQuery when**:
- Single navigation properties
- Small result sets
- Consistency critical

### Question 6: Prune Collections
**Q:** What is collection pruning in EF Core 9 and how does it improve performance?

**A:** Collection pruning automatically sets empty collections to null instead of empty lists.

**Benefits**:
1. **Memory**: Null uses 8 bytes, empty list uses ~40 bytes
2. **GC pressure**: Fewer objects to track
3. **Serialization**: Smaller JSON output

Example:
```csharp
// Query customers with completed orders
var customers = await context.Customers
    .Include(c => c.Orders.Where(o => o.Status == "Completed"))
    .ToListAsync();

// EF Core 8: Customer with 0 completed orders
// customer.Orders = new List<Order>() // Empty list

// EF Core 9: Customer with 0 completed orders  
// customer.Orders = null // Null (pruned)
```

Impact on 10,000 customers with 20% having orders:
- EF Core 8: 8,000 empty lists × 40 bytes = 320 KB wasted
- EF Core 9: 8,000 nulls × 8 bytes = 64 KB

**Handle in code**:
```csharp
// Null-safe access
var orderCount = customer.Orders?.Count ?? 0;
var hasOrders = customer.Orders?.Any() == true;
```

### Question 7: Compiled Queries Performance
**Q:** How much performance improvement can compiled queries provide in EF Core 9?

**A:** Performance improvements:

**Regular Query**:
- Query translation: ~50-100μs per execution
- Cached after first run, but still overhead
- Good for varied queries

**Compiled Query**:
- Query translation: Once at compilation
- Execution: ~10-20μs (5x faster)
- Best for frequently executed queries

Benchmark:
```csharp
// Regular query: 1 million executions
for (int i = 0; i < 1_000_000; i++)
{
    var product = await context.Products
        .Where(p => p.Id == i)
        .FirstOrDefaultAsync();
}
// Total: ~80 seconds

// Compiled query: 1 million executions
var getProduct = EF.CompileAsyncQuery(
    (DbContext ctx, int id) => 
        ctx.Products.FirstOrDefault(p => p.Id == id));

for (int i = 0; i < 1_000_000; i++)
{
    var product = await getProduct(context, i);
}
// Total: ~15 seconds (5.3x faster)
```

**Use compiled queries for**:
- High-frequency queries (>1000/sec)
- APIs with tight latency requirements
- Dashboard queries executed repeatedly

### Question 8: JSON Query Performance
**Q:** What are the performance implications of querying JSON columns vs normalized tables?

**A:** Performance comparison:

**JSON Columns**:
- **Read**: Fast for fetching complete document
- **Query**: Slower for filtering on JSON properties (may not use indexes)
- **Update**: Entire document rewritten
- **Storage**: Potentially more compact

**Normalized Tables**:
- **Read**: Slower for complete data (requires joins)
- **Query**: Fast with proper indexes
- **Update**: Only affected rows updated
- **Storage**: More overhead (referential integrity)

Benchmarks (10,000 products):
```csharp
// Query JSON property: ~150ms (table scan)
var products = await context.Products
    .Where(p => p.Attributes.Color == "Red")
    .ToListAsync();

// Query normalized column: ~5ms (index seek)
var products = await context.Products
    .Where(p => p.Color == "Red")
    .ToListAsync();

// Fetch complete document with 5 attributes:
// JSON: ~3ms (single column)
// Normalized: ~8ms (5 joins)
```

**Best practice**: JSON for flexible attributes, normalized for query-critical fields.

### Question 9: Cosmos DB vs SQL Server Choice
**Q:** When should you choose Cosmos DB provider over SQL Server in EF Core 9?

**A:** Choose **Cosmos DB** when:
1. **Global distribution**: Need multi-region replication
2. **Elastic scale**: Variable workload with spikes
3. **Document model**: Natural document structure
4. **Low latency**: <10ms read requirements
5. **NoSQL benefits**: Flexible schema

Choose **SQL Server** when:
1. **Complex transactions**: Multi-table ACID transactions
2. **Complex joins**: Heavy relational queries
3. **OLAP workloads**: Complex aggregations, reporting
4. **Cost sensitive**: Predictable costs
5. **Existing expertise**: Team knows SQL Server well

Cost comparison:
```
Cosmos DB:
- 400 RU/s minimum: ~$24/month
- 10,000 RU/s: ~$580/month
- Pay for throughput + storage

SQL Server (Azure):
- Basic: ~$5/month
- Standard S3: ~$150/month
- Premium: ~$450/month
- Pay for tier + storage
```

**Hybrid approach**: Use both – Cosmos for user data, SQL for analytics.

### Question 10: Migration Best Practices
**Q:** What strategy should you use for zero-downtime migrations in production?

**A:** Zero-downtime migration strategy:

**Phase 1: Additive Changes**
```csharp
// Add new column (nullable)
migrationBuilder.AddColumn<string>(
    name: "NewEmail",
    table: "Users",
    nullable: true);

// Deploy code that writes to both old and new
```

**Phase 2: Data Migration**
```csharp
// Backfill data (can run anytime)
migrationBuilder.Sql(@"
    UPDATE Users 
    SET NewEmail = Email 
    WHERE NewEmail IS NULL
");
```

**Phase 3: Cutover**
```csharp
// Deploy code that reads from new column
// Verify in production
```

**Phase 4: Cleanup**
```csharp
// Make column required
migrationBuilder.AlterColumn<string>(
    name: "NewEmail",
    table: "Users",
    nullable: false);

// Drop old column
migrationBuilder.DropColumn(
    name: "Email",
    table: "Users");
```

**Key principles**:
- Always additive first
- Backward compatible code
- Separate data migration from schema
- Verify each phase
- Rollback plan ready

---

**Last Updated: January 2026 - .NET 9**
