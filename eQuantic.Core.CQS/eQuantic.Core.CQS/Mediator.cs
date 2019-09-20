using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eQuantic.Core.Collections;
using eQuantic.Core.CQS.Commands;
using eQuantic.Core.CQS.Queries;
using eQuantic.Core.Ioc;

namespace eQuantic.Core.CQS
{
    public class Mediator : IMediator
    {
        private readonly IContainer container;

        public Mediator(IContainer container)
        {
            this.container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query) where TResult : class
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return null;
        }

        public Task<IPagedEnumerable<TResult>> ExecuteAsync<TResult>(IPagedQuery<TResult> query) where TResult : class
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return null;
        }

        public async Task ExecuteAsync(ICommand command)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var handlers = ResolveCommandHandler(command);
            await Task.WhenAll(handlers.Select(h => h.Execute(command)));
        }

        public async Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var handlers = ResolveCommandHandler<ICommand<TResult>, TResult>(command);
            return (await Task.WhenAll(handlers.Select(h => h.Execute(command)))).FirstOrDefault();
        }

        private IEnumerable<ICommandHandler<TCommand>> ResolveCommandHandler<TCommand>(TCommand command)
                                    where TCommand : ICommand
        {
            var type = typeof(ICommandHandler<TCommand>);
            return (IEnumerable<ICommandHandler<TCommand>>)this.container.ResolveAll(type);
        }

        private IEnumerable<ICommandHandler<TCommand, TResult>> ResolveCommandHandler<TCommand, TResult>(TCommand command)
                                    where TCommand : ICommand<TResult>
        {
            var handlerType = typeof(ICommandHandler<,>);
            var type = handlerType.MakeGenericType(command.GetType(), typeof(TResult));

            return (IEnumerable<ICommandHandler<TCommand, TResult>>)this.container.ResolveAll(type);
        }
    }
}