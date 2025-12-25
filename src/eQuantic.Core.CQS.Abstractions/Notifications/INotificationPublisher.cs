namespace eQuantic.Core.CQS.Abstractions.Notifications;

/// <summary>
/// Publisher for notifications
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    /// Publishes a notification to all registered handlers
    /// </summary>
    /// <typeparam name="TNotification">The notification type</typeparam>
    /// <param name="notification">The notification to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    /// Publishes a notification to all handlers, executing them in parallel
    /// </summary>
    Task PublishParallel<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}