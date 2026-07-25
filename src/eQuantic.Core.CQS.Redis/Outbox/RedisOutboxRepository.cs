using System.Text.Json;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Redis.Options;
using StackExchange.Redis;

namespace eQuantic.Core.CQS.Redis.Outbox;

/// <summary>Redis Outbox Repository</summary>
public class RedisOutboxRepository(IConnectionMultiplexer redis, RedisOptions options) : IOutboxRepository
{
    /// <summary>
    /// How long a claimed message stays invisible to other relays. A relay that
    /// dies mid-delivery releases its batch after this window instead of
    /// stranding it.
    /// </summary>
    private static readonly TimeSpan ClaimLease = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Takes the messages already due and leases them in the same breath. Redis
    /// runs a script atomically, so two relays cannot come away with the same id.
    /// </summary>
    private const string ClaimScript = """
        local ids = redis.call('ZRANGEBYSCORE', KEYS[1], '-inf', ARGV[1], 'LIMIT', 0, ARGV[2])
        for i = 1, #ids do
            redis.call('ZADD', KEYS[1], ARGV[3], ids[i])
        end
        return ids
        """;

    private readonly IDatabase _db = redis.GetDatabase(options.Database);
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private string Key(Guid id) => $"{options.KeyPrefix}outbox:{id}";
    private string PendingKey => $"{options.KeyPrefix}outbox:pending";

    /// <summary>
    /// The pending set is scored by when the message becomes deliverable, which
    /// for a new message is simply when it was created — so the due ones come out
    /// oldest first and a backed-off one stays out of range until its time comes.
    /// </summary>
    private static double DueScore(IOutboxMessage msg) => (msg.NextAttemptAt ?? msg.CreatedAt).Ticks;

    public async Task Add(IOutboxMessage msg, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(msg, _json);
        var key = Key(msg.Id);
        await _db.StringSetAsync(key, json);
        if (options.DefaultExpiration.HasValue)
            await _db.KeyExpireAsync(key, options.DefaultExpiration.Value);
        await _db.SortedSetAddAsync(PendingKey, msg.Id.ToString(), DueScore(msg));
    }

    /// <inheritdoc />
    /// <remarks>Claims the batch atomically through <see cref="ClaimScript" />.</remarks>
    public async Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize = 100, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var claimed = await _db.ScriptEvaluateAsync(
            ClaimScript,
            [PendingKey],
            [now.Ticks, batchSize, (now + ClaimLease).Ticks]);

        var ids = (RedisValue[]?)claimed ?? [];
        var result = new List<IOutboxMessage>();
        foreach (var id in ids)
            if (Guid.TryParse(id.ToString(), out var guid) && await LoadMessage(guid) is { } msg)
                result.Add(msg);
        return result;
    }

    public async Task MarkProcessed(Guid messageId, CancellationToken ct = default)
    {
        if (await LoadMessage(messageId) is { } msg)
        {
            msg.State = OutboxMessageState.Processed;
            msg.ProcessedAt = DateTime.UtcNow;
            msg.NextAttemptAt = null;
            await Save(msg);
        }
        await _db.SortedSetRemoveAsync(PendingKey, messageId.ToString());
    }

    /// <inheritdoc />
    public async Task MarkFailed(
        Guid messageId, string error, int maxAttempts, TimeSpan backoff, CancellationToken ct = default)
    {
        if (await LoadMessage(messageId) is not { } msg) return;

        msg.Attempts++;
        msg.LastError = error;

        var exhausted = msg.Attempts >= maxAttempts;
        msg.State = exhausted ? OutboxMessageState.Failed : OutboxMessageState.Pending;
        msg.NextAttemptAt = exhausted ? null : DateTime.UtcNow + backoff * msg.Attempts;

        await Save(msg);

        // A dead letter leaves the queue and is kept only for inspection; anything
        // still retryable goes back in, due when its backoff elapses.
        if (exhausted)
            await _db.SortedSetRemoveAsync(PendingKey, messageId.ToString());
        else
            await _db.SortedSetAddAsync(PendingKey, messageId.ToString(), DueScore(msg));
    }

    public async Task CleanupProcessed(TimeSpan olderThan, CancellationToken ct = default)
    {
        // In production, scan for processed messages and delete old ones
        await Task.CompletedTask;
    }

    private async Task Save(OutboxMessage msg)
    {
        var key = Key(msg.Id);
        // keepTtl, so re-saving a message does not resurrect one that was meant to expire.
        await _db.StringSetAsync(key, JsonSerializer.Serialize(msg, _json), expiry: null, keepTtl: true);
    }

    private async Task<OutboxMessage?> LoadMessage(Guid id)
    {
        var json = await _db.StringGetAsync(Key(id));
        return json.IsNullOrEmpty ? null : JsonSerializer.Deserialize<OutboxMessage>(json.ToString(), _json);
    }
}
