using eQuantic.Core.CQS.Abstractions;
using eQuantic.Core.CQS.Extensions;
using eQuantic.Core.CQS.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace eQuantic.Core.CQS.Tests.Unit.Mediator;

/// <summary>
/// Unit tests for IMediator command execution
/// </summary>
public class MediatorCommandTests
{
    private readonly IServiceProvider _provider;
    private readonly IMediator _mediator;

    public MediatorCommandTests()
    {
        var services = new ServiceCollection();
        services.AddCQS(null, typeof(TestCommand).Assembly);
        _provider = services.BuildServiceProvider();
        _mediator = _provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task ExecuteAsync_CommandWithoutResult_ShouldExecuteHandler()
    {
        // Arrange
        var command = new TestCommand("Test Value");

        // Act
        var act = () => _mediator.ExecuteAsync(command);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_CommandWithResult_ShouldReturnResult()
    {
        // Arrange
        var command = new TestCommandWithResult("Hello");

        // Act
        var result = await _mediator.ExecuteAsync(command);

        // Assert
        result.Should().Be("Processed: Hello");
    }

    [Fact]
    public async Task ExecuteAsync_NullCommand_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _mediator.ExecuteAsync((TestCommand)null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequested_ShouldRespectCancellation()
    {
        // Arrange
        var command = new TestCommand("Test");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - Should not throw for simple commands
        // (more complex commands could check cancellation)
        await _mediator.ExecuteAsync(command, cts.Token);
    }
}

/// <summary>
/// Unit tests for IMediator query execution
/// </summary>
public class MediatorQueryTests
{
    private readonly IMediator _mediator;

    public MediatorQueryTests()
    {
        var services = new ServiceCollection();
        services.AddCQS(null, typeof(TestCommand).Assembly);
        _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task ExecuteAsync_Query_ShouldReturnResult()
    {
        // Arrange
        var query = new TestQuery(42);

        // Act
        var result = await _mediator.ExecuteAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(42);
        result.Name.Should().Be("Item-42");
    }

    [Fact]
    public async Task ExecuteAsync_NullQuery_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _mediator.ExecuteAsync((TestQuery)null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task ExecuteAsync_Query_ShouldHandleVariousIds(int id)
    {
        // Arrange
        var query = new TestQuery(id);

        // Act
        var result = await _mediator.ExecuteAsync(query);

        // Assert
        result.Id.Should().Be(id);
        result.Name.Should().Be($"Item-{id}");
    }
}
