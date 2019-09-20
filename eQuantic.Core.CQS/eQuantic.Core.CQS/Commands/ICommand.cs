namespace eQuantic.Core.CQS.Commands
{
    public interface ICommand
    {
    }

    public interface ICommand<TResult> : ICommand
    {
    }
}