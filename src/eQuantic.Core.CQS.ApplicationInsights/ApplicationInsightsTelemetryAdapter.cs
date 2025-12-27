using eQuantic.Core.CQS.Abstractions.Telemetry;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace eQuantic.Core.CQS.ApplicationInsights;

/// <summary>
/// Application Insights implementation of ICqsTelemetry.
/// Provides distributed tracing using Application Insights SDK.
/// </summary>
public sealed class ApplicationInsightsTelemetryAdapter : ICqsTelemetry
{
    private readonly TelemetryClient _telemetryClient;
    private readonly AsyncLocal<IOperationHolder<DependencyTelemetry>?> _currentOperation = new();

    public ApplicationInsightsTelemetryAdapter(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public ApplicationInsightsTelemetryAdapter(string connectionString)
    {
        var config = new TelemetryConfiguration
        {
            ConnectionString = connectionString
        };
        _telemetryClient = new TelemetryClient(config);
    }

    public IDisposable StartActivity(string operationType, string operationName, IDictionary<string, object?>? tags = null)
    {
        var telemetryName = $"{operationType}.{operationName}";
        var operation = _telemetryClient.StartOperation<DependencyTelemetry>(telemetryName);
        
        operation.Telemetry.Type = "CQS";
        operation.Telemetry.Data = operationType;
        
        if (tags != null)
        {
            foreach (var tag in tags)
            {
                if (tag.Value != null)
                {
                    operation.Telemetry.Properties[tag.Key] = tag.Value.ToString();
                }
            }
        }
        
        _currentOperation.Value = operation;
        return new OperationScope(this, operation);
    }

    public void RecordException(Exception exception)
    {
        var operation = _currentOperation.Value;
        if (operation != null)
        {
            operation.Telemetry.Success = false;
            operation.Telemetry.ResultCode = exception.GetType().Name;
        }
        
        _telemetryClient.TrackException(exception);
    }

    public void SetTag(string key, object? value)
    {
        var operation = _currentOperation.Value;
        if (operation != null && value != null)
        {
            operation.Telemetry.Properties[key] = value.ToString();
        }
    }

    public void RecordMetric(string name, double value, IDictionary<string, object?>? tags = null)
    {
        var metric = new MetricTelemetry(name, value);
        
        if (tags != null)
        {
            foreach (var tag in tags)
            {
                if (tag.Value != null)
                {
                    metric.Properties[tag.Key] = tag.Value.ToString();
                }
            }
        }
        
        _telemetryClient.TrackMetric(metric);
    }

    private sealed class OperationScope : IDisposable
    {
        private readonly ApplicationInsightsTelemetryAdapter _adapter;
        private readonly IOperationHolder<DependencyTelemetry> _operation;
        private bool _disposed;

        public OperationScope(ApplicationInsightsTelemetryAdapter adapter, IOperationHolder<DependencyTelemetry> operation)
        {
            _adapter = adapter;
            _operation = operation;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            _operation.Dispose();
            _adapter._currentOperation.Value = null;
        }
    }
}
