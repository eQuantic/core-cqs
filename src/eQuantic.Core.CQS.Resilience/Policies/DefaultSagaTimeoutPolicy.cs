using eQuantic.Core.CQS.Abstractions.Resilience;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Resilience.Options;

namespace eQuantic.Core.CQS.Resilience.Policies;

/// <summary>
/// Default implementation of ISagaTimeoutPolicy.
/// Uses ResilienceOptions to determine timeout values.
/// Implements DIP - depends on abstractions.
/// </summary>
public class DefaultSagaTimeoutPolicy : ISagaTimeoutPolicy
{
    private readonly ResilienceOptions _options;
    private readonly IDeadLetterHandler? _deadLetterHandler;

    public DefaultSagaTimeoutPolicy(ResilienceOptions options, IDeadLetterHandler? deadLetterHandler = null)
    {
        _options = options;
        _deadLetterHandler = deadLetterHandler;
    }

    /// <inheritdoc />
    public TimeSpan GetTimeout(ISagaData saga)
    {
        // If saga implements IResilientSagaData, use its timeout
        if (saga is IResilientSagaData resilientSaga && resilientSaga.Timeout.HasValue)
        {
            return resilientSaga.Timeout.Value;
        }
        
        return _options.DefaultSagaTimeout;
    }

    /// <inheritdoc />
    public async Task OnTimeoutAsync(ISagaData saga, CancellationToken cancellationToken = default)
    {
        // Mark saga as failed
        saga.State = SagaState.Failed;
        
        // If dead letter is enabled, send to dead letter queue
        if (_options.EnableDeadLetterQueue && _deadLetterHandler != null)
        {
            var reason = $"Saga timeout after {GetTimeout(saga).TotalMinutes} minutes";
            await _deadLetterHandler.HandleAsync(saga, reason, cancellationToken);
        }
    }
}
