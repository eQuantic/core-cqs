using eQuantic.Core.CQS.Abstractions.Outbox;

namespace eQuantic.Core.CQS.Azure.Outbox;

/// <summary>
/// Publishes outbox messages to Azure Service Bus
/// </summary>
public interface IOutboxPublisher
{
    /// <summary>
    /// Publishes a single message to the message bus
    /// </summary>
    Task PublishAsync(IOutboxMessage message, CancellationToken ct = default);

    /// <summary>
    /// Publishes a batch of messages to the message bus
    /// </summary>
    Task PublishBatchAsync(IEnumerable<IOutboxMessage> messages, CancellationToken ct = default);
}