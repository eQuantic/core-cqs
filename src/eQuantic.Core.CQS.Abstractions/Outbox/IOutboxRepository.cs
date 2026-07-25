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
    /// Gets pending messages for processing, oldest first.
    /// </summary>
    /// <remarks>
    /// Only messages deliverable <em>now</em> are returned: one whose
    /// <see cref="IOutboxMessage.NextAttemptAt" /> lies in the future is skipped
    /// until its backoff elapses.
    /// <para>
    /// Implementations over a store that can lock rows claim the batch, so
    /// concurrent relays never receive the same message. Those that cannot are
    /// at-least-once and expect a single relay instance — each implementation
    /// documents which of the two it is.
    /// </para>
    /// </remarks>
    Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as processed
    /// </summary>
    Task MarkProcessed(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a failed delivery attempt and decides what happens next: while
    /// attempts remain the message stays pending and is rescheduled with
    /// backoff; only once they are exhausted does it become
    /// <see cref="OutboxMessageState.Failed" /> — a dead letter kept for
    /// inspection.
    /// </summary>
    /// <param name="messageId">The message that failed.</param>
    /// <param name="error">The failure detail, for diagnosis.</param>
    /// <param name="maxAttempts">Attempts allowed before the message is dead-lettered.</param>
    /// <param name="backoff">Base delay before the next attempt, scaled by the attempt count.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <remarks>
    /// The retry policy is a parameter rather than repository state, so a relay
    /// can be tuned without reconfiguring the store and the decision is made
    /// where the failure is actually observed.
    /// </remarks>
    Task MarkFailed(
        Guid messageId,
        string error,
        int maxAttempts,
        TimeSpan backoff,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes old processed messages
    /// </summary>
    Task CleanupProcessed(TimeSpan olderThan, CancellationToken cancellationToken = default);
}