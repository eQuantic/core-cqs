using eQuantic.Core.CQS.Abstractions.Resilience;
using eQuantic.Core.CQS.Abstractions.Sagas;
using Polly;
using Polly.Timeout;

namespace eQuantic.Core.CQS.Polly.Policies;

/// <summary>
/// Saga timeout policy implementation using Polly.
/// </summary>
public class PollySagaTimeoutPolicy : ISagaTimeoutPolicy
{
    private readonly PollySagaOptions _options;
    private readonly IDeadLetterHandler? _deadLetterHandler;

    public PollySagaTimeoutPolicy(PollySagaOptions options, IDeadLetterHandler? deadLetterHandler = null)
    {
        _options = options;
        _deadLetterHandler = deadLetterHandler;
    }

    public TimeSpan GetTimeout(ISagaData saga)
    {
        if (saga is IResilientSagaData resilientSaga && resilientSaga.Timeout.HasValue)
        {
            return resilientSaga.Timeout.Value;
        }
        return _options.DefaultTimeout;
    }

    public async Task OnTimeoutAsync(ISagaData saga, CancellationToken cancellationToken = default)
    {
        saga.State = SagaState.Failed;
        
        if (_options.EnableDeadLetter && _deadLetterHandler != null)
        {
            await _deadLetterHandler.HandleAsync(saga, "Saga timeout (Polly)", cancellationToken);
        }
    }

    /// <summary>
    /// Creates a Polly timeout resilience pipeline for saga operations.
    /// </summary>
    public ResiliencePipeline CreateTimeoutPipeline(ISagaData saga)
    {
        var timeout = GetTimeout(saga);
        
        return new ResiliencePipelineBuilder()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = timeout,
                OnTimeout = args =>
                {
                    // Log or handle timeout
                    return default;
                }
            })
            .Build();
    }
}

/// <summary>
/// Configuration options for Polly saga policies.
/// </summary>
public class PollySagaOptions
{
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(30);
    public bool EnableDeadLetter { get; set; } = true;
}
