# ASP.NET Core 9 Features - Complete Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Minimal APIs Enhancements](#minimal-apis-enhancements)
3. [Native AOT Support](#native-aot-support)
4. [OpenAPI Document Generation](#openapi-document-generation)
5. [New Middleware Features](#new-middleware-features)
6. [Blazor .NET 9 Updates](#blazor-net-9-updates)
7. [SignalR Improvements](#signalr-improvements)
8. [Best Practices](#best-practices)
9. [Interview Questions](#interview-questions)

## Introduction

ASP.NET Core 9, released with .NET 9 in November 2024, introduces powerful features focused on performance, developer productivity, and modern web development patterns. This guide explores key enhancements with practical examples and production-ready patterns.

## Minimal APIs Enhancements

### Typed Results and Improved Filters

```csharp
// .NET 9: Enhanced typed results
public class ProductsApi
{
    public static void MapProductEndpoints(WebApplication app)
    {
        var products = app.MapGroup("/api/products")
            .WithTags("Products")
            .WithOpenApi();
        
        // Typed results with better IntelliSense
        products.MapGet("/", GetAllProducts)
            .Produces<List<Product>>(200)
            .Produces(404);
        
        products.MapGet("/{id:int}", GetProductById)
            .Produces<Product>(200)
            .Produces(404);
        
        products.MapPost("/", CreateProduct)
            .Produces<Product>(201)
            .Produces<ValidationProblemDetails>(400);
        
        products.MapPut("/{id:int}", UpdateProduct)
            .Produces(204)
            .Produces<ValidationProblemDetails>(400)
            .Produces(404);
        
        products.MapDelete("/{id:int}", DeleteProduct)
            .Produces(204)
            .Produces(404);
    }
    
    static async Task<Results<Ok<List<Product>>, NotFound>> GetAllProducts(
        IProductRepository repository)
    {
        var products = await repository.GetAllAsync();
        return TypedResults.Ok(products);
    }
    
    static async Task<Results<Ok<Product>, NotFound>> GetProductById(
        int id,
        IProductRepository repository)
    {
        var product = await repository.GetByIdAsync(id);
        return product is not null
            ? TypedResults.Ok(product)
            : TypedResults.NotFound();
    }
    
    static async Task<Results<Created<Product>, ValidationProblem>> CreateProduct(
        CreateProductRequest request,
        IProductRepository repository,
        IValidator<CreateProductRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(
                validationResult.ToDictionary());
        }
        
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Category = request.Category
        };
        
        await repository.AddAsync(product);
        
        return TypedResults.Created($"/api/products/{product.Id}", product);
    }
}
```

### Endpoint Filters

```csharp
// .NET 9: Improved endpoint filters
public class ValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        // Find validators in arguments
        foreach (var arg in context.Arguments)
        {
            if (arg is null) continue;
            
            var argType = arg.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argType);
            var validator = context.HttpContext.RequestServices
                .GetService(validatorType);
            
            if (validator is not null)
            {
                var validateMethod = validatorType
                    .GetMethod("ValidateAsync", [argType, typeof(CancellationToken)]);
                
                var result = await (Task<ValidationResult>)validateMethod!
                    .Invoke(validator, [arg, context.HttpContext.RequestAborted])!;
                
                if (!result.IsValid)
                {
                    return TypedResults.ValidationProblem(
                        result.ToDictionary());
                }
            }
        }
        
        return await next(context);
    }
}

// Logging filter
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
        var endpoint = context.HttpContext.GetEndpoint()?.DisplayName;
        _logger.LogInformation("Executing endpoint: {Endpoint}", endpoint);
        
        var stopwatch = Stopwatch.StartNew();
        var result = await next(context);
        stopwatch.Stop();
        
        _logger.LogInformation(
            "Endpoint {Endpoint} executed in {Duration}ms",
            endpoint,
            stopwatch.ElapsedMilliseconds);
        
        return result;
    }
}

// Usage
app.MapPost("/api/orders", CreateOrder)
    .AddEndpointFilter<ValidationFilter>()
    .AddEndpointFilter<LoggingFilter>();
```

### Parameter Binding Improvements

```csharp
// .NET 9: Enhanced parameter binding
public class OrdersApi
{
    // Bind from multiple sources
    public static IResult CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromHeader(Name = "X-User-Id")] string userId,
        [FromQuery] bool notify,
        IOrderService orderService,
        ILogger<OrdersApi> logger)
    {
        logger.LogInformation(
            "Creating order for user {UserId}, notify: {Notify}",
            userId, notify);
        
        var order = orderService.CreateOrder(request, userId);
        
        if (notify)
        {
            // Send notification
        }
        
        return TypedResults.Created($"/api/orders/{order.Id}", order);
    }
    
    // Complex type binding with validation
    public static async Task<IResult> SearchOrders(
        [AsParameters] OrderSearchParameters parameters,
        IOrderService orderService)
    {
        var orders = await orderService.SearchAsync(parameters);
        return TypedResults.Ok(orders);
    }
}

// .NET 9: AsParameters for cleaner signatures
public record OrderSearchParameters(
    [FromQuery] string? CustomerId,
    [FromQuery] OrderStatus? Status,
    [FromQuery] DateTime? FromDate,
    [FromQuery] DateTime? ToDate,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 20
);
```

### Route Groups and Organization

```csharp
// .NET 9: Better API organization
public static class ApiEndpoints
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api")
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .WithOpenApi();
        
        // Products group
        var products = api.MapGroup("/products")
            .WithTags("Products");
        
        products.MapGet("/", ProductHandlers.GetAll);
        products.MapGet("/{id}", ProductHandlers.GetById);
        products.MapPost("/", ProductHandlers.Create)
            .RequireAuthorization("admin");
        products.MapPut("/{id}", ProductHandlers.Update)
            .RequireAuthorization("admin");
        products.MapDelete("/{id}", ProductHandlers.Delete)
            .RequireAuthorization("admin");
        
        // Orders group
        var orders = api.MapGroup("/orders")
            .WithTags("Orders");
        
        orders.MapGet("/", OrderHandlers.GetAll);
        orders.MapGet("/{id}", OrderHandlers.GetById);
        orders.MapPost("/", OrderHandlers.Create);
        orders.MapPut("/{id}/status", OrderHandlers.UpdateStatus);
        
        // Public endpoints (no auth required)
        var publicApi = app.MapGroup("/api/public")
            .WithOpenApi();
        
        publicApi.MapGet("/health", () => Results.Ok("Healthy"));
        publicApi.MapGet("/version", () => Results.Ok(new { Version = "1.0.0" }));
    }
}
```

## Native AOT Support

### AOT-Compatible APIs

```csharp
// .NET 9: Native AOT compatible minimal API
var builder = WebApplication.CreateSlimBuilder(args);

// Configure services for AOT
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(
        0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

// Simple, AOT-friendly endpoints
app.MapGet("/", () => "Hello, AOT!");

app.MapGet("/api/users/{id:int}", (int id) => 
    new User { Id = id, Name = "John Doe" });

app.MapPost("/api/users", (User user) =>
{
    // Process user
    return Results.Created($"/api/users/{user.Id}", user);
});

await app.RunAsync();

// JSON source generation for AOT
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(List<User>))]
[JsonSerializable(typeof(ApiResponse<User>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}

public record User
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
```

### AOT Publishing

```xml
<!-- Project file configuration for Native AOT -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>true</InvariantGlobalization>
    <StripSymbols>true</StripSymbols>
    <IlcOptimizationPreference>Speed</IlcOptimizationPreference>
  </PropertyGroup>
</Project>
```

```bash
# Publish with Native AOT
dotnet publish -c Release

# Results:
# - Single executable (no .NET runtime needed)
# - ~15MB file size (vs ~70MB with regular publish)
# - <100ms startup time (vs ~500ms)
# - Lower memory usage (~25MB vs ~60MB)
```

### AOT Best Practices

```csharp
// AOT-compatible dependency injection
var builder = WebApplication.CreateSlimBuilder(args);

// ✅ Good: Direct registration
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// ✅ Good: Keyed services (AOT compatible)
builder.Services.AddKeyedSingleton<ICache, RedisCache>("redis");
builder.Services.AddKeyedSingleton<ICache, MemoryCache>("memory");

// ❌ Avoid: Dynamic assembly scanning
// builder.Services.AddControllers(); // Not AOT compatible

// ✅ Good: Explicit JSON context
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.AddContext<AppJsonSerializerContext>();
});

var app = builder.Build();

// ✅ Good: Type-safe route handlers
app.MapGet("/api/products", 
    ([FromServices] IProductService service) => service.GetAll());

// ❌ Avoid: Reflection-based features
// app.MapControllers(); // Not AOT compatible

await app.RunAsync();
```

## OpenAPI Document Generation

### Built-in OpenAPI Support

```csharp
// .NET 9: Built-in OpenAPI generation (no Swashbuckle needed)
var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI services
builder.Services.AddOpenApi();

var app = builder.Build();

// Map OpenAPI endpoint
app.MapOpenApi();

// Endpoints automatically included in OpenAPI
app.MapGet("/api/products", () => 
    new[] { new Product { Id = 1, Name = "Widget" } })
    .WithName("GetProducts")
    .WithDescription("Retrieves all products")
    .WithSummary("Get all products")
    .WithTags("Products");

app.Run();
```

### Advanced OpenAPI Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure OpenAPI
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new()
        {
            Title = "My API",
            Version = "v1",
            Description = "A sample API for demonstration",
            Contact = new()
            {
                Name = "API Support",
                Email = "support@example.com",
                Url = new Uri("https://example.com/support")
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
                Description = "Enter your JWT token"
            }
        };
        
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Serve OpenAPI at custom path
app.MapOpenApi("/openapi/{documentName}.json");

// Optional: Add Scalar UI (modern alternative to Swagger UI)
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}

app.Run();
```

### Endpoint Metadata for OpenAPI

```csharp
public class ProductsApi
{
    public static void MapEndpoints(WebApplication app)
    {
        var products = app.MapGroup("/api/products")
            .WithTags("Products")
            .WithDescription("Product management endpoints");
        
        products.MapGet("/", GetProducts)
            .WithName("GetProducts")
            .WithSummary("Get all products")
            .WithDescription("Retrieves a list of all available products")
            .Produces<List<Product>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);
        
        products.MapGet("/{id:int}", GetProductById)
            .WithName("GetProductById")
            .WithSummary("Get product by ID")
            .WithDescription("Retrieves a specific product by its unique identifier")
            .Produces<Product>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        
        products.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Create new product")
            .WithDescription("Creates a new product with the provided information")
            .Produces<Product>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .RequireAuthorization("admin");
    }
    
    static Task<List<Product>> GetProducts() => 
        Task.FromResult(new List<Product>());
    
    static Task<Results<Ok<Product>, NotFound>> GetProductById(int id) =>
        Task.FromResult<Results<Ok<Product>, NotFound>>(
            TypedResults.NotFound());
    
    static Task<Results<Created<Product>, ValidationProblem>> CreateProduct(
        CreateProductRequest request) =>
        Task.FromResult<Results<Created<Product>, ValidationProblem>>(
            TypedResults.ValidationProblem(new Dictionary<string, string[]>()));
}

// Custom OpenAPI operation filter
public class AddResponseHeadersOperationFilter : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        operation.Responses["200"].Headers = new Dictionary<string, OpenApiHeader>
        {
            ["X-Rate-Limit"] = new()
            {
                Description = "Number of requests remaining",
                Schema = new() { Type = "integer" }
            },
            ["X-Rate-Limit-Reset"] = new()
            {
                Description = "UTC timestamp when the rate limit resets",
                Schema = new() { Type = "string", Format = "date-time" }
            }
        };
        
        return Task.CompletedTask;
    }
}
```

## New Middleware Features

### Middleware Improvements

```csharp
// .NET 9: Enhanced middleware
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;
    
    public RequestTimingMiddleware(
        RequestDelegate next,
        ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = context.TraceIdentifier;
        
        _logger.LogInformation(
            "Request {RequestId} started: {Method} {Path}",
            requestId, context.Request.Method, context.Request.Path);
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Request {RequestId} completed in {Duration}ms with status {StatusCode}",
                requestId, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
        }
    }
}

// .NET 9: Conditional middleware
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseWhen(
        this IApplicationBuilder app,
        Func<HttpContext, bool> predicate,
        Action<IApplicationBuilder> configuration)
    {
        return app.Use(async (context, next) =>
        {
            if (predicate(context))
            {
                var builder = app.New();
                configuration(builder);
                builder.Build()(context);
            }
            
            await next();
        });
    }
}

