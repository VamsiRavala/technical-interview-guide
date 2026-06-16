using System.Text.Json.Serialization;

namespace Promotions.Domain.Models;

/// <summary>
/// The promotion aggregate root. Persisted as a JSON document in Cosmos DB,
/// partitioned by <see cref="Region"/> so reads/writes for a campaign stay
/// inside a single logical partition.
/// </summary>
public sealed class Promotion
{
    /// <summary>Cosmos DB item id. Lower-cased GUID string.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("n");

    /// <summary>
    /// Cosmos partition key. Co-locating a campaign's reads/writes in one
    /// partition keeps point reads at 1 RU and avoids cross-partition fan-out.
    /// </summary>
    [JsonPropertyName("region")]
    public string Region { get; set; } = "global";

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public PromotionType Type { get; set; }

    [JsonPropertyName("status")]
    public PromotionStatus Status { get; set; } = PromotionStatus.Draft;

    /// <summary>Percentage (0-100) or flat amount, interpreted by <see cref="Type"/>.</summary>
    [JsonPropertyName("discountValue")]
    public decimal DiscountValue { get; set; }

    [JsonPropertyName("startDate")]
    public DateTimeOffset StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTimeOffset EndDate { get; set; }

    [JsonPropertyName("storeIds")]
    public List<string> StoreIds { get; set; } = new();

    [JsonPropertyName("applicableProducts")]
    public List<string> ApplicableProducts { get; set; } = new();

    /// <summary>Minimum cart value required for the promotion to apply.</summary>
    [JsonPropertyName("minimumCartValue")]
    public decimal MinimumCartValue { get; set; }

    [JsonPropertyName("createdBy")]
    public string CreatedBy { get; set; } = "system";

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("approvedBy")]
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Optimistic-concurrency token. Maps to the Cosmos <c>_etag</c> system
    /// property and is used for compare-and-swap updates.
    /// </summary>
    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }

    public bool IsCurrentlyValid(DateTimeOffset now) =>
        Status == PromotionStatus.Active && now >= StartDate && now <= EndDate;
}
