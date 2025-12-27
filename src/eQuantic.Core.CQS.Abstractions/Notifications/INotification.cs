using eQuantic.Core.Eventing;

namespace eQuantic.Core.CQS.Abstractions.Notifications;

/// <summary>
/// Marker interface for notifications (events that can have multiple handlers).
/// Extends IEvent from Core.Eventing for ecosystem integration.
/// </summary>
public interface INotification : IEvent
{
}
