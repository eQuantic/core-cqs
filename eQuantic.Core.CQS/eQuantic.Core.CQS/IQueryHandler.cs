using System.Threading.Tasks;
using eQuantic.Core.CQS.Queries;

namespace eQuantic.Core.CQS
{
    public interface IQueryHandler<in TQuery, TResult>
        where TQuery : IQuery<TResult>
        where TResult : class
    {
        Task<TResult> Execute(TQuery query);
    }
}