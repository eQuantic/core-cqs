namespace eQuantic.Core.CQS.Abstractions.Notifications;

/// <summary>
/// Handler for notifications
/// </summary>
/// <typeparam name="TNotification">The notification type</typeparam>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles the notification
    /// </summary>
    /// <param name="notification">The notification to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}
