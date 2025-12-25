using eQuantic.Core.CQS.Abstractions.Streaming;
using eQuantic.Core.CQS.Abstractions;
using eQuantic.Core.CQS.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Xunit;

namespace eQuantic.Core.CQS.Tests.Unit.Streaming;

// ============================================================
// TEST STREAM QUERIES & HANDLERS
// ============================================================

public record TestStreamQuery(int Count) : IStreamQuery<int>;

public class TestStreamQueryHandler : IStreamQueryHandler<TestStreamQuery, int>
{
    public async IAsyncEnumerable<int> Handle(
        TestStreamQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (int i = 1; i <= query.Count && !cancellationToken.IsCancellationRequested; i++)
        {
            yield return i;
            await Task.Delay(10, cancellationToken);
        }
    }
}

/// <summary>
/// Unit tests for Stream Query execution
/// </summary>
public class StreamQueryTests
{
    private readonly IMediator _mediator;

    public StreamQueryTests()
    {
        var services = new ServiceCollection();
        services.AddCQS(null, typeof(StreamQueryTests).Assembly);
        _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task ExecuteStreamAsync_ShouldReturnAllItems()
    {
        // Arrange
        var query = new TestStreamQuery(5);
        var results = new List<int>();

        // Act
        await foreach (var item in _mediator.ExecuteStreamAsync(query))
        {
            results.Add(item);
        }

        // Assert
        results.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public async Task ExecuteStreamAsync_EmptyStream_ShouldReturnNoItems()
    {
        // Arrange
        var query = new TestStreamQuery(0);
        var results = new List<int>();

        // Act
        await foreach (var item in _mediator.ExecuteStreamAsync(query))
        {
            results.Add(item);
        }

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteStreamAsync_WithCancellation_ShouldStopEarly()
    {
        // Arrange
        var query = new TestStreamQuery(100);
        var results = new List<int>();
        var cts = new CancellationTokenSource();

        // Act
        try
        {
            await foreach (var item in _mediator.ExecuteStreamAsync(query, cts.Token))
            {
                results.Add(item);
                if (results.Count >= 3)
                    cts.Cancel();
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }

        // Assert
        results.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    public async Task ExecuteStreamAsync_VariousCounts_ShouldReturnCorrectAmount(int count)
    {
        // Arrange
        var query = new TestStreamQuery(count);
        var results = new List<int>();

        // Act
        await foreach (var item in _mediator.ExecuteStreamAsync(query))
        {
            results.Add(item);
        }

        // Assert
        results.Should().HaveCount(count);
    }
}
