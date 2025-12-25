using System.Collections.Concurrent;
using eQuantic.Core.CQS.Abstractions.Scheduling;

namespace eQuantic.Core.CQS.Scheduling;

/// <summary>
/// In-memory job scheduler (for development/testing)
/// </summary>
public class InMemoryJobScheduler : IJobScheduler
{
    private readonly ConcurrentDictionary<Guid, IScheduledJob> _jobs = new();

    public Task<Guid> Schedule<TRequest>(TRequest request, DateTime executeAt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var job = new ScheduledJob<TRequest>
        {
            ScheduledAt = executeAt,
            Request = request
        };

        _jobs.TryAdd(job.JobId, job);
        return Task.FromResult(job.JobId);
    }

    public Task<Guid> Schedule<TRequest>(TRequest request, TimeSpan delay, CancellationToken cancellationToken = default)
    {
        return Schedule(request, DateTime.UtcNow.Add(delay), cancellationToken);
    }

    public Task<bool> Cancel(Guid jobId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_jobs.TryRemove(jobId, out _));
    }

    public Task<IReadOnlyList<IScheduledJob>> GetPending(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var pending = _jobs.Values
            .Where(j => j.ScheduledAt <= now)
            .OrderBy(j => j.ScheduledAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<IScheduledJob>>(pending);
    }

    /// <summary>
    /// Processes pending jobs by retrieving and removing them
    /// </summary>
    public async Task<IReadOnlyList<IScheduledJob>> DequeueReady(CancellationToken cancellationToken = default)
    {
        var pending = await GetPending(cancellationToken);
        
        foreach (var job in pending)
        {
            _jobs.TryRemove(job.JobId, out _);
        }

        return pending;
    }
}
