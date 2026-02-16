# ASP.NET Core Web API - Best Practices & Production Patterns

## Table of Contents
1. [Introduction](#introduction)
2. [RESTful API Design](#restful-api-design)
3. [API Versioning](#api-versioning)
4. [Model Validation & Error Handling](#model-validation--error-handling)
5. [Response Caching](#response-caching)
6. [Rate Limiting & Throttling](#rate-limiting--throttling)
7. [API Documentation](#api-documentation)
8. [Security Best Practices](#security-best-practices)
9. [Performance Optimization](#performance-optimization)
10. [Interview Questions](#interview-questions)

## Introduction

This guide covers production-ready patterns and best practices for building enterprise-scale Web APIs with ASP.NET Core 9. These patterns are based on years of experience building scalable, secure, and maintainable APIs.

## RESTful API Design

### HTTP Methods & Status Codes

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
    
    // GET api/products - List all products
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var products = await _productService.GetProductsAsync(page, pageSize, search);
        return Ok(products);
    }
    
    // GET api/products/{id} - Get single product
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found" });
        
        return Ok(product);
    }
    
    // POST api/products - Create new product
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        var result = await _productService.CreateAsync(dto);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            result.Value);
    }
    
    // PUT api/products/{id} - Update existing product
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var result = await _productService.UpdateAsync(id, dto);
        
        if (result.IsFailure)
        {
            return result.Error.Contains("not found")
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }
        
        return NoContent();
    }
    
    // PATCH api/products/{id} - Partial update
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PartialUpdate(
        Guid id,
        [FromBody] JsonPatchDocument<UpdateProductDto> patchDoc)
    {
        var product = await _productService.GetByIdAsync(id);
        
        if (product == null)
            return NotFound();
        
        var productToPatch = _mapper.Map<UpdateProductDto>(product);
        patchDoc.ApplyTo(productToPatch, ModelState);
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        await _productService.UpdateAsync(id, productToPatch);
        return NoContent();
    }
    
    // DELETE api/products/{id} - Delete product
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteAsync(id);
        
        if (result.IsFailure)
            return NotFound(new { error = result.Error });
        
        return NoContent();
    }
}
```

### Resource Naming Conventions

```csharp
// ✅ GOOD: Use plural nouns for collections
[Route("api/products")]
[Route("api/orders")]
[Route("api/customers")]

// ❌ BAD: Don't use verbs
[Route("api/getProducts")]
[Route("api/createOrder")]

// ✅ GOOD: Nested resources for relationships
[HttpGet("customers/{customerId}/orders")]
[HttpGet("orders/{orderId}/items")]

// ✅ GOOD: Use lowercase with hyphens for multi-word resources
[Route("api/product-categories")]
[Route("api/shipping-addresses")]

// ❌ BAD: Don't use camelCase or PascalCase in URLs
[Route("api/ProductCategories")]
[Route("api/shippingAddresses")]
```

### Query Parameters for Filtering, Sorting, Paging

```csharp
public class ProductQueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;
    
    public int Page { get; set; } = 1;
    
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
    
    public string? Search { get; set; }
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; } = "name";
    public string SortOrder { get; set; } = "asc";
}

[HttpGet]
public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
    [FromQuery] ProductQueryParameters parameters)
{
    var products = await _productService.GetProductsAsync(parameters);
    
    // Add pagination metadata to headers
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(new
    {
        totalCount = products.TotalCount,
        pageSize = products.PageSize,
        currentPage = products.CurrentPage,
        totalPages = products.TotalPages,
        hasPrevious = products.HasPrevious,
        hasNext = products.HasNext
    }));
    
    return Ok(products);
}

// Example usage: GET api/products?page=1&pageSize=20&search=laptop&minPrice=500&sortBy=price&sortOrder=desc
```

### HATEOAS (Hypermedia as the Engine of Application State)

```csharp
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<LinkDto> Links { get; set; } = new();
}

