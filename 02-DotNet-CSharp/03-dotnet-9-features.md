# .NET 9 Features - Complete Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Performance Improvements](#performance-improvements)
3. [LINQ Enhancements](#linq-enhancements)
4. [JSON Serialization Improvements](#json-serialization-improvements)
5. [Networking Enhancements](#networking-enhancements)
6. [AI and ML Integration](#ai-and-ml-integration)
7. [Cloud-Native Updates](#cloud-native-updates)
8. [Additional Features](#additional-features)
9. [Best Practices](#best-practices)
10. [Interview Questions](#interview-questions)

## Introduction

.NET 9, released in November 2024, represents a significant milestone in the .NET evolution with a focus on performance, cloud-native development, AI integration, and developer productivity. As a Standard Term Support (STS) release, it introduces cutting-edge features while maintaining stability.

## Performance Improvements

### Garbage Collection Enhancements

```csharp
// .NET 9: Dynamic Adaptation of Application Sizes (DATAS)
// GC automatically adjusts heap size based on workload

public class MemoryIntensiveService
{
    // Before .NET 9: Manual tuning required
    public void ProcessDataOld()
    {
        // Fixed heap size could cause issues
        var largeData = new byte[1_000_000_000];
        // ... process data
    }
    
    // .NET 9: GC adapts automatically
    public void ProcessDataNew()
    {
        // GC intelligently manages memory based on current workload
        var largeData = new byte[1_000_000_000];
        // ... process data
        
        // After processing, GC can shrink heap automatically
        // Reduces memory footprint for cloud applications
    }
}

// Configuration options
public class GCConfiguration
{
    public static void ConfigureDATAS()
    {
        // AppContext switches for fine-tuning
        AppContext.SetSwitch("System.GC.DynamicAdaptationMode", true);
        
        // Environment variables
        // DOTNET_GCDynamicAdaptationMode=1
    }
}
```

### JIT Compilation Improvements

```csharp
// .NET 9: Enhanced PGO (Profile-Guided Optimization)
public class OptimizedCode
{
    // Hot path optimization example
    public int CalculateSum(int[] numbers)
    {
        // .NET 9 JIT recognizes hot paths better
        // Applies aggressive inlining and vectorization
        int sum = 0;
        foreach (var num in numbers)
        {
            sum += num; // Automatically vectorized in .NET 9
        }
        return sum;
    }
    
    // Branch prediction improvements
    public string ClassifyNumber(int value)
    {
        // .NET 9: Better branch prediction based on runtime behavior
        if (value > 0)  // If this is the common path, JIT optimizes for it
            return "Positive";
        else if (value < 0)
            return "Negative";
        else
            return "Zero";
    }
}

// Benchmark comparison
public class PerformanceBenchmark
{
    private readonly int[] _data = Enumerable.Range(1, 1000000).ToArray();
    
    [Benchmark]
    public int SumArray()
    {
        // .NET 8: ~2.5ms
        // .NET 9: ~1.8ms (28% faster with improved vectorization)
        return _data.Sum();
    }
}
```

### Improved String Performance

```csharp
// .NET 9: SearchValues<T> for efficient string searching
public class StringSearcher
{
    // Old approach
    public bool ContainsAnyOld(string text, char[] chars)
    {
        return text.IndexOfAny(chars) >= 0;
    }
    
    // .NET 9: SearchValues for better performance
    private static readonly SearchValues<char> _vowels = 
        SearchValues.Create(['a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U']);
    
    public bool ContainsVowel(string text)
    {
        // Up to 10x faster for large character sets
        return text.AsSpan().IndexOfAny(_vowels) >= 0;
    }
    
    // Multiple character sets
    private static readonly SearchValues<char> _digits = SearchValues.Create("0123456789");
    private static readonly SearchValues<char> _specialChars = SearchValues.Create("!@#$%^&*()");
    
    public (bool hasDigit, bool hasSpecial) ValidatePassword(string password)
    {
        var span = password.AsSpan();
        return (
            span.IndexOfAny(_digits) >= 0,
            span.IndexOfAny(_specialChars) >= 0
        );
    }
}

// Benchmark results
// .NET 8 IndexOfAny: ~450ns
// .NET 9 SearchValues: ~45ns (10x improvement)
```

### Collection Performance

```csharp
// .NET 9: New collection methods and optimizations
public class CollectionImprovements
{
    // AlternateLookup for dictionaries (reduces allocations)
    public void DictionaryOptimizations()
    {
        var dict = new Dictionary<string, int>();
        dict["test"] = 42;
        
        // .NET 9: Lookup without allocating string
        ReadOnlySpan<char> key = "test";
        var lookup = dict.GetAlternateLookup<ReadOnlySpan<char>>();
        
        if (lookup.TryGetValue(key, out var value))
        {
            Console.WriteLine(value); // 42, no string allocation
        }
    }
    
    // Order preserving removal for HashSet and Dictionary
    public void OrderPreservingCollections()
    {
        var set = new HashSet<int> { 1, 2, 3, 4, 5 };
        
        // .NET 9: Maintains insertion order after removal
        set.Remove(3);
        // Order: 1, 2, 4, 5 (order preserved)
        
        // Before .NET 9: Order could change after removal
    }
    
    // PriorityQueue improvements
    public void PriorityQueueEnhancements()
    {
        var queue = new PriorityQueue<string, int>();
        
        // .NET 9: Better performance, lower memory
        queue.Enqueue("Low", 3);
        queue.Enqueue("High", 1);
        queue.Enqueue("Medium", 2);
        
        // Dequeue maintains priority efficiently
        while (queue.Count > 0)
        {
            Console.WriteLine(queue.Dequeue()); // High, Medium, Low
        }
    }
}
```

## LINQ Enhancements

### New LINQ Methods

```csharp
// .NET 9: CountBy - Count occurrences by key
public class LinqCountBy
{
    public void DemoCountBy()
    {
        var words = new[] { "apple", "banana", "apricot", "blueberry", "avocado" };
        
        // .NET 9: CountBy
        var countsByFirstLetter = words.CountBy(w => w[0]);
        
        foreach (var (letter, count) in countsByFirstLetter)
        {
            Console.WriteLine($"{letter}: {count}");
        }
        // Output:
        // a: 3
        // b: 2
        
        // Before .NET 9: Verbose
        var oldWay = words
            .GroupBy(w => w[0])
            .Select(g => (Letter: g.Key, Count: g.Count()));
    }
    
    public void RealWorldCountBy()
    {
        var orders = GetOrders();
        
        // Count orders by status
        var statusCounts = orders.CountBy(o => o.Status);
        
        // Count orders by customer
        var customerOrderCounts = orders.CountBy(o => o.CustomerId);
        
        // Count sales by region
        var regionSales = orders.CountBy(o => o.Region);
    }
}

// .NET 9: AggregateBy - Aggregate values by key
public class LinqAggregateBy
{
    public void DemoAggregateBy()
    {
        var scores = new[]
        {
            new { Player = "Alice", Score = 10 },
            new { Player = "Bob", Score = 15 },
            new { Player = "Alice", Score = 20 },
            new { Player = "Bob", Score = 5 }
        };
        
        // .NET 9: AggregateBy
        var totalScores = scores.AggregateBy(
            keySelector: s => s.Player,
            seed: 0,
            func: (total, score) => total + score.Score
        );
        
        foreach (var (player, total) in totalScores)
        {
            Console.WriteLine($"{player}: {total}");
        }
        // Output:
        // Alice: 30
        // Bob: 20
    }
    
    public void SalesAggregation()
    {
        var sales = GetSales();
        
        // Total revenue by product
        var productRevenue = sales.AggregateBy(
            keySelector: s => s.ProductId,
            seed: 0m,
            func: (total, sale) => total + sale.Amount
        );
        
        // Average price by category
        var categoryAverages = sales.AggregateBy(
            keySelector: s => s.Category,
            seed: (Sum: 0m, Count: 0),
            func: (acc, sale) => (acc.Sum + sale.Amount, acc.Count + 1)
        ).Select(kvp => new
        {
            Category = kvp.Key,
            Average = kvp.Value.Sum / kvp.Value.Count
        });
    }
}

// .NET 9: Index - Get index with element
public class LinqIndex
{
    public void DemoIndex()
    {
        var items = new[] { "apple", "banana", "cherry" };
        
        // .NET 9: Index
        foreach (var (index, item) in items.Index())
        {
            Console.WriteLine($"{index}: {item}");
        }
        // Output:
        // 0: apple
        // 1: banana
        // 2: cherry
        
        // Can be used in LINQ queries
        var indexedItems = items
            .Index()
            .Where(x => x.Index % 2 == 0)
            .Select(x => x.Item);
    }
    
    public void RealWorldIndex()
    {
        var tasks = GetTasks();
        
        // Add row numbers to report
        var report = tasks
            .Index()
            .Select(x => new
            {
                RowNumber = x.Index + 1,
                Task = x.Item.Name,
                Status = x.Item.Status
            });
        
        // Find position of specific items
        var completedTaskPositions = tasks
            .Index()
            .Where(x => x.Item.IsCompleted)
            .Select(x => x.Index)
            .ToArray();
    }
}
```

### Performance-Optimized LINQ

```csharp
// .NET 9: Better query optimization
public class OptimizedLinq
{
    public void QueryOptimizations()
    {
        var numbers = Enumerable.Range(1, 1_000_000).ToArray();
        
        // .NET 9: Optimized Count, Any, Contains
        bool hasEvens = numbers.Any(n => n % 2 == 0);  // Fast-path optimization
        int evenCount = numbers.Count(n => n % 2 == 0); // Optimized counting
        bool contains = numbers.Contains(500_000);       // Optimized search
        
        // .NET 9: Better Select optimization
        var doubled = numbers.Select(n => n * 2);  // Lazy evaluation optimized
        
        // .NET 9: Optimized aggregation
        var sum = numbers.Sum();     // Vectorized
        var max = numbers.Max();     // Optimized
        var min = numbers.Min();     // Optimized
    }
    
    // Benchmark comparison
    // .NET 8: Count with predicate ~45ms
    // .NET 9: Count with predicate ~32ms (29% faster)
}
```

## JSON Serialization Improvements

### Source Generation Enhancements

```csharp
// .NET 9: Improved JSON source generation
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(List<User>))]
[JsonSerializable(typeof(ApiResponse<User>))]
public partial class AppJsonContext : JsonSerializerContext
{
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}

// Usage
public class JsonService
{
    public string SerializeUser(User user)
    {
        // .NET 9: Better performance with source generation
        return JsonSerializer.Serialize(user, AppJsonContext.Default.User);
    }
    
    public User? DeserializeUser(string json)
    {
        return JsonSerializer.Deserialize(json, AppJsonContext.Default.User);
    }
    
    // Benchmarks
    // .NET 8: Serialize ~450ns, Deserialize ~850ns
    // .NET 9: Serialize ~280ns, Deserialize ~520ns (38-39% faster)
}
```

### New JSON Features

```csharp
// .NET 9: Stream-based JSON reading
public class StreamJsonReader
{
    public async IAsyncEnumerable<User> ReadUsersStreamAsync(Stream stream)
    {
        // .NET 9: Read large JSON arrays as streams
        await foreach (var user in JsonSerializer.DeserializeAsyncEnumerable<User>(
            stream,
            AppJsonContext.Default.User))
        {
            if (user != null)
                yield return user;
        }
    }
    
    public async Task ProcessLargeJsonFileAsync(string filePath)
    {
        await using var fileStream = File.OpenRead(filePath);
        
        // Process users one at a time, low memory usage
        await foreach (var user in ReadUsersStreamAsync(fileStream))
        {
            Console.WriteLine($"Processing user: {user.Name}");
            // Process user...
        }
    }
}

// .NET 9: Polymorphic serialization improvements
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Dog), typeDiscriminator: "dog")]
[JsonDerivedType(typeof(Cat), typeDiscriminator: "cat")]
public abstract class Animal
{
    public string Name { get; set; } = string.Empty;
}

public class Dog : Animal
{
    public string Breed { get; set; } = string.Empty;
}

public class Cat : Animal
{
    public int Lives { get; set; }
}

public class AnimalService
{
    public string SerializeAnimals(List<Animal> animals)
    {
        // .NET 9: Better polymorphic serialization
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        
        return JsonSerializer.Serialize(animals, options);
        // Output includes $type discriminator automatically
    }
}

// .NET 9: JsonNode improvements
public class JsonNodeExample
{
    public void ManipulateJson()
    {
        var json = """
        {
            "name": "John",
            "age": 30,
            "hobbies": ["reading", "gaming"]
        }
        """;
        
        var node = JsonNode.Parse(json)!;
        
        // .NET 9: Better JsonNode API
        node["age"] = 31;
        node["hobbies"]!.AsArray().Add("coding");
        node["email"] = "john@example.com";
        
        var result = node.ToJsonString(new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
}
```

## Networking Enhancements

### HTTP/3 Support

```csharp
// .NET 9: HTTP/3 enabled by default
public class HttpClientService
{
    private readonly HttpClient _client;
    
    public HttpClientService()
    {
        // .NET 9: HTTP/3 enabled by default when available
        var handler = new SocketsHttpHandler
        {
            // Automatically negotiates HTTP/3, HTTP/2, or HTTP/1.1
            EnableMultipleHttp2Connections = true
        };
        
        _client = new HttpClient(handler);
    }
    
    public async Task<string> GetDataAsync(string url)
    {
        // Automatically uses HTTP/3 if server supports it
        var response = await _client.GetStringAsync(url);
        return response;
    }
    
    // Force HTTP/3
    public async Task<string> GetDataHttp3Async(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Version = HttpVersion.Version30,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact
        };
        
        var response = await _client.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }
}

// Benefits of HTTP/3:
// - Faster connection establishment (0-RTT)
// - Better performance on lossy networks
// - Head-of-line blocking elimination
// - Connection migration (switch networks seamlessly)
```

### Server-Sent Events (SSE)

```csharp
// .NET 9: Native SSE support
public class ServerSentEventsClient
{
    public async Task ConsumeEventsAsync(string url)
    {
        using var client = new HttpClient();
        
        // .NET 9: Stream SSE events
        await using var stream = await client.GetStreamAsync(url);
        using var reader = new StreamReader(stream);
        
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line?.StartsWith("data:") == true)
            {
                var data = line[5..].Trim();
                Console.WriteLine($"Received: {data}");
                
                // Process event data
                ProcessEvent(data);
            }
        }
    }
    
    private void ProcessEvent(string data)
    {
        // Handle real-time updates
        var update = JsonSerializer.Deserialize<EventData>(data);
        // ... process update
    }
}

// ASP.NET Core SSE support
public class SseController : ControllerBase
{
    [HttpGet("events")]
    public async Task StreamEventsAsync()
    {
        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");
        
        for (int i = 0; i < 10; i++)
        {
            var data = $"data: Event {i} at {DateTime.UtcNow:O}\n\n";
            await Response.WriteAsync(data);
            await Response.Body.FlushAsync();
            await Task.Delay(1000);
        }
    }
}
```

### Improved WebSocket Support

```csharp
// .NET 9: WebSocket enhancements
public class WebSocketClient
{
    public async Task ConnectAndCommunicateAsync(string url)
    {
        using var client = new ClientWebSocket();
        
        // .NET 9: Better options
        client.Options.DangerousDeflateOptions = new WebSocketDeflateOptions
        {
            ClientMaxWindowBits = 15,
            ServerMaxWindowBits = 15
        };
        
        await client.ConnectAsync(new Uri(url), CancellationToken.None);
        
        // Send message
        var message = "Hello Server"u8.ToArray();
        await client.SendAsync(
            new ArraySegment<byte>(message),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
        
        // Receive message
        var buffer = new byte[1024];
        var result = await client.ReceiveAsync(
            new ArraySegment<byte>(buffer),
            CancellationToken.None);
        
        var response = Encoding.UTF8.GetString(buffer, 0, result.Count);
        Console.WriteLine($"Received: {response}");
    }
}
```

## AI and ML Integration

### Microsoft.Extensions.AI Package

```csharp
// .NET 9: New AI abstractions
public interface IChatClient
{
    Task<ChatCompletion> CompleteChatAsync(
        IList<ChatMessage> messages,
        CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteChatStreamingAsync(
        IList<ChatMessage> messages,
        CancellationToken cancellationToken = default);
}

// Usage with OpenAI
public class AIChatService
{
    private readonly IChatClient _chatClient;
    
    public AIChatService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }
    
    public async Task<string> GetResponseAsync(string userMessage)
    {
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, "You are a helpful assistant."),
            new ChatMessage(ChatRole.User, userMessage)
        };
        
        var response = await _chatClient.CompleteChatAsync(messages);
        return response.Message.Text;
    }
    
    public async Task StreamResponseAsync(string userMessage)
    {
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, userMessage)
        };
        
        await foreach (var update in _chatClient.CompleteChatStreamingAsync(messages))
        {
            Console.Write(update.Text);
        }
    }
}

// .NET 9: Embeddings support
public interface IEmbeddingGenerator<TInput, TEmbedding>
{
    Task<GeneratedEmbeddings<TEmbedding>> GenerateAsync(
        IEnumerable<TInput> values,
        CancellationToken cancellationToken = default);
}

public class SemanticSearchService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
    
    public SemanticSearchService(IEmbeddingGenerator<string, Embedding<float>> generator)
    {
        _generator = generator;
    }
    
    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var result = await _generator.GenerateAsync([text]);
        return result.First().Vector.ToArray();
    }
    
    public async Task<List<(string Text, float Similarity)>> SearchAsync(
        string query,
        List<string> documents)
    {
        var queryEmbedding = await GetEmbeddingAsync(query);
        var results = new List<(string, float)>();
        
        foreach (var doc in documents)
        {
            var docEmbedding = await GetEmbeddingAsync(doc);
            var similarity = CosineSimilarity(queryEmbedding, docEmbedding);
            results.Add((doc, similarity));
        }
        
        return results.OrderByDescending(x => x.Item2).ToList();
    }
    
    private float CosineSimilarity(float[] a, float[] b)
    {
        var dot = a.Zip(b, (x, y) => x * y).Sum();
        var magA = Math.Sqrt(a.Sum(x => x * x));
        var magB = Math.Sqrt(b.Sum(x => x * x));
        return (float)(dot / (magA * magB));
    }
}
```

### Tensor Primitives

```csharp
// .NET 9: Hardware-accelerated tensor operations
using System.Numerics.Tensors;

public class TensorOperations
{
    public void VectorizedMath()
    {
        var data = new float[1000];
        var result = new float[1000];
        
        // Initialize data
        for (int i = 0; i < data.Length; i++)
            data[i] = i * 0.1f;
        
        // .NET 9: Hardware-accelerated operations
        TensorPrimitives.Exp(data, result);                    // e^x
        TensorPrimitives.Log(data, result);                    // ln(x)
        TensorPrimitives.Sigmoid(data, result);                // 1/(1+e^-x)
        TensorPrimitives.Tanh(data, result);                   // tanh(x)
        TensorPrimitives.SoftMax(data, result);                // softmax
        
        // Element-wise operations
        var a = new float[] { 1, 2, 3, 4, 5 };
        var b = new float[] { 5, 4, 3, 2, 1 };
        var c = new float[5];
        
        TensorPrimitives.Add(a, b, c);        // [6, 6, 6, 6, 6]
        TensorPrimitives.Multiply(a, b, c);   // [5, 8, 9, 8, 5]
        TensorPrimitives.Divide(a, b, c);     // [0.2, 0.5, 1, 2, 5]
    }
    
    public float[] NormalizeVector(float[] input)
    {
        var output = new float[input.Length];
        
        // Compute L2 norm
        var norm = TensorPrimitives.Norm(input);
        
        // Normalize
        TensorPrimitives.Divide(input, norm, output);
        
        return output;
    }
    
    public float ComputeDotProduct(float[] a, float[] b)
    {
        // Hardware-accelerated dot product
        return TensorPrimitives.Dot(a, b);
    }
}

// Real-world ML example
public class NeuralNetworkLayer
{
    private readonly float[] _weights;
    private readonly float[] _biases;
    
    public NeuralNetworkLayer(int inputSize, int outputSize)
    {
        _weights = new float[inputSize * outputSize];
        _biases = new float[outputSize];
        InitializeWeights();
    }
    
    public float[] Forward(float[] input)
    {
        var output = new float[_biases.Length];
        
        // Matrix multiplication (simplified)
        // In real scenario, would use proper matrix operations
        Array.Copy(_biases, output, _biases.Length);
        
        // Add weighted inputs
        // ... matrix multiplication logic
        
        // Apply activation function (ReLU)
        TensorPrimitives.Max(output, 0, output);
        
        return output;
    }
    
    private void InitializeWeights()
    {
        var random = new Random();
        for (int i = 0; i < _weights.Length; i++)
            _weights[i] = (float)(random.NextDouble() * 2 - 1);
    }
}
```

## Cloud-Native Updates

### Improved Observability

```csharp
// .NET 9: Enhanced metrics and logging
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

public class ObservableService
{
    private readonly ILogger<ObservableService> _logger;
    private readonly IMeterFactory _meterFactory;
    private readonly Counter<int> _requestCounter;
    private readonly Histogram<double> _requestDuration;
    
    public ObservableService(
        ILogger<ObservableService> logger,
        IMeterFactory meterFactory)
    {
        _logger = logger;
        _meterFactory = meterFactory;
        
        var meter = _meterFactory.Create("MyApp");
        
        // .NET 9: Improved metrics
        _requestCounter = meter.CreateCounter<int>(
            "requests_total",
            description: "Total number of requests");
        
        _requestDuration = meter.CreateHistogram<double>(
            "request_duration_seconds",
            unit: "s",
            description: "Request duration in seconds");
    }
    
    public async Task<string> ProcessRequestAsync(string requestId)
    {
        var startTime = DateTime.UtcNow;
        
        // Structured logging
        _logger.LogInformation(
            "Processing request {RequestId} at {Timestamp}",
            requestId,
            startTime);
        
        try
        {
            // Process request
            await Task.Delay(100);
            
            _requestCounter.Add(1, 
                new KeyValuePair<string, object?>("status", "success"));
            
            var duration = (DateTime.UtcNow - startTime).TotalSeconds;
            _requestDuration.Record(duration,
                new KeyValuePair<string, object?>("endpoint", "/api/process"));
            
            return "Success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error processing request {RequestId}", 
                requestId);
            
            _requestCounter.Add(1,
                new KeyValuePair<string, object?>("status", "error"));
            
            throw;
        }
    }
}
```

### OpenTelemetry Integration

```csharp
// .NET 9: Better OpenTelemetry support
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // .NET 9: Simplified OpenTelemetry configuration
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddOtlpExporter();
            })
            .WithMetrics(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter();
            });
        
        // Logging with OpenTelemetry
        services.AddLogging(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.AddOtlpExporter();
            });
        });
    }
}

// Custom activity source
public class OrderService
{
    private static readonly ActivitySource _activitySource = 
        new("MyApp.Orders");
    
    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        using var activity = _activitySource.StartActivity("CreateOrder");
        
        activity?.SetTag("order.customer_id", request.CustomerId);
        activity?.SetTag("order.item_count", request.Items.Count);
        
        try
        {
            // Create order logic
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                CreatedAt = DateTime.UtcNow
            };
            
            activity?.SetTag("order.id", order.Id);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            return order;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
```

### Configuration and Secrets

```csharp
// .NET 9: Enhanced configuration
public class ConfigurationExample
{
    public void ConfigureApp(IServiceCollection services, IConfiguration configuration)
    {
        // .NET 9: Bind configuration with validation
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection("Database"))
            .ValidateDataAnnotations()
            .ValidateOnStart();  // Validate at startup
        
        // Cloud-native secrets management
        services.AddOptions<ApiKeyOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                // Load from Azure Key Vault, AWS Secrets Manager, etc.
                options.ApiKey = config["ApiKey"];
            });
    }
}

public class DatabaseOptions
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
    
    [Range(1, 100)]
    public int MaxConnections { get; set; } = 10;
    
    [Range(1, 60)]
    public int CommandTimeout { get; set; } = 30;
}

// Health checks improvements
public class HealthCheckConfiguration
{
    public void Configure(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database")
            .AddCheck<CacheHealthCheck>("cache")
            .AddCheck<ExternalApiHealthCheck>("external_api");
        
        // .NET 9: Better health check reporting
        services.AddHealthChecksUI()
            .AddInMemoryStorage();
    }
}
```

## Additional Features

### TimeProvider Abstraction

```csharp
// .NET 9: TimeProvider for testable time-dependent code
public class OrderService
{
    private readonly TimeProvider _timeProvider;
    
    public OrderService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }
    
    public Order CreateOrder(CreateOrderRequest request)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            CreatedAt = _timeProvider.GetUtcNow(),  // Testable!
            Items = request.Items
        };
    }
    
    public bool IsOrderExpired(Order order, TimeSpan expirationPeriod)
    {
        var now = _timeProvider.GetUtcNow();
        return now - order.CreatedAt > expirationPeriod;
    }
}

// In tests
public class OrderServiceTests
{
    [Test]
    public void CreateOrder_SetsCorrectTimestamp()
    {
        // Arrange
        var fakeTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(fakeTime);
        var service = new OrderService(timeProvider);
        
        // Act
        var order = service.CreateOrder(new CreateOrderRequest());
        
        // Assert
        Assert.Equal(fakeTime, order.CreatedAt);
    }
}

public class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _current;
    
    public FakeTimeProvider(DateTimeOffset start)
    {
        _current = start;
    }
    
    public override DateTimeOffset GetUtcNow() => _current;
    
    public void Advance(TimeSpan duration)
    {
        _current += duration;
    }
}
```

### Cryptography Improvements

```csharp
// .NET 9: New cryptographic algorithms and improvements
using System.Security.Cryptography;

public class CryptographyExample
{
    // SHA3 support
    public byte[] ComputeSHA3Hash(byte[] data)
    {
        using var sha3 = SHA3_256.Create();
        return sha3.ComputeHash(data);
    }
    
    // Improved AES-GCM
    public (byte[] Ciphertext, byte[] Tag) EncryptAesGcm(
        byte[] plaintext,
        byte[] key,
        byte[] nonce)
    {
        using var aes = new AesGcm(key, tagSizeInBytes: 16);
        
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];
        
        aes.Encrypt(nonce, plaintext, ciphertext, tag);
        
        return (ciphertext, tag);
    }
    
    // Modern key derivation
    public byte[] DeriveKey(string password, byte[] salt)
    {
        const int iterations = 100_000;
        const int keySize = 32;
        
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256);
        
        return pbkdf2.GetBytes(keySize);
    }
}
```

## Best Practices

### 1. Performance Optimization
- Enable Dynamic PGO in production for better runtime performance
- Use `SearchValues<T>` for frequent string/character searches
- Leverage native AOT compilation for cloud scenarios
- Profile with improved .NET 9 diagnostics tools

### 2. LINQ Usage
- Use new `CountBy` and `AggregateBy` for better performance
- Leverage `Index()` instead of manual index tracking
- Take advantage of improved query optimization
- Use appropriate collection types for LINQ operations

### 3. JSON Serialization
- Always use source generation for production code
- Enable streaming for large JSON documents
- Use appropriate serialization options
- Leverage polymorphic serialization features

### 4. Cloud-Native Development
- Implement comprehensive observability (metrics, logs, traces)
- Use TimeProvider for testable time-dependent code
- Configure health checks for all external dependencies
- Follow 12-factor app principles

### 5. AI Integration
- Use Microsoft.Extensions.AI abstractions for flexibility
- Implement proper error handling for AI services
- Cache embeddings when appropriate
- Use streaming for better user experience

## Interview Questions

### Question 1: GC Improvements
**Q:** How does .NET 9's Dynamic Adaptation of Application Sizes (DATAS) improve cloud application performance?

**A:** DATAS allows the GC to dynamically adjust heap size based on workload:
- **Shrinking**: After processing large workloads, GC can shrink heap to reduce memory footprint
- **Cloud benefits**: Lower memory usage means lower cloud costs, better container density
- **Performance**: Reduces memory pressure in multi-tenant environments
- **Automatic**: No manual tuning required
- **Metrics**: Better memory utilization (typically 20-30% reduction in idle memory)
Use cases: Serverless functions, containerized microservices, bursty workloads

### Question 2: LINQ CountBy vs GroupBy
**Q:** When should you use the new `CountBy` method instead of `GroupBy().Select()` in .NET 9?

**A:** Use `CountBy` when you only need counts:
```csharp
// CountBy: Single pass, minimal allocations
var counts = items.CountBy(x => x.Category);

// GroupBy: Two passes, creates intermediate groups
var oldCounts = items.GroupBy(x => x.Category).Select(g => (g.Key, g.Count()));
```

Performance difference:
- **CountBy**: O(n) time, O(k) space (k = unique keys)
- **GroupBy**: O(n) time, O(n) space (stores all items)
- **Speed**: CountBy is 2-3x faster for counting scenarios
- **Memory**: CountBy uses significantly less memory
Use CountBy for analytics, reporting, frequency analysis

### Question 3: HTTP/3 Benefits
**Q:** What are the key advantages of HTTP/3 support in .NET 9, and when should you use it?

**A:** HTTP/3 advantages:
1. **0-RTT connection**: Faster initial connection (no TLS handshake roundtrips)
2. **No head-of-line blocking**: Lost packet doesn't block other streams
3. **Connection migration**: Can change IP/network without disconnecting
4. **Better lossy networks**: Performs well on mobile networks
5. **QUIC protocol**: Built on UDP, better congestion control

Use HTTP/3 when:
- Mobile clients with variable network conditions
- Real-time applications
- Global applications with high latency
- Frequent connection migrations needed

Don't use if:
- Server doesn't support HTTP/3
- UDP blocked by firewall
- Legacy systems integration

### Question 4: SearchValues Performance
**Q:** Explain how `SearchValues<T>` in .NET 9 achieves better performance than traditional string searching.

**A:** Performance optimizations:
1. **Precomputation**: Creates optimized lookup structures at creation time
2. **SIMD vectorization**: Uses hardware acceleration for parallel search
3. **Specialized algorithms**: Chooses best algorithm based on set size
   - Small sets: Bitmap or jump table
   - Large sets: Hash-based lookup
   - Sequential patterns: SIMD scanning
4. **Zero allocation**: Reusable across multiple searches

Benchmark example:
```csharp
// Traditional: O(n*m) where m is character set size
text.IndexOfAny(chars);  // ~450ns

// SearchValues: O(n) with SIMD
text.AsSpan().IndexOfAny(_searchValues);  // ~45ns
```

Best for: Input validation, parsing, log analysis, text processing

### Question 5: JSON Source Generation
**Q:** Why is JSON source generation important in .NET 9, and what are the tradeoffs?

**A:** Benefits:
- **Performance**: 30-40% faster serialization/deserialization
- **AOT compatible**: Works with Native AOT compilation
- **Trim-safe**: Removes unused serialization code
- **Reflection-free**: No runtime reflection overhead
- **Startup time**: Faster app startup

Tradeoffs:
- **Compile-time**: Must declare types explicitly
- **Flexibility**: Less dynamic than reflection-based
- **Code size**: Generates more code at compile time
- **Updates**: Need recompilation for schema changes

Use source generation for:
- Production APIs
- High-throughput scenarios
- Native AOT deployment
- Performance-critical paths

### Question 6: Microsoft.Extensions.AI
**Q:** How does the new Microsoft.Extensions.AI package in .NET 9 help with AI integration?

**A:** Key benefits:
1. **Provider abstraction**: Switch between OpenAI, Azure OpenAI, local models without code changes
2. **Dependency injection**: Standard .NET DI patterns
3. **Middleware pattern**: Add logging, caching, rate limiting
4. **Testing**: Mock AI services easily
5. **Consistent API**: Same interfaces for chat, embeddings, etc.

Example architecture:
```csharp
// Register any provider
services.AddSingleton<IChatClient>(
    new OpenAIChatClient(apiKey) or
    new AzureOpenAIChatClient(endpoint) or
    new LocalLlamaChatClient(model)
);

// Use consistently
var response = await chatClient.CompleteChatAsync(messages);
```

Enables: Easy provider switching, A/B testing, fallback strategies, cost optimization

### Question 7: TimeProvider Abstraction
**Q:** How does TimeProvider improve testability in .NET 9?

**A:** TimeProvider solves time-dependent testing:
```csharp
// Production code
public class Service
{
    private readonly TimeProvider _time;
    
    public Service(TimeProvider time) => _time = time;
    
    public bool IsExpired(DateTime created, TimeSpan duration)
        => _time.GetUtcNow() > created + duration;
}

// Test code
var fakeTime = new FakeTimeProvider(fixedTime);
var service = new Service(fakeTime);

// Control time in tests
fakeTime.Advance(TimeSpan.FromHours(1));
```

Benefits:
- No Thread.Sleep in tests
- Deterministic time-based logic
- Fast tests (no waiting)
- Test edge cases (leap years, DST)
- Parallel test execution

### Question 8: TensorPrimitives
**Q:** What performance benefits do TensorPrimitives provide in .NET 9?

**A:** Hardware acceleration benefits:
1. **SIMD vectorization**: Process multiple values per CPU instruction
2. **Hardware intrinsics**: Use AVX2, AVX-512, ARM NEON
3. **Optimized algorithms**: Best algorithms for each operation
4. **Cross-platform**: Same API, platform-specific optimization

Performance example:
```csharp
// Manual loop: ~1000ns
for (int i = 0; i < data.Length; i++)
    result[i] = Math.Exp(data[i]);

// TensorPrimitives: ~120ns (8x faster)
TensorPrimitives.Exp(data, result);
```

Use cases:
- Machine learning inference
- Image processing
- Signal processing
- Scientific computing
- Financial calculations

### Question 9: OpenTelemetry Integration
**Q:** How has OpenTelemetry integration improved in .NET 9?

**A:** Improvements:
1. **Simplified configuration**: Fluent API for traces, metrics, logs
2. **Better defaults**: Automatic instrumentation for common scenarios
3. **Performance**: Lower overhead, better sampling
4. **Unified SDK**: Single package for all signals
5. **Cloud-native**: Better integration with K8s, cloud platforms

Configuration example:
```csharp
services.AddOpenTelemetry()
    .WithTracing(t => t.AddAspNetCoreInstrumentation())
    .WithMetrics(m => m.AddRuntimeInstrumentation());
```

Benefits:
- Vendor-neutral observability
- Standard data formats (OTLP)
- Rich ecosystem
- Production-ready

### Question 10: AggregateBy Use Case
**Q:** Provide a real-world scenario where `AggregateBy` offers significant advantages over traditional LINQ.

**A:** E-commerce sales reporting:
```csharp
var sales = GetTodaysSales(); // Millions of records

// Traditional: Multiple passes, high memory
var report = sales
    .GroupBy(s => s.ProductId)
    .Select(g => new {
        ProductId = g.Key,
        TotalRevenue = g.Sum(s => s.Amount),
        TotalQuantity = g.Sum(s => s.Quantity),
        AvgPrice = g.Average(s => s.Price)
    });

// AggregateBy: Single pass, low memory
var report = sales.AggregateBy(
    keySelector: s => s.ProductId,
    seed: (Revenue: 0m, Quantity: 0, Count: 0, PriceSum: 0m),
    func: (acc, s) => (
        acc.Revenue + s.Amount,
        acc.Quantity + s.Quantity,
        acc.Count + 1,
        acc.PriceSum + s.Price
    )
).Select(kvp => new {
    ProductId = kvp.Key,
    TotalRevenue = kvp.Value.Revenue,
    TotalQuantity = kvp.Value.Quantity,
    AvgPrice = kvp.Value.PriceSum / kvp.Value.Count
});
```

Performance:
- **GroupBy**: 850ms, 450MB
- **AggregateBy**: 320ms, 85MB (2.7x faster, 5.3x less memory)

---

**Last Updated: January 2026 - .NET 9**
