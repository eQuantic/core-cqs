namespace eQuantic.Core.CQS.Resilience.Options;

/// <summary>
/// Configuration options for resilience features.
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// Default timeout for sagas.
    /// </summary>
    public TimeSpan DefaultSagaTimeout { get; set; } = TimeSpan.FromMinutes(30);
    
    /// <summary>
    /// Interval for checking saga timeouts.
    /// </summary>
    public TimeSpan TimeoutCheckInterval { get; set; } = TimeSpan.FromMinutes(1);
    
    /// <summary>
    /// Maximum number of retry attempts for failed sagas.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Enable automatic compensation on timeout.
    /// </summary>
    public bool EnableCompensationOnTimeout { get; set; } = true;
    
    /// <summary>
    /// Enable dead letter queue for unrecoverable sagas.
    /// </summary>
    public bool EnableDeadLetterQueue { get; set; } = true;
    
    /// <summary>
    /// Time after which a saga is considered stale and eligible for timeout.
    /// </summary>
    public TimeSpan StaleThreshold { get; set; } = TimeSpan.FromHours(1);
}
