namespace eQuantic.Core.CQS.Abstractions.Streaming;

/// <summary>
/// Handler for stream queries
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResult">The result item type</typeparam>
public interface IStreamQueryHandler<in TQuery, out TResult>
    where TQuery : IStreamQuery<TResult>
{
    /// <summary>
    /// Handles the stream query and returns an async enumerable of results
    /// </summary>
    /// <param name="query">The stream query to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of results</returns>
    IAsyncEnumerable<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}
