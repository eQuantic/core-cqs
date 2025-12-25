namespace eQuantic.Core.CQS.Abstractions.Notifications;

/// <summary>
/// Strategy for how notifications are published
/// </summary>
public enum NotificationPublishStrategy
{
    /// <summary>
    /// Execute handlers sequentially
    /// </summary>
    Sequential,
    
    /// <summary>
    /// Execute handlers in parallel
    /// </summary>
    Parallel,
    
    /// <summary>
    /// Execute handlers in parallel and wait for all to complete
    /// </summary>
    ParallelWhenAll,
    
    /// <summary>
    /// Execute handlers in parallel and complete when any finishes
    /// </summary>
    ParallelWhenAny
}