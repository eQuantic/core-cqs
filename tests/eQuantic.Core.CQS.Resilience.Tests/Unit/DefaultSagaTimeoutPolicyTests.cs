using eQuantic.Core.CQS.Abstractions.Resilience;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Resilience.Options;
using eQuantic.Core.CQS.Resilience.Policies;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace eQuantic.Core.CQS.Resilience.Tests.Unit;

public class DefaultSagaTimeoutPolicyTests
{
    [Fact]
    public void GetTimeout_ShouldReturnDefaultTimeout_WhenSagaDoesNotImplementIResilientSagaData()
    {
        // Arrange
        var options = new ResilienceOptions { DefaultSagaTimeout = TimeSpan.FromMinutes(30) };
        var policy = new DefaultSagaTimeoutPolicy(options);
        var saga = Substitute.For<ISagaData>();

        // Act
        var timeout = policy.GetTimeout(saga);

        // Assert
        timeout.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public void GetTimeout_ShouldReturnSagaTimeout_WhenSagaImplementsIResilientSagaData()
    {
        // Arrange
        var options = new ResilienceOptions { DefaultSagaTimeout = TimeSpan.FromMinutes(30) };
        var policy = new DefaultSagaTimeoutPolicy(options);
        var saga = new TestResilientSagaData { CustomTimeout = TimeSpan.FromMinutes(10) };

        // Act
        var timeout = policy.GetTimeout(saga);

        // Assert
        timeout.Should().Be(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task OnTimeoutAsync_ShouldMarkSagaAsFailed()
    {
        // Arrange
        var options = new ResilienceOptions { EnableDeadLetterQueue = false };
        var policy = new DefaultSagaTimeoutPolicy(options);
        var saga = Substitute.For<ISagaData>();

        // Act
        await policy.OnTimeoutAsync(saga);

        // Assert
        saga.State.Should().Be(SagaState.Failed);
    }

    [Fact]
    public async Task OnTimeoutAsync_ShouldCallDeadLetterHandler_WhenEnabled()
    {
        // Arrange
        var deadLetterHandler = Substitute.For<IDeadLetterHandler>();
        var options = new ResilienceOptions { EnableDeadLetterQueue = true };
        var policy = new DefaultSagaTimeoutPolicy(options, deadLetterHandler);
        var saga = Substitute.For<ISagaData>();

        // Act
        await policy.OnTimeoutAsync(saga);

        // Assert
        await deadLetterHandler.Received(1).HandleAsync(saga, Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private class TestResilientSagaData : IResilientSagaData
    {
        public Guid SagaId { get; set; } = Guid.NewGuid();
        public SagaState State { get; set; } = SagaState.NotStarted;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public int CurrentStep { get; set; }
        
        public TimeSpan CustomTimeout { get; set; } = TimeSpan.FromMinutes(15);
        TimeSpan? IResilientSagaData.Timeout => CustomTimeout;
        
        public string? LastError { get; set; }
        public int RetryAttempt { get; set; }
    }
}
