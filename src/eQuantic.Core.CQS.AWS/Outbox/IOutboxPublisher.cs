using eQuantic.Core.CQS.Abstractions.Outbox;

namespace eQuantic.Core.CQS.AWS.Outbox;

/// <summary>
/// Publishes outbox messages to AWS SQS
/// </summary>
public interface IOutboxPublisher
{
    /// <summary>
    /// Publishes a single message to SQS
    /// </summary>
    Task PublishAsync(IOutboxMessage message, CancellationToken ct = default);

    /// <summary>
    /// Publishes a batch of messages to SQS (max 10 per batch)
    /// </summary>
    Task PublishBatchAsync(IEnumerable<IOutboxMessage> messages, CancellationToken ct = default);
}