using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.AWS.Options;

namespace eQuantic.Core.CQS.AWS.Outbox;

// ============================================================
// OPTIONS
// ============================================================

// ============================================================
// OUTBOX PUBLISHER
// ============================================================

/// <summary>
/// AWS SQS implementation of IOutboxPublisher
/// </summary>
public sealed class SqsOutboxPublisher : IOutboxPublisher
{
    private readonly IAmazonSQS _sqsClient;
    private readonly SqsOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SqsOutboxPublisher(IAmazonSQS sqsClient, SqsOptions options)
    {
        _sqsClient = sqsClient;
        _options = options;
    }

    public async Task PublishAsync(IOutboxMessage message, CancellationToken ct = default)
    {
        var request = new SendMessageRequest
        {
            QueueUrl = _options.QueueUrl,
            MessageBody = message.Payload,
            DelaySeconds = _options.DelaySeconds,
            MessageAttributes = CreateMessageAttributes(message)
        };

        await _sqsClient.SendMessageAsync(request, ct);
    }

    public async Task PublishBatchAsync(IEnumerable<IOutboxMessage> messages, CancellationToken ct = default)
    {
        var messageList = messages.ToList();
        if (messageList.Count == 0) return;

        // SQS allows max 10 messages per batch
        var batches = messageList
            .Select((msg, idx) => new { msg, idx })
            .GroupBy(x => x.idx / Math.Min(_options.MaxBatchSize, 10))
            .Select(g => g.Select(x => x.msg).ToList());

        foreach (var batch in batches)
        {
            var request = new SendMessageBatchRequest
            {
                QueueUrl = _options.QueueUrl,
                Entries = batch.Select((msg, idx) => new SendMessageBatchRequestEntry
                {
                    Id = idx.ToString(),
                    MessageBody = msg.Payload,
                    DelaySeconds = _options.DelaySeconds,
                    MessageAttributes = CreateMessageAttributes(msg)
                }).ToList()
            };

            var response = await _sqsClient.SendMessageBatchAsync(request, ct);

            if (response.Failed.Count > 0)
            {
                var failures = string.Join(", ", response.Failed.Select(f => $"{f.Id}: {f.Message}"));
                throw new InvalidOperationException($"Failed to send some messages: {failures}");
            }
        }
    }

    private static Dictionary<string, MessageAttributeValue> CreateMessageAttributes(IOutboxMessage message)
    {
        var attributes = new Dictionary<string, MessageAttributeValue>
        {
            ["MessageType"] = new()
            {
                DataType = "String",
                StringValue = message.MessageType
            },
            ["MessageId"] = new()
            {
                DataType = "String",
                StringValue = message.Id.ToString()
            },
            ["CreatedAt"] = new()
            {
                DataType = "String",
                StringValue = message.CreatedAt.ToString("O")
            }
        };

        if (!string.IsNullOrEmpty(message.CorrelationId))
        {
            attributes["CorrelationId"] = new()
            {
                DataType = "String",
                StringValue = message.CorrelationId
            };
        }

        return attributes;
    }
}

// ============================================================
// DEPENDENCY INJECTION
// ============================================================