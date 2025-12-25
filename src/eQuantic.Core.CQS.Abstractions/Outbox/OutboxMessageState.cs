namespace eQuantic.Core.CQS.Abstractions.Outbox;

/// <summary>
/// State of an outbox message
/// </summary>
public enum OutboxMessageState
{
    /// <summary>
    /// Message is pending processing
    /// </summary>
    Pending,

    /// <summary>
    /// Message is being processed
    /// </summary>
    Processing,

    /// <summary>
    /// Message was processed successfully
    /// </summary>
    Processed,

    /// <summary>
    /// Message processing failed
    /// </summary>
    Failed
}