using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Abstractions;
using Promotions.Domain.Models;

namespace Promotions.Infrastructure.Cosmos;

/// <summary>
/// Cosmos DB document repository for the promotion aggregate (topic 10).
/// Demonstrates: partition-key point reads (1 RU), parameterized queries,
/// optimistic concurrency via ETag, and Session consistency by default.
/// </summary>
public sealed class CosmosPromotionRepository : IPromotionRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosPromotionRepository> _logger;

    public CosmosPromotionRepository(CosmosClient client, CosmosOptions options, ILogger<CosmosPromotionRepository> logger)
    {
        _container = client.GetContainer(options.Database, options.Container);
        _logger = logger;
    }

    public async Task<Promotion?> GetAsync(string id, string region, CancellationToken ct = default)
    {
        try
        {
            // Point read: id + partition key. Cheapest possible Cosmos operation.
            var response = await _container.ReadItemAsync<Promotion>(id, new PartitionKey(region), cancellationToken: ct);
            _logger.LogDebug("Cosmos point read {Id} cost {RU} RU", id, response.RequestCharge);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<Promotion>> ListByRegionAsync(string region, CancellationToken ct = default)
    {
        // Single-partition query — scoped to one partition key, so no cross-partition fan-out.
        var query = new QueryDefinition("SELECT * FROM c WHERE c.region = @region ORDER BY c.createdAt DESC")
            .WithParameter("@region", region);

        return await RunQueryAsync(query, new PartitionKey(region), ct);
    }

    public async Task<IReadOnlyList<Promotion>> ListExpiringBeforeAsync(DateTimeOffset cutoff, CancellationToken ct = default)
    {
        // Cross-partition query (no partition key supplied) — used by the expiry timer scan.
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.status = @active AND c.endDate <= @cutoff")
            .WithParameter("@active", (int)PromotionStatus.Active)
            .WithParameter("@cutoff", cutoff);

        return await RunQueryAsync(query, partitionKey: null, ct);
    }

    public async Task<Promotion> UpsertAsync(Promotion promotion, CancellationToken ct = default)
    {
        // If we hold an ETag, do a compare-and-swap so concurrent writers can't clobber each other.
        var requestOptions = string.IsNullOrEmpty(promotion.ETag)
            ? null
            : new ItemRequestOptions { IfMatchEtag = promotion.ETag };

        try
        {
            var response = await _container.UpsertItemAsync(
                promotion, new PartitionKey(promotion.Region), requestOptions, ct);
            promotion.ETag = response.ETag;
            return promotion;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
        {
            _logger.LogWarning("Optimistic concurrency conflict upserting promotion {Id}", promotion.Id);
            throw new InvalidOperationException(
                $"Promotion {promotion.Id} was modified by another writer. Reload and retry.", ex);
        }
    }

    private async Task<IReadOnlyList<Promotion>> RunQueryAsync(
        QueryDefinition query, PartitionKey? partitionKey, CancellationToken ct)
    {
        var options = new QueryRequestOptions();
        if (partitionKey is { } pk) options.PartitionKey = pk;

        var results = new List<Promotion>();
        using var iterator = _container.GetItemQueryIterator<Promotion>(query, requestOptions: options);
        double totalRu = 0;
        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(ct);
            totalRu += page.RequestCharge;
            results.AddRange(page);
        }
        _logger.LogDebug("Cosmos query returned {Count} items for {RU} RU", results.Count, totalRu);
        return results;
    }
}
