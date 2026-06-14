# gRPC

## What is gRPC?

- **gRPC** = *Google Remote Procedure Call* (open-sourced in 2015).
- Based on **HTTP/2** and **Protocol Buffers (protobuf)**.
- Allows calling functions on a remote server as if they were local methods.

Think of it like remote function calls instead of REST requests — a process calling a procedure on another computer.

Why teams adopt RPC-style communication for server-to-server calls:
- **Scale** → easy to add instances.
- **Security** → TLS/mTLS built in.
- **Upgrades** → contracts make upgrades easier to manage.
- **Any language** → cross-language code generation.

gRPC specifically builds on:
- **Protocol Buffers** for the wire format.
- **HTTP/2** for transport (binary frames, multiplexing, streaming).

## How gRPC Works

1. Define service & messages in a `.proto` file.
2. Compile the `.proto` to auto-generate client & server code.
3. Client calls methods like local functions (`GetUser()`).

Example `.proto`:

```protobuf
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

## Protocol Buffers

Protocol Buffers (protobuf) is the interface definition language and binary serialization format used by gRPC.

- Roughly **4x faster than JSON** on the wire.
- Not all fields are serialized — only those that are set, keeping payloads compact.
- Compiled with the protobuf compiler (`protoc`); check the version with:

```text
protoc --version
```

- Tools like `grpcurl` let you call gRPC services using protocol buffers from the command line.
- HTTP/2's binary frames make streaming efficient and upgrades easier to roll out.

## When to Use gRPC

Ideal when:
- **Microservices** need to talk internally.
- **Real-time streaming** is needed (chat, IoT, video).
- **Low latency & high performance** is critical.
- **Strong typing** with auto-generated client/server code is desired.

Not ideal when:
- Building a public API for developers (REST/GraphQL are easier).
- You need simple debugging (binary data is harder to inspect).

## Implementing gRPC in .NET

### 1. Create Project

```bash
dotnet new grpc -o GrpcDemo
cd GrpcDemo
```

### 2. Define Proto File

`Protos/user.proto`:

```protobuf
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

### 3. Implement Service (Server)

`Services/UserService.cs`:

```csharp
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

### 4. Register Service

`Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<GrpcDemo.Services.UserServiceImpl>();
app.MapGet("/", () => "Use a gRPC client to communicate with this server.");

app.Run();
```

### 5. Create Client (Console Example)

```bash
dotnet new console -o GrpcClient
cd GrpcClient
dotnet add package Grpc.Net.Client
dotnet add package Google.Protobuf
dotnet add package Grpc.Tools
```

`Program.cs`:

```csharp
using Grpc.Net.Client;
using GrpcDemo;

using var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new UserService.UserServiceClient(channel);

var response = await client.GetUserAsync(new UserRequest { Id = 1 });
Console.WriteLine($"User: {response.Name}, Email: {response.Email}");
```

## Full Example: gRPC with EF Core in .NET

This shows how to implement a gRPC service in .NET with Protobuf contracts on top of Entity Framework Core, and how another service (client) communicates with it.

### 1. Define Protobuf Contract

`Protos/complaint.proto`:

```protobuf
syntax = "proto3";

option csharp_namespace = "DealerCare.Grpc";

service ComplaintService {
  rpc CreateComplaint (CreateComplaintRequest) returns (ComplaintResponse);
  rpc GetComplaint (ComplaintRequest) returns (ComplaintResponse);
}

message CreateComplaintRequest {
  string customerId = 1;
  string complaintText = 2;
}

message ComplaintRequest {
  string complaintId = 1;
}

message ComplaintResponse {
  string complaintId = 1;
  string customerId = 2;
  string complaintText = 3;
  string resolutionText = 4;
}
```

### 2. Add gRPC and EF Core Packages

In your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Grpc.AspNetCore" Version="2.56.0" />
  <PackageReference Include="Google.Protobuf" Version="3.24.3" />
  <PackageReference Include="Grpc.Tools" Version="2.56.0" PrivateAssets="All" />
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="7.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.0.0" />
</ItemGroup>

<ItemGroup>
  <Protobuf Include="Protos\complaint.proto" GrpcServices="Server" />
</ItemGroup>
```

### 3. EF Core Model and DbContext

```csharp
public class Complaint
{
    public Guid ComplaintId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string ComplaintText { get; set; } = string.Empty;
    public string ResolutionText { get; set; } = string.Empty;
}

public class DealerCareDbContext : DbContext
{
    public DealerCareDbContext(DbContextOptions<DealerCareDbContext> options)
        : base(options) { }

    public DbSet<Complaint> Complaints { get; set; }
}
```

### 4. gRPC Service Implementation

