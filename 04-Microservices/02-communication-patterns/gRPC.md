# ⚡ gRPC in .NET

## 🔹 What is gRPC?

-   **gRPC** = *Google Remote Procedure Call* (open-sourced in 2015).\
-   Based on **HTTP/2** and **Protocol Buffers (protobuf)**.\
-   Allows calling functions on a remote server as if they were **local
    methods**.

👉 Think of it like **remote function calls** instead of REST requests.

------------------------------------------------------------------------

## 🔹 When to Use gRPC

✅ Ideal when:\
- **Microservices** need to talk internally.\
- **Real-time streaming** (chat, IoT, video).\
- **Low latency & high performance** is critical.\
- **Strong typing** (auto-generated client/server code).

⚠️ Not ideal when:\
- Building a **public API for developers** (REST/GraphQL easier).\
- Need **simple debugging** (binary data is harder to inspect).

------------------------------------------------------------------------

## 🔹 How gRPC Works

1.  Define service & messages in `.proto` file.\
2.  Compile `.proto` → auto-generate **client & server code**.\
3.  Client calls methods like local functions (`GetUser()`).

**Example `.proto`:**

``` proto
syntax = "proto3";

service UserService {
  rpc GetUser (UserRequest) returns (UserResponse);
}

message UserRequest {
  int32 id = 1;
}

message UserResponse {
  int32 id = 1;
  string name = 2;
  string email = 3;
}
```

------------------------------------------------------------------------

## 🔹 Implementing gRPC in .NET

### 1. Create Project

``` bash
dotnet new grpc -o GrpcDemo
cd GrpcDemo
```

------------------------------------------------------------------------

### 2. Define Proto File

**Protos/user.proto**

``` proto
syntax = "proto3";

option csharp_namespace = "GrpcDemo";

service UserService {
  rpc GetUser (UserRequest) returns (UserResponse);
}

message UserRequest {
  int32 id = 1;
}

message UserResponse {
  int32 id = 1;
  string name = 2;
  string email = 3;
}
```

------------------------------------------------------------------------

### 3. Implement Service (Server)

**Services/UserService.cs**

``` csharp
using Grpc.Core;

namespace GrpcDemo.Services
{
    public class UserServiceImpl : UserService.UserServiceBase
    {
        public override Task<UserResponse> GetUser(UserRequest request, ServerCallContext context)
        {
            return Task.FromResult(new UserResponse
            {
                Id = request.Id,
                Name = "Alice",
                Email = "alice@example.com"
            });
        }
    }
}
```

------------------------------------------------------------------------

### 4. Register Service

**Program.cs**

``` csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<GrpcDemo.Services.UserServiceImpl>();
app.MapGet("/", () => "Use a gRPC client to communicate with this server.");

app.Run();
```

------------------------------------------------------------------------

### 5. Create Client (Console Example)

``` bash
dotnet new console -o GrpcClient
cd GrpcClient
dotnet add package Grpc.Net.Client
dotnet add package Google.Protobuf
dotnet add package Grpc.Tools
```

**Program.cs**

``` csharp
using Grpc.Net.Client;
using GrpcDemo;

using var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new UserService.UserServiceClient(channel);

var response = await client.GetUserAsync(new UserRequest { Id = 1 });
Console.WriteLine($"User: {response.Name}, Email: {response.Email}");
```

------------------------------------------------------------------------

## 🔹 Advantages of gRPC

✅ **Fast & efficient** (binary Protocol Buffers).\
✅ **HTTP/2** → multiplexing, streaming, low overhead.\
✅ **Strong typing** (generated code from `.proto`).\
✅ **Streaming support** (client, server, bidirectional).\
✅ Excellent for **microservices**.

------------------------------------------------------------------------

## 🔹 Disadvantages of gRPC

⚠️ **Harder debugging** (binary, not human-readable).\
⚠️ **Limited browser support** (needs gRPC-Web).\
⚠️ **Steeper learning curve** than REST.\
⚠️ Overkill for simple CRUD APIs.

