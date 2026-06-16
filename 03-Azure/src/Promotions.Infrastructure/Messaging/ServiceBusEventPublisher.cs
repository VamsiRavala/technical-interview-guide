using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Abstractions;
using Promotions.Domain.Events;

namespace Promotions.Infrastructure.Messaging;

/// <summary>
/// Publishes promotion domain events to Azure Service Bus (topic 05).
/// Demonstrates: ContentType + Subject for subscription SQL filters,
/// SessionId for ordered per-promotion processing (FIFO), and a stable
/// MessageId for built-in duplicate detection.
/// </summary>
public sealed class ServiceBusEventPublisher : IEventPublisher
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusEventPublisher> _logger;

    public ServiceBusEventPublisher(ServiceBusClient client, ServiceBusOptions options, ILogger<ServiceBusEventPublisher> logger)
    {
        _sender = client.CreateSender(options.QueueOrTopic);
        _logger = logger;
    }

    public async Task PublishAsync(PromotionEvent @event, CancellationToken ct = default)
    {
        var body = JsonSerializer.Serialize(@event, @event.GetType());
        var message = new ServiceBusMessage(body)
        {
            MessageId = @event.EventId,            // duplicate detection key
            Subject = @event.EventType,            // routing/filtering ("Promotion.Approved")
            ContentType = "application/json",
            SessionId = @event.PromotionId,        // ordered processing per promotion
            CorrelationId = @event.PromotionId
        };
        message.ApplicationProperties["region"] = @event.Region;
        message.ApplicationProperties["eventType"] = @event.EventType;

        await _sender.SendMessageAsync(message, ct);
        _logger.LogInformation("Published {EventType} for {PromotionId} to Service Bus", @event.EventType, @event.PromotionId);
    }
}
