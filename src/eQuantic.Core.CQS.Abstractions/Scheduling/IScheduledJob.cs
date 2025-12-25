namespace eQuantic.Core.CQS.Abstractions.Scheduling;

/// <summary>
/// Represents a scheduled job
/// </summary>
public interface IScheduledJob
{
    /// <summary>
    /// Unique identifier for this job
    /// </summary>
    Guid JobId { get; }

    /// <summary>
    /// When this job should execute
    /// </summary>
    DateTime ScheduledAt { get; }

    /// <summary>
    /// The command or query to execute
    /// </summary>
    object Request { get; }
}