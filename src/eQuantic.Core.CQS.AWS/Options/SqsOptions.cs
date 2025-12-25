namespace eQuantic.Core.CQS.AWS;

/// <summary>
/// AWS SQS configuration options
/// </summary>
public class SqsOptions
{
    /// <summary>
    /// SQS Queue URL
    /// </summary>
    public string QueueUrl { get; set; } = "";

    /// <summary>
    /// AWS Region (if not using Queue URL with region embedded)
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Maximum batch size for sending messages (AWS limit is 10)
    /// </summary>
    public int MaxBatchSize { get; set; } = 10;

    /// <summary>
    /// Message delay in seconds (0-900)
    /// </summary>
    public int DelaySeconds { get; set; } = 0;
}