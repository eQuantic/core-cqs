namespace eQuantic.Core.CQS.Azure.Options;

/// <summary>
/// Azure Service Bus configuration options
/// </summary>
public class ServiceBusOptions
{
    /// <summary>
    /// Service Bus connection string
    /// </summary>
    public string ConnectionString { get; set; } = "";

    /// <summary>
    /// Queue or Topic name to send messages to
    /// </summary>
    public string QueueOrTopicName { get; set; } = "outbox-messages";

    /// <summary>
    /// If true, sends to a Topic; if false, sends to a Queue
    /// </summary>
    public bool UseTopic { get; set; } = false;

    /// <summary>
    /// Maximum batch size for sending messages (max 100)
    /// </summary>
    public int MaxBatchSize { get; set; } = 100;
}