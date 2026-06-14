# Comprehensive Testing in .NET 9

## Table of Contents
- [Testing Framework Overview](#testing-framework-overview)
- [xUnit, NUnit, and MSTest Comparison](#xunit-nunit-and-mstest-comparison)
- [Integration Testing](#integration-testing)
- [Testcontainers for .NET](#testcontainers-for-net)
- [Mocking Strategies](#mocking-strategies)
- [Code Coverage](#code-coverage)
- [Testing Best Practices](#testing-best-practices)
- [Testing Async Code](#testing-async-code)
- [Database Testing](#database-testing)
- [Interview Questions](#interview-questions)

## Testing Framework Overview

.NET 9 supports multiple testing frameworks, each with unique features and philosophies.

### Test Anatomy

```csharp
// AAA Pattern: Arrange, Act, Assert
public class CalculatorTests
{
    [Fact]
    public void Add_TwoPositiveNumbers_ReturnsSum()
    {
        // Arrange - Setup test data and dependencies
        var calculator = new Calculator();
        int a = 5, b = 3;
        
        // Act - Execute the method under test
        var result = calculator.Add(a, b);
        
        // Assert - Verify the result
        Assert.Equal(8, result);
    }
}
```

### Test Categories

```csharp
// Unit Tests - Test single component in isolation
// Integration Tests - Test component interactions
// End-to-End Tests - Test complete user scenarios
// Performance Tests - Test performance characteristics

[Trait("Category", "Unit")]
public class UnitTest { }

[Trait("Category", "Integration")]
public class IntegrationTest { }

[Trait("Category", "E2E")]
public class EndToEndTest { }
```

## xUnit, NUnit, and MSTest Comparison

### xUnit (Recommended for .NET)

```csharp
using Xunit;

public class XUnitExamples
{
    // Fact - Simple test without parameters
    [Fact]
    public void SimpleFact()
    {
        var result = 2 + 2;
        Assert.Equal(4, result);
    }
    
    // Theory - Data-driven test
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(2, 3, 5)]
    [InlineData(-1, 1, 0)]
    public void Add_VariousInputs_ReturnsExpectedSum(int a, int b, int expected)
    {
        var calculator = new Calculator();
        var result = calculator.Add(a, b);
        Assert.Equal(expected, result);
    }
    
    // MemberData - Complex test data
    public static IEnumerable<object[]> TestData =>
        new List<object[]>
        {
            new object[] { 1, 2, 3 },
            new object[] { 2, 3, 5 },
            new object[] { -1, 1, 0 }
        };
    
    [Theory]
    [MemberData(nameof(TestData))]
    public void Add_MemberData_ReturnsExpectedSum(int a, int b, int expected)
    {
        var calculator = new Calculator();
        var result = calculator.Add(a, b);
        Assert.Equal(expected, result);
    }
    
    // ClassData - Reusable test data
    public class AddTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { 1, 2, 3 };
            yield return new object[] { 2, 3, 5 };
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() 
            => GetEnumerator();
    }
    
    [Theory]
    [ClassData(typeof(AddTestData))]
    public void Add_ClassData_ReturnsExpectedSum(int a, int b, int expected)
    {
        var calculator = new Calculator();
        var result = calculator.Add(a, b);
        Assert.Equal(expected, result);
    }
    
    // Setup/Teardown via constructor and IDisposable
    private readonly TestFixture _fixture;
    
    public XUnitExamples()
    {
        _fixture = new TestFixture(); // Runs before each test
    }
    
    public void Dispose()
    {
        _fixture.Dispose(); // Runs after each test
    }
}

// Shared context across tests
public class DatabaseFixture : IDisposable
{
    public AppDbContext DbContext { get; }
    
    public DatabaseFixture()
    {
        DbContext = CreateDbContext();
        DbContext.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
    
    private AppDbContext CreateDbContext() => new();
}

public class TestsUsingSharedContext : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    
    public TestsUsingSharedContext(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public void Test_UsesSharedDatabase()
    {
        // Use _fixture.DbContext
    }
}
```

### NUnit

```csharp
using NUnit.Framework;

[TestFixture]
public class NUnitExamples
{
    private Calculator _calculator = null!;
    
    [SetUp]
    public void Setup()
    {
        _calculator = new Calculator(); // Runs before each test
    }
    
    [TearDown]
    public void Teardown()
    {
        // Cleanup after each test
    }
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        // Runs once before all tests in fixture
    }
    
    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        // Runs once after all tests in fixture
    }
    
    [Test]
    public void Add_TwoNumbers_ReturnsSum()
    {
        var result = _calculator.Add(2, 3);
        Assert.That(result, Is.EqualTo(5));
    }
    
    [TestCase(1, 2, 3)]
    [TestCase(2, 3, 5)]
    [TestCase(-1, 1, 0)]
    public void Add_VariousInputs_ReturnsExpectedSum(int a, int b, int expected)
    {
        var result = _calculator.Add(a, b);
        Assert.That(result, Is.EqualTo(expected));
    }
    
    // Rich assertion syntax
    [Test]
    public void RichAssertions()
    {
        var list = new List<int> { 1, 2, 3 };
        
        Assert.That(list, Has.Count.EqualTo(3));
        Assert.That(list, Contains.Item(2));
        Assert.That(list, Is.Ordered);
        Assert.That(list, Is.All.GreaterThan(0));
    }
    
    [Test, Category("Slow")]
    [Timeout(5000)]
    public void SlowTest()
    {
        // Must complete within 5 seconds
    }
}
```

### MSTest

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class MSTestExamples
{
    private Calculator _calculator = null!;
    
    [TestInitialize]
    public void Initialize()
    {
        _calculator = new Calculator();
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        // Cleanup after each test
    }
    
    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        // Runs once before all tests
    }
    
    [ClassCleanup]
    public static void ClassCleanup()
    {
        // Runs once after all tests
    }
    
    [TestMethod]
    public void Add_TwoNumbers_ReturnsSum()
    {
        var result = _calculator.Add(2, 3);
        Assert.AreEqual(5, result);
    }
    
    [DataTestMethod]
    [DataRow(1, 2, 3)]
    [DataRow(2, 3, 5)]
    [DataRow(-1, 1, 0)]
    public void Add_VariousInputs_ReturnsExpectedSum(int a, int b, int expected)
    {
        var result = _calculator.Add(a, b);
        Assert.AreEqual(expected, result);
    }
    
    [TestMethod]
    [TestCategory("Integration")]
    [Timeout(5000)]
    public void IntegrationTest()
    {
        // Test implementation
    }
}
```

### Framework Comparison

```
Feature                 | xUnit      | NUnit      | MSTest
------------------------|------------|------------|------------
Microsoft Support       | ✓          | ✓          | ✓✓
Parallel Execution      | ✓✓ Default | ✓          | ✓
Setup/Teardown          | Constructor| [SetUp]    | [TestInitialize]
Data-Driven Tests       | [Theory]   | [TestCase] | [DataTestMethod]
Assertion Style         | Assert.*   | Assert.That| Assert.Are*
Modern API              | ✓✓         | ✓✓         | ✓
Community Preference    | ✓✓         | ✓          | -
.NET Core/9 Focus       | ✓✓         | ✓✓         | ✓

Recommendation: xUnit for new projects (used by .NET team)
```

## Integration Testing

### WebApplicationFactory for API Testing

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

// Make Program class accessible to tests
public partial class Program { }

public class ProductApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public ProductApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetProducts_ReturnsSuccessStatusCode()
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
        var newProduct = new CreateProductRequest
        {
            Name = "Test Product",
            Price = 19.99m
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/products", newProduct);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(created);
        Assert.Equal(newProduct.Name, created.Name);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task CreateProduct_WithInvalidName_ReturnsBadRequest(string? name)
    {
        // Arrange
        var invalidProduct = new CreateProductRequest
        {
            Name = name!,
            Price = 19.99m
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/products", invalidProduct);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
```

### Custom WebApplicationFactory

```csharp
public class CustomWebApplicationFactory<TProgram> 
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            
            // Add in-memory database
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });
            
            // Replace real services with test doubles
            services.AddScoped<IEmailService, FakeEmailService>();
            services.AddScoped<IPaymentService, FakePaymentService>();
            
            // Build service provider
            var sp = services.BuildServiceProvider();
            
            // Seed test data
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            SeedTestData(db);
        });
        
        builder.UseEnvironment("Testing");
    }
    
    private static void SeedTestData(AppDbContext db)
    {
        db.Products.AddRange(
            new Product { Id = 1, Name = "Product 1", Price = 10.00m },
            new Product { Id = 2, Name = "Product 2", Price = 20.00m }
        );
        db.SaveChanges();
    }
}

// Usage
public class ProductApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public ProductApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Test_UsesCustomFactory()
    {
        var response = await _client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
    }
}
```

### Testing with Authentication

```csharp
public class AuthenticatedApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    
    public AuthenticatedApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    private HttpClient CreateAuthenticatedClient(string userId = "test-user", string role = "User")
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test", options => { });
            });
        }).CreateClient();
    }
    
    [Fact]
    public async Task ProtectedEndpoint_WithAuth_ReturnsSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        
        // Act
        var response = await client.GetAsync("/api/protected");
        
        // Assert
        response.EnsureSuccessStatusCode();
    }
    
    [Fact]
    public async Task AdminEndpoint_WithUserRole_ReturnsForbidden()
    {
        // Arrange
        var client = CreateAuthenticatedClient(role: "User");
        
        // Act
        var response = await client.GetAsync("/api/admin");
        
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}

// Test authentication handler
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }
    
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Role, "User")
        };
        
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

## Testcontainers for .NET

Testcontainers allows running real dependencies (databases, message brokers) in Docker containers during tests.

### Basic Testcontainers Setup

```csharp
using Testcontainers.PostgreSql;
using Xunit;

public class PostgresIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("testdb")
        .WithUsername("test")
        .WithPassword("test")
        .WithPortBinding(5432, true) // Random host port
        .Build();
    
    private AppDbContext _dbContext = null!;
    
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        
        _dbContext = new AppDbContext(options);
        await _dbContext.Database.MigrateAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }
    
    [Fact]
    public async Task Product_CanBeInsertedAndRetrieved()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Price = 29.99m
        };
        
        // Act
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        
        // Assert
        var retrieved = await _dbContext.Products.FindAsync(product.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(product.Name, retrieved.Name);
    }
}
```

### Multiple Containers

```csharp
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Testcontainers.RabbitMq;

public class MultiContainerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private readonly RedisContainer _redis;
    private readonly RabbitMqContainer _rabbitmq;
    
    public MultiContainerTests()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .Build();
        
        _redis = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();
        
        _rabbitmq = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        // Start all containers in parallel
        await Task.WhenAll(
            _postgres.StartAsync(),
            _redis.StartAsync(),
            _rabbitmq.StartAsync()
        );
    }
    
    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _postgres.DisposeAsync().AsTask(),
            _redis.DisposeAsync().AsTask(),
            _rabbitmq.DisposeAsync().AsTask()
        );
    }
    
    [Fact]
    public async Task IntegrationTest_WithMultipleServices()
    {
        // Use _postgres.GetConnectionString()
        // Use _redis.GetConnectionString()
        // Use _rabbitmq.GetConnectionString()
        
        // Test integration across services
    }
}
```

### Custom Container Configuration

```csharp
public class CustomContainerTests : IAsyncLifetime
{
    private readonly IContainer _sqlServer;
    
    public CustomContainerTests()
    {
        _sqlServer = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("SA_PASSWORD", "YourStrong@Passw0rd")
            .WithPortBinding(1433, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(1433))
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();
        
        // Wait for SQL Server to be ready
        await Task.Delay(5000);
    }
    
    public async Task DisposeAsync()
    {
        await _sqlServer.DisposeAsync();
    }
    
    [Fact]
    public async Task Test_WithSqlServer()
    {
        var connectionString = $"Server=localhost,{_sqlServer.GetMappedPublicPort(1433)};" +
                              "Database=TestDb;User Id=sa;Password=YourStrong@Passw0rd;" +
                              "TrustServerCertificate=True";
        
        // Use connection string
    }
}
```

## Mocking Strategies

### Moq - Popular Mocking Framework

```csharp
using Moq;
using Xunit;

public class MoqExamples
{
    [Fact]
    public void OrderService_CreateOrder_CallsRepository()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        var mockEmail = new Mock<IEmailService>();
        
        mockRepo.Setup(r => r.SaveAsync(It.IsAny<Order>()))
            .ReturnsAsync(new Order { Id = 1 });
        
        var service = new OrderService(mockRepo.Object, mockEmail.Object);
        
        // Act
        var result = service.CreateOrderAsync(new CreateOrderRequest()).Result;
        
        // Assert
        mockRepo.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Once);
        mockEmail.Verify(e => e.SendOrderConfirmationAsync(It.IsAny<Order>()), Times.Once);
    }
    
    [Fact]
    public void ProductService_GetProduct_ReturnsFromCache()
    {
        // Arrange
        var mockCache = new Mock<ICache>();
        var mockRepo = new Mock<IProductRepository>();
        
        var cachedProduct = new Product { Id = 1, Name = "Cached" };
        mockCache.Setup(c => c.Get<Product>("product:1"))
            .Returns(cachedProduct);
        
        var service = new ProductService(mockRepo.Object, mockCache.Object);
        
        // Act
        var result = service.GetProductAsync(1).Result;
        
        // Assert
        Assert.Equal(cachedProduct, result);
        mockRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }
    
    [Fact]
    public void Calculator_Divide_ByZero_ThrowsException()
    {
        // Arrange
        var mockValidator = new Mock<IValidator>();
        mockValidator.Setup(v => v.ValidateDivision(It.IsAny<int>(), 0))
            .Throws<DivideByZeroException>();
        
        var calculator = new Calculator(mockValidator.Object);
        
        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => calculator.Divide(10, 0));
    }
    
    [Fact]
    public void Service_CallsCallback()
    {
        // Arrange
        var mockService = new Mock<INotificationService>();
        Order? capturedOrder = null;
        
        mockService.Setup(s => s.NotifyAsync(It.IsAny<Order>()))
            .Callback<Order>(order => capturedOrder = order)
            .ReturnsAsync(true);
        
        // Act
        var order = new Order { Id = 1, Total = 100m };
        mockService.Object.NotifyAsync(order).Wait();
        
        // Assert
        Assert.NotNull(capturedOrder);
        Assert.Equal(100m, capturedOrder.Total);
    }
    
    [Fact]
    public void Service_ReturnsSequence()
    {
        // Arrange
        var mock = new Mock<ICounterService>();
        mock.SetupSequence(s => s.GetNext())
            .Returns(1)
            .Returns(2)
            .Returns(3);
        
        // Act & Assert
        Assert.Equal(1, mock.Object.GetNext());
        Assert.Equal(2, mock.Object.GetNext());
        Assert.Equal(3, mock.Object.GetNext());
    }
}
```

### NSubstitute - Alternative Mocking

```csharp
using NSubstitute;
using Xunit;

