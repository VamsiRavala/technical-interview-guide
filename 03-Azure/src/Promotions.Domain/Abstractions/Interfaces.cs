using Promotions.Domain.Events;
using Promotions.Domain.Models;

namespace Promotions.Domain.Abstractions;

/// <summary>Document persistence for the promotion aggregate (Cosmos DB).</summary>
public interface IPromotionRepository
{
    Task<Promotion?> GetAsync(string id, string region, CancellationToken ct = default);
    Task<IReadOnlyList<Promotion>> ListByRegionAsync(string region, CancellationToken ct = default);
    Task<IReadOnlyList<Promotion>> ListExpiringBeforeAsync(DateTimeOffset cutoff, CancellationToken ct = default);
    Task<Promotion> UpsertAsync(Promotion promotion, CancellationToken ct = default);
}

/// <summary>
/// Publishes domain events to Azure Service Bus (reliable enterprise messaging,
/// ordered per session, transactional). Used for commands/workflow events.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync(PromotionEvent @event, CancellationToken ct = default);
}

/// <summary>
/// Publishes lightweight reactive notifications to Azure Event Grid for
/// fan-out to many independent subscribers (near-real-time, push, filterable).
/// </summary>
public interface IEventGridPublisher
{
    Task PublishAsync(PromotionEvent @event, CancellationToken ct = default);
}

/// <summary>Simple, cheap background-job queue (Azure Storage Queue).</summary>
public interface IStorageQueueClient
{
    Task EnqueueAsync<T>(T message, CancellationToken ct = default);
}

/// <summary>High-throughput telemetry stream (Azure Event Hubs).</summary>
public interface IRedemptionStream
{
    Task SendAsync(RedemptionEvent redemption, CancellationToken ct = default);
}

/// <summary>Blob storage for exported promotion catalogs / POS payloads.</summary>
public interface IBlobStorageService
{
    Task<Uri> UploadCatalogAsync(string region, string content, CancellationToken ct = default);
}

/// <summary>Cross-platform push notifications (Azure Notification Hubs).</summary>
public interface IPushNotificationService
{
    Task RegisterDeviceAsync(DeviceRegistration registration, CancellationToken ct = default);
    Task NotifyPromotionActivatedAsync(Promotion promotion, CancellationToken ct = default);
}
