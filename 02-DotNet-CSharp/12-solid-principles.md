# SOLID Principles in .NET - Production Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Single Responsibility Principle (SRP)](#single-responsibility-principle-srp)
3. [Open/Closed Principle (OCP)](#openclosed-principle-ocp)
4. [Liskov Substitution Principle (LSP)](#liskov-substitution-principle-lsp)
5. [Interface Segregation Principle (ISP)](#interface-segregation-principle-isp)
6. [Dependency Inversion Principle (DIP)](#dependency-inversion-principle-dip)
7. [Real-World Examples](#real-world-examples)
8. [Interview Questions](#interview-questions)

## Introduction

SOLID is an acronym for five design principles intended to make software designs more understandable, flexible, and maintainable. These principles were introduced by Robert C. Martin (Uncle Bob) and are fundamental to object-oriented programming and clean code.

### Why SOLID Matters

```csharp
// ❌ WITHOUT SOLID: Tightly coupled, hard to test, difficult to maintain
public class UserService
{
    public void RegisterUser(string email, string password)
    {
        // Direct database access
        using var connection = new SqlConnection("connection_string");
        connection.Execute("INSERT INTO Users...");
        
        // Direct SMTP email
        var smtp = new SmtpClient("smtp.gmail.com");
        smtp.Send(new MailMessage("from@example.com", email, "Welcome", "Body"));
        
        // Direct logging
        File.AppendAllText("log.txt", $"User registered: {email}");
    }
}

// ✅ WITH SOLID: Loosely coupled, testable, maintainable
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;
    
    public UserService(
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
    }
    
    public async Task<Result> RegisterUserAsync(string email, string password)
    {
        var user = new User(email, password);
        await _userRepository.AddAsync(user);
        await _emailService.SendWelcomeEmailAsync(email);
        _logger.LogInformation("User registered: {Email}", email);
        return Result.Success();
    }
}
```

## Single Responsibility Principle (SRP)

> **"A class should have one, and only one, reason to change."**

A class should have only one responsibility or job. If a class has multiple responsibilities, changes to one responsibility might affect the others.

### Bad Example

```csharp
// ❌ Multiple responsibilities in one class
public class UserManager
{
    public void CreateUser(string email, string password)
    {
        // 1. Validation logic
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email is required");
        
        if (password.Length < 8)
            throw new ArgumentException("Password too short");
        
        // 2. Database logic
        using var connection = new SqlConnection("connection_string");
        var command = new SqlCommand("INSERT INTO Users VALUES (@Email, @Password)", connection);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@Password", password);
        connection.Open();
        command.ExecuteNonQuery();
        
        // 3. Email logic
        var smtpClient = new SmtpClient("smtp.gmail.com");
        smtpClient.Send("from@example.com", email, "Welcome", "Welcome to our app!");
        
        // 4. Logging logic
        File.AppendAllText("log.txt", $"User created: {email} at {DateTime.Now}");
    }
}
```

### Good Example

```csharp
// ✅ Each class has a single responsibility

// 1. Validation
public class UserValidator
{
    public ValidationResult Validate(CreateUserDto dto)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(dto.Email))
            errors.Add("Email is required");
        else if (!IsValidEmail(dto.Email))
            errors.Add("Invalid email format");
        
        if (string.IsNullOrWhiteSpace(dto.Password))
            errors.Add("Password is required");
        else if (dto.Password.Length < 8)
            errors.Add("Password must be at least 8 characters");
        
        return new ValidationResult(errors.Count == 0, errors);
    }
    
    private bool IsValidEmail(string email) => 
        new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email);
}

// 2. Data Access
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}

// 3. Email Service
public class EmailService : IEmailService
{
    private readonly IEmailClient _emailClient;
    private readonly EmailSettings _settings;
    
    public EmailService(IEmailClient emailClient, EmailSettings settings)
    {
        _emailClient = emailClient;
        _settings = settings;
    }
    
    public async Task SendWelcomeEmailAsync(string toEmail)
    {
        var message = new EmailMessage
        {
            To = toEmail,
            From = _settings.FromAddress,
            Subject = "Welcome!",
            Body = "Welcome to our application!"
        };
        
        await _emailClient.SendAsync(message);
    }
}

// 4. User Service - Orchestrates the responsibilities
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;
    private readonly UserValidator _validator;
    
    public UserService(
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<UserService> logger,
        UserValidator validator)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
        _validator = validator;
    }
    
    public async Task<Result<User>> CreateUserAsync(CreateUserDto dto)
    {
        // Validate
        var validationResult = _validator.Validate(dto);
        if (!validationResult.IsValid)
            return Result<User>.Failure(string.Join(", ", validationResult.Errors));
        
        // Check if user exists
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            return Result<User>.Failure("User already exists");
        
        // Create user
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };
        
        await _userRepository.AddAsync(user);
        
        // Send email
        await _emailService.SendWelcomeEmailAsync(user.Email);
        
        // Log
        _logger.LogInformation("User created: {Email}", user.Email);
        
        return Result<User>.Success(user);
    }
    
    private string HashPassword(string password)
    {
        // Use proper password hashing (BCrypt, Argon2, etc.)
        return Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(password)));
    }
}
```

### Benefits of SRP

1. **Easier to Understand**: Each class has a clear purpose
2. **Easier to Test**: Smaller, focused classes are easier to unit test
3. **Less Coupling**: Changes to one responsibility don't affect others
4. **Better Reusability**: Single-purpose classes can be reused in different contexts

## Open/Closed Principle (OCP)

> **"Software entities should be open for extension but closed for modification."**

You should be able to extend a class's behavior without modifying its source code.

### Bad Example

```csharp
// ❌ Need to modify the class every time a new payment method is added
public class PaymentProcessor
{
    public void ProcessPayment(PaymentType type, decimal amount)
    {
        switch (type)
        {
            case PaymentType.CreditCard:
                Console.WriteLine($"Processing credit card payment: ${amount}");
                // Credit card logic
                break;
            
            case PaymentType.PayPal:
                Console.WriteLine($"Processing PayPal payment: ${amount}");
                // PayPal logic
                break;
            
            case PaymentType.BankTransfer:
                Console.WriteLine($"Processing bank transfer: ${amount}");
                // Bank transfer logic
                break;
            
            // Every new payment method requires modifying this class
            default:
                throw new ArgumentException("Unknown payment type");
        }
    }
}
```

### Good Example

```csharp
// ✅ Open for extension, closed for modification

// Base abstraction
public interface IPaymentMethod
{
    Task<PaymentResult> ProcessAsync(decimal amount);
    string GetPaymentMethodName();
}

// Concrete implementations
public class CreditCardPayment : IPaymentMethod
{
    private readonly string _cardNumber;
    private readonly IPaymentGateway _gateway;
    
    public CreditCardPayment(string cardNumber, IPaymentGateway gateway)
    {
        _cardNumber = cardNumber;
        _gateway = gateway;
    }
    
    public async Task<PaymentResult> ProcessAsync(decimal amount)
    {
        // Credit card specific logic
        var maskedCard = MaskCardNumber(_cardNumber);
        Console.WriteLine($"Processing credit card payment: ${amount} with card {maskedCard}");
        
        return await _gateway.ChargeAsync(_cardNumber, amount);
    }
    
    public string GetPaymentMethodName() => "Credit Card";
    
    private string MaskCardNumber(string cardNumber) => 
        $"****-****-****-{cardNumber[^4..]}";
}

public class PayPalPayment : IPaymentMethod
{
    private readonly string _email;
    private readonly IPayPalClient _client;
    
    public PayPalPayment(string email, IPayPalClient client)
    {
        _email = email;
        _client = client;
    }
    
    public async Task<PaymentResult> ProcessAsync(decimal amount)
    {
        Console.WriteLine($"Processing PayPal payment: ${amount} for {_email}");
        return await _client.ProcessPaymentAsync(_email, amount);
    }
    
    public string GetPaymentMethodName() => "PayPal";
}

public class BankTransferPayment : IPaymentMethod
{
    private readonly string _accountNumber;
    private readonly IBankingService _bankingService;
    
    public BankTransferPayment(string accountNumber, IBankingService bankingService)
    {
        _accountNumber = accountNumber;
        _bankingService = bankingService;
    }
    
    public async Task<PaymentResult> ProcessAsync(decimal amount)
    {
        Console.WriteLine($"Processing bank transfer: ${amount} to account {_accountNumber}");
        return await _bankingService.TransferAsync(_accountNumber, amount);
    }
    
    public string GetPaymentMethodName() => "Bank Transfer";
}

// Payment processor that doesn't need modification
public class PaymentProcessor
{
    private readonly ILogger<PaymentProcessor> _logger;
    
    public PaymentProcessor(ILogger<PaymentProcessor> logger)
    {
        _logger = logger;
    }
    
    public async Task<PaymentResult> ProcessPaymentAsync(IPaymentMethod paymentMethod, decimal amount)
    {
        _logger.LogInformation(
            "Processing {PaymentMethod} payment for amount {Amount}", 
            paymentMethod.GetPaymentMethodName(), 
            amount);
        
        try
        {
            var result = await paymentMethod.ProcessAsync(amount);
            
            if (result.IsSuccess)
                _logger.LogInformation("Payment successful: {TransactionId}", result.TransactionId);
            else
                _logger.LogWarning("Payment failed: {Error}", result.Error);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment processing failed");
            return PaymentResult.Failure("Payment processing error");
        }
    }
}

// Usage - Adding new payment method doesn't require modifying existing code
public class CryptoCurrencyPayment : IPaymentMethod
{
    private readonly string _walletAddress;
    private readonly ICryptoService _cryptoService;
    
    public CryptoCurrencyPayment(string walletAddress, ICryptoService cryptoService)
    {
        _walletAddress = walletAddress;
        _cryptoService = cryptoService;
    }
    
    public async Task<PaymentResult> ProcessAsync(decimal amount)
    {
        Console.WriteLine($"Processing crypto payment: ${amount} to wallet {_walletAddress}");
        return await _cryptoService.SendPaymentAsync(_walletAddress, amount);
    }
    
    public string GetPaymentMethodName() => "Cryptocurrency";
}
```

### Another Example: Discount Calculator

```csharp
// Open for extension using Strategy pattern
public interface IDiscountStrategy
{
    decimal CalculateDiscount(decimal amount);
    string GetDiscountName();
}

public class NoDiscount : IDiscountStrategy
{
    public decimal CalculateDiscount(decimal amount) => 0;
    public string GetDiscountName() => "No Discount";
}

public class PercentageDiscount : IDiscountStrategy
{
    private readonly decimal _percentage;
    
    public PercentageDiscount(decimal percentage)
    {
        _percentage = percentage;
    }
    
    public decimal CalculateDiscount(decimal amount) => amount * (_percentage / 100);
    public string GetDiscountName() => $"{_percentage}% Discount";
}

public class FixedAmountDiscount : IDiscountStrategy
{
    private readonly decimal _fixedAmount;
    
    public FixedAmountDiscount(decimal fixedAmount)
    {
        _fixedAmount = fixedAmount;
    }
    
    public decimal CalculateDiscount(decimal amount) => 
        Math.Min(_fixedAmount, amount);
    
    public string GetDiscountName() => $"${_fixedAmount} Off";
}

public class Order
{
    public decimal Amount { get; set; }
    private IDiscountStrategy _discountStrategy = new NoDiscount();
    
    public void SetDiscountStrategy(IDiscountStrategy strategy)
    {
        _discountStrategy = strategy;
    }
    
    public decimal GetTotal()
    {
        var discount = _discountStrategy.CalculateDiscount(Amount);
        return Amount - discount;
    }
}
```

## Liskov Substitution Principle (LSP)

> **"Objects of a superclass should be replaceable with objects of its subclasses without breaking the application."**

Derived classes must be substitutable for their base classes. Subtypes must be behaviorally compatible with their base types.

### Bad Example

```csharp
// ❌ Violates LSP - Square changes Rectangle's behavior
public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }
    
    public int CalculateArea() => Width * Height;
}

public class Square : Rectangle
{
    // Violates LSP: Changing width/height has unexpected side effects
    public override int Width
    {
        get => base.Width;
        set
        {
            base.Width = value;
            base.Height = value; // Side effect!
        }
    }
    
    public override int Height
    {
        get => base.Height;
        set
        {
            base.Height = value;
            base.Width = value; // Side effect!
        }
    }
}

// This breaks with Square
public void ResizeRectangle(Rectangle rectangle)
{
    rectangle.Width = 5;
    rectangle.Height = 10;
    
    // Expected area: 50
    // Actual area with Square: 100 (both dimensions become 10)
    var area = rectangle.CalculateArea();
    Assert.Equal(50, area); // Fails with Square!
}
```

### Good Example

```csharp
// ✅ Proper abstraction that respects LSP
public abstract class Shape
{
    public abstract int CalculateArea();
    public abstract string GetShapeType();
}

public class Rectangle : Shape
{
    public int Width { get; set; }
    public int Height { get; set; }
    
    public Rectangle(int width, int height)
    {
        Width = width;
        Height = height;
    }
    
    public override int CalculateArea() => Width * Height;
    public override string GetShapeType() => "Rectangle";
}

public class Square : Shape
{
    public int Side { get; set; }
    
    public Square(int side)
    {
        Side = side;
    }
    
    public override int CalculateArea() => Side * Side;
    public override string GetShapeType() => "Square";
}

// Works correctly with both shapes
public void PrintShapeArea(Shape shape)
{
    Console.WriteLine($"{shape.GetShapeType()} area: {shape.CalculateArea()}");
}
```

### Real-World Example: File Storage

```csharp
// ❌ Violates LSP
public class FileStorage
{
    public virtual void Save(string filename, byte[] content)
    {
        File.WriteAllBytes(filename, content);
    }
    
    public virtual byte[] Load(string filename)
    {
        return File.ReadAllBytes(filename);
    }
}

public class ReadOnlyFileStorage : FileStorage
{
    public override void Save(string filename, byte[] content)
    {
        // Violates LSP: Changes expected behavior
        throw new NotSupportedException("This storage is read-only");
    }
}

// ✅ Respects LSP
public interface IFileReader
{
    byte[] Load(string filename);
}

public interface IFileWriter
{
    void Save(string filename, byte[] content);
}

public interface IFileStorage : IFileReader, IFileWriter { }

public class FileStorage : IFileStorage
{
    public void Save(string filename, byte[] content)
    {
        File.WriteAllBytes(filename, content);
    }
    
    public byte[] Load(string filename)
    {
        return File.ReadAllBytes(filename);
    }
}

public class ReadOnlyFileStorage : IFileReader
{
    public byte[] Load(string filename)
    {
        return File.ReadAllBytes(filename);
    }
}

// Now classes can depend on what they actually need
public class ReportGenerator
{
    private readonly IFileReader _fileReader; // Only needs reading
    
    public ReportGenerator(IFileReader fileReader)
    {
        _fileReader = fileReader;
    }
}
```

## Interface Segregation Principle (ISP)

> **"No client should be forced to depend on methods it does not use."**

Many specific interfaces are better than one general-purpose interface. Classes shouldn't be forced to implement interfaces they don't use.

### Bad Example

```csharp
// ❌ Fat interface - forces implementations to provide methods they don't need
public interface IWorker
{
    void Work();
    void Eat();
    void Sleep();
    void TakeMeetings();
    void WriteCode();
    void ManageTeam();
}

// Robot doesn't eat or sleep!
public class Robot : IWorker
{
    public void Work() => Console.WriteLine("Robot working");
    public void Eat() => throw new NotSupportedException(); // Forced to implement
    public void Sleep() => throw new NotSupportedException(); // Forced to implement
    public void TakeMeetings() => throw new NotSupportedException();
    public void WriteCode() => Console.WriteLine("Robot coding");
    public void ManageTeam() => throw new NotSupportedException();
}

// Junior developer doesn't manage teams
public class JuniorDeveloper : IWorker
{
    public void Work() => Console.WriteLine("Junior working");
    public void Eat() => Console.WriteLine("Junior eating");
    public void Sleep() => Console.WriteLine("Junior sleeping");
    public void TakeMeetings() => Console.WriteLine("Junior in meeting");
    public void WriteCode() => Console.WriteLine("Junior coding");
    public void ManageTeam() => throw new NotSupportedException(); // Forced to implement
}
```

### Good Example

```csharp
// ✅ Segregated interfaces
public interface IWorkable
{
    void Work();
}

public interface IFeedable
{
    void Eat();
}

public interface ISleepable
{
    void Sleep();
}

public interface IMeetingParticipant
{
    void AttendMeeting();
}

public interface ICoder
{
    void WriteCode();
}

public interface ITeamManager
{
    void ManageTeam();
    void ConductPerformanceReview();
}

// Robot only implements what it needs
public class Robot : IWorkable, ICoder
{
    public void Work() => Console.WriteLine("Robot working");
    public void WriteCode() => Console.WriteLine("Robot coding");
}

// Junior developer implements relevant interfaces
public class JuniorDeveloper : IWorkable, IFeedable, ISleepable, IMeetingParticipant, ICoder
{
    public void Work() => Console.WriteLine("Junior working");
    public void Eat() => Console.WriteLine("Junior eating");
    public void Sleep() => Console.WriteLine("Junior sleeping");
    public void AttendMeeting() => Console.WriteLine("Junior in meeting");
    public void WriteCode() => Console.WriteLine("Junior coding");
}

// Senior developer includes management responsibilities
public class SeniorDeveloper : IWorkable, IFeedable, ISleepable, IMeetingParticipant, ICoder, ITeamManager
{
    public void Work() => Console.WriteLine("Senior working");
    public void Eat() => Console.WriteLine("Senior eating");
    public void Sleep() => Console.WriteLine("Senior sleeping");
    public void AttendMeeting() => Console.WriteLine("Senior in meeting");
    public void WriteCode() => Console.WriteLine("Senior coding");
    public void ManageTeam() => Console.WriteLine("Senior managing team");
    public void ConductPerformanceReview() => Console.WriteLine("Senior conducting review");
}
```

### Real-World Example: Persistence

```csharp
// ❌ Fat interface
public interface IRepository<T>
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
    IEnumerable<T> Search(Expression<Func<T, bool>> predicate);
    int Count();
    void BulkInsert(IEnumerable<T> entities);
    void BulkUpdate(IEnumerable<T> entities);
    void BulkDelete(IEnumerable<int> ids);
}

// ✅ Segregated interfaces
public interface IReadRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
}

public interface IWriteRepository<T>
{
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public interface IBulkRepository<T>
{
    Task BulkInsertAsync(IEnumerable<T> entities);
    Task BulkUpdateAsync(IEnumerable<T> entities);
    Task BulkDeleteAsync(IEnumerable<int> ids);
}

// Now components can depend on only what they need
public class ProductQuery
{
    private readonly IReadRepository<Product> _repository;
    
    public ProductQuery(IReadRepository<Product> repository)
    {
        _repository = repository; // Only needs read operations
    }
}

public class ProductImporter
{
    private readonly IBulkRepository<Product> _repository;
    
    public ProductImporter(IBulkRepository<Product> repository)
    {
        _repository = repository; // Only needs bulk operations
    }
}
```

## Dependency Inversion Principle (DIP)

> **"High-level modules should not depend on low-level modules. Both should depend on abstractions."**
> **"Abstractions should not depend on details. Details should depend on abstractions."**

### Bad Example

```csharp
// ❌ High-level module depends on low-level module
public class EmailNotification
{
    public void Send(string to, string message)
    {
        // Direct dependency on SMTP
        var smtpClient = new SmtpClient("smtp.gmail.com");
        smtpClient.Send("from@example.com", to, "Notification", message);
    }
}

public class OrderService
{
    // Tightly coupled to EmailNotification
    private readonly EmailNotification _emailNotification;
    
    public OrderService()
    {
        _emailNotification = new EmailNotification();
    }
    
    public void ProcessOrder(Order order)
    {
        // Process order logic
        _emailNotification.Send(order.CustomerEmail, "Order processed");
    }
}
```

### Good Example

```csharp
// ✅ Both depend on abstractions

// Abstraction
public interface INotificationService
{
    Task SendAsync(string to, string subject, string message);
}

// Low-level module implements abstraction
public class EmailNotification : INotificationService
{
    private readonly IEmailClient _emailClient;
    private readonly EmailSettings _settings;
    
    public EmailNotification(IEmailClient emailClient, EmailSettings settings)
    {
        _emailClient = emailClient;
        _settings = settings;
    }
    
    public async Task SendAsync(string to, string subject, string message)
    {
        await _emailClient.SendAsync(new EmailMessage
        {
            To = to,
            From = _settings.FromAddress,
            Subject = subject,
            Body = message
        });
    }
}

public class SmsNotification : INotificationService
{
    private readonly ISmsClient _smsClient;
    
    public SmsNotification(ISmsClient smsClient)
    {
        _smsClient = smsClient;
    }
    
    public async Task SendAsync(string to, string subject, string message)
    {
        await _smsClient.SendAsync(to, $"{subject}: {message}");
    }
}

// High-level module depends on abstraction
public class OrderService
{
    private readonly INotificationService _notificationService;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;
    
    public OrderService(
        INotificationService notificationService,
        IOrderRepository orderRepository,
        ILogger<OrderService> logger)
    {
        _notificationService = notificationService;
        _orderRepository = orderRepository;
        _logger = logger;
    }
    
    public async Task<Result> ProcessOrderAsync(Order order)
    {
        try
        {
            await _orderRepository.SaveAsync(order);
            
            await _notificationService.SendAsync(
                order.CustomerEmail,
                "Order Confirmation",
                $"Your order #{order.Id} has been processed");
            
            _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
            return Result.Failure("Order processing failed");
        }
    }
}

// Dependency injection configuration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotifications(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var notificationType = configuration["Notifications:Type"];
        
        switch (notificationType)
        {
            case "Email":
                services.AddScoped<INotificationService, EmailNotification>();
                break;
            case "SMS":
                services.AddScoped<INotificationService, SmsNotification>();
                break;
            default:
                throw new InvalidOperationException("Invalid notification type");
        }
        
        return services;
    }
}
```

### Advanced DIP Example: Repository Pattern

```csharp
// Abstraction in Application layer (high-level)
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
}

// Implementation in Infrastructure layer (low-level)
public class SqlServerOrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    
    public SqlServerOrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
    
    public async Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();
    }
    
    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }
}

// Alternative implementation
public class CosmosDbOrderRepository : IOrderRepository
{
    private readonly CosmosClient _cosmosClient;
    
    // Implementation using Cosmos DB
    public async Task<Order?> GetByIdAsync(Guid id)
    {
        // Cosmos DB specific implementation
        throw new NotImplementedException();
    }
    
    // Other methods...
}
```

## Real-World Examples

### Complete Order Processing System

```csharp
// Domain Layer
public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Total { get; private set; }
    
    public Result Confirm()
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure("Only pending orders can be confirmed");
        
        Status = OrderStatus.Confirmed;
        return Result.Success();
    }
}

// Interfaces (Application Layer)
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
}

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount);
}

public interface INotificationService
{
    Task SendOrderConfirmationAsync(Guid orderId);
}

public interface IInventoryService
{
    Task<bool> ReserveItemsAsync(Guid orderId);
}

// Business Logic (Application Layer) - SRP
public class OrderConfirmationService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderConfirmationService> _logger;
    
    public OrderConfirmationService(
        IOrderRepository orderRepository,
        IPaymentService paymentService,
        IInventoryService inventoryService,
        INotificationService notificationService,
        ILogger<OrderConfirmationService> logger)
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _inventoryService = inventoryService;
        _notificationService = notificationService;
        _logger = logger;
    }
    
    public async Task<Result> ConfirmOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return Result.Failure("Order not found");
        
        // Domain logic
        var confirmResult = order.Confirm();
        if (confirmResult.IsFailure)
            return confirmResult;
        
        // Process payment
        var paymentResult = await _paymentService.ProcessPaymentAsync(orderId, order.Total);
        if (!paymentResult.IsSuccess)
            return Result.Failure("Payment failed");
        
        // Reserve inventory
        var inventoryReserved = await _inventoryService.ReserveItemsAsync(orderId);
        if (!inventoryReserved)
            return Result.Failure("Inventory reservation failed");
        
        // Save order
        await _orderRepository.UpdateAsync(order);
        
        // Send notification
        await _notificationService.SendOrderConfirmationAsync(orderId);
        
        _logger.LogInformation("Order {OrderId} confirmed successfully", orderId);
        return Result.Success();
    }
}

// Infrastructure Implementations - DIP, ISP
public class StripePaymentService : IPaymentService
{
    private readonly IStripeClient _stripeClient;
    
    public async Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount)
    {
        // Stripe-specific implementation
        throw new NotImplementedException();
    }
}

public class EmailNotificationService : INotificationService
{
    private readonly IEmailClient _emailClient;
    
    public async Task SendOrderConfirmationAsync(Guid orderId)
    {
        // Email-specific implementation
        throw new NotImplementedException();
    }
}
```

## Interview Questions

### Conceptual Questions

1. **What are SOLID principles and why are they important?**
   - SOLID is an acronym for five design principles that make software more maintainable, flexible, and scalable.
   - They help reduce coupling, increase cohesion, and make code easier to test and modify.

2. **Explain the difference between SRP and ISP**
   - SRP: A class should have only one reason to change (one responsibility)
   - ISP: Clients shouldn't depend on interfaces they don't use (many small interfaces vs one large)
   - SRP is about classes, ISP is about interfaces

3. **How does DIP enable testability?**
   - By depending on abstractions, you can easily swap implementations
   - In tests, replace real implementations with mocks/stubs
   - Enables testing without external dependencies (databases, APIs, etc.)

4. **What's the relationship between DIP and Dependency Injection?**
   - DIP is a principle: depend on abstractions
   - Dependency Injection is a technique to implement DIP
   - DI containers (like .NET's built-in container) help manage dependencies

### Practical Questions

5. **How would you refactor a God class to follow SRP?**
   - Identify distinct responsibilities
   - Extract each responsibility into its own class
   - Use composition to combine behaviors
   - Apply dependency injection

6. **Give an example of violating LSP**
   ```csharp
   // Violation: Throwing exception in derived class
   public class Bird { public virtual void Fly() { } }
   public class Ostrich : Bird 
   { 
       public override void Fly() => throw new NotSupportedException(); 
   }
   
   // Fix: Rethink hierarchy
   public abstract class Bird { }
   public abstract class FlyingBird : Bird { public abstract void Fly(); }
   public class Sparrow : FlyingBird { public override void Fly() { } }
   public class Ostrich : Bird { } // Doesn't inherit Fly
   ```

7. **When would you violate SOLID principles?**
   - For simple CRUD operations, full SOLID might be over-engineering
   - In prototypes or MVPs where speed is critical
   - When the cost of abstraction exceeds the benefit
   - But always be ready to refactor when complexity grows

8. **How do SOLID principles relate to Clean Architecture?**
   - Clean Architecture is built on SOLID principles
   - DIP ensures layers depend on abstractions
   - SRP and ISP help define layer boundaries
   - OCP allows extending behavior without modifying core layers
   - LSP ensures proper polymorphism across layers

### Code Review Questions

9. **Identify SOLID violations in this code:**
   ```csharp
   public class UserController
   {
       public void CreateUser(string email, string password)
       {
           // Validation
           if (string.IsNullOrEmpty(email)) throw new Exception();
           
           // Database
           var connection = new SqlConnection("...");
           connection.Execute("INSERT...");
           
           // Email
           new SmtpClient().Send("...");
           
           // Logging
           File.AppendAllText("log.txt", "...");
       }
   }
   ```
   
   **Violations:**
   - SRP: Multiple responsibilities (validation, database, email, logging)
   - OCP: Can't extend without modifying
   - DIP: Depends on concrete implementations
   - Hard to test

10. **How would you refactor the above code?**
    - Extract validation into IValidator
    - Extract database into IUserRepository
    - Extract email into IEmailService
    - Use ILogger for logging
    - Inject all dependencies
    - Orchestrate in a service class

## See Also

- [Clean Architecture](./11-clean-architecture.md)
- [Dependency Injection](./13-dependency-injection.md)
- [Repository Pattern](./15-repository-pattern.md)
- [Design Patterns](./Design%20Pattern/)

## Additional Resources

- [SOLID Principles by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2020/10/18/Solid-Relevance.html)
- [Pluralsight: SOLID Principles of Object Oriented Design](https://www.pluralsight.com/courses/principles-oo-design)
- [Clean Code by Robert C. Martin](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)
