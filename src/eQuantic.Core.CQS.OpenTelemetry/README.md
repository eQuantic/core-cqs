# eQuantic.Core.CQS.OpenTelemetry

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.OpenTelemetry.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.OpenTelemetry/)

OpenTelemetry integration for eQuantic CQS. Provides distributed tracing and metrics for commands, queries, sagas, and outbox operations.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.OpenTelemetry
```

## Configuration

```csharp
services.AddCqs(options =>
{
    options.UseRedis<OrderSaga>(redis =>
    {
        redis.ConnectionString = "localhost:6379";
    });

    options.UseOpenTelemetry<OrderSaga>(otel =>
    {
        otel.ServiceName = "OrderService";
        otel.ServiceVersion = "1.0.0";
        otel.TraceCommands = true;
        otel.TraceQueries = true;
        otel.TraceSagas = true;
        otel.TraceOutbox = true;
        otel.RecordExceptions = true;
        otel.CollectMetrics = true;
    });
});
```

## Options

| Option             | Type      | Default        | Description                       |
| ------------------ | --------- | -------------- | --------------------------------- |
| `ServiceName`      | `string`  | `"CqsService"` | Name of the service for telemetry |
| `ServiceVersion`   | `string?` | `null`         | Version of the service            |
| `TraceCommands`    | `bool`    | `true`         | Trace command execution           |
| `TraceQueries`     | `bool`    | `true`         | Trace query execution             |
| `TraceSagas`       | `bool`    | `true`         | Trace saga operations             |
| `TraceOutbox`      | `bool`    | `true`         | Trace outbox operations           |
| `RecordExceptions` | `bool`    | `true`         | Record exceptions in traces       |
| `CollectMetrics`   | `bool`    | `true`         | Collect metrics                   |

## Traces

All traces use the activity source `eQuantic.Core.CQS` with the following tags:

- `cqs.operation.type` - Operation type (Command, Query, Saga, Outbox)
- `cqs.operation.name` - Operation name
- `cqs.command.type` - Command type name
- `cqs.query.type` - Query type name
- `cqs.saga.id` - Saga ID
- `cqs.saga.type` - Saga type name
- `cqs.saga.state` - Saga state
- `cqs.outbox.message_id` - Outbox message ID
- `cqs.outbox.message_type` - Outbox message type

## Metrics

Available metrics:

- `cqs.outbox.published` - Number of outbox messages published
- `cqs.outbox.failed` - Number of failed outbox publish attempts
