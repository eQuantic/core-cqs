using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.MongoDb.Options;
using MongoDB.Driver;

namespace eQuantic.Core.CQS.MongoDb.Outbox;

/// <summary>MongoDB Outbox Repository</summary>
public class MongoDbOutboxRepository : IOutboxRepository
{
    /// <summary>
    /// How long a claimed message stays invisible to other relays. A relay that
    /// dies mid-delivery releases its batch after this window instead of
    /// stranding it.
    /// </summary>
    private static readonly TimeSpan ClaimLease = TimeSpan.FromMinutes(1);

    private readonly IMongoCollection<OutboxMessage> _collection;

    public MongoDbOutboxRepository(IMongoClient client, MongoDbOptions options)
    {
        var db = client.GetDatabase(options.DatabaseName);
        _collection = db.GetCollection<OutboxMessage>($"{options.CollectionPrefix}outbox");
    }

    public async Task Add(IOutboxMessage msg, CancellationToken ct = default) =>
        await _collection.InsertOneAsync((OutboxMessage)msg, cancellationToken: ct);

    /// <inheritdoc />
    /// <remarks>
    /// Claims the batch one message at a time with <c>FindOneAndUpdate</c> — each
    /// call is atomic on a single document, so concurrent relays never take the
    /// same message. The claim leases the message by pushing
    /// <see cref="IOutboxMessage.NextAttemptAt" /> out; a relay that dies
    /// mid-delivery releases its batch when the lease expires.
    /// </remarks>
    public async Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize = 100, CancellationToken ct = default)
    {
        var result = new List<IOutboxMessage>();
        for (var i = 0; i < batchSize; i++)
        {
            var now = DateTime.UtcNow;
            var claimed = await _collection.FindOneAndUpdateAsync(
                Builders<OutboxMessage>.Filter.And(
                    Builders<OutboxMessage>.Filter.Eq(x => x.State, OutboxMessageState.Pending),
                    Builders<OutboxMessage>.Filter.Or(
                        Builders<OutboxMessage>.Filter.Eq(x => x.NextAttemptAt, null),
                        Builders<OutboxMessage>.Filter.Lte(x => x.NextAttemptAt, now))),
                Builders<OutboxMessage>.Update.Set(x => x.NextAttemptAt, (DateTime?)(now + ClaimLease)),
                new FindOneAndUpdateOptions<OutboxMessage>
                {
                    Sort = Builders<OutboxMessage>.Sort.Ascending(x => x.CreatedAt),
                    ReturnDocument = ReturnDocument.After
                },
                ct);

            if (claimed is null) break;
            result.Add(claimed);
        }

        return result;
    }

    public async Task MarkProcessed(Guid messageId, CancellationToken ct = default) =>
        await _collection.UpdateOneAsync(Builders<OutboxMessage>.Filter.Eq(x => x.Id, messageId),
            Builders<OutboxMessage>.Update
                .Set(x => x.State, OutboxMessageState.Processed)
                .Set(x => x.ProcessedAt, DateTime.UtcNow)
                .Set(x => x.NextAttemptAt, (DateTime?)null), cancellationToken: ct);

    /// <inheritdoc />
    public async Task MarkFailed(
        Guid messageId, string error, int maxAttempts, TimeSpan backoff, CancellationToken ct = default)
    {
        var filter = Builders<OutboxMessage>.Filter.Eq(x => x.Id, messageId);
        // The increment has to land before the state decision, otherwise a
        // concurrent failure could read a stale attempt count.
        var current = await _collection.FindOneAndUpdateAsync(filter,
            Builders<OutboxMessage>.Update.Inc(x => x.Attempts, 1).Set(x => x.LastError, error),
            new FindOneAndUpdateOptions<OutboxMessage> { ReturnDocument = ReturnDocument.After }, ct);

        if (current is null) return;

        var exhausted = current.Attempts >= maxAttempts;
        await _collection.UpdateOneAsync(filter,
            Builders<OutboxMessage>.Update
                .Set(x => x.State, exhausted ? OutboxMessageState.Failed : OutboxMessageState.Pending)
                .Set(x => x.NextAttemptAt, exhausted ? null : (DateTime?)(DateTime.UtcNow + backoff * current.Attempts)),
            cancellationToken: ct);
    }

    public async Task CleanupProcessed(TimeSpan olderThan, CancellationToken ct = default) =>
        await _collection.DeleteManyAsync(Builders<OutboxMessage>.Filter.And(
            Builders<OutboxMessage>.Filter.Eq(x => x.State, OutboxMessageState.Processed),
            Builders<OutboxMessage>.Filter.Lt(x => x.ProcessedAt, DateTime.UtcNow.Subtract(olderThan))), ct);
}