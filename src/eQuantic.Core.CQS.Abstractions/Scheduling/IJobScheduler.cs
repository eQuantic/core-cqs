namespace eQuantic.Core.CQS.Abstractions.Scheduling;

/// <summary>
/// Interface for scheduling jobs
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Schedules a command or query for later execution
    /// </summary>
    Task<Guid> Schedule<TRequest>(TRequest request, DateTime executeAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules a command or query with a delay
    /// </summary>
    Task<Guid> Schedule<TRequest>(TRequest request, TimeSpan delay, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a scheduled job
    /// </summary>
    Task<bool> Cancel(Guid jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending jobs
    /// </summary>
    Task<IReadOnlyList<IScheduledJob>> GetPending(CancellationToken cancellationToken = default);
}