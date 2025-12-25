using System.Collections.Concurrent;
using eQuantic.Core.CQS.Abstractions.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Notifications;

/// <summary>
/// Default implementation of INotificationPublisher
/// </summary>
public class NotificationPublisher : INotificationPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Type, object> Handlers = new();

    /// <summary>
    /// Creates a new NotificationPublisher
    /// </summary>
    public NotificationPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();
        
        foreach (var handler in handlers)
        {
            await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task PublishParallel<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();
        
        var tasks = handlers.Select(h => h.Handle(notification, cancellationToken));
        
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
