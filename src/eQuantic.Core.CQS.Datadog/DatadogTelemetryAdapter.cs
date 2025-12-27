using Datadog.Trace;
using eQuantic.Core.CQS.Abstractions.Telemetry;

namespace eQuantic.Core.CQS.Datadog;

/// <summary>
/// Datadog implementation of ICqsTelemetry.
/// Provides distributed tracing using Datadog Trace SDK.
/// </summary>
public sealed class DatadogTelemetryAdapter : ICqsTelemetry
{
    private readonly AsyncLocal<ISpan?> _currentSpan = new();
    private readonly string _serviceName;

    public DatadogTelemetryAdapter(string? serviceName = null)
    {
        _serviceName = serviceName ?? "cqs-service";
    }

    public IDisposable StartActivity(string operationType, string operationName, IDictionary<string, object?>? tags = null)
    {
        var scope = Tracer.Instance.StartActive($"{operationType}.{operationName}");
        var span = scope.Span;
        
        span.ServiceName = _serviceName;
        span.Type = "cqs";
        span.SetTag("cqs.operation.type", operationType);
        span.SetTag("cqs.operation.name", operationName);
        
        if (tags != null)
        {
            foreach (var tag in tags)
            {
                if (tag.Value != null)
                {
                    span.SetTag(tag.Key, tag.Value.ToString());
                }
            }
        }
        
        _currentSpan.Value = span;
        return new SpanScope(this, scope);
    }

    public void RecordException(Exception exception)
    {
        var span = _currentSpan.Value;
        if (span != null)
        {
            span.Error = true;
            span.SetTag("error.msg", exception.Message);
            span.SetTag("error.type", exception.GetType().FullName);
            span.SetTag("error.stack", exception.StackTrace);
        }
    }

    public void SetTag(string key, object? value)
    {
        var span = _currentSpan.Value;
        if (span != null && value != null)
        {
            span.SetTag(key, value.ToString());
        }
    }

    public void RecordMetric(string name, double value, IDictionary<string, object?>? tags = null)
    {
        var span = _currentSpan.Value;
        if (span != null)
        {
            // Datadog metrics are recorded as tags on the span
            span.SetTag($"metric.{name}", value.ToString("F2"));
        }
        
        // Note: For true metrics, use DogStatsD client directly
    }

    private sealed class SpanScope : IDisposable
    {
        private readonly DatadogTelemetryAdapter _adapter;
        private readonly IScope _scope;
        private bool _disposed;

        public SpanScope(DatadogTelemetryAdapter adapter, IScope scope)
        {
            _adapter = adapter;
            _scope = scope;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            _scope.Dispose();
            _adapter._currentSpan.Value = null;
        }
    }
}
