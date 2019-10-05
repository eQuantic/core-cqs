using System.Threading;
using System.Threading.Tasks;

namespace eQuantic.Core.CQS.Pipeline 
{
    public interface IPostProcessor<in TRequest, in TResponse>
    {
        Task Process(TRequest request, TResponse response, CancellationToken cancellationToken);
    }
}