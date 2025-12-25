using eQuantic.Core.CQS.Abstractions.Scheduling;

namespace eQuantic.Core.CQS.Scheduling;

/// <summary>
/// Scheduled job implementation
/// </summary>
public sealed record ScheduledJob<TRequest> : IScheduledJob
{
    public Guid JobId { get; init; } = Guid.NewGuid();
    public DateTime ScheduledAt { get; init; }
    public required TRequest Request { get; init; }

    object IScheduledJob.Request => Request!;
}
