using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Collections;
using eQuantic.Core.CQS.Extensions;
using eQuantic.Core.CQS.Queries;
using eQuantic.Core.Ioc;

namespace eQuantic.Core.CQS.Handlers
{
    internal abstract class PagedQueryHandlerWrapper<TResult> : HandlerWithResultBase
    {
        public abstract Task<IPagedEnumerable<TResult>> Execute(IPagedQuery<TResult> query, CancellationToken cancellationToken,
            IContainer container);
    }

    internal class PagedQueryHandlerWrapperImpl<TQuery, TResult> : PagedQueryHandlerWrapper<TResult>
        where TQuery : IPagedQuery<TResult>
        where TResult : class
    {
        public override Task<object> Execute(object query, CancellationToken cancellationToken,
            IContainer container)
        {
            return Execute((IPagedQuery<TResult>)query, cancellationToken, container)
                .ContinueWith(t => (object) t.Result);
        }

        public override Task<IPagedEnumerable<TResult>> Execute(IPagedQuery<TResult> query, CancellationToken cancellationToken,
            IContainer container)
        {
            Task<IPagedEnumerable<TResult>> Handler() => container.ResolveHandler<IPagedQueryHandler<TQuery, TResult>>().Execute((TQuery) query, cancellationToken);

            return container
                .ResolveAll<IPipelineBehavior<TQuery, IPagedEnumerable<TResult>>>()
                .Reverse()
                .Aggregate((HandlerDelegate<IPagedEnumerable<TResult>>) Handler, (next, pipeline) => () => pipeline.Execute((TQuery)query, cancellationToken, next))();
        }
    }
}