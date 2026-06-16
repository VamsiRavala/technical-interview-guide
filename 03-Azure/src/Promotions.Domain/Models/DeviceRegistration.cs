namespace Promotions.Domain.Models;

/// <summary>
/// A device that wants push notifications about promotions. Maps to an Azure
/// Notification Hubs installation; <see cref="Tags"/> drive tag-based targeting
/// (e.g. "region:US", "store:S001").
/// </summary>
public sealed class DeviceRegistration
{
    public string InstallationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>fcm | apns | wns — the target platform notification system.</summary>
    public string Platform { get; set; } = "fcm";

    /// <summary>Platform-specific device token (PNS handle).</summary>
    public string PushHandle { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new();
}
