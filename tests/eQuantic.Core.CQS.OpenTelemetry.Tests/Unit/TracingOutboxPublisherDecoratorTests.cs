using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Abstractions.Telemetry;
using eQuantic.Core.CQS.OpenTelemetry.Decorators;
using eQuantic.Core.CQS.OpenTelemetry.Options;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace eQuantic.Core.CQS.OpenTelemetry.Tests.Unit;

public class TracingOutboxPublisherDecoratorTests
{
    private readonly ICqsTelemetry _mockTelemetry;
    private readonly IOutboxPublisher _mockInner;
    private readonly OpenTelemetryOptions _options;

    public TracingOutboxPublisherDecoratorTests()
    {
        _mockTelemetry = Substitute.For<ICqsTelemetry>();
        _mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IDictionary<string, object?>>())
            .Returns(Substitute.For<IDisposable>());
        _mockInner = Substitute.For<IOutboxPublisher>();
        _options = new OpenTelemetryOptions { TraceOutbox = true };
    }

    [Fact]
    public async Task PublishAsync_ShouldStartActivity_WhenTraceOutboxEnabled()
    {
        // Arrange
        var decorator = new TracingOutboxPublisherDecorator(_mockInner, _mockTelemetry, _options);
        var message = CreateTestMessage();

        // Act
        await decorator.PublishAsync(message);

        // Assert
        _mockTelemetry.Received(1).StartActivity("Outbox", "Publish", Arg.Any<IDictionary<string, object?>>());
        await _mockInner.Received(1).PublishAsync(message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_ShouldRecordMetric_OnSuccess()
    {
        // Arrange
        var decorator = new TracingOutboxPublisherDecorator(_mockInner, _mockTelemetry, _options);
        var message = CreateTestMessage();

        // Act
        await decorator.PublishAsync(message);

        // Assert
        _mockTelemetry.Received(1).RecordMetric("cqs.outbox.published", 1, Arg.Any<IDictionary<string, object?>>());
    }

    [Fact]
    public async Task PublishAsync_ShouldRecordException_OnFailure()
    {
        // Arrange
        var exception = new InvalidOperationException("Publish failed");
        _mockInner.PublishAsync(Arg.Any<IOutboxMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(exception));
        
        var decorator = new TracingOutboxPublisherDecorator(_mockInner, _mockTelemetry, _options);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => decorator.PublishAsync(CreateTestMessage()));
        _mockTelemetry.Received(1).RecordException(exception);
        _mockTelemetry.Received(1).RecordMetric("cqs.outbox.failed", 1, Arg.Any<IDictionary<string, object?>>());
    }

    [Fact]
    public async Task PublishBatchAsync_ShouldRecordBatchSize()
    {
        // Arrange
        var decorator = new TracingOutboxPublisherDecorator(_mockInner, _mockTelemetry, _options);
        var messages = new[] { CreateTestMessage(), CreateTestMessage(), CreateTestMessage() };

        // Act
        await decorator.PublishBatchAsync(messages);

        // Assert
        _mockTelemetry.Received(1).RecordMetric("cqs.outbox.published", 3);
    }

    private static IOutboxMessage CreateTestMessage()
    {
        var message = Substitute.For<IOutboxMessage>();
        message.Id.Returns(Guid.NewGuid());
        message.MessageType.Returns("TestMessage");
        return message;
    }
}