// Usage
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api"),
    apiApp =>
    {
        apiApp.UseRateLimiter();
        apiApp.UseResponseCompression();
    });
```

### Rate Limiting

```csharp
// .NET 9: Enhanced rate limiting
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    // Fixed window limiter
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 10;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    
    // Sliding window limiter
    options.AddSlidingWindowLimiter("sliding", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6;
        opt.PermitLimit = 100;
    });
    
    // Token bucket limiter
    options.AddTokenBucketLimiter("token", opt =>
    {
        opt.TokenLimit = 1000;
        opt.TokensPerPeriod = 100;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
    });
    
    // Concurrency limiter
    options.AddConcurrencyLimiter("concurrency", opt =>
    {
        opt.PermitLimit = 50;
        opt.QueueLimit = 20;
    });
    
    // Custom policy based on user
    options.AddPolicy<string, UserRateLimitPolicy>("user");
    
    // Default rejection response
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = 
                retryAfter.TotalSeconds.ToString();
        }
        
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Rate limit exceeded. Please try again later." },
            cancellationToken);
    };
});

var app = builder.Build();

app.UseRateLimiter();

// Apply rate limiting to endpoints
app.MapGet("/api/public", () => "Public endpoint")
    .RequireRateLimiting("fixed");

app.MapGet("/api/expensive", () => "Expensive operation")
    .RequireRateLimiting("concurrency");

