using System.Collections.Concurrent;
using eQuantic.Core.Collections;
using eQuantic.Core.CQS.Abstractions;
using eQuantic.Core.CQS.Abstractions.Commands;
using eQuantic.Core.CQS.Abstractions.Queries;
using eQuantic.Core.CQS.Abstractions.Streaming;
using eQuantic.Core.CQS.Handlers;
using eQuantic.Core.CQS.Streaming;

namespace eQuantic.Core.CQS;

/// <summary>
/// Default implementation of IMediator
/// </summary>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    
    private static readonly ConcurrentDictionary<Type, object> QueryHandlers = new();
    private static readonly ConcurrentDictionary<Type, object> PagedQueryHandlers = new();
    private static readonly ConcurrentDictionary<Type, object> CommandHandlers = new();
    private static readonly ConcurrentDictionary<Type, object> CommandWithResultHandlers = new();
    private static readonly ConcurrentDictionary<Type, object> StreamQueryHandlers = new();

    /// <summary>
    /// Creates a new Mediator instance
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving handlers</param>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default) 
        where TResult : class
    {
        ArgumentNullException.ThrowIfNull(query);

        var queryType = query.GetType();

        var handler = (QueryHandlerWrapper<TResult>)QueryHandlers.GetOrAdd(queryType,
            t => Activator.CreateInstance(
                typeof(QueryHandlerWrapperImpl<,>).MakeGenericType(queryType, typeof(TResult)))!);

        return handler.Execute(query, cancellationToken, _serviceProvider);
    }

    /// <inheritdoc />
    public Task<IPagedEnumerable<TResult>> ExecuteAsync<TResult>(IPagedQuery<TResult> query, CancellationToken cancellationToken = default) 
        where TResult : class
    {
        ArgumentNullException.ThrowIfNull(query);

        var queryType = query.GetType();

        var handler = (PagedQueryHandlerWrapper<TResult>)PagedQueryHandlers.GetOrAdd(queryType,
            t => Activator.CreateInstance(
                typeof(PagedQueryHandlerWrapperImpl<,>).MakeGenericType(queryType, typeof(TResult)))!);

        return handler.Execute(query, cancellationToken, _serviceProvider);
    }

    /// <inheritdoc />
    public Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandType = command.GetType();

        var handler = (CommandHandlerWrapper)CommandHandlers.GetOrAdd(commandType,
            t => Activator.CreateInstance(
                typeof(CommandHandlerWrapperImpl<>).MakeGenericType(commandType))!);

        return handler.Execute(command, cancellationToken, _serviceProvider);
    }

    /// <inheritdoc />
    public Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandType = command.GetType();

        var handler = (CommandHandlerWrapper<TResult>)CommandWithResultHandlers.GetOrAdd(commandType,
            t => Activator.CreateInstance(
                typeof(CommandHandlerWrapperImpl<,>).MakeGenericType(commandType, typeof(TResult)))!);

        return handler.Execute(command, cancellationToken, _serviceProvider);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResult> ExecuteStreamAsync<TResult>(IStreamQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var queryType = query.GetType();

        var handler = (StreamQueryHandlerWrapper<TResult>)StreamQueryHandlers.GetOrAdd(queryType,
            t => Activator.CreateInstance(
                typeof(StreamQueryHandlerWrapperImpl<,>).MakeGenericType(queryType, typeof(TResult)))!);

        return handler.Handle(query, cancellationToken, _serviceProvider);
    }
}