using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace eQuantic.Core.CQS.Pipeline 
{

    public class PostProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    {
        private readonly IEnumerable<IPostProcessor<TRequest, TResponse>> _postProcessors;

        public PostProcessorBehavior(IEnumerable<IPostProcessor<TRequest, TResponse>> postProcessors)
        {
            _postProcessors = postProcessors;
        }

        public async Task<TResponse> Execute(TRequest request, CancellationToken cancellationToken, HandlerDelegate<TResponse> next)
        {
            var response = await next().ConfigureAwait(false);

            foreach (var processor in _postProcessors)
            {
                await processor.Process(request, response, cancellationToken).ConfigureAwait(false);
            }

            return response;
        }
    }
}