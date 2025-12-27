using eQuantic.Core.CQS.Abstractions;
using Polly;
using Polly.Retry;

namespace eQuantic.Core.CQS.Polly.Behaviors;

/// <summary>
/// Pipeline behavior that implements retry with Polly.
/// </summary>
public class PollyRetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ResiliencePipeline _pipeline;

    public PollyRetryBehavior(PollyOptions options)
    {
        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = options.MaxRetryAttempts,
                Delay = options.InitialDelay,
                BackoffType = options.UseExponentialBackoff ? DelayBackoffType.Exponential : DelayBackoffType.Constant,
                UseJitter = options.UseJitter,
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex => 
                    options.ShouldRetry?.Invoke(ex) ?? true)
            })
            .Build();
    }

    public PollyRetryBehavior(ResiliencePipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public async Task<TResponse> Execute(TRequest request, CancellationToken cancellationToken, HandlerDelegate<TResponse> next)
    {
        return await _pipeline.ExecuteAsync(async ct => await next(), cancellationToken);
    }
}

/// <summary>
/// Configuration options for Polly behaviors.
/// </summary>
public class PollyOptions
{
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMilliseconds(100);
    public bool UseExponentialBackoff { get; set; } = true;
    public bool UseJitter { get; set; } = true;
    public Func<Exception, bool>? ShouldRetry { get; set; }
}
