using eQuantic.Core.Collections;
using eQuantic.Core.CQS.Abstractions.Queries;

namespace eQuantic.Core.CQS.Abstractions.Handlers;

/// <summary>
/// Handler for paged queries
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResult">The result item type</typeparam>
public interface IPagedQueryHandler<in TQuery, TResult>
    where TQuery : IPagedQuery<TResult>
    where TResult : class
{
    /// <summary>
    /// Executes the paged query
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paged result</returns>
    Task<IPagedEnumerable<TResult>> Execute(TQuery query, CancellationToken cancellationToken);
}