using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Promotions.Functions.Triggers;

/// <summary>
/// Storage Account topic (08): blob trigger. Fires when a freshly generated POS
/// catalog lands in the "promotion-catalogs" container — e.g. to validate it and
/// kick off distribution to store systems. In production prefer an Event Grid
/// "BlobCreated" subscription over the polling blob trigger at scale.
/// </summary>
public sealed class BlobTriggers
{
    private readonly ILogger<BlobTriggers> _logger;

    public BlobTriggers(ILogger<BlobTriggers> logger) => _logger = logger;

    [Function("OnCatalogUploaded")]
    public void OnCatalogUploaded(
        [BlobTrigger("%Azure:Storage:CatalogContainer%/{name}", Connection = "StorageConnection")] byte[] content,
        string name)
    {
        _logger.LogInformation("Catalog blob '{Name}' uploaded ({Bytes} bytes) — ready to distribute to POS",
            name, content.Length);
    }
}
