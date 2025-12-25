using System.Text.Json;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Redis.Options;
using StackExchange.Redis;

namespace eQuantic.Core.CQS.Redis.Outbox;

/// <summary>Redis Outbox Repository</summary>
public class RedisOutboxRepository(IConnectionMultiplexer redis, RedisOptions options) : IOutboxRepository
{
    private readonly IDatabase _db = redis.GetDatabase(options.Database);
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private string Key(Guid id) => $"{options.KeyPrefix}outbox:{id}";
    private string PendingKey => $"{options.KeyPrefix}outbox:pending";

    public async Task Add(IOutboxMessage msg, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(msg, _json);
        var key = Key(msg.Id);
        await _db.StringSetAsync(key, json);
        if (options.DefaultExpiration.HasValue)
            await _db.KeyExpireAsync(key, options.DefaultExpiration.Value);
        await _db.SortedSetAddAsync(PendingKey, msg.Id.ToString(), msg.CreatedAt.Ticks);
    }

    public async Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize = 100, CancellationToken ct = default)
    {
        var ids = await _db.SortedSetRangeByRankAsync(PendingKey, 0, batchSize - 1);
        var result = new List<IOutboxMessage>();
        foreach (var id in ids)
            if (Guid.TryParse(id, out var guid) && await LoadMessage(guid) is { } msg)
                result.Add(msg);
        return result;
    }

    public async Task MarkProcessed(Guid messageId, CancellationToken ct = default)
    {
        var json = await _db.StringGetAsync(Key(messageId));
        if (!json.IsNullOrEmpty)
        {
            var msg = JsonSerializer.Deserialize<OutboxMessage>(json.ToString(), _json);
            if (msg != null)
            {
                msg.State = OutboxMessageState.Processed;
                msg.ProcessedAt = DateTime.UtcNow;
                await _db.StringSetAsync(Key(messageId), JsonSerializer.Serialize(msg, _json));
            }
        }
        await _db.SortedSetRemoveAsync(PendingKey, messageId.ToString());
    }

    public async Task MarkFailed(Guid messageId, string error, CancellationToken ct = default)
    {
        var json = await _db.StringGetAsync(Key(messageId));
        if (!json.IsNullOrEmpty)
        {
            var msg = JsonSerializer.Deserialize<OutboxMessage>(json.ToString(), _json);
            if (msg != null)
            {
                msg.State = OutboxMessageState.Failed;
                msg.LastError = error;
                msg.Attempts++;
                await _db.StringSetAsync(Key(messageId), JsonSerializer.Serialize(msg, _json));
            }
        }
    }

    public async Task CleanupProcessed(TimeSpan olderThan, CancellationToken ct = default)
    {
        // In production, scan for processed messages and delete old ones
        await Task.CompletedTask;
    }

    private async Task<OutboxMessage?> LoadMessage(Guid id)
    {
        var json = await _db.StringGetAsync(Key(id));
        return json.IsNullOrEmpty ? null : JsonSerializer.Deserialize<OutboxMessage>(json.ToString(), _json);
    }
}