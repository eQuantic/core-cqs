namespace eQuantic.Core.CQS.Abstractions.Outbox;

/// <summary>
/// Publishes outbox messages to an external message broker.
/// Implements ISP - focused interface for publishing only.
/// </summary>
public interface IOutboxPublisher
{
    /// <summary>
    /// Publishes a single message.
    /// </summary>
    Task PublishAsync(IOutboxMessage message, CancellationToken ct = default);
    
    /// <summary>
    /// Publishes multiple messages in a batch.
    /// </summary>
    Task PublishBatchAsync(IEnumerable<IOutboxMessage> messages, CancellationToken ct = default);
}
