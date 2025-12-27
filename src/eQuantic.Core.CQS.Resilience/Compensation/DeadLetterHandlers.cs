using eQuantic.Core.CQS.Abstractions.Resilience;
using eQuantic.Core.CQS.Abstractions.Sagas;
using Microsoft.Extensions.Logging;

namespace eQuantic.Core.CQS.Resilience.Compensation;

/// <summary>
/// Dead letter handler that logs to the configured logger.
/// Default implementation when no queue is configured.
/// </summary>
public class LoggingDeadLetterHandler : IDeadLetterHandler
{
    private readonly ILogger<LoggingDeadLetterHandler> _logger;

    public LoggingDeadLetterHandler(ILogger<LoggingDeadLetterHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(ISagaData saga, string reason, CancellationToken cancellationToken = default)
    {
        _logger.LogError(
            "Saga {SagaId} sent to dead letter. Reason: {Reason}. State: {State}. Started: {StartedAt}",
            saga.SagaId,
            reason,
            saga.State,
            saga.StartedAt);

        return Task.CompletedTask;
    }
}

/// <summary>
/// No-op dead letter handler.
/// </summary>
public class NoOpDeadLetterHandler : IDeadLetterHandler
{
    public Task HandleAsync(ISagaData saga, string reason, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
