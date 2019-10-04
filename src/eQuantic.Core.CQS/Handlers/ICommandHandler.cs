using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.CQS.Commands;

namespace eQuantic.Core.CQS
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task Execute(TCommand command, CancellationToken cancellationToken);
    }

    public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand
    {
        Task<TResult> Execute(TCommand command, CancellationToken cancellationToken);
    }
}