# eQuantic.Core.CQS.MongoDb

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.MongoDb.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.MongoDb/)

MongoDB provider for eQuantic.Core.CQS - Saga Repository, Outbox, and Job Scheduler implementations.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.MongoDb
```

## Configuration

```csharp
services.AddCQS(options => options
    .FromAssemblyContaining<Program>()
    .UseMongoDb<MySagaData>(mongo =>
    {
        mongo.ConnectionString = "mongodb://localhost:27017";
        mongo.DatabaseName = "myapp";
        mongo.SagasCollection = "sagas";
        mongo.OutboxCollection = "outbox";
        mongo.JobsCollection = "jobs";
    }));
```

## Features

### Saga Repository

Stores saga state in MongoDB collections with BSON serialization.

### Outbox Repository

Stores outbox messages for reliable event publishing.

### Job Scheduler

Schedule deferred command execution.

## Options

| Option             | Description               | Default  |
| ------------------ | ------------------------- | -------- |
| `ConnectionString` | MongoDB connection string | Required |
| `DatabaseName`     | Database name             | `cqs`    |
| `SagasCollection`  | Collection for sagas      | `sagas`  |
| `OutboxCollection` | Collection for outbox     | `outbox` |
| `JobsCollection`   | Collection for jobs       | `jobs`   |

## License

MIT License
