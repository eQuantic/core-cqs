namespace eQuantic.Core.CQS.Handlers;

/// <summary>
/// Base class for handler wrappers (commands without result)
/// </summary>
internal abstract class HandlerBase
{
    public abstract Task Execute(object request, CancellationToken cancellationToken, IServiceProvider serviceProvider);
}

/// <summary>
/// Base class for handler wrappers with return value
/// </summary>
internal abstract class HandlerWithResultBase
{
    public abstract Task<object?> Execute(object request, CancellationToken cancellationToken, IServiceProvider serviceProvider);
}