using eQuantic.Core.Collections;

namespace eQuantic.Core.CQS.Abstractions.Queries;

/// <summary>
/// Marker interface for paged queries
/// </summary>
/// <typeparam name="TResult">The result item type</typeparam>
public interface IPagedQuery<TResult> : IQuery<IPagedEnumerable<TResult>>
    where TResult : class
{
    /// <summary>
    /// The page index (0-based)
    /// </summary>
    int PageIndex { get; }
    
    /// <summary>
    /// The number of items per page
    /// </summary>
    int PageSize { get; }
}