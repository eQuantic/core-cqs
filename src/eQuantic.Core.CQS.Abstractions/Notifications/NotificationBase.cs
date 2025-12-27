using eQuantic.Core.Eventing;

namespace eQuantic.Core.CQS.Abstractions.Notifications;

/// <summary>
/// Base class for notifications that provides IEvent implementation.
/// </summary>
public abstract class NotificationBase : EventBase, INotification
{
}
