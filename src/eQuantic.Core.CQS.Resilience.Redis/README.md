# eQuantic.Core.CQS.Resilience.Redis

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.Resilience.Redis.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.Resilience.Redis/)

Redis dead letter handler for eQuantic CQS resilience.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.Resilience.Redis
```

## Configuration

### With existing Redis connection

```csharp
services.AddCqs(options =>
{
    options.UseResilience<MySaga>();
    options.UseRedisDeadLetter(redis =>
    {
        redis.KeyPrefix = "myapp:saga";
        redis.Database = 1;
        redis.Expiry = TimeSpan.FromDays(30);
    });
});
```

### With new connection

```csharp
services.AddCqs(options =>
{
    options.UseRedisDeadLetter("localhost:6379", redis =>
    {
        redis.KeyPrefix = "myapp:saga";
    });
});
```

## Features

- Store failed sagas in Redis list
- Retrieve dead letters for reprocessing
- Configurable expiry
- JSON serialization
