using eQuantic.Core.CQS.Abstractions;
using eQuantic.Core.CQS.Abstractions.Handlers;
using eQuantic.Core.CQS.Abstractions.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Handlers;

/// <summary>
/// Wrapper for query handlers
/// </summary>
internal abstract class QueryHandlerWrapper<TResult> : HandlerWithResultBase
    where TResult : class
{
    public abstract Task<TResult> Execute(IQuery<TResult> query, CancellationToken cancellationToken, IServiceProvider serviceProvider);
}

/// <summary>
/// Implementation of query handler wrapper
/// </summary>
internal class QueryHandlerWrapperImpl<TQuery, TResult> : QueryHandlerWrapper<TResult>
    where TQuery : IQuery<TResult>
    where TResult : class
{
    public override async Task<object?> Execute(object request, CancellationToken cancellationToken, IServiceProvider serviceProvider)
    {
        return await Execute((IQuery<TResult>)request, cancellationToken, serviceProvider);
    }

    public override Task<TResult> Execute(IQuery<TResult> query, CancellationToken cancellationToken, IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        
        Task<TResult> Handler() => handler.Execute((TQuery)query, cancellationToken);

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TQuery, TResult>>().Reverse();
        
        return behaviors.Aggregate(
            (HandlerDelegate<TResult>)Handler,
            (next, pipeline) => () => pipeline.Execute((TQuery)query, cancellationToken, next))();
    }
}