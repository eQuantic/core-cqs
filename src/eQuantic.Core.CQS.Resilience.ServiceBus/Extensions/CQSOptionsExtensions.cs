using Azure.Messaging.ServiceBus;
using eQuantic.Core.CQS.Abstractions.Options;
using eQuantic.Core.CQS.Abstractions.Resilience;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Resilience.ServiceBus.Extensions;

/// <summary>
/// Extension methods for configuring Service Bus dead letter handler.
/// </summary>
public static class CQSOptionsExtensions
{
    /// <summary>
    /// Configures Azure Service Bus as the dead letter storage.
    /// </summary>
    public static CQSOptions UseServiceBusDeadLetter(
        this CQSOptions options,
        Action<ServiceBusDeadLetterOptions> configure)
    {
        var sbOptions = new ServiceBusDeadLetterOptions();
        configure(sbOptions);
        
        options.Services.AddSingleton(sbOptions);
        options.Services.AddSingleton(_ => new ServiceBusClient(sbOptions.ConnectionString));
        options.Services.AddSingleton<IDeadLetterHandler, ServiceBusDeadLetterHandler>();
        
        return options;
    }

    /// <summary>
    /// Configures Service Bus dead letter with an existing client.
    /// </summary>
    public static CQSOptions UseServiceBusDeadLetter(
        this CQSOptions options,
        ServiceBusClient client,
        Action<ServiceBusDeadLetterOptions>? configure = null)
    {
        var sbOptions = new ServiceBusDeadLetterOptions();
        configure?.Invoke(sbOptions);
        
        options.Services.AddSingleton(sbOptions);
        options.Services.AddSingleton(client);
        options.Services.AddSingleton<IDeadLetterHandler, ServiceBusDeadLetterHandler>();
        
        return options;
    }
}
