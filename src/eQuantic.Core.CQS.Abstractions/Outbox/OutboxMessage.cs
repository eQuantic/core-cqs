namespace eQuantic.Core.CQS.Abstractions.Outbox;

/// <summary>
/// Default implementation of IOutboxMessage
/// </summary>
public sealed class OutboxMessage : IOutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string MessageType { get; init; }
    public required string Payload { get; init; }
    public OutboxMessageState State { get; set; } = OutboxMessageState.Pending;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public string? CorrelationId { get; init; }
}
