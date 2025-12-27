using Amazon.SQS;
using Amazon.SQS.Model;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.AWS.Options;
using eQuantic.Core.CQS.AWS.Outbox;
using eQuantic.Core.CQS.Tests.Commons.Data;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace eQuantic.Core.CQS.AWS.Tests.Unit;

public class SqsOutboxPublisherTests
{
    private readonly IAmazonSQS _mockSqsClient;
    private readonly SqsOptions _options;

    public SqsOutboxPublisherTests()
    {
        _mockSqsClient = Substitute.For<IAmazonSQS>();
        _options = new SqsOptions
        {
            QueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789/test-queue",
            DelaySeconds = 0,
            MaxBatchSize = 10
        };
    }

    [Fact]
    public async Task PublishAsync_ShouldSendMessageToSqs()
    {
        // Arrange
        var message = new TestOutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageType = "OrderCreated",
            Payload = "{\"orderId\": \"123\"}",
            CorrelationId = "corr-123"
        };
        
        var publisher = new SqsOutboxPublisher(_mockSqsClient, _options);

        // Act
        await publisher.PublishAsync(message);

        // Assert
        await _mockSqsClient.Received(1).SendMessageAsync(
            Arg.Is<SendMessageRequest>(r => 
                r.QueueUrl == _options.QueueUrl &&
                r.MessageBody == message.Payload),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_ShouldSetMessageAttributes()
    {
        // Arrange
        var message = new TestOutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageType = "UserRegistered",
            Payload = "{\"userId\": \"456\"}",
            CreatedAt = DateTime.UtcNow
        };
        
        var publisher = new SqsOutboxPublisher(_mockSqsClient, _options);

        // Act
        await publisher.PublishAsync(message);

        // Assert
        await _mockSqsClient.Received(1).SendMessageAsync(
            Arg.Is<SendMessageRequest>(r => 
                r.MessageAttributes.ContainsKey("MessageType") &&
                r.MessageAttributes["MessageType"].StringValue == message.MessageType &&
                r.MessageAttributes.ContainsKey("MessageId") &&
                r.MessageAttributes["MessageId"].StringValue == message.Id.ToString()),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_WithCorrelationId_ShouldSetCorrelationIdAttribute()
    {
        // Arrange
        var message = new TestOutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageType = "TestEvent",
            Payload = "{}",
            CorrelationId = "test-correlation-id"
        };
        
        var publisher = new SqsOutboxPublisher(_mockSqsClient, _options);

        // Act
        await publisher.PublishAsync(message);

        // Assert
        await _mockSqsClient.Received(1).SendMessageAsync(
            Arg.Is<SendMessageRequest>(r => 
                r.MessageAttributes.ContainsKey("CorrelationId") &&
                r.MessageAttributes["CorrelationId"].StringValue == message.CorrelationId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishBatchAsync_WithEmptyList_ShouldNotSendAnyMessages()
    {
        // Arrange
        var publisher = new SqsOutboxPublisher(_mockSqsClient, _options);

        // Act
        await publisher.PublishBatchAsync(new List<TestOutboxMessage>());

        // Assert
        await _mockSqsClient.DidNotReceive().SendMessageBatchAsync(
            Arg.Any<SendMessageBatchRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishBatchAsync_ShouldSendBatchToSqs()
    {
        // Arrange
        var messages = new List<TestOutboxMessage>
        {
            new() { Id = Guid.NewGuid(), MessageType = "Event1", Payload = "{}" },
            new() { Id = Guid.NewGuid(), MessageType = "Event2", Payload = "{}" },
            new() { Id = Guid.NewGuid(), MessageType = "Event3", Payload = "{}" }
        };
        
        _mockSqsClient.SendMessageBatchAsync(Arg.Any<SendMessageBatchRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SendMessageBatchResponse { Failed = new List<BatchResultErrorEntry>() });
        
        var publisher = new SqsOutboxPublisher(_mockSqsClient, _options);

        // Act
        await publisher.PublishBatchAsync(messages);

        // Assert
        await _mockSqsClient.Received(1).SendMessageBatchAsync(
            Arg.Is<SendMessageBatchRequest>(r => 
                r.QueueUrl == _options.QueueUrl &&
                r.Entries.Count == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishBatchAsync_WhenBatchFails_ShouldThrowException()
    {
        // Arrange
        var messages = new List<TestOutboxMessage>
        {
            new() { Id = Guid.NewGuid(), MessageType = "Event1", Payload = "{}" }
        };
        
        _mockSqsClient.SendMessageBatchAsync(Arg.Any<SendMessageBatchRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SendMessageBatchResponse
            {
                Failed = new List<BatchResultErrorEntry>
                {
                    new() { Id = "0", Message = "Test error" }
                }
            });
        
        var publisher = new SqsOutboxPublisher(_mockSqsClient, _options);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => publisher.PublishBatchAsync(messages));
    }
}
