using eQuantic.Core.Collections;
using eQuantic.Core.CQS.Abstractions;
using eQuantic.Core.CQS.Abstractions.Handlers;
using eQuantic.Core.CQS.Abstractions.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Handlers;

/// <summary>
/// Wrapper for paged query handlers
/// </summary>
internal abstract class PagedQueryHandlerWrapper<TResult> : HandlerWithResultBase
    where TResult : class
{
    public abstract Task<IPagedEnumerable<TResult>> Execute(IPagedQuery<TResult> query, CancellationToken cancellationToken, IServiceProvider serviceProvider);
}

/// <summary>
/// Implementation of paged query handler wrapper
/// </summary>
internal class PagedQueryHandlerWrapperImpl<TQuery, TResult> : PagedQueryHandlerWrapper<TResult>
    where TQuery : IPagedQuery<TResult>
    where TResult : class
{
    public override async Task<object?> Execute(object request, CancellationToken cancellationToken, IServiceProvider serviceProvider)
    {
        return await Execute((IPagedQuery<TResult>)request, cancellationToken, serviceProvider);
    }

    public override Task<IPagedEnumerable<TResult>> Execute(IPagedQuery<TResult> query, CancellationToken cancellationToken, IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IPagedQueryHandler<TQuery, TResult>>();
        
        Task<IPagedEnumerable<TResult>> Handler() => handler.Execute((TQuery)query, cancellationToken);

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TQuery, IPagedEnumerable<TResult>>>().Reverse();
        
        return behaviors.Aggregate(
            (HandlerDelegate<IPagedEnumerable<TResult>>)Handler,
            (next, pipeline) => () => pipeline.Execute((TQuery)query, cancellationToken, next))();
    }
}