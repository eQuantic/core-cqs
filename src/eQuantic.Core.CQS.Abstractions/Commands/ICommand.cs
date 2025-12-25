namespace eQuantic.Core.CQS.Abstractions.Commands;

/// <summary>
/// Marker interface for commands without result
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Marker interface for commands that return a result
/// </summary>
/// <typeparam name="TResult">The result type</typeparam>
public interface ICommand<TResult> : ICommand
{
}