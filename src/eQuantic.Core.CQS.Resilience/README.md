# eQuantic.Core.CQS.Resilience

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.Resilience.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.Resilience/)

Resilience features for eQuantic CQS. Provides saga timeout monitoring, compensation handling, and dead letter queue support.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.Resilience
```

## Configuration

```csharp
services.AddCqs(options =>
{
    options.UseRedis<OrderSaga>(redis =>
    {
        redis.ConnectionString = "localhost:6379";
    });

    options.UseResilience<OrderSaga>(resilience =>
    {
        resilience.DefaultSagaTimeout = TimeSpan.FromMinutes(30);
        resilience.TimeoutCheckInterval = TimeSpan.FromMinutes(1);
        resilience.EnableCompensationOnTimeout = true;
        resilience.EnableDeadLetterQueue = true;
    });

    // Register compensation handler
    options.WithCompensation<OrderSaga, OrderCompensationHandler>();
});
```

## Options

| Option                        | Type       | Default      | Description                                      |
| ----------------------------- | ---------- | ------------ | ------------------------------------------------ |
| `DefaultSagaTimeout`          | `TimeSpan` | `30 minutes` | Default timeout for sagas                        |
| `TimeoutCheckInterval`        | `TimeSpan` | `1 minute`   | Interval for checking saga timeouts              |
| `MaxRetryAttempts`            | `int`      | `3`          | Maximum retry attempts for failed sagas          |
| `EnableCompensationOnTimeout` | `bool`     | `true`       | Enable automatic compensation on timeout         |
| `EnableDeadLetterQueue`       | `bool`     | `true`       | Enable dead letter queue for unrecoverable sagas |

## Custom Compensation Handler

```csharp
public class OrderCompensationHandler : ICompensationHandler<OrderSaga>
{
    public async Task CompensateAsync(OrderSaga saga, Exception? exception, CancellationToken ct)
    {
        // Rollback order, refund payment, notify customer, etc.
    }
}
```

## Delegate-based Compensation

```csharp
options.WithCompensation<OrderSaga>(async (saga, ex, ct) =>
{
    // Inline compensation logic
    await RefundPaymentAsync(saga.PaymentId, ct);
});
```

## Custom Dead Letter Handler

```csharp
public class QueueDeadLetterHandler : IDeadLetterHandler
{
    public async Task HandleAsync(ISagaData saga, string reason, CancellationToken ct)
    {
        // Send to Azure Service Bus, RabbitMQ, etc.
    }
}

// Register
options.WithDeadLetterHandler<QueueDeadLetterHandler>();
```
