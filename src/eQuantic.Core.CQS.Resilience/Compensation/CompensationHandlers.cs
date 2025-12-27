using eQuantic.Core.CQS.Abstractions.Resilience;
using eQuantic.Core.CQS.Abstractions.Sagas;
using Microsoft.Extensions.Logging;

namespace eQuantic.Core.CQS.Resilience.Compensation;

/// <summary>
/// Executes compensation logic for failed sagas.
/// Implements SRP - only handles compensation execution.
/// </summary>
public class CompensationExecutor
{
    private readonly ILogger<CompensationExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CompensationExecutor(ILogger<CompensationExecutor> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Executes compensation for a saga.
    /// </summary>
    public async Task ExecuteAsync<TSaga>(TSaga saga, Exception? exception, CancellationToken ct = default)
        where TSaga : ISagaData
    {
        var handlerType = typeof(ICompensationHandler<TSaga>);
        var handler = _serviceProvider.GetService(handlerType) as ICompensationHandler<TSaga>;
        
        if (handler == null)
        {
            _logger.LogWarning("No compensation handler found for saga type {SagaType}", typeof(TSaga).Name);
            return;
        }

        try
        {
            _logger.LogInformation("Executing compensation for saga {SagaId} of type {SagaType}", 
                saga.SagaId, typeof(TSaga).Name);
            
            await handler.CompensateAsync(saga, exception, ct);
            
            _logger.LogInformation("Compensation completed for saga {SagaId}", saga.SagaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Compensation failed for saga {SagaId}", saga.SagaId);
            throw;
        }
    }
}

/// <summary>
/// Compensation handler that uses a delegate.
/// Implements OCP - new compensation logic without new classes.
/// </summary>
public class DelegateCompensationHandler<TSaga> : ICompensationHandler<TSaga>
    where TSaga : ISagaData
{
    private readonly Func<TSaga, Exception?, CancellationToken, Task> _compensate;

    public DelegateCompensationHandler(Func<TSaga, Exception?, CancellationToken, Task> compensate)
    {
        _compensate = compensate;
    }

    public Task CompensateAsync(TSaga saga, Exception? exception, CancellationToken cancellationToken = default)
    {
        return _compensate(saga, exception, cancellationToken);
    }
}

/// <summary>
/// No-op compensation handler for sagas that don't need compensation.
/// Implements Null Object pattern.
/// </summary>
public class NoOpCompensationHandler<TSaga> : ICompensationHandler<TSaga>
    where TSaga : ISagaData
{
    public Task CompensateAsync(TSaga saga, Exception? exception, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
