namespace eQuantic.Core.CQS.Abstractions.Pipeline;

/// <summary>
/// Pre-processor that runs before the handler
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
public interface IPreProcessor<in TRequest>
{
    /// <summary>
    /// Processes the request before the handler
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Process(TRequest request, CancellationToken cancellationToken);
}