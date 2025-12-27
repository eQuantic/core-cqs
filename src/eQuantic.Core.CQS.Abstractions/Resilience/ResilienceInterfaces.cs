using eQuantic.Core.CQS.Abstractions.Sagas;

namespace eQuantic.Core.CQS.Abstractions.Resilience;

/// <summary>
/// Extended saga data interface with resilience capabilities.
/// Implements LSP - can be used anywhere ISagaData is expected.
/// </summary>
public interface IResilientSagaData : ISagaData
{
    /// <summary>
    /// Gets the timeout for this saga. Returns null to use default.
    /// </summary>
    TimeSpan? Timeout => null;
    
    /// <summary>
    /// Gets the maximum number of retry attempts.
    /// </summary>
    int MaxRetries => 3;
    
    /// <summary>
    /// Gets the last error message if the saga failed.
    /// </summary>
    string? LastError { get; set; }
    
    /// <summary>
    /// Gets or sets the current retry attempt.
    /// </summary>
    int RetryAttempt { get; set; }
}

/// <summary>
/// Policy for saga timeout handling.
/// Implements ISP - focused interface for timeout policy only.
/// </summary>
public interface ISagaTimeoutPolicy
{
    /// <summary>
    /// Gets the timeout duration for a saga.
    /// </summary>
    TimeSpan GetTimeout(ISagaData saga);
    
    /// <summary>
    /// Handles saga timeout.
    /// </summary>
    Task OnTimeoutAsync(ISagaData saga, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler for saga compensation logic.
/// Implements ISP - focused interface for compensation only.
/// </summary>
/// <typeparam name="TSaga">The saga data type</typeparam>
public interface ICompensationHandler<in TSaga> where TSaga : ISagaData
{
    /// <summary>
    /// Executes compensation logic for a failed saga.
    /// </summary>
    Task CompensateAsync(TSaga saga, Exception? exception, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler for dead letter queue operations.
/// Implements ISP - focused interface for dead letter handling only.
/// </summary>
public interface IDeadLetterHandler
{
    /// <summary>
    /// Handles a saga that cannot be processed.
    /// </summary>
    Task HandleAsync(ISagaData saga, string reason, CancellationToken cancellationToken = default);
}
