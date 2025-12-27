using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Redis.Options;
using eQuantic.Core.CQS.Redis.Outbox;
using eQuantic.Core.CQS.Redis.Tests.Fixtures;
using eQuantic.Core.CQS.Tests.Commons.Data;
using eQuantic.Core.CQS.Tests.Commons.Fixtures;
using FluentAssertions;
using Xunit;

namespace eQuantic.Core.CQS.Redis.Tests.Integration;

[Collection("Redis")]
public class RedisOutboxRepositoryTests
{
    private readonly RedisContainerFixture _fixture;
    private readonly RedisOutboxRepository _repository;
    private readonly RedisOptions _options;

    public RedisOutboxRepositoryTests(RedisContainerFixture fixture)
    {
        _fixture = fixture;
        _options = new RedisOptions
        {
            ConnectionString = fixture.ConnectionString,
            KeyPrefix = $"test:{Guid.NewGuid():N}:",
            Database = 0,
            DefaultExpiration = TimeSpan.FromMinutes(5)
        };
        _repository = new RedisOutboxRepository(fixture.Connection, _options);
    }

    [DockerAvailableFact]
    public async Task Add_ShouldStoreMessage()
    {
        // Arrange
        var message = CreateTestMessage();

        // Act
        await _repository.Add(message);

        // Assert
        var pending = await _repository.GetPending(10);
        pending.Should().ContainSingle(m => m.Id == message.Id);
    }

    [DockerAvailableFact]
    public async Task GetPending_ShouldReturnMessagesInOrder()
    {
        // Arrange
        var msg1 = CreateTestMessage();
        msg1.CreatedAt = DateTime.UtcNow.AddMinutes(-3);
        var msg2 = CreateTestMessage();
        msg2.CreatedAt = DateTime.UtcNow.AddMinutes(-2);
        var msg3 = CreateTestMessage();
        msg3.CreatedAt = DateTime.UtcNow.AddMinutes(-1);
        
        await _repository.Add(msg1);
        await _repository.Add(msg2);
        await _repository.Add(msg3);

        // Act
        var pending = await _repository.GetPending(10);

        // Assert
        pending.Should().HaveCount(3);
        pending[0].Id.Should().Be(msg1.Id);
        pending[1].Id.Should().Be(msg2.Id);
        pending[2].Id.Should().Be(msg3.Id);
    }

    [DockerAvailableFact]
    public async Task GetPending_ShouldRespectBatchSize()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var msg = CreateTestMessage();
            msg.CreatedAt = DateTime.UtcNow.AddMinutes(-i);
            await _repository.Add(msg);
        }

        // Act
        var pending = await _repository.GetPending(3);

        // Assert
        pending.Should().HaveCount(3);
    }

    [DockerAvailableFact]
    public async Task MarkProcessed_ShouldUpdateStateAndRemoveFromPending()
    {
        // Arrange
        var message = CreateTestMessage();
        await _repository.Add(message);

        // Act
        await _repository.MarkProcessed(message.Id);

        // Assert
        var pending = await _repository.GetPending(10);
        pending.Should().NotContain(m => m.Id == message.Id);
    }

    [DockerAvailableFact]
    public async Task MarkFailed_ShouldUpdateStateAndIncrementAttempts()
    {
        // Arrange
        var message = CreateTestMessage();
        await _repository.Add(message);

        // Act
        await _repository.MarkFailed(message.Id, "Connection timeout");
        await _repository.MarkFailed(message.Id, "Connection timeout again");

        // Assert
        var pending = await _repository.GetPending(10);
        var failedMsg = pending.Should().ContainSingle(m => m.Id == message.Id).Subject;
        failedMsg.State.Should().Be(OutboxMessageState.Failed);
        failedMsg.LastError.Should().Be("Connection timeout again");
        failedMsg.Attempts.Should().Be(2);
    }

    private static TestOutboxMessage CreateTestMessage() => new()
    {
        Id = Guid.NewGuid(),
        MessageType = "TestEvent",
        Payload = "{\"orderId\": \"123\"}",
        State = OutboxMessageState.Pending,
        CreatedAt = DateTime.UtcNow,
        CorrelationId = Guid.NewGuid().ToString()
    };
}
