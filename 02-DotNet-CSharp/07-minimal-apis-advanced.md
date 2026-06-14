# Advanced Minimal APIs in .NET 9

## Table of Contents
- [Introduction to Minimal APIs](#introduction-to-minimal-apis)
- [Endpoint Filters](#endpoint-filters)
- [Route Groups and Organization](#route-groups-and-organization)
- [Typed Results](#typed-results)
- [OpenAPI and Documentation](#openapi-and-documentation)
- [Validation and Binding](#validation-and-binding)
- [Testing Strategies](#testing-strategies)
- [Best Practices and Patterns](#best-practices-and-patterns)
- [Advanced Scenarios](#advanced-scenarios)
- [Interview Questions](#interview-questions)

## Introduction to Minimal APIs

Minimal APIs provide a lightweight approach to building HTTP APIs in .NET with minimal overhead and ceremony. Introduced in .NET 6 and significantly enhanced in .NET 9.

### Basic Structure

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// Define endpoints
app.MapGet("/", () => "Hello, Minimal API!");

app.MapGet("/api/products/{id:int}", (int id) => 
    Results.Ok(new { Id = id, Name = $"Product {id}" }));

app.Run();
```

### Slim Builder (New in .NET 9)

```csharp
using Microsoft.AspNetCore.Builder;

// WebApplication.CreateSlimBuilder excludes:
// - Built-in middleware
// - Some default services
// - Larger dependency graph
// Perfect for Native AOT scenarios

var builder = WebApplication.CreateSlimBuilder(args);

// Explicitly add only what you need
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

app.Run();

// JSON Source Generator for AOT
[JsonSerializable(typeof(HealthResponse))]
internal partial class AppJsonContext : JsonSerializerContext { }

record HealthResponse(string Status);
```

## Endpoint Filters

Endpoint filters provide a way to execute code before and after endpoint handlers, similar to middleware but specific to individual endpoints.

### Creating Custom Filters

```csharp
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

// Simple logging filter
public class LoggingFilter : IEndpointFilter
{
    private readonly ILogger<LoggingFilter> _logger;
    
    public LoggingFilter(ILogger<LoggingFilter> logger)
    {
        _logger = logger;
    }
    
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Executing endpoint: {Endpoint}", 
            context.HttpContext.Request.Path);
        
        try
        {
            var result = await next(context);
            return result;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Endpoint executed in {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds);
        }
    }
}

// Usage
app.MapGet("/api/products/{id}", (int id) => 
    Results.Ok(new Product(id, $"Product {id}")))
    .AddEndpointFilter<LoggingFilter>();
```

### Validation Filter

```csharp
using FluentValidation;

// Validation filter using FluentValidation
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices
            .GetService<IValidator<T>>();
            
        if (validator is null)
            return await next(context);
        
        var argument = context.Arguments
            .OfType<T>()
            .FirstOrDefault();
            
        if (argument is null)
            return await next(context);
        
        var validationResult = await validator.ValidateAsync(argument);
        
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(
                validationResult.ToDictionary());
        }
        
        return await next(context);
    }
}

// Define validator
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
            
        RuleFor(x => x.Price)
            .GreaterThan(0);
    }
}

// Register and use
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

app.MapPost("/api/products", (CreateProductRequest request) =>
{
    return Results.Created($"/api/products/{request.Id}", request);
})
.AddEndpointFilter<ValidationFilter<CreateProductRequest>>();

record CreateProductRequest(int Id, string Name, decimal Price);
```

### Authorization Filter

```csharp
// Custom authorization filter
public class RequireClaimFilter : IEndpointFilter
{
    private readonly string _claimType;
    private readonly string _claimValue;
    
    public RequireClaimFilter(string claimType, string claimValue)
    {
        _claimType = claimType;
        _claimValue = claimValue;
    }
    
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;
        
        if (!user.HasClaim(_claimType, _claimValue))
        {
            return Results.Forbid();
        }
        
        return await next(context);
    }
}

// Usage with factory method
app.MapDelete("/api/products/{id}", (int id) => 
    Results.NoContent())
    .AddEndpointFilter(async (context, next) =>
    {
        var user = context.HttpContext.User;
        if (!user.HasClaim("role", "admin"))
            return Results.Forbid();
        return await next(context);
    })
    .RequireAuthorization();
```

### Rate Limiting Filter

```csharp
using System.Threading.RateLimiting;

// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var userIdentifier = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: userIdentifier,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            });
    });
    
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync(
            "Rate limit exceeded. Try again later.", 
            cancellationToken);
    };
});

