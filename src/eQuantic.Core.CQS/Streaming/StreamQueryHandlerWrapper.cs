using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using eQuantic.Core.CQS.Abstractions.Streaming;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Streaming;

/// <summary>
/// Wrapper for stream query handlers
/// </summary>
internal abstract class StreamQueryHandlerWrapper<TResult>
{
    public abstract IAsyncEnumerable<TResult> Handle(object query, CancellationToken cancellationToken, IServiceProvider serviceProvider);
}

/// <summary>
/// Implementation of stream query handler wrapper
/// </summary>
internal class StreamQueryHandlerWrapperImpl<TQuery, TResult> : StreamQueryHandlerWrapper<TResult>
    where TQuery : IStreamQuery<TResult>
{
    public override IAsyncEnumerable<TResult> Handle(object query, CancellationToken cancellationToken, IServiceProvider serviceProvider)
    {
        return HandleInternal((TQuery)query, cancellationToken, serviceProvider);
    }

    private async IAsyncEnumerable<TResult> HandleInternal(
        TQuery query, 
        [EnumeratorCancellation] CancellationToken cancellationToken,
        IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IStreamQueryHandler<TQuery, TResult>>();
        
        await foreach (var item in handler.Handle(query, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }
}
