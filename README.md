# eQuantic.Core.CQS

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS/)
[![Build Status](https://github.com/eQuantic/core-cqs/workflows/CI%2FCD/badge.svg)](https://github.com/eQuantic/core-cqs/actions)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A modern, high-performance CQS/CQRS framework for .NET with pipeline behaviors, notifications, streaming, and source generators. MIT-licensed alternative to MediatR.

## Features

- ✅ **Commands & Queries** - Clear separation of read and write operations
- ✅ **Pipeline Behaviors** - Cross-cutting concerns (logging, validation, etc.)
- ✅ **Notifications** - Publish-subscribe pattern with multiple handlers
- ✅ **Streaming** - IAsyncEnumerable support for large datasets
- ✅ **Paged Queries** - Built-in pagination support
- ✅ **Pre/Post Processors** - Hook into request lifecycle
- ✅ **Modern .NET** - Targets .NET 6, 8, and 9

## Installation

```bash
dotnet add package eQuantic.Core.CQS
```

## Quick Start

### 1. Register Services

```csharp
builder.Services.AddCQS(options =>
{
    options.UsePreProcessor = true;
    options.UsePostProcessor = true;
}, typeof(Program).Assembly);
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
        // Your logic here
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
    {
        return await _mediator.ExecuteAsync(new GetUserByIdQuery(id));
    }
}
```

## Advanced Features

### Commands with Results

```csharp
public record CreateUserCommand(string Name) : ICommand<Guid>;
```

### Notifications

```csharp
public record UserCreatedNotification(Guid UserId) : INotification;

public class SendWelcomeEmailHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task Handle(UserCreatedNotification notification, CancellationToken ct)
    {
        // Send email
    }
}

// Publish
await _notificationPublisher.Publish(new UserCreatedNotification(userId));
```

### Streaming

```csharp
public record GetAllUsersStreamQuery : IStreamQuery<UserDto>;

// Handler returns IAsyncEnumerable<UserDto>
var users = _mediator.ExecuteStreamAsync(new GetAllUsersStreamQuery());
await foreach (var user in users)
{
    Console.WriteLine(user.Name);
}
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
| Paged Queries      | Built-in ✅         | Manual           |
| Streaming          | IAsyncEnumerable ✅ | IAsyncEnumerable |
| Notifications      | ✅                  | ✅               |
| Pipeline Behaviors | ✅                  | ✅               |

## License

MIT License - See [LICENSE](LICENSE) for details.
