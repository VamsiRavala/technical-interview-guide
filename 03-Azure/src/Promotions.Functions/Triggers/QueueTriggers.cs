using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Abstractions;
using Promotions.Domain.Services;

namespace Promotions.Functions.Triggers;

/// <summary>
/// Storage Queue topic (09): pick up cheap background jobs. Here a
/// CatalogRebuildJob causes us to read the region's promotions from Cosmos and
/// write a fresh POS catalog blob (Storage Account, topic 08) — which in turn
/// fires the blob trigger downstream.
/// </summary>
public sealed class QueueTriggers
{
    private readonly IPromotionRepository _repository;
    private readonly IBlobStorageService _blob;
    private readonly ILogger<QueueTriggers> _logger;

    public QueueTriggers(IPromotionRepository repository, IBlobStorageService blob, ILogger<QueueTriggers> logger)
    {
        _repository = repository;
        _blob = blob;
        _logger = logger;
    }

    [Function("RebuildCatalog")]
    public async Task RebuildCatalog(
        [QueueTrigger("%Azure:Storage:JobQueue%", Connection = "StorageConnection")] string message)
    {
        var job = JsonSerializer.Deserialize<CatalogRebuildJob>(message);
        if (job is null)
        {
            _logger.LogWarning("Could not deserialize catalog rebuild job: {Message}", message);
            return;
        }

        var promotions = await _repository.ListByRegionAsync(job.Region);
        var catalog = JsonSerializer.Serialize(new
        {
            region = job.Region,
            generatedAt = DateTimeOffset.UtcNow,
            promotions
        });

        var uri = await _blob.UploadCatalogAsync(job.Region, catalog);
        _logger.LogInformation("Rebuilt catalog for region {Region}: {Uri}", job.Region, uri);
    }
}
