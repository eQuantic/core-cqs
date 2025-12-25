using eQuantic.Core.CQS.Abstractions;
using eQuantic.Core.CQS.Abstractions.Pipeline;

namespace eQuantic.Core.CQS.Pipeline;

/// <summary>
/// Pipeline behavior that runs pre-processors before the handler
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class PreProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IPreProcessor<TRequest>> _preProcessors;

    /// <summary>
    /// Creates a new PreProcessorBehavior
    /// </summary>
    /// <param name="preProcessors">The pre-processors to run</param>
    public PreProcessorBehavior(IEnumerable<IPreProcessor<TRequest>> preProcessors)
    {
        _preProcessors = preProcessors;
    }

    /// <inheritdoc />
    public async Task<TResponse> Execute(TRequest request, CancellationToken cancellationToken, HandlerDelegate<TResponse> next)
    {
        foreach (var processor in _preProcessors)
        {
            await processor.Process(request, cancellationToken).ConfigureAwait(false);
        }

        return await next().ConfigureAwait(false);
    }
}