var app = builder.Build();
app.UseRateLimiter();

// Apply rate limiting to specific endpoints
app.MapGet("/api/products", () => GetProducts())
    .RequireRateLimiting("fixed");
```

## Route Groups and Organization

Route groups help organize related endpoints with shared configuration.

### Basic Route Groups

```csharp
// Create a route group for products
var productsGroup = app.MapGroup("/api/products")
    .WithTags("Products")
    .WithOpenApi();

productsGroup.MapGet("/", GetAllProducts);
productsGroup.MapGet("/{id:int}", GetProductById);
productsGroup.MapPost("/", CreateProduct);
productsGroup.MapPut("/{id:int}", UpdateProduct);
productsGroup.MapDelete("/{id:int}", DeleteProduct);

// Handlers
static async Task<IResult> GetAllProducts(IProductRepository repo)
{
    var products = await repo.GetAllAsync();
    return Results.Ok(products);
}

static async Task<IResult> GetProductById(int id, IProductRepository repo)
{
    var product = await repo.GetByIdAsync(id);
    return product is null ? Results.NotFound() : Results.Ok(product);
}
```

### Nested Groups with Shared Filters

```csharp
// API group with versioning
var apiV1 = app.MapGroup("/api/v1")
    .AddEndpointFilter<LoggingFilter>()
    .RequireRateLimiting("fixed");

// Products group
var products = apiV1.MapGroup("/products")
    .WithTags("Products");

products.MapGet("/", GetAllProducts);
products.MapGet("/{id}", GetProductById);

// Admin products group with authorization
var adminProducts = products.MapGroup("/admin")
    .RequireAuthorization("AdminPolicy")
    .AddEndpointFilter<AuditFilter>();

adminProducts.MapPost("/", CreateProduct);
adminProducts.MapDelete("/{id}", DeleteProduct);

// Categories group
var categories = apiV1.MapGroup("/categories")
    .WithTags("Categories");

categories.MapGet("/", GetAllCategories);
categories.MapGet("/{id}", GetCategoryById);
```

### Extension Methods for Organization

```csharp
// Create extension methods for clean organization
public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllProducts)
            .WithName("GetAllProducts")
            .WithSummary("Retrieves all products")
            .Produces<List<Product>>(StatusCodes.Status200OK);
            
        group.MapGet("/{id:int}", GetProductById)
            .WithName("GetProductById")
            .Produces<Product>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
            
        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .Produces<Product>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
            
        return group;
    }
    
    private static async Task<IResult> GetAllProducts(
        IProductRepository repository,
        CancellationToken ct)
    {
        var products = await repository.GetAllAsync(ct);
        return Results.Ok(products);
    }
    
    private static async Task<IResult> GetProductById(
        int id,
        IProductRepository repository,
        CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(id, ct);
        return product is null ? Results.NotFound() : Results.Ok(product);
    }
    
    private static async Task<IResult> CreateProduct(
        CreateProductRequest request,
        IProductRepository repository,
        CancellationToken ct)
    {
        var product = new Product(0, request.Name, request.Price);
        var created = await repository.CreateAsync(product, ct);
        return Results.Created($"/api/products/{created.Id}", created);
    }
}

// Usage
var products = app.MapGroup("/api/products")
    .WithTags("Products");
    
products.MapProductEndpoints();
```

## Typed Results

Typed results provide compile-time safety and better IntelliSense support.

### Using Results<T>

```csharp
using Microsoft.AspNetCore.Http.HttpResults;

// Traditional approach - not type-safe
app.MapGet("/api/products/{id}", (int id) =>
{
    if (id <= 0)
        return Results.BadRequest("Invalid ID");
    
    var product = GetProduct(id);
    if (product is null)
        return Results.NotFound();
    
    return Results.Ok(product);
}); // Return type is IResult

// Typed approach - compile-time safety
app.MapGet("/api/products/{id}", GetProductByIdTyped);

