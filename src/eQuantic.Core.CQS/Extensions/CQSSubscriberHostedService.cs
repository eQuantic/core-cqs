using eQuantic.Core.Eventing;
using Microsoft.Extensions.Hosting;

namespace eQuantic.Core.CQS.Extensions;

/// <summary>
/// Hosted service that manages the CQS external event subscriber lifecycle.
/// </summary>
public class CQSSubscriberHostedService : IHostedService
{
    private readonly IExternalEventSubscriber _subscriber;

    public CQSSubscriberHostedService(IExternalEventSubscriber subscriber)
    {
        _subscriber = subscriber;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _subscriber.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _subscriber.StopAsync(cancellationToken);
    }
}
