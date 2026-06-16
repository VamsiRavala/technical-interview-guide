using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Models;
using Promotions.Domain.Services;

namespace Promotions.Functions.Triggers;

/// <summary>
/// Azure Functions topic (03): HTTP-triggered serverless endpoints. Shows the
/// trigger + DI-injected services pattern in the isolated worker model.
/// </summary>
public sealed class HttpTriggers
{
    private readonly PromotionService _service;
    private readonly ILogger<HttpTriggers> _logger;

    public HttpTriggers(PromotionService service, ILogger<HttpTriggers> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>POST /api/promotions — create a promotion via the function host.</summary>
    [Function("CreatePromotion")]
    public async Task<HttpResponseData> Create(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "promotions")] HttpRequestData req)
    {
        var promotion = await req.ReadFromJsonAsync<Promotion>();
        if (promotion is null)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var created = await _service.CreateAsync(promotion);
        _logger.LogInformation("Created promotion {Id} via HTTP trigger", created.Id);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(created);
        return response;
    }
}