app.MapGet("/api/user/{id}", (string id) => $"User {id}")
    .RequireRateLimiting("user");

app.Run();

// Custom user-based rate limiting
public class UserRateLimitPolicy : IRateLimiterPolicy<string>
{
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; }
    
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst("sub")?.Value ?? "anonymous";
        
        return RateLimitPartition.GetTokenBucketLimiter(userId, _ =>
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = httpContext.User.Identity?.IsAuthenticated == true ? 1000 : 100,
                TokensPerPeriod = 50,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1)
            });
    }
}
```

### Output Caching

```csharp
// .NET 9: Output caching for APIs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOutputCache(options =>
{
    // Default policy
    options.AddBasePolicy(builder => builder
        .Expire(TimeSpan.FromMinutes(5))
        .Tag("default"));
    
    // Policy for frequently accessed data
    options.AddPolicy("frequent", builder => builder
        .Expire(TimeSpan.FromMinutes(1))
        .Tag("frequent"));
    
    // Policy with vary by query
    options.AddPolicy("varyByQuery", builder => builder
        .Expire(TimeSpan.FromMinutes(10))
        .SetVaryByQuery("page", "pageSize")
        .Tag("paged"));
    
    // Policy with vary by header
    options.AddPolicy("varyByAcceptLanguage", builder => builder
        .Expire(TimeSpan.FromMinutes(30))
        .SetVaryByHeader("Accept-Language")
        .Tag("localized"));
});

