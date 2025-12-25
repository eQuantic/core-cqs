using System.Text.Json;
using eQuantic.Core.CQS.Abstractions.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.CQS.EntityFramework.Scheduling;

/// <summary>EF Core Job Scheduler</summary>
public class EfJobScheduler<TContext> : IJobScheduler
    where TContext : DbContext, ICqsDbContext
{
    private readonly TContext _context;
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public EfJobScheduler(TContext context) => _context = context;

    public async Task<Guid> Schedule<TRequest>(TRequest request, DateTime executeAt, CancellationToken ct = default)
    {
        var entity = new ScheduledJobEntity
        {
            Id = Guid.NewGuid(),
            ScheduledAt = executeAt,
            RequestType = typeof(TRequest).AssemblyQualifiedName ?? typeof(TRequest).FullName ?? "",
            RequestJson = JsonSerializer.Serialize(request, _json)
        };
        _context.ScheduledJobs.Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity.Id;
    }

    public Task<Guid> Schedule<TRequest>(TRequest request, TimeSpan delay, CancellationToken ct = default) =>
        Schedule(request, DateTime.UtcNow.Add(delay), ct);

    public async Task<bool> Cancel(Guid jobId, CancellationToken ct = default)
    {
        var entity = await _context.ScheduledJobs.FindAsync(new object[] { jobId }, ct);
        if (entity == null) return false;
        _context.ScheduledJobs.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public Task<IReadOnlyList<IScheduledJob>> GetPending(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<IScheduledJob>>(Array.Empty<IScheduledJob>()); // Simplified
}