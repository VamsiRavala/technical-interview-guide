using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Abstractions;
using Promotions.Domain.Events;
using Promotions.Domain.Models;

namespace Promotions.Functions.Triggers;

/// <summary>
/// Azure Functions topic (03): timer trigger (scheduled serverless). Every 15
/// minutes, scan Cosmos for active promotions past their end date, mark them
/// Expired, and publish a Promotion.Expired event. CRON: "0 *&#47;15 * * * *".
/// </summary>
public sealed class TimerTriggers
{
    private readonly IPromotionRepository _repository;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<TimerTriggers> _logger;

    public TimerTriggers(IPromotionRepository repository, IEventPublisher publisher, ILogger<TimerTriggers> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _logger = logger;
    }

    [Function("ExpirePromotions")]
    public async Task ExpirePromotions([TimerTrigger("0 */15 * * * *")] TimerInfo timer)
    {
        var now = DateTimeOffset.UtcNow;
        var expiring = await _repository.ListExpiringBeforeAsync(now);
        _logger.LogInformation("Expiry scan found {Count} promotions to expire", expiring.Count);

        foreach (var promotion in expiring)
        {
            promotion.Status = PromotionStatus.Expired;
            await _repository.UpsertAsync(promotion);
            await _publisher.PublishAsync(new PromotionExpiredEvent
            {
                PromotionId = promotion.Id,
                Region = promotion.Region
            });
        }
    }
}
