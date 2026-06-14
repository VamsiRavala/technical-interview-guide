# Performance Optimization in .NET 9

## Table of Contents
- [Introduction to Performance](#introduction-to-performance)
- [Span<T> and Memory<T>](#spant-and-memoryt)
- [ValueTask for Async Performance](#valuetask-for-async-performance)
- [ArrayPool and Memory Pooling](#arraypool-and-memory-pooling)
- [Profile-Guided Optimization (PGO)](#profile-guided-optimization-pgo)
- [Benchmarking with BenchmarkDotNet](#benchmarking-with-benchmarkdotnet)
- [Memory Optimization Techniques](#memory-optimization-techniques)
- [JIT Compilation Tips](#jit-compilation-tips)
- [Real-World Performance Scenarios](#real-world-performance-scenarios)
- [Interview Questions](#interview-questions)

## Introduction to Performance

Performance optimization in .NET 9 focuses on reducing allocations, minimizing CPU cycles, and leveraging modern hardware capabilities.

### Performance Principles

```csharp
// Three pillars of .NET performance:
// 1. Reduce Allocations - Less GC pressure
// 2. Optimize Hot Paths - Focus on frequently executed code
// 3. Measure First - Use profiling before optimizing

// Example: Allocation comparison
public class AllocationComparison
{
    // ❌ Creates new string (allocation)
    public string ProcessBad(string input)
    {
        return input.ToUpper(); // Allocates new string
    }
    
    // ✅ Zero allocation with Span
    public void ProcessGood(ReadOnlySpan<char> input, Span<char> output)
    {
        for (int i = 0; i < input.Length; i++)
        {
            output[i] = char.ToUpper(input[i]);
        }
    }
}
```

### Performance Metrics

```csharp
using System.Diagnostics;

public class PerformanceMetrics
{
    public static void MeasureOperation(Action operation)
    {
        // Measure time
        var sw = Stopwatch.StartNew();
        
        // Measure memory
        var beforeMem = GC.GetTotalMemory(true);
        var beforeGen0 = GC.CollectionCount(0);
        var beforeGen1 = GC.CollectionCount(1);
        var beforeGen2 = GC.CollectionCount(2);
        
        // Execute operation
        operation();
        
        sw.Stop();
        
        var afterMem = GC.GetTotalMemory(false);
        var afterGen0 = GC.CollectionCount(0);
        var afterGen1 = GC.CollectionCount(1);
        var afterGen2 = GC.CollectionCount(2);
        
        Console.WriteLine($"Time: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"Memory: {(afterMem - beforeMem) / 1024}KB");
        Console.WriteLine($"Gen0: {afterGen0 - beforeGen0}");
        Console.WriteLine($"Gen1: {afterGen1 - beforeGen1}");
        Console.WriteLine($"Gen2: {afterGen2 - beforeGen2}");
    }
}
```

## Span<T> and Memory<T>

Span<T> and Memory<T> enable zero-allocation operations over contiguous memory regions.

### Understanding Span<T>

```csharp
using System;

public class SpanExamples
{
    // Span<T> is a stack-only type (ref struct)
    // Cannot be stored in fields, cannot be boxed, cannot be used with async
    
    // Example 1: Slicing arrays without allocation
    public void SlicingExample()
    {
        int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        
        // ❌ Traditional - creates new array (allocation)
        var slice = numbers.Skip(2).Take(5).ToArray();
        
        // ✅ With Span - no allocation
        Span<int> span = numbers;
        Span<int> sliced = span.Slice(2, 5);
        
        // Modify slice (modifies original array)
        sliced[0] = 100; // numbers[2] is now 100
    }
    
    // Example 2: Stack allocation with stackalloc
    public int SumNumbers()
    {
        // Allocate on stack - no GC overhead
        Span<int> numbers = stackalloc int[100];
        
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = i + 1;
        }
        
        int sum = 0;
        foreach (var num in numbers)
        {
            sum += num;
        }
        
        return sum;
    }
    
    // Example 3: String manipulation without allocation
    public bool IsValidEmail(ReadOnlySpan<char> email)
    {
        int atIndex = email.IndexOf('@');
        if (atIndex <= 0) return false;
        
        int dotIndex = email.LastIndexOf('.');
        if (dotIndex <= atIndex) return false;
        
        // No string allocations for these checks!
        return true;
    }
    
    // Example 4: Parsing without substring allocations
    public (string Name, int Age) ParsePerson(ReadOnlySpan<char> input)
    {
        // Input format: "John,30"
        int commaIndex = input.IndexOf(',');
        
        // Extract name without allocation
        var namePart = input.Slice(0, commaIndex);
        var name = new string(namePart);
        
        // Extract and parse age without allocation
        var agePart = input.Slice(commaIndex + 1);
        int age = int.Parse(agePart);
        
        return (name, age);
    }
}
```

### Memory<T> for Async Operations

```csharp
using System;
using System.Threading.Tasks;

public class MemoryExamples
{
    // Memory<T> is like Span<T> but can be used with async
    // Can be stored in fields and used across await boundaries
    
    private readonly Memory<byte> _buffer;
    
    public MemoryExamples()
    {
        _buffer = new byte[4096];
    }
    
    // Example 1: Async file reading
    public async Task<int> ReadFileAsync(Stream stream)
    {
        // Memory<T> works with async
        int bytesRead = await stream.ReadAsync(_buffer);
        
        // Convert to Span for processing
        Span<byte> data = _buffer.Span.Slice(0, bytesRead);
        ProcessData(data);
        
        return bytesRead;
    }
    
    private void ProcessData(Span<byte> data)
    {
        // Process data without allocation
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(data[i] ^ 0xFF); // XOR operation
        }
    }
    
    // Example 2: Memory pooling with Memory<T>
    public async Task<string> ProcessLargeDataAsync(Stream input)
    {
        using var owner = MemoryPool<byte>.Shared.Rent(65536);
        Memory<byte> buffer = owner.Memory;
        
        int totalRead = 0;
        int bytesRead;
        
        while ((bytesRead = await input.ReadAsync(buffer.Slice(totalRead))) > 0)
        {
            totalRead += bytesRead;
            
            if (totalRead >= buffer.Length)
                break;
        }
        
        // Process the data
        return System.Text.Encoding.UTF8.GetString(buffer.Span.Slice(0, totalRead));
    }
}
```

### Advanced Span Patterns

```csharp
public class AdvancedSpanPatterns
{
    // Pattern 1: Span-based parsing
    public static bool TryParseKeyValue(
        ReadOnlySpan<char> input,
        out ReadOnlySpan<char> key,
        out ReadOnlySpan<char> value)
    {
        key = default;
        value = default;
        
        int separatorIndex = input.IndexOf('=');
        if (separatorIndex < 0) return false;
        
        key = input.Slice(0, separatorIndex).Trim();
        value = input.Slice(separatorIndex + 1).Trim();
        
        return true;
    }
    
    // Pattern 2: Span-based CSV parsing
    public static void ParseCsvLine(ReadOnlySpan<char> line, Span<ReadOnlySpan<char>> columns)
    {
        int columnIndex = 0;
        int startIndex = 0;
        
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == ',' || i == line.Length - 1)
            {
                int endIndex = line[i] == ',' ? i : i + 1;
                columns[columnIndex++] = line.Slice(startIndex, endIndex - startIndex);
                startIndex = i + 1;
                
                if (columnIndex >= columns.Length) break;
            }
        }
    }
    
    // Pattern 3: Span for binary data manipulation
    public static void SwapEndianness(Span<byte> data)
    {
        for (int i = 0; i < data.Length / 2; i++)
        {
            (data[i], data[data.Length - 1 - i]) = (data[data.Length - 1 - i], data[i]);
        }
    }
    
    // Pattern 4: Using Span with structs (no boxing)
    public static int CalculateChecksum(ReadOnlySpan<int> values)
    {
        int checksum = 0;
        foreach (var value in values)
        {
            checksum ^= value;
        }
        return checksum;
    }
}
```

## ValueTask for Async Performance

ValueTask<T> reduces allocations in async code when operations often complete synchronously.

### Understanding ValueTask

```csharp
using System.Threading.Tasks;

public class ValueTaskExamples
{
    private readonly Dictionary<int, string> _cache = new();
    
    // ❌ Task<T> always allocates
    public async Task<string> GetDataTaskAsync(int id)
    {
        if (_cache.TryGetValue(id, out var cached))
        {
            return cached; // Still allocates Task<string>
        }
        
        var data = await FetchFromDatabaseAsync(id);
        _cache[id] = data;
        return data;
    }
    
    // ✅ ValueTask<T> avoids allocation for cached results
    public ValueTask<string> GetDataValueTaskAsync(int id)
    {
        if (_cache.TryGetValue(id, out var cached))
        {
            return new ValueTask<string>(cached); // No allocation!
        }
        
        return new ValueTask<string>(FetchAndCacheAsync(id));
    }
    
    private async Task<string> FetchAndCacheAsync(int id)
    {
        var data = await FetchFromDatabaseAsync(id);
        _cache[id] = data;
        return data;
    }
    
    private Task<string> FetchFromDatabaseAsync(int id)
    {
        // Simulate database call
        return Task.FromResult($"Data {id}");
    }
}
```

### ValueTask Best Practices

```csharp
public class ValueTaskBestPractices
{
    // Pattern 1: Interface design with ValueTask
    public interface IDataService
    {
        // Use ValueTask when:
        // 1. Method is often synchronous (cache hits)
        // 2. High-frequency calls
        // 3. Performance-critical path
        ValueTask<User?> GetUserAsync(int id);
    }
    
    public class CachedDataService : IDataService
    {
        private readonly Dictionary<int, User> _cache = new();
        private readonly IDatabase _database;
        
        public CachedDataService(IDatabase database)
        {
            _database = database;
        }
        
        public ValueTask<User?> GetUserAsync(int id)
        {
            // Synchronous path - no allocation
            if (_cache.TryGetValue(id, out var user))
            {
                return new ValueTask<User?>(user);
            }
            
            // Asynchronous path - allocates Task
            return new ValueTask<User?>(LoadUserAsync(id));
        }
        
        private async Task<User?> LoadUserAsync(int id)
        {
            var user = await _database.QueryAsync<User>(id);
            if (user != null)
            {
                _cache[id] = user;
            }
            return user;
        }
    }
    
    // Pattern 2: Avoid awaiting ValueTask multiple times
    public async Task ConsumeValueTaskAsync()
    {
        var service = new CachedDataService(null!);
        
        // ❌ WRONG - ValueTask can only be awaited once!
        var task = service.GetUserAsync(1);
        var user1 = await task;
        var user2 = await task; // DANGER!
        
        // ✅ CORRECT - Await immediately
        var user = await service.GetUserAsync(1);
        
        // ✅ CORRECT - Convert to Task if needed multiple times
        var taskToAwaitMultipleTimes = service.GetUserAsync(1).AsTask();
        var result1 = await taskToAwaitMultipleTimes;
        var result2 = await taskToAwaitMultipleTimes;
    }
    
    // Pattern 3: Using ValueTask for pooled async state machines
    public async ValueTask ProcessStreamAsync(Stream stream)
    {
        var buffer = new byte[4096];
        int bytesRead;
        
        // ValueTask reduces allocations in loop
        while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
        {
            ProcessChunk(buffer.AsSpan(0, bytesRead));
        }
    }
    
    private void ProcessChunk(Span<byte> chunk)
    {
        // Process data
    }
}

public interface IDatabase
{
    Task<T?> QueryAsync<T>(int id) where T : class;
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

## ArrayPool and Memory Pooling

ArrayPool reduces GC pressure by reusing arrays instead of allocating new ones.

### Using ArrayPool<T>

```csharp
using System.Buffers;

public class ArrayPoolExamples
{
    // Example 1: Basic ArrayPool usage
    public void ProcessDataWithPool()
    {
        // Rent array from pool
        var buffer = ArrayPool<byte>.Shared.Rent(4096);
        
        try
        {
            // Use buffer (only use up to length you need)
            int actualLength = FillBuffer(buffer);
            ProcessBuffer(buffer.AsSpan(0, actualLength));
        }
        finally
        {
            // Always return to pool
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    
    // Example 2: Using pooled array with Span
    public string ProcessLargeString(string input)
    {
        if (input.Length < 1024)
        {
            // Small string - use stack
            Span<char> buffer = stackalloc char[input.Length];
            input.AsSpan().ToUpperInvariant(buffer);
            return new string(buffer);
        }
        
        // Large string - use pool
        var pooledArray = ArrayPool<char>.Shared.Rent(input.Length);
        try
        {
            Span<char> buffer = pooledArray.AsSpan(0, input.Length);
            input.AsSpan().ToUpperInvariant(buffer);
            return new string(buffer);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(pooledArray);
        }
    }
    
    // Example 3: Custom ArrayPool configuration
    public void ConfiguredArrayPool()
    {
        // Create custom pool with specific settings
        var pool = ArrayPool<byte>.Create(
            maxArrayLength: 1024 * 1024,  // 1MB max
            maxArraysPerBucket: 50         // Keep 50 arrays per bucket
        );
        
        var buffer = pool.Rent(65536);
        try
        {
            // Use buffer
        }
        finally
        {
            pool.Return(buffer, clearArray: true); // Clear for security
        }
    }
    
    private int FillBuffer(byte[] buffer) => buffer.Length;
    private void ProcessBuffer(Span<byte> buffer) { }
}
```

### Memory Pool Pattern

```csharp
using System.Buffers;

public class MemoryPoolPattern
{
    // Example: High-performance HTTP request processing
    public async Task<string> ReadRequestBodyAsync(Stream stream)
    {
        using IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(8192);
        Memory<byte> memory = owner.Memory;
        
        int totalRead = 0;
        int bytesRead;
        
        while ((bytesRead = await stream.ReadAsync(memory.Slice(totalRead))) > 0)
        {
            totalRead += bytesRead;
            
            if (totalRead >= memory.Length)
                break; // Buffer full
        }
        
        return System.Text.Encoding.UTF8.GetString(memory.Span.Slice(0, totalRead));
    }
    
    // Example: Batch processing with pooling
    public void ProcessBatch<T>(IEnumerable<T> items, Action<T[]> processor, int batchSize = 100)
    {
        var buffer = ArrayPool<T>.Shared.Rent(batchSize);
        try
        {
            int count = 0;
            
            foreach (var item in items)
            {
                buffer[count++] = item;
                
                if (count == batchSize)
                {
                    processor(buffer.AsSpan(0, count).ToArray());
                    count = 0;
                }
            }
            
            // Process remaining items
            if (count > 0)
            {
                processor(buffer.AsSpan(0, count).ToArray());
            }
        }
        finally
        {
            ArrayPool<T>.Shared.Return(buffer);
        }
    }
}
```

## Profile-Guided Optimization (PGO)

PGO is a compilation technique where the JIT uses runtime profiling data to optimize hot paths.

### Enabling PGO

```xml
<!-- In .csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    
    <!-- Enable Dynamic PGO (default in .NET 9) -->
    <TieredCompilation>true</TieredCompilation>
    <TieredCompilationQuickJit>true</TieredCompilationQuickJit>
    
    <!-- Enable PGO -->
    <TieredPGO>true</TieredPGO>
  </PropertyGroup>
</Project>
```

### PGO Benefits

```csharp
public class PgoExample
{
    // PGO optimizes based on actual execution patterns
    
    public interface IProcessor
    {
        void Process(string data);
    }
    
    public class TypeA : IProcessor
    {
        public void Process(string data) => Console.WriteLine($"A: {data}");
    }
    
    public class TypeB : IProcessor
    {
        public void Process(string data) => Console.WriteLine($"B: {data}");
    }
    
    // Without PGO: Virtual call overhead
    // With PGO: If 99% of calls are TypeA, JIT devirtualizes and inlines
    public void ProcessMany(IProcessor processor, List<string> items)
    {
        foreach (var item in items)
        {
            processor.Process(item); // PGO optimizes this!
        }
    }
    
    // PGO also helps with:
    // - Guarded devirtualization
    // - On-stack replacement (OSR)
    // - Loop optimization
    // - Branch prediction
}
```

### Measuring PGO Impact

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

[SimpleJob(RuntimeMoniker.Net90, baseline: true)]
[MemoryDiagnoser]
public class PgoBenchmark
{
    private readonly IProcessor[] _processors;
    
    public PgoBenchmark()
    {
        // 99% TypeA, 1% TypeB (typical in real apps)
        _processors = new IProcessor[100];
        for (int i = 0; i < 99; i++)
            _processors[i] = new TypeA();
        _processors[99] = new TypeB();
    }
    
    [Benchmark]
    public void ProcessWithPgo()
    {
        // PGO sees TypeA is hot path, optimizes accordingly
        foreach (var processor in _processors)
        {
            processor.Process("data");
        }
    }
}
```

## Benchmarking with BenchmarkDotNet

BenchmarkDotNet is the gold standard for .NET performance measurement.

### Basic Benchmarking

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
[DisassemblyDiagnoser]
public class StringBenchmarks
{
    private const string Input = "Hello, World!";
    
    [Benchmark(Baseline = true)]
    public string ToUpperString()
    {
        return Input.ToUpper(); // Allocates new string
    }
    
    [Benchmark]
    public string ToUpperSpan()
    {
        Span<char> buffer = stackalloc char[Input.Length];
        Input.AsSpan().ToUpperInvariant(buffer);
        return new string(buffer);
    }
    
    [Benchmark]
    public string ToUpperArrayPool()
    {
        var array = ArrayPool<char>.Shared.Rent(Input.Length);
        try
        {
            Input.AsSpan().ToUpperInvariant(array.AsSpan(0, Input.Length));
            return new string(array, 0, Input.Length);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(array);
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<StringBenchmarks>();
    }
}

// Results (example):
// |           Method |      Mean |   Error |  Allocated |
// |----------------- |----------:|--------:|-----------:|
// |   ToUpperString  |  45.32 ns | 0.89 ns |      56 B  |
// |     ToUpperSpan  |  23.15 ns | 0.45 ns |      26 B  |
// | ToUpperArrayPool |  28.67 ns | 0.52 ns |      26 B  |
```

### Advanced Benchmarking

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[ThreadingDiagnoser]
public class CollectionBenchmarks
{
    [Params(10, 100, 1000)]
    public int Size { get; set; }
    
    private int[] _data = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(0, Size).ToArray();
    }
    
    [Benchmark]
    public int SumArray()
    {
        int sum = 0;
        for (int i = 0; i < _data.Length; i++)
        {
            sum += _data[i];
        }
        return sum;
    }
    
    [Benchmark]
    public int SumSpan()
    {
        Span<int> span = _data;
        int sum = 0;
        foreach (var value in span)
        {
            sum += value;
        }
        return sum;
    }
    
    [Benchmark]
    public int SumLinq()
    {
        return _data.Sum(); // LINQ overhead
    }
}
```

## Memory Optimization Techniques

### Struct vs Class

```csharp
// Rule: Use struct for small, immutable data (<= 16 bytes)

// ❌ Class - heap allocation, GC overhead
public class PointClass
{
    public int X { get; set; }
    public int Y { get; set; }
    // Size: Object header (8/16 bytes) + 8 bytes data
}

// ✅ Struct - stack allocation, no GC
public readonly struct PointStruct
{
    public int X { get; }
    public int Y { get; }
    
    public PointStruct(int x, int y)
    {
        X = x;
        Y = y;
    }
    // Size: 8 bytes, no object header
}

public class StructVsClassDemo
{
    public void AllocateMany()
    {
        // Class: 1 million heap allocations
        var points1 = new PointClass[1_000_000];
        for (int i = 0; i < points1.Length; i++)
            points1[i] = new PointClass();
        
        // Struct: 1 allocation (array), points on stack/inline
        var points2 = new PointStruct[1_000_000];
        for (int i = 0; i < points2.Length; i++)
            points2[i] = new PointStruct(i, i);
    }
}
```

### String Interning

```csharp
public class StringOptimization
{
    // String interning saves memory for repeated strings
    public void StringInterning()
    {
        // ❌ Without interning - multiple instances
        var s1 = new string("hello".ToCharArray());
        var s2 = new string("hello".ToCharArray());
        Console.WriteLine(ReferenceEquals(s1, s2)); // False
        
        // ✅ With interning - single instance
        var s3 = string.Intern(new string("hello".ToCharArray()));
        var s4 = string.Intern(new string("hello".ToCharArray()));
        Console.WriteLine(ReferenceEquals(s3, s4)); // True
    }
    
    // Use StringBuilder for concatenation
    public string BuildString(int count)
    {
        // ❌ Multiple allocations
        string result = "";
        for (int i = 0; i < count; i++)
        {
            result += i.ToString(); // New string each iteration
        }
        return result;
    }
    
    public string BuildStringOptimized(int count)
    {
        // ✅ Single final allocation
        var sb = new StringBuilder(count * 4); // Pre-size if possible
        for (int i = 0; i < count; i++)
        {
            sb.Append(i);
        }
        return sb.ToString();
    }
}
```

### Collection Sizing

```csharp
public class CollectionSizing
{
    // Pre-size collections to avoid resizing
    public List<int> CreateListBad()
    {
        var list = new List<int>(); // Default capacity: 4
        for (int i = 0; i < 1000; i++)
        {
            list.Add(i); // Multiple internal array resizes
        }
        return list;
    }
    
    public List<int> CreateListGood()
    {
        var list = new List<int>(1000); // Pre-sized
        for (int i = 0; i < 1000; i++)
        {
            list.Add(i); // No resizing
        }
        return list;
    }
    
    // Use appropriate collection type
    public void CollectionChoice()
    {
        // Dictionary<K,V> - O(1) lookup
        var dict = new Dictionary<int, string>(1000);
        
        // HashSet<T> - O(1) contains
        var set = new HashSet<int>(1000);
        
        // List<T> - O(1) index, O(n) contains
        var list = new List<int>(1000);
        
        // Use frozen collections for read-only data (.NET 8+)
        var frozen = list.ToFrozenSet(); // Optimized for lookups
    }
}
```

## JIT Compilation Tips

### Aggressive Inlining

```csharp
using System.Runtime.CompilerServices;

public class InliningExamples
{
    // Small, frequently called methods are auto-inlined
    public int Add(int a, int b)
    {
        return a + b; // Will be inlined
    }
    
    // Force inlining for critical paths
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int MultiplyAndAdd(int a, int b, int c)
    {
        return (a * b) + c;
    }
    
    // Prevent inlining for rarely called methods
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void RarelyCalledMethod()
    {
        // Large method body that shouldn't bloat call sites
    }
    
    // Aggressive optimization
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public double FastMath(double x, double y)
    {
        return Math.Sqrt(x * x + y * y);
    }
}
```

### Loop Optimization

```csharp
public class LoopOptimization
{
    // JIT optimizes loops automatically, but help it:
    
    // ✅ Good: Forward loop, clear bounds
    public int SumArrayForward(int[] array)
    {
        int sum = 0;
        for (int i = 0; i < array.Length; i++)
        {
            sum += array[i];
        }
        return sum;
    }
    
    // ✅ Good: Hoist invariant calculations
    public void ProcessArray(int[] array, int multiplier)
    {
        int factor = multiplier * 2; // Hoisted out of loop
        for (int i = 0; i < array.Length; i++)
        {
            array[i] *= factor;
        }
    }
    
    // ✅ Good: SIMD-friendly patterns
    public void MultiplyArrays(Span<float> a, ReadOnlySpan<float> b, ReadOnlySpan<float> c)
    {
        // JIT can vectorize this with SIMD
        for (int i = 0; i < a.Length; i++)
        {
            a[i] = b[i] * c[i];
        }
    }
}
```

## Real-World Performance Scenarios

### High-Performance JSON Parsing

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

// Use source generators for AOT and performance
[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(List<Product>))]
internal partial class AppJsonContext : JsonSerializerContext { }

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class JsonPerformance
{
    public List<Product> ParseProducts(ReadOnlySpan<byte> json)
    {
        // Zero-allocation parsing with Utf8JsonReader
        var reader = new Utf8JsonReader(json);
        var products = new List<Product>();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Parse individual product
                var product = JsonSerializer.Deserialize<Product>(
                    ref reader, 
                    AppJsonContext.Default.Product);
                
                if (product != null)
                    products.Add(product);
            }
        }
        
        return products;
    }
    
    public void SerializeWithContext(Product product, Stream output)
    {
        // Use source-generated serializer for best performance
        JsonSerializer.Serialize(output, product, AppJsonContext.Default.Product);
    }
}
```

### Efficient File Processing

```csharp
using System.IO.Pipelines;

public class FileProcessing
{
    // Use pipelines for high-throughput I/O
    public async Task ProcessLargeFileAsync(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        var reader = PipeReader.Create(stream);
        
        while (true)
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;
            
            // Process buffer
            ProcessBuffer(buffer);
            
            // Tell reader we're done with this data
            reader.AdvanceTo(buffer.End);
            
            if (result.IsCompleted)
                break;
        }
        
        await reader.CompleteAsync();
    }
    
    private void ProcessBuffer(ReadOnlySequence<byte> buffer)
    {
        foreach (var segment in buffer)
        {
            Span<byte> span = segment.Span;
            // Process without allocation
        }
    }
}
```

## Interview Questions

### 1. What is Span<T> and when should you use it?
**Answer**: Span<T> is a stack-only ref struct representing a contiguous region of memory. Benefits: (1) Zero allocations - no heap memory, (2) Works with arrays, stackalloc, native memory, (3) Slicing without copying. Use for: parsing, string manipulation, buffer operations. Cannot use with: async/await, fields, boxing, Task. For async scenarios, use Memory<T> instead. Perfect for hot paths needing high performance without GC pressure.

### 2. Explain the difference between Span<T> and Memory<T>.
**Answer**: Span<T> is stack-only (ref struct), faster but more restricted - can't use with async, can't store in fields, can't box. Memory<T> is a regular struct, can use with async, store in fields, pass across await boundaries. Both provide view over contiguous memory without copying. Use Span<T> for synchronous operations, Memory<T> for async operations. Memory<T>.Span converts to Span<T> for processing.

### 3. What is ValueTask and when should you prefer it over Task?
**Answer**: ValueTask<T> is a struct that can represent synchronous or asynchronous results, avoiding allocation when operation completes synchronously. Use when: (1) Frequently synchronous (cache hits), (2) High-frequency calls, (3) Performance-critical. Task always allocates on heap. Gotchas: Can only await ValueTask once, can't use with Task.WhenAll directly (convert with .AsTask()). ~40% faster for synchronous completions. Perfect for repository methods with caching.

### 4. How does ArrayPool<T> improve performance?
**Answer**: ArrayPool<T> reuses arrays instead of allocating new ones, reducing GC pressure. Call ArrayPool<T>.Shared.Rent(size) to get array, Return() when done. Benefits: (1) Reduces Gen0/Gen1 collections, (2) Faster than allocation for large arrays, (3) Shared pool across app. Best for: temporary buffers, frequent allocations, large arrays. Always return in finally block. May get larger array than requested - use Span to work with actual length needed.

### 5. What is Profile-Guided Optimization (PGO) in .NET 9?
**Answer**: PGO uses runtime profiling to optimize hot paths. JIT compiles code with tier 0 (quick), collects execution data, recompiles with tier 1 (optimized based on actual usage). Benefits: (1) Devirtualization - if interface call is 99% one type, removes virtual overhead, (2) Inlining hot paths, (3) Loop optimization, (4) Better branch prediction. Enable with TieredPGO=true (default .NET 9). ~15-30% perf improvement in real apps. Dynamic PGO happens at runtime.

### 6. What are the key principles for reducing allocations in .NET?
**Answer**: (1) Use Span<T>/Memory<T> for data manipulation, (2) Use ArrayPool for temporary buffers, (3) Use ValueTask for async with frequent sync completion, (4) Avoid LINQ in hot paths, (5) Use struct for small immutable types (<= 16 bytes), (6) Pre-size collections (List/Dictionary capacity), (7) Use StringBuilder for concatenation, (8) Avoid boxing value types, (9) Use stackalloc for small buffers, (10) Cache frequently used objects. Measure with BenchmarkDotNet's [MemoryDiagnoser].

### 7. How do you measure and benchmark .NET performance correctly?
**Answer**: Use BenchmarkDotNet: (1) Add [MemoryDiagnoser] for allocation tracking, (2) [Benchmark(Baseline=true)] to compare against baseline, (3) Run in Release mode, (4) Use [Params] for different input sizes, (5) Include [GlobalSetup] for initialization, (6) Run multiple iterations (BenchmarkDotNet handles this), (7) Check for outliers, (8) Compare against different runtimes with [SimpleJob], (9) Use [DisassemblyDiagnoser] to see JIT output, (10) Avoid Debug.WriteLine in benchmarks. Never use Stopwatch for microbenchmarks.

### 8. What is aggressive inlining and when should you use it?
**Answer**: Aggressive inlining eliminates method call overhead by inserting method body at call site. Use [MethodImpl(MethodImplOptions.AggressiveInlining)] for: (1) Small methods called frequently, (2) Property getters/setters in hot paths, (3) Simple math operations. JIT auto-inlines methods <= 32 bytes IL. Benefits: Eliminates call overhead, enables further optimizations. Tradeoffs: Larger code size (instruction cache pressure), longer JIT time. Measure impact - don't cargo-cult. Use [MethodImpl(MethodImplOptions.NoInlining)] for cold paths to reduce code size.

### 9. How do struct and class differ in terms of performance?
**Answer**: Struct: (1) Stack allocated (if not boxed/in array), (2) No heap allocation/GC overhead, (3) Copied by value (can be slower for large structs), (4) No inheritance, (5) Better cache locality in arrays. Class: (1) Heap allocated, (2) GC overhead, (3) Reference semantics (cheap to pass), (4) Support inheritance, (5) 16-24 byte object header overhead. Use struct when: <= 16 bytes, immutable, frequently allocated. Use class for: mutable data, large objects, inheritance needed. readonly struct prevents defensive copies.

### 10. What are the performance implications of LINQ and when should you avoid it?
**Answer**: LINQ allocations: (1) Enumerator objects for each query, (2) Lambda closures captured in delegates, (3) Intermediate collections (ToList, ToArray), (4) Deferred execution overhead. Hot path alternatives: (1) for/foreach loops, (2) Span-based operations, (3) Pre-sized collections, (4) Manual iteration. LINQ is fine for: cold paths, one-time operations, readability-critical code. Measure impact. LINQ optimizations: (1) IList<T> optimizations, (2) Specialized iterators, (3) Value tuples avoid boxing. In .NET 9, LINQ improved but still allocates vs manual loops.

---

**Last Updated: January 2026 - .NET 9**

**Related Topics**: See also [Native AOT](./06-native-aot.md), [Testing .NET 9](./10-testing-dotnet-9.md)
