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
    /// Correlation ID for tracing
    /// </summary>
    string? CorrelationId { get; }
}