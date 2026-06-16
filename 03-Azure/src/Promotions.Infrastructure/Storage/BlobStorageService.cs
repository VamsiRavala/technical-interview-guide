using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Abstractions;

namespace Promotions.Infrastructure.Storage;

/// <summary>
/// Stores exported promotion catalog payloads as blobs (topic 08). A blob
/// upload to the "promotion-catalogs" container is what an Event Grid
/// BlobCreated subscription / Functions blob trigger reacts to downstream.
/// </summary>
public sealed class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _client;
    private readonly StorageOptions _options;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(BlobServiceClient client, StorageOptions options, ILogger<BlobStorageService> logger)
    {
        _client = client;
        _options = options;
        _logger = logger;
    }

    public async Task<Uri> UploadCatalogAsync(string region, string content, CancellationToken ct = default)
    {
        var container = _client.GetBlobContainerClient(_options.CatalogContainer);
        await container.CreateIfNotExistsAsync(cancellationToken: ct);

        var blobName = $"{region}/catalog-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.json";
        var blob = container.GetBlobClient(blobName);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = "application/json" }
        }, ct);

        _logger.LogInformation("Uploaded catalog blob {Blob}", blobName);
        return blob.Uri;
    }
}
