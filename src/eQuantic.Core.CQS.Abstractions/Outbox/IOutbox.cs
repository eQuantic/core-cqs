namespace eQuantic.Core.CQS.Abstractions.Outbox;

/// <summary>
/// Service for adding messages to outbox
/// </summary>
public interface IOutbox
{
    /// <summary>
    /// Enqueues a message for reliable delivery
    /// </summary>
    Task Enqueue<T>(T message, string? correlationId = null, CancellationToken cancellationToken = default) where T : notnull;
}