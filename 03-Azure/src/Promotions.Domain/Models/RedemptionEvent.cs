using System.Text.Json.Serialization;

namespace Promotions.Domain.Models;

/// <summary>
/// A single promotion redemption emitted by a POS terminal. These arrive at
/// very high volume (millions/day on peak), so they are streamed through
/// Azure Event Hubs rather than queued — this is telemetry, not a command.
/// </summary>
public sealed class RedemptionEvent
{
    [JsonPropertyName("promotionId")]
    public string PromotionId { get; set; } = string.Empty;

    [JsonPropertyName("storeId")]
    public string StoreId { get; set; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; set; } = "global";

    [JsonPropertyName("discountApplied")]
    public decimal DiscountApplied { get; set; }

    [JsonPropertyName("cartValue")]
    public decimal CartValue { get; set; }

    [JsonPropertyName("redeemedAt")]
    public DateTimeOffset RedeemedAt { get; set; } = DateTimeOffset.UtcNow;
}
