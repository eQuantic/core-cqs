using eQuantic.Core.CQS.Abstractions.Resilience;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Resilience.Compensation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace eQuantic.Core.CQS.Resilience.Tests.Unit;

public class CompensationHandlerTests
{
    [Fact]
    public async Task DelegateCompensationHandler_ShouldInvokeDelegate()
    {
        // Arrange
        var wasInvoked = false;
        var handler = new DelegateCompensationHandler<TestSagaData>(
            (saga, ex, ct) => 
            {
                wasInvoked = true;
                return Task.CompletedTask;
            });
        
        var saga = new TestSagaData();

        // Act
        await handler.CompensateAsync(saga, null);

        // Assert
        wasInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task DelegateCompensationHandler_ShouldReceiveException()
    {
        // Arrange
        Exception? receivedException = null;
        var expectedEx = new InvalidOperationException("test");
        var handler = new DelegateCompensationHandler<TestSagaData>(
            (saga, ex, ct) => 
            {
                receivedException = ex;
                return Task.CompletedTask;
            });

        // Act
        await handler.CompensateAsync(new TestSagaData(), expectedEx);

        // Assert
        receivedException.Should().BeSameAs(expectedEx);
    }

    [Fact]
    public async Task NoOpCompensationHandler_ShouldComplete()
    {
        // Arrange
        var handler = new NoOpCompensationHandler<TestSagaData>();

        // Act
        await handler.CompensateAsync(new TestSagaData(), null);

        // Assert - should not throw and complete immediately
    }

    [Fact]
    public async Task LoggingDeadLetterHandler_ShouldLogError()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingDeadLetterHandler>>();
        var handler = new LoggingDeadLetterHandler(logger);
        var saga = Substitute.For<ISagaData>();
        saga.SagaId.Returns(Guid.NewGuid());
        saga.State.Returns(SagaState.Failed);
        saga.StartedAt.Returns(DateTime.UtcNow);

        // Act
        await handler.HandleAsync(saga, "Timeout expired");

        // Assert
        logger.ReceivedWithAnyArgs(1).LogError(default, default(Exception), default);
    }

    private class TestSagaData : ISagaData
    {
        public Guid SagaId { get; set; } = Guid.NewGuid();
        public SagaState State { get; set; } = SagaState.NotStarted;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public int CurrentStep { get; set; }
    }
}
