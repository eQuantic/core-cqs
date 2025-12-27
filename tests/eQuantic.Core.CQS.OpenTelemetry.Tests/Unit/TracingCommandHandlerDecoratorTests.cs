using eQuantic.Core.CQS.Abstractions.Commands;
using eQuantic.Core.CQS.Abstractions.Handlers;
using eQuantic.Core.CQS.Abstractions.Telemetry;
using eQuantic.Core.CQS.OpenTelemetry.Decorators;
using eQuantic.Core.CQS.OpenTelemetry.Options;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace eQuantic.Core.CQS.OpenTelemetry.Tests.Unit;

public class TracingCommandHandlerDecoratorTests
{
    private readonly ICqsTelemetry _mockTelemetry;
    private readonly OpenTelemetryOptions _options;

    public TracingCommandHandlerDecoratorTests()
    {
        _mockTelemetry = Substitute.For<ICqsTelemetry>();
        _mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IDictionary<string, object?>>())
            .Returns(Substitute.For<IDisposable>());
        _options = new OpenTelemetryOptions { TraceCommands = true };
    }

    [Fact]
    public async Task Execute_ShouldStartActivity_WhenTraceCommandsEnabled()
    {
        // Arrange
        var innerHandler = Substitute.For<ICommandHandler<TestCommand>>();
        var decorator = new TracingCommandHandlerDecorator<TestCommand>(innerHandler, _mockTelemetry, _options);
        var command = new TestCommand();

        // Act
        await decorator.Execute(command, CancellationToken.None);

        // Assert
        _mockTelemetry.Received(1).StartActivity("Command", "TestCommand", Arg.Any<IDictionary<string, object?>>());
        await innerHandler.Received(1).Execute(command, CancellationToken.None);
    }

    [Fact]
    public async Task Execute_ShouldNotStartActivity_WhenTraceCommandsDisabled()
    {
        // Arrange
        var options = new OpenTelemetryOptions { TraceCommands = false };
        var innerHandler = Substitute.For<ICommandHandler<TestCommand>>();
        var decorator = new TracingCommandHandlerDecorator<TestCommand>(innerHandler, _mockTelemetry, options);
        var command = new TestCommand();

        // Act
        await decorator.Execute(command, CancellationToken.None);

        // Assert
        _mockTelemetry.DidNotReceive().StartActivity(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IDictionary<string, object?>>());
        await innerHandler.Received(1).Execute(command, CancellationToken.None);
    }

    [Fact]
    public async Task Execute_ShouldRecordException_WhenHandlerThrows()
    {
        // Arrange
        var innerHandler = Substitute.For<ICommandHandler<TestCommand>>();
        var exception = new InvalidOperationException("Test error");
        innerHandler.Execute(Arg.Any<TestCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(exception));
        
        var decorator = new TracingCommandHandlerDecorator<TestCommand>(innerHandler, _mockTelemetry, _options);
        var command = new TestCommand();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => decorator.Execute(command, CancellationToken.None));
        _mockTelemetry.Received(1).RecordException(exception);
    }

    [Fact]
    public async Task Execute_WithResult_ShouldReturnResult()
    {
        // Arrange
        var innerHandler = Substitute.For<ICommandHandler<TestCommandWithResult, string>>();
        innerHandler.Execute(Arg.Any<TestCommandWithResult>(), Arg.Any<CancellationToken>())
            .Returns("success");
        
        var decorator = new TracingCommandHandlerDecorator<TestCommandWithResult, string>(innerHandler, _mockTelemetry, _options);
        var command = new TestCommandWithResult();

        // Act
        var result = await decorator.Execute(command, CancellationToken.None);

        // Assert
        result.Should().Be("success");
        _mockTelemetry.Received(1).StartActivity("Command", "TestCommandWithResult", Arg.Any<IDictionary<string, object?>>());
    }

    public class TestCommand : ICommand { }
    public class TestCommandWithResult : ICommand<string> { }
}