public class LinkDto
{
    public string Href { get; set; } = string.Empty;
    public string Rel { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
}

public class ProductService
{
    public ProductDto CreateProductWithLinks(Product product, IUrlHelper urlHelper)
    {
        var dto = _mapper.Map<ProductDto>(product);
        
        // Self link
        dto.Links.Add(new LinkDto
        {
            Href = urlHelper.Link("GetProduct", new { id = product.Id })!,
            Rel = "self",
            Method = "GET"
        });
        
        // Update link
        dto.Links.Add(new LinkDto
        {
            Href = urlHelper.Link("UpdateProduct", new { id = product.Id })!,
            Rel = "update",
            Method = "PUT"
        });
        
        // Delete link
        dto.Links.Add(new LinkDto
        {
            Href = urlHelper.Link("DeleteProduct", new { id = product.Id })!,
            Rel = "delete",
            Method = "DELETE"
        });
        
        return dto;
    }
}
```

## API Versioning

### URL Versioning

```csharp
// Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// V1 Controller
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ProductsV1Controller : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<ProductV1Dto>> GetAll()
    {
        // V1 implementation
        return Ok(products);
    }
}

// V2 Controller with breaking changes
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
public class ProductsV2Controller : ControllerBase
{
    [HttpGet]
    public ActionResult<PagedResult<ProductV2Dto>> GetAll(
        [FromQuery] ProductQueryParameters parameters)
    {
        // V2 implementation with pagination
        return Ok(pagedProducts);
    }
}

// Usage:
// GET /api/v1/products
// GET /api/v2/products
```

### Header Versioning

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new HeaderApiVersionReader("X-API-Version");
});

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public ActionResult<IEnumerable<ProductV1Dto>> GetAllV1()
    {
        return Ok(productsV1);
    }
    
    [HttpGet]
    [MapToApiVersion("2.0")]
    public ActionResult<PagedResult<ProductV2Dto>> GetAllV2()
    {
        return Ok(productsV2);
    }
}

// Usage: GET /api/products with header X-API-Version: 1.0
```

### Deprecating API Versions

```csharp
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    [Obsolete("This version is deprecated. Please use v2.0")]
    public ActionResult<IEnumerable<ProductDto>> GetAllV1()
    {
        Response.Headers.Append("X-API-Deprecated", "true");
        Response.Headers.Append("X-API-Sunset-Date", "2024-12-31");
        Response.Headers.Append("X-API-Migration-Guide", "https://api.example.com/docs/v1-to-v2");
        
        return Ok(products);
    }
}
```

## Model Validation & Error Handling

### Data Annotations

```csharp
public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    public int Stock { get; set; }
    
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string ContactEmail { get; set; } = string.Empty;
    
    [Url(ErrorMessage = "Invalid URL")]
    public string? WebsiteUrl { get; set; }
    
    [Required]
    public Guid CategoryId { get; set; }
    
    [MinLength(1, ErrorMessage = "At least one tag is required")]
    public List<string> Tags { get; set; } = new();
}
```

### FluentValidation

```csharp
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    private readonly IProductRepository _productRepository;
    
    public CreateProductDtoValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository;
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .Length(3, 200).WithMessage("Name must be between 3 and 200 characters")
            .MustAsync(BeUniqueName).WithMessage("Product name already exists");
        
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThan(1000000).WithMessage("Price seems unrealistic");
        
        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");
        
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .MustAsync(CategoryExists).WithMessage("Category does not exist");
        
        RuleFor(x => x.Tags)
            .NotEmpty().WithMessage("At least one tag is required")
            .Must(tags => tags.Count <= 10).WithMessage("Maximum 10 tags allowed");
    }
    
    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        var existingProduct = await _productRepository.GetByNameAsync(name, cancellationToken);
        return existingProduct == null;
    }
    
    private async Task<bool> CategoryExists(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        return category != null;
    }
}

// Registration
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
```

### Global Exception Handler

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }
    
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);
        
        var (statusCode, title, detail) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                GetValidationErrors(validationEx)
            ),
            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                notFoundEx.Message
            ),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "You are not authorized to access this resource"
            ),
            ForbiddenException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "You don't have permission to access this resource"
            ),
            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                "Conflict",
                conflictEx.Message
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred"
            )
        };
        
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        
        // Add trace ID for debugging
        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        
        // Add timestamp
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        
        return true;
    }
    
    private static string GetValidationErrors(ValidationException exception)
    {
        var errors = exception.Errors
            .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
            .ToList();
        
        return string.Join("; ", errors);
    }
}

// Registration
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
```

### Custom Validation Filter

```csharp
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            
            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "One or more validation errors occurred"
            };
            
            context.Result = new BadRequestObjectResult(problemDetails);
            return;
        }
        
        await next();
    }
}

