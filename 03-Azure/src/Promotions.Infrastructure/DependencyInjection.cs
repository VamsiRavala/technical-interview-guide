using Azure;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Promotions.Domain.Abstractions;
using Promotions.Infrastructure.Cosmos;
using Promotions.Infrastructure.Events;
using Promotions.Infrastructure.Messaging;
using Promotions.Infrastructure.Notifications;
using Promotions.Infrastructure.Storage;

namespace Promotions.Infrastructure;

/// <summary>
/// Registers every Azure client + abstraction. Clients are singletons (they
/// pool connections internally). Auth: when an explicit key/connection string
/// is configured it is used (local dev / emulators); otherwise
/// <see cref="DefaultAzureCredential"/> + Managed Identity (production).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPromotionsInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var options = new AzureOptions();
        config.GetSection(AzureOptions.SectionName).Bind(options);
        services.AddSingleton(options);
        services.AddSingleton(options.Cosmos);
        services.AddSingleton(options.ServiceBus);
        services.AddSingleton(options.Storage);

        var credential = new DefaultAzureCredential();

        // --- Cosmos DB (10) ---
        services.AddSingleton(_ =>
        {
            var clientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
                ConsistencyLevel = ConsistencyLevel.Session
            };
            return string.IsNullOrEmpty(options.Cosmos.AccountKey)
                ? new CosmosClient(options.Cosmos.AccountEndpoint, credential, clientOptions)
                : new CosmosClient(options.Cosmos.AccountEndpoint, options.Cosmos.AccountKey, clientOptions);
        });
        services.AddSingleton<IPromotionRepository, CosmosPromotionRepository>();

        // --- Service Bus (05) ---
        services.AddSingleton(_ => string.IsNullOrEmpty(options.ServiceBus.ConnectionString)
            ? new ServiceBusClient(options.ServiceBus.FullyQualifiedNamespace, credential)
            : new ServiceBusClient(options.ServiceBus.ConnectionString));
        services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();

        // --- Event Grid (07) ---
        services.AddSingleton(_ => string.IsNullOrEmpty(options.EventGrid.TopicKey)
            ? new EventGridPublisherClient(new Uri(options.EventGrid.TopicEndpoint), credential)
            : new EventGridPublisherClient(new Uri(options.EventGrid.TopicEndpoint), new AzureKeyCredential(options.EventGrid.TopicKey)));
        services.AddSingleton<IEventGridPublisher, EventGridPublisher>();

        // --- Event Hubs (06) ---
        services.AddSingleton(_ => string.IsNullOrEmpty(options.EventHubs.ConnectionString)
            ? new EventHubProducerClient(options.EventHubs.FullyQualifiedNamespace, options.EventHubs.HubName, credential)
            : new EventHubProducerClient(options.EventHubs.ConnectionString, options.EventHubs.HubName));
        services.AddSingleton<IRedemptionStream, EventHubsRedemptionStream>();

        // --- Storage: Blob (08) + Queue (09) ---
        services.AddSingleton(_ => string.IsNullOrEmpty(options.Storage.ConnectionString)
            ? new BlobServiceClient(new Uri(options.Storage.BlobServiceUri!), credential)
            : new BlobServiceClient(options.Storage.ConnectionString));
        services.AddSingleton<IBlobStorageService, BlobStorageService>();

        services.AddSingleton(_ =>
        {
            var queueClientOptions = new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 };
            return string.IsNullOrEmpty(options.Storage.ConnectionString)
                ? new QueueClient(new Uri($"{options.Storage.QueueServiceUri!.TrimEnd('/')}/{options.Storage.JobQueue}"), credential, queueClientOptions)
                : new QueueClient(options.Storage.ConnectionString, options.Storage.JobQueue, queueClientOptions);
        });
        services.AddSingleton<IStorageQueueClient, StorageQueueClientAdapter>();

        // --- Notification Hubs (11) ---
        services.AddSingleton(_ =>
            NotificationHubClient.CreateClientFromConnectionString(
                options.NotificationHub.ConnectionString, options.NotificationHub.HubName));
        services.AddSingleton<IPushNotificationService, NotificationHubService>();

        return services;
    }
}
