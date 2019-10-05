using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace eQuantic.Core.CQS.Pipeline 
{

    public class PreProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    {
        private readonly IEnumerable<IPreProcessor<TRequest>> _preProcessors;

        public PreProcessorBehavior (IEnumerable<IPreProcessor<TRequest>> preProcessors) {
            _preProcessors = preProcessors;
        }

        public async Task<TResponse> Execute (TRequest request, CancellationToken cancellationToken, HandlerDelegate<TResponse> next) {
            foreach (var processor in _preProcessors) {
                await processor.Process (request, cancellationToken).ConfigureAwait (false);
            }

            return await next ().ConfigureAwait (false);
        }
    }
}