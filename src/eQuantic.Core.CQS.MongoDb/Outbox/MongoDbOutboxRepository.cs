using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.MongoDb.Options;
using MongoDB.Driver;

namespace eQuantic.Core.CQS.MongoDb.Outbox;

/// <summary>MongoDB Outbox Repository</summary>
public class MongoDbOutboxRepository : IOutboxRepository
{
    private readonly IMongoCollection<OutboxMessage> _collection;

    public MongoDbOutboxRepository(IMongoClient client, MongoDbOptions options)
    {
        var db = client.GetDatabase(options.DatabaseName);
        _collection = db.GetCollection<OutboxMessage>($"{options.CollectionPrefix}outbox");
    }

    public async Task Add(IOutboxMessage msg, CancellationToken ct = default) =>
        await _collection.InsertOneAsync((OutboxMessage)msg, cancellationToken: ct);

    public async Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize = 100, CancellationToken ct = default) =>
        await _collection.Find(Builders<OutboxMessage>.Filter.Eq(x => x.State, OutboxMessageState.Pending))
            .SortBy(x => x.CreatedAt).Limit(batchSize).ToListAsync(ct);

    public async Task MarkProcessed(Guid messageId, CancellationToken ct = default) =>
        await _collection.UpdateOneAsync(Builders<OutboxMessage>.Filter.Eq(x => x.Id, messageId),
            Builders<OutboxMessage>.Update.Set(x => x.State, OutboxMessageState.Processed).Set(x => x.ProcessedAt, DateTime.UtcNow), cancellationToken: ct);

    public async Task MarkFailed(Guid messageId, string error, CancellationToken ct = default) =>
        await _collection.UpdateOneAsync(Builders<OutboxMessage>.Filter.Eq(x => x.Id, messageId),
            Builders<OutboxMessage>.Update.Set(x => x.State, OutboxMessageState.Failed).Set(x => x.LastError, error).Inc(x => x.Attempts, 1), cancellationToken: ct);

    public async Task CleanupProcessed(TimeSpan olderThan, CancellationToken ct = default) =>
        await _collection.DeleteManyAsync(Builders<OutboxMessage>.Filter.And(
            Builders<OutboxMessage>.Filter.Eq(x => x.State, OutboxMessageState.Processed),
            Builders<OutboxMessage>.Filter.Lt(x => x.ProcessedAt, DateTime.UtcNow.Subtract(olderThan))), ct);
}