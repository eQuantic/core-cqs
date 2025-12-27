# eQuantic.Core.CQS.Abstractions

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.Abstractions.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.Abstractions/)

Interfaces and abstractions for the eQuantic.Core.CQS framework.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.Abstractions
```

## When to Use

Use this package when you want to:

- Define commands, queries, and handlers in a separate assembly
- Create provider implementations without depending on the full CQS package
- Build libraries that integrate with eQuantic.Core.CQS

## Key Interfaces

### Commands & Queries

```csharp
// Command without result
public record MyCommand(string Data) : ICommand;

// Command with result
public record CreateCommand(string Name) : ICommand<Guid>;

// Query with result
public record GetByIdQuery(Guid Id) : IQuery<MyDto>;
```

### Handlers

```csharp
public class MyCommandHandler : ICommandHandler<MyCommand>
{
    public Task Execute(MyCommand command, CancellationToken ct) { }
}

public class MyQueryHandler : IQueryHandler<GetByIdQuery, MyDto>
{
    public Task<MyDto> Execute(GetByIdQuery query, CancellationToken ct) { }
}
```

### Sagas

```csharp
public class MySagaData : ISagaData
{
    public Guid SagaId { get; set; }
    public SagaState State { get; set; }
    // Custom data
}
```

### Outbox

```csharp
public interface IOutboxRepository
{
    Task Add(IOutboxMessage message, CancellationToken ct);
    Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize, CancellationToken ct);
    Task MarkProcessed(Guid messageId, CancellationToken ct);
}
```

## License

MIT License - See [LICENSE](../../LICENSE) for details.
