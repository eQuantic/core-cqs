using System.Reflection;
using eQuantic.Core.CQS.Abstractions;
using eQuantic.Core.CQS.Abstractions.Handlers;
using eQuantic.Core.CQS.Abstractions.Notifications;
using eQuantic.Core.CQS.Abstractions.Streaming;
using eQuantic.Core.CQS.Handlers;
using eQuantic.Core.CQS.Notifications;
using eQuantic.Core.CQS.Pipeline;
using eQuantic.Core.CQS.Streaming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace eQuantic.Core.CQS.Extensions;

/// <summary>
/// Extension methods for registering CQS services with Microsoft.Extensions.DependencyInjection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CQS services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional configuration action</param>
    /// <param name="assemblies">Assemblies to scan for handlers. If empty, scans the calling assembly.</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCQS(
        this IServiceCollection services,
        Action<CQSOptions>? configure = null,
        params Assembly[] assemblies)
    {
        var options = new CQSOptions();
        configure?.Invoke(options);

        services.TryAddSingleton(options);
        services.TryAddScoped<IMediator, Mediator>();
        services.TryAddScoped<INotificationPublisher, NotificationPublisher>();

        var assembliesToScan = assemblies.Length > 0 
            ? assemblies 
            : new[] { Assembly.GetCallingAssembly() };

        RegisterHandlers(services, assembliesToScan);
        RegisterNotificationHandlers(services, assembliesToScan);
        RegisterStreamHandlers(services, assembliesToScan);
        RegisterPipelineBehaviors(services, assembliesToScan, options);

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        var handlerTypes = new[]
        {
            typeof(ICommandHandler<>),
            typeof(ICommandHandler<,>),
            typeof(IQueryHandler<,>),
            typeof(IPagedQueryHandler<,>)
        };

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
            {
                foreach (var handlerType in handlerTypes)
                {
                    var interfaces = type.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerType);

                    foreach (var @interface in interfaces)
                    {
                        services.TryAddTransient(@interface, type);
                    }
                }
            }
        }
    }

    private static void RegisterNotificationHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>));

                foreach (var @interface in interfaces)
                {
                    // Notifications can have multiple handlers, so use AddTransient instead of TryAddTransient
                    services.AddTransient(@interface, type);
                }
            }
        }
    }

    private static void RegisterStreamHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStreamQueryHandler<,>));

                foreach (var @interface in interfaces)
                {
                    services.TryAddTransient(@interface, type);
                }
            }
        }
    }

    private static void RegisterPipelineBehaviors(
        IServiceCollection services, 
        Assembly[] assemblies, 
        CQSOptions options)
    {
        // Register built-in behaviors if enabled
        if (options.UsePreProcessor)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PreProcessorBehavior<,>));
        }

        if (options.UsePostProcessor)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PostProcessorBehavior<,>));
        }

        // Scan for custom pipeline behaviors
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
            {
                var pipelineInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && 
                               (i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<>) ||
                                i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)));

                foreach (var @interface in pipelineInterfaces)
                {
                    services.TryAddTransient(@interface, type);
                }
            }
        }
    }
}

/// <summary>
/// Configuration options for CQS
/// </summary>
public class CQSOptions
{
    /// <summary>
    /// Whether to use the built-in pre-processor behavior
    /// </summary>
    public bool UsePreProcessor { get; set; } = false;

    /// <summary>
    /// Whether to use the built-in post-processor behavior
    /// </summary>
    public bool UsePostProcessor { get; set; } = false;
}
