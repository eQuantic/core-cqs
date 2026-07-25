namespace eQuantic.Core.CQS.Abstractions.Outbox;

/// <summary>
/// Service for adding messages to outbox
/// </summary>
public interface IOutbox
{
    /// <summary>
    /// Enqueues a message for reliable delivery.
    /// </summary>
    /// <param name="message">The message to deliver.</param>
    /// <param name="correlationId">Correlation id for tracing.</param>
    /// <param name="context">
    /// Ambient context to travel with the message (JSON by convention), for
    /// anything a handler will need that only exists now — see
    /// <see cref="IOutboxMessage.Context" />.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task Enqueue<T>(
        T message,
        string? correlationId = null,
        string? context = null,
        CancellationToken cancellationToken = default) where T : notnull;
}