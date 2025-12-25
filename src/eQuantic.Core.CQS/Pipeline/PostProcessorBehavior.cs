using eQuantic.Core.CQS.Abstractions;
using eQuantic.Core.CQS.Abstractions.Pipeline;

namespace eQuantic.Core.CQS.Pipeline;

/// <summary>
/// Pipeline behavior that runs post-processors after the handler
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class PostProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IPostProcessor<TRequest, TResponse>> _postProcessors;

    /// <summary>
    /// Creates a new PostProcessorBehavior
    /// </summary>
    /// <param name="postProcessors">The post-processors to run</param>
    public PostProcessorBehavior(IEnumerable<IPostProcessor<TRequest, TResponse>> postProcessors)
    {
        _postProcessors = postProcessors;
    }

    /// <inheritdoc />
    public async Task<TResponse> Execute(TRequest request, CancellationToken cancellationToken, HandlerDelegate<TResponse> next)
    {
        var response = await next().ConfigureAwait(false);

        foreach (var processor in _postProcessors)
        {
            await processor.Process(request, response, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }
}