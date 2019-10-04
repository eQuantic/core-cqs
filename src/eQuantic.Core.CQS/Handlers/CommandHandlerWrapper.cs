using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.CQS.Commands;
using eQuantic.Core.CQS.Extensions;
using eQuantic.Core.Ioc;

namespace eQuantic.Core.CQS.Handlers
{

    internal abstract class CommandHandlerWrapper : HandlerBase
    {
        public abstract Task Execute(ICommand command, CancellationToken cancellationToken,
            IContainer container);
    }
    internal abstract class CommandHandlerWrapper<TResult> : HandlerWithResultBase
    {
        public abstract Task<TResult> Execute(ICommand<TResult> command, CancellationToken cancellationToken,
            IContainer container);
    }

    internal class CommandHandlerWrapperImpl<TCommand> : CommandHandlerWrapper
        where TCommand : ICommand
    {
        public override Task Execute(object command, CancellationToken cancellationToken,
            IContainer container)
        {
            return Execute((ICommand)command, cancellationToken, container);
        }

        public override Task Execute(ICommand command, CancellationToken cancellationToken,
            IContainer container)
        {
            Task Handler() => container.ResolveHandler<ICommandHandler<TCommand>>().Execute((TCommand) command, cancellationToken);

            return container
                .ResolveAll<IPipelineBehavior<TCommand>>()
                .Reverse()
                .Aggregate((HandlerDelegate) Handler, (next, pipeline) => () => pipeline.Execute((TCommand)command, cancellationToken, next))();
        }
    }

    internal class CommandHandlerWrapperImpl<TCommand, TResult> : CommandHandlerWrapper<TResult>
        where TCommand : ICommand<TResult>
    {
        public override Task<object> Execute(object command, CancellationToken cancellationToken,
            IContainer container)
        {
            return Execute((ICommand<TResult>)command, cancellationToken, container)
                .ContinueWith(t => (object) t.Result);
        }

        public override Task<TResult> Execute(ICommand<TResult> command, CancellationToken cancellationToken,
            IContainer container)
        {
            Task<TResult> Handler() => container.ResolveHandler<ICommandHandler<TCommand, TResult>>().Execute((TCommand) command, cancellationToken);

            return container
                .ResolveAll<IPipelineBehavior<TCommand, TResult>>()
                .Reverse()
                .Aggregate((HandlerDelegate<TResult>) Handler, (next, pipeline) => () => pipeline.Execute((TCommand)command, cancellationToken, next))();
        }
    }
}