var app = builder.Build();

app.UseOutputCache();

// Cache endpoint responses
app.MapGet("/api/products", GetProducts)
    .CacheOutput("frequent");

app.MapGet("/api/products/search", SearchProducts)
    .CacheOutput("varyByQuery");

app.MapGet("/api/categories", GetCategories)
    .CacheOutput(builder => builder
        .Expire(TimeSpan.FromHours(1))
        .Tag("categories"));

// Invalidate cache
app.MapPost("/api/products", async (Product product, IOutputCacheStore cache) =>
{
    // Add product logic...
    
    // Invalidate cached responses
    await cache.EvictByTagAsync("frequent", CancellationToken.None);
    
    return Results.Created($"/api/products/{product.Id}", product);
});

app.Run();
```

## Blazor .NET 9 Updates

### Static SSR Improvements

```razor
@* .NET 9: Enhanced static server-side rendering *@
@page "/products"
@rendermode InteractiveServer

<PageTitle>Products</PageTitle>

<h1>Products</h1>

@if (products == null)
{
    <p>Loading...</p>
}
else
{
    <div class="product-grid">
        @foreach (var product in products)
        {
            <ProductCard Product="@product" OnAddToCart="HandleAddToCart" />
        }
    </div>
}

@code {
    private List<Product>? products;
    
    [Inject]
    private IProductService ProductService { get; set; } = default!;
    
    protected override async Task OnInitializedAsync()
    {
        products = await ProductService.GetAllAsync();
    }
    
    private void HandleAddToCart(Product product)
    {
        // Handle add to cart
    }
}
```

### Streaming Rendering

```razor
@* .NET 9: Streaming rendering for better perceived performance *@
@page "/dashboard"
@attribute [StreamRendering]

