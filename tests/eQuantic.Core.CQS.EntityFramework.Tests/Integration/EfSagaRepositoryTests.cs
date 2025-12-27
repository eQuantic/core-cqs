using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.EntityFramework.Sagas;
using eQuantic.Core.CQS.EntityFramework.Tests.Fixtures;
using eQuantic.Core.CQS.Tests.Commons.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace eQuantic.Core.CQS.EntityFramework.Tests.Integration;

public class EfSagaRepositoryTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly EfSagaRepository<TestSagaData, TestDbContext> _repository;

    public EfSagaRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        
        _context = new TestDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        
        _repository = new EfSagaRepository<TestSagaData, TestDbContext>(_context);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [Fact]
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

    [Fact]
    public async Task Load_WhenNotExists_ShouldReturnNull()
    {
        // Act
        var result = await _repository.Load(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
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

    [Fact]
    public async Task Find_ShouldReturnMatchingSagas()
    {
        // Arrange
        var saga1 = new TestSagaData { SagaId = Guid.NewGuid(), OrderId = "EF-FIND-001", Amount = 100 };
        var saga2 = new TestSagaData { SagaId = Guid.NewGuid(), OrderId = "EF-FIND-002", Amount = 200 };
        var saga3 = new TestSagaData { SagaId = Guid.NewGuid(), OrderId = "EF-OTHER-001", Amount = 150 };
        
        await _repository.Save(saga1);
        await _repository.Save(saga2);
        await _repository.Save(saga3);

        // Act
        var results = await _repository.Find(s => s.OrderId.StartsWith("EF-FIND"));

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(s => s.OrderId == "EF-FIND-001");
        results.Should().Contain(s => s.OrderId == "EF-FIND-002");
    }

    [Fact]
    public async Task GetIncomplete_ShouldReturnOnlyInProgressOrNotStarted()
    {
        // Arrange
        var incomplete = new TestSagaData { SagaId = Guid.NewGuid(), OrderId = "EF-INC", State = SagaState.InProgress };
        var notStarted = new TestSagaData { SagaId = Guid.NewGuid(), OrderId = "EF-NS", State = SagaState.NotStarted };
        var completed = new TestSagaData { SagaId = Guid.NewGuid(), OrderId = "EF-COMP", State = SagaState.Completed };
        
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

    [Fact]
    public async Task Save_ShouldUpdateExistingSaga()
    {
        // Arrange
        var saga = new TestSagaData
        {
            SagaId = Guid.NewGuid(),
            OrderId = "EF-UPDATE-001",
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