static Results<Ok<Product>, NotFound, BadRequest<string>> GetProductByIdTyped(int id)
{
    if (id <= 0)
        return TypedResults.BadRequest("Invalid ID");
    
    var product = GetProduct(id);
    if (product is null)
        return TypedResults.NotFound();
    
    return TypedResults.Ok(product);
}
```

### Complex Typed Results

```csharp
// Multiple possible return types
static Results<Ok<Product>, NotFound, ValidationProblem, UnauthorizedHttpResult> 
    CreateProduct(
        CreateProductRequest request,
        HttpContext context,
        IProductRepository repository)
{
    // Authorization check
    if (!context.User.Identity?.IsAuthenticated ?? true)
        return TypedResults.Unauthorized();
    
    // Validation
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = new[] { "Name is required" }
        };
        return TypedResults.ValidationProblem(errors);
    }
    
    // Business logic
    var product = new Product(0, request.Name, request.Price);
    var created = repository.Create(product);
    
    if (created is null)
        return TypedResults.NotFound();
    
    return TypedResults.Ok(created);
}

app.MapPost("/api/products", CreateProduct)
    .Produces<Product>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .ProducesValidationProblem()
    .Produces(StatusCodes.Status401Unauthorized);
```

### Custom Typed Results

```csharp
// Create custom result type
public sealed class CreatedWithLocation<T> : IResult
{
    private readonly string _location;
    private readonly T _value;
    
    public CreatedWithLocation(string location, T value)
    {
        _location = location;
        _value = value;
    }
    
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status201Created;
        httpContext.Response.Headers.Location = _location;
        await httpContext.Response.WriteAsJsonAsync(_value);
    }
}

// Extension method
public static class CustomResults
{
    public static CreatedWithLocation<T> CreatedWithLocation<T>(string location, T value)
    {
        return new CreatedWithLocation<T>(location, value);
    }
}

// Usage
app.MapPost("/api/products", (CreateProductRequest request) =>
{
    var product = new Product(1, request.Name, request.Price);
    return CustomResults.CreatedWithLocation($"/api/products/{product.Id}", product);
});
```

## OpenAPI and Documentation

.NET 9 includes built-in OpenAPI support with improved integration.

### Basic OpenAPI Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI services (new in .NET 9)
builder.Services.AddOpenApi();

var app = builder.Build();

// Map OpenAPI endpoints
app.MapOpenApi();

// Optional: Add Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "My API V1");
    });
}

// Document endpoints
app.MapGet("/api/products", GetProducts)
    .WithName("GetProducts")
    .WithSummary("Get all products")
    .WithDescription("Retrieves a list of all available products")
    .WithTags("Products")
    .Produces<List<Product>>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status500InternalServerError);
```

### Advanced OpenAPI Customization

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "My Advanced API",
            Version = "v1",
            Description = "A comprehensive API for product management",
            Contact = new()
            {
                Name = "API Support",
                Email = "support@example.com"
            },
            License = new()
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        };
        
        // Add security scheme
        document.Components ??= new();
        document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
        {
            ["Bearer"] = new()
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            }
        };
        
        return Task.CompletedTask;
    });
});

// Add detailed endpoint documentation
app.MapPost("/api/products", CreateProduct)
    .WithOpenApi(operation =>
    {
        operation.Summary = "Create a new product";
        operation.Description = "Creates a new product with the provided details";
        operation.Parameters[0].Description = "The product creation request";
        
        operation.Responses["201"].Description = "Product created successfully";
        operation.Responses["400"].Description = "Invalid product data";
        
        return operation;
    })
    .Accepts<CreateProductRequest>("application/json")
    .Produces<Product>(StatusCodes.Status201Created)
    .ProducesValidationProblem();
```

### XML Comments Integration

```csharp
// Enable XML documentation
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<XmlCommentsTransformer>();
});

/// <summary>
/// Gets a product by its ID
/// </summary>
/// <param name="id">The product identifier</param>
/// <param name="repository">The product repository</param>
/// <returns>The product if found, otherwise 404</returns>
/// <response code="200">Returns the product</response>
/// <response code="404">Product not found</response>
[EndpointSummary("Get product by ID")]
[EndpointDescription("Retrieves a single product by its unique identifier")]
static async Task<Results<Ok<Product>, NotFound>> GetProductById(
    int id,
    IProductRepository repository)
{
    var product = await repository.GetByIdAsync(id);
    return product is null 
        ? TypedResults.NotFound() 
        : TypedResults.Ok(product);
}
```

## Validation and Binding

### Parameter Binding

```csharp
// Route parameters
app.MapGet("/api/products/{id:int}", (int id) => 
    Results.Ok(new { Id = id }));

