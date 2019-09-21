using System.Threading.Tasks;
using eQuantic.Core.Collections;
using eQuantic.Core.CQS.Queries;

namespace eQuantic.Core.CQS
{
    public interface IPagedQueryHandler<in TQuery, TResult>
        where TQuery : IPagedQuery<TResult>
        where TResult : class
    {
        Task<IPagedEnumerable<TResult>> Execute(TQuery query);
    }
}