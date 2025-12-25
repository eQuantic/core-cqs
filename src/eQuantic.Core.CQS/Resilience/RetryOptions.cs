namespace eQuantic.Core.CQS.Resilience;

/// <summary>
/// Configuration options for retry policies
/// </summary>
public record RetryOptions
{
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Initial delay between retries (doubles with each attempt for exponential backoff)
    /// </summary>
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Maximum delay between retries
    /// </summary>
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Multiplier for exponential backoff
    /// </summary>
    public double BackoffMultiplier { get; init; } = 2.0;

    /// <summary>
    /// Whether to add jitter to delay to prevent thundering herd
    /// </summary>
    public bool UseJitter { get; init; } = true;

    /// <summary>
    /// Exception types that should trigger a retry (empty = retry all)
    /// </summary>
    public Type[] RetryableExceptions { get; init; } = Array.Empty<Type>();

    /// <summary>
    /// Predicate to determine if an exception should trigger a retry
    /// </summary>
    public Func<Exception, bool>? ShouldRetry { get; init; }
}
