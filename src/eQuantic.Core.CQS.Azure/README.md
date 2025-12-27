# eQuantic.Core.CQS.Azure

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.Azure.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.Azure/)

Azure Service Bus integration for eQuantic.Core.CQS Outbox pattern.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.Azure
```

## Configuration

```csharp
services.AddCQS(options => options
    .FromAssemblyContaining<Program>()
    .UseAzureServiceBus(sb =>
    {
        sb.ConnectionString = "Endpoint=sb://...";
        sb.QueueOrTopicName = "outbox-messages";
        sb.UseTopic = false; // true for topics
    }));
```

## Features

### Outbox Publisher

Publishes outbox messages to Azure Service Bus queues or topics.

```csharp
public class OrderCreatedHandler : INotificationHandler<OrderCreated>
{
    private readonly IOutboxPublisher _publisher;

    public async Task Handle(OrderCreated notification, CancellationToken ct)
    {
        await _publisher.PublishAsync(new OutboxMessage
        {
            MessageType = "OrderCreated",
            Payload = JsonSerializer.Serialize(notification)
        });
    }
}
```

### Batch Publishing

```csharp
await _publisher.PublishBatchAsync(messages);
```

## Message Properties

Messages include metadata:

- `MessageType` → ApplicationProperty
- `MessageId` → MessageId
- `CreatedAt` → ApplicationProperty
- `CorrelationId` → CorrelationId

## Options

| Option             | Description                   | Default  |
| ------------------ | ----------------------------- | -------- |
| `ConnectionString` | Service Bus connection string | Required |
| `QueueOrTopicName` | Queue or topic name           | Required |
| `UseTopic`         | Use topic instead of queue    | `false`  |

## License

MIT License
