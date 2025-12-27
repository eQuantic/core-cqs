using System.Text.Json;
using eQuantic.Core.CQS.Abstractions.Resilience;
using eQuantic.Core.CQS.Abstractions.Sagas;
using StackExchange.Redis;

namespace eQuantic.Core.CQS.Resilience.Redis;

/// <summary>
/// Redis implementation of IDeadLetterHandler.
/// Stores failed sagas in a Redis list for later processing.
/// </summary>
public class RedisDeadLetterHandler : IDeadLetterHandler
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisDeadLetterOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisDeadLetterHandler(IConnectionMultiplexer redis, RedisDeadLetterOptions options)
    {
        _redis = redis;
        _options = options;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task HandleAsync(ISagaData saga, string reason, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase(_options.Database);
        
        var deadLetterEntry = new DeadLetterEntry
        {
            SagaId = saga.SagaId,
            SagaType = saga.GetType().FullName!,
            State = saga.State.ToString(),
            Reason = reason,
            FailedAt = DateTime.UtcNow,
            SagaData = JsonSerializer.Serialize(saga, saga.GetType(), _jsonOptions)
        };
        
        var json = JsonSerializer.Serialize(deadLetterEntry, _jsonOptions);
        var key = $"{_options.KeyPrefix}:deadletter";
        
        await db.ListRightPushAsync(key, json);
        
        // Optional: Set expiry on the list
        if (_options.Expiry.HasValue)
        {
            await db.KeyExpireAsync(key, _options.Expiry.Value);
        }
    }

    /// <summary>
    /// Retrieves dead letter entries for processing.
    /// </summary>
    public async Task<IReadOnlyList<DeadLetterEntry>> GetDeadLettersAsync(int count = 100)
    {
        var db = _redis.GetDatabase(_options.Database);
        var key = $"{_options.KeyPrefix}:deadletter";
        
        var values = await db.ListRangeAsync(key, 0, count - 1);
        var entries = new List<DeadLetterEntry>();
        
        foreach (var value in values)
        {
            if (!value.IsNullOrEmpty)
            {
                var entry = JsonSerializer.Deserialize<DeadLetterEntry>(value.ToString(), _jsonOptions);
                if (entry != null) entries.Add(entry);
            }
        }
        
        return entries;
    }

    /// <summary>
    /// Removes a dead letter entry after successful reprocessing.
    /// </summary>
    public async Task RemoveAsync(DeadLetterEntry entry)
    {
        var db = _redis.GetDatabase(_options.Database);
        var key = $"{_options.KeyPrefix}:deadletter";
        var json = JsonSerializer.Serialize(entry, _jsonOptions);
        
        await db.ListRemoveAsync(key, json, 1);
    }
}

/// <summary>
/// Dead letter entry stored in Redis.
/// </summary>
public class DeadLetterEntry
{
    public Guid SagaId { get; set; }
    public string SagaType { get; set; } = "";
    public string State { get; set; } = "";
    public string Reason { get; set; } = "";
    public DateTime FailedAt { get; set; }
    public string SagaData { get; set; } = "";
}

/// <summary>
/// Configuration options for Redis dead letter handler.
/// </summary>
public class RedisDeadLetterOptions
{
    public string KeyPrefix { get; set; } = "cqs:saga";
    public int Database { get; set; } = 0;
    public TimeSpan? Expiry { get; set; }
}
