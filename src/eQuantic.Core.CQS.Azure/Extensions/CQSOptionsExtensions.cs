using Azure.Messaging.ServiceBus;
using eQuantic.Core.CQS.Abstractions.Options;
using eQuantic.Core.CQS.Azure.Options;
using eQuantic.Core.CQS.Azure.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Azure.Extensions;

/// <summary>
/// CQSOptions extension methods for Azure Service Bus
/// </summary>
public static class CQSOptionsExtensions
{
    /// <summary>
    /// Configures Azure Service Bus as the outbox publisher
    /// </summary>
    /// <param name="options">The CQS options</param>
    /// <param name="configure">Service Bus configuration</param>
    /// <returns>The CQS options for chaining</returns>
    public static CQSOptions UseAzureServiceBus(
        this CQSOptions options, 
        Action<ServiceBusOptions> configure)
    {
        var sbOptions = new ServiceBusOptions();
        configure(sbOptions);

        options.Services.AddSingleton(sbOptions);
        options.Services.AddSingleton(_ => new ServiceBusClient(sbOptions.ConnectionString));
        options.Services.AddSingleton<IOutboxPublisher, ServiceBusOutboxPublisher>();

        return options;
    }

    /// <summary>
    /// Configures Azure Service Bus with an existing ServiceBusClient
    /// </summary>
    public static CQSOptions UseAzureServiceBus(
        this CQSOptions options, 
        ServiceBusClient client,
        Action<ServiceBusOptions> configure)
    {
        var sbOptions = new ServiceBusOptions();
        configure(sbOptions);

        options.Services.AddSingleton(sbOptions);
        options.Services.AddSingleton(client);
        options.Services.AddSingleton<IOutboxPublisher, ServiceBusOutboxPublisher>();

        return options;
    }
}
