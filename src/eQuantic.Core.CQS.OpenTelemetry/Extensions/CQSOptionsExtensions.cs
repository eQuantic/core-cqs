using eQuantic.Core.CQS.Abstractions.Options;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Abstractions.Telemetry;
using eQuantic.Core.CQS.OpenTelemetry.Decorators;
using eQuantic.Core.CQS.OpenTelemetry.Diagnostics;
using eQuantic.Core.CQS.OpenTelemetry.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace eQuantic.Core.CQS.OpenTelemetry.Extensions;

/// <summary>
/// CQSOptions extension methods for OpenTelemetry integration.
/// Follows the same fluent API pattern as other providers.
/// </summary>
public static class CQSOptionsExtensions
{
    /// <summary>
    /// Configures OpenTelemetry tracing and metrics for CQS operations.
    /// </summary>
    /// <param name="options">The CQS options</param>
    /// <param name="configure">OpenTelemetry configuration action</param>
    /// <returns>The CQS options for chaining</returns>
    public static CQSOptions UseOpenTelemetry(
        this CQSOptions options,
        Action<OpenTelemetryOptions>? configure = null)
    {
        var otelOptions = new OpenTelemetryOptions();
        configure?.Invoke(otelOptions);

        // Register options as singleton
        options.Services.AddSingleton(otelOptions);
        
        // Register telemetry adapter
        options.Services.AddSingleton<ICqsTelemetry, OpenTelemetryAdapter>();
        
        // Register outbox publisher decorator
        options.Services.Decorate<IOutboxPublisher, TracingOutboxPublisherDecorator>();
        
        return options;
    }
    
    /// <summary>
    /// Configures OpenTelemetry with saga repository tracing.
    /// </summary>
    /// <typeparam name="TSagaData">The saga data type</typeparam>
    /// <param name="options">The CQS options</param>
    /// <param name="configure">OpenTelemetry configuration action</param>
    /// <returns>The CQS options for chaining</returns>
    public static CQSOptions UseOpenTelemetry<TSagaData>(
        this CQSOptions options,
        Action<OpenTelemetryOptions>? configure = null)
        where TSagaData : ISagaData
    {
        // First configure base OpenTelemetry
        options.UseOpenTelemetry(configure);
        
        // Then decorate the saga repository
        options.Services.Decorate<ISagaRepository<TSagaData>, TracingSagaRepositoryDecorator<TSagaData>>();
        
        return options;
    }
}

/// <summary>
/// Service collection extensions for decorator pattern support.
/// </summary>
internal static class ServiceCollectionDecoratorExtensions
{
    /// <summary>
    /// Decorates a service with another implementation.
    /// Implements OCP - allows extending services without modifying them.
    /// </summary>
    public static IServiceCollection Decorate<TService, TDecorator>(this IServiceCollection services)
        where TService : class
        where TDecorator : class, TService
    {
        var descriptor = services.LastOrDefault(d => d.ServiceType == typeof(TService));
        if (descriptor == null) return services;

        var decoratorFactory = ActivatorUtilities.CreateFactory(typeof(TDecorator), new[] { typeof(TService) });

        services.Replace(ServiceDescriptor.Describe(
            typeof(TService),
            sp =>
            {
                var inner = CreateInstance(sp, descriptor);
                return (TService)decoratorFactory(sp, new object[] { inner });
            },
            descriptor.Lifetime));

        return services;
    }

    private static object CreateInstance(IServiceProvider sp, ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance != null)
            return descriptor.ImplementationInstance;

        if (descriptor.ImplementationFactory != null)
            return descriptor.ImplementationFactory(sp);

        return ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType!);
    }
}
