using System.Text.Json;
using Azure.Messaging.ServiceBus;
using eQuantic.Core.CQS.Abstractions.Resilience;
using eQuantic.Core.CQS.Abstractions.Sagas;

namespace eQuantic.Core.CQS.Resilience.ServiceBus;

/// <summary>
/// Azure Service Bus implementation of IDeadLetterHandler.
/// Sends failed sagas to a Service Bus queue for later processing.
/// </summary>
public sealed class ServiceBusDeadLetterHandler : IDeadLetterHandler, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;
    private readonly ServiceBusDeadLetterOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public ServiceBusDeadLetterHandler(ServiceBusClient client, ServiceBusDeadLetterOptions options)
    {
        _options = options;
        _sender = client.CreateSender(options.QueueName);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task HandleAsync(ISagaData saga, string reason, CancellationToken cancellationToken = default)
    {
        var deadLetterEntry = new
        {
            SagaId = saga.SagaId,
            SagaType = saga.GetType().FullName,
            State = saga.State.ToString(),
            Reason = reason,
            FailedAt = DateTime.UtcNow,
            SagaData = saga
        };
        
        var json = JsonSerializer.Serialize(deadLetterEntry, _jsonOptions);
        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            MessageId = $"deadletter-{saga.SagaId}",
            Subject = $"DeadLetter:{saga.GetType().Name}",
            ApplicationProperties =
            {
                ["sagaId"] = saga.SagaId.ToString(),
                ["sagaType"] = saga.GetType().FullName!,
                ["reason"] = reason,
                ["failedAt"] = DateTime.UtcNow.ToString("O")
            }
        };
        
        if (_options.TimeToLive.HasValue)
        {
            message.TimeToLive = _options.TimeToLive.Value;
        }
        
        await _sender.SendMessageAsync(message, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
    }
}

/// <summary>
/// Configuration options for Service Bus dead letter handler.
/// </summary>
public class ServiceBusDeadLetterOptions
{
    public string ConnectionString { get; set; } = "";
    public string QueueName { get; set; } = "cqs-deadletter";
    public TimeSpan? TimeToLive { get; set; }
}
