using Promotions.Domain.Abstractions;
using Promotions.Domain.Models;
using Promotions.Domain.Services;
using Promotions.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Observability — Application Insights (Monitoring & Diagnostics topic).
builder.Services.AddApplicationInsightsTelemetry();

// Register all Azure clients + abstractions (Cosmos, Service Bus, Event Grid,
// Event Hubs, Blob, Queue, Notification Hubs).
builder.Services.AddPromotionsInfrastructure(builder.Configuration);
builder.Services.AddScoped<PromotionService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// --- Liveness/readiness (used by AKS probes) ---
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

var promos = app.MapGroup("/api/promotions").WithTags("Promotions");

// Create + submit a promotion for approval.
promos.MapPost("/", async (Promotion promotion, PromotionService service, CancellationToken ct) =>
{
    var created = await service.CreateAsync(promotion, ct);
    return Results.Created($"/api/promotions/{created.Id}?region={created.Region}", created);
})
.WithName("CreatePromotion");

// Point read by id + region (partition key).
promos.MapGet("/{id}", async (string id, string region, IPromotionRepository repo, CancellationToken ct) =>
{
    var promotion = await repo.GetAsync(id, region, ct);
    return promotion is null ? Results.NotFound() : Results.Ok(promotion);
})
.WithName("GetPromotion");

// List a region's promotions (single-partition query).
promos.MapGet("/", async (string region, IPromotionRepository repo, CancellationToken ct) =>
    Results.Ok(await repo.ListByRegionAsync(region, ct)))
.WithName("ListPromotions");

// Activate an approved promotion (publishes events + queues catalog rebuild).
promos.MapPost("/{id}/activate", async (string id, string region, PromotionService service, CancellationToken ct) =>
{
    var activated = await service.ActivateAsync(id, region, ct);
    return Results.Ok(activated);
})
.WithName("ActivatePromotion");

// --- Device registration for push notifications (Notification Hubs) ---
app.MapPost("/api/devices", async (DeviceRegistration registration, IPushNotificationService push, CancellationToken ct) =>
{
    await push.RegisterDeviceAsync(registration, ct);
    return Results.Accepted();
})
.WithTags("Devices");

// --- POS redemption ingestion (Event Hubs telemetry stream) ---
app.MapPost("/api/redemptions", async (RedemptionEvent redemption, IRedemptionStream stream, CancellationToken ct) =>
{
    await stream.SendAsync(redemption, ct);
    return Results.Accepted();
})
.WithTags("Redemptions");

app.Run();
