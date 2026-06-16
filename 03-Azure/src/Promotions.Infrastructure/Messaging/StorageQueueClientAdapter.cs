using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using Promotions.Domain.Abstractions;

namespace Promotions.Infrastructure.Messaging;

/// <summary>
/// Thin wrapper over an Azure Storage Queue (topic 09) for cheap, simple
/// background jobs — e.g. "regenerate the POS catalog for region X". Messages
/// are Base64-encoded JSON (the Functions queue trigger expects Base64).
/// </summary>
public sealed class StorageQueueClientAdapter : IStorageQueueClient
{
    private readonly QueueClient _queue;
    private readonly ILogger<StorageQueueClientAdapter> _logger;

    public StorageQueueClientAdapter(QueueClient queue, ILogger<StorageQueueClientAdapter> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    public async Task EnqueueAsync<T>(T message, CancellationToken ct = default)
    {
        await _queue.CreateIfNotExistsAsync(cancellationToken: ct);
        var json = JsonSerializer.Serialize(message);
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
        await _queue.SendMessageAsync(base64, cancellationToken: ct);
        _logger.LogInformation("Enqueued {Type} job to storage queue {Queue}", typeof(T).Name, _queue.Name);
    }
}
