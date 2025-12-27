using eQuantic.Core.CQS.Abstractions;
using eQuantic.Core.CQS.Abstractions.Resilience;

namespace eQuantic.Core.CQS.Resilience;

/// <summary>
/// Pipeline behavior that implements retry with exponential backoff
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly RetryOptions _options;
    private static readonly Random Jitter = new();

    /// <summary>
    /// Creates a new retry behavior with default options
    /// </summary>
    public RetryBehavior() : this(new RetryOptions())
    {
    }

    /// <summary>
    /// Creates a new retry behavior with specified options
    /// </summary>
    public RetryBehavior(RetryOptions options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public async Task<TResponse> Execute(TRequest request, CancellationToken cancellationToken, HandlerDelegate<TResponse> next)
    {
        var retryAttempt = 0;
        var delay = _options.InitialDelay;

        while (true)
        {
            try
            {
                return await next().ConfigureAwait(false);
            }
            catch (Exception ex) when (ShouldRetry(ex, retryAttempt))
            {
                retryAttempt++;
                
                var actualDelay = CalculateDelay(delay);
                await Task.Delay(actualDelay, cancellationToken).ConfigureAwait(false);
                
                delay = CalculateNextDelay(delay);
            }
        }
    }

    private bool ShouldRetry(Exception exception, int currentAttempt)
    {
        if (currentAttempt >= _options.MaxRetries)
            return false;

        // Check custom predicate first
        if (_options.ShouldRetry is not null)
            return _options.ShouldRetry(exception);

        // If no specific exceptions configured, retry all
        if (_options.RetryableExceptions.Length == 0)
            return true;

        // Check if exception type matches any retryable exception
        return _options.RetryableExceptions.Any(t => t.IsInstanceOfType(exception));
    }

    private TimeSpan CalculateDelay(TimeSpan baseDelay)
    {
        if (!_options.UseJitter)
            return baseDelay;

        // Add up to 25% jitter
        var jitterFactor = 1.0 + (Jitter.NextDouble() * 0.25);
        var delayWithJitter = TimeSpan.FromTicks((long)(baseDelay.Ticks * jitterFactor));
        
        return delayWithJitter > _options.MaxDelay ? _options.MaxDelay : delayWithJitter;
    }

    private TimeSpan CalculateNextDelay(TimeSpan currentDelay)
    {
        var nextDelay = TimeSpan.FromTicks((long)(currentDelay.Ticks * _options.BackoffMultiplier));
        return nextDelay > _options.MaxDelay ? _options.MaxDelay : nextDelay;
    }
}