```csharp
public class ComplaintServiceImpl : ComplaintService.ComplaintServiceBase
{
    private readonly DealerCareDbContext _db;

    public ComplaintServiceImpl(DealerCareDbContext db)
    {
        _db = db;
    }

    public override async Task<ComplaintResponse> CreateComplaint(CreateComplaintRequest request, ServerCallContext context)
    {
        var complaint = new Complaint
        {
            ComplaintId = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            ComplaintText = request.ComplaintText,
            ResolutionText = "Pending"
        };

        _db.Complaints.Add(complaint);
        await _db.SaveChangesAsync();

        return new ComplaintResponse
        {
            ComplaintId = complaint.ComplaintId.ToString(),
            CustomerId = complaint.CustomerId,
            ComplaintText = complaint.ComplaintText,
            ResolutionText = complaint.ResolutionText
        };
    }

    public override async Task<ComplaintResponse> GetComplaint(ComplaintRequest request, ServerCallContext context)
    {
        var complaint = await _db.Complaints.FindAsync(Guid.Parse(request.ComplaintId));

        if (complaint == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Complaint not found"));

        return new ComplaintResponse
        {
            ComplaintId = complaint.ComplaintId.ToString(),
            CustomerId = complaint.CustomerId,
            ComplaintText = complaint.ComplaintText,
            ResolutionText = complaint.ResolutionText
        };
    }
}
```

### 5. Configure gRPC Server in ASP.NET Core

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddDbContext<DealerCareDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DealerCareDb")));

var app = builder.Build();

app.MapGrpcService<ComplaintServiceImpl>();
app.MapGet("/", () => "gRPC Complaint Service running...");

app.Run();
```

### 6. Client Service Consuming gRPC

```csharp
using Grpc.Net.Client;
using DealerCare.Grpc;

var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new ComplaintService.ComplaintServiceClient(channel);

// Create complaint
var createResponse = await client.CreateComplaintAsync(new CreateComplaintRequest
{
    CustomerId = "CUST001",
    ComplaintText = "AC not cooling after service."
});
Console.WriteLine($"Created Complaint ID: {createResponse.ComplaintId}");

// Retrieve complaint
var getResponse = await client.GetComplaintAsync(new ComplaintRequest
{
    ComplaintId = createResponse.ComplaintId
});
Console.WriteLine($"Complaint: {getResponse.ComplaintText}");
Console.WriteLine($"Resolution: {getResponse.ResolutionText}");
```

### Communication Flow

1. **Client App** sends a `CreateComplaint` request via gRPC.
2. **gRPC Server (ComplaintService)** stores the complaint in SQL DB via EF Core.
3. **Client App** sends a `GetComplaint` request.
4. **Server** fetches from the EF Core DB and returns a Protobuf response.

## Authentication & Authorization in gRPC

gRPC is built on HTTP/2 and uses metadata headers for passing credentials. Authentication verifies who you are; authorization decides what you can do.

### Authentication in gRPC

Authentication = verifying caller identity.

**1. JWT / OAuth2 tokens**

```csharp
// Client
var headers = new Metadata();
headers.Add("Authorization", "Bearer " + jwtToken);
var response = await client.GetUserAsync(new UserRequest { Id = 1 }, headers);
```

```csharp
// Server
var authHeader = context.RequestHeaders.GetValue("Authorization");
if (authHeader == null || !ValidateToken(authHeader))
    throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token"));
```

**2. API Keys**

```csharp
headers.Add("x-api-key", "my-secret-key");
```

**3. Transport Security (TLS/mTLS)**

- **TLS** → secures channel + server authentication.
- **mTLS** → both client & server authenticate with certificates.
- Used in microservices (banking, fintech, enterprises).

### Authorization in gRPC

Authorization = deciding what the authenticated caller can do. Applied after authentication, enforced at service/method level or via role-based policies.

```csharp
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

Custom interceptor for role checks:

```csharp
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

### Flow of Authentication + Authorization

```text
Client -> Server: RPC Call + Metadata (Authorization: Bearer <JWT>)
Server -> Auth Service: Validate Token
Auth Service -> Server: Identity + Roles
Server -> Authorization Policy: Check roles/permissions
Authorization Policy -> Server: Allow/Deny
Server -> Client: Response or PermissionDenied
```

### Best Practices

- Always use TLS (or mTLS for service-to-service).
- Use JWT/OAuth2 for user-to-service APIs.
- Apply RBAC/ABAC authorization at service/method level.
- Use interceptors for reusable authentication logic.
- Return correct gRPC status codes: `Unauthenticated` (invalid credentials), `PermissionDenied` (valid user, but not allowed).

## Advantages and Disadvantages

**Advantages:**
- Fast & efficient (binary Protocol Buffers).
- HTTP/2 multiplexing, streaming, low overhead.
- Strong typing (generated code from `.proto`).
- Streaming support (client, server, bidirectional).
- Excellent for microservices.

**Disadvantages:**
- Harder debugging (binary, not human-readable).
- Limited browser support (needs gRPC-Web).
- Steeper learning curve than REST.
- Overkill for simple CRUD APIs.

## Real-World Examples

- **Google** → internal microservices.
- **Netflix** → microservice inter-communication.
- **Square, Dropbox** → high-performance APIs.
- **Trading platforms** → low-latency market data & order execution.

## Summary

- gRPC is a high-performance, strongly typed RPC framework, best for microservices and real-time streaming.
- In .NET, use **Grpc.AspNetCore** with `.proto` files, optionally backed by EF Core for persistence.
- Advantages: fast, efficient, typed, streaming. Disadvantages: complex, binary, less browser-friendly.
