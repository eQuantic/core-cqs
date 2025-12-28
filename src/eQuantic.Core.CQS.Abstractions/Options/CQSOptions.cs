using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Abstractions.Options;

/// <summary>
/// Configuration options for CQS with fluent API
/// </summary>
public class CQSOptions
{
    private readonly List<Assembly> _assemblies = new();

    /// <summary>
    /// The service collection for registering services
    /// </summary>
    public IServiceCollection Services { get; set; } = null!;

    /// <summary>
    /// Whether to use the built-in pre-processor behavior
    /// </summary>
    public bool UsePreProcessor { get; set; } = false;

    /// <summary>
    /// Whether to use the built-in post-processor behavior
    /// </summary>
    public bool UsePostProcessor { get; set; } = false;

    /// <summary>
    /// The assemblies to scan for handlers
    /// </summary>
    public IReadOnlyList<Assembly> Assemblies => _assemblies;

    // ============================================================
    // ASSEMBLY CONFIGURATION (Fluent)
    // ============================================================

    /// <summary>
    /// Scans the specified assembly for handlers
    /// </summary>
    public CQSOptions FromAssembly(Assembly assembly)
    {
        _assemblies.Add(assembly);
        return this;
    }

    /// <summary>
    /// Scans multiple assemblies for handlers
    /// </summary>
    public CQSOptions FromAssemblies(params Assembly[] assemblies)
    {
        _assemblies.AddRange(assemblies);
        return this;
    }

    /// <summary>
    /// Scans multiple assemblies for handlers
    /// </summary>
    public CQSOptions FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        _assemblies.AddRange(assemblies);
        return this;
    }

    /// <summary>
    /// Scans the assembly containing the specified type for handlers
    /// </summary>
    public CQSOptions FromAssemblyContaining<T>()
    {
        _assemblies.Add(typeof(T).Assembly);
        return this;
    }

    /// <summary>
    /// Scans the assembly containing the specified type for handlers
    /// </summary>
    public CQSOptions FromAssemblyContaining(Type type)
    {
        _assemblies.Add(type.Assembly);
        return this;
    }

    /// <summary>
    /// Scans the calling assembly for handlers
    /// </summary>
    public CQSOptions FromCallingAssembly()
    {
        _assemblies.Add(Assembly.GetCallingAssembly());
        return this;
    }

    /// <summary>
    /// Scans the entry assembly for handlers
    /// </summary>
    public CQSOptions FromEntryAssembly()
    {
        var entry = Assembly.GetEntryAssembly();
        if (entry != null)
            _assemblies.Add(entry);
        return this;
    }

    // ============================================================
    // EXTERNAL EVENT INTEGRATION (Fluent)
    // ============================================================

    /// <summary>
    /// Configures external event publishing using the specified publisher.
    /// Events will be published to external message brokers (Azure, AWS, RabbitMQ, Kafka, etc.)
    /// </summary>
    /// <typeparam name="TPublisher">The external publisher type.</typeparam>
    public CQSOptions UseExternalPublisher<TPublisher>()
        where TPublisher : class, eQuantic.Core.Eventing.IExternalEventPublisher
    {
        Services.AddSingleton<eQuantic.Core.Eventing.IExternalEventPublisher, TPublisher>();
        return this;
    }

    /// <summary>
    /// Configures external event subscription using the specified subscriber.
    /// Events from external message brokers will be dispatched to local notification handlers.
    /// </summary>
    /// <typeparam name="TSubscriber">The external subscriber type.</typeparam>
    public CQSOptions UseExternalSubscriber<TSubscriber>()
        where TSubscriber : class, eQuantic.Core.Eventing.IExternalEventSubscriber
    {
        Services.AddSingleton<eQuantic.Core.Eventing.IExternalEventSubscriber, TSubscriber>();
        return this;
    }
}