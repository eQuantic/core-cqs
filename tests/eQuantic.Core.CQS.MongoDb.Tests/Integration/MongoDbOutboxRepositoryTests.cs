using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.MongoDb.Options;
using eQuantic.Core.CQS.MongoDb.Outbox;
using eQuantic.Core.CQS.MongoDb.Tests.Fixtures;
using eQuantic.Core.CQS.Tests.Commons.Fixtures;
using FluentAssertions;
using MongoDB.Driver;
using Xunit;

namespace eQuantic.Core.CQS.MongoDb.Tests.Integration;

[Collection("MongoDB")]
public class MongoDbOutboxRepositoryTests : IAsyncLifetime
{
    private const string CollectionName = "test_outbox";

    private readonly MongoContainerFixture _fixture;
    private readonly MongoDbOutboxRepository _repository;

    public MongoDbOutboxRepositoryTests(MongoContainerFixture fixture)
    {
        _fixture = fixture;
        _repository = new MongoDbOutboxRepository(fixture.Client, new MongoDbOptions
        {
            DatabaseName = "test_db",
            CollectionPrefix = "test_"
        });
    }

    /// <summary>
    /// The container is shared by the whole collection, so each test starts from an empty
    /// queue — otherwise a leftover message would sneak into someone else's batch.
    /// </summary>
    public Task InitializeAsync() => _fixture.Database.DropCollectionAsync(CollectionName);

    public Task DisposeAsync() => Task.CompletedTask;

    [DockerAvailableFact]
    public async Task Add_ShouldStoreMessageWithItsContext()
    {
        // Arrange
        var message = CreateTestMessage(context: """{"actor.id":"42"}""");

        // Act
        await _repository.Add(message);

        // Assert
        var pending = await _repository.GetPending(10);
        var stored = pending.Should().ContainSingle(m => m.Id == message.Id).Subject;
        stored.MessageType.Should().Be(message.MessageType);
        stored.Payload.Should().Be(message.Payload);
        stored.State.Should().Be(OutboxMessageState.Pending);
        stored.CorrelationId.Should().Be(message.CorrelationId);
        stored.Context.Should().Be("""{"actor.id":"42"}""");
    }

    [DockerAvailableFact]
    public async Task GetPending_ShouldReturnOldestFirstWithinBatchSize()
    {
        // Arrange
        var oldest = CreateTestMessage(DateTime.UtcNow.AddMinutes(-3));
        var middle = CreateTestMessage(DateTime.UtcNow.AddMinutes(-2));
        var newest = CreateTestMessage(DateTime.UtcNow.AddMinutes(-1));
        await _repository.Add(newest);
        await _repository.Add(oldest);
        await _repository.Add(middle);

        // Act
        var pending = await _repository.GetPending(2);

        // Assert
        pending.Should().HaveCount(2);
        pending[0].Id.Should().Be(oldest.Id);
        pending[1].Id.Should().Be(middle.Id);
    }

    [DockerAvailableFact]
    public async Task GetPending_ShouldNotHandTheSameMessageToASecondRelay()
    {
        // Arrange
        var message = CreateTestMessage();
        await _repository.Add(message);

        // Act — the claim leases the message, so a concurrent relay comes away empty
        var first = await _repository.GetPending(10);
        var second = await _repository.GetPending(10);

        // Assert
        first.Should().ContainSingle(m => m.Id == message.Id);
        second.Should().BeEmpty();
    }

    [DockerAvailableFact]
    public async Task MarkProcessed_ShouldSettleTheMessageAsProcessed()
    {
        // Arrange
        var message = CreateTestMessage();
        await _repository.Add(message);

        // Act
        await _repository.MarkProcessed(message.Id);

        // Assert
        var stored = await Load(message.Id);
        stored!.State.Should().Be(OutboxMessageState.Processed);
        stored.ProcessedAt.Should().NotBeNull();
        stored.NextAttemptAt.Should().BeNull();

        await _repository.CleanupProcessed(TimeSpan.Zero);
        (await Load(message.Id)).Should().BeNull();
    }

    [DockerAvailableFact]
    public async Task MarkFailed_WithAttemptsRemaining_ShouldHoldMessageBackUntilBackoffElapses()
    {
        // Arrange
        var message = CreateTestMessage();
        await _repository.Add(message);

        // Act
        await _repository.MarkFailed(message.Id, "Connection timeout", maxAttempts: 3, backoff: TimeSpan.FromMinutes(5));

        // Assert — still pending, but not yet due
        (await _repository.GetPending(10)).Should().BeEmpty();

        var stored = await Load(message.Id);
        stored!.State.Should().Be(OutboxMessageState.Pending);
        stored.Attempts.Should().Be(1);
        stored.LastError.Should().Be("Connection timeout");
        stored.NextAttemptAt.Should().NotBeNull().And.BeAfter(DateTime.UtcNow);
    }

    [DockerAvailableFact]
    public async Task MarkFailed_WithElapsedBackoff_ShouldOfferMessageAgain()
    {
        // Arrange
        var message = CreateTestMessage();
        await _repository.Add(message);

        // Act
        await _repository.MarkFailed(message.Id, "Connection timeout", maxAttempts: 3, backoff: TimeSpan.Zero);

        // Assert
        var pending = await _repository.GetPending(10);
        var retried = pending.Should().ContainSingle(m => m.Id == message.Id).Subject;
        retried.State.Should().Be(OutboxMessageState.Pending);
        retried.Attempts.Should().Be(1);
        retried.LastError.Should().Be("Connection timeout");
    }

    [DockerAvailableFact]
    public async Task MarkFailed_WithAttemptsExhausted_ShouldDeadLetterMessage()
    {
        // Arrange
        var message = CreateTestMessage();
        await _repository.Add(message);

        // Act
        await _repository.MarkFailed(message.Id, "First", maxAttempts: 2, backoff: TimeSpan.Zero);
        await _repository.MarkFailed(message.Id, "Second", maxAttempts: 2, backoff: TimeSpan.Zero);

        // Assert — off the queue, kept for inspection
        (await _repository.GetPending(10)).Should().BeEmpty();

        var stored = await Load(message.Id);
        stored!.State.Should().Be(OutboxMessageState.Failed);
        stored.Attempts.Should().Be(2);
        stored.LastError.Should().Be("Second");
        stored.NextAttemptAt.Should().BeNull();
    }

    private static OutboxMessage CreateTestMessage(DateTime? createdAt = null, string? context = null) => new()
    {
        MessageType = "TestEvent",
        Payload = "{\"orderId\": \"123\"}",
        CreatedAt = createdAt ?? DateTime.UtcNow,
        CorrelationId = Guid.NewGuid().ToString(),
        Context = context
    };

    private async Task<OutboxMessage?> Load(Guid id) =>
        await _fixture.Database.GetCollection<OutboxMessage>(CollectionName)
            .Find(Builders<OutboxMessage>.Filter.Eq(x => x.Id, id))
            .FirstOrDefaultAsync();
}
