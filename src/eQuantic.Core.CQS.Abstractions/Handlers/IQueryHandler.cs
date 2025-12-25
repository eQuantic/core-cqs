using eQuantic.Core.CQS.Abstractions.Queries;

namespace eQuantic.Core.CQS.Abstractions.Handlers;

/// <summary>
/// Handler for queries
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
    where TResult : class
{
    /// <summary>
    /// Executes the query
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The query result</returns>
    Task<TResult> Execute(TQuery query, CancellationToken cancellationToken);
}