using eQuantic.Core.Collections;
using eQuantic.Core.CQS.Abstractions.Commands;
using eQuantic.Core.CQS.Abstractions.Queries;
using eQuantic.Core.CQS.Abstractions.Streaming;

namespace eQuantic.Core.CQS.Abstractions;

/// <summary>
/// Mediator interface for dispatching commands and queries
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Executes a query and returns the result
    /// </summary>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="query">The query to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The query result</returns>
    Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        where TResult : class;

    /// <summary>
    /// Executes a paged query and returns the paged result
    /// </summary>
    /// <typeparam name="TResult">The result item type</typeparam>
    /// <param name="query">The paged query to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paged result</returns>
    Task<IPagedEnumerable<TResult>> ExecuteAsync<TResult>(IPagedQuery<TResult> query, CancellationToken cancellationToken = default)
        where TResult : class;

    /// <summary>
    /// Executes a command without returning a result
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a command and returns the result
    /// </summary>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="command">The command to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The command result</returns>
    Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a stream query and returns an async enumerable of results
    /// </summary>
    /// <typeparam name="TResult">The result item type</typeparam>
    /// <param name="query">The stream query to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of results</returns>
    IAsyncEnumerable<TResult> ExecuteStreamAsync<TResult>(IStreamQuery<TResult> query, CancellationToken cancellationToken = default);
}