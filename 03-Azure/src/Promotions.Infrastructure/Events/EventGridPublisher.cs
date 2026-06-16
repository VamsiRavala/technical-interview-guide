using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Abstractions;
using Promotions.Domain.Events;

namespace Promotions.Infrastructure.Events;

/// <summary>
/// Publishes promotion events to an Azure Event Grid custom topic (topic 07).
/// Event Grid is the reactive fan-out backbone: many independent subscribers
/// (Functions, Logic Apps, webhooks) filter on <c>eventType</c> + <c>subject</c>
/// without the publisher knowing about them.
/// </summary>
public sealed class EventGridPublisher : IEventGridPublisher
{
    private readonly EventGridPublisherClient _client;
    private readonly ILogger<EventGridPublisher> _logger;

    public EventGridPublisher(EventGridPublisherClient client, ILogger<EventGridPublisher> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task PublishAsync(PromotionEvent @event, CancellationToken ct = default)
    {
        var gridEvent = new EventGridEvent(
            subject: $"promotions/{@event.Region}/{@event.PromotionId}",
            eventType: @event.EventType,
            dataVersion: "1.0",
            data: BinaryData.FromObjectAsJson(@event));

        await _client.SendEventAsync(gridEvent, ct);
        _logger.LogInformation("Published {EventType} to Event Grid for {PromotionId}", @event.EventType, @event.PromotionId);
    }
}
