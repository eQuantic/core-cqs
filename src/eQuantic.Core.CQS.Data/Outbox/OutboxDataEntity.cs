using eQuantic.Core.Data.Repository;

namespace eQuantic.Core.CQS.Data.Outbox;

/// <summary>
/// The outbox message as a native <see cref="IEntity{TKey}" /> — persisted by the eQuantic.Core.Data engine on
/// whichever store is configured. <see cref="State" /> is stored as an <see cref="int" /> (mirroring
/// <see cref="eQuantic.Core.CQS.Abstractions.Outbox.OutboxMessageState" />) so the shape stays portable across
/// relational and document stores alike.
/// </summary>
public sealed class OutboxDataEntity : IEntity<Guid>
{
    /// <summary>The message identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>The message/event type name.</summary>
    public string MessageType { get; set; } = "";

    /// <summary>The serialized payload.</summary>
    public string Payload { get; set; } = "";

    /// <summary>The current state, as <see cref="eQuantic.Core.CQS.Abstractions.Outbox.OutboxMessageState" /> cast to int.</summary>
    public int State { get; set; }

    /// <summary>When the message was created (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the message was last processed (UTC), if ever.</summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>The number of processing attempts.</summary>
    public int Attempts { get; set; }

    /// <summary>The last error, if the message failed.</summary>
    public string? LastError { get; set; }

    /// <summary>A correlation identifier for tracing.</summary>
    public string? CorrelationId { get; set; }

    /// <inheritdoc />
    public Guid GetKey() => Id;

    /// <inheritdoc />
    public void SetKey(Guid key) => Id = key;
}
