# eQuantic.Core.CQS.Redis

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.Redis.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.Redis/)

Redis provider for eQuantic.Core.CQS - Saga Repository, Outbox, and Job Scheduler implementations.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.Redis
```

## Configuration

```csharp
services.AddCQS(options => options
    .FromAssemblyContaining<Program>()
    .UseRedis<MySagaData>(redis =>
    {
        redis.ConnectionString = "localhost:6379";
        redis.KeyPrefix = "myapp:";
        redis.Database = 0;
        redis.DefaultExpiration = TimeSpan.FromDays(30);
    }));
```

## Features

### Saga Repository

Stores saga state in Redis with JSON serialization.

```csharp
public class OrderSaga : Saga<OrderSagaData>
{
    private readonly ISagaRepository<OrderSagaData> _repository;

    // Saga state is automatically persisted to Redis
}
```

### Outbox Repository

Stores outbox messages in Redis sorted sets for ordered processing.

### Job Scheduler

Schedule deferred command execution using Redis.

## Options

| Option              | Description               | Default                |
| ------------------- | ------------------------- | ---------------------- |
| `ConnectionString`  | Redis connection string   | `localhost:6379`       |
| `KeyPrefix`         | Prefix for all Redis keys | `cqs:`                 |
| `Database`          | Redis database number     | `0`                    |
| `DefaultExpiration` | TTL for stored data       | `null` (no expiration) |

## License

MIT License
