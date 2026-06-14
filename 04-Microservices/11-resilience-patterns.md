# Resilience Patterns

Distributed systems fail in partial, transient ways — a slow downstream service, a dropped network packet, a momentary timeout. Two foundational patterns guard against these: the **Circuit Breaker** (stop calling a failing service) and **Retry + Backoff** (try again sensibly on transient faults). In .NET, both are typically implemented with **Polly** (and its built-in resilience integration in .NET 8), often together.

## Circuit Breaker Pattern

### What is the Circuit Breaker Pattern?

- A **Circuit Breaker** is like an electrical fuse for service calls.
- It prevents your system from constantly retrying a failing service, which can cause cascading failures.
- Instead of hammering a broken service, the circuit "opens" after repeated failures and gives it time to recover.

### States of a Circuit Breaker

1. **Closed** → Requests flow normally. Failures are counted.
2. **Open** → Too many failures, circuit opens, requests fail immediately (fast fail).
3. **Half-Open** → After a cooldown period, allow a few test requests.
   - If they succeed → go back to Closed.
   - If they fail → stay Open.

This protects the system and allows graceful degradation.

```text
[*] --> Closed
Closed --> Open: Failures exceed threshold
Open --> HalfOpen: After timeout
HalfOpen --> Closed: Test request success
HalfOpen --> Open: Test request fails
```

### Benefits

- Prevents cascading failures.
- Improves fault tolerance.
- Provides fast failure (no waiting for timeouts).
- Supports graceful recovery.

### Implementation in .NET (Polly)

Install Polly:

```bash
dotnet add package Polly
```

Circuit Breaker with HttpClient:

```csharp
using Polly;
using Polly.CircuitBreaker;

var breakerPolicy = Policy
    .Handle<HttpRequestException>() // what exceptions to handle
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 3,    // threshold
        durationOfBreak: TimeSpan.FromSeconds(30),  // open state time
        onBreak: (ex, breakDelay) =>
        {
            Console.WriteLine($"Circuit opened for {breakDelay.TotalSeconds} seconds due to: {ex.Message}");
        },
        onReset: () => Console.WriteLine("Circuit closed. Service recovered."),
        onHalfOpen: () => Console.WriteLine("Circuit in half-open state, testing...")
    );

var httpClient = new HttpClient();

// Usage: Wrap the HTTP call
try
{
    await breakerPolicy.ExecuteAsync(async () =>
    {
        var response = await httpClient.GetAsync("https://example.com/api");
        response.EnsureSuccessStatusCode();
        Console.WriteLine("Request succeeded!");
    });
}
catch (BrokenCircuitException)
{
    Console.WriteLine("Circuit is open! Fast failing...");
}
```

### Circuit Breaker in ASP.NET Core with HttpClientFactory

Configure it at startup with typed clients:

```csharp
builder.Services.AddHttpClient("ExternalApiClient")
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));
```

Then inject and use:

```csharp
public class MyService
{
    private readonly HttpClient _client;
    public MyService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient("ExternalApiClient");
    }

    public async Task<string> GetData()
    {
        var response = await _client.GetStringAsync("/data");
        return response;
    }
}
```

### Alternatives

- **YARP (Yet Another Reverse Proxy)** + Polly policies.
- **Ocelot API Gateway** (supports circuit breaker and retry).
- **Resilience Middleware** (built-in Polly integration in .NET 8).

## Retry + Backoff Pattern

### What is Retry + Backoff?

- **Retry** = When a request fails due to a transient fault (e.g., network glitch, temporary unavailability), the client retries the operation.
- **Backoff** = Instead of retrying immediately in a tight loop, wait before retrying. This avoids overwhelming a service that is already struggling.

Typically combined as Retry with Exponential Backoff.

### Types of Backoff Strategies

1. **Fixed Delay** — Always wait the same time between retries (e.g., 2s, 2s, 2s).
2. **Linear Backoff** — Waits increase linearly (e.g., 1s, 2s, 3s).
3. **Exponential Backoff** (most common) — Wait time doubles with each retry (e.g., 1s, 2s, 4s, 8s).
4. **Exponential Backoff with Jitter** — Add randomness (jitter) to avoid the thundering herd problem (many clients retrying at the same intervals), e.g., 1-2s, 2-4s, 4-8s.

### Benefits

- Improves resilience against transient failures.
- Reduces load on struggling services.
- Helps smooth recovery after outages.
- With jitter, avoids synchronized spikes.

### Implementation in .NET (Polly)

Install Polly:

```bash
dotnet add package Polly
```

Retry with Fixed Delay:

```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)); // retry 3 times, wait 2s each
```

Retry with Exponential Backoff:

```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(
        retryCount: 5,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2^n seconds
        onRetry: (exception, delay) =>
        {
            Console.WriteLine($"Retrying after {delay.TotalSeconds}s due to: {exception.Message}");
        });
```

Retry with Exponential Backoff + Jitter:

```csharp
var jitterer = new Random();

var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(
        retryCount: 5,
        sleepDurationProvider: attempt =>
            TimeSpan.FromSeconds(Math.Pow(2, attempt)) +
            TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)), // add jitter
        onRetry: (exception, delay) =>
        {
            Console.WriteLine($"Delaying {delay.TotalSeconds}s before next retry due to: {exception.Message}");
        });
```

### Flow

```text
Client -> Service: Request
Service -> Client: Failure
Client -> Service: Retry (after 1s)
Service -> Client: Failure
Client -> Service: Retry (after 2s)
Service -> Client: Failure
Client -> Service: Retry (after 4s + jitter)
Service -> Client: Success
```

## Combining Retry + Circuit Breaker

The two patterns are frequently combined so transient faults are retried while a persistently failing service still trips the breaker:

```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

var circuitBreakerPolicy = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));

var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
```

## Summary

- **Circuit Breaker** = protect services from repeated failures; states cycle Closed → Open → Half-Open.
- **Retry** = try again on transient errors; **Backoff** = wait progressively longer before retrying, with exponential backoff + jitter as the best practice for distributed systems.
- In .NET, use **Polly** (`WaitAndRetryAsync`, `CircuitBreakerAsync`) with HttpClient or ASP.NET Core's `HttpClientFactory`, and combine retry, circuit breaker, and timeout for robust microservices resilience.
