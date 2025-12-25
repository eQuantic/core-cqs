using System.Text.Json;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Redis.Options;
using StackExchange.Redis;

namespace eQuantic.Core.CQS.Redis.Sagas;

/// <summary>Redis Saga Repository</summary>
public class RedisSagaRepository<TData>(IConnectionMultiplexer redis, RedisOptions options) : ISagaRepository<TData>
    where TData : ISagaData
{
    private readonly IDatabase _db = redis.GetDatabase(options.Database);
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private string Key(Guid id) => $"{options.KeyPrefix}saga:{typeof(TData).Name}:{id}";
    private string IndexKey => $"{options.KeyPrefix}saga:{typeof(TData).Name}:idx";

    public async Task Save(TData data, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(data, _json);
        await _db.StringSetAsync(Key(data.SagaId), json, options.DefaultExpiration);
        await _db.SetAddAsync(IndexKey, data.SagaId.ToString());
    }

    public async Task<TData?> Load(Guid sagaId, CancellationToken ct = default)
    {
        var json = await _db.StringGetAsync(Key(sagaId));
        return json.IsNullOrEmpty ? default : JsonSerializer.Deserialize<TData>(json.ToString(), _json);
    }

    public async Task<IReadOnlyList<TData>> Find(Func<TData, bool> predicate, CancellationToken ct = default) =>
        (await GetAll(ct)).Where(predicate).ToList();

    public Task<IReadOnlyList<TData>> GetIncomplete(CancellationToken ct = default) =>
        Find(s => s.State is SagaState.InProgress or SagaState.NotStarted, ct);

    public async Task Delete(Guid sagaId, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(Key(sagaId));
        await _db.SetRemoveAsync(IndexKey, sagaId.ToString());
    }

    private async Task<List<TData>> GetAll(CancellationToken ct)
    {
        var ids = await _db.SetMembersAsync(IndexKey);
        var result = new List<TData>();
        foreach (var id in ids)
            if (Guid.TryParse(id, out var guid) && await Load(guid, ct) is { } saga)
                result.Add(saga);
        return result;
    }
}