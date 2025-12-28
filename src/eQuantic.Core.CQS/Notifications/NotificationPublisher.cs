using eQuantic.Core.CQS.Abstractions.Notifications;
using eQuantic.Core.Eventing;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Notifications;

/// <summary>
/// Default implementation of INotificationPublisher.
/// Optionally publishes notifications to external message brokers via IExternalEventPublisher.
/// </summary>
public class NotificationPublisher : INotificationPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IExternalEventPublisher? _externalPublisher;

    /// <summary>
    /// Creates a new NotificationPublisher with local dispatch only.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public NotificationPublisher(IServiceProvider serviceProvider)
        : this(serviceProvider, null)
    {
    }

    /// <summary>
    /// Creates a new NotificationPublisher with optional external publishing.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="externalPublisher">Optional external event publisher (Azure, AWS, RabbitMQ, etc.).</param>
    public NotificationPublisher(
        IServiceProvider serviceProvider,
        IExternalEventPublisher? externalPublisher)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _externalPublisher = externalPublisher;
    }

    /// <inheritdoc />
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        // Dispatch to local handlers
        var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();
        
        foreach (var handler in handlers)
        {
            await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
        }

        // Publish to external message broker (if configured)
        await PublishExternalAsync(notification, cancellationToken);
    }

    /// <inheritdoc />
    public async Task PublishParallel<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        // Dispatch to local handlers in parallel
        var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();
        var tasks = handlers.Select(h => h.Handle(notification, cancellationToken));
        await Task.WhenAll(tasks).ConfigureAwait(false);

        // Publish to external message broker (if configured)
        await PublishExternalAsync(notification, cancellationToken);
    }

    private async Task PublishExternalAsync<TNotification>(TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        if (_externalPublisher != null && notification is IEvent @event)
        {
            await _externalPublisher.PublishAsync(@event, cancellationToken);
        }
    }
}
