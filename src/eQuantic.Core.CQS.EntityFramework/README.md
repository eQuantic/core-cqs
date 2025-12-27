# eQuantic.Core.CQS.EntityFramework

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.EntityFramework.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.EntityFramework/)

Entity Framework Core provider for eQuantic.Core.CQS - Uses your existing DbContext.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.EntityFramework
```

## Configuration

### 1. Implement ICqsDbContext

```csharp
public class AppDbContext : DbContext, ICqsDbContext
{
    public DbSet<SagaEntity> Sagas { get; set; }
    public DbSet<OutboxEntity> Outbox { get; set; }
    public DbSet<JobEntity> Jobs { get; set; }

    // Your other entities...
}
```

### 2. Register Services

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

services.AddCQS(options => options
    .FromAssemblyContaining<Program>()
    .UseEntityFramework<MySagaData, AppDbContext>());
```

## Features

- **Saga Repository**: Leverages EF Core for saga persistence
- **Outbox Repository**: Transactional outbox with your DbContext
- **Job Scheduler**: Scheduled jobs stored in your database

## Benefits

- Uses your existing DbContext and migrations
- Transactional consistency with your domain entities
- Supports all EF Core providers (SQL Server, PostgreSQL, MySQL, SQLite)

## License

MIT License
