using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.PostgreSql.Options;
using eQuantic.Core.CQS.PostgreSql.Sagas;
using eQuantic.Core.CQS.PostgreSql.Tests.Fixtures;
using eQuantic.Core.CQS.Tests.Commons.Data;
using eQuantic.Core.CQS.Tests.Commons.Fixtures;
using FluentAssertions;
using Xunit;

namespace eQuantic.Core.CQS.PostgreSql.Tests.Integration;

[Collection("PostgreSql")]
public class PostgreSqlSagaRepositoryTests
{
    private readonly PostgreSqlContainerFixture _fixture;
    private readonly PostgreSqlSagaRepository<TestSagaData> _repository;
    private readonly PostgreSqlOptions _options;

    public PostgreSqlSagaRepositoryTests(PostgreSqlContainerFixture fixture)
    {
        _fixture = fixture;
        _options = new PostgreSqlOptions
        {
            ConnectionString = fixture.ConnectionString,
            Schema = "public",
            AutoCreateTables = true
        };
        _repository = new PostgreSqlSagaRepository<TestSagaData>(_options);
    }

    [DockerAvailableFact]
    public async Task Save_ShouldStoreSagaData()
    {
        // Arrange
        var saga = new TestSagaData
        {
            SagaId = Guid.NewGuid(),
            OrderId = "ORD-001",
            Amount = 99.99m,
            State = SagaState.InProgress
        };

        // Act
        await _repository.Save(saga);

        // Assert
        var loaded = await _repository.Load(saga.SagaId);
        loaded.Should().NotBeNull();
        loaded!.OrderId.Should().Be(saga.OrderId);
        loaded.Amount.Should().Be(saga.Amount);
        loaded.State.Should().Be(SagaState.InProgress);
    }

    [DockerAvailableFact]
    public async Task Load_WhenNotExists_ShouldReturnNull()
    {
        // Act
        var result = await _repository.Load(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [DockerAvailableFact]
    public async Task Delete_ShouldRemoveSagaData()
    {
        // Arrange
        var saga = new TestSagaData { SagaId = Guid.NewGuid(), OrderId = "ORD-002" };
        await _repository.Save(saga);

        // Act
        await _repository.Delete(saga.SagaId);

        // Assert
        var loaded = await _repository.Load(saga.SagaId);
        loaded.Should().BeNull();
    }

    [DockerAvailableFact]
    public async Task Find_ShouldReturnMatchingSagas()
    {
        // Arrange
        var saga1 = new TestSagaData { SagaId = Guid.NewGuid(), OrderId = "PG-FIND-001", Amount = 100 };
        var saga2 = new TestSagaData { SagaId = Guid.NewGuid(), OrderId = "PG-FIND-002", Amount = 200 };
        var saga3 = new TestSagaData { SagaId = Guid.NewGuid(), OrderId = "PG-OTHER-001", Amount = 150 };
        
        await _repository.Save(saga1);
        await _repository.Save(saga2);
        await _repository.Save(saga3);

        // Act
        var results = await _repository.Find(s => s.OrderId.StartsWith("PG-FIND"));

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(s => s.OrderId == "PG-FIND-001");
        results.Should().Contain(s => s.OrderId == "PG-FIND-002");
    }

    [DockerAvailableFact]
    public async Task GetIncomplete_ShouldReturnOnlyInProgressOrNotStarted()
    {
        // Arrange
        var incomplete = new TestSagaData { SagaId = Guid.NewGuid(), OrderId = "PG-INC", State = SagaState.InProgress };
        var notStarted = new TestSagaData { SagaId = Guid.NewGuid(), OrderId = "PG-NS", State = SagaState.NotStarted };
        var completed = new TestSagaData { SagaId = Guid.NewGuid(), OrderId = "PG-COMP", State = SagaState.Completed };
        
        await _repository.Save(incomplete);
        await _repository.Save(notStarted);
        await _repository.Save(completed);

        // Act
        var results = await _repository.GetIncomplete();

        // Assert
        results.Should().Contain(s => s.SagaId == incomplete.SagaId);
        results.Should().Contain(s => s.SagaId == notStarted.SagaId);
        results.Should().NotContain(s => s.SagaId == completed.SagaId);
    }

    [DockerAvailableFact]
    public async Task Save_ShouldUpdateExistingSaga()
    {
        // Arrange
        var saga = new TestSagaData
        {
            SagaId = Guid.NewGuid(),
            OrderId = "PG-UPDATE-001",
            Amount = 50,
            State = SagaState.NotStarted
        };
        await _repository.Save(saga);

        // Act
        saga.Amount = 100;
        saga.State = SagaState.Completed;
        await _repository.Save(saga);

        // Assert
        var loaded = await _repository.Load(saga.SagaId);
        loaded!.Amount.Should().Be(100);
        loaded.State.Should().Be(SagaState.Completed);
    }
}
