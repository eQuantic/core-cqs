using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Azure;

/// <summary>
/// Extension methods for registering Azure Service Bus services
/// </summary>
public static class ServiceBusServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Service Bus Outbox Publisher to the service collection
    /// </summary>
    public static IServiceCollection AddCQSAzureServiceBus(
        this IServiceCollection services,
        Action<ServiceBusOptions> configure)
    {
        var options = new ServiceBusOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton(_ => new ServiceBusClient(options.ConnectionString));
        services.AddSingleton<IOutboxPublisher, ServiceBusOutboxPublisher>();

        return services;
    }

    /// <summary>
    /// Adds Azure Service Bus Outbox Publisher with existing ServiceBusClient
    /// </summary>
    public static IServiceCollection AddCQSAzureServiceBus(
        this IServiceCollection services,
        ServiceBusClient client,
        Action<ServiceBusOptions> configure)
    {
        var options = new ServiceBusOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton(client);
        services.AddSingleton<IOutboxPublisher, ServiceBusOutboxPublisher>();

        return services;
    }
}