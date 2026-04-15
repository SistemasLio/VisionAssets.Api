using System.Collections.Concurrent;

namespace VisionAssets.Api;

/// <summary>Armazenamento em memória para MVP; substituir por BD (SQL Server, PostgreSQL, …) em produção.</summary>
public sealed class InventorySnapshotStore
{
    private readonly ConcurrentDictionary<string, SnapshotRecord> _byIdempotency = new(StringComparer.Ordinal);
    private readonly ILogger<InventorySnapshotStore> _logger;

    public InventorySnapshotStore(ILogger<InventorySnapshotStore> logger)
    {
        _logger = logger;
    }

    public SnapshotRecord Accept(string jsonBody, string? idempotencyKey)
    {
        if (!string.IsNullOrEmpty(idempotencyKey) && _byIdempotency.TryGetValue(idempotencyKey, out var existing))
        {
            _logger.LogInformation("Idempotência: chave {Key} reutilizada.", idempotencyKey);
            return existing;
        }

        var record = new SnapshotRecord(Guid.NewGuid(), DateTimeOffset.UtcNow);
        if (!string.IsNullOrEmpty(idempotencyKey))
            _byIdempotency[idempotencyKey] = record;

        _logger.LogInformation("Snapshot aceite ({Bytes} bytes JSON).", jsonBody.Length);
        return record;
    }
}

public readonly record struct SnapshotRecord(Guid CorrelationId, DateTimeOffset ReceivedAtUtc);
