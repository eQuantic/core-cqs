using System.Text.Json;
using eQuantic.Core.CQS.Abstractions.Scheduling;
using eQuantic.Core.CQS.MongoDb.Options;
using MongoDB.Driver;

namespace eQuantic.Core.CQS.MongoDb.Scheduling;

/// <summary>MongoDB Job Scheduler</summary>
public class MongoDbJobScheduler : IJobScheduler
{
    private readonly IMongoCollection<MongoDbJob> _collection;
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public MongoDbJobScheduler(IMongoClient client, MongoDbOptions options)
    {
        var db = client.GetDatabase(options.DatabaseName);
        _collection = db.GetCollection<MongoDbJob>($"{options.CollectionPrefix}jobs");
    }

    public async Task<Guid> Schedule<TRequest>(TRequest request, DateTime executeAt, CancellationToken ct = default)
    {
        var job = new MongoDbJob
        {
            Id = Guid.NewGuid(),
            ScheduledAt = executeAt,
            RequestType = typeof(TRequest).AssemblyQualifiedName ?? typeof(TRequest).FullName ?? "",
            RequestJson = JsonSerializer.Serialize(request, _json)
        };
        await _collection.InsertOneAsync(job, cancellationToken: ct);
        return job.Id;
    }

    public Task<Guid> Schedule<TRequest>(TRequest request, TimeSpan delay, CancellationToken ct = default) =>
        Schedule(request, DateTime.UtcNow.Add(delay), ct);

    public async Task<bool> Cancel(Guid jobId, CancellationToken ct = default)
    {
        var result = await _collection.DeleteOneAsync(Builders<MongoDbJob>.Filter.Eq(x => x.Id, jobId), ct);
        return result.DeletedCount > 0;
    }

    public Task<IReadOnlyList<IScheduledJob>> GetPending(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<IScheduledJob>>(Array.Empty<IScheduledJob>()); // Simplified - full impl would deserialize
}