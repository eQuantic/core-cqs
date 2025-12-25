namespace eQuantic.Core.CQS.Abstractions.Outbox;

/// <summary>
/// Repository for outbox messages
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Adds a message to the outbox
    /// </summary>
    Task Add(IOutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending messages for processing
    /// </summary>
    Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as processed
    /// </summary>
    Task MarkProcessed(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as failed
    /// </summary>
    Task MarkFailed(Guid messageId, string error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes old processed messages
    /// </summary>
    Task CleanupProcessed(TimeSpan olderThan, CancellationToken cancellationToken = default);
}