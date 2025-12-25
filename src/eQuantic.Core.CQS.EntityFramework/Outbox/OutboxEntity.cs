namespace eQuantic.Core.CQS.EntityFramework.Outbox;

/// <summary>
/// EF Core entity for Outbox messages
/// </summary>
public class OutboxEntity
{
    public Guid Id { get; set; }
    public string MessageType { get; set; } = "";
    public string Payload { get; set; } = "";
    public int State { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public string? CorrelationId { get; set; }
}