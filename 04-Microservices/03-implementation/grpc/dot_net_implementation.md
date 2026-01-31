# 🚗 gRPC with EF Core in .NET -- Full Example

This guide shows how to implement a **gRPC service in .NET** with
**Protobuf contracts** on top of **Entity Framework Core**, and how
another service (client) communicates with it.

------------------------------------------------------------------------

## 1. Define Protobuf Contract

Create a file: `Protos/complaint.proto`

``` proto
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

------------------------------------------------------------------------

## 2. Add gRPC and EF Core Packages

In your `.csproj`:

``` xml
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

------------------------------------------------------------------------

## 3. EF Core Model and DbContext

``` csharp
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

------------------------------------------------------------------------

## 4. gRPC Service Implementation

``` csharp
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

------------------------------------------------------------------------

## 5. Configure gRPC Server in ASP.NET Core

``` csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddDbContext<DealerCareDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DealerCareDb")));

var app = builder.Build();

app.MapGrpcService<ComplaintServiceImpl>();
app.MapGet("/", () => "gRPC Complaint Service running...");

app.Run();
```

------------------------------------------------------------------------

## 6. Client Service Consuming gRPC

``` csharp
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

------------------------------------------------------------------------

## 🔄 Communication Flow

1.  **Client App** → sends `CreateComplaint` request via gRPC.\
2.  **gRPC Server (ComplaintService)** → stores complaint in SQL DB via
    EF Core.\
3.  **Client App** → sends `GetComplaint` request.\
4.  **Server** → fetches from EF Core DB → returns Protobuf response.

------------------------------------------------------------------------

## ✅ Summary

-   **Protobuf** defines contract (`complaint.proto`).\
-   **EF Core** handles persistence.\
-   **ASP.NET Core gRPC** hosts the service.\
-   **Client service** calls via `Grpc.Net.Client`.\
-   This creates a **fast, strongly typed, DB-backed gRPC
    microservice**.