public class NSubstituteExamples
{
    [Fact]
    public void OrderService_CreateOrder_CallsRepository()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        var emailService = Substitute.For<IEmailService>();
        
        repository.SaveAsync(Arg.Any<Order>())
            .Returns(new Order { Id = 1 });
        
        var service = new OrderService(repository, emailService);
        
        // Act
        var result = service.CreateOrderAsync(new CreateOrderRequest()).Result;
        
        // Assert
        repository.Received(1).SaveAsync(Arg.Any<Order>());
        emailService.Received(1).SendOrderConfirmationAsync(Arg.Any<Order>());
    }
    
    [Fact]
    public void Calculator_Divide_ByZero_ThrowsException()
    {
        // Arrange
        var validator = Substitute.For<IValidator>();
        validator.ValidateDivision(Arg.Any<int>(), 0)
            .Returns(x => throw new DivideByZeroException());
        
        var calculator = new Calculator(validator);
        
        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => calculator.Divide(10, 0));
    }
    
    [Fact]
    public void Service_ArgumentMatching()
    {
        // Arrange
        var service = Substitute.For<IProductService>();
        
        // Match specific values
        service.GetProductAsync(1).Returns(new Product { Name = "Product 1" });
        service.GetProductAsync(2).Returns(new Product { Name = "Product 2" });
        
        // Match conditions
        service.GetProductAsync(Arg.Is<int>(x => x > 100))
            .Returns((Product?)null);
        
        // Act & Assert
        Assert.Equal("Product 1", service.GetProductAsync(1).Result?.Name);
        Assert.Null(service.GetProductAsync(101).Result);
    }
}
```

### Fake Implementations

```csharp
// Sometimes better than mocks for complex logic
public class FakeProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();
    private int _nextId = 1;
    
    public Task<Product?> GetByIdAsync(int id)
    {
        return Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
    }
    
    public Task<List<Product>> GetAllAsync()
    {
        return Task.FromResult(_products.ToList());
    }
    
    public Task<Product> CreateAsync(Product product)
    {
        product.Id = _nextId++;
        _products.Add(product);
        return Task.FromResult(product);
    }
    
    public Task UpdateAsync(Product product)
    {
        var existing = _products.FirstOrDefault(p => p.Id == product.Id);
        if (existing != null)
        {
            _products.Remove(existing);
            _products.Add(product);
        }
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _products.Remove(product);
        }
        return Task.CompletedTask;
    }
}

