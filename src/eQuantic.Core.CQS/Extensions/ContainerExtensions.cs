using System;
using eQuantic.Core.Ioc;

namespace eQuantic.Core.CQS.Extensions
{
    internal static class ContainerExtensions
    {
        public static THandler ResolveHandler<THandler>(this IContainer container)
        {
            THandler handler;

            try
            {
                handler = container.Resolve<THandler>();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error constructing handler for request of type {typeof(THandler)}. Register your handlers with the container. See the samples in GitHub for examples.", e);
            }

            if (handler == null)
            {
                throw new InvalidOperationException($"Handler was not found for request of type {typeof(THandler)}. Register your handlers with the container. See the samples in GitHub for examples.");
            }

            return handler;
        }
    }
}