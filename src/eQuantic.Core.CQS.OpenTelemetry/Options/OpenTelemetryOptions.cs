namespace eQuantic.Core.CQS.OpenTelemetry.Options;

/// <summary>
/// Configuration options for OpenTelemetry integration.
/// Implements fluent configuration pattern.
/// </summary>
public class OpenTelemetryOptions
{
    /// <summary>
    /// Gets or sets the service name for telemetry.
    /// </summary>
    public string ServiceName { get; set; } = "CqsService";
    
    /// <summary>
    /// Gets or sets the service version.
    /// </summary>
    public string? ServiceVersion { get; set; }
    
    /// <summary>
    /// Gets or sets whether to trace command execution.
    /// </summary>
    public bool TraceCommands { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to trace query execution.
    /// </summary>
    public bool TraceQueries { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to trace saga operations.
    /// </summary>
    public bool TraceSagas { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to trace outbox operations.
    /// </summary>
    public bool TraceOutbox { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to record exceptions in traces.
    /// </summary>
    public bool RecordExceptions { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to collect metrics.
    /// </summary>
    public bool CollectMetrics { get; set; } = true;
    
    /// <summary>
    /// Gets or sets custom tags to add to all activities.
    /// </summary>
    public IDictionary<string, object> GlobalTags { get; set; } = new Dictionary<string, object>();
}
