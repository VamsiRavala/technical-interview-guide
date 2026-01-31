# C# 13 Features - Complete Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Params Collections](#params-collections)
3. [New Escape Sequence \e](#new-escape-sequence-e)
4. [Method Group Natural Types](#method-group-natural-types)
5. [Implicit Indexer Access in Object Initializers](#implicit-indexer-access-in-object-initializers)
6. [Ref and Unsafe in Iterators and Async](#ref-and-unsafe-in-iterators-and-async)
7. [Allows Ref Struct Interfaces](#allows-ref-struct-interfaces)
8. [More Partial Members](#more-partial-members)
9. [Best Practices](#best-practices)
10. [Interview Questions](#interview-questions)

## Introduction

C# 13, released with .NET 9 in November 2024, introduces several powerful features that enhance developer productivity, improve performance, and expand the capabilities of the language. This guide covers all major features with practical examples and real-world use cases.

## Params Collections

### Overview
C# 13 extends the `params` keyword to work with any collection type that supports collection expressions, not just arrays. This includes `Span<T>`, `ReadOnlySpan<T>`, `List<T>`, `IEnumerable<T>`, and custom collection types.

### Syntax and Examples

```csharp
// Traditional params with array (C# 1.0+)
public void LogMessagesArray(params string[] messages)
{
    foreach (var msg in messages)
        Console.WriteLine(msg);
}

// C# 13: params with Span<T> (better performance, no heap allocation)
public void LogMessagesSpan(params ReadOnlySpan<char> message)
{
    Console.WriteLine(message);
}

// C# 13: params with List<T>
public void ProcessNumbers(params List<int> numbers)
{
    numbers.Sort();
    Console.WriteLine($"Sorted: {string.Join(", ", numbers)}");
}

// C# 13: params with IEnumerable<T>
public void FilterItems(params IEnumerable<string> items)
{
    var filtered = items.Where(x => x.Length > 3);
    foreach (var item in filtered)
        Console.WriteLine(item);
}

// C# 13: params with custom collection type
public class CustomCollection<T> : IEnumerable<T>
{
    private readonly List<T> _items = new();
    
    public void Add(T item) => _items.Add(item);
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public void ProcessCustom(params CustomCollection<int> numbers)
{
    foreach (var num in numbers)
        Console.WriteLine(num * 2);
}

// Usage
LogMessagesSpan("Hello World");
ProcessNumbers(1, 5, 3, 2, 4);
FilterItems("a", "test", "hello", "hi");
ProcessCustom(10, 20, 30);
```

### Performance Benefits

```csharp
// Performance comparison: Array vs Span
public class ParamsPerformance
{
    // Old way: Allocates array on heap
    public int SumArray(params int[] numbers)
    {
        return numbers.Sum();
    }
    
    // C# 13: No heap allocation for small collections
    public int SumSpan(params ReadOnlySpan<int> numbers)
    {
        int sum = 0;
        foreach (var num in numbers)
            sum += num;
        return sum;
    }
}

// Benchmark results (example):
// SumArray(1,2,3,4,5): ~50ns, 40 bytes allocated
// SumSpan(1,2,3,4,5):  ~10ns, 0 bytes allocated
```

### Real-World Use Case

```csharp
public class LoggingService
{
    // Efficient logging with structured data
    public void LogEvent(string eventName, params ReadOnlySpan<KeyValuePair<string, object>> properties)
    {
        Console.WriteLine($"Event: {eventName}");
        foreach (var prop in properties)
        {
            Console.WriteLine($"  {prop.Key}: {prop.Value}");
        }
    }
}

// Usage
var logger = new LoggingService();
logger.LogEvent("UserLogin", 
    new("UserId", 123),
    new("Timestamp", DateTime.UtcNow),
    new("IpAddress", "192.168.1.1")
);
```

## New Escape Sequence \e

### Overview
C# 13 introduces the `\e` escape sequence as a shorthand for the escape character (ESC, ASCII 27), commonly used for ANSI terminal color codes and terminal control sequences.

### Syntax and Examples

```csharp
// Before C# 13: Using \x1b or \u001b
string redTextOld = "\x1b[31mError\x1b[0m";
string blueTextOld = "\u001b[34mInfo\u001b[0m";

// C# 13: Using \e (more readable)
string redText = "\e[31mError\e[0m";
string blueText = "\e[34mInfo\e[0m";
string greenText = "\e[32mSuccess\e[0m";
string yellowText = "\e[33mWarning\e[0m";

Console.WriteLine(redText);
Console.WriteLine(blueText);
Console.WriteLine(greenText);
Console.WriteLine(yellowText);
```

### Terminal Control Helper Class

```csharp
public static class TerminalColors
{
    // Foreground colors
    public const string Reset = "\e[0m";
    public const string Black = "\e[30m";
    public const string Red = "\e[31m";
    public const string Green = "\e[32m";
    public const string Yellow = "\e[33m";
    public const string Blue = "\e[34m";
    public const string Magenta = "\e[35m";
    public const string Cyan = "\e[36m";
    public const string White = "\e[37m";
    
    // Background colors
    public const string BgRed = "\e[41m";
    public const string BgGreen = "\e[42m";
    public const string BgYellow = "\e[43m";
    public const string BgBlue = "\e[44m";
    
    // Styles
    public const string Bold = "\e[1m";
    public const string Dim = "\e[2m";
    public const string Underline = "\e[4m";
    public const string Blink = "\e[5m";
    public const string Reverse = "\e[7m";
    
    // Cursor control
    public const string ClearScreen = "\e[2J";
    public const string CursorHome = "\e[H";
    
    public static string Color(string text, string color) 
        => $"{color}{text}{Reset}";
}

// Usage
Console.WriteLine(TerminalColors.Color("Error occurred!", TerminalColors.Red));
Console.WriteLine(TerminalColors.Color("Operation successful", TerminalColors.Green));
Console.WriteLine($"{TerminalColors.Bold}{TerminalColors.Blue}Important Info{TerminalColors.Reset}");
```

### Real-World Example: Enhanced Console Logger

```csharp
public class ColoredLogger
{
    public void LogError(string message)
        => Console.WriteLine($"{TerminalColors.Red}[ERROR]{TerminalColors.Reset} {message}");
    
    public void LogWarning(string message)
        => Console.WriteLine($"{TerminalColors.Yellow}[WARN]{TerminalColors.Reset} {message}");
    
    public void LogInfo(string message)
        => Console.WriteLine($"{TerminalColors.Cyan}[INFO]{TerminalColors.Reset} {message}");
    
    public void LogSuccess(string message)
        => Console.WriteLine($"{TerminalColors.Green}[SUCCESS]{TerminalColors.Reset} {message}");
    
    public void LogDebug(string message)
        => Console.WriteLine($"{TerminalColors.Dim}[DEBUG]{TerminalColors.Reset} {message}");
}

// Usage
var logger = new ColoredLogger();
logger.LogInfo("Application starting...");
logger.LogSuccess("Database connected");
logger.LogWarning("Cache miss for key: user_123");
logger.LogError("Failed to process request");
```

## Method Group Natural Types

### Overview
C# 13 improves type inference for method groups, allowing the compiler to infer more natural types without explicit delegate declarations. This simplifies code and improves readability.

### Examples

```csharp
public class Calculator
{
    public int Add(int a, int b) => a + b;
    public int Subtract(int a, int b) => a - b;
    public int Multiply(int a, int b) => a * b;
}

// C# 13: Natural type inference for method groups
var calc = new Calculator();

// Before: Required explicit delegate type
Func<int, int, int> addFunc = calc.Add;

// C# 13: Natural type inference
var add = calc.Add;  // Inferred as delegate matching signature
var result = add(5, 3);  // Works naturally

// Works with var in more scenarios
var operations = new[] { calc.Add, calc.Subtract, calc.Multiply };
foreach (var operation in operations)
{
    Console.WriteLine(operation(10, 5));
}
```

### LINQ and Functional Programming

```csharp
public class DataProcessor
{
    public bool IsEven(int n) => n % 2 == 0;
    public bool IsPositive(int n) => n > 0;
    public int Square(int n) => n * n;
    public string Format(int n) => $"Number: {n}";
}

var processor = new DataProcessor();
var numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// C# 13: Method groups work more naturally with LINQ
var evenNumbers = numbers.Where(processor.IsEven);
var positiveNumbers = numbers.Where(processor.IsPositive);
var squared = numbers.Select(processor.Square);
var formatted = numbers.Select(processor.Format);

// Chaining works naturally
var result = numbers
    .Where(processor.IsEven)
    .Select(processor.Square)
    .Select(processor.Format);

foreach (var item in result)
    Console.WriteLine(item);
```

### Event Handler Simplification

```csharp
public class Button
{
    public event EventHandler? Clicked;
    
    public void Click() => Clicked?.Invoke(this, EventArgs.Empty);
}

public class ClickHandler
{
    public void OnButtonClick(object? sender, EventArgs e)
    {
        Console.WriteLine("Button clicked!");
    }
    
    public void OnButtonClickWithValidation(object? sender, EventArgs e)
    {
        Console.WriteLine("Button clicked with validation!");
    }
}

// C# 13: Simplified event handler registration
var button = new Button();
var handler = new ClickHandler();

// Natural method group usage
button.Clicked += handler.OnButtonClick;
button.Clicked += handler.OnButtonClickWithValidation;

button.Click();
```

## Implicit Indexer Access in Object Initializers

### Overview
C# 13 allows implicit access to indexers in object initializers without explicitly using the `this` keyword, making initialization syntax more concise.

### Syntax and Examples

```csharp
// Dictionary initialization
public class Configuration
{
    private readonly Dictionary<string, string> _settings = new();
    
    public string this[string key]
    {
        get => _settings[key];
        set => _settings[key] = value;
    }
}

// C# 13: Implicit indexer in initializer
var config = new Configuration
{
    ["AppName"] = "MyApplication",
    ["Version"] = "1.0.0",
    ["Environment"] = "Production",
    ["LogLevel"] = "Information"
};

// Before C# 13: Required more verbose syntax
var configOld = new Configuration();
configOld["AppName"] = "MyApplication";
configOld["Version"] = "1.0.0";
```

### Custom Collection with Indexer

```csharp
public class Grid<T>
{
    private readonly Dictionary<(int, int), T> _cells = new();
    
    public T this[int row, int col]
    {
        get => _cells[(row, col)];
        set => _cells[(row, col)] = value;
    }
    
    public int Rows { get; init; }
    public int Cols { get; init; }
}

// C# 13: Clean initialization with implicit indexer
var grid = new Grid<string>
{
    Rows = 3,
    Cols = 3,
    [0, 0] = "A1",
    [0, 1] = "A2",
    [0, 2] = "A3",
    [1, 0] = "B1",
    [1, 1] = "B2",
    [1, 2] = "B3"
};
```

### Real-World Example: Matrix Initialization

```csharp
public class Matrix
{
    private readonly double[,] _data;
    
    public Matrix(int rows, int cols)
    {
        _data = new double[rows, cols];
        Rows = rows;
        Cols = cols;
    }
    
    public int Rows { get; }
    public int Cols { get; }
    
    public double this[int row, int col]
    {
        get => _data[row, col];
        set => _data[row, col] = value;
    }
}

// Identity matrix initialization
var identityMatrix = new Matrix(3, 3)
{
    [0, 0] = 1.0,
    [1, 1] = 1.0,
    [2, 2] = 1.0
};

// Custom matrix
var customMatrix = new Matrix(2, 3)
{
    [0, 0] = 1.5, [0, 1] = 2.0, [0, 2] = 3.5,
    [1, 0] = 4.0, [1, 1] = 5.5, [1, 2] = 6.0
};
```

## Ref and Unsafe in Iterators and Async

### Overview
C# 13 removes restrictions that previously prevented using `ref` locals and `unsafe` code in iterator methods and async methods, enabling more performance-critical scenarios.

### Ref in Async Methods

```csharp
public class DataProcessor
{
    // C# 13: ref locals in async methods
    public async Task<int> ProcessLargeArrayAsync(int[] data)
    {
        await Task.Delay(100); // Simulate async work
        
        // Can now use ref locals in async methods
        ref int first = ref data[0];
        ref int last = ref data[^1];
        
        first *= 2;
        last *= 2;
        
        return first + last;
    }
    
    // C# 13: ref returns in async methods
    public async Task<int> FindMaxAsync(int[] numbers)
    {
        await Task.Delay(50);
        
        ref int max = ref numbers[0];
        for (int i = 1; i < numbers.Length; i++)
        {
            ref int current = ref numbers[i];
            if (current > max)
                max = ref current;
        }
        
        return max;
    }
}

// Usage
var processor = new DataProcessor();
var data = new[] { 1, 2, 3, 4, 5 };
var result = await processor.ProcessLargeArrayAsync(data);
Console.WriteLine($"Result: {result}, Array: [{string.Join(", ", data)}]");
```

### Unsafe in Iterators

```csharp
public class UnsafeIterator
{
    // C# 13: unsafe code in iterators
    public unsafe IEnumerable<int> ReadUnmanagedMemory(int* ptr, int length)
    {
        for (int i = 0; i < length; i++)
        {
            yield return ptr[i];
        }
    }
    
    // C# 13: Combined unsafe and ref in iterator
    public unsafe IEnumerable<byte> ProcessBytes(byte* buffer, int size)
    {
        for (int i = 0; i < size; i++)
        {
            ref byte current = ref buffer[i];
            current = (byte)(current ^ 0xFF); // XOR transformation
            yield return current;
        }
    }
}

// Usage
unsafe
{
    int[] managedArray = { 10, 20, 30, 40, 50 };
    fixed (int* ptr = managedArray)
    {
        var iterator = new UnsafeIterator();
        foreach (var value in iterator.ReadUnmanagedMemory(ptr, managedArray.Length))
        {
            Console.WriteLine(value);
        }
    }
}
```

### Real-World Example: High-Performance Image Processing

```csharp
public class ImageProcessor
{
    // C# 13: Async method with unsafe operations
    public async Task<byte[]> ProcessImageAsync(byte[] imageData, int width, int height)
    {
        // Simulate async I/O
        await Task.Delay(1);
        
        var result = new byte[imageData.Length];
        Array.Copy(imageData, result, imageData.Length);
        
        unsafe
        {
            fixed (byte* ptr = result)
            {
                // Direct memory manipulation for performance
                ref byte firstPixel = ref ptr[0];
                
                for (int i = 0; i < imageData.Length; i++)
                {
                    ptr[i] = (byte)(255 - ptr[i]); // Invert colors
                }
            }
        }
        
        return result;
    }
    
    // C# 13: Iterator with unsafe operations
    public unsafe IEnumerable<(int X, int Y, byte Value)> EnumeratePixels(
        byte[] imageData, int width, int height)
    {
        fixed (byte* ptr = imageData)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    yield return (x, y, ptr[index]);
                }
            }
        }
    }
}
```

## Allows Ref Struct Interfaces

### Overview
C# 13 allows ref structs to implement interfaces, enabling them to participate in generic programming while maintaining their performance characteristics and stack-only allocation.

### Basic Implementation

```csharp
// Interface that can be implemented by ref structs
public interface IProcessor<T>
{
    void Process(T value);
    T GetResult();
}

// C# 13: ref struct implementing interface
public ref struct SpanProcessor : IProcessor<int>
{
    private Span<int> _buffer;
    private int _sum;
    
    public SpanProcessor(Span<int> buffer)
    {
        _buffer = buffer;
        _sum = 0;
    }
    
    public void Process(int value)
    {
        _sum += value;
        if (_buffer.Length > 0)
        {
            _buffer[0] = value;
            _buffer = _buffer[1..];
        }
    }
    
    public int GetResult() => _sum;
}

// Usage
Span<int> buffer = stackalloc int[10];
var processor = new SpanProcessor(buffer);
processor.Process(5);
processor.Process(10);
Console.WriteLine($"Sum: {processor.GetResult()}");
```

### Generic Programming with Ref Structs

```csharp
// Generic method that works with any IProcessor
public static T ProcessCollection<T, TProcessor>(
    ReadOnlySpan<T> items, 
    TProcessor processor) 
    where TProcessor : IProcessor<T>
{
    foreach (var item in items)
    {
        processor.Process(item);
    }
    return processor.GetResult();
}

// Ref struct implementing IProcessor
public ref struct AverageCalculator : IProcessor<double>
{
    private double _sum;
    private int _count;
    
    public void Process(double value)
    {
        _sum += value;
        _count++;
    }
    
    public double GetResult() => _count > 0 ? _sum / _count : 0;
}

// Usage with zero heap allocations
ReadOnlySpan<double> numbers = stackalloc double[] { 1.5, 2.5, 3.5, 4.5, 5.5 };
var calculator = new AverageCalculator();
var average = ProcessCollection(numbers, calculator);
Console.WriteLine($"Average: {average}");
```

### Real-World Example: High-Performance Text Processing

```csharp
public interface ITextAnalyzer
{
    void AnalyzeChar(char c);
    (int Words, int Lines, int Characters) GetStatistics();
}

public ref struct TextStatistics : ITextAnalyzer
{
    private int _words;
    private int _lines;
    private int _characters;
    private bool _inWord;
    
    public void AnalyzeChar(char c)
    {
        _characters++;
        
        if (c == '\n')
        {
            _lines++;
            _inWord = false;
        }
        else if (char.IsWhiteSpace(c))
        {
            _inWord = false;
        }
        else if (!_inWord)
        {
            _words++;
            _inWord = true;
        }
    }
    
    public (int Words, int Lines, int Characters) GetStatistics() 
        => (_words, _lines, _characters);
}

// Generic analyzer processor
public static (int Words, int Lines, int Characters) AnalyzeText<TAnalyzer>(
    ReadOnlySpan<char> text, 
    TAnalyzer analyzer) 
    where TAnalyzer : ITextAnalyzer
{
    foreach (char c in text)
    {
        analyzer.AnalyzeChar(c);
    }
    return analyzer.GetStatistics();
}

// Usage - zero heap allocations
ReadOnlySpan<char> text = "Hello World\nThis is a test\nC# 13 is awesome";
var stats = new TextStatistics();
var result = AnalyzeText(text, stats);
Console.WriteLine($"Words: {result.Words}, Lines: {result.Lines}, Chars: {result.Characters}");
```

## More Partial Members

### Overview
C# 13 expands partial member support beyond methods to include properties, indexers, and more member types, enabling better code generation scenarios and separation of concerns.

### Partial Properties

```csharp
// File 1: Declaration
public partial class UserProfile
{
    public partial string FirstName { get; set; }
    public partial string LastName { get; set; }
    public partial string FullName { get; }
}

// File 2: Implementation (could be source-generated)
public partial class UserProfile
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    
    public partial string FirstName
    {
        get => _firstName;
        set => _firstName = value?.Trim() ?? string.Empty;
    }
    
    public partial string LastName
    {
        get => _lastName;
        set => _lastName = value?.Trim() ?? string.Empty;
    }
    
    public partial string FullName => $"{FirstName} {LastName}";
}
```

### Partial Indexers

```csharp
// File 1: Declaration
public partial class DataStore
{
    public partial string this[string key] { get; set; }
}

// File 2: Implementation
public partial class DataStore
{
    private readonly Dictionary<string, string> _data = new();
    
    public partial string this[string key]
    {
        get => _data.TryGetValue(key, out var value) ? value : string.Empty;
        set => _data[key] = value;
    }
}
```

### Real-World Example: Source Generator Integration

```csharp
// User-written code
[AutoNotify]
public partial class PersonViewModel
{
    public partial string Name { get; set; }
    public partial int Age { get; set; }
    public partial string Email { get; set; }
}

// Source-generated code (separate file)
public partial class PersonViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private string _name = string.Empty;
    public partial string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }
    
    private int _age;
    public partial int Age
    {
        get => _age;
        set
        {
            if (_age != value)
            {
                _age = value;
                OnPropertyChanged(nameof(Age));
            }
        }
    }
    
    private string _email = string.Empty;
    public partial string Email
    {
        get => _email;
        set
        {
            if (_email != value)
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }
    }
    
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

## Best Practices

### 1. Params Collections
- Use `ReadOnlySpan<T>` for params when you don't need to store the collection
- Prefer `params List<T>` when the method needs to modify the collection
- Use `params IEnumerable<T>` for LINQ-heavy operations

### 2. Escape Sequence \e
- Create constants for commonly used ANSI codes
- Check terminal support before using ANSI codes in production
- Consider using libraries like Spectre.Console for cross-platform compatibility

### 3. Method Groups
- Leverage natural type inference to reduce boilerplate
- Be explicit when the inferred type might be ambiguous
- Use method groups with LINQ for cleaner, more readable code

### 4. Ref Structs with Interfaces
- Only implement interfaces on ref structs when generic programming benefits outweigh constraints
- Remember ref structs still can't be boxed or used in async method state machines
- Use for high-performance scenarios where zero-allocation is critical

### 5. Partial Members
- Use for source generators and code generation scenarios
- Keep declarations and implementations in separate files for clarity
- Document which parts are generated vs. hand-written

## Interview Questions

### Question 1: Params Collections Performance
**Q:** What are the performance benefits of using `params ReadOnlySpan<T>` over `params T[]` in C# 13?

**A:** `params ReadOnlySpan<T>` offers several performance advantages:
- **Zero heap allocations**: For small collections, the compiler can allocate on the stack
- **No array overhead**: No array object creation, bounds checking is minimal
- **Better cache locality**: Stack allocation improves CPU cache efficiency
- **Reduced GC pressure**: No garbage collector involvement for stack-allocated spans
- Benchmarks show 5-10x performance improvements for methods called frequently with small parameter counts

### Question 2: Escape Sequence Usage
**Q:** When would you use the new `\e` escape sequence in C# 13, and what are the compatibility considerations?

**A:** The `\e` escape sequence is used for:
- Terminal color formatting (ANSI color codes)
- Cursor positioning and terminal control
- Creating CLI tools with colored output
- Progress bars and interactive console applications

Compatibility considerations:
- Not all terminals support ANSI codes (Windows cmd.exe requires Windows 10+)
- Should check `Console.IsOutputRedirected` before using
- Consider using cross-platform libraries for production code
- May not work when output is redirected to files

### Question 3: Ref Structs and Interfaces
**Q:** What restrictions still apply to ref structs even when they implement interfaces in C# 13?

**A:** Even with interface implementation, ref structs still:
- Cannot be boxed (no object reference allowed)
- Cannot be used as type arguments in generic types that require class constraint
- Cannot be used in async method state machines
- Cannot be used in iterator blocks (yield return)
- Must remain on the stack (no heap allocation)
- Cannot be used as fields in non-ref structs
- Cannot implement interface methods that return Task or ValueTask

### Question 4: Partial Properties
**Q:** How do partial properties in C# 13 benefit source generator scenarios?

**A:** Partial properties enable:
- **Clean separation**: User writes declaration, generator provides implementation
- **INotifyPropertyChanged**: Generators can add change notification automatically
- **Validation logic**: Generators can inject validation in setters
- **Computed properties**: Generators can create derived property implementations
- **Cross-cutting concerns**: Logging, caching, lazy initialization without polluting user code
- **Better IntelliSense**: Users see clean declarations without implementation details

### Question 5: Method Group Natural Types
**Q:** How does method group natural type inference in C# 13 improve LINQ queries?

**A:** Natural type inference improves LINQ by:
- Eliminating explicit lambda expressions: `.Where(x => IsValid(x))` becomes `.Where(IsValid)`
- Better performance: Method groups can be optimized better than lambdas
- More readable code: Intent is clearer with method names
- Easier refactoring: Extract method works seamlessly with LINQ
- Type safety: Compiler catches signature mismatches earlier
- Works with instance and static methods naturally

### Question 6: Implicit Indexer Access
**Q:** Provide a real-world scenario where implicit indexer access in object initializers improves code maintainability.

**A:** Configuration management example:
```csharp
// Before: Verbose, easy to miss items
var config = new AppConfig();
config["Database:Host"] = "localhost";
config["Database:Port"] = "5432";
config["Cache:TTL"] = "300";

// C# 13: Clear, concise, all settings visible in one place
var config = new AppConfig
{
    ["Database:Host"] = "localhost",
    ["Database:Port"] = "5432",
    ["Cache:TTL"] = "300",
    ["Logging:Level"] = "Information"
};
```
Benefits: All configuration visible in one block, easier code reviews, reduced initialization errors, better readability.

### Question 7: Ref and Unsafe in Async
**Q:** What performance scenarios benefit from using ref locals in async methods in C# 13?

**A:** Key scenarios include:
- **Large array/buffer manipulation**: Direct reference avoids copying values
- **Struct modification**: Modify large structs in-place without copying
- **High-frequency updates**: Game loops, real-time processing where allocation must be minimal
- **Memory-mapped files**: Direct memory access with async I/O operations
- **Interop scenarios**: Working with unmanaged memory in async contexts
Example: Processing video frames asynchronously while maintaining zero-copy semantics

### Question 8: Params Collection Types
**Q:** When should you choose `params IEnumerable<T>` over `params List<T>` or `params T[]`?

**A:** Choose `params IEnumerable<T>` when:
- Method only needs to iterate once (no indexing needed)
- Working with LINQ queries extensively
- Want to accept any collection type (maximum flexibility)
- Method doesn't modify the collection

Choose `params List<T>` when:
- Need to modify collection (add/remove items)
- Require multiple iterations
- Need indexing and Count property

Choose `params ReadOnlySpan<T>` when:
- Performance is critical
- Collections are small (< 256 elements)
- Zero allocation is required

### Question 9: Terminal Escape Sequences
**Q:** How would you implement a cross-platform logging system using C# 13's `\e` escape sequence?

**A:** Implementation approach:
```csharp
public class CrossPlatformLogger
{
    private readonly bool _supportsAnsi;
    
    public CrossPlatformLogger()
    {
        _supportsAnsi = !Console.IsOutputRedirected && 
                        Environment.OSVersion.Platform == PlatformID.Unix ||
                        (OperatingSystem.IsWindows() && Environment.OSVersion.Version.Major >= 10);
    }
    
    public void Log(string message, LogLevel level)
    {
        var colored = _supportsAnsi 
            ? $"\e[{GetColorCode(level)}m{message}\e[0m"
            : message;
        Console.WriteLine(colored);
    }
}
```
This checks platform support and gracefully degrades on unsupported systems.

### Question 10: Partial Members Code Generation
**Q:** What are the advantages of using partial properties over partial methods for source generators?

**A:** Advantages of partial properties:
- **Natural syntax**: Properties match how developers think about data
- **Better encapsulation**: Can generate backing fields privately
- **IntelliSense-friendly**: Users see properties, not methods
- **Consistent API**: Public API looks like normal properties
- **Easier debugging**: Property getters/setters show in debugger naturally
- **Expression syntax**: Can use expression bodies for computed properties
- **Init accessors**: Can use init for immutable properties
Partial methods are better for behavior/actions, partial properties for data/state.

---

**Last Updated: January 2026 - .NET 9**
