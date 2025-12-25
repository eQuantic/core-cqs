namespace eQuantic.Core.CQS.Resilience;

/// <summary>
/// Attribute to mark handlers that should use retry policy
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class RetryAttribute : Attribute
{
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Initial delay in milliseconds
    /// </summary>
    public int InitialDelayMs { get; set; } = 100;

    /// <summary>
    /// Maximum delay in milliseconds
    /// </summary>
    public int MaxDelayMs { get; set; } = 30000;

    /// <summary>
    /// Backoff multiplier
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Whether to add jitter
    /// </summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>
    /// Exception types to retry on
    /// </summary>
    public Type[]? RetryOn { get; set; }
}
