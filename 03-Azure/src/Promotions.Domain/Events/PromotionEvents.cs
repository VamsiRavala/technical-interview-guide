using Promotions.Domain.Models;

namespace Promotions.Domain.Events;

/// <summary>
/// Base for every domain event. Carries the correlation metadata needed to
/// trace a message end-to-end across Service Bus, Event Grid and Functions.
/// </summary>
public abstract record PromotionEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public string PromotionId { get; init; } = string.Empty;
    public string Region { get; init; } = "global";
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Stable event type name, used as the Service Bus subject /
    /// Event Grid event type for subscription filtering.</summary>
    public abstract string EventType { get; }
}

/// <summary>Raised when a marketer submits a new promotion for approval.</summary>
public sealed record PromotionCreatedEvent : PromotionEvent
{
    public override string EventType => "Promotion.Created";
    public string Name { get; init; } = string.Empty;
    public PromotionType Type { get; init; }
    public decimal DiscountValue { get; init; }
}

/// <summary>Raised when the approval workflow completes successfully.</summary>
public sealed record PromotionApprovedEvent : PromotionEvent
{
    public override string EventType => "Promotion.Approved";
    public string ApprovedBy { get; init; } = string.Empty;
}

/// <summary>Raised when the approval workflow rejects a promotion.</summary>
public sealed record PromotionRejectedEvent : PromotionEvent
{
    public override string EventType => "Promotion.Rejected";
    public string Reason { get; init; } = string.Empty;
}

/// <summary>Raised when an approved promotion goes live and must reach POS.</summary>
public sealed record PromotionActivatedEvent : PromotionEvent
{
    public override string EventType => "Promotion.Activated";
    public IReadOnlyList<string> StoreIds { get; init; } = Array.Empty<string>();
}

/// <summary>Raised by the expiry timer scan when a promotion passes its end date.</summary>
public sealed record PromotionExpiredEvent : PromotionEvent
{
    public override string EventType => "Promotion.Expired";
}
