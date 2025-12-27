using eQuantic.Core.CQS.Abstractions.Options;
using eQuantic.Core.CQS.Abstractions.Resilience;
using eQuantic.Core.CQS.Polly.Behaviors;
using eQuantic.Core.CQS.Polly.Policies;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Polly.Extensions;

/// <summary>
/// Extension methods for configuring Polly resilience.
/// </summary>
public static class CQSOptionsExtensions
{
    /// <summary>
    /// Configures Polly as the resilience provider.
    /// </summary>
    public static CQSOptions UsePolly(this CQSOptions options, Action<PollyOptions>? configure = null)
    {
        var pollyOptions = new PollyOptions();
        configure?.Invoke(pollyOptions);
        
        options.Services.AddSingleton(pollyOptions);
        options.Services.AddSingleton(typeof(Abstractions.IPipelineBehavior<,>), typeof(PollyRetryBehavior<,>));
        
        return options;
    }

    /// <summary>
    /// Configures Polly saga timeout policy.
    /// </summary>
    public static CQSOptions UsePollyTimeout(this CQSOptions options, Action<PollySagaOptions>? configure = null)
    {
        var sagaOptions = new PollySagaOptions();
        configure?.Invoke(sagaOptions);
        
        options.Services.AddSingleton(sagaOptions);
        options.Services.AddSingleton<ISagaTimeoutPolicy, PollySagaTimeoutPolicy>();
        
        return options;
    }
}
