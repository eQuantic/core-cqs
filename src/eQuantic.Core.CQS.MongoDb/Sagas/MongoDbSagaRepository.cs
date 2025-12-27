using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.MongoDb.Options;
using MongoDB.Driver;

namespace eQuantic.Core.CQS.MongoDb.Sagas;

/// <summary>MongoDB Saga Repository</summary>
public class MongoDbSagaRepository<TData> : ISagaRepository<TData> where TData : ISagaData
{
    private readonly IMongoCollection<TData> _collection;

    public MongoDbSagaRepository(IMongoClient client, MongoDbOptions options)
    {
        var db = client.GetDatabase(options.DatabaseName);
        _collection = db.GetCollection<TData>($"{options.CollectionPrefix}saga_{typeof(TData).Name.ToLowerInvariant()}");
        
        // Create unique index on SagaId - skip if SagaId is already the _id field
        // or if index already exists with different options
        try
        {
            _collection.Indexes.CreateOne(new CreateIndexModel<TData>(
                Builders<TData>.IndexKeys.Ascending(x => x.SagaId), 
                new CreateIndexOptions { Unique = true, Name = "saga_id_unique" }));
        }
        catch (MongoCommandException ex) when (
            ex.CodeName == "IndexOptionsConflict" || 
            ex.CodeName == "CannotCreateIndex" ||
            ex.Code == 67 ||  // CannotCreateIndex - SagaId is _id
            ex.Code == 85)    // IndexOptionsConflict
        {
            // Index cannot be created or already exists, ignore
        }
    }

    public async Task Save(TData data, CancellationToken ct = default) =>
        await _collection.ReplaceOneAsync(Builders<TData>.Filter.Eq(x => x.SagaId, data.SagaId), data, new ReplaceOptions { IsUpsert = true }, ct);

    public async Task<TData?> Load(Guid sagaId, CancellationToken ct = default) =>
        await _collection.Find(Builders<TData>.Filter.Eq(x => x.SagaId, sagaId)).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<TData>> Find(Func<TData, bool> predicate, CancellationToken ct = default) =>
        (await _collection.Find(FilterDefinition<TData>.Empty).ToListAsync(ct)).Where(predicate).ToList();

    public async Task<IReadOnlyList<TData>> GetIncomplete(CancellationToken ct = default) =>
        await _collection.Find(Builders<TData>.Filter.Or(
            Builders<TData>.Filter.Eq(x => x.State, SagaState.InProgress),
            Builders<TData>.Filter.Eq(x => x.State, SagaState.NotStarted))).ToListAsync(ct);

    public async Task Delete(Guid sagaId, CancellationToken ct = default) =>
        await _collection.DeleteOneAsync(Builders<TData>.Filter.Eq(x => x.SagaId, sagaId), ct);
}