// Registration
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});
```

## Response Caching

### Output Caching (.NET 9)

```csharp
// Program.cs
builder.Services.AddOutputCache(options =>
{
    // Default policy
    options.AddBasePolicy(builder => builder
        .Expire(TimeSpan.FromMinutes(10)));
    
    // Named policy for products
    options.AddPolicy("Products", builder => builder
        .Expire(TimeSpan.FromMinutes(5))
        .SetVaryByQuery("page", "pageSize", "category")
        .Tag("products"));
    
    // Policy for frequently accessed data
    options.AddPolicy("Static", builder => builder
        .Expire(TimeSpan.FromHours(1))
        .Tag("static"));
});

var app = builder.Build();
app.UseOutputCache();

// Controller
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    [OutputCache(PolicyName = "Products")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null)
    {
        var products = await _productService.GetProductsAsync(page, pageSize, category);
        return Ok(products);
    }
    
    [HttpGet("{id}")]
    [OutputCache(Duration = 60)] // Cache for 60 seconds
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        return product == null ? NotFound() : Ok(product);
    }
    
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductDto dto,
        [FromServices] IOutputCacheStore cacheStore)
    {
        var result = await _productService.CreateAsync(dto);
        
        // Invalidate cache
        await cacheStore.EvictByTagAsync("products", CancellationToken.None);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }
}
```

### Response Caching Middleware

```csharp
builder.Services.AddResponseCaching();

var app = builder.Build();
app.UseResponseCaching();

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    [HttpGet]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "search" })]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll([FromQuery] string? search = null)
    {
        var categories = await _categoryService.GetAllAsync(search);
        return Ok(categories);
    }
    
    [HttpGet("{id}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        return category == null ? NotFound() : Ok(category);
    }
    
    [HttpPost]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }
}
```

### Distributed Caching

```csharp
// Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ProductApi_";
});

// Cache service
public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;
    
    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key, cancellationToken);
            
            if (cachedData == null)
                return null;
            
            return JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached data for key: {Key}", key);
            return null;
        }
    }
    
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            
            var serializedData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedData, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching data for key: {Key}", key);
        }
    }
    
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached data for key: {Key}", key);
        }
    }
}

// Usage in service
public class ProductService
{
    private readonly ICacheService _cacheService;
    private readonly IProductRepository _repository;
    
    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"product:{id}";
        
        // Try to get from cache
        var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);
        if (cachedProduct != null)
            return cachedProduct;
        
        // Get from database
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return null;
        
        var productDto = _mapper.Map<ProductDto>(product);
        
        // Cache for 10 minutes
        await _cacheService.SetAsync(cacheKey, productDto, TimeSpan.FromMinutes(10));
        
        return productDto;
    }
}
```

## Rate Limiting & Throttling

### Fixed Window Rate Limiting (.NET 9)

```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    // Fixed window: 100 requests per minute
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });
    
    // Sliding window: more sophisticated
    options.AddSlidingWindowLimiter("sliding", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.SegmentsPerWindow = 6; // 10-second segments
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });
    
    // Token bucket: handle bursts
    options.AddTokenBucketLimiter("token", limiterOptions =>
    {
        limiterOptions.TokenLimit = 100;
        limiterOptions.TokensPerPeriod = 20;
        limiterOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        limiterOptions.QueueLimit = 10;
    });
    
    // Concurrency limiter: limit concurrent requests
    options.AddConcurrencyLimiter("concurrency", limiterOptions =>
    {
        limiterOptions.PermitLimit = 50;
        limiterOptions.QueueLimit = 10;
    });
    
    // Policy for authenticated users
    options.AddPolicy("authenticated", context =>
    {
        var username = context.User.Identity?.Name ?? "anonymous";
        
        return RateLimitPartition.GetFixedWindowLimiter(username, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1000,
            Window = TimeSpan.FromMinutes(1)
        });
    });
    
    // Default rejection response
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
        }
        
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later.",
            retryAfter = retryAfter?.TotalSeconds
        }, cancellationToken);
    };
});

var app = builder.Build();
app.UseRateLimiter();

