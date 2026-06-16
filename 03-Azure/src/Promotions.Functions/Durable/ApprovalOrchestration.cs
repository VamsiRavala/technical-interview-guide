using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Models;

namespace Promotions.Functions.Durable;

/// <summary>
/// Durable Functions topic (04). One orchestration that demonstrates four of
/// the canonical patterns end-to-end for promotion approval:
///   1. Function chaining     — validate → (decision) → distribute → activate
///   2. Human interaction     — WaitForExternalEvent for an approver's decision
///   3. Monitor / timeout     — a durable timer races the approval (auto-reject)
///   4. Fan-out / fan-in       — distribute to all stores in parallel, then aggregate
/// State, checkpoints and retries are handled by the Durable Task Framework.
/// </summary>
public static class ApprovalOrchestration
{
    [Function(nameof(ApprovalOrchestrator))]
    public static async Task<ApprovalOutcome> ApprovalOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<ApprovalInput>()!;
        // Replay-safe logger: suppresses duplicate logs during orchestration replays.
        var logger = context.CreateReplaySafeLogger(nameof(ApprovalOrchestrator));

        // Activities can transiently fail (Cosmos throttling, etc.) — retry with backoff.
        var retry = TaskOptions.FromRetryPolicy(new RetryPolicy(
            maxNumberOfAttempts: 3,
            firstRetryInterval: TimeSpan.FromSeconds(5),
            backoffCoefficient: 2.0));

        // --- 1. Chaining: validate the promotion first ---
        var isValid = await context.CallActivityAsync<bool>(
            nameof(ApprovalActivities.ValidatePromotion), input, retry);
        if (!isValid)
        {
            await context.CallActivityAsync(nameof(ApprovalActivities.SetStatus),
                new StatusUpdate(input, PromotionStatus.Rejected, "Failed validation"), retry);
            return new ApprovalOutcome(input.PromotionId, "Rejected", 0);
        }

        // --- 2 + 3. Human interaction with a monitor-style timeout ---
        // Wait for an approver to raise "ApprovalDecision"; auto-reject after 24h.
        using var cts = new CancellationTokenSource();
        var timeout = context.CreateTimer(context.CurrentUtcDateTime.AddHours(24), cts.Token);
        var approvalTask = context.WaitForExternalEvent<ApprovalDecision>("ApprovalDecision");

        var winner = await Task.WhenAny(approvalTask, timeout);
        ApprovalDecision decision;
        if (winner == approvalTask)
        {
            cts.Cancel(); // cancel the durable timer
            decision = approvalTask.Result;
        }
        else
        {
            logger.LogWarning("Approval timed out for {Id} — auto-rejecting", input.PromotionId);
            decision = new ApprovalDecision(false, "system", "Approval timed out after 24h");
        }

        if (!decision.Approved)
        {
            await context.CallActivityAsync(nameof(ApprovalActivities.SetStatus),
                new StatusUpdate(input, PromotionStatus.Rejected, decision.Reason), retry);
            return new ApprovalOutcome(input.PromotionId, "Rejected", 0);
        }

        // Approved — record approval, then proceed to distribution.
        await context.CallActivityAsync(nameof(ApprovalActivities.SetStatus),
            new StatusUpdate(input, PromotionStatus.Approved, decision.Approver), retry);

        // --- 4. Fan-out / fan-in: push to every store in parallel ---
        var storeIds = await context.CallActivityAsync<string[]>(
            nameof(ApprovalActivities.GetStoreIds), input, retry);

        var distributions = storeIds
            .Select(storeId => context.CallActivityAsync<StoreDistributionResult>(
                nameof(ApprovalActivities.DistributeToStore), new StoreTarget(input.PromotionId, storeId), retry))
            .ToList();

        var results = await Task.WhenAll(distributions); // fan-in
        var reached = results.Count(r => r.Success);

        // Final chaining step: mark active.
        await context.CallActivityAsync(nameof(ApprovalActivities.SetStatus),
            new StatusUpdate(input, PromotionStatus.Active, decision.Approver), retry);

        logger.LogInformation("Promotion {Id} activated, reached {Reached}/{Total} stores",
            input.PromotionId, reached, storeIds.Length);
        return new ApprovalOutcome(input.PromotionId, "Active", reached);
    }
}
