using eQuantic.Core.CQS.Abstractions.Options;
using eQuantic.Core.CQS.Abstractions.Resilience;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Resilience.Compensation;
using eQuantic.Core.CQS.Resilience.Monitoring;
using eQuantic.Core.CQS.Resilience.Options;
using eQuantic.Core.CQS.Resilience.Policies;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Resilience.Extensions;

/// <summary>
/// CQSOptions extension methods for resilience features.
/// </summary>
public static class CQSOptionsExtensions
{
    /// <summary>
    /// Configures resilience features for CQS.
    /// </summary>
    public static CQSOptions UseResilience(
        this CQSOptions options,
        Action<ResilienceOptions>? configure = null)
    {
        var resilienceOptions = new ResilienceOptions();
        configure?.Invoke(resilienceOptions);

        options.Services.AddSingleton(resilienceOptions);
        options.Services.AddSingleton<ISagaTimeoutPolicy, DefaultSagaTimeoutPolicy>();
        options.Services.AddSingleton<IDeadLetterHandler, LoggingDeadLetterHandler>();
        options.Services.AddSingleton<CompensationExecutor>();

        return options;
    }

    /// <summary>
    /// Configures resilience with saga timeout monitoring.
    /// </summary>
    public static CQSOptions UseResilience<TSagaData>(
        this CQSOptions options,
        Action<ResilienceOptions>? configure = null)
        where TSagaData : ISagaData
    {
        options.UseResilience(configure);
        
        // Register background service for timeout monitoring
        options.Services.AddHostedService<SagaTimeoutBackgroundService<TSagaData>>();

        return options;
    }

    /// <summary>
    /// Registers a compensation handler for a saga type.
    /// </summary>
    public static CQSOptions WithCompensation<TSaga, THandler>(this CQSOptions options)
        where TSaga : ISagaData
        where THandler : class, ICompensationHandler<TSaga>
    {
        options.Services.AddScoped<ICompensationHandler<TSaga>, THandler>();
        return options;
    }

    /// <summary>
    /// Registers a delegate-based compensation handler.
    /// </summary>
    public static CQSOptions WithCompensation<TSaga>(
        this CQSOptions options,
        Func<TSaga, Exception?, CancellationToken, Task> compensate)
        where TSaga : ISagaData
    {
        options.Services.AddSingleton<ICompensationHandler<TSaga>>(
            new DelegateCompensationHandler<TSaga>(compensate));
        return options;
    }

    /// <summary>
    /// Configures a custom dead letter handler.
    /// </summary>
    public static CQSOptions WithDeadLetterHandler<THandler>(this CQSOptions options)
        where THandler : class, IDeadLetterHandler
    {
        options.Services.AddSingleton<IDeadLetterHandler, THandler>();
        return options;
    }
}
