# Native AOT in .NET 9

## Table of Contents
- [What is Native AOT?](#what-is-native-aot)
- [How Native AOT Works](#how-native-aot-works)
- [Benefits of Native AOT](#benefits-of-native-aot)
- [Limitations and Constraints](#limitations-and-constraints)
- [Publishing AOT Applications](#publishing-aot-applications)
- [Compatibility Analysis](#compatibility-analysis)
- [Performance Comparisons](#performance-comparisons)
- [Real-World Scenarios](#real-world-scenarios)
- [Best Practices](#best-practices)
- [Interview Questions](#interview-questions)

## What is Native AOT?

Native Ahead-of-Time (AOT) compilation is a deployment model introduced in .NET 7 and significantly enhanced in .NET 9. It compiles your .NET application directly to native code during publish, rather than relying on Just-In-Time (JIT) compilation at runtime.

### Key Characteristics

- **Ahead-of-Time Compilation**: Code is compiled to native machine code before execution
- **Self-Contained Deployment**: No .NET runtime required on target machine
- **Single-File Executable**: Produces a single native executable
- **Platform-Specific**: Compiled binaries are specific to the target OS and architecture

```csharp
// Example: Simple Native AOT-compatible console application
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine($"Hello from Native AOT!");
        Console.WriteLine($"Started at: {DateTime.UtcNow}");
        
        // Direct method calls work perfectly with AOT
        ProcessData(args);
    }
    
    static void ProcessData(string[] data)
    {
        foreach (var item in data)
        {
            Console.WriteLine($"Processing: {item}");
        }
    }
}
```

## How Native AOT Works

### Compilation Pipeline

1. **IL Code Generation**: C# compiler generates Intermediate Language (IL)
2. **AOT Compilation**: ILCompiler analyzes and compiles IL to native code
3. **Trimming**: Unused code is removed (tree shaking)
4. **Native Code Generation**: Platform-specific native code is generated
5. **Linking**: Native code is linked into a single executable

```xml
<!-- Project file configuration for Native AOT -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>true</InvariantGlobalization>
    <IlcOptimizationPreference>Speed</IlcOptimizationPreference>
    <IlcGenerateStackTraceData>false</IlcGenerateStackTraceData>
  </PropertyGroup>
</Project>
```

### Trimming Process

Native AOT relies heavily on trimming to reduce application size:

```csharp
// Trimming-friendly code
public class DataProcessor
{
    // Direct type usage - trim-safe
    public void ProcessString(string input)
    {
        var result = input.ToUpper();
        Console.WriteLine(result);
    }
    
    // Generic methods are preserved when used
    public T Transform<T>(T value) where T : class
    {
        return value;
    }
}

// Usage that preserves the method
var processor = new DataProcessor();
processor.ProcessString("test");
processor.Transform<string>("data");
```

## Benefits of Native AOT

### 1. Startup Time

Native AOT applications start significantly faster because there's no JIT compilation overhead:

```csharp
// Measuring startup time
using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        var startTime = Stopwatch.GetTimestamp();
        
        // Application logic
        Console.WriteLine("Application started");
        
        var elapsedMs = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
        Console.WriteLine($"Startup time: {elapsedMs}ms");
        // Native AOT: ~5-15ms
        // JIT: ~50-200ms
    }
}
```

### 2. Memory Footprint

Reduced memory consumption due to no JIT compiler or IL metadata:

```csharp
// Memory-efficient Native AOT application
using System;

class MemoryEfficientApp
{
    static void Main()
    {
        var beforeMem = GC.GetTotalMemory(true);
        
        ProcessLargeData();
        
        var afterMem = GC.GetTotalMemory(true);
        Console.WriteLine($"Memory used: {(afterMem - beforeMem) / 1024}KB");
        // Native AOT: Lower base memory usage
    }
    
    static void ProcessLargeData()
    {
        Span<int> numbers = stackalloc int[1000];
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = i * 2;
        }
    }
}
```

### 3. Self-Contained Deployment

No external runtime dependencies:

```bash
# Single executable with no runtime requirements
./MyApp
# vs JIT requiring:
# dotnet MyApp.dll
```

### 4. Smaller Distribution Size

With trimming, Native AOT apps can be very small:

```
Comparison (Simple API):
- JIT self-contained: ~85 MB
- Native AOT (default): ~15 MB
- Native AOT (optimized): ~8 MB
- Native AOT (size-optimized): ~3 MB
```

## Limitations and Constraints

### 1. Reflection Limitations

Native AOT has limited support for reflection:

```csharp
// ❌ This will fail at runtime with Native AOT
public class ReflectionExample
{
    public void UnsafeReflection()
    {
        var typeName = "System.String";
        var type = Type.GetType(typeName); // May return null
        var method = type?.GetMethod("Concat"); // Unreliable
    }
}

// ✅ This works - direct type access
public class SafeApproach
{
    public void SafeCode()
    {
        var type = typeof(string);
        var method = type.GetMethod("Concat");
        // Works because type is known at compile time
    }
}

// ✅ Use source generators instead
[JsonSerializable(typeof(Person))]
public partial class PersonContext : JsonSerializerContext { }

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

// Usage
var person = new Person { Name = "John", Age = 30 };
var json = JsonSerializer.Serialize(person, PersonContext.Default.Person);
```

### 2. Dynamic Code Generation

Dynamic code generation is not supported:

```csharp
// ❌ Avoid dynamic code generation
public class DynamicCode
{
    public void EmitCode()
    {
        // System.Reflection.Emit is not supported
        var dynamicMethod = new DynamicMethod("Test", null, null);
        // Will fail with Native AOT
    }
}

// ✅ Use static alternatives
public class StaticAlternative
{
    // Use expression trees compiled at build time
    private static readonly Func<int, int, int> Add = (a, b) => a + b;
    
    public int AddNumbers(int x, int y)
    {
        return Add(x, y);
    }
}
```

### 3. Assembly Loading

Runtime assembly loading is not supported:

```csharp
// ❌ Dynamic assembly loading
public class DynamicLoading
{
    public void LoadAssembly()
    {
        Assembly.LoadFrom("Plugin.dll"); // Not supported
    }
}

// ✅ Static references
public class StaticReferences
{
    public void UseStaticReference()
    {
        // Add assembly reference at compile time
        var plugin = new MyPlugin.PluginClass();
        plugin.Execute();
    }
}
```

## Publishing AOT Applications

### Basic Publishing

```bash
# Publish for current platform
dotnet publish -c Release

# Publish for specific runtime
dotnet publish -c Release -r win-x64
dotnet publish -c Release -r linux-x64
dotnet publish -c Release -r osx-arm64

# With size optimization
dotnet publish -c Release -r linux-x64 /p:IlcOptimizationPreference=Size
```

### Advanced Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <PublishAot>true</PublishAot>
    
    <!-- Trimming Settings -->
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>full</TrimMode>
    
    <!-- AOT Optimization -->
    <IlcOptimizationPreference>Speed</IlcOptimizationPreference>
    
    <!-- Size Reduction -->
    <IlcGenerateStackTraceData>false</IlcGenerateStackTraceData>
    <IlcGenerateDgmlFile>false</IlcGenerateDgmlFile>
    <IlcFoldIdenticalMethodBodies>true</IlcFoldIdenticalMethodBodies>
    
    <!-- Globalization -->
    <InvariantGlobalization>true</InvariantGlobalization>
    
    <!-- Debug Settings -->
    <DebuggerSupport>false</DebuggerSupport>
    <EnableUnsafeBinaryFormatterSerialization>false</EnableUnsafeBinaryFormatterSerialization>
  </PropertyGroup>
</Project>
```

### Trimming Warnings

```csharp
// Suppress trimming warnings when you know code is safe
using System.Diagnostics.CodeAnalysis;

public class TrimSafe
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    public static void ProcessType(Type type)
    {
        // Tell trimmer to preserve public properties
        foreach (var prop in type.GetProperties())
        {
            Console.WriteLine(prop.Name);
        }
    }
    
    // Preserve entire type from trimming
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Person))]
    public static void EnsurePersonPreserved()
    {
        // Person type will not be trimmed
    }
}
```

## Compatibility Analysis

### Checking AOT Compatibility

```bash
# Analyze project for AOT compatibility
dotnet publish -c Release -r linux-x64 /p:PublishAot=true --no-build /p:_WarnOnTrimming=true
```

### Common Compatibility Issues

```csharp
// Issue 1: JSON Serialization without source generators
// ❌ Not AOT-compatible
var json = JsonSerializer.Serialize(new { Name = "John" });

// ✅ AOT-compatible with source generators
[JsonSerializable(typeof(Person))]
partial class AppJsonContext : JsonSerializerContext { }

var person = new Person { Name = "John" };
var json = JsonSerializer.Serialize(person, AppJsonContext.Default.Person);

// Issue 2: LINQ expressions on IQueryable
// ❌ May not work with some providers
IQueryable<Person> query = dbContext.People;
var result = query.Where(p => p.Name == "John");

// ✅ Use compiled queries
var compiled = EF.CompileQuery((AppDbContext ctx, string name) 
    => ctx.People.Where(p => p.Name == name));
```

## Performance Comparisons

### Startup Time Benchmarks

```
Application Type          | JIT    | Native AOT | Improvement
--------------------------|--------|------------|------------
Console App (Hello World) | 180ms  | 8ms        | 22.5x
Minimal API              | 450ms  | 35ms       | 12.9x
Web API with EF Core     | 1200ms | 95ms       | 12.6x
gRPC Service             | 350ms  | 28ms       | 12.5x
```

### Memory Usage Benchmarks

```
Application Type          | JIT    | Native AOT | Reduction
--------------------------|--------|------------|----------
Console App              | 28 MB  | 12 MB      | 57%
Minimal API              | 65 MB  | 32 MB      | 51%
Web API with EF Core     | 95 MB  | 58 MB      | 39%
```

### Distribution Size

```
Configuration                    | Size
---------------------------------|-------
JIT Self-Contained              | 85 MB
Native AOT Default              | 15 MB
Native AOT with Size Optimization| 8 MB
Native AOT Minimal              | 3 MB
```

## Real-World Scenarios

### 1. Minimal API with Native AOT

```csharp
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateSlimBuilder(args);

// Configure JSON serialization for AOT
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});

var app = builder.Build();

app.MapGet("/", () => "Hello from Native AOT!");

app.MapGet("/api/users/{id}", (int id) => 
    Results.Ok(new User { Id = id, Name = $"User {id}" }));

app.MapPost("/api/users", (User user) => 
    Results.Created($"/api/users/{user.Id}", user));

app.Run();

public record User(int Id, string Name);

[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(User[]))]
internal partial class AppJsonContext : JsonSerializerContext { }
```

### 2. AWS Lambda Function

```csharp
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using System.Text.Json.Serialization;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

public class Function
{
    public APIGatewayProxyResponse FunctionHandler(
        APIGatewayProxyRequest request, 
        ILambdaContext context)
    {
        var response = new ResponseModel
        {
            Message = "Hello from Native AOT Lambda!",
            RequestId = context.RequestId
        };
        
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(response, LambdaJsonContext.Default.ResponseModel),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            }
        };
    }
}

public record ResponseModel(string Message, string RequestId);

[JsonSerializable(typeof(ResponseModel))]
[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
internal partial class LambdaJsonContext : JsonSerializerContext { }
```

### 3. Command-Line Tool

```csharp
using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Fast file processor built with Native AOT");
        
        var inputOption = new Option<FileInfo>(
            "--input",
            "Input file to process");
            
        var outputOption = new Option<FileInfo>(
            "--output", 
            "Output file path");
            
        rootCommand.AddOption(inputOption);
        rootCommand.AddOption(outputOption);
        
        rootCommand.SetHandler(async (FileInfo input, FileInfo output) =>
        {
            await ProcessFile(input, output);
        }, inputOption, outputOption);
        
        return await rootCommand.InvokeAsync(args);
    }
    
    static async Task ProcessFile(FileInfo input, FileInfo output)
    {
        var content = await File.ReadAllTextAsync(input.FullName);
        var processed = content.ToUpper();
        await File.WriteAllTextAsync(output.FullName, processed);
        Console.WriteLine($"Processed {input.Name} -> {output.Name}");
    }
}
```

## Best Practices

### 1. Use Source Generators

```csharp
// Always use source generators for serialization
[JsonSerializable(typeof(WeatherForecast))]
[JsonSerializable(typeof(List<WeatherForecast>))]
internal partial class WeatherJsonContext : JsonSerializerContext { }