<h1>Dashboard</h1>

<div class="dashboard">
    <div class="widget">
        <h2>Sales</h2>
        @if (salesData != null)
        {
            <SalesChart Data="@salesData" />
        }
        else
        {
            <LoadingSpinner />
        }
    </div>
    
    <div class="widget">
        <h2>Recent Orders</h2>
        @if (recentOrders != null)
        {
            <OrdersList Orders="@recentOrders" />
        }
        else
        {
            <LoadingSpinner />
        }
    </div>
    
    <div class="widget">
        <h2>Inventory Status</h2>
        @if (inventoryData != null)
        {
            <InventoryChart Data="@inventoryData" />
        }
        else
        {
            <LoadingSpinner />
        }
    </div>
</div>

@code {
    private SalesData? salesData;
    private List<Order>? recentOrders;
    private InventoryData? inventoryData;
    
    protected override async Task OnInitializedAsync()
    {
        // Load data in parallel, render progressively
        var salesTask = LoadSalesDataAsync();
        var ordersTask = LoadRecentOrdersAsync();
        var inventoryTask = LoadInventoryDataAsync();
        
        await Task.WhenAll(salesTask, ordersTask, inventoryTask);
    }
    
    private async Task LoadSalesDataAsync()
    {
        await Task.Delay(500); // Simulate API call
        salesData = new SalesData(); // Load actual data
        StateHasChanged(); // Update UI incrementally
    }
    
    private async Task LoadRecentOrdersAsync()
    {
        await Task.Delay(300);
        recentOrders = new List<Order>();
        StateHasChanged();
    }
    
    private async Task LoadInventoryDataAsync()
    {
        await Task.Delay(700);
        inventoryData = new InventoryData();
        StateHasChanged();
    }
}
```

### Form Handling Improvements

```razor
@* .NET 9: Enhanced form handling *@
@page "/register"
@using Microsoft.AspNetCore.Components.Forms

<EditForm Model="@model" OnValidSubmit="HandleValidSubmit" FormName="register">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <div class="form-group">
        <label for="email">Email:</label>
        <InputText id="email" @bind-Value="model.Email" class="form-control" />
        <ValidationMessage For="@(() => model.Email)" />
    </div>
    
    <div class="form-group">
        <label for="password">Password:</label>
        <InputText type="password" id="password" @bind-Value="model.Password" class="form-control" />
        <ValidationMessage For="@(() => model.Password)" />
    </div>
    
    <div class="form-group">
        <label for="confirmPassword">Confirm Password:</label>
        <InputText type="password" id="confirmPassword" @bind-Value="model.ConfirmPassword" class="form-control" />
        <ValidationMessage For="@(() => model.ConfirmPassword)" />
    </div>
    
    <button type="submit" class="btn btn-primary">Register</button>
</EditForm>

@if (!string.IsNullOrEmpty(message))
{
    <div class="alert alert-info mt-3">@message</div>
}

