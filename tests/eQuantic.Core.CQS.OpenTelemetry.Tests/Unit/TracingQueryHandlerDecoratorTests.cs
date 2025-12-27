using eQuantic.Core.CQS.Abstractions.Handlers;
using eQuantic.Core.CQS.Abstractions.Queries;
using eQuantic.Core.CQS.Abstractions.Telemetry;
using eQuantic.Core.CQS.OpenTelemetry.Decorators;
using eQuantic.Core.CQS.OpenTelemetry.Options;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace eQuantic.Core.CQS.OpenTelemetry.Tests.Unit;

public class TracingQueryHandlerDecoratorTests
{
    private readonly ICqsTelemetry _mockTelemetry;
    private readonly OpenTelemetryOptions _options;

    public TracingQueryHandlerDecoratorTests()
    {
        _mockTelemetry = Substitute.For<ICqsTelemetry>();
        _mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IDictionary<string, object?>>())
            .Returns(Substitute.For<IDisposable>());
        _options = new OpenTelemetryOptions { TraceQueries = true };
    }

    [Fact]
    public async Task Execute_ShouldStartActivity_WhenTraceQueriesEnabled()
    {
        // Arrange
        var innerHandler = Substitute.For<IQueryHandler<TestQuery, TestResult>>();
        innerHandler.Execute(Arg.Any<TestQuery>(), Arg.Any<CancellationToken>())
            .Returns(new TestResult { Value = "test" });
        
        var decorator = new TracingQueryHandlerDecorator<TestQuery, TestResult>(innerHandler, _mockTelemetry, _options);
        var query = new TestQuery();

        // Act
        var result = await decorator.Execute(query, CancellationToken.None);

        // Assert
        result.Value.Should().Be("test");
        _mockTelemetry.Received(1).StartActivity("Query", "TestQuery", Arg.Any<IDictionary<string, object?>>());
    }

    [Fact]
    public async Task Execute_ShouldNotStartActivity_WhenTraceQueriesDisabled()
    {
        // Arrange
        var options = new OpenTelemetryOptions { TraceQueries = false };
        var innerHandler = Substitute.For<IQueryHandler<TestQuery, TestResult>>();
        innerHandler.Execute(Arg.Any<TestQuery>(), Arg.Any<CancellationToken>())
            .Returns(new TestResult { Value = "test" });
        
        var decorator = new TracingQueryHandlerDecorator<TestQuery, TestResult>(innerHandler, _mockTelemetry, options);

        // Act
        await decorator.Execute(new TestQuery(), CancellationToken.None);

        // Assert
        _mockTelemetry.DidNotReceive().StartActivity(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IDictionary<string, object?>>());
    }

    [Fact]
    public async Task Execute_ShouldRecordException_WhenQueryFails()
    {
        // Arrange
        var innerHandler = Substitute.For<IQueryHandler<TestQuery, TestResult>>();
        var exception = new InvalidOperationException("Query failed");
        innerHandler.Execute(Arg.Any<TestQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<TestResult>(exception));
        
        var decorator = new TracingQueryHandlerDecorator<TestQuery, TestResult>(innerHandler, _mockTelemetry, _options);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => decorator.Execute(new TestQuery(), CancellationToken.None));
        _mockTelemetry.Received(1).RecordException(exception);
    }

    public class TestQuery : IQuery<TestResult> { }
    public class TestResult { public string Value { get; set; } = ""; }
}