// Query string parameters
app.MapGet("/api/products/search", 
    ([FromQuery] string? name, [FromQuery] decimal? minPrice) =>
    {
        return Results.Ok(new { Name = name, MinPrice = minPrice });
    });

// Header binding
app.MapGet("/api/products", 
    ([FromHeader(Name = "X-API-Version")] string apiVersion) =>
    {
        return Results.Ok(new { Version = apiVersion });
    });

// Request body binding
app.MapPost("/api/products", 
    ([FromBody] CreateProductRequest request) =>
    {
        return Results.Created($"/api/products/{request.Id}", request);
    });

// Service injection
app.MapGet("/api/products", 
    (IProductRepository repository, ILogger<Program> logger) =>
    {
        logger.LogInformation("Getting all products");
        return repository.GetAll();
    });

// HttpContext access
app.MapGet("/api/products", (HttpContext context) =>
{
    var userAgent = context.Request.Headers.UserAgent.ToString();
    return Results.Ok(new { UserAgent = userAgent });
});
```

### Custom Binding

```csharp
// Custom binder using TryParse pattern
public record ProductFilter
{
    public string? Name { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    
    public static bool TryParse(string? value, out ProductFilter? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;
        
        // Parse format: name:value;minPrice:10;maxPrice:100
        var parts = value.Split(';');
        var filter = new ProductFilter();
        
        foreach (var part in parts)
        {
            var keyValue = part.Split(':');
            if (keyValue.Length != 2) continue;
            
            switch (keyValue[0].ToLower())
            {
                case "name":
                    filter = filter with { Name = keyValue[1] };
                    break;
                case "minprice":
                    if (decimal.TryParse(keyValue[1], out var min))
                        filter = filter with { MinPrice = min };
                    break;
                case "maxprice":
                    if (decimal.TryParse(keyValue[1], out var max))
                        filter = filter with { MaxPrice = max };
                    break;
            }
        }
        
        result = filter;
        return true;
    }
}

// Usage - automatically bound from query string
app.MapGet("/api/products/filter", 
    (ProductFilter? filter) =>
    {
        return Results.Ok(filter);
    });
// Call: /api/products/filter?filter=name:laptop;minPrice:500;maxPrice:2000
```

### Validation with Data Annotations

```csharp
using System.ComponentModel.DataAnnotations;
using MiniValidation;

public record CreateProductRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; init; } = string.Empty;
    
    [Range(0.01, 999999.99)]
    public decimal Price { get; init; }
    
    [EmailAddress]
    public string? ContactEmail { get; init; }
}

// Add MiniValidation package
builder.Services.AddEndpointsApiExplorer();

app.MapPost("/api/products", async (CreateProductRequest request) =>
{
    if (!MiniValidator.TryValidate(request, out var errors))
    {
        return Results.ValidationProblem(errors);
    }
    
    // Process valid request
    return Results.Created($"/api/products/{1}", request);
});
```

## Testing Strategies

### Unit Testing with WebApplicationFactory

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

public class ProductEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public ProductEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetProducts_ReturnsSuccessAndList()
    {
        // Act
        var response = await _client.GetAsync("/api/products");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotNull(products);
    }
    
    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new CreateProductRequest(0, "Test Product", 19.99m);
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/products", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);
        Assert.Equal("Test Product", product.Name);
    }
}
```

### Custom WebApplicationFactory

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);
            
            // Add in-memory database
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });
            
            // Replace real services with test doubles
            services.AddScoped<IProductRepository, FakeProductRepository>();
        });
        
        builder.UseEnvironment("Testing");
    }
}

public class FakeProductRepository : IProductRepository
{
    private readonly List<Product> _products = new()
    {
        new Product(1, "Test Product 1", 10.00m),
        new Product(2, "Test Product 2", 20.00m)
    };
    
