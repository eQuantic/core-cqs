namespace eQuantic.Core.CQS.Abstractions.Outbox;

/// <summary>
/// Represents a message in the outbox for reliable delivery
/// </summary>
public interface IOutboxMessage
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Type name of the message/event
    /// </summary>
    string MessageType { get; }

    /// <summary>
    /// Serialized message payload
    /// </summary>
    string Payload { get; }

    /// <summary>
    /// Current state
    /// </summary>
    OutboxMessageState State { get; set; }

    /// <summary>
    /// When the message was created
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// When the message was last processed
    /// </summary>
    DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Number of processing attempts
    /// </summary>
    int Attempts { get; set; }

    /// <summary>
    /// Last error message if failed
    /// </summary>
    string? LastError { get; set; }

    /// <summary>
    /// When the message becomes deliverable again after a failed attempt. Null
    /// means immediately. <see cref="IOutboxRepository.GetPending" /> skips
    /// messages whose next attempt is still in the future.
    /// </summary>
    DateTime? NextAttemptAt { get; set; }

    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Ambient context captured when the message was enqueued, as a serialized
    /// payload (JSON by convention).
    /// </summary>
    /// <remarks>
    /// A relay delivers out of band, so anything that exists only during the
    /// originating request — the acting user, its address, a tenant — is gone by
    /// the time a handler runs. Whatever a handler must know about the caller
    /// has to travel with the message, and <see cref="CorrelationId" /> is a
    /// single opaque string: enough to stitch traces together, not enough to
    /// answer "who did this".
    /// </remarks>
    string? Context { get; }
}