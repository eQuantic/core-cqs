using System.Text.Json;
using eQuantic.Core.CQS.Abstractions.Scheduling;
using eQuantic.Core.CQS.Redis.Options;
using StackExchange.Redis;

namespace eQuantic.Core.CQS.Redis.Scheduling;

/// <summary>Redis Job Scheduler</summary>
public class RedisJobScheduler(IConnectionMultiplexer redis, RedisOptions options) : IJobScheduler
{
    private readonly IDatabase _db = redis.GetDatabase(options.Database);
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private string Key(Guid id) => $"{options.KeyPrefix}job:{id}";
    private string ScheduleKey => $"{options.KeyPrefix}job:schedule";

    public async Task<Guid> Schedule<TRequest>(TRequest request, DateTime executeAt, CancellationToken ct = default)
    {
        var jobId = Guid.NewGuid();
        var job = new { JobId = jobId, ScheduledAt = executeAt, RequestType = typeof(TRequest).AssemblyQualifiedName, Request = request };
        await _db.StringSetAsync(Key(jobId), JsonSerializer.Serialize(job, _json));
        await _db.SortedSetAddAsync(ScheduleKey, jobId.ToString(), executeAt.Ticks);
        return jobId;
    }

    public Task<Guid> Schedule<TRequest>(TRequest request, TimeSpan delay, CancellationToken ct = default) =>
        Schedule(request, DateTime.UtcNow.Add(delay), ct);

    public async Task<bool> Cancel(Guid jobId, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(Key(jobId));
        return await _db.SortedSetRemoveAsync(ScheduleKey, jobId.ToString());
    }

    public async Task<IReadOnlyList<IScheduledJob>> GetPending(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow.Ticks;
        var ids = await _db.SortedSetRangeByScoreAsync(ScheduleKey, 0, now);
        // Return empty for now - full implementation would deserialize jobs
        return Array.Empty<IScheduledJob>();
    }
}