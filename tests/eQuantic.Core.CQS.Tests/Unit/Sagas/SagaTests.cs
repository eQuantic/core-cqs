using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Sagas;
using FluentAssertions;
using Xunit;

namespace eQuantic.Core.CQS.Tests.Unit.Sagas;

// ============================================================
// TEST SAGA DATA & SAGA
// ============================================================

public class OrderSagaData : SagaDataBase
{
    public Guid OrderId { get; set; }
    public bool PaymentProcessed { get; set; }
    public bool InventoryReserved { get; set; }
    public bool Shipped { get; set; }
}

public class OrderSaga : Saga<OrderSagaData>
{
    public bool Step2ShouldFail { get; set; }
    public List<string> ExecutedSteps { get; } = new();
    public List<string> CompensatedSteps { get; } = new();

    protected override void ConfigureSteps()
    {
        Step(
            name: "ProcessPayment",
            execute: async (data, ct) =>
            {
                ExecutedSteps.Add("ProcessPayment");
                data.PaymentProcessed = true;
                await Task.CompletedTask;
            },
            compensate: async (data, ct) =>
            {
                CompensatedSteps.Add("ProcessPayment");
                data.PaymentProcessed = false;
                await Task.CompletedTask;
            });

        Step(
            name: "ReserveInventory",
            execute: async (data, ct) =>
            {
                if (Step2ShouldFail)
                    throw new InvalidOperationException("Inventory failed");
                
                ExecutedSteps.Add("ReserveInventory");
                data.InventoryReserved = true;
                await Task.CompletedTask;
            },
            compensate: async (data, ct) =>
            {
                CompensatedSteps.Add("ReserveInventory");
                data.InventoryReserved = false;
                await Task.CompletedTask;
            });

        Step(
            name: "Ship",
            execute: async (data, ct) =>
            {
                ExecutedSteps.Add("Ship");
                data.Shipped = true;
                await Task.CompletedTask;
            },
            compensate: async (data, ct) =>
            {
                CompensatedSteps.Add("Ship");
                data.Shipped = false;
                await Task.CompletedTask;
            });
    }
}

/// <summary>
/// Unit tests for Saga execution and compensation
/// </summary>
public class SagaTests
{
    [Fact]
    public async Task Execute_AllStepsSucceed_ShouldCompleteSuccessfully()
    {
        // Arrange
        var saga = new OrderSaga();
        var data = new OrderSagaData { OrderId = Guid.NewGuid() };

        // Act
        var result = await saga.Execute(data);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.WasCompensated.Should().BeFalse();
        data.State.Should().Be(SagaState.Completed);
        data.PaymentProcessed.Should().BeTrue();
        data.InventoryReserved.Should().BeTrue();
        data.Shipped.Should().BeTrue();
        saga.ExecutedSteps.Should().BeEquivalentTo(new[] { "ProcessPayment", "ReserveInventory", "Ship" });
    }

    [Fact]
    public async Task Execute_StepFails_ShouldCompensatePreviousSteps()
    {
        // Arrange
        var saga = new OrderSaga { Step2ShouldFail = true };
        var data = new OrderSagaData { OrderId = Guid.NewGuid() };

        // Act
        var result = await saga.Execute(data);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.WasCompensated.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("Inventory failed");
        data.State.Should().Be(SagaState.Compensated);
        
        // Should have executed only first step
        saga.ExecutedSteps.Should().BeEquivalentTo(new[] { "ProcessPayment" });
        
        // Should have compensated first step
        saga.CompensatedSteps.Should().BeEquivalentTo(new[] { "ProcessPayment" });
        
        // Data should be rolled back
        data.PaymentProcessed.Should().BeFalse();
        data.InventoryReserved.Should().BeFalse();
        data.Shipped.Should().BeFalse();
    }

    [Fact]
    public async Task Execute_ShouldSetStartedAt()
    {
        // Arrange
        var saga = new OrderSaga();
        var data = new OrderSagaData { OrderId = Guid.NewGuid() };
        var before = DateTime.UtcNow;

        // Act
        await saga.Execute(data);

        // Assert
        data.StartedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task Execute_Success_ShouldSetCompletedAt()
    {
        // Arrange
        var saga = new OrderSaga();
        var data = new OrderSagaData { OrderId = Guid.NewGuid() };

        // Act
        await saga.Execute(data);

        // Assert
        data.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Execute_ShouldTrackCurrentStep()
    {
        // Arrange
        var saga = new OrderSaga();
        var data = new OrderSagaData { OrderId = Guid.NewGuid() };

        // Act
        await saga.Execute(data);

        // Assert
        data.CurrentStep.Should().Be(3); // All 3 steps completed
    }
}

/// <summary>
/// Unit tests for InMemorySagaRepository
/// </summary>
public class InMemorySagaRepositoryTests
{
    [Fact]
    public async Task Save_And_Load_ShouldRoundTrip()
    {
        // Arrange
        var repo = new InMemorySagaRepository<OrderSagaData>();
        var data = new OrderSagaData 
        { 
            SagaId = Guid.NewGuid(), 
            OrderId = Guid.NewGuid(),
            PaymentProcessed = true
        };

        // Act
        await repo.Save(data);
        var loaded = await repo.Load(data.SagaId);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.SagaId.Should().Be(data.SagaId);
        loaded.OrderId.Should().Be(data.OrderId);
        loaded.PaymentProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task Load_NonExistent_ShouldReturnNull()
    {
        // Arrange
        var repo = new InMemorySagaRepository<OrderSagaData>();

        // Act
        var result = await repo.Load(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetIncomplete_ShouldReturnOnlyIncomplete()
    {
        // Arrange
        var repo = new InMemorySagaRepository<OrderSagaData>();
        var inProgress = new OrderSagaData { SagaId = Guid.NewGuid(), State = SagaState.InProgress };
        var completed = new OrderSagaData { SagaId = Guid.NewGuid(), State = SagaState.Completed };
        var notStarted = new OrderSagaData { SagaId = Guid.NewGuid(), State = SagaState.NotStarted };

        await repo.Save(inProgress);
        await repo.Save(completed);
        await repo.Save(notStarted);

        // Act
        var incomplete = await repo.GetIncomplete();

        // Assert
        incomplete.Should().HaveCount(2);
        incomplete.Should().Contain(s => s.SagaId == inProgress.SagaId);
        incomplete.Should().Contain(s => s.SagaId == notStarted.SagaId);
    }

    [Fact]
    public async Task Delete_ShouldRemoveSaga()
    {
        // Arrange
        var repo = new InMemorySagaRepository<OrderSagaData>();
        var data = new OrderSagaData { SagaId = Guid.NewGuid() };
        await repo.Save(data);

        // Act
        await repo.Delete(data.SagaId);
        var loaded = await repo.Load(data.SagaId);

        // Assert
        loaded.Should().BeNull();
    }
}
