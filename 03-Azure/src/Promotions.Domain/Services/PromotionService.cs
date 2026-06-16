using Promotions.Domain.Abstractions;
using Promotions.Domain.Events;
using Promotions.Domain.Models;

namespace Promotions.Domain.Services;

/// <summary>
/// Application service that coordinates the write side of the domain. Depends
/// only on abstractions, so it is shared unchanged by the API and the
/// Functions host. Persists to Cosmos, then announces via Service Bus
/// (reliable workflow event) and Event Grid (reactive fan-out).
/// </summary>
public sealed class PromotionService
{
    private readonly IPromotionRepository _repository;
    private readonly IEventPublisher _serviceBus;
    private readonly IEventGridPublisher _eventGrid;
    private readonly IStorageQueueClient _queue;

    public PromotionService(
        IPromotionRepository repository,
        IEventPublisher serviceBus,
        IEventGridPublisher eventGrid,
        IStorageQueueClient queue)
    {
        _repository = repository;
        _serviceBus = serviceBus;
        _eventGrid = eventGrid;
        _queue = queue;
    }

    /// <summary>Creates a draft promotion, submits it for approval, and announces it.</summary>
    public async Task<Promotion> CreateAsync(Promotion promotion, CancellationToken ct = default)
    {
        ValidateDates(promotion);
        promotion.Status = PromotionStatus.PendingApproval;
        promotion.CreatedAt = DateTimeOffset.UtcNow;
        var saved = await _repository.UpsertAsync(promotion, ct);

        var created = new PromotionCreatedEvent
        {
            PromotionId = saved.Id,
            Region = saved.Region,
            Name = saved.Name,
            Type = saved.Type,
            DiscountValue = saved.DiscountValue
        };
        // Service Bus carries the workflow command; Event Grid notifies subscribers.
        await _serviceBus.PublishAsync(created, ct);
        await _eventGrid.PublishAsync(created, ct);
        return saved;
    }

    /// <summary>Activates an approved promotion and queues catalog regeneration.</summary>
    public async Task<Promotion> ActivateAsync(string id, string region, CancellationToken ct = default)
    {
        var promotion = await _repository.GetAsync(id, region, ct)
            ?? throw new KeyNotFoundException($"Promotion {id} not found in region {region}.");

        if (promotion.Status != PromotionStatus.Approved)
            throw new InvalidOperationException($"Cannot activate promotion in status {promotion.Status}.");

        promotion.Status = PromotionStatus.Active;
        var saved = await _repository.UpsertAsync(promotion, ct);

        var activated = new PromotionActivatedEvent
        {
            PromotionId = saved.Id,
            Region = saved.Region,
            StoreIds = saved.StoreIds
        };
        await _serviceBus.PublishAsync(activated, ct);
        await _eventGrid.PublishAsync(activated, ct);

        // Cheap background job: rebuild the POS catalog blob for this region.
        await _queue.EnqueueAsync(new CatalogRebuildJob(saved.Region), ct);
        return saved;
    }

    private static void ValidateDates(Promotion promotion)
    {
        if (promotion.EndDate <= promotion.StartDate)
            throw new ArgumentException("Promotion endDate must be after startDate.");
        if (promotion.Type == PromotionType.PercentageDiscount && promotion.DiscountValue is < 0 or > 100)
            throw new ArgumentException("Percentage discount must be between 0 and 100.");
    }
}

/// <summary>Payload for the Storage Queue background job.</summary>
public sealed record CatalogRebuildJob(string Region);