// Use with ASP.NET Core
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, WeatherJsonContext.Default);
});
```

### 2. Avoid Reflection

```csharp
// ❌ Avoid
var type = Type.GetType(typeName);

// ✅ Use direct types
var type = typeof(MyClass);

// ✅ Or use attributes to preserve types
[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MyClass))]
```

### 3. Test AOT Compatibility Early

```bash
# Add to CI/CD pipeline
dotnet publish -c Release -r linux-x64 /p:PublishAot=true
```

### 4. Profile and Optimize

```csharp
using System.Diagnostics;

// Measure actual startup improvement
public class Startup
{
    private static readonly long StartTicks = Stopwatch.GetTimestamp();
    
    public static void LogStartupTime()
    {
        var elapsed = Stopwatch.GetElapsedTime(StartTicks);
        Console.WriteLine($"Startup: {elapsed.TotalMilliseconds}ms");
    }
}
```

## Interview Questions

### 1. What is Native AOT and how does it differ from traditional JIT compilation in .NET?
**Answer**: Native AOT (Ahead-of-Time) compiles .NET applications to native machine code during publish time, producing a self-contained executable without needing the .NET runtime. JIT (Just-in-Time) compilation happens at runtime - IL code is compiled to native code when methods are first called. Native AOT offers faster startup (5-20x), smaller memory footprint, and self-contained deployment, but has limitations with reflection and dynamic code generation.

### 2. What are the main limitations of Native AOT and how do you work around them?
**Answer**: Main limitations include: (1) Limited reflection - use source generators and `[DynamicallyAccessedMembers]` attributes; (2) No dynamic code generation (System.Reflection.Emit) - use static alternatives; (3) No runtime assembly loading - use compile-time references; (4) Some libraries incompatible - check compatibility and use AOT-friendly alternatives. Source generators solve most serialization issues.

### 3. How do you make JSON serialization work with Native AOT?
**Answer**: Use System.Text.Json source generators. Create a JsonSerializerContext with [JsonSerializable] attributes for each type, then use the generated context: `JsonSerializer.Serialize(obj, MyContext.Default.MyType)`. For ASP.NET Core, configure with `ConfigureHttpJsonOptions` to add the context to TypeInfoResolverChain.

### 4. What is tree shaking/trimming in the context of Native AOT?
**Answer**: Trimming (tree shaking) removes unused code during AOT compilation to reduce executable size. The compiler analyzes which types, methods, and dependencies are actually used and removes everything else. This can reduce app size from 85MB (self-contained) to 8MB or less. Configure with `<PublishTrimmed>true</PublishTrimmed>` and `<TrimMode>full</TrimMode>`.

### 5. How does Native AOT impact startup time and memory usage?
**Answer**: Native AOT dramatically improves both. Startup is 10-20x faster (e.g., 35ms vs 450ms for Minimal API) because there's no JIT compilation. Memory usage is 40-60% lower because there's no JIT compiler in memory, no IL metadata, and no JIT compilation overhead. This makes it ideal for serverless functions, microservices, and CLI tools.

### 6. What are the IlcOptimizationPreference options and when to use each?
**Answer**: `Speed` optimizes for execution speed (default), producing faster code but larger binaries. `Size` optimizes for smaller executable size, useful when distribution size matters more than raw performance. Configure with `<IlcOptimizationPreference>Speed|Size</IlcOptimizationPreference>`. For Lambda functions or containers, Size is often better. For long-running services, Speed may be preferred.

### 7. How do you handle Entity Framework Core with Native AOT?
**Answer**: Use compiled queries with `EF.CompileQuery()` to avoid runtime expression compilation. Avoid dynamic LINQ on IQueryable. Use source generators for model configuration when possible. As of .NET 9, EF Core has improved AOT support, but some features like lazy loading and certain query patterns may still require testing. Always verify with actual AOT publishing.

### 8. What is the difference between PublishAot and PublishTrimmed?
**Answer**: `PublishAot` enables full Native AOT compilation, producing a native executable without .NET runtime dependency. `PublishTrimmed` removes unused code from a standard .NET app but still requires the runtime and uses JIT. PublishAot implies PublishTrimmed, but also includes native code generation. AOT provides better startup and memory, but has more constraints.

### 9. How do you diagnose and fix AOT compatibility warnings?
**Answer**: Run `dotnet publish` with AOT enabled to see warnings. Common issues: (1) ILxxxx warnings indicate trim/AOT problems; (2) Add `[DynamicallyAccessedMembers]` for reflection; (3) Use `[DynamicDependency]` to preserve types; (4) Replace reflection with source generators; (5) Check library compatibility. Use `<TrimmerRootAssembly>` to preserve entire assemblies if needed.

### 10. When should you use Native AOT vs traditional JIT deployment?
**Answer**: Use Native AOT for: (1) Serverless functions (Lambda, Azure Functions) needing fast cold starts; (2) CLI tools requiring quick startup; (3) Microservices with many instances to reduce memory; (4) Edge computing with resource constraints; (5) Desktop apps needing simple deployment. Use JIT for: (1) Apps heavily using reflection/dynamic code; (2) Existing apps with incompatible libraries; (3) Development scenarios needing faster build times; (4) Apps requiring maximum runtime flexibility.

---

**Last Updated: January 2026 - .NET 9**

**Related Topics**: See also [Minimal APIs Advanced](./07-minimal-apis-advanced.md), [Performance Optimization](./09-performance-optimization.md)