@code {
    [SupplyParameterFromForm]
    private RegisterModel model { get; set; } = new();
    
    private string? message;
    
    private async Task HandleValidSubmit()
    {
        // Process registration
        message = "Registration successful!";
        await Task.CompletedTask;
    }
    
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
        
        [Required]
        [MinLength(8)]
        public string Password { get; set; } = "";
        
        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = "";
    }
}
```

## SignalR Improvements

### Performance Enhancements

```csharp
// .NET 9: SignalR with improved performance
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly IChatService _chatService;
    
    public ChatHub(ILogger<ChatHub> logger, IChatService chatService)
    {
        _logger = logger;
        _chatService = chatService;
    }
    
    // .NET 9: Better async streaming
    public async IAsyncEnumerable<ChatMessage> StreamMessages(
        string roomId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId, cancellationToken);
        
        await foreach (var message in _chatService.GetMessagesAsync(roomId, cancellationToken))
        {
            yield return message;
        }
    }
    
    // Improved error handling
    public async Task SendMessage(string roomId, string message)
    {
        try
        {
            var chatMessage = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                RoomId = roomId,
                UserId = Context.UserIdentifier ?? "anonymous",
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            
            await _chatService.SaveMessageAsync(chatMessage);
            
            // Broadcast to all clients in room
            await Clients.Group(roomId).SendAsync("ReceiveMessage", chatMessage);
            
            _logger.LogInformation(
                "Message {MessageId} sent to room {RoomId}",
                chatMessage.Id, roomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to room {RoomId}", roomId);
            throw;
        }
    }
    
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation(
            "Client {ConnectionId} connected",
            Context.ConnectionId);
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogError(exception,
                "Client {ConnectionId} disconnected with error",
                Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation(
                "Client {ConnectionId} disconnected",
                Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}

// Client setup
public class SignalRService
{
    private HubConnection? _connection;
    
    public async Task ConnectAsync(string hubUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect() // Automatic reconnection
            .AddMessagePackProtocol() // Better performance than JSON
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();
        
        _connection.On<ChatMessage>("ReceiveMessage", HandleMessage);
        
        await _connection.StartAsync();
    }
    
    public async Task SendMessageAsync(string roomId, string message)
    {
        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync("SendMessage", roomId, message);
        }
    }
    
    public async IAsyncEnumerable<ChatMessage> StreamMessagesAsync(string roomId)
    {
        if (_connection?.State == HubConnectionState.Connected)
        {
            await foreach (var message in _connection.StreamAsync<ChatMessage>(
                "StreamMessages", roomId))
            {
                yield return message;
            }
        }
    }
    
    private void HandleMessage(ChatMessage message)
    {
        Console.WriteLine($"[{message.Timestamp}] {message.UserId}: {message.Message}");
    }
}
```

### Hub Configuration

```csharp
// .NET 9: Enhanced SignalR configuration
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(options =>
{
    // .NET 9: Improved options
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 128 * 1024; // 128 KB
    options.StreamBufferCapacity = 10;
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.MaximumParallelInvocationsPerClient = 1;
})
.AddMessagePackProtocol() // Better performance
.AddStackExchangeRedis("localhost:6379", options =>
{
    options.Configuration.ChannelPrefix = "MyApp";
});

var app = builder.Build();

app.MapHub<ChatHub>("/chatHub");
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
```

## Best Practices

### 1. Minimal APIs
- Use typed results for better compile-time safety
- Organize endpoints with route groups
- Apply filters for cross-cutting concerns
- Use `AsParameters` for complex parameter binding
- Leverage endpoint metadata for OpenAPI generation

### 2. Native AOT
- Use `WebApplication.CreateSlimBuilder()` for AOT
- Implement JSON source generation for all types
- Avoid reflection-based features
- Test AOT compatibility early in development
- Monitor binary size and startup time

### 3. OpenAPI
- Use built-in OpenAPI over third-party libraries
- Add comprehensive metadata to endpoints
- Version your APIs properly
- Include authentication schemes in OpenAPI docs
- Use Scalar or similar modern UI for API testing

### 4. Performance
- Enable output caching for appropriate endpoints
- Configure rate limiting to protect resources
- Use streaming rendering in Blazor for better UX
- Implement connection multiplexing with HTTP/3
- Profile and optimize hot paths

### 5. SignalR
- Use MessagePack protocol for better performance
- Implement proper error handling and logging
- Configure appropriate timeouts
- Use Redis backplane for scale-out scenarios
- Monitor connection metrics

## Interview Questions

### Question 1: Typed Results Benefits
**Q:** What are the advantages of using typed results in ASP.NET Core 9 Minimal APIs?

**A:** Key advantages:
1. **Type safety**: Compiler catches return type mismatches
2. **IntelliSense**: Better IDE support and autocompletion
3. **OpenAPI**: Automatic, accurate API documentation
4. **Refactoring**: Safer code changes with compile-time checks
5. **Testing**: Easier to test specific result types

Example:
```csharp
// Type-safe with compile-time checking
static async Task<Results<Ok<Product>, NotFound>> GetProduct(int id)
{
    var product = await repository.GetByIdAsync(id);
    return product is not null 
        ? TypedResults.Ok(product)      // ✓ Matches return type
        : TypedResults.NotFound();       // ✓ Matches return type
    // return TypedResults.BadRequest(); // ✗ Compile error
}
```

Improves maintainability and reduces runtime errors.

### Question 2: Native AOT Tradeoffs
**Q:** What are the tradeoffs of using Native AOT in ASP.NET Core 9?

**A:** **Benefits**:
- **Startup**: <100ms vs ~500ms traditional (5x faster)
- **Memory**: ~25MB vs ~60MB (60% reduction)
- **Size**: ~15MB vs ~70MB deployment (78% smaller)
- **Cost**: Lower cloud infrastructure costs
- **Security**: Reduced attack surface

**Limitations**:
- **Reflection**: Limited reflection support
- **Dynamic code**: No runtime code generation
- **Compatibility**: Not all libraries support AOT
- **Build time**: Longer compilation times
- **Debugging**: More complex debugging

**Best for**: Serverless functions, containers, microservices with tight resource constraints

**Avoid for**: Applications requiring extensive reflection, dynamic plugin systems

### Question 3: OpenAPI Built-in vs Swashbuckle
**Q:** Why should you use ASP.NET Core 9's built-in OpenAPI instead of Swashbuckle?

**A:** Advantages of built-in OpenAPI:
1. **Performance**: Faster, no runtime reflection overhead
2. **AOT compatible**: Works with Native AOT compilation
3. **Maintenance**: Official Microsoft support, better integration
4. **Simpler**: Less configuration needed
5. **Modern**: Supports OpenAPI 3.1 specification
6. **Size**: Smaller deployment footprint

Comparison:
```csharp
// Built-in: Simple, fast
builder.Services.AddOpenApi();
app.MapOpenApi();

// Swashbuckle: More features, slower, not AOT compatible
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
app.UseSwagger();
app.UseSwaggerUI();
```

Use Swashbuckle only if you need specific customization not available in built-in OpenAPI.

### Question 4: Rate Limiting Policies
**Q:** When would you use each rate limiting algorithm in ASP.NET Core 9?

**A:** Algorithm selection guide:

**Fixed Window**: Simple rate limiting
- Use: Basic API throttling
- Example: 100 requests per minute
- Pros: Simple, predictable
- Cons: Burst at window boundaries

**Sliding Window**: Smoother rate limiting
- Use: APIs requiring consistent load
- Example: 100 requests per minute, 6 segments
- Pros: No boundary bursts
- Cons: More memory usage

**Token Bucket**: Burst handling
- Use: APIs with occasional spikes
- Example: 1000 token limit, +100 every 10s
- Pros: Allows bursts, flexible
- Cons: Complex configuration

**Concurrency**: Limit simultaneous operations
- Use: Resource-intensive operations
- Example: Max 50 concurrent requests
- Pros: Protects backend resources
- Cons: Doesn't limit request rate

Real-world example: Use token bucket for user-facing APIs (allows bursts), concurrency for database-heavy operations.

### Question 5: Blazor Streaming Rendering
**Q:** How does streaming rendering improve Blazor SSR performance in .NET 9?

**A:** Streaming rendering benefits:
1. **Perceived performance**: Shows content progressively
2. **TTFB**: Fast Time To First Byte
3. **Incremental updates**: Renders sections as data loads
4. **Better UX**: User sees content sooner

Example:
```razor
@attribute [StreamRendering]

@* Initial render shows immediately *@
<h1>Dashboard</h1>

@* These stream as data arrives *@
@if (data1 != null) { <Widget1 /> }
@if (data2 != null) { <Widget2 /> }
@if (data3 != null) { <Widget3 /> }
```

Timeline:
- 0ms: HTML with header sent
- 100ms: Widget1 data arrives, streamed to client
- 200ms: Widget2 data arrives, streamed to client
- 300ms: Widget3 data arrives, streamed to client

Without streaming: User waits 300ms before seeing anything.
With streaming: User sees header immediately, widgets appear progressively.

### Question 6: SignalR MessagePack
**Q:** Why use MessagePack protocol instead of JSON in SignalR?

**A:** MessagePack advantages:
1. **Size**: 30-50% smaller payload
2. **Speed**: 2-3x faster serialization
3. **Binary**: More efficient than text
4. **Performance**: Lower CPU usage

Benchmark comparison:
```
JSON Protocol:
- Serialize: 450ns
- Deserialize: 850ns
- Size: 1024 bytes

MessagePack Protocol:
- Serialize: 180ns (2.5x faster)
- Deserialize: 320ns (2.7x faster)
- Size: 512 bytes (50% smaller)
```

Setup:
```csharp
builder.Services.AddSignalR()
    .AddMessagePackProtocol();
```

Use for: High-frequency updates, real-time games, live dashboards, trading platforms

### Question 7: Output Caching Strategy
**Q:** Design an output caching strategy for an e-commerce API.

**A:** Strategy by endpoint type:

**Product Catalog** (changes rarely):
```csharp
app.MapGet("/api/products", GetProducts)
    .CacheOutput(builder => builder
        .Expire(TimeSpan.FromHours(1))
        .Tag("products"));
```

**Product Search** (varies by query):
```csharp
app.MapGet("/api/products/search", Search)
    .CacheOutput(builder => builder
        .Expire(TimeSpan.FromMinutes(10))
        .SetVaryByQuery("q", "category", "page")
        .Tag("search"));
```

**User Cart** (user-specific, short-lived):
```csharp
app.MapGet("/api/cart", GetCart)
    .CacheOutput(builder => builder
        .Expire(TimeSpan.FromMinutes(5))
        .SetVaryByHeader("Authorization")
        .Tag("cart", userId));
```

**Invalidation**:
```csharp
app.MapPost("/api/products", async (Product p, IOutputCacheStore cache) =>
{
    await SaveProduct(p);
    await cache.EvictByTagAsync("products", default);
    return TypedResults.Created(...);
});
```

### Question 8: Endpoint Filters vs Middleware
**Q:** When should you use endpoint filters instead of middleware in ASP.NET Core 9?

**A:** Use endpoint filters when:
- Logic applies to specific endpoints only
- Need access to endpoint metadata
- Want compile-time endpoint association
- Implementing cross-cutting concerns per-route

Use middleware when:
- Logic applies to all requests
- Need to short-circuit pipeline
- Handling low-level HTTP concerns
- Order matters globally

Example:
```csharp
// Endpoint filter: Specific to API endpoints
app.MapPost("/api/orders", CreateOrder)
    .AddEndpointFilter<ValidationFilter>();  // Only on this endpoint

// Middleware: Global authentication
app.UseAuthentication();  // Applies to all requests
app.UseAuthorization();
```

Filters are more granular and type-safe; middleware is more powerful and global.

### Question 9: AsParameters Pattern
**Q:** How does the `[AsParameters]` attribute improve API design?

**A:** Improvements:
1. **Cleaner signatures**: Reduces parameter count
2. **Reusability**: Share parameter objects across endpoints
3. **Validation**: Centralized validation logic
4. **Documentation**: Better OpenAPI schemas
5. **Testability**: Easier to mock and test

Before:
```csharp
app.MapGet("/api/products", (
    int page, 
    int pageSize, 
    string? category, 
    decimal? minPrice, 
    decimal? maxPrice, 
    string? sortBy) => 
{
    // 6 parameters!
});
```

After:
```csharp
app.MapGet("/api/products", 
    ([AsParameters] ProductSearchParams p) =>
{
    // Clean, reusable
});

record ProductSearchParams(
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 20,
    [FromQuery] string? Category,
    [FromQuery] decimal? MinPrice,
    [FromQuery] decimal? MaxPrice,
    [FromQuery] string? SortBy
);
```

Benefits: Better IntelliSense, easier refactoring, shared validation, cleaner code.

### Question 10: WebApplication.CreateSlimBuilder
**Q:** What's the difference between `CreateBuilder` and `CreateSlimBuilder`?

**A:** Comparison:

**CreateBuilder** (standard):
- Includes all standard services
- Supports MVC, Razor Pages, etc.
- ~70MB deployment
- ~500ms startup
- Full feature set

**CreateSlimBuilder** (AOT-optimized):
- Minimal services only
- Optimized for Minimal APIs
- ~15MB deployment
- ~100ms startup
- AOT compatible

Services comparison:
```csharp
// CreateBuilder includes:
// - Logging, Configuration
// - IHttpContextAccessor
// - Routing, Endpoints
// - JSON serialization
// - Data protection
// - CORS, Authentication (if added)

// CreateSlimBuilder includes:
// - Minimal logging
// - Essential configuration
// - Basic routing only
// - Requires explicit JSON context
```

Use `CreateSlimBuilder` for: Microservices, serverless functions, cloud-native APIs with tight resource constraints.

Use `CreateBuilder` for: Full-featured applications, when AOT isn't required.

---

**Last Updated: January 2026 - .NET 9**
