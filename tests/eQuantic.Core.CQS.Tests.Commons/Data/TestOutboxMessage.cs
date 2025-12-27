using eQuantic.Core.CQS.Abstractions.Outbox;

namespace eQuantic.Core.CQS.Tests.Commons.Data;

/// <summary>
/// Test outbox message for integration tests.
/// </summary>
public class TestOutboxMessage : IOutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MessageType { get; set; } = "TestMessage";
    public string Payload { get; set; } = "{}";
    public OutboxMessageState State { get; set; } = OutboxMessageState.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public string? CorrelationId { get; set; }
}
