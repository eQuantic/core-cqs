using Dapper;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.PostgreSql.Options;
using eQuantic.Core.CQS.PostgreSql.Outbox;
using eQuantic.Core.CQS.PostgreSql.Tests.Fixtures;
using eQuantic.Core.CQS.Tests.Commons.Data;
using eQuantic.Core.CQS.Tests.Commons.Fixtures;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace eQuantic.Core.CQS.PostgreSql.Tests.Integration;

[Collection("PostgreSql")]
public class PostgreSqlOutboxRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _fixture;
    private readonly PostgreSqlOutboxRepository _repository;
    private readonly PostgreSqlOptions _options;

    public PostgreSqlOutboxRepositoryTests(PostgreSqlContainerFixture fixture)
    {
        _fixture = fixture;
        _options = new PostgreSqlOptions
        {
            ConnectionString = fixture.ConnectionString,
            Schema = "public",
            AutoCreateTables = true
        };
        _repository = new PostgreSqlOutboxRepository(_options);
    }

    /// <summary>
    /// The container is shared by the whole collection, so each test starts from an empty
    /// queue — otherwise a leftover message would sneak into someone else's batch.
    /// </summary>
    public async Task InitializeAsync()
    {
        await using var conn = new NpgsqlConnection(_fixture.ConnectionString);
        await conn.ExecuteAsync("DROP TABLE IF EXISTS public.outbox");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [DockerAvailableFact]
    public async Task Add_ShouldStoreMessageWithItsContext()
    {
        // Arrange
        var message = CreateTestMessage();
        message.Context = """{"actor.id":"42"}""";

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

        // Act — the claim leases the batch, so a concurrent relay comes away empty
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

        // Assert — the state has to be Processed, the one CleanupProcessed goes looking for
        var row = await ReadRow(message.Id);
        row.State.Should().Be((int)OutboxMessageState.Processed);
        row.ProcessedAt.Should().NotBeNull();
        row.NextAttemptAt.Should().BeNull();

        await _repository.CleanupProcessed(TimeSpan.Zero);
        (await CountRows(message.Id)).Should().Be(0);
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

        var row = await ReadRow(message.Id);
        row.State.Should().Be((int)OutboxMessageState.Pending);
        row.Attempts.Should().Be(1);
        row.LastError.Should().Be("Connection timeout");
        row.NextAttemptAt.Should().NotBeNull().And.BeAfter(DateTime.UtcNow);
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

        var row = await ReadRow(message.Id);
        row.State.Should().Be((int)OutboxMessageState.Failed);
        row.Attempts.Should().Be(2);
        row.LastError.Should().Be("Second");
        row.NextAttemptAt.Should().BeNull();
    }

    private static TestOutboxMessage CreateTestMessage(DateTime? createdAt = null) => new()
    {
        Id = Guid.NewGuid(),
        MessageType = "TestEvent",
        Payload = "{\"orderId\": \"123\"}",
        State = OutboxMessageState.Pending,
        CreatedAt = createdAt ?? DateTime.UtcNow,
        CorrelationId = Guid.NewGuid().ToString()
    };

    private async Task<OutboxRow> ReadRow(Guid id)
    {
        await using var conn = new NpgsqlConnection(_fixture.ConnectionString);
        return await conn.QuerySingleAsync<OutboxRow>(
            @"SELECT state, attempts, last_error as LastError, processed_at as ProcessedAt,
                     next_attempt_at as NextAttemptAt
              FROM public.outbox WHERE id = @Id", new { Id = id });
    }

    private async Task<int> CountRows(Guid id)
    {
        await using var conn = new NpgsqlConnection(_fixture.ConnectionString);
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM public.outbox WHERE id = @Id", new { Id = id });
    }

    private sealed record OutboxRow(
        int State, int Attempts, string? LastError, DateTime? ProcessedAt, DateTime? NextAttemptAt);
}
