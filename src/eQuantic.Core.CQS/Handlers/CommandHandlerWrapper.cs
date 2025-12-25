using eQuantic.Core.CQS.Abstractions;
using eQuantic.Core.CQS.Abstractions.Commands;
using eQuantic.Core.CQS.Abstractions.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Handlers;

/// <summary>
/// Wrapper for command handlers without result
/// </summary>
internal abstract class CommandHandlerWrapper : HandlerBase
{
    public abstract Task Execute(ICommand command, CancellationToken cancellationToken, IServiceProvider serviceProvider);
}

/// <summary>
/// Wrapper for command handlers with result
/// </summary>
/// <typeparam name="TResult">The result type</typeparam>
internal abstract class CommandHandlerWrapper<TResult> : HandlerWithResultBase
{
    public abstract Task<TResult> Execute(ICommand<TResult> command, CancellationToken cancellationToken, IServiceProvider serviceProvider);
}

/// <summary>
/// Implementation of command handler wrapper without result
/// </summary>
internal class CommandHandlerWrapperImpl<TCommand> : CommandHandlerWrapper
    where TCommand : ICommand
{
    public override Task Execute(object request, CancellationToken cancellationToken, IServiceProvider serviceProvider)
    {
        return Execute((ICommand)request, cancellationToken, serviceProvider);
    }

    public override Task Execute(ICommand command, CancellationToken cancellationToken, IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        
        Task Handler() => handler.Execute((TCommand)command, cancellationToken);

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TCommand>>().Reverse();
        
        return behaviors.Aggregate(
            (HandlerDelegate)Handler,
            (next, pipeline) => () => pipeline.Execute((TCommand)command, cancellationToken, next))();
    }
}

/// <summary>
/// Implementation of command handler wrapper with result
/// </summary>
internal class CommandHandlerWrapperImpl<TCommand, TResult> : CommandHandlerWrapper<TResult>
    where TCommand : ICommand<TResult>
{
    public override async Task<object?> Execute(object request, CancellationToken cancellationToken, IServiceProvider serviceProvider)
    {
        return await Execute((ICommand<TResult>)request, cancellationToken, serviceProvider);
    }

    public override Task<TResult> Execute(ICommand<TResult> command, CancellationToken cancellationToken, IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        
        Task<TResult> Handler() => handler.Execute((TCommand)command, cancellationToken);

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TCommand, TResult>>().Reverse();
        
        return behaviors.Aggregate(
            (HandlerDelegate<TResult>)Handler,
            (next, pipeline) => () => pipeline.Execute((TCommand)command, cancellationToken, next))();
    }
}