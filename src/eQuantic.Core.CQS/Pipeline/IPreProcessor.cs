using System.Threading;
using System.Threading.Tasks;

namespace eQuantic.Core.CQS.Pipeline 
{
    public interface IPreProcessor<in TRequest>
    {
        Task Process(TRequest request, CancellationToken cancellationToken);
    }
}