    public Task<List<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_products);
    }
    
    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
    }
}
```

### Integration Testing with Test Containers

```csharp
using Testcontainers.PostgreSql;
using Xunit;

public class DatabaseIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("testdb")
        .WithUsername("test")
        .WithPassword("test")
        .Build();
    
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);
                    
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseNpgsql(_dbContainer.GetConnectionString());
                    });
                });
            });
        
        _client = _factory.CreateClient();
        
        // Run migrations
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _factory.DisposeAsync();
        _client.Dispose();
    }
    
    [Fact]
    public async Task CreateAndRetrieveProduct_WorksWithRealDatabase()
    {
        // Arrange
        var request = new CreateProductRequest(0, "Integration Test Product", 29.99m);
        
        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/api/products", request);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<Product>();
        
        // Act - Retrieve
        var getResponse = await _client.GetAsync($"/api/products/{created!.Id}");
        getResponse.EnsureSuccessStatusCode();
        var retrieved = await getResponse.Content.ReadFromJsonAsync<Product>();
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(created.Name, retrieved.Name);
        Assert.Equal(created.Price, retrieved.Price);
    }
}
```

## Best Practices and Patterns

### 1. Organize Endpoints

```csharp
// Use extension methods and route groups
public static class WebApplicationExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api")
            .WithOpenApi();
        
        api.MapProductEndpoints();
        api.MapCategoryEndpoints();
        api.MapOrderEndpoints();
        
        return app;
    }
}

// In Program.cs
var app = builder.Build();
app.MapApiEndpoints();
app.Run();
```

### 2. Use Typed Results

```csharp
// Always prefer typed results for compile-time safety
static Results<Ok<Product>, NotFound, ValidationProblem> GetProduct(int id)
{
    // Implementation
}
```

### 3. Implement Proper Error Handling

```csharp
// Global exception handling
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features
            .Get<IExceptionHandlerFeature>();
        
        var error = exceptionHandlerFeature?.Error;
        
        var problemDetails = new ProblemDetails
        {
            Title = "An error occurred",
            Status = StatusCodes.Status500InternalServerError,
            Detail = error?.Message
        };
        
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});
```

### 4. Use Dependency Injection

```csharp
// Register services properly
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<IProductCache, ProductCache>();

// Inject in endpoints
app.MapGet("/api/products", async (
    IProductRepository repository,
    IProductCache cache,
    ILogger<Program> logger) =>
{
    logger.LogInformation("Fetching products");
    var products = await cache.GetOrCreateAsync(
        "all-products",
        async () => await repository.GetAllAsync());
    return Results.Ok(products);
});
```

## Advanced Scenarios

### Response Caching

```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));
    
    options.AddPolicy("ProductCache", builder => 
        builder.Expire(TimeSpan.FromMinutes(10))
               .Tag("products"));
});

var app = builder.Build();
app.UseOutputCache();

app.MapGet("/api/products", GetProducts)
    .CacheOutput("ProductCache");

app.MapPost("/api/products", CreateProduct)
    .CacheOutput(policy => policy.Expire(TimeSpan.Zero).Tag("products"));
```

### Request/Response Compression

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

var app = builder.Build();
app.UseResponseCompression();
```

## Interview Questions

### 1. What are Minimal APIs and how do they differ from Controller-based APIs?
**Answer**: Minimal APIs are a lightweight approach to building HTTP APIs in .NET without controllers. They use function-based endpoints defined directly on WebApplication. Differences: (1) Less ceremony - no controller classes, (2) Better performance - less abstraction overhead, (3) Simpler dependency injection per endpoint, (4) Ideal for microservices and small APIs. Controller-based APIs offer better organization for large applications with many endpoints.

### 2. Explain endpoint filters and how they differ from middleware.
**Answer**: Endpoint filters execute code before/after specific endpoint handlers, implementing IEndpointFilter. Middleware runs for all requests in the pipeline. Key differences: (1) Filters are endpoint-specific, middleware is global, (2) Filters have access to endpoint metadata and typed arguments, (3) Filters can short-circuit specific endpoints, (4) Filters are more targeted for logging, validation, authorization per endpoint. Use filters for endpoint-specific concerns, middleware for cross-cutting concerns.

