using eQuantic.Core.CQS.Abstractions;
using eQuantic.Core.CQS.Abstractions.Notifications;
using eQuantic.Core.CQS.Abstractions.Commands;
using eQuantic.Core.CQS.Abstractions.Queries;
using eQuantic.Core.CQS.Abstractions.Handlers;
using eQuantic.Core.CQS.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace eQuantic.Core.CQS.Tests.Integration;

// ============================================================
// INTEGRATION TEST COMMANDS, QUERIES & NOTIFICATIONS
// ============================================================

public record CreateUserCommand(string Name, string Email) : ICommand<Guid>;
public record GetUserQuery(Guid UserId) : IQuery<UserDto>;
public record UserDto(Guid Id, string Name, string Email);
public class UserCreatedNotification : NotificationBase
{
    public Guid UserId { get; }
    public string Email { get; }
    public UserCreatedNotification(Guid userId, string email) => (UserId, Email) = (userId, email);
}

public class UserStore
{
    private readonly Dictionary<Guid, UserDto> _users = new();

    public void Add(Guid id, string name, string email) => _users[id] = new UserDto(id, name, email);
    public UserDto? Get(Guid id) => _users.TryGetValue(id, out var user) ? user : null;
}

public sealed class CreateUserHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly UserStore _store;
    private readonly INotificationPublisher _notificationPublisher;

    public CreateUserHandler(UserStore store, INotificationPublisher notificationPublisher)
    {
        _store = store;
        _notificationPublisher = notificationPublisher;
    }

    public async Task<Guid> Execute(CreateUserCommand command, CancellationToken ct)
    {
        var userId = Guid.NewGuid();
        _store.Add(userId, command.Name, command.Email);
        
        // Publish notification
        await _notificationPublisher.Publish(new UserCreatedNotification(userId, command.Email), ct);
        
        return userId;
    }
}

public sealed class GetUserHandler : IQueryHandler<GetUserQuery, UserDto>
{
    private readonly UserStore _store;

    public GetUserHandler(UserStore store) => _store = store;

    public Task<UserDto> Execute(GetUserQuery query, CancellationToken ct)
    {
        var user = _store.Get(query.UserId) 
            ?? throw new KeyNotFoundException($"User {query.UserId} not found");
        return Task.FromResult(user);
    }
}

public class UserCreatedEmailHandler : INotificationHandler<UserCreatedNotification>
{
    public static List<string> SentEmails { get; } = new();

    public Task Handle(UserCreatedNotification notification, CancellationToken ct)
    {
        SentEmails.Add(notification.Email);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Integration tests for complete CQS workflows
/// </summary>
public class CqsIntegrationTests : IDisposable
{
    private readonly IServiceProvider _provider;
    private readonly IMediator _mediator;

    public CqsIntegrationTests()
    {
        UserCreatedEmailHandler.SentEmails.Clear();

        var services = new ServiceCollection();
        services.AddCQS(options => options.FromAssemblyContaining<CqsIntegrationTests>());
        services.AddSingleton<UserStore>();
        
        _provider = services.BuildServiceProvider();
        _mediator = _provider.GetRequiredService<IMediator>();
    }

    public void Dispose()
    {
        UserCreatedEmailHandler.SentEmails.Clear();
    }

    [Fact]
    public async Task CreateUser_ShouldCreateAndBeQueryable()
    {
        // Arrange
        var command = new CreateUserCommand("John Doe", "john@example.com");

        // Act
        var userId = await _mediator.ExecuteAsync(command);
        var user = await _mediator.ExecuteAsync(new GetUserQuery(userId));

        // Assert
        user.Should().NotBeNull();
        user.Name.Should().Be("John Doe");
        user.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task CreateUser_ShouldPublishNotification()
    {
        // Arrange
        var command = new CreateUserCommand("Jane Smith", "jane@example.com");

        // Act
        await _mediator.ExecuteAsync(command);

        // Assert
        UserCreatedEmailHandler.SentEmails.Should().Contain("jane@example.com");
    }

    [Fact]
    public async Task CreateUser_MultipleUsers_ShouldAllBeQueryable()
    {
        // Arrange
        var commands = new[]
        {
            new CreateUserCommand("User1", "user1@test.com"),
            new CreateUserCommand("User2", "user2@test.com"),
            new CreateUserCommand("User3", "user3@test.com")
        };

        // Act
        var userIds = new List<Guid>();
        foreach (var cmd in commands)
        {
            userIds.Add(await _mediator.ExecuteAsync(cmd));
        }

        // Assert
        foreach (var (userId, index) in userIds.Select((id, i) => (id, i)))
        {
            var user = await _mediator.ExecuteAsync(new GetUserQuery(userId));
            user.Name.Should().Be($"User{index + 1}");
        }
    }

    [Fact]
    public async Task GetNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var query = new GetUserQuery(Guid.NewGuid());

        // Act
        var act = () => _mediator.ExecuteAsync(query);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public void ServiceCollection_ShouldResolveAllCqsServices()
    {
        // Assert
        _provider.GetService<IMediator>().Should().NotBeNull();
        _provider.GetService<INotificationPublisher>().Should().NotBeNull();
    }
}
