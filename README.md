# eQuantic.Core.CQS

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS/)
[![Build Status](https://github.com/eQuantic/core-cqs/workflows/CI%2FCD/badge.svg)](https://github.com/eQuantic/core-cqs/actions)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A modern, high-performance CQS/CQRS framework for .NET with pipeline behaviors, notifications, streaming, sagas, and distributed system support. MIT-licensed alternative to MediatR.

## Features

- ✅ **Commands & Queries** - Clear separation of read and write operations
- ✅ **Pipeline Behaviors** - Cross-cutting concerns (logging, validation, etc.)
- ✅ **Notifications** - Publish-subscribe pattern with multiple handlers
- ✅ **Streaming** - IAsyncEnumerable support for large datasets
- ✅ **Sagas** - Multi-step transactions with compensation
- ✅ **Outbox Pattern** - Reliable message publishing
- ✅ **Job Scheduling** - Deferred command execution
- ✅ **Paged Queries** - Built-in pagination support
- ✅ **Modern .NET** - Targets .NET 6, 8, and 9

## Packages

| Package                          | Description              | NuGet                                                                                                                                         |
| -------------------------------- | ------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------- |
| `eQuantic.Core.CQS`              | Core framework           | [![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS/)                           |
| `eQuantic.Core.CQS.Abstractions` | Interfaces and contracts | [![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.Abstractions.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.Abstractions/) |
| `eQuantic.Core.CQS.Generators`   | Source generators        | [![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.Generators.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.Generators/)     |

### Persistence Providers

| Package                             | Provider   | Features                               |
| ----------------------------------- | ---------- | -------------------------------------- |
| `eQuantic.Core.CQS.Redis`           | Redis      | Saga Repository, Outbox, Job Scheduler |
| `eQuantic.Core.CQS.MongoDb`         | MongoDB    | Saga Repository, Outbox, Job Scheduler |
| `eQuantic.Core.CQS.PostgreSql`      | PostgreSQL | Saga Repository, Outbox, Job Scheduler |
| `eQuantic.Core.CQS.EntityFramework` | EF Core    | Saga Repository, Outbox, Job Scheduler |

### Cloud Messaging

| Package                   | Provider          | Features                       |
| ------------------------- | ----------------- | ------------------------------ |
| `eQuantic.Core.CQS.Azure` | Azure Service Bus | Outbox Publisher (Queue/Topic) |
| `eQuantic.Core.CQS.AWS`   | Amazon SQS        | Outbox Publisher               |

### Telemetry Providers

| Package                                 | Provider           | Features                     |
| --------------------------------------- | ------------------ | ---------------------------- |
| `eQuantic.Core.CQS.OpenTelemetry`       | OpenTelemetry      | Distributed tracing, metrics |
| `eQuantic.Core.CQS.ApplicationInsights` | Azure App Insights | Distributed tracing, metrics |
| `eQuantic.Core.CQS.Datadog`             | Datadog APM        | Distributed tracing          |

### Resilience Providers

| Package                                   | Provider          | Features                   |
| ----------------------------------------- | ----------------- | -------------------------- |
| `eQuantic.Core.CQS.Resilience`            | Default           | Saga timeout, compensation |
| `eQuantic.Core.CQS.Polly`                 | Polly             | Retry, circuit breaker     |
| `eQuantic.Core.CQS.Resilience.Redis`      | Redis             | Dead letter queue          |
| `eQuantic.Core.CQS.Resilience.ServiceBus` | Azure Service Bus | Dead letter queue          |

## Installation

```bash
# Core package
dotnet add package eQuantic.Core.CQS

# Optional: Provider packages
dotnet add package eQuantic.Core.CQS.Redis
dotnet add package eQuantic.Core.CQS.Azure
```

## Quick Start

### 1. Register Services

```csharp
// Basic setup
services.AddCQS(options => options
    .FromAssemblyContaining<Program>());

// With providers
services.AddCQS(options => options
    .FromAssemblyContaining<Program>()
    .UsePreProcessor = true;
    .UseRedis(redis => redis.ConnectionString = "localhost:6379")
    .UseAzureServiceBus(sb => {
        sb.ConnectionString = "Endpoint=sb://...";
        sb.QueueOrTopicName = "outbox";
    }));
```

### 2. Define a Query

```csharp
public record GetUserByIdQuery(Guid Id) : IQuery<UserDto>;
```

### 3. Create a Handler

```csharp
public class GetUserByIdHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Execute(GetUserByIdQuery query, CancellationToken ct)
    {
        return new UserDto(query.Id, "John Doe");
    }
}
```

### 4. Execute via Mediator

```csharp
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet("{id}")]
    public async Task<UserDto> Get(Guid id)
        => await _mediator.ExecuteAsync(new GetUserByIdQuery(id));
}
```

## Advanced Features

### Commands

```csharp
// Command without result
public record DeleteUserCommand(Guid Id) : ICommand;

// Command with result
public record CreateUserCommand(string Name) : ICommand<Guid>;
```

### Notifications (Pub/Sub)

```csharp
public record UserCreatedNotification(Guid UserId) : INotification;

public class SendWelcomeEmailHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task Handle(UserCreatedNotification notification, CancellationToken ct)
    {
        // Send email
    }
}

// Publish to all handlers
await _notificationPublisher.Publish(new UserCreatedNotification(userId));
```

### Streaming

```csharp
public record GetAllUsersStreamQuery : IStreamQuery<UserDto>;

await foreach (var user in _mediator.ExecuteStreamAsync(new GetAllUsersStreamQuery()))
{
    Console.WriteLine(user.Name);
}
```

### Sagas (Multi-step Transactions)

```csharp
public class OrderSaga : Saga<OrderSagaData>
{
    protected override void ConfigureSteps()
    {
        Step("ProcessPayment",
            execute: async (data, ct) => { /* charge */ },
            compensate: async (data, ct) => { /* refund */ });

        Step("ReserveInventory",
            execute: async (data, ct) => { /* reserve */ },
            compensate: async (data, ct) => { /* release */ });

        Step("Ship",
            execute: async (data, ct) => { /* ship */ },
            compensate: async (data, ct) => { /* cancel shipment */ });
    }
}

// Execute - automatic compensation on failure
var result = await saga.Execute(new OrderSagaData { OrderId = orderId });
if (!result.IsSuccess)
{
    Console.WriteLine($"Saga failed: {result.Error?.Message}");
}
```

### Azure Service Bus Integration

```csharp
services.AddCQSAzureServiceBus(options =>
{
    options.ConnectionString = "Endpoint=sb://...";
    options.QueueOrTopicName = "outbox-messages";
    options.UseTopic = false;
});

// Publish outbox messages
await _outboxPublisher.PublishAsync(message);
await _outboxPublisher.PublishBatchAsync(messages);
```

### AWS SQS Integration

```csharp
services.AddCQSAwsSqs(options =>
{
    options.QueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789/my-queue";
    options.Region = "us-east-1";
});
```

### Pipeline Behaviors

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Execute(TRequest request, CancellationToken ct, HandlerDelegate<TResponse> next)
    {
        _logger.LogInformation("Handling {Request}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {Request}", typeof(TRequest).Name);
        return response;
    }
}
```

## Comparison with MediatR

| Feature            | eQuantic.Core.CQS   | MediatR          |
| ------------------ | ------------------- | ---------------- |
| License            | MIT ✅              | Commercial 💰    |
| Sagas              | Built-in ✅         | ❌               |
| Outbox Pattern     | Built-in ✅         | ❌               |
| Cloud Messaging    | Azure/AWS ✅        | ❌               |
| Paged Queries      | Built-in ✅         | Manual           |
| Streaming          | IAsyncEnumerable ✅ | IAsyncEnumerable |
| Notifications      | ✅                  | ✅               |
| Pipeline Behaviors | ✅                  | ✅               |

## License

MIT License - See [LICENSE](LICENSE) for details.
