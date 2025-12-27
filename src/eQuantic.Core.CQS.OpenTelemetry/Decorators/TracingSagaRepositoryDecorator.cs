using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Abstractions.Telemetry;
using eQuantic.Core.CQS.OpenTelemetry.Diagnostics;
using eQuantic.Core.CQS.OpenTelemetry.Options;

namespace eQuantic.Core.CQS.OpenTelemetry.Decorators;

/// <summary>
/// Decorator that adds tracing to saga repository operations.
/// Implements OCP - extends functionality without modifying original repository.
/// Implements DIP - depends on ICqsTelemetry abstraction.
/// </summary>
public sealed class TracingSagaRepositoryDecorator<TData> : ISagaRepository<TData>
    where TData : ISagaData
{
    private readonly ISagaRepository<TData> _inner;
    private readonly ICqsTelemetry _telemetry;
    private readonly OpenTelemetryOptions _options;
    private readonly string _sagaType;
    
    public TracingSagaRepositoryDecorator(
        ISagaRepository<TData> inner,
        ICqsTelemetry telemetry,
        OpenTelemetryOptions options)
    {
        _inner = inner;
        _telemetry = telemetry;
        _options = options;
        _sagaType = typeof(TData).Name;
    }
    
    public async Task Save(TData data, CancellationToken cancellationToken = default)
    {
        if (!_options.TraceSagas)
        {
            await _inner.Save(data, cancellationToken);
            return;
        }
        
        using var _ = _telemetry.StartActivity(
            CqsActivitySource.Operations.Saga,
            "Save",
            new Dictionary<string, object?>
            {
                [CqsActivitySource.Tags.SagaId] = data.SagaId,
                [CqsActivitySource.Tags.SagaType] = _sagaType,
                [CqsActivitySource.Tags.SagaState] = data.State.ToString()
            });
        
        try
        {
            await _inner.Save(data, cancellationToken);
        }
        catch (Exception ex)
        {
            _telemetry.RecordException(ex);
            throw;
        }
    }
    
    public async Task<TData?> Load(Guid sagaId, CancellationToken cancellationToken = default)
    {
        if (!_options.TraceSagas)
        {
            return await _inner.Load(sagaId, cancellationToken);
        }
        
        using var _ = _telemetry.StartActivity(
            CqsActivitySource.Operations.Saga,
            "Load",
            new Dictionary<string, object?>
            {
                [CqsActivitySource.Tags.SagaId] = sagaId,
                [CqsActivitySource.Tags.SagaType] = _sagaType
            });
        
        try
        {
            var result = await _inner.Load(sagaId, cancellationToken);
            _telemetry.SetTag("saga.found", result != null);
            return result;
        }
        catch (Exception ex)
        {
            _telemetry.RecordException(ex);
            throw;
        }
    }
    
    public async Task<IReadOnlyList<TData>> Find(Func<TData, bool> predicate, CancellationToken cancellationToken = default)
    {
        if (!_options.TraceSagas)
        {
            return await _inner.Find(predicate, cancellationToken);
        }
        
        using var _ = _telemetry.StartActivity(
            CqsActivitySource.Operations.Saga,
            "Find",
            new Dictionary<string, object?>
            {
                [CqsActivitySource.Tags.SagaType] = _sagaType
            });
        
        try
        {
            var result = await _inner.Find(predicate, cancellationToken);
            _telemetry.SetTag("saga.count", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _telemetry.RecordException(ex);
            throw;
        }
    }
    
    public async Task<IReadOnlyList<TData>> GetIncomplete(CancellationToken cancellationToken = default)
    {
        if (!_options.TraceSagas)
        {
            return await _inner.GetIncomplete(cancellationToken);
        }
        
        using var _ = _telemetry.StartActivity(
            CqsActivitySource.Operations.Saga,
            "GetIncomplete",
            new Dictionary<string, object?>
            {
                [CqsActivitySource.Tags.SagaType] = _sagaType
            });
        
        try
        {
            var result = await _inner.GetIncomplete(cancellationToken);
            _telemetry.SetTag("saga.incomplete_count", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _telemetry.RecordException(ex);
            throw;
        }
    }
    
    public async Task Delete(Guid sagaId, CancellationToken cancellationToken = default)
    {
        if (!_options.TraceSagas)
        {
            await _inner.Delete(sagaId, cancellationToken);
            return;
        }
        
        using var _ = _telemetry.StartActivity(
            CqsActivitySource.Operations.Saga,
            "Delete",
            new Dictionary<string, object?>
            {
                [CqsActivitySource.Tags.SagaId] = sagaId,
                [CqsActivitySource.Tags.SagaType] = _sagaType
            });
        
        try
        {
            await _inner.Delete(sagaId, cancellationToken);
        }
        catch (Exception ex)
        {
            _telemetry.RecordException(ex);
            throw;
        }
    }
}
