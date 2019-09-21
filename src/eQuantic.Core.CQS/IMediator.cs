using System.Threading.Tasks;
using eQuantic.Core.Collections;
using eQuantic.Core.CQS.Commands;
using eQuantic.Core.CQS.Queries;

namespace eQuantic.Core.CQS
{
    public interface IMediator
    {
        Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query)
            where TResult : class;

        Task<IPagedEnumerable<TResult>> ExecuteAsync<TResult>(IPagedQuery<TResult> query)
            where TResult : class;

        Task ExecuteAsync(ICommand command);

        Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command);
    }
}