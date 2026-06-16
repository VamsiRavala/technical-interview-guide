namespace Promotions.Infrastructure;

/// <summary>
/// Strongly-typed configuration for every Azure resource the solution touches.
/// Bind from configuration section "Azure". In production prefer
/// fully-qualified namespaces + Managed Identity over connection strings.
/// </summary>
public sealed class AzureOptions
{
    public const string SectionName = "Azure";

    public CosmosOptions Cosmos { get; set; } = new();
    public ServiceBusOptions ServiceBus { get; set; } = new();
    public EventGridOptions EventGrid { get; set; } = new();
    public EventHubsOptions EventHubs { get; set; } = new();
    public StorageOptions Storage { get; set; } = new();
    public NotificationHubOptions NotificationHub { get; set; } = new();
}

public sealed class CosmosOptions
{
    /// <summary>e.g. https://promo-cosmos.documents.azure.com:443/ </summary>
    public string AccountEndpoint { get; set; } = string.Empty;
    public string? AccountKey { get; set; }
    public string Database { get; set; } = "promotions";
    public string Container { get; set; } = "promotions";
}

public sealed class ServiceBusOptions
{
    /// <summary>e.g. promo-sb.servicebus.windows.net </summary>
    public string FullyQualifiedNamespace { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public string QueueOrTopic { get; set; } = "promotion-events";
}

public sealed class EventGridOptions
{
    public string TopicEndpoint { get; set; } = string.Empty;
    public string? TopicKey { get; set; }
}

public sealed class EventHubsOptions
{
    public string FullyQualifiedNamespace { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public string HubName { get; set; } = "redemptions";
}

public sealed class StorageOptions
{
    public string? ConnectionString { get; set; }
    /// <summary>e.g. https://promostorage.blob.core.windows.net </summary>
    public string? BlobServiceUri { get; set; }
    public string? QueueServiceUri { get; set; }
    public string CatalogContainer { get; set; } = "promotion-catalogs";
    public string JobQueue { get; set; } = "promotion-jobs";
}

public sealed class NotificationHubOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string HubName { get; set; } = "promotions-hub";
}
