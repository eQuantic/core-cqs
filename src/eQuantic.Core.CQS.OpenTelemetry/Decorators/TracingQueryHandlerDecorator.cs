using eQuantic.Core.CQS.Abstractions.Handlers;
using eQuantic.Core.CQS.Abstractions.Queries;
using eQuantic.Core.CQS.Abstractions.Telemetry;
using eQuantic.Core.CQS.OpenTelemetry.Diagnostics;
using eQuantic.Core.CQS.OpenTelemetry.Options;

namespace eQuantic.Core.CQS.OpenTelemetry.Decorators;

/// <summary>
/// Decorator that adds tracing to query handlers.
/// Implements OCP - extends functionality without modifying original handler.
/// Implements DIP - depends on ICqsTelemetry abstraction.
/// </summary>
public sealed class TracingQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
    where TResult : class
{
    private readonly IQueryHandler<TQuery, TResult> _inner;
    private readonly ICqsTelemetry _telemetry;
    private readonly OpenTelemetryOptions _options;
    
    public TracingQueryHandlerDecorator(
        IQueryHandler<TQuery, TResult> inner,
        ICqsTelemetry telemetry,
        OpenTelemetryOptions options)
    {
        _inner = inner;
        _telemetry = telemetry;
        _options = options;
    }
    
    public async Task<TResult> Execute(TQuery query, CancellationToken cancellationToken)
    {
        if (!_options.TraceQueries)
        {
            return await _inner.Execute(query, cancellationToken);
        }
        
        var queryType = typeof(TQuery).Name;
        
        using var _ = _telemetry.StartActivity(
            CqsActivitySource.Operations.Query,
            queryType,
            new Dictionary<string, object?>
            {
                [CqsActivitySource.Tags.QueryType] = queryType
            });
        
        try
        {
            return await _inner.Execute(query, cancellationToken);
        }
        catch (Exception ex)
        {
            _telemetry.RecordException(ex);
            throw;
        }
    }
}
