namespace Promotions.Domain.Models;

/// <summary>
/// Lifecycle states a promotion moves through. Transitions are driven by the
/// approval Durable Functions orchestration and the expiry timer scan.
/// </summary>
public enum PromotionStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Active = 4,
    Expired = 5,
    Retired = 6
}
