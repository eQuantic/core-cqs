using System.Diagnostics;

namespace eQuantic.Core.CQS.OpenTelemetry.Diagnostics;

/// <summary>
/// Provides the ActivitySource for CQS tracing.
/// Single Responsibility: manages activity source lifecycle.
/// </summary>
public static class CqsActivitySource
{
    /// <summary>
    /// The name of the activity source.
    /// </summary>
    public const string SourceName = "eQuantic.Core.CQS";
    
    /// <summary>
    /// Gets the activity source for CQS operations.
    /// </summary>
    public static ActivitySource Source { get; private set; } = new(SourceName);
    
    /// <summary>
    /// Initializes the activity source with the specified version.
    /// </summary>
    public static void Initialize(string? version = null)
    {
        Source = new ActivitySource(SourceName, version);
    }
    
    /// <summary>
    /// Operation types for categorizing activities.
    /// </summary>
    public static class Operations
    {
        public const string Command = "Command";
        public const string Query = "Query";
        public const string Saga = "Saga";
        public const string Outbox = "Outbox";
        public const string Notification = "Notification";
    }
    
    /// <summary>
    /// Standard tag names for CQS activities.
    /// </summary>
    public static class Tags
    {
        public const string OperationType = "cqs.operation.type";
        public const string OperationName = "cqs.operation.name";
        public const string CommandType = "cqs.command.type";
        public const string QueryType = "cqs.query.type";
        public const string SagaId = "cqs.saga.id";
        public const string SagaType = "cqs.saga.type";
        public const string SagaState = "cqs.saga.state";
        public const string OutboxMessageId = "cqs.outbox.message_id";
        public const string OutboxMessageType = "cqs.outbox.message_type";
        public const string Success = "cqs.success";
        public const string ErrorType = "cqs.error.type";
    }
}
