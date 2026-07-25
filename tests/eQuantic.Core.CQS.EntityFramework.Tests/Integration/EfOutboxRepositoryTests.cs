using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.EntityFramework.Outbox;
using eQuantic.Core.CQS.EntityFramework.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace eQuantic.Core.CQS.EntityFramework.Tests.Integration;

public class EfOutboxRepositoryTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly EfOutboxRepository<TestDbContext> _repository;

    public EfOutboxRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new TestDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new EfOutboxRepository<TestDbContext>(_context);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
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
    public async Task MarkProcessed_ShouldSettleTheMessageAsProcessed()
    {
        // Arrange
        var message = CreateTestMessage();
        await _repository.Add(message);

        // Act
        await _repository.MarkProcessed(message.Id);

        // Assert
        var stored = await Load(message.Id);
        stored!.State.Should().Be((int)OutboxMessageState.Processed);
        stored.ProcessedAt.Should().NotBeNull();
        stored.NextAttemptAt.Should().BeNull();

        await _repository.CleanupProcessed(TimeSpan.Zero);
        (await Load(message.Id)).Should().BeNull();
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

        var stored = await Load(message.Id);
        stored!.State.Should().Be((int)OutboxMessageState.Pending);
        stored.Attempts.Should().Be(1);
        stored.LastError.Should().Be("Connection timeout");
        stored.NextAttemptAt.Should().NotBeNull().And.BeAfter(DateTime.UtcNow);
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
        retried.LastError.Should().Be("Connection timeout");
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

        var stored = await Load(message.Id);
        stored!.State.Should().Be((int)OutboxMessageState.Failed);
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

    private Task<OutboxEntity?> Load(Guid id) =>
        _context.OutboxMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
}
