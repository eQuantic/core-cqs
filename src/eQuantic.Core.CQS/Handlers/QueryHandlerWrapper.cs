using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.CQS.Extensions;
using eQuantic.Core.CQS.Queries;
using eQuantic.Core.Ioc;

namespace eQuantic.Core.CQS.Handlers
{
    internal abstract class QueryHandlerWrapper<TResult> : HandlerWithResultBase
    {
        public abstract Task<TResult> Execute(IQuery<TResult> query, CancellationToken cancellationToken,
            IContainer container);
    }

    internal class QueryHandlerWrapperImpl<TQuery, TResult> : QueryHandlerWrapper<TResult>
        where TQuery : IQuery<TResult>
        where TResult : class
    {
        public override Task<object> Execute(object query, CancellationToken cancellationToken,
            IContainer container)
        {
            return Execute((IQuery<TResult>)query, cancellationToken, container)
                .ContinueWith(t => (object) t.Result);
        }

        public override Task<TResult> Execute(IQuery<TResult> query, CancellationToken cancellationToken,
            IContainer container)
        {
            Task<TResult> Handler() => container.ResolveHandler<IQueryHandler<TQuery, TResult>>().Execute((TQuery) query, cancellationToken);

            return container
                .ResolveAll<IPipelineBehavior<TQuery, TResult>>()
                .Reverse()
                .Aggregate((HandlerDelegate<TResult>) Handler, (next, pipeline) => () => pipeline.Execute((TQuery)query, cancellationToken, next))();
        }
    }
}