// Usage in tests
public class ProductServiceTests
{
    [Fact]
    public async Task GetProduct_ReturnsProduct()
    {
        // Arrange
        var repository = new FakeProductRepository();
        var product = await repository.CreateAsync(new Product { Name = "Test" });
        var service = new ProductService(repository);
        
        // Act
        var result = await service.GetProductAsync(product.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }
}
```

## Code Coverage

### Using Coverlet

```xml
<!-- Install coverlet.collector -->
<ItemGroup>
  <PackageReference Include="coverlet.collector" Version="6.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate HTML report with ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html

# Open report
open coveragereport/index.html
```

### Coverage Configuration

```xml
<!-- coverlet.runsettings -->
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <Format>cobertura</Format>
          <Exclude>[*.Tests]*,[*]*.Generated.*</Exclude>
          <Include>[MyApp.*]*</Include>
          <ExcludeByFile>**/Migrations/**/*.cs</ExcludeByFile>
          <IncludeTestAssembly>false</IncludeTestAssembly>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

```bash
# Use settings file
dotnet test --settings coverlet.runsettings
```

### Interpreting Coverage Metrics

```
Line Coverage:    Percentage of lines executed
Branch Coverage:  Percentage of decision points executed
Method Coverage:  Percentage of methods called

Good Coverage Targets:
- Critical Business Logic: 90-100%
- Service Layer: 80-90%
- Controllers/APIs: 70-80%
- Overall: 70-80%

Note: 100% coverage doesn't mean bug-free code!
```

## Testing Best Practices

### Test Naming Conventions

```csharp
public class TestNamingExamples
{
    // Pattern: MethodName_Scenario_ExpectedResult
    
    [Fact]
    public void Add_TwoPositiveNumbers_ReturnsSum()
    {
        // Clear what's being tested
    }
    
    [Fact]
    public void GetUser_WithInvalidId_ReturnsNull()
    {
        // Clear scenario and expectation
    }
    
    [Fact]
    public void ProcessPayment_WhenInsufficientFunds_ThrowsException()
    {
        // Clear failure case
    }
}
```

### Single Assertion Principle

```csharp
public class AssertionExamples
{
    // ❌ Multiple unrelated assertions
    [Fact]
    public void BadTest()
    {
        var user = CreateUser();
        Assert.NotNull(user);
        Assert.Equal("John", user.Name);
        
        var order = CreateOrder(user);
        Assert.NotNull(order);
        Assert.Equal(100m, order.Total);
    }
    
    // ✅ Separate tests for separate concerns
    [Fact]
    public void CreateUser_ReturnsUser_WithCorrectName()
    {
        var user = CreateUser();
        
        Assert.NotNull(user);
        Assert.Equal("John", user.Name);
    }
    
    [Fact]
    public void CreateOrder_ForUser_SetsCorrectTotal()
    {
        var user = CreateUser();
        var order = CreateOrder(user);
        
        Assert.NotNull(order);
        Assert.Equal(100m, order.Total);
    }
    
    private User CreateUser() => new User { Name = "John" };
    private Order CreateOrder(User user) => new Order { Total = 100m };
}
```

### Test Independence

```csharp
// ❌ Tests depend on execution order
public class BadTests
{
    private static User? _user;
    
    [Fact]
    public void Test1_CreateUser()
    {
        _user = new User { Id = 1, Name = "John" };
    }
    
    [Fact]
    public void Test2_UpdateUser() // Depends on Test1!
    {
        _user!.Name = "Jane";
        Assert.Equal("Jane", _user.Name);
    }
}

// ✅ Independent tests
public class GoodTests
{
    [Fact]
    public void CreateUser_SetsProperties()
    {
        var user = new User { Id = 1, Name = "John" };
        Assert.Equal("John", user.Name);
    }
    
    [Fact]
    public void UpdateUser_ChangesName()
    {
        var user = new User { Id = 1, Name = "John" };
        user.Name = "Jane";
        Assert.Equal("Jane", user.Name);
    }
}
```

### Test Data Builders

```csharp
// Builder pattern for test data
public class ProductBuilder
{
    private int _id = 1;
    private string _name = "Default Product";
    private decimal _price = 10.00m;
    private bool _isActive = true;
    
    public ProductBuilder WithId(int id)
    {
        _id = id;
        return this;
    }
    
    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public ProductBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }
    
    public ProductBuilder Inactive()
    {
        _isActive = false;
        return this;
    }
    
    public Product Build()
    {
        return new Product
        {
            Id = _id,
            Name = _name,
            Price = _price,
            IsActive = _isActive
        };
    }
}

// Usage
public class ProductServiceTests
{
    [Fact]
    public void GetActiveProducts_ExcludesInactiveProducts()
    {
        // Arrange
        var products = new[]
        {
            new ProductBuilder().WithName("Active").Build(),
            new ProductBuilder().WithName("Inactive").Inactive().Build()
        };
        
        var service = new ProductService(products);
        
        // Act
        var result = service.GetActiveProducts();
        
        // Assert
        Assert.Single(result);
        Assert.Equal("Active", result.First().Name);
    }
}
```

## Testing Async Code

### Async Test Patterns

```csharp
public class AsyncTestExamples
{
    // xUnit handles async tests automatically
    [Fact]
    public async Task GetUserAsync_ReturnsUser()
    {
        // Arrange
        var service = new UserService();
        
        // Act
        var user = await service.GetUserAsync(1);
        
        // Assert
        Assert.NotNull(user);
    }
    
    // Testing Task cancellation
    [Fact]
    public async Task LongRunningOperation_CanBeCancelled()
    {
        // Arrange
        var service = new DataService();
        var cts = new CancellationTokenSource();
        
        // Act
        var task = service.ProcessDataAsync(cts.Token);
        cts.Cancel();
        
        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
    }
    
    // Testing timeouts
    [Fact]
    public async Task Operation_CompletesWithinTimeout()
    {
        // Arrange
        var service = new SlowService();
        
        // Act
        var task = service.SlowOperationAsync();
        var completedTask = await Task.WhenAny(task, Task.Delay(1000));
        
        // Assert
        Assert.Equal(task, completedTask); // Completed before timeout
    }
    
    // Testing parallel operations
    [Fact]
    public async Task ProcessMultiple_HandlesParallelExecution()
    {
        // Arrange
        var service = new BatchService();
        var items = Enumerable.Range(1, 10).ToList();
        
        // Act
        await service.ProcessInParallelAsync(items);
        
        // Assert
        Assert.All(items, item => Assert.True(item > 0));
    }
}
```

### Testing Race Conditions

```csharp
public class ConcurrencyTests
{
    [Fact]
    public async Task Counter_IsThreadSafe()
    {
        // Arrange
        var counter = new ThreadSafeCounter();
        var tasks = new List<Task>();
        
        // Act - 100 concurrent increments
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() => counter.Increment()));
        }
        await Task.WhenAll(tasks);
        
        // Assert
        Assert.Equal(100, counter.Value);
    }
    
    [Fact]
    public async Task Cache_HandlesSimultaneousAccess()
    {
        // Arrange
        var cache = new ConcurrentCache<int, string>();
        var tasks = new List<Task<string>>();
        
        // Act - Multiple simultaneous reads/writes
        for (int i = 0; i < 50; i++)
        {
            int id = i;
            tasks.Add(Task.Run(() => cache.GetOrAddAsync(id, () => $"Value {id}")));
        }
        
        var results = await Task.WhenAll(tasks);
        
        // Assert
        Assert.Equal(50, results.Length);
        Assert.All(results, Assert.NotNull);
    }
}
```

## Database Testing

### In-Memory Database

```csharp
public class InMemoryDatabaseTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new AppDbContext(options);
    }
    
    [Fact]
    public async Task AddProduct_SavesAndRetrievesCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new ProductRepository(context);
        
        var product = new Product
        {
            Name = "Test Product",
            Price = 19.99m
        };
        
        // Act
        await repository.CreateAsync(product);
        var retrieved = await repository.GetByIdAsync(product.Id);
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(product.Name, retrieved.Name);
        Assert.Equal(product.Price, retrieved.Price);
    }
}
```

### Testing Transactions

```csharp
public class TransactionTests
{
    [Fact]
    public async Task OrderService_RollsBackOnFailure()
    {
        // Arrange
        using var context = CreateContext();
        var service = new OrderService(context);
        
        // Act & Assert
        await Assert.ThrowsAsync<InsufficientInventoryException>(
            () => service.CreateOrderAsync(new CreateOrderRequest
            {
                ProductId = 1,
                Quantity = 1000 // More than available
            }));
        
        // Verify no order was created
        var orders = await context.Orders.ToListAsync();
        Assert.Empty(orders);
    }
}
```

## Interview Questions

### 1. What are the differences between xUnit, NUnit, and MSTest?
**Answer**: xUnit: (1) No [SetUp]/[TearDown], uses constructor/IDisposable, (2) Parallel by default, (3) [Fact] for tests, [Theory] for data-driven, (4) Preferred by .NET team. NUnit: (1) [SetUp]/[TearDown] methods, (2) Rich Assert.That syntax, (3) [Test] and [TestCase], (4) Mature framework. MSTest: (1) [TestInitialize]/[TestCleanup], (2) [TestMethod] and [DataTestMethod], (3) Built by Microsoft, (4) Visual Studio integration. Recommendation: xUnit for new projects due to modern design and .NET team adoption.

### 2. What is WebApplicationFactory and how do you use it for integration testing?
**Answer**: WebApplicationFactory creates test server for ASP.NET Core apps without deploying. Usage: (1) Make Program class accessible (partial class), (2) Create test class with IClassFixture<WebApplicationFactory<Program>>, (3) Get HttpClient from factory, (4) Make HTTP requests to test endpoints. Benefits: Real request pipeline, test middleware, test routing, test authentication. Can customize with ConfigureWebHost to replace services (use in-memory DB, fake services). Allows end-to-end API testing without deployment.

### 3. What are Testcontainers and why use them instead of mocks?
**Answer**: Testcontainers runs real dependencies (databases, Redis, RabbitMQ) in Docker containers during tests. Benefits vs mocks: (1) Tests against real database behavior (constraints, transactions, queries), (2) Catches integration issues mocks miss, (3) Tests migrations, (4) More confidence in production behavior. Tradeoffs: Slower than mocks, requires Docker, more resource intensive. Use for: integration tests, database-specific features, migration testing. Use mocks for: unit tests, external APIs, fast tests. Testcontainers bridges gap between unit and E2E tests.

### 4. Explain the difference between Moq and NSubstitute.
**Answer**: Moq: (1) Setup-based syntax (.Setup().Returns()), (2) Verify with .Verify(), (3) More explicit, (4) Widely used, (5) Example: mock.Setup(x => x.Get()).Returns(value). NSubstitute: (1) Constraint-based syntax (cleaner), (2) Verify with .Received(), (3) Less verbose, (4) Example: substitute.Get().Returns(value). Both provide: mocking interfaces, verifying calls, argument matching. NSubstitute generally considered cleaner/more readable. Moq has more features for complex scenarios. Choose based on team preference.

### 5. How do you test async/await code effectively?
**Answer**: (1) Mark test method async Task, await async methods, (2) Test cancellation with CancellationTokenSource, Assert.ThrowsAsync<OperationCanceledException>, (3) Test timeouts with Task.WhenAny, (4) Test parallel execution with Task.WhenAll, (5) Test race conditions by running operations concurrently, (6) Use ValueTask appropriately in tested code, (7) Avoid async void (use async Task), (8) Test both sync and async paths if using ValueTask. xUnit handles async tests automatically. Never use .Result or .Wait() in tests - always await.

### 6. What is code coverage and what's a good target?
**Answer**: Code coverage measures percentage of code executed by tests. Types: (1) Line coverage - % lines executed, (2) Branch coverage - % decision points executed, (3) Method coverage - % methods called. Use Coverlet for .NET. Good targets: Critical business logic 90-100%, service layer 80-90%, APIs 70-80%, overall 70-80%. Important: 100% coverage ≠ bug-free. Focus on quality tests over coverage percentage. Uncovered code is definitely untested, covered code may not be well tested.

### 7. What is the AAA pattern in testing?
**Answer**: AAA = Arrange, Act, Assert. (1) Arrange: Set up test data, dependencies, expected values, (2) Act: Execute method/operation being tested (should be one line), (3) Assert: Verify result matches expectations. Benefits: Clear test structure, easy to understand, maintainable. Sometimes add cleanup (AAAC pattern). Related patterns: Given-When-Then (BDD style), similar concept. AAA helps keep tests focused on single behavior. If arrange section is huge, consider test data builders or factories.

### 8. How do you handle test data setup and teardown?
**Answer**: xUnit: (1) Constructor for setup (runs before each test), (2) IDisposable.Dispose for teardown (runs after each test), (3) IClassFixture<T> for shared context across test class, (4) ICollectionFixture<T> for shared context across multiple classes. Alternatives: Test data builders, factory methods, Object Mother pattern. For databases: (1) Transactions that rollback, (2) In-memory databases recreated per test, (3) Testcontainers destroyed after tests. Important: Tests must be independent - no shared mutable state.

### 9. What's the difference between fakes, mocks, and stubs?
**Answer**: Stub: Returns canned responses, no verification. Example: stub.GetUser() returns hardcoded user. Mock: Verifies interactions, expectations on how it's called. Example: verify SaveAsync called exactly once. Fake: Working implementation with shortcuts. Example: InMemoryUserRepository instead of real DB. When to use: (1) Stubs for simple test inputs, (2) Mocks when verifying behavior matters, (3) Fakes for complex logic or when behavior matters more than calls. Test state when possible, behavior when necessary. Over-mocking leads to brittle tests.

### 10. How do you test a method that calls external APIs?
**Answer**: Options: (1) Mock IHttpClientFactory/HttpClient with Moq/NSubstitute, (2) Use HttpMessageHandler mock for lower-level control, (3) Record/replay with libraries like WireMock.NET, (4) Create fake service implementation, (5) Integration tests with test endpoint. Best practice: Create abstraction (IExternalService), mock that in unit tests, test real implementation in integration tests with test API or WireMock. For resilience: test retries, timeouts, circuit breakers. Use Polly for resilience. Never call real external APIs in automated tests (slow, unreliable, costs money).

---

**Last Updated: January 2026 - .NET 9**

**Related Topics**: See also [Minimal APIs Advanced](./07-minimal-apis-advanced.md), [Performance Optimization](./09-performance-optimization.md)
