using System.Text.Json;
using Azure.Messaging.ServiceBus;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Azure.Options;

namespace eQuantic.Core.CQS.Azure.Outbox;

// ============================================================
// OPTIONS
// ============================================================

// ============================================================
// OUTBOX PUBLISHER
// ============================================================

/// <summary>
/// Azure Service Bus implementation of IOutboxPublisher
/// </summary>
public sealed class ServiceBusOutboxPublisher(ServiceBusClient client, ServiceBusOptions options)
    : IOutboxPublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender = client.CreateSender(options.QueueOrTopicName);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task PublishAsync(IOutboxMessage message, CancellationToken ct = default)
    {
        var sbMessage = CreateServiceBusMessage(message);
        await _sender.SendMessageAsync(sbMessage, ct);
    }

    public async Task PublishBatchAsync(IEnumerable<IOutboxMessage> messages, CancellationToken ct = default)
    {
        var messageList = messages.ToList();
        if (messageList.Count == 0) return;

        using var batch = await _sender.CreateMessageBatchAsync(ct);

        foreach (var message in messageList)
        {
            var sbMessage = CreateServiceBusMessage(message);
            if (!batch.TryAddMessage(sbMessage))
            {
                // Batch is full, send it and start a new one
                await _sender.SendMessagesAsync(batch, ct);
                using var newBatch = await _sender.CreateMessageBatchAsync(ct);
                if (!newBatch.TryAddMessage(sbMessage))
                {
                    throw new InvalidOperationException($"Message {message.Id} is too large for batch");
                }
            }
        }

        if (batch.Count > 0)
        {
            await _sender.SendMessagesAsync(batch, ct);
        }
    }

    private ServiceBusMessage CreateServiceBusMessage(IOutboxMessage message)
    {
        var sbMessage = new ServiceBusMessage(message.Payload)
        {
            MessageId = message.Id.ToString(),
            ContentType = "application/json",
            Subject = message.MessageType
        };

        if (!string.IsNullOrEmpty(message.CorrelationId))
        {
            sbMessage.CorrelationId = message.CorrelationId;
        }

        sbMessage.ApplicationProperties["messageType"] = message.MessageType;
        sbMessage.ApplicationProperties["createdAt"] = message.CreatedAt.ToString("O");

        return sbMessage;
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
    }
}

// ============================================================
// DEPENDENCY INJECTION
// ============================================================