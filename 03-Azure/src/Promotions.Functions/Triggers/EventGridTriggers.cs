using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Abstractions;
using Promotions.Domain.Events;

namespace Promotions.Functions.Triggers;

/// <summary>
/// Event Grid topic (07): reactive subscriber. When a Promotion.Activated event
/// is delivered, send a push notification (Notification Hubs, topic 11) to
/// devices tagged for that region. Event Grid gives push delivery + retries +
/// dead-lettering, and many independent subscribers can filter the same topic.
/// </summary>
public sealed class EventGridTriggers
{
    private readonly IPromotionRepository _repository;
    private readonly IPushNotificationService _push;
    private readonly ILogger<EventGridTriggers> _logger;

    public EventGridTriggers(IPromotionRepository repository, IPushNotificationService push, ILogger<EventGridTriggers> logger)
    {
        _repository = repository;
        _push = push;
        _logger = logger;
    }

    [Function("OnPromotionEvent")]
    public async Task OnPromotionEvent(
        [EventGridTrigger] Azure.Messaging.EventGrid.EventGridEvent gridEvent)
    {
        _logger.LogInformation("Event Grid delivered {EventType} subject {Subject}", gridEvent.EventType, gridEvent.Subject);

        if (gridEvent.EventType != "Promotion.Activated")
            return;

        var activated = JsonSerializer.Deserialize<PromotionActivatedEvent>(gridEvent.Data.ToString());
        if (activated is null) return;

        var promotion = await _repository.GetAsync(activated.PromotionId, activated.Region);
        if (promotion is null)
        {
            _logger.LogWarning("Promotion {Id} not found for push", activated.PromotionId);
            return;
        }

        await _push.NotifyPromotionActivatedAsync(promotion);
    }
}
