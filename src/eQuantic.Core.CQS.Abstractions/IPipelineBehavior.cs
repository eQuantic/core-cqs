namespace eQuantic.Core.CQS.Abstractions;

/// <summary>
/// Delegate for the next handler in the pipeline
/// </summary>
public delegate Task HandlerDelegate();

/// <summary>
/// Delegate for the next handler in the pipeline with a result
/// </summary>
/// <typeparam name="TResult">The result type</typeparam>
public delegate Task<TResult> HandlerDelegate<TResult>();

/// <summary>
/// Pipeline behavior for commands/queries without result
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
public interface IPipelineBehavior<in TRequest>
{
    /// <summary>
    /// Executes the behavior
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="next">The next handler in the pipeline</param>
    Task Execute(TRequest request, CancellationToken cancellationToken, HandlerDelegate next);
}

/// <summary>
/// Pipeline behavior for commands/queries with result
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public interface IPipelineBehavior<in TRequest, TResult>
{
    /// <summary>
    /// Executes the behavior
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <returns>The result from the next handler or modified result</returns>
    Task<TResult> Execute(TRequest request, CancellationToken cancellationToken, HandlerDelegate<TResult> next);
}