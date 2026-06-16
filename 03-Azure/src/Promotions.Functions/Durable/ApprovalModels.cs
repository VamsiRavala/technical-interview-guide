namespace Promotions.Functions.Durable;

/// <summary>Input passed to the approval orchestration.</summary>
public sealed record ApprovalInput(string PromotionId, string Region);

/// <summary>The human approver's decision, raised as an external event.</summary>
public sealed record ApprovalDecision(bool Approved, string Approver, string? Reason);

/// <summary>Per-store distribution result, aggregated in the fan-in step.</summary>
public sealed record StoreDistributionResult(string StoreId, bool Success);

/// <summary>Final orchestration output, queryable via the status endpoint.</summary>
public sealed record ApprovalOutcome(string PromotionId, string Status, int StoresReached);
