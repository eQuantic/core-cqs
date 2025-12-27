# eQuantic.Core.CQS.AWS

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.AWS.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.AWS/)

Amazon SQS integration for eQuantic.Core.CQS Outbox pattern.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.AWS
```

## Configuration

```csharp
services.AddCQS(options => options
    .FromAssemblyContaining<Program>()
    .UseAwsSqs(sqs =>
    {
        sqs.QueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789/my-queue";
        sqs.Region = "us-east-1";
    }));
```

## Features

### Outbox Publisher

Publishes outbox messages to Amazon SQS queues.

```csharp
await _publisher.PublishAsync(message);
```

### Batch Publishing

Supports batch publishing with SQS limit of 10 messages per batch.

```csharp
await _publisher.PublishBatchAsync(messages); // Auto-batches if > 10
```

## Message Attributes

Messages include metadata as SQS message attributes:

- `MessageType`
- `MessageId`
- `CreatedAt`
- `CorrelationId`

## Options

| Option     | Description   | Default                 |
| ---------- | ------------- | ----------------------- |
| `QueueUrl` | SQS queue URL | Required                |
| `Region`   | AWS region    | Uses default SDK region |

## AWS Credentials

Uses the default AWS SDK credential chain:

1. Environment variables
2. AWS credentials file
3. IAM role (for EC2/Lambda)

## License

MIT License
