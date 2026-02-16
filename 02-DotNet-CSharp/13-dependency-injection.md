# Dependency Injection in .NET - Complete Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Core Concepts](#core-concepts)
3. [Service Lifetimes](#service-lifetimes)
4. [Registration Patterns](#registration-patterns)
5. [Advanced Scenarios](#advanced-scenarios)
6. [Best Practices](#best-practices)
7. [Real-World Examples](#real-world-examples)
8. [Interview Questions](#interview-questions)

## Introduction

Dependency Injection (DI) is a design pattern and a core feature of ASP.NET Core that implements Inversion of Control (IoC) for resolving dependencies. It promotes loose coupling and improves testability, maintainability, and flexibility.

### Why Dependency Injection?

```csharp
// ❌ WITHOUT DI: Tight coupling, hard to test
public class OrderService
{
    private readonly SqlOrderRepository _repository;
    private readonly SmtpEmailService _emailService;
    
    public OrderService()
    {
        // Creating concrete dependencies
        _repository = new SqlOrderRepository("connection_string");
        _emailService = new SmtpEmailService("smtp.gmail.com");
    }
    
    // Cannot test without real database and SMTP server
    public async Task CreateOrder(Order order)
    {
        await _repository.SaveAsync(order);
        await _emailService.SendConfirmationAsync(order);
    }
}

// ✅ WITH DI: Loose coupling, easy to test
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IEmailService _emailService;
    
    // Dependencies injected via constructor
    public OrderService(IOrderRepository repository, IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }
    
    // Easy to test with mock implementations
    public async Task CreateOrder(Order order)
    {
        await _repository.SaveAsync(order);
        await _emailService.SendConfirmationAsync(order);
    }
}
```

### Benefits

1. **Testability**: Easy to inject mocks and fakes
2. **Loose Coupling**: Depend on abstractions, not implementations
3. **Flexibility**: Swap implementations without changing code
4. **Maintainability**: Centralized configuration
5. **Separation of Concerns**: Business logic separate from object creation

## Core Concepts

### Service Container (IServiceProvider)

The service container is responsible for creating and managing object instances and their dependencies.

```csharp
// Program.cs - Service Registration
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

// Service Resolution happens automatically
app.MapGet("/orders/{id}", async (int id, OrderService orderService) =>
{
    // OrderService is resolved with all its dependencies
    return await orderService.GetOrderAsync(id);
});
```

### Constructor Injection (Recommended)

```csharp
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderService> _logger;
    private readonly IOptions<OrderSettings> _settings;
    
    // All dependencies injected via constructor
    public OrderService(
        IOrderRepository repository,
        IEmailService emailService,
        ILogger<OrderService> logger,
        IOptions<OrderSettings> settings)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }
}
```

### Method Injection

```csharp
// Useful for dependencies only needed in specific methods
public class ReportGenerator
{
    // FromServices attribute gets dependency from DI container
    public IActionResult GenerateReport([FromServices] IReportService reportService)
    {
        var report = reportService.Generate();
        return Ok(report);
    }
}
```

### Property Injection (Not Recommended)

```csharp
// Avoid property injection - makes dependencies less explicit
public class OrderService
{
    // Property injection - not recommended
    [Inject]
    public IOrderRepository Repository { get; set; }
}
```

## Service Lifetimes

### Transient

**Created each time they're requested**. Best for lightweight, stateless services.

```csharp
// Register
builder.Services.AddTransient<IEmailService, EmailService>();

// Each request gets a new instance
public class OrderController : ControllerBase
{
    private readonly IEmailService _email1;
    private readonly IEmailService _email2;
    
    public OrderController(IEmailService email1, IEmailService email2)
    {
        // email1 and email2 are DIFFERENT instances
        _email1 = email1;
        _email2 = email2;
    }
}
```

**When to use:**
- Lightweight services
- Stateless services
- Services that are not thread-safe

```csharp
// Good for transient
public class EmailValidator
{
    public bool IsValid(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}

builder.Services.AddTransient<EmailValidator>();
```

### Scoped

**Created once per request** (HTTP request in web applications). Reused within the same request.

```csharp
// Register
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, ApplicationDbContext>();

// Within the same HTTP request, same instance is used
public class OrderService
{
    private readonly IOrderRepository _repository;
    
    public OrderService(IOrderRepository repository)
    {
        // Same instance as in OrderController for this request
        _repository = repository;
    }
}

public class OrderController : ControllerBase
{
    private readonly IOrderRepository _repository;
    private readonly OrderService _orderService;
    
    public OrderController(IOrderRepository repository, OrderService orderService)
    {
        // Both get the SAME repository instance for this HTTP request
        _repository = repository;
        _orderService = orderService;
    }
}
```

**When to use:**
- Database contexts (DbContext)
- Repository pattern implementations
- Unit of Work pattern
- Request-specific state

```csharp
// Good for scoped
public class ApplicationDbContext : DbContext, IUnitOfWork
{
    // Shared within single request
}

builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
```

### Singleton

**Created once** and reused for the lifetime of the application.

```csharp
// Register
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// All requests share the SAME instance
public class CacheService
{
    private readonly ICacheService _cache;
    
    public CacheService(ICacheService cache)
    {
        // Same cache instance across all requests
        _cache = cache;
    }
}
```

**When to use:**
- Configuration objects
- Logging services
- Caching services
- Thread-safe services
- Expensive to create objects

```csharp
// Good for singleton
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    
    public MemoryCacheService()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
    }
    
    // Thread-safe operations
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        return _memoryCache.Get<T>(key);
    }
}

builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
```

### Lifetime Best Practices

```csharp
// ✅ GOOD: Longer lifetime depending on shorter lifetime
public class SingletonService
{
    private readonly ILogger<SingletonService> _logger; // Singleton
    
    public SingletonService(ILogger<SingletonService> logger)
    {
        _logger = logger;
    }
}

// ❌ BAD: Shorter lifetime depending on longer lifetime
public class TransientService
{
    // DON'T inject scoped/singleton into transient if they hold state
    private readonly ApplicationDbContext _context; // Scoped in Transient!
    
    public TransientService(ApplicationDbContext context)
    {
        _context = context; // May cause memory leaks!
    }
}

// ❌ VERY BAD: Singleton depending on scoped
public class SingletonCache
{
    // This will cause a runtime error or captive dependency
    private readonly ApplicationDbContext _context; // Scoped in Singleton!
    
    public SingletonCache(ApplicationDbContext context)
    {
        _context = context; // ERROR: Cannot consume scoped service from singleton
    }
}
```

## Registration Patterns

### Basic Registration

```csharp
// Interface to implementation
builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();

// Concrete class
builder.Services.AddScoped<OrderService>();

// Factory method
builder.Services.AddScoped<IEmailService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var smtpHost = config["Email:SmtpHost"];
    return new EmailService(smtpHost);
});

// Instance registration (singleton only)
builder.Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
```

### Multiple Implementations

```csharp
// Register multiple implementations
builder.Services.AddTransient<INotificationService, EmailNotification>();
builder.Services.AddTransient<INotificationService, SmsNotification>();
builder.Services.AddTransient<INotificationService, PushNotification>();

// Resolve all implementations
public class NotificationDispatcher
{
    private readonly IEnumerable<INotificationService> _notificationServices;
    
    public NotificationDispatcher(IEnumerable<INotificationService> notificationServices)
    {
        _notificationServices = notificationServices;
    }
    
    public async Task NotifyAllAsync(string message)
    {
        foreach (var service in _notificationServices)
        {
            await service.SendAsync(message);
        }
    }
}
```

### Named Services (Using Keyed Services in .NET 8+)

```csharp
// .NET 8+ Keyed Services
builder.Services.AddKeyedScoped<IPaymentService, StripePaymentService>("stripe");
builder.Services.AddKeyedScoped<IPaymentService, PayPalPaymentService>("paypal");
builder.Services.AddKeyedScoped<IPaymentService, SquarePaymentService>("square");

// Inject specific implementation
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _stripeService;
    private readonly IPaymentService _paypalService;
    
    public PaymentController(
        [FromKeyedServices("stripe")] IPaymentService stripeService,
        [FromKeyedServices("paypal")] IPaymentService paypalService)
    {
        _stripeService = stripeService;
        _paypalService = paypalService;
    }
}

// Or use factory pattern for pre-.NET 8
public interface IPaymentServiceFactory
{
    IPaymentService GetService(string provider);
}

public class PaymentServiceFactory : IPaymentServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public PaymentServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IPaymentService GetService(string provider)
    {
        return provider.ToLower() switch
        {
            "stripe" => _serviceProvider.GetRequiredService<StripePaymentService>(),
            "paypal" => _serviceProvider.GetRequiredService<PayPalPaymentService>(),
            "square" => _serviceProvider.GetRequiredService<SquarePaymentService>(),
            _ => throw new ArgumentException($"Unknown payment provider: {provider}")
        };
    }
}
```

### Generic Services

```csharp
// Open generic registration
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
}

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    
    public Repository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }
    
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }
}

// Usage - automatically resolves to Repository<Product>
public class ProductService
{
    private readonly IRepository<Product> _repository;
    
    public ProductService(IRepository<Product> repository)
    {
        _repository = repository;
    }
}
```

### Conditional Registration

```csharp
// Register based on configuration
var emailProvider = builder.Configuration["EmailProvider"];

if (emailProvider == "SendGrid")
{
    builder.Services.AddScoped<IEmailService, SendGridEmailService>();
}
else if (emailProvider == "SMTP")
{
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, MockEmailService>();
}

// Register based on environment
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IPaymentService, MockPaymentService>();
}
else
{
    builder.Services.AddScoped<IPaymentService, StripePaymentService>();
}
```

### TryAdd Methods

```csharp
// Only registers if not already registered
builder.Services.TryAddScoped<IOrderRepository, SqlOrderRepository>();
builder.Services.TryAddScoped<IOrderRepository, CosmosOrderRepository>(); // Won't register

// Replace existing registration
builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
builder.Services.Replace(ServiceDescriptor.Scoped<IOrderRepository, CosmosOrderRepository>());

// Remove registration
builder.Services.RemoveAll<IOrderRepository>();
```

## Advanced Scenarios

### Options Pattern

```csharp
// Configuration class
public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
}

// appsettings.json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "user@example.com",
    "Password": "password",
    "FromAddress": "noreply@example.com"
  }
}

// Registration
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Inject IOptions<T>
public class EmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;
    
    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }
    
    public async Task SendAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort);
        // Use settings...
    }
}

// Validate options on startup
builder.Services.AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection("EmailSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

public class EmailSettings
{
    [Required]
    public string SmtpHost { get; set; } = string.Empty;
    
    [Range(1, 65535)]
    public int SmtpPort { get; set; }
    
    [Required]
    [EmailAddress]
    public string FromAddress { get; set; } = string.Empty;
}
```

### Service Locator Pattern (Anti-Pattern, Avoid)

```csharp
// ❌ BAD: Service Locator - makes dependencies implicit
public class OrderService
{
    private readonly IServiceProvider _serviceProvider;
    
    public OrderService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task ProcessOrder(Order order)
    {
        // Hidden dependencies!
        var repository = _serviceProvider.GetRequiredService<IOrderRepository>();
        var emailService = _serviceProvider.GetRequiredService<IEmailService>();
        
        await repository.SaveAsync(order);
        await emailService.SendConfirmationAsync(order);
    }
}

// ✅ GOOD: Explicit dependencies via constructor
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IEmailService _emailService;
    
    public OrderService(IOrderRepository repository, IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }
    
    public async Task ProcessOrder(Order order)
    {
        await _repository.SaveAsync(order);
        await _emailService.SendConfirmationAsync(order);
    }
}
```

### Decorators

```csharp
// Base service
public interface IOrderService
{
    Task<Result> CreateOrderAsync(Order order);
}

public class OrderService : IOrderService
{
    public async Task<Result> CreateOrderAsync(Order order)
    {
        // Core logic
        return Result.Success();
    }
}

// Logging decorator
public class LoggingOrderService : IOrderService
{
    private readonly IOrderService _innerService;
    private readonly ILogger<LoggingOrderService> _logger;
    
    public LoggingOrderService(IOrderService innerService, ILogger<LoggingOrderService> logger)
    {
        _innerService = innerService;
        _logger = logger;
    }
    
    public async Task<Result> CreateOrderAsync(Order order)
    {
        _logger.LogInformation("Creating order {OrderId}", order.Id);
        
        var result = await _innerService.CreateOrderAsync(order);
        
        if (result.IsSuccess)
            _logger.LogInformation("Order {OrderId} created successfully", order.Id);
        else
            _logger.LogError("Failed to create order {OrderId}: {Error}", order.Id, result.Error);
        
        return result;
    }
}

// Caching decorator
public class CachingOrderService : IOrderService
{
    private readonly IOrderService _innerService;
    private readonly ICacheService _cacheService;
    
    public CachingOrderService(IOrderService innerService, ICacheService cacheService)
    {
        _innerService = innerService;
        _cacheService = cacheService;
    }
    
    public async Task<Result> CreateOrderAsync(Order order)
    {
        var result = await _innerService.CreateOrderAsync(order);
        
        if (result.IsSuccess)
        {
            await _cacheService.SetAsync($"order:{order.Id}", order, TimeSpan.FromMinutes(10));
        }
        
        return result;
    }
}

// Registration with decorators
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IOrderService>(sp =>
{
    var orderService = sp.GetRequiredService<OrderService>();
    var logger = sp.GetRequiredService<ILogger<LoggingOrderService>>();
    var cacheService = sp.GetRequiredService<ICacheService>();
    
    // Wrap with logging, then caching
    var loggingService = new LoggingOrderService(orderService, logger);
    return new CachingOrderService(loggingService, cacheService);
});
```

### Scope Creation

```csharp
// Creating a scope manually
public class BackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    
    public BackgroundWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Create a scope for scoped services
            using (var scope = _serviceProvider.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                await orderService.ProcessPendingOrdersAsync();
            }
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

## Best Practices

### 1. Prefer Constructor Injection

```csharp
// ✅ GOOD: Dependencies are explicit and required
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IEmailService _emailService;
    
    public OrderService(IOrderRepository repository, IEmailService emailService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }
}
```

### 2. Follow Dependency Inversion Principle

```csharp
// ✅ Depend on abstractions
public class OrderService
{
    private readonly IOrderRepository _repository; // Interface
    
    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }
}

// ❌ Don't depend on concrete implementations
public class OrderService
{
    private readonly SqlOrderRepository _repository; // Concrete class
    
    public OrderService(SqlOrderRepository repository)
    {
        _repository = repository;
    }
}
```

### 3. Use Appropriate Lifetimes

```csharp
// ✅ DbContext should be scoped
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ✅ Stateless services can be transient
builder.Services.AddTransient<IEmailValidator, EmailValidator>();

// ✅ Configuration and caching should be singleton
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
```

### 4. Avoid Service Locator

```csharp
// ❌ BAD
public class OrderService
{
    private readonly IServiceProvider _serviceProvider;
    
    public OrderService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void DoSomething()
    {
        var service = _serviceProvider.GetService<ISomeService>();
    }
}

// ✅ GOOD
public class OrderService
{
    private readonly ISomeService _service;
    
    public OrderService(ISomeService service)
    {
        _service = service;
    }
}
```

### 5. Keep Constructors Simple

```csharp
// ✅ GOOD: Constructor only assigns dependencies
public class OrderService
{
    private readonly IOrderRepository _repository;
    
    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }
}

// ❌ BAD: Constructor does work
public class OrderService
{
    private readonly IOrderRepository _repository;
    
    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
        _repository.Initialize(); // Don't do work in constructor!
        LoadConfiguration(); // Don't do this!
    }
}
```

### 6. Extension Methods for Organization

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        return services;
    }
    
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        
        return services;
    }
}

// Usage
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
```

## Real-World Examples

### Complete E-Commerce Service Layer

```csharp
// Interfaces
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
}

public interface IEmailService
{
    Task SendOrderConfirmationAsync(Order order);
}

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount);
}

public interface IInventoryService
{
    Task<bool> ReserveItemsAsync(Guid orderId);
    Task ReleaseItemsAsync(Guid orderId);
}

// Main service with multiple dependencies
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEmailService _emailService;
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<OrderService> _logger;
    private readonly IOptions<OrderSettings> _settings;
    
    public OrderService(
        IOrderRepository orderRepository,
        IEmailService emailService,
        IPaymentService paymentService,
        IInventoryService inventoryService,
        ILogger<OrderService> logger,
        IOptions<OrderSettings> settings)
    {
        _orderRepository = orderRepository;
        _emailService = emailService;
        _paymentService = paymentService;
        _inventoryService = inventoryService;
        _logger = logger;
        _settings = settings;
    }
    
    public async Task<Result<Order>> CreateAndProcessOrderAsync(CreateOrderDto dto)
    {
        var order = new Order(dto.CustomerId, dto.Items);
        
        try
        {
            // Reserve inventory
            var inventoryReserved = await _inventoryService.ReserveItemsAsync(order.Id);
            if (!inventoryReserved)
                return Result<Order>.Failure("Insufficient inventory");
            
            // Process payment
            var paymentResult = await _paymentService.ProcessPaymentAsync(order.Id, order.Total);
            if (!paymentResult.IsSuccess)
            {
                await _inventoryService.ReleaseItemsAsync(order.Id);
                return Result<Order>.Failure("Payment failed");
            }
            
            // Save order
            await _orderRepository.AddAsync(order);
            
            // Send confirmation
            await _emailService.SendOrderConfirmationAsync(order);
            
            _logger.LogInformation("Order {OrderId} created and processed successfully", order.Id);
            return Result<Order>.Success(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order");
            await _inventoryService.ReleaseItemsAsync(order.Id);
            throw;
        }
    }
}

// Registration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderManagement(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options
        services.Configure<OrderSettings>(
            configuration.GetSection("OrderSettings"));
        
        // Services
        services.AddScoped<OrderService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<IInventoryService, InventoryService>();
        
        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        return services;
    }
}
```

## Interview Questions

### Conceptual Questions

1. **What is Dependency Injection and why is it important?**
   - DI is a design pattern for implementing IoC
   - Promotes loose coupling and testability
   - Dependencies are provided externally rather than created internally

2. **Explain the difference between service lifetimes**
   - **Transient**: New instance every time
   - **Scoped**: New instance per request
   - **Singleton**: Single instance for application lifetime

3. **What is the difference between DI and Service Locator?**
   - DI: Dependencies explicitly declared in constructor
   - Service Locator: Dependencies resolved from container, hidden
   - DI is preferred as it makes dependencies explicit

4. **What is a captive dependency?**
   - When a longer-lived service (singleton) holds a shorter-lived service (scoped)
   - Can cause memory leaks and state issues
   - Example: Singleton holding a reference to scoped DbContext

### Practical Questions

5. **When would you use each service lifetime?**
   ```csharp
   // Transient: Lightweight, stateless
   builder.Services.AddTransient<IValidator, OrderValidator>();
   
   // Scoped: Per-request state, DbContext
   builder.Services.AddScoped<ApplicationDbContext>();
   
   // Singleton: Application-wide, expensive to create
   builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
   ```

6. **How do you inject multiple implementations?**
   ```csharp
   // Register all
   builder.Services.AddTransient<INotification, EmailNotification>();
   builder.Services.AddTransient<INotification, SmsNotification>();
   
   // Inject IEnumerable
   public class NotificationService
   {
       private readonly IEnumerable<INotification> _notifications;
       
       public NotificationService(IEnumerable<INotification> notifications)
       {
           _notifications = notifications;
       }
   }
   ```

7. **How do you test code that uses DI?**
   ```csharp
   [Fact]
   public async Task CreateOrder_Should_Save_And_Send_Email()
   {
       // Arrange
       var mockRepository = new Mock<IOrderRepository>();
       var mockEmailService = new Mock<IEmailService>();
       
       var service = new OrderService(
           mockRepository.Object,
           mockEmailService.Object);
       
       // Act
       await service.CreateOrderAsync(new Order());
       
       // Assert
       mockRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
       mockEmailService.Verify(e => e.SendAsync(It.IsAny<Order>()), Times.Once);
   }
   ```

8. **What's wrong with this code?**
   ```csharp
   public class OrderService
   {
       // ❌ Static field defeats DI
       private static readonly IOrderRepository _repository = new SqlRepository();
   }
   
   // Should be:
   public class OrderService
   {
       private readonly IOrderRepository _repository;
       
       public OrderService(IOrderRepository repository)
       {
           _repository = repository;
       }
   }
   ```

### Advanced Questions

9. **How would you implement a plugin system with DI?**
   ```csharp
   // Plugin interface
   public interface IPlugin
   {
       string Name { get; }
       Task ExecuteAsync();
   }
   
   // Auto-register all plugins
   var pluginTypes = Assembly.GetExecutingAssembly()
       .GetTypes()
       .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract);
   
   foreach (var type in pluginTypes)
   {
       builder.Services.AddTransient(typeof(IPlugin), type);
   }
   
   // Use all plugins
   public class PluginExecutor
   {
       private readonly IEnumerable<IPlugin> _plugins;
       
       public PluginExecutor(IEnumerable<IPlugin> plugins)
       {
           _plugins = plugins;
       }
   }
   ```

10. **How do you handle conditional service registration based on configuration?**
    ```csharp
    var storageType = builder.Configuration["StorageType"];
    
    switch (storageType)
    {
        case "SqlServer":
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
            break;
        
        case "CosmosDb":
            builder.Services.AddSingleton<CosmosClient>(sp =>
                new CosmosClient(cosmosConnectionString));
            builder.Services.AddScoped<IOrderRepository, CosmosOrderRepository>();
            break;
        
        default:
            throw new InvalidOperationException($"Unknown storage type: {storageType}");
    }
    ```

## See Also

- [SOLID Principles](./12-solid-principles.md)
- [Clean Architecture](./11-clean-architecture.md)
- [Testing Strategies](./10-testing-dotnet-9.md)
- [ASP.NET Core Middleware](./04-aspnet-core-9.md)

## Additional Resources

- [Microsoft Docs: Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Dependency Injection Principles, Practices, and Patterns](https://www.manning.com/books/dependency-injection-principles-practices-patterns)
- [.NET Dependency Injection Best Practices](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)
