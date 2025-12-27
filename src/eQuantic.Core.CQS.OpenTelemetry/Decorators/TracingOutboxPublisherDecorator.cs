using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Abstractions.Telemetry;
using eQuantic.Core.CQS.OpenTelemetry.Diagnostics;
using eQuantic.Core.CQS.OpenTelemetry.Options;

namespace eQuantic.Core.CQS.OpenTelemetry.Decorators;

/// <summary>
/// Decorator that adds tracing to outbox publisher operations.
/// Implements OCP - extends functionality without modifying original publisher.
/// Implements DIP - depends on ICqsTelemetry abstraction.
/// </summary>
public sealed class TracingOutboxPublisherDecorator : IOutboxPublisher
{
    private readonly IOutboxPublisher _inner;
    private readonly ICqsTelemetry _telemetry;
    private readonly OpenTelemetryOptions _options;
    
    public TracingOutboxPublisherDecorator(
        IOutboxPublisher inner,
        ICqsTelemetry telemetry,
        OpenTelemetryOptions options)
    {
        _inner = inner;
        _telemetry = telemetry;
        _options = options;
    }
    
    public async Task PublishAsync(IOutboxMessage message, CancellationToken ct = default)
    {
        if (!_options.TraceOutbox)
        {
            await _inner.PublishAsync(message, ct);
            return;
        }
        
        using var _ = _telemetry.StartActivity(
            CqsActivitySource.Operations.Outbox,
            "Publish",
            new Dictionary<string, object?>
            {
                [CqsActivitySource.Tags.OutboxMessageId] = message.Id,
                [CqsActivitySource.Tags.OutboxMessageType] = message.MessageType
            });
        
        try
        {
            await _inner.PublishAsync(message, ct);
            _telemetry.RecordMetric("cqs.outbox.published", 1, new Dictionary<string, object?>
            {
                ["message_type"] = message.MessageType
            });
        }
        catch (Exception ex)
        {
            _telemetry.RecordException(ex);
            _telemetry.RecordMetric("cqs.outbox.failed", 1, new Dictionary<string, object?>
            {
                ["message_type"] = message.MessageType,
                ["error_type"] = ex.GetType().Name
            });
            throw;
        }
    }
    
    public async Task PublishBatchAsync(IEnumerable<IOutboxMessage> messages, CancellationToken ct = default)
    {
        if (!_options.TraceOutbox)
        {
            await _inner.PublishBatchAsync(messages, ct);
            return;
        }
        
        var messageList = messages.ToList();
        
        using var _ = _telemetry.StartActivity(
            CqsActivitySource.Operations.Outbox,
            "PublishBatch",
            new Dictionary<string, object?>
            {
                ["batch_size"] = messageList.Count
            });
        
        try
        {
            await _inner.PublishBatchAsync(messageList, ct);
            _telemetry.RecordMetric("cqs.outbox.published", messageList.Count);
        }
        catch (Exception ex)
        {
            _telemetry.RecordException(ex);
            _telemetry.RecordMetric("cqs.outbox.failed", messageList.Count);
            throw;
        }
    }
}
