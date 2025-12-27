namespace eQuantic.Core.CQS.Abstractions.Telemetry;

/// <summary>
/// Abstraction for CQS telemetry operations.
/// Implements ISP - focused interface for telemetry only.
/// </summary>
public interface ICqsTelemetry
{
    /// <summary>
    /// Starts a new telemetry activity/span.
    /// </summary>
    /// <param name="operationType">Type of operation (Command, Query, Saga, Outbox)</param>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="tags">Optional tags to add to the activity</param>
    /// <returns>Disposable scope that ends the activity when disposed</returns>
    IDisposable StartActivity(string operationType, string operationName, IDictionary<string, object?>? tags = null);
    
    /// <summary>
    /// Records an exception on the current activity.
    /// </summary>
    void RecordException(Exception exception);
    
    /// <summary>
    /// Sets a tag on the current activity.
    /// </summary>
    void SetTag(string key, object? value);
    
    /// <summary>
    /// Records a metric value.
    /// </summary>
    void RecordMetric(string name, double value, IDictionary<string, object?>? tags = null);
}

/// <summary>
/// Null object implementation of ICqsTelemetry for when telemetry is disabled.
/// </summary>
public sealed class NullCqsTelemetry : ICqsTelemetry
{
    public static readonly NullCqsTelemetry Instance = new();
    
    private NullCqsTelemetry() { }
    
    public IDisposable StartActivity(string operationType, string operationName, IDictionary<string, object?>? tags = null) 
        => NullDisposable.Instance;
    
    public void RecordException(Exception exception) { }
    public void SetTag(string key, object? value) { }
    public void RecordMetric(string name, double value, IDictionary<string, object?>? tags = null) { }
    
    private sealed class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();
        public void Dispose() { }
    }
}
