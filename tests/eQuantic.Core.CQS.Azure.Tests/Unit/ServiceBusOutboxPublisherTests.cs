using Azure.Messaging.ServiceBus;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Azure.Options;
using eQuantic.Core.CQS.Azure.Outbox;
using eQuantic.Core.CQS.Tests.Commons.Data;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace eQuantic.Core.CQS.Azure.Tests.Unit;

public class ServiceBusOutboxPublisherTests
{
    private readonly ServiceBusClient _mockClient;
    private readonly ServiceBusSender _mockSender;
    private readonly ServiceBusOptions _options;

    public ServiceBusOutboxPublisherTests()
    {
        _mockSender = Substitute.For<ServiceBusSender>();
        _mockClient = Substitute.For<ServiceBusClient>();
        _mockClient.CreateSender(Arg.Any<string>()).Returns(_mockSender);
        
        _options = new ServiceBusOptions
        {
            ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
            QueueOrTopicName = "test-queue"
        };
    }

    [Fact]
    public async Task PublishAsync_ShouldSendMessageToServiceBus()
    {
        // Arrange
        var message = new TestOutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageType = "OrderCreated",
            Payload = "{\"orderId\": \"123\"}",
            CorrelationId = "corr-123"
        };
        
        var publisher = new ServiceBusOutboxPublisher(_mockClient, _options);

        // Act
        await publisher.PublishAsync(message);

        // Assert
        await _mockSender.Received(1).SendMessageAsync(
            Arg.Is<ServiceBusMessage>(m => 
                m.MessageId == message.Id.ToString() &&
                m.Subject == message.MessageType &&
                m.CorrelationId == message.CorrelationId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_ShouldSetApplicationProperties()
    {
        // Arrange
        var message = new TestOutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageType = "UserRegistered",
            Payload = "{\"userId\": \"456\"}",
            CreatedAt = DateTime.UtcNow
        };
        
        var publisher = new ServiceBusOutboxPublisher(_mockClient, _options);

        // Act
        await publisher.PublishAsync(message);

        // Assert
        await _mockSender.Received(1).SendMessageAsync(
            Arg.Is<ServiceBusMessage>(m => 
                m.ApplicationProperties.ContainsKey("messageType") &&
                m.ApplicationProperties["messageType"].ToString() == message.MessageType),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_WithoutCorrelationId_ShouldNotSetCorrelationId()
    {
        // Arrange
        var message = new TestOutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageType = "TestEvent",
            Payload = "{}",
            CorrelationId = null
        };
        
        var publisher = new ServiceBusOutboxPublisher(_mockClient, _options);

        // Act
        await publisher.PublishAsync(message);

        // Assert
        await _mockSender.Received(1).SendMessageAsync(
            Arg.Is<ServiceBusMessage>(m => string.IsNullOrEmpty(m.CorrelationId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeSender()
    {
        // Arrange
        var publisher = new ServiceBusOutboxPublisher(_mockClient, _options);

        // Act
        await publisher.DisposeAsync();

        // Assert
        await _mockSender.Received(1).DisposeAsync();
    }
}
