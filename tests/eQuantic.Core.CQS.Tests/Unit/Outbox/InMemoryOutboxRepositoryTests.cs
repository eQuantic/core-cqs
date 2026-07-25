using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Outbox;
using FluentAssertions;
using Xunit;

namespace eQuantic.Core.CQS.Tests.Unit.Outbox;

public class InMemoryOutboxRepositoryTests
{
    private readonly InMemoryOutboxRepository _repository = new();

    [Fact]
    public async Task Enqueue_ShouldCarryContextThroughToTheStoredMessage()
    {
        // Arrange
        var outbox = new eQuantic.Core.CQS.Outbox.Outbox(_repository);

        // Act
        await outbox.Enqueue(new { OrderId = "ORD-1" }, correlationId: "corr-1", context: """{"actor.id":"42"}""");

        // Assert
        var pending = await _repository.GetPending(10);
        var stored = pending.Should().ContainSingle().Subject;
        stored.CorrelationId.Should().Be("corr-1");
        stored.Context.Should().Be("""{"actor.id":"42"}""");
        stored.Payload.Should().Contain("ORD-1");
    }

    [Fact]
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

    [Fact]
    public async Task MarkFailed_WithAttemptsRemaining_ShouldHoldMessageBackUntilBackoffElapses()
    {
        // Arrange
        var message = CreateTestMessage();
        await _repository.Add(message);

        // Act
        await _repository.MarkFailed(message.Id, "Connection timeout", maxAttempts: 3, backoff: TimeSpan.FromMinutes(5));

        // Assert — still pending, but not yet due
        (await _repository.GetPending(10)).Should().BeEmpty();
        message.State.Should().Be(OutboxMessageState.Pending);
        message.Attempts.Should().Be(1);
        message.LastError.Should().Be("Connection timeout");
        message.NextAttemptAt.Should().NotBeNull().And.BeAfter(DateTime.UtcNow);
    }

    [Fact]
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
    }

    [Fact]
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
        message.State.Should().Be(OutboxMessageState.Failed);
        message.Attempts.Should().Be(2);
        message.LastError.Should().Be("Second");
        message.NextAttemptAt.Should().BeNull();
    }

    [Fact]
    public async Task MarkProcessed_ShouldSettleTheMessageAsProcessed()
    {
        // Arrange
        var message = CreateTestMessage();
        await _repository.Add(message);

        // Act
        await _repository.MarkProcessed(message.Id);

        // Assert
        (await _repository.GetPending(10)).Should().BeEmpty();
        message.State.Should().Be(OutboxMessageState.Processed);

        await _repository.CleanupProcessed(TimeSpan.Zero);
        (await _repository.GetPending(10)).Should().BeEmpty();
    }

    private static OutboxMessage CreateTestMessage(DateTime? createdAt = null) => new()
    {
        MessageType = "TestEvent",
        Payload = "{\"orderId\": \"123\"}",
        CreatedAt = createdAt ?? DateTime.UtcNow
    };
}
