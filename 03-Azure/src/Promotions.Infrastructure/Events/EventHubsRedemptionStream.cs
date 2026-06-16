using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Abstractions;
using Promotions.Domain.Models;

namespace Promotions.Infrastructure.Events;

/// <summary>
/// Streams high-volume POS redemption telemetry into Azure Event Hubs (topic 06).
/// Uses the PromotionId as the partition key so all redemptions for one
/// promotion land on the same partition (ordered, parallelizable consumers).
/// </summary>
public sealed class EventHubsRedemptionStream : IRedemptionStream
{
    private readonly EventHubProducerClient _producer;
    private readonly ILogger<EventHubsRedemptionStream> _logger;

    public EventHubsRedemptionStream(EventHubProducerClient producer, ILogger<EventHubsRedemptionStream> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    public async Task SendAsync(RedemptionEvent redemption, CancellationToken ct = default)
    {
        // Partition key keeps a promotion's events ordered on one partition.
        var batch = await _producer.CreateBatchAsync(
            new CreateBatchOptions { PartitionKey = redemption.PromotionId }, ct);

        var data = new EventData(BinaryData.FromObjectAsJson(redemption));
        if (!batch.TryAdd(data))
            throw new InvalidOperationException("Redemption event too large for an Event Hubs batch.");

        await _producer.SendAsync(batch, ct);
        _logger.LogDebug("Streamed redemption for {PromotionId} to Event Hubs", redemption.PromotionId);
    }
}
