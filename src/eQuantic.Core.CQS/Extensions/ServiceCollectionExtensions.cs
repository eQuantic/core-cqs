using System.Reflection;
using eQuantic.Core.CQS.Abstractions;
using eQuantic.Core.CQS.Abstractions.Handlers;
using eQuantic.Core.CQS.Abstractions.Notifications;
using eQuantic.Core.CQS.Abstractions.Options;
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
    /// Adds CQS services to the service collection using fluent configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Configuration action with fluent API</param>
    /// <returns>The service collection for chaining</returns>
    /// <example>
    /// <code>
    /// services.AddCQS(options => options
    ///     .FromAssemblyContaining&lt;Program&gt;()
    ///     .UseRedis(redis => redis.ConnectionString = "...")
    ///     .UseAzureServiceBus(sb => sb.ConnectionString = "..."));
    /// </code>
    /// </example>
    public static IServiceCollection AddCQS(
        this IServiceCollection services,
        Action<CQSOptions> configure)
    {
        var options = new CQSOptions { Services = services };
        configure(options);

        services.TryAddSingleton(options);
        services.TryAddScoped<IMediator, Mediator>();
        
        // Register NotificationPublisher with factory to support optional IExternalEventPublisher
        services.TryAddScoped<INotificationPublisher>(sp =>
        {
            var externalPublisher = sp.GetService<eQuantic.Core.Eventing.IExternalEventPublisher>();
            return new NotificationPublisher(sp, externalPublisher);
        });

        // Register HostedService for external subscriber if registered
        var subscriberDescriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(eQuantic.Core.Eventing.IExternalEventSubscriber));
        if (subscriberDescriptor != null)
        {
            services.AddHostedService<CQSSubscriberHostedService>();
        }

        var assembliesToScan = options.Assemblies.Count > 0 
            ? options.Assemblies.ToArray() 
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
                    // Notifications can have multiple handlers
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
        if (options.UsePreProcessor)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PreProcessorBehavior<,>));
        }

        if (options.UsePostProcessor)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PostProcessorBehavior<,>));
        }

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
