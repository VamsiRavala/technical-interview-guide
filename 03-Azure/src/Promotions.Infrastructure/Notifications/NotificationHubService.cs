using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Abstractions;
using Promotions.Domain.Models;

namespace Promotions.Infrastructure.Notifications;

/// <summary>
/// Cross-platform push notifications via Azure Notification Hubs (topic 11).
/// Registers device installations with tags and sends tag-targeted pushes so a
/// promotion activation only reaches devices for the relevant region/stores.
/// </summary>
public sealed class NotificationHubService : IPushNotificationService
{
    private readonly NotificationHubClient _hub;
    private readonly ILogger<NotificationHubService> _logger;

    public NotificationHubService(NotificationHubClient hub, ILogger<NotificationHubService> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task RegisterDeviceAsync(DeviceRegistration registration, CancellationToken ct = default)
    {
        var installation = new Installation
        {
            InstallationId = registration.InstallationId,
            Platform = registration.Platform.ToLowerInvariant() switch
            {
                "apns" => NotificationPlatform.Apns,
                "wns" => NotificationPlatform.Wns,
                _ => NotificationPlatform.Fcm
            },
            PushChannel = registration.PushHandle,
            Tags = registration.Tags
        };

        await _hub.CreateOrUpdateInstallationAsync(installation, ct);
        _logger.LogInformation("Registered device {Id} ({Platform}) with tags {Tags}",
            registration.InstallationId, registration.Platform, string.Join(",", registration.Tags));
    }

    public async Task NotifyPromotionActivatedAsync(Promotion promotion, CancellationToken ct = default)
    {
        // Template push so each platform renders the same logical message.
        var payload = new Dictionary<string, string>
        {
            ["title"] = "New promotion live!",
            ["body"] = $"{promotion.Name} — save now at participating stores."
        };

        // Target only devices tagged for this region.
        var tagExpression = $"region:{promotion.Region}";
        await _hub.SendTemplateNotificationAsync(payload, tagExpression, ct);
        _logger.LogInformation("Sent activation push for {PromotionId} to tag {Tag}", promotion.Id, tagExpression);
    }
}