------------------------------------------------------------------------

## 🔹 Real-World Examples of gRPC Usage

-   **Google** → internal microservices.\
-   **Netflix** → microservice inter-communication.\
-   **Square, Dropbox** → high-performance APIs.\
-   **Trading platforms** → low-latency market data & order execution.

------------------------------------------------------------------------

## ✅ Summary

-   **gRPC** = high-performance, strongly typed RPC framework.\
-   Best for **microservices** & **real-time streaming**.\
-   In .NET → use **Grpc.AspNetCore** with `.proto` files.\
-   **Advantages**: Fast, efficient, typed, streaming.\
-   **Disadvantages**: Complex, binary, less browser-friendly.

# 🔐 Authentication & Authorization in gRPC

gRPC is built on **HTTP/2** and uses **metadata headers** for passing
credentials.\
Authentication verifies **who you are**, and Authorization decides
**what you can do**.

------------------------------------------------------------------------

## 🔹 Authentication in gRPC

**Authentication = verifying caller identity.**

### Common Methods

1.  **JWT / OAuth2 tokens**

``` csharp
// Client
var headers = new Metadata();
headers.Add("Authorization", "Bearer " + jwtToken);
var response = await client.GetUserAsync(new UserRequest { Id = 1 }, headers);
```

``` csharp
// Server
var authHeader = context.RequestHeaders.GetValue("Authorization");
if (authHeader == null || !ValidateToken(authHeader))
    throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token"));
```

2.  **API Keys**

``` csharp
headers.Add("x-api-key", "my-secret-key");
```

3.  **Transport Security (TLS/mTLS)**

-   **TLS** → secures channel + server authentication.\
-   **mTLS** → both client & server authenticate with certificates.\
-   Used in microservices (banking, fintech, enterprises).

------------------------------------------------------------------------

## 🔹 Authorization in gRPC

**Authorization = deciding what the authenticated caller can do.**

-   Applied **after authentication**.\
-   Can be enforced at service/method level or via role-based policies.

### Example (ASP.NET Core gRPC)

``` csharp
public class UserServiceImpl : UserService.UserServiceBase
{
    [Authorize(Roles = "Admin")]
    public override Task<UserResponse> GetUser(UserRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UserResponse
        {
            Id = request.Id,
            Name = "Alice",
            Email = "alice@example.com"
        });
    }
}
```

### Custom Interceptor for Role Checks

``` csharp
public class AuthInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, 
        ServerCallContext context, 
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var role = context.RequestHeaders.GetValue("Role");
        if (role != "Admin")
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Forbidden"));

        return await continuation(request, context);
    }
}
```

------------------------------------------------------------------------

## 🔹 Flow of Authentication + Authorization

``` mermaid
sequenceDiagram
    Client->>Server: RPC Call + Metadata (Authorization: Bearer <JWT>)
    Server->>Auth Service: Validate Token
    Auth Service-->>Server: Identity + Roles
    Server->>Authorization Policy: Check roles/permissions
    Authorization Policy-->>Server: Allow/Deny
    Server-->>Client: Response or PermissionDenied
```

------------------------------------------------------------------------

## 🔹 Best Practices

✅ Always use **TLS** (or mTLS for service-to-service).\
✅ Use **JWT/OAuth2** for user-to-service APIs.\
✅ Apply **RBAC/ABAC** authorization at service/method level.\
✅ Use **interceptors** for reusable authentication logic.\
✅ Return correct gRPC status codes:\
- `Unauthenticated` (invalid credentials).\
- `PermissionDenied` (valid user, but not allowed).

------------------------------------------------------------------------

## ✅ Summary

-   **Authentication in gRPC** → Verifies *who* the client is (JWT, API
    key, TLS, mTLS).\
-   **Authorization in gRPC** → Defines *what* the client can do (roles,
    policies).\
-   Implement with **metadata headers, interceptors, attributes**.\
-   Real-world: OAuth2 (Google APIs), mTLS (finance, enterprise
    microservices).

