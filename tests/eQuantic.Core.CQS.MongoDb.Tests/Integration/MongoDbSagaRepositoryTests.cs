using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.MongoDb.Options;
using eQuantic.Core.CQS.MongoDb.Sagas;
using eQuantic.Core.CQS.MongoDb.Tests.Fixtures;
using eQuantic.Core.CQS.Tests.Commons.Fixtures;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Xunit;

namespace eQuantic.Core.CQS.MongoDb.Tests.Integration;

/// <summary>MongoDB-specific test saga data - MongoDB generates its own _id</summary>
[BsonIgnoreExtraElements]
public class MongoTestSagaData : ISagaData
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    
    public Guid SagaId { get; set; } = Guid.NewGuid();
    public SagaState State { get; set; } = SagaState.NotStarted;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int CurrentStep { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

[Collection("MongoDB")]
public class MongoDbSagaRepositoryTests
{
    private readonly MongoContainerFixture _fixture;
    private readonly MongoDbSagaRepository<MongoTestSagaData> _repository;

    public MongoDbSagaRepositoryTests(MongoContainerFixture fixture)
    {
        _fixture = fixture;
        var options = new MongoDbOptions
        {
            DatabaseName = "test_db",
            CollectionPrefix = $"test_{Guid.NewGuid():N}_"
        };
        _repository = new MongoDbSagaRepository<MongoTestSagaData>(fixture.Client, options);
    }

    [DockerAvailableFact]
    public async Task Save_ShouldStoreSagaData()
    {
        // Arrange
        var saga = new MongoTestSagaData
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
        var saga = new MongoTestSagaData { SagaId = Guid.NewGuid(), OrderId = "ORD-002" };
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
        var saga1 = new MongoTestSagaData { SagaId = Guid.NewGuid(), OrderId = "FIND-001", Amount = 100 };
        var saga2 = new MongoTestSagaData { SagaId = Guid.NewGuid(), OrderId = "FIND-002", Amount = 200 };
        var saga3 = new MongoTestSagaData { SagaId = Guid.NewGuid(), OrderId = "OTHER-001", Amount = 150 };
        
        await _repository.Save(saga1);
        await _repository.Save(saga2);
        await _repository.Save(saga3);

        // Act
        var results = await _repository.Find(s => s.OrderId.StartsWith("FIND"));

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(s => s.OrderId == "FIND-001");
        results.Should().Contain(s => s.OrderId == "FIND-002");
    }

    [DockerAvailableFact]
    public async Task GetIncomplete_ShouldReturnOnlyInProgressOrNotStarted()
    {
        // Arrange
        var incomplete = new MongoTestSagaData { SagaId = Guid.NewGuid(), State = SagaState.InProgress };
        var notStarted = new MongoTestSagaData { SagaId = Guid.NewGuid(), State = SagaState.NotStarted };
        var completed = new MongoTestSagaData { SagaId = Guid.NewGuid(), State = SagaState.Completed };
        
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
        var saga = new MongoTestSagaData
        {
            SagaId = Guid.NewGuid(),
            OrderId = "UPDATE-001",
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
