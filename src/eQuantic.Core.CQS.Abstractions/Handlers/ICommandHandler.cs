namespace eQuantic.Core.CQS.Abstractions.Handlers;

/// <summary>
/// Handler for commands without result
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
public interface ICommandHandler<in TCommand> 
    where TCommand : Commands.ICommand
{
    /// <summary>
    /// Executes the command
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Execute(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Handler for commands with result
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public interface ICommandHandler<in TCommand, TResult> 
    where TCommand : Commands.ICommand<TResult>
{
    /// <summary>
    /// Executes the command and returns a result
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The command result</returns>
    Task<TResult> Execute(TCommand command, CancellationToken cancellationToken);
}