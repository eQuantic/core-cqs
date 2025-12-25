namespace eQuantic.Core.CQS.Abstractions.Pipeline;

/// <summary>
/// Post-processor that runs after the handler
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IPostProcessor<in TRequest, in TResponse>
{
    /// <summary>
    /// Processes the request and response after the handler
    /// </summary>
    /// <param name="request">The original request</param>
    /// <param name="response">The response from the handler</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Process(TRequest request, TResponse response, CancellationToken cancellationToken);
}