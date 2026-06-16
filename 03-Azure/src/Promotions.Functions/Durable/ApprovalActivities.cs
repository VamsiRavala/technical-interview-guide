using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Abstractions;
using Promotions.Domain.Events;
using Promotions.Domain.Models;

namespace Promotions.Functions.Durable;

public sealed record StatusUpdate(ApprovalInput Input, PromotionStatus Status, string? Note);
public sealed record StoreTarget(string PromotionId, string StoreId);

/// <summary>
/// Activity functions — the deterministic units of work the orchestrator calls.
/// Unlike the orchestrator (which must be deterministic), activities may do I/O:
/// here they read/write Cosmos and publish events. Constructor-injected
/// dependencies come from the same DI container as every other function.
/// </summary>
public sealed class ApprovalActivities
{
    private readonly IPromotionRepository _repository;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<ApprovalActivities> _logger;

    public ApprovalActivities(IPromotionRepository repository, IEventPublisher publisher, ILogger<ApprovalActivities> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _logger = logger;
    }

    [Function(nameof(ValidatePromotion))]
    public async Task<bool> ValidatePromotion([ActivityTrigger] ApprovalInput input)
    {
        var promotion = await _repository.GetAsync(input.PromotionId, input.Region);
        if (promotion is null) return false;
        return promotion.EndDate > promotion.StartDate
            && promotion.StoreIds.Count > 0
            && promotion.DiscountValue > 0;
    }

    [Function(nameof(GetStoreIds))]
    public async Task<string[]> GetStoreIds([ActivityTrigger] ApprovalInput input)
    {
        var promotion = await _repository.GetAsync(input.PromotionId, input.Region);
        return promotion?.StoreIds.ToArray() ?? Array.Empty<string>();
    }

    [Function(nameof(DistributeToStore))]
    public Task<StoreDistributionResult> DistributeToStore([ActivityTrigger] StoreTarget target)
    {
        // Simulated POS push. In reality: call the store's distribution endpoint.
        _logger.LogInformation("Distributing promotion {Id} to store {Store}", target.PromotionId, target.StoreId);
        return Task.FromResult(new StoreDistributionResult(target.StoreId, true));
    }

    [Function(nameof(SetStatus))]
    public async Task SetStatus([ActivityTrigger] StatusUpdate update)
    {
        var promotion = await _repository.GetAsync(update.Input.PromotionId, update.Input.Region);
        if (promotion is null) return;

        promotion.Status = update.Status;
        if (update.Status == PromotionStatus.Approved || update.Status == PromotionStatus.Active)
            promotion.ApprovedBy = update.Note;
        await _repository.UpsertAsync(promotion);

        PromotionEvent? evt = update.Status switch
        {
            PromotionStatus.Approved => new PromotionApprovedEvent
            {
                PromotionId = promotion.Id, Region = promotion.Region, ApprovedBy = update.Note ?? "unknown"
            },
            PromotionStatus.Active => new PromotionActivatedEvent
            {
                PromotionId = promotion.Id, Region = promotion.Region, StoreIds = promotion.StoreIds
            },
            PromotionStatus.Rejected => new PromotionRejectedEvent
            {
                PromotionId = promotion.Id, Region = promotion.Region, Reason = update.Note ?? "rejected"
            },
            _ => null
        };
        if (evt is not null) await _publisher.PublishAsync(evt);
    }
}
