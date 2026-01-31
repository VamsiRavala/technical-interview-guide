# C# 12 Features - Complete Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Primary Constructors](#primary-constructors)
3. [Collection Expressions](#collection-expressions)
4. [Inline Arrays](#inline-arrays)
5. [Optional Parameters in Lambda Expressions](#optional-parameters-in-lambda-expressions)
6. [Ref Readonly Parameters](#ref-readonly-parameters)
7. [Alias Any Type](#alias-any-type)
8. [Experimental Attributes](#experimental-attributes)
9. [Best Practices](#best-practices)
10. [Interview Questions](#interview-questions)

## Introduction

C# 12, released with .NET 8 in November 2023, brings significant enhancements focused on improving code clarity, reducing boilerplate, and enhancing performance. This guide explores all major features with practical examples and real-world applications.

## Primary Constructors

### Overview
Primary constructors allow you to add constructor parameters directly in the class or struct declaration, automatically making those parameters available throughout the type without explicit field declarations.

### Basic Syntax

```csharp
// Traditional approach (before C# 12)
public class PersonOld
{
    private readonly string _firstName;
    private readonly string _lastName;
    
    public PersonOld(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
    }
    
    public string FullName => $"{_firstName} {_lastName}";
}

// C# 12: Primary constructor
public class Person(string firstName, string lastName)
{
    public string FullName => $"{firstName} {lastName}";
    
    public string GetInitials() => $"{firstName[0]}.{lastName[0]}.";
}

// Usage
var person = new Person("John", "Doe");
Console.WriteLine(person.FullName);        // John Doe
Console.WriteLine(person.GetInitials());   // J.D.
```

### With Dependency Injection

```csharp
// Perfect for dependency injection scenarios
public class OrderService(
    IOrderRepository repository,
    IEmailService emailService,
    ILogger<OrderService> logger)
{
    public async Task<Order> CreateOrderAsync(OrderRequest request)
    {
        logger.LogInformation("Creating order for {Customer}", request.CustomerId);
        
        var order = new Order
        {
            CustomerId = request.CustomerId,
            Items = request.Items,
            CreatedAt = DateTime.UtcNow
        };
        
        await repository.SaveAsync(order);
        await emailService.SendOrderConfirmationAsync(order);
        
        return order;
    }
    
    public async Task<IEnumerable<Order>> GetOrdersAsync(int customerId)
    {
        logger.LogInformation("Retrieving orders for customer {CustomerId}", customerId);
        return await repository.GetByCustomerAsync(customerId);
    }
}
```

### With Validation

```csharp
public class BankAccount(string accountNumber, decimal initialBalance)
{
    private decimal _balance = initialBalance >= 0 
        ? initialBalance 
        : throw new ArgumentException("Initial balance cannot be negative", nameof(initialBalance));
    
    public string AccountNumber { get; } = !string.IsNullOrWhiteSpace(accountNumber)
        ? accountNumber
        : throw new ArgumentException("Account number cannot be empty", nameof(accountNumber));
    
    public decimal Balance => _balance;
    
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive");
        
        _balance += amount;
        Console.WriteLine($"Deposited {amount:C} to {accountNumber}. New balance: {_balance:C}");
    }
    
    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive");
        if (amount > _balance)
            throw new InvalidOperationException("Insufficient funds");
        
        _balance -= amount;
        Console.WriteLine($"Withdrew {amount:C} from {accountNumber}. New balance: {_balance:C}");
    }
}
```

### Structs with Primary Constructors

```csharp
// Perfect for value types
public readonly struct Point(double x, double y)
{
    public double X { get; } = x;
    public double Y { get; } = y;
    
    public double DistanceFromOrigin() => Math.Sqrt(x * x + y * y);
    
    public Point Translate(double dx, double dy) => new(x + dx, y + dy);
    
    public override string ToString() => $"({x}, {y})";
}

public readonly struct Color(byte red, byte green, byte blue)
{
    public byte R { get; } = red;
    public byte G { get; } = green;
    public byte B { get; } = blue;
    
    public string ToHex() => $"#{red:X2}{green:X2}{blue:X2}";
    
    public Color Lighten(byte amount) => new(
        Math.Min((byte)(red + amount), (byte)255),
        Math.Min((byte)(green + amount), (byte)255),
        Math.Min((byte)(blue + amount), (byte)255)
    );
}

// Usage
var point = new Point(3, 4);
Console.WriteLine($"Distance: {point.DistanceFromOrigin()}");
var moved = point.Translate(1, 1);
Console.WriteLine($"Moved to: {moved}");

var color = new Color(100, 150, 200);
Console.WriteLine($"Hex: {color.ToHex()}");
```

## Collection Expressions

### Overview
Collection expressions provide a unified, concise syntax for creating and initializing collections of any type, using square brackets `[]`.

### Basic Syntax

```csharp
// Empty collection
int[] emptyArray = [];
List<string> emptyList = [];
Span<int> emptySpan = [];

// Initialized collections
int[] numbers = [1, 2, 3, 4, 5];
List<string> names = ["Alice", "Bob", "Charlie"];
Span<int> span = [10, 20, 30, 40];

// Before C# 12 (comparison)
int[] oldNumbers = new[] { 1, 2, 3, 4, 5 };
List<string> oldNames = new List<string> { "Alice", "Bob", "Charlie" };
```

### Spread Operator

```csharp
// Combining collections with spread operator (..)
int[] first = [1, 2, 3];
int[] second = [4, 5, 6];
int[] combined = [..first, ..second];  // [1, 2, 3, 4, 5, 6]

// Adding elements before and after
int[] extended = [0, ..first, ..second, 7, 8];  // [0, 1, 2, 3, 4, 5, 6, 7, 8]

// With different collection types
List<string> list1 = ["A", "B"];
string[] array1 = ["C", "D"];
List<string> merged = [..list1, ..array1, "E"];  // ["A", "B", "C", "D", "E"]
```

### Practical Examples

```csharp
// Building query results
public class UserService
{
    public List<User> GetAllUsers(bool includeAdmins)
    {
        User[] regularUsers = GetRegularUsers();
        User[] admins = GetAdminUsers();
        
        return includeAdmins 
            ? [..regularUsers, ..admins]
            : [..regularUsers];
    }
    
    public int[] GetScores(int[] baseScores, int bonusScore)
    {
        return [..baseScores, bonusScore];
    }
}

// Creating test data
public static class TestData
{
    public static List<Product> GetTestProducts() =>
    [
        new Product { Id = 1, Name = "Laptop", Price = 999.99m },
        new Product { Id = 2, Name = "Mouse", Price = 29.99m },
        new Product { Id = 3, Name = "Keyboard", Price = 79.99m }
    ];
    
    public static int[] GetSampleNumbers() => [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
}

// Filtering and combining
public static int[] FilterAndCombine(int[] source, Predicate<int> predicate)
{
    var filtered = source.Where(x => predicate(x)).ToArray();
    var defaults = [0, -1];
    return [..defaults, ..filtered];
}

// Usage
var numbers = [5, 10, 15, 20, 25];
var result = FilterAndCombine(numbers, x => x > 10);  // [0, -1, 15, 20, 25]
```

### With Spans for Performance

```csharp
public class HighPerformanceProcessor
{
    // Zero-allocation span creation
    public int ProcessNumbers(ReadOnlySpan<int> input)
    {
        Span<int> buffer = [..input];  // Copy to mutable span
        
        // Process in place
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] *= 2;
        }
        
        return buffer[0];
    }
    
    // Creating spans from multiple sources
    public ReadOnlySpan<byte> CombineBuffers(
        ReadOnlySpan<byte> header,
        ReadOnlySpan<byte> body,
        ReadOnlySpan<byte> footer)
    {
        Span<byte> result = stackalloc byte[header.Length + body.Length + footer.Length];
        
        header.CopyTo(result);
        body.CopyTo(result[header.Length..]);
        footer.CopyTo(result[(header.Length + body.Length)..]);
        
        return result;
    }
}
```

## Inline Arrays

### Overview
Inline arrays allow you to create fixed-size arrays directly within structs with minimal overhead and optimal performance, perfect for high-performance scenarios.

### Basic Declaration

```csharp
// C# 12: Inline array
[System.Runtime.CompilerServices.InlineArray(10)]
public struct Buffer10<T>
{
    private T _element0;
    
    // Compiler generates indexer access automatically
}

// Usage
Buffer10<int> buffer;
buffer[0] = 100;
buffer[9] = 200;

for (int i = 0; i < 10; i++)
{
    buffer[i] = i * 10;
}

// Can be used with spans
Span<int> span = buffer;
Console.WriteLine(span[5]);  // 50
```

### Matrix Implementation

```csharp
[InlineArray(9)]
public struct Matrix3x3
{
    private double _element;
}

public class MatrixOperations
{
    public static Matrix3x3 CreateIdentity()
    {
        Matrix3x3 matrix = default;
        matrix[0] = 1; // [0,0]
        matrix[4] = 1; // [1,1]
        matrix[8] = 1; // [2,2]
        return matrix;
    }
    
    public static void PrintMatrix(Matrix3x3 matrix)
    {
        for (int i = 0; i < 9; i++)
        {
            Console.Write($"{matrix[i],6:F2}");
            if ((i + 1) % 3 == 0)
                Console.WriteLine();
        }
    }
    
    public static Matrix3x3 Multiply(Matrix3x3 a, Matrix3x3 b)
    {
        Matrix3x3 result = default;
        
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                double sum = 0;
                for (int k = 0; k < 3; k++)
                {
                    sum += a[row * 3 + k] * b[k * 3 + col];
                }
                result[row * 3 + col] = sum;
            }
        }
        
        return result;
    }
}
```

### High-Performance Buffer

```csharp
[InlineArray(256)]
public struct PacketBuffer
{
    private byte _element;
}

public class NetworkPacket
{
    private PacketBuffer _data;
    private int _length;
    
    public void WriteHeader(byte version, byte flags)
    {
        _data[0] = version;
        _data[1] = flags;
        _length = 2;
    }
    
    public void WriteData(ReadOnlySpan<byte> data)
    {
        if (_length + data.Length > 256)
            throw new InvalidOperationException("Buffer overflow");
        
        Span<byte> buffer = _data;
        data.CopyTo(buffer[_length..]);
        _length += data.Length;
    }
    
    public ReadOnlySpan<byte> GetPacket()
    {
        Span<byte> buffer = _data;
        return buffer[.._length];
    }
}

// Usage
var packet = new NetworkPacket();
packet.WriteHeader(1, 0x80);
packet.WriteData("Hello"u8);
ReadOnlySpan<byte> data = packet.GetPacket();
```

### Real-World Example: Color Palette

```csharp
[InlineArray(16)]
public struct ColorPalette
{
    private uint _color;  // ARGB format
}

public class PaletteManager
{
    public static ColorPalette CreateVGA16()
    {
        ColorPalette palette = default;
        
        // Standard VGA 16-color palette
        uint[] colors = 
        [
            0xFF000000, 0xFF0000AA, 0xFF00AA00, 0xFF00AAAA,
            0xFFAA0000, 0xFFAA00AA, 0xFFAA5500, 0xFFAAAAAA,
            0xFF555555, 0xFF5555FF, 0xFF55FF55, 0xFF55FFFF,
            0xFFFF5555, 0xFFFF55FF, 0xFFFFFF55, 0xFFFFFFFF
        ];
        
        for (int i = 0; i < 16; i++)
        {
            palette[i] = colors[i];
        }
        
        return palette;
    }
    
    public static uint GetColor(ColorPalette palette, int index)
    {
        if (index < 0 || index >= 16)
            throw new ArgumentOutOfRangeException(nameof(index));
        
        return palette[index];
    }
}
```

## Optional Parameters in Lambda Expressions

### Overview
C# 12 allows lambda expressions to have optional parameters with default values, making them more flexible and reducing the need for overloads.

### Basic Syntax

```csharp
// Lambda with optional parameters
var greet = (string name, string greeting = "Hello") => $"{greeting}, {name}!";

Console.WriteLine(greet("Alice"));              // Hello, Alice!
Console.WriteLine(greet("Bob", "Hi"));          // Hi, Bob!
Console.WriteLine(greet("Charlie", "Welcome")); // Welcome, Charlie!

// Multiple optional parameters
var calculate = (int a, int b = 10, int c = 5) => a + b + c;

Console.WriteLine(calculate(1));         // 16 (1 + 10 + 5)
Console.WriteLine(calculate(1, 2));      // 8  (1 + 2 + 5)
Console.WriteLine(calculate(1, 2, 3));   // 6  (1 + 2 + 3)
```

### With LINQ

```csharp
public class ProductCatalog
{
    private List<Product> _products = new();
    
    public void AddProducts()
    {
        // Using lambda with optional parameter in Where clause
        var filterByPrice = (Product p, decimal maxPrice = 100m) => p.Price <= maxPrice;
        
        var affordableProducts = _products.Where(p => filterByPrice(p)).ToList();
        var premiumProducts = _products.Where(p => filterByPrice(p, 1000m)).ToList();
        
        // With Select
        var formatter = (Product p, string format = "standard") => format switch
        {
            "detailed" => $"{p.Name} - ${p.Price:F2} ({p.Category})",
            "simple" => $"{p.Name} - ${p.Price:F2}",
            _ => p.Name
        };
        
        var standardFormatted = _products.Select(p => formatter(p)).ToList();
        var detailedFormatted = _products.Select(p => formatter(p, "detailed")).ToList();
    }
}
```

### Event Handlers with Optional Parameters

```csharp
public class Button
{
    public event Action<string, bool>? Clicked;
    
    public void Click(string source)
    {
        // Lambda with optional parameter for logging
        var logger = (string message, bool verbose = false) =>
        {
            Console.WriteLine(verbose 
                ? $"[{DateTime.Now:HH:mm:ss}] {message}" 
                : message);
        };
        
        logger($"Button clicked from {source}");
        logger($"Button clicked from {source}", verbose: true);
        
        Clicked?.Invoke(source, false);
    }
}
```

### Async Lambdas with Optional Parameters

```csharp
public class DataLoader
{
    public async Task LoadDataAsync()
    {
        // Async lambda with optional parameters
        var fetchData = async (string url, int timeout = 30, CancellationToken ct = default) =>
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(timeout) };
            return await client.GetStringAsync(url, ct);
        };
        
        // Use with defaults
        var data1 = await fetchData("https://api.example.com/data");
        
        // Use with custom timeout
        var data2 = await fetchData("https://api.example.com/large-data", 60);
        
        // Use with cancellation token
        var cts = new CancellationTokenSource();
        var data3 = await fetchData("https://api.example.com/data", ct: cts.Token);
    }
}
```

### Factory Pattern with Optional Parameters

```csharp
public class ConfigurationFactory
{
    public Configuration CreateConfiguration()
    {
        // Lambda factory with optional parameters
        var createSection = (
            string name, 
            string value,
            bool encrypted = false,
            string? description = null) =>
        {
            return new ConfigSection
            {
                Name = name,
                Value = encrypted ? Encrypt(value) : value,
                IsEncrypted = encrypted,
                Description = description ?? $"Configuration for {name}"
            };
        };
        
        return new Configuration
        {
            Sections = new[]
            {
                createSection("Database", "Server=localhost;"),
                createSection("ApiKey", "secret-key", encrypted: true),
                createSection("Timeout", "30", description: "Request timeout in seconds")
            }
        };
    }
    
    private string Encrypt(string value) => Convert.ToBase64String(
        System.Text.Encoding.UTF8.GetBytes(value));
}
```

## Ref Readonly Parameters

### Overview
`ref readonly` parameters allow passing references to values without allowing modifications, combining the performance benefits of `ref` with the safety of `readonly`. This is especially useful for large structs.

### Basic Syntax

```csharp
// Large struct that's expensive to copy
public readonly struct LargeData
{
    public readonly double[] Values;
    public readonly string Description;
    public readonly DateTime Timestamp;
    
    public LargeData(int size)
    {
        Values = new double[size];
        Description = $"Data set with {size} values";
        Timestamp = DateTime.UtcNow;
    }
}

// Method with ref readonly parameter
public class DataProcessor
{
    // Efficient: Passes by reference, prevents modification
    public double CalculateAverage(ref readonly LargeData data)
    {
        // data = new LargeData(10); // Compilation error: Cannot assign
        return data.Values.Length > 0 ? data.Values.Average() : 0;
    }
    
    // Compare with different approaches
    public double CalculateAverageByValue(LargeData data) // Slow: Copies entire struct
        => data.Values.Length > 0 ? data.Values.Average() : 0;
    
    public double CalculateAverageByRef(ref LargeData data) // Fast but unsafe: Caller can modify
        => data.Values.Length > 0 ? data.Values.Average() : 0;
    
    public double CalculateAverageByIn(in LargeData data) // Similar to ref readonly
        => data.Values.Length > 0 ? data.Values.Average() : 0;
}

// Usage
var data = new LargeData(1000);
var processor = new DataProcessor();
double avg = processor.CalculateAverage(ref readonly data);
```

### Performance Comparison

```csharp
public readonly struct Matrix4x4
{
    private readonly float m11, m12, m13, m14;
    private readonly float m21, m22, m23, m24;
    private readonly float m31, m32, m33, m34;
    private readonly float m41, m42, m43, m44;
    
    // 16 floats = 64 bytes
    
    public Matrix4x4(float value)
    {
        m11 = m22 = m33 = m44 = value;
        m12 = m13 = m14 = m21 = m23 = m24 = 0;
        m31 = m32 = m34 = m41 = m42 = m43 = 0;
    }
}

public class MatrixOperations
{
    // Best practice: Use ref readonly for large structs
    public static float GetDeterminant(ref readonly Matrix4x4 matrix)
    {
        // Calculate determinant without copying 64 bytes
        return 1.0f; // Simplified
    }
    
    // Combine two matrices
    public static Matrix4x4 Multiply(ref readonly Matrix4x4 a, ref readonly Matrix4x4 b)
    {
        // Efficient multiplication without copying parameters
        return new Matrix4x4(1.0f); // Simplified
    }
}
```

### Real-World Example: Physics Engine

```csharp
public readonly struct RigidBody
{
    public readonly Vector3 Position;
    public readonly Vector3 Velocity;
    public readonly Quaternion Rotation;
    public readonly float Mass;
    public readonly Vector3 AngularVelocity;
    
    public RigidBody(Vector3 position, float mass)
    {
        Position = position;
        Mass = mass;
        Velocity = Vector3.Zero;
        Rotation = Quaternion.Identity;
        AngularVelocity = Vector3.Zero;
    }
}

public class PhysicsEngine
{
    public void ApplyForce(ref readonly RigidBody body, ref readonly Vector3 force, float deltaTime)
    {
        // Efficiently reads large struct without copying
        var acceleration = force / body.Mass;
        var newVelocity = body.Velocity + acceleration * deltaTime;
        var newPosition = body.Position + newVelocity * deltaTime;
        
        Console.WriteLine($"New position: {newPosition}");
    }
    
    public bool CheckCollision(ref readonly RigidBody a, ref readonly RigidBody b)
    {
        // Efficient collision detection
        var distance = Vector3.Distance(a.Position, b.Position);
        return distance < 1.0f; // Simplified
    }
    
    public RigidBody UpdatePhysics(ref readonly RigidBody body, float deltaTime)
    {
        // Gravity
        var gravity = new Vector3(0, -9.81f, 0);
        var newVelocity = body.Velocity + gravity * deltaTime;
        var newPosition = body.Position + newVelocity * deltaTime;
        
        return new RigidBody(newPosition, body.Mass);
    }
}
```

## Alias Any Type

### Overview
C# 12 extends the `using` alias directive to support any type, including tuples, pointers, and array types, making code more readable when dealing with complex type signatures.

### Basic Syntax

```csharp
// Alias for tuple types
using Point = (double X, double Y);
using Point3D = (double X, double Y, double Z);
using ColorRGB = (byte R, byte G, byte B);

// Alias for complex generic types
using UserCache = System.Collections.Generic.Dictionary<int, (string Name, string Email)>;
using OrderMap = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>;

// Alias for array types
using ByteArray = byte[];
using Matrix = double[][];

// Usage
public class GeometryService
{
    public Point CreatePoint(double x, double y) => (x, y);
    
    public double Distance(Point p1, Point p2)
    {
        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
    
    public Point3D To3D(Point point, double z) => (point.X, point.Y, z);
}

public class UserService
{
    private UserCache _cache = new();
    
    public void AddUser(int id, string name, string email)
    {
        _cache[id] = (name, email);
    }
    
    public (string Name, string Email)? GetUser(int id)
    {
        return _cache.TryGetValue(id, out var user) ? user : null;
    }
}
```

### Complex Type Aliases

```csharp
// Alias for nested generics
using StudentGrades = System.Collections.Generic.Dictionary<
    string, // Student ID
    System.Collections.Generic.Dictionary<
        string, // Course Code
        (double Grade, string LetterGrade)
    >
>;

using EventHandlers = System.Collections.Generic.List<
    System.Func<object, System.EventArgs, System.Threading.Tasks.Task>
>;

public class GradeManager
{
    private StudentGrades _grades = new();
    
    public void AddGrade(string studentId, string courseCode, double grade)
    {
        if (!_grades.ContainsKey(studentId))
        {
            _grades[studentId] = new();
        }
        
        string letterGrade = grade switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _ => "F"
        };
        
        _grades[studentId][courseCode] = (grade, letterGrade);
    }
    
    public double? GetGPA(string studentId)
    {
        if (!_grades.TryGetValue(studentId, out var courses))
            return null;
        
        return courses.Values.Average(g => g.Grade) / 25.0; // Simplified GPA calculation
    }
}
```

### Pointer Type Aliases (Unsafe)

```csharp
// Alias for pointer types
using IntPtr = int*;
using CharPtr = char*;
using VoidPtr = void*;

public unsafe class UnsafeOperations
{
    public void ProcessBuffer(IntPtr buffer, int length)
    {
        for (int i = 0; i < length; i++)
        {
            buffer[i] *= 2;
        }
    }
    
    public string ReadString(CharPtr ptr)
    {
        var sb = new System.Text.StringBuilder();
        while (*ptr != '\0')
        {
            sb.Append(*ptr);
            ptr++;
        }
        return sb.ToString();
    }
}
```

### Functional Programming Aliases

```csharp
// Alias for function types
using Predicate = System.Func<int, bool>;
using Transform = System.Func<string, string>;
using AsyncOperation = System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<bool>>;
using ResultHandler = System.Action<bool, string?>;

public class FunctionalProcessor
{
    public IEnumerable<int> Filter(IEnumerable<int> source, Predicate predicate)
    {
        return source.Where(x => predicate(x));
    }
    
    public IEnumerable<string> Map(IEnumerable<string> source, Transform transform)
    {
        return source.Select(s => transform(s));
    }
    
    public async Task ExecuteAsync(AsyncOperation operation, ResultHandler onComplete)
    {
        try
        {
            var result = await operation(CancellationToken.None);
            onComplete(result, null);
        }
        catch (Exception ex)
        {
            onComplete(false, ex.Message);
        }
    }
}
```

## Experimental Attributes

### Overview
C# 12 introduces the `[Experimental]` attribute to mark APIs as experimental, helping developers understand that certain features may change or be removed in future versions.

### Basic Usage

```csharp
// Mark an experimental feature
[Experimental("EXP001")]
public class ExperimentalFeature
{
    public void NewMethod()
    {
        Console.WriteLine("This is experimental");
    }
}

// Suppress warning for known experimental usage
#pragma warning disable EXP001
var feature = new ExperimentalFeature();
feature.NewMethod();
#pragma warning restore EXP001

// Or use attribute to suppress
[Experimental("EXP001")]
public class TestingExperimental
{
    public void Test()
    {
        var feature = new ExperimentalFeature(); // No warning
        feature.NewMethod();
    }
}
```

### Library Development Example

```csharp
// Experimental API in a library
namespace MyLibrary
{
    [Experimental("MYLIB001")]
    public class NewAlgorithm
    {
        public int[] Sort(int[] array)
        {
            // New sorting algorithm being tested
            return array.OrderBy(x => x).ToArray();
        }
    }
    
    [Experimental("MYLIB002")]
    public interface IAsyncCache
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
    }
    
    // Stable API
    public class StandardSort
    {
        public int[] Sort(int[] array)
        {
            Array.Sort(array);
            return array;
        }
    }
}
```

### Gradual API Evolution

```csharp
public class DataService
{
    // Old stable API
    public string GetData(int id)
    {
        return $"Data for {id}";
    }
    
    // New experimental API
    [Experimental("DS001")]
    public async Task<Result<string>> GetDataAsync(int id, CancellationToken ct = default)
    {
        await Task.Delay(100, ct);
        return Result<string>.Success($"Data for {id}");
    }
    
    // Experimental feature flag
    [Experimental("DS002")]
    public bool EnableCaching { get; set; }
}

public class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}
```

## Best Practices

### 1. Primary Constructors
- Use for dependency injection scenarios - reduces boilerplate significantly
- Add validation in property initializers or constructor body when needed
- Prefer primary constructors for immutable types and simple classes
- Be aware that parameters are captured and may extend object lifetime

### 2. Collection Expressions
- Use `[]` syntax for better readability and consistency
- Leverage spread operator `..` for combining collections efficiently
- Prefer collection expressions over traditional initialization syntax
- Use with Spans for zero-allocation scenarios

### 3. Inline Arrays
- Use for fixed-size buffers in performance-critical code
- Perfect for interop scenarios and low-level programming
- Remember they're value types - copying copies all elements
- Use with Span<T> for safe, efficient access

### 4. Ref Readonly Parameters
- Use for structs larger than 16 bytes to avoid copying
- Prefer over `in` parameters for clarity (they're similar)
- Document when methods require ref readonly for performance
- Combine with readonly structs for maximum safety

### 5. Type Aliases
- Use to simplify complex generic type signatures
- Create aliases for commonly used tuple types
- Keep aliases close to their usage (same file or namespace)
- Use meaningful names that convey purpose

### 6. Experimental Attributes
- Always mark unstable APIs with `[Experimental]`
- Use unique diagnostic IDs for each experimental feature
- Document migration path from experimental to stable
- Review and remove attribute when API stabilizes

## Interview Questions

### Question 1: Primary Constructors
**Q:** What are the key differences between primary constructors and traditional constructors in C# 12?

**A:** Key differences include:
- **Syntax**: Primary constructors are declared in the type declaration itself
- **Field generation**: Parameters don't automatically become fields - they're captured variables
- **Lifetime**: Primary constructor parameters live as long as the object (captured in closures)
- **Validation**: Can be done in property initializers or additional constructor bodies
- **Use cases**: Best for DI scenarios, simple classes, and reducing boilerplate
- **Limitations**: Cannot be combined with field initializers using those same names
- **Memory**: May use slightly more memory due to captured variables vs explicit fields

### Question 2: Collection Expressions vs Traditional
**Q:** What are the performance implications of collection expressions compared to traditional collection initialization?

**A:** Performance characteristics:
- **Compile-time optimization**: Compiler can optimize collection expressions better
- **Type inference**: Reduces runtime type checks
- **Spread operator**: Generally efficient, but multiple spreads can allocate intermediary collections
- **Span creation**: Collection expressions with Spans can be stack-allocated (zero heap allocation)
- **Array initialization**: Similar performance to array initializer syntax
- **List initialization**: May be slightly faster due to better size prediction
Overall, collection expressions have equal or better performance in most scenarios.

### Question 3: Inline Arrays Use Cases
**Q:** When should you use inline arrays instead of regular arrays or Span<T>?

**A:** Use inline arrays when:
- **Fixed size known at compile time**: Size never changes
- **High-performance scenarios**: Need value-type semantics without heap allocation
- **Interop**: Working with native code expecting fixed-size buffers
- **Embedding data**: Need to embed fixed-size data directly in a struct
- **Cache-friendly**: Data locality is critical for performance

Don't use when:
- Size needs to be dynamic
- Need reference semantics
- Size is large (>256 bytes - causes stack overflow risk)
- Copying cost is too high

### Question 4: Ref Readonly Performance
**Q:** Explain the performance difference between `ref readonly`, `in`, and passing by value for structs.

**A:** Performance comparison:
```csharp
// By value: Copies entire struct (expensive for large structs)
void Method1(LargeStruct s) { } // 64-byte copy

// By ref readonly: Passes reference, prevents modification (8 bytes on 64-bit)
void Method2(ref readonly LargeStruct s) { } // Just pointer

// By in: Similar to ref readonly, slightly different semantics
void Method3(in LargeStruct s) { } // Just pointer
```

- **By value**: O(n) where n is struct size - causes copying overhead
- **Ref readonly/in**: O(1) - just passes pointer (8 bytes)
- **Difference**: `ref readonly` requires ref at call site, `in` doesn't
- **When to use**: Use ref readonly for structs >16 bytes that shouldn't be modified

### Question 5: Type Aliases Scenarios
**Q:** Provide real-world scenarios where type aliases significantly improve code maintainability.

**A:** Key scenarios:
1. **Complex generics**: `using UserRepository = Dictionary<Guid, (User user, DateTime lastAccess)>;`
2. **Tuple types**: `using Coordinate = (double Latitude, double Longitude);`
3. **Function signatures**: `using ValidationRule = Func<string, (bool isValid, string? error)>;`
4. **Pointer types**: `using BytePtr = byte*;` in unsafe code
5. **Nested collections**: `using GraphAdjacencyList = Dictionary<int, List<(int node, double weight)>>;`

Benefits: Single point of change, improved readability, easier refactoring, self-documenting code.

### Question 6: Lambda Optional Parameters
**Q:** How do optional parameters in lambda expressions differ from regular method optional parameters?

**A:** Key differences:
- **Type inference**: Lambdas with optional parameters require explicit parameter types
- **Overload resolution**: Lambdas with optionals don't create multiple overloads
- **Delegate compatibility**: Must match delegate signature exactly
- **Default value evaluation**: Evaluated at call site, not at definition
- **Syntax**: `(Type param = default) => ...` vs method `Method(Type param = default)`

Similarities:
- Same default value rules apply
- Can use named arguments at call site
- Can mix required and optional parameters

### Question 7: Experimental Attribute Strategy
**Q:** What's the best strategy for managing experimental APIs in a production library?

**A:** Best practices:
1. **Clear diagnostic IDs**: Use consistent, documented IDs (e.g., "LIB001", "LIB002")
2. **Separate namespaces**: Put experimental APIs in `.Experimental` namespaces
3. **Documentation**: Clearly document experimental status, purpose, and planned changes
4. **Versioning**: Plan removal or stabilization for specific version
5. **Feature flags**: Consider runtime feature flags for experimental behavior
6. **Migration path**: Document how to migrate when API stabilizes
7. **Breaking changes**: Communicate that breaking changes are expected
8. **Feedback**: Provide channels for users to give feedback

### Question 8: Collection Expression Spread
**Q:** What are the performance considerations when using multiple spread operators in collection expressions?

**A:** Considerations:
```csharp
// Single spread: Efficient
var result = [..source1, ..source2];

// Multiple spreads with modifications: May create intermediate collections
var result = [0, ..source1.Select(x => x * 2), ..source2.Where(x => x > 0), 100];
```

- **Size prediction**: Compiler can't always predict final size, may reallocate
- **Iteration**: Each spread iterates its source once
- **Intermediate allocations**: LINQ operations in spreads create intermediate enumerables
- **Optimization**: Compiler optimizes known collection types (arrays, List<T>)
- **Best practice**: Prefer spreading existing collections over spreading LINQ queries

### Question 9: Primary Constructor Lifetime
**Q:** Explain the lifetime implications of primary constructor parameters vs explicit fields.

**A:** Lifetime differences:
```csharp
// Primary constructor - parameters captured
public class Service(ILogger logger, HttpClient client)
{
    public void Log() => logger.LogInformation("Test"); // Captures logger
    // logger and client live as long as Service instance
}

// Explicit fields
public class Service
{
    private readonly ILogger _logger;
    
    public Service(ILogger logger)
    {
        _logger = logger; // Explicit field, same lifetime
    }
}
```

- **Primary constructor params**: Live as long as object (captured in closures)
- **Explicit fields**: Same lifetime, but explicit control
- **Memory impact**: Primary constructors may use slightly more memory
- **GC impact**: Both prevent GC of referenced objects equally
- **Best practice**: No practical difference for most scenarios

### Question 10: Inline Array Safety
**Q:** What are the safety considerations when working with inline arrays?

**A:** Safety concerns:
1. **Bounds checking**: Indexer access is bounds-checked, but Span conversion isn't
2. **Stack overflow**: Large inline arrays can cause stack overflow
3. **Copying cost**: Assigning inline array copies all elements
4. **Unsafe usage**: Easy to convert to Span and then unsafe pointer
5. **Initialization**: Default initialization may not be obvious for complex types
6. **Thread safety**: No built-in synchronization for concurrent access

Safe practices:
- Keep inline arrays small (<256 bytes)
- Use with Span<T> for safe, efficient access
- Validate indices when using indexer
- Document size limitations
- Consider readonly for immutability

---

**Last Updated: January 2026 - .NET 9**