// Apply to controller or action
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    [EnableRateLimiting("sliding")] // Override with different policy
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }
    
    [HttpPost]
    [EnableRateLimiting("authenticated")] // Higher limit for authenticated users
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        var result = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }
    
    [HttpGet("expensive")]
    [EnableRateLimiting("concurrency")] // Limit concurrent requests
    public async Task<ActionResult> ExpensiveOperation()
    {
        // Long-running operation
        await Task.Delay(5000);
        return Ok();
    }
    
    [HttpGet("public")]
    [DisableRateLimiting] // Disable rate limiting
    public ActionResult GetPublicInfo()
    {
        return Ok(new { version = "1.0", status = "healthy" });
    }
}
```

### Custom Rate Limiting by IP

```csharp
options.AddPolicy("byIp", context =>
{
    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    
    return RateLimitPartition.GetSlidingWindowLimiter(ipAddress, _ => new SlidingWindowRateLimiterOptions
    {
        PermitLimit = 50,
        Window = TimeSpan.FromMinutes(1),
        SegmentsPerWindow = 6
    });
});
```

## API Documentation

### Swagger/OpenAPI Configuration

```csharp
// Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Product API",
        Description = "An ASP.NET Core Web API for managing products",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com",
            Url = new Uri("https://example.com/support")
        },
        License = new OpenApiLicense
        {
            Name = "Use under LICX",
            Url = new Uri("https://example.com/license")
        }
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    
    // Add JWT authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Group by version
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out var methodInfo)) return false;
        
        var versions = methodInfo.DeclaringType?
            .GetCustomAttributes(true)
            .OfType<ApiVersionAttribute>()
            .SelectMany(attr => attr.Versions);
        
        return versions?.Any(v => $"v{v}" == docName) ?? false;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
        options.RoutePrefix = string.Empty; // Serve at root
    });
}
```

### XML Documentation Comments

```csharp
/// <summary>
/// Creates a new product
/// </summary>
/// <param name="dto">Product creation data</param>
/// <returns>The created product</returns>
/// <response code="201">Returns the newly created product</response>
/// <response code="400">If the product data is invalid</response>
/// <response code="401">If the user is not authenticated</response>
[HttpPost]
[ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<ActionResult<ProductDto>> Create(
    /// <summary>Product details</summary>
    [FromBody] CreateProductDto dto)
{
    var result = await _productService.CreateAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
}
```

## Security Best Practices

### HTTPS Enforcement

```csharp
var app = builder.Build();

// Force HTTPS
app.UseHttpsRedirection();

// HSTS (HTTP Strict Transport Security)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
```

### CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins(
                "https://example.com",
                "https://app.example.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
    
    options.AddPolicy("Development", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("AllowSpecificOrigins");
}
```

### Input Sanitization

```csharp
public class InputSanitizerAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var arg in context.ActionArguments.Values.Where(v => v is string))
        {
            var input = arg as string;
            if (input != null)
            {
                // Remove potentially dangerous characters
                input = System.Web.HttpUtility.HtmlEncode(input);
                // Additional sanitization...
            }
        }
        
        base.OnActionExecuting(context);
    }
}
```

## Performance Optimization

### Asynchronous Programming

```csharp
// ✅ GOOD: Async all the way
[HttpGet]
public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
{
    var products = await _productService.GetAllAsync();
    return Ok(products);
}

// ❌ BAD: Blocking async code
[HttpGet]
public ActionResult<IEnumerable<ProductDto>> GetAll()
{
    var products = _productService.GetAllAsync().Result; // DON'T DO THIS
    return Ok(products);
}
```

### Pagination

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

public async Task<PagedResult<ProductDto>> GetProductsAsync(
    int page,
    int pageSize,
    string? search = null)
{
    var query = _context.Products.AsQueryable();
    
    if (!string.IsNullOrEmpty(search))
    {
        query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
    }
    
    var totalCount = await query.CountAsync();
    
    var items = await query
        .OrderBy(p => p.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
        .ToListAsync();
    
    return new PagedResult<ProductDto>
    {
        Items = items,
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount
    };
}
```

### Projection with AutoMapper

```csharp
// ✅ GOOD: Project at database level
public async Task<IEnumerable<ProductDto>> GetAllAsync()
{
    return await _context.Products
        .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
        .ToListAsync();
}

// ❌ BAD: Load everything then map
public async Task<IEnumerable<ProductDto>> GetAllAsync()
{
    var products = await _context.Products.ToListAsync(); // Loads all columns
    return _mapper.Map<IEnumerable<ProductDto>>(products);
}
```

### Response Compression

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

var app = builder.Build();
app.UseResponseCompression();
```

## Interview Questions

### Design Questions

1. **How would you design a RESTful API for a social media platform?**
   - Resources: users, posts, comments, likes
   - Nested resources: /users/{id}/posts, /posts/{id}/comments
   - Pagination for feeds
   - Filtering and sorting
   - Rate limiting for API protection
   - Caching strategies for hot data

2. **Explain your approach to API versioning. When would you use each strategy?**
   - URL versioning: Easy to understand, clear separation
   - Header versioning: Cleaner URLs, more complex
   - Media type versioning: RESTful, flexible
   - Choose based on client needs and backward compatibility requirements

3. **How do you handle long-running operations in Web API?**
   ```csharp
   // Return 202 Accepted with location
   [HttpPost("process")]
   public async Task<IActionResult> ProcessLargeFile([FromBody] FileDto file)
   {
       var jobId = Guid.NewGuid();
       _ = Task.Run(() => _backgroundService.ProcessFileAsync(jobId, file));
       
       return AcceptedAtAction(
           nameof(GetJobStatus),
           new { jobId },
           new { jobId, status = "Processing" });
   }
   
   [HttpGet("jobs/{jobId}")]
   public async Task<ActionResult<JobStatus>> GetJobStatus(Guid jobId)
   {
       var status = await _jobService.GetStatusAsync(jobId);
       return Ok(status);
   }
   ```

### Performance Questions

4. **How would you optimize an API that's experiencing slow response times?**
   - Add caching (output cache, distributed cache)
   - Implement pagination
   - Use database query optimization
   - Add response compression
   - Use async/await properly
   - Consider CDN for static content
   - Implement database indexing

5. **Explain the difference between Output Caching and Response Caching**
   - Output Caching: Server-side, ASP.NET Core 9 feature, more flexible
   - Response Caching: Can be client-side or server-side, uses HTTP headers
   - Output caching is generally more powerful and recommended for .NET 9+

### Security Questions

6. **How do you secure a Web API?**
   - Authentication (JWT, OAuth2)
   - Authorization (role-based, claims-based, policy-based)
   - HTTPS only
   - CORS configuration
   - Rate limiting
   - Input validation
   - SQL injection prevention
   - XSS protection

7. **What's the difference between authentication and authorization?**
   - Authentication: Verifying who you are (login)
   - Authorization: Verifying what you can do (permissions)
   - Both are required for secure APIs

### Practical Questions

8. **How would you implement bulk operations efficiently?**
   ```csharp
   [HttpPost("bulk")]
   public async Task<ActionResult<BulkOperationResult>> BulkCreate(
       [FromBody] List<CreateProductDto> products)
   {
       if (products.Count > 1000)
           return BadRequest("Maximum 1000 products per request");
       
       var result = await _productService.BulkCreateAsync(products);
       return Ok(result);
   }
   ```

9. **How do you handle file uploads in Web API?**
   ```csharp
   [HttpPost("upload")]
   [RequestSizeLimit(104857600)] // 100 MB
   public async Task<IActionResult> Upload(IFormFile file)
   {
       if (file.Length == 0)
           return BadRequest("File is empty");
       
       if (file.Length > 104857600)
           return BadRequest("File size exceeds 100 MB");
       
       var allowedExtensions = new[] { ".jpg", ".png", ".pdf" };
       var extension = Path.GetExtension(file.FileName);
       
       if (!allowedExtensions.Contains(extension.ToLower()))
           return BadRequest("Invalid file type");
       
       await _fileService.SaveAsync(file);
       return Ok();
   }
   ```

10. **How would you implement search functionality with multiple filters?**
    ```csharp
    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<ProductDto>>> Search(
        [FromQuery] ProductSearchQuery query)
    {
        var results = await _productService.SearchAsync(query);
        return Ok(results);
    }
    
    public class ProductSearchQuery
    {
        public string? SearchTerm { get; set; }
        public string? Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string>? Tags { get; set; }
        public string? SortBy { get; set; }
        public string SortOrder { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
    ```

## See Also

- [Authentication & Authorization](./08-auth-2026.md)
- [Performance Optimization](./09-performance-optimization.md)
- [Testing Strategies](./10-testing-dotnet-9.md)
- [Clean Architecture](./11-clean-architecture.md)

## Additional Resources

- [Microsoft Docs: Web API Best Practices](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [RESTful API Design](https://restfulapi.net/)
- [API Security Best Practices](https://owasp.org/www-project-api-security/)