### 3. What are typed results (Results<T>) and what benefits do they provide?
**Answer**: Results<T> is a union type representing multiple possible IResult types from an endpoint. Example: `Results<Ok<Product>, NotFound, BadRequest>`. Benefits: (1) Compile-time safety - compiler verifies all return paths, (2) Better IntelliSense and documentation, (3) Automatic OpenAPI documentation of all response types, (4) Explicit contract of what endpoint can return, (5) Easier refactoring. Use TypedResults static class to create typed results.

### 4. How do route groups help organize Minimal APIs?
**Answer**: Route groups (MapGroup) allow applying shared configuration to multiple endpoints. Benefits: (1) Common prefix for related endpoints, (2) Shared filters and middleware, (3) Common authorization policies, (4) Shared OpenAPI tags and metadata, (5) Better code organization. Can be nested for hierarchical organization. Example: `/api/v1/products` group with shared rate limiting and logging filters.

### 5. How do you implement validation in Minimal APIs?
**Answer**: Multiple approaches: (1) Endpoint filters with FluentValidation or Data Annotations, (2) MiniValidation library for simple validation, (3) Custom validation logic in endpoint handlers, (4) Combine with Results.ValidationProblem() for RFC 7807 problem details. Recommended: Create reusable ValidationFilter<T> that validates and returns ValidationProblem if invalid. Register validators with DI and apply filter to endpoints needing validation.

### 6. Explain parameter binding in Minimal APIs.
**Answer**: Minimal APIs automatically bind parameters from: (1) Route values `{id:int}`, (2) Query string `[FromQuery]`, (3) Headers `[FromHeader]`, (4) Request body `[FromBody]` (JSON), (5) Services via DI, (6) HttpContext, (7) Custom binding via TryParse pattern. Order matters - route values checked first, then query, then body. Complex types from body, simple types from route/query by default.

### 7. How do you test Minimal APIs effectively?
**Answer**: Use WebApplicationFactory for integration tests: (1) Create test class with IClassFixture<WebApplicationFactory<Program>>, (2) Override ConfigureWebHost to replace services with test doubles, (3) Use HttpClient to call endpoints, (4) Assert responses with ReadFromJsonAsync<T>, (5) For real database tests, use Testcontainers to spin up containers. Mock dependencies with services.AddScoped in test factory. Can test endpoint logic in isolation by extracting to static methods.

### 8. What is the difference between WebApplication.CreateBuilder and CreateSlimBuilder?
**Answer**: CreateSlimBuilder (new in .NET 9) creates minimal builder for Native AOT scenarios: (1) Excludes built-in middleware and services, (2) Smaller dependency graph, (3) Faster startup, (4) Smaller executable size, (5) Better AOT compatibility. CreateBuilder includes full feature set: Kestrel configuration, JSON options, logging, etc. Use CreateSlimBuilder for serverless, edge computing, or when explicit control needed. Regular CreateBuilder for full-featured apps.

### 9. How do you implement OpenAPI documentation for Minimal APIs?
**Answer**: Use AddOpenApi() (built-in .NET 9): (1) Call services.AddOpenApi() to register services, (2) Call app.MapOpenApi() to expose /openapi/v1.json, (3) Use .WithOpenApi() fluent API to document endpoints, (4) Use .Produces<T>() for response types, (5) Add document transformers for global configuration, (6) Enable XML comments with GenerateDocumentationFile, (7) Use WithSummary, WithDescription, WithTags for metadata. Automatic integration with typed results.

### 10. What are best practices for organizing Minimal APIs in production applications?
**Answer**: (1) Use route groups for logical organization, (2) Extract endpoints to extension methods (MapProductEndpoints), (3) Use typed results for type safety, (4) Implement endpoint filters for cross-cutting concerns, (5) Separate endpoint handlers from route definitions for testability, (6) Use source generators for JSON serialization (AOT compatibility), (7) Implement proper error handling with ProblemDetails, (8) Add comprehensive OpenAPI documentation, (9) Use output caching for read-heavy endpoints, (10) Organize by feature/domain rather than technical concerns.

---

**Last Updated: January 2026 - .NET 9**

**Related Topics**: See also [Native AOT](./06-native-aot.md), [Authentication 2026](./08-auth-2026.md), [Testing .NET 9](./10-testing-dotnet-9.md)
