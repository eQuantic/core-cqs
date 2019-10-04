using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Collections;
using eQuantic.Core.CQS.Commands;
using eQuantic.Core.CQS.Handlers;
using eQuantic.Core.CQS.Queries;
using eQuantic.Core.Ioc;

namespace eQuantic.Core.CQS
{
    public class Mediator : IMediator
    {
        private readonly IContainer container;
        private static readonly ConcurrentDictionary<Type, object> _queryHandlers = new ConcurrentDictionary<Type, object>();
        private static readonly ConcurrentDictionary<Type, object> _pagedQueryHandlers = new ConcurrentDictionary<Type, object>();
        private static readonly ConcurrentDictionary<Type, object> _commandHandlers = new ConcurrentDictionary<Type, object>();
        private static readonly ConcurrentDictionary<Type, object> _commandWithResultHandlers = new ConcurrentDictionary<Type, object>();

        public Mediator(IContainer container)
        {
            this.container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default) where TResult : class
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var queryType = query.GetType();

            var handler = (QueryHandlerWrapper<TResult>)_queryHandlers.GetOrAdd(queryType,
                t => Activator.CreateInstance(typeof(QueryHandlerWrapperImpl<,>).MakeGenericType(queryType, typeof(TResult))));

            return handler.Execute(query, cancellationToken, this.container);
        }

        public Task<IPagedEnumerable<TResult>> ExecuteAsync<TResult>(IPagedQuery<TResult> query, CancellationToken cancellationToken = default) where TResult : class
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var queryType = query.GetType();

            var handler = (PagedQueryHandlerWrapper<TResult>)_pagedQueryHandlers.GetOrAdd(queryType,
                t => Activator.CreateInstance(typeof(PagedQueryHandlerWrapperImpl<,>).MakeGenericType(queryType, typeof(TResult))));

            return handler.Execute(query, cancellationToken, this.container);
        }

        public Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var commandType = command.GetType();

            var handler = (CommandHandlerWrapper)_commandHandlers.GetOrAdd(commandType,
                t => Activator.CreateInstance(typeof(CommandHandlerWrapperImpl<>).MakeGenericType(commandType)));

            return handler.Execute(command, cancellationToken, this.container);
        }

        public Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var commandType = command.GetType();

            var handler = (CommandHandlerWrapper<TResult>)_commandWithResultHandlers.GetOrAdd(commandType,
                t => Activator.CreateInstance(typeof(CommandHandlerWrapperImpl<,>).MakeGenericType(commandType, typeof(TResult))));

            return handler.Execute(command, cancellationToken, this.container);
        }
    }
}