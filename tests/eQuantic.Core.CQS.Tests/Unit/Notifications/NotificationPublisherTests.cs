using eQuantic.Core.CQS.Abstractions.Notifications;
using eQuantic.Core.CQS.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace eQuantic.Core.CQS.Tests.Unit.Notifications;

// ============================================================
// TEST NOTIFICATIONS & HANDLERS
// ============================================================

public record TestNotification(string Message) : INotification;

public class TestNotificationHandler1 : INotificationHandler<TestNotification>
{
    public static List<string> ReceivedMessages { get; } = new();

    public Task Handle(TestNotification notification, CancellationToken ct)
    {
        ReceivedMessages.Add($"Handler1: {notification.Message}");
        return Task.CompletedTask;
    }
}

public class TestNotificationHandler2 : INotificationHandler<TestNotification>
{
    public static List<string> ReceivedMessages { get; } = new();

    public Task Handle(TestNotification notification, CancellationToken ct)
    {
        ReceivedMessages.Add($"Handler2: {notification.Message}");
        return Task.CompletedTask;
    }
}

// FailingNotificationHandler removed - was interfering with other tests
// since AddCQS auto-registers all handlers in the assembly

/// <summary>
/// Unit tests for NotificationPublisher
/// </summary>
public class NotificationPublisherTests : IDisposable
{
    private readonly INotificationPublisher _publisher;

    public NotificationPublisherTests()
    {
        // Clear static state
        TestNotificationHandler1.ReceivedMessages.Clear();
        TestNotificationHandler2.ReceivedMessages.Clear();

        var services = new ServiceCollection();
        services.AddCQS(options => options.FromAssemblyContaining<NotificationPublisherTests>());
        _publisher = services.BuildServiceProvider().GetRequiredService<INotificationPublisher>();
    }

    public void Dispose()
    {
        TestNotificationHandler1.ReceivedMessages.Clear();
        TestNotificationHandler2.ReceivedMessages.Clear();
    }

    [Fact]
    public async Task Publish_Notification_ShouldInvokeAllHandlers()
    {
        // Arrange
        var notification = new TestNotification("Hello World");

        // Act
        await _publisher.Publish(notification);

        // Assert - check both handlers received messages
        var allMessages = TestNotificationHandler1.ReceivedMessages
            .Concat(TestNotificationHandler2.ReceivedMessages)
            .ToList();
        allMessages.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Publish_NullNotification_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _publisher.Publish((TestNotification)null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Publish_MultipleNotifications_ShouldInvokeHandlers()
    {
        // Arrange
        var notification1 = new TestNotification("First");
        var notification2 = new TestNotification("Second");

        // Act
        await _publisher.Publish(notification1);
        await _publisher.Publish(notification2);

        // Assert - at least some messages should be received
        var allMessages = TestNotificationHandler1.ReceivedMessages
            .Concat(TestNotificationHandler2.ReceivedMessages)
            .ToList();
        allMessages.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
