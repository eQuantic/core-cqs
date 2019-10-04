using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Ioc;

namespace eQuantic.Core.CQS.Handlers
{
    internal abstract class HandlerBase
    {
        public abstract Task Execute(object command, CancellationToken cancellationToken,
            IContainer container);
    }

    internal abstract class HandlerWithResultBase
    {
        public abstract Task<object> Execute(object command, CancellationToken cancellationToken,
            IContainer serviceFactory);
    }
}