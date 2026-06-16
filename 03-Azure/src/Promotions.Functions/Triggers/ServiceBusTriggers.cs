using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Events;

namespace Promotions.Functions.Triggers;

/// <summary>
/// Service Bus topic (05): consume promotion workflow events. Sessions are
/// enabled so events for one promotion are processed in order (FIFO). On an
/// unhandled exception the host abandons the message; after max delivery
/// attempts it is dead-lettered automatically.
/// </summary>
public sealed class ServiceBusTriggers
{
    private readonly ILogger<ServiceBusTriggers> _logger;

    public ServiceBusTriggers(ILogger<ServiceBusTriggers> logger) => _logger = logger;

    [Function("ProcessPromotionEvent")]
    public void ProcessPromotionEvent(
        [ServiceBusTrigger("%Azure:ServiceBus:QueueOrTopic%", Connection = "ServiceBusConnection", IsSessionsEnabled = true)]
        string body,
        FunctionContext context)
    {
        // The "eventType" application property tells us which concrete event this is.
        var eventType = context.BindingContext.BindingData.TryGetValue("Subject", out var subject)
            ? subject?.ToString()
            : "unknown";

        _logger.LogInformation("Service Bus received {EventType}: {Body}", eventType, body);

        switch (eventType)
        {
            case "Promotion.Created":
                var created = JsonSerializer.Deserialize<PromotionCreatedEvent>(body);
                _logger.LogInformation("Pricing/Store services would react to created promotion {Id}", created?.PromotionId);
                break;
            case "Promotion.Activated":
                var activated = JsonSerializer.Deserialize<PromotionActivatedEvent>(body);
                _logger.LogInformation("Distributing activated promotion {Id} to {Count} stores",
                    activated?.PromotionId, activated?.StoreIds.Count);
                break;
            default:
                _logger.LogWarning("Unhandled Service Bus event type {EventType}", eventType);
                break;
        }
    }
}
