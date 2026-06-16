using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Models;

namespace Promotions.Functions.Triggers;

/// <summary>
/// Event Hubs topic (06): high-throughput stream processing. Redemption events
/// arrive in batches (maxEventBatchSize in host.json) for efficient, parallel,
/// per-partition consumption — the right tool for millions of telemetry
/// events/sec, versus Service Bus for discrete commands.
/// </summary>
public sealed class EventHubTriggers
{
    private readonly ILogger<EventHubTriggers> _logger;

    public EventHubTriggers(ILogger<EventHubTriggers> logger) => _logger = logger;

    [Function("AggregateRedemptions")]
    public void AggregateRedemptions(
        [EventHubTrigger("%Azure:EventHubs:HubName%", Connection = "EventHubsConnection", ConsumerGroup = "$Default")]
        string[] events)
    {
        var redemptions = events
            .Select(e => JsonSerializer.Deserialize<RedemptionEvent>(e))
            .Where(r => r is not null)
            .Cast<RedemptionEvent>()
            .ToList();

        // A real analytics consumer would upsert rolling counters into Cosmos /
        // a read model. Here we just demonstrate batch stream aggregation.
        var byPromotion = redemptions
            .GroupBy(r => r.PromotionId)
            .Select(g => new { PromotionId = g.Key, Count = g.Count(), TotalDiscount = g.Sum(r => r.DiscountApplied) });

        foreach (var stat in byPromotion)
        {
            _logger.LogInformation("Promotion {Id}: {Count} redemptions, {Total:C} discount this batch",
                stat.PromotionId, stat.Count, stat.TotalDiscount);
        }
    }
}
