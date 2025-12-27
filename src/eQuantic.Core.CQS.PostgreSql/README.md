# eQuantic.Core.CQS.PostgreSql

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.PostgreSql.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.PostgreSql/)

PostgreSQL provider for eQuantic.Core.CQS - Saga Repository, Outbox, and Job Scheduler using Dapper.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.PostgreSql
```

## Configuration

```csharp
services.AddCQS(options => options
    .FromAssemblyContaining<Program>()
    .UsePostgreSql<MySagaData>(pg =>
    {
        pg.ConnectionString = "Host=localhost;Database=myapp;Username=user;Password=pass";
        pg.Schema = "cqs";
        pg.SagasTable = "sagas";
        pg.OutboxTable = "outbox";
        pg.JobsTable = "jobs";
    }));
```

## Database Setup

Create the required tables:

```sql
CREATE SCHEMA IF NOT EXISTS cqs;

CREATE TABLE cqs.sagas (
    id UUID PRIMARY KEY,
    type VARCHAR(500) NOT NULL,
    data JSONB NOT NULL,
    state INT NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE cqs.outbox (
    id UUID PRIMARY KEY,
    type VARCHAR(500) NOT NULL,
    payload JSONB NOT NULL,
    state INT NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    processed_at TIMESTAMPTZ NULL
);

CREATE TABLE cqs.jobs (
    id UUID PRIMARY KEY,
    command_type VARCHAR(500) NOT NULL,
    command_data JSONB NOT NULL,
    scheduled_at TIMESTAMPTZ NOT NULL,
    executed_at TIMESTAMPTZ NULL
);
```

## Features

- **Saga Repository**: JSONB storage with efficient querying
- **Outbox Repository**: Transactional outbox pattern support
- **Job Scheduler**: Scheduled command execution

## License

MIT License
