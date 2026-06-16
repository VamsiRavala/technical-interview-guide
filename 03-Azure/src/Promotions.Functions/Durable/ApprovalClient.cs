using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Promotions.Functions.Durable;

/// <summary>
/// Client functions for the approval orchestration — the "Async HTTP API"
/// Durable pattern: start returns 202 + a status URL the caller polls; a
/// second endpoint raises the human approval event into the running instance.
/// </summary>
public sealed class ApprovalClient
{
    private readonly ILogger<ApprovalClient> _logger;

    public ApprovalClient(ILogger<ApprovalClient> logger) => _logger = logger;

    /// <summary>POST /api/promotions/{id}/approval?region=US — start the workflow.</summary>
    [Function("StartApprovalWorkflow")]
    public async Task<HttpResponseData> Start(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "promotions/{id}/approval")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var region = System.Web.HttpUtility.ParseQueryString(req.Url.Query).Get("region") ?? "global";

        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(ApprovalOrchestration.ApprovalOrchestrator), new ApprovalInput(id, region));

        _logger.LogInformation("Started approval orchestration {InstanceId} for promotion {Id}", instanceId, id);

        // Returns 202 Accepted with statusQueryGetUri / sendEventPostUri / etc.
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }

    /// <summary>POST /api/approvals/{instanceId}/decision — raise the human decision.</summary>
    [Function("SubmitApprovalDecision")]
    public async Task<HttpResponseData> SubmitDecision(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "approvals/{instanceId}/decision")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        string instanceId)
    {
        var decision = await req.ReadFromJsonAsync<ApprovalDecision>();
        if (decision is null)
            return req.CreateResponse(HttpStatusCode.BadRequest);

        await client.RaiseEventAsync(instanceId, "ApprovalDecision", decision);
        _logger.LogInformation("Raised ApprovalDecision (approved={Approved}) to {InstanceId}",
            decision.Approved, instanceId);

        return req.CreateResponse(HttpStatusCode.Accepted);
    }
}
