using System.Threading;
using System.Threading.Tasks;

namespace eQuantic.Core.CQS
{
    public delegate Task<TResult> HandlerDelegate<TResult>();
    
    public delegate Task HandlerDelegate();

    public interface IPipelineBehavior<in TCommand>
    {
        Task Execute(TCommand command, CancellationToken cancellationToken, HandlerDelegate next);
    }

    public interface IPipelineBehavior<in TQueryOrCommand, TResult>
    {
        Task<TResult> Execute(TQueryOrCommand command, CancellationToken cancellationToken, HandlerDelegate<TResult> next);
    }
}