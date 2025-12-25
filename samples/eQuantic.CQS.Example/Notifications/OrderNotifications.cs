using eQuantic.Core.CQS.Abstractions.Notifications;

namespace eQuantic.CQS.Example.Notifications;

// ============================================================
// NOTIFICATIONS - Pub/Sub events handled by multiple handlers
// ============================================================

/// <summary>
/// Notification when an order is placed
/// </summary>
public record OrderPlacedNotification(Guid OrderId, string CustomerName, decimal Total) : INotification;

/// <summary>
/// Notification when a product is low on stock
/// </summary>
public record LowStockNotification(int ProductId, string ProductName, int CurrentStock) : INotification;

// ============================================================
// NOTIFICATION HANDLERS - Multiple handlers per notification
// ============================================================

/// <summary>
/// Sends email when order is placed
/// </summary>
public sealed class OrderEmailHandler : INotificationHandler<OrderPlacedNotification>
{
    private readonly TextWriter _output;

    public OrderEmailHandler(TextWriter output) => _output = output;

    public async Task Handle(OrderPlacedNotification notification, CancellationToken ct)
    {
        await _output.WriteLineAsync($"📧 Email Sent: Order {notification.OrderId} confirmed to {notification.CustomerName}");
    }
}

/// <summary>
/// Updates analytics when order is placed
/// </summary>
public sealed class OrderAnalyticsHandler : INotificationHandler<OrderPlacedNotification>
{
    private readonly TextWriter _output;

    public OrderAnalyticsHandler(TextWriter output) => _output = output;

    public async Task Handle(OrderPlacedNotification notification, CancellationToken ct)
    {
        await _output.WriteLineAsync($"📊 Analytics Updated: New order worth ${notification.Total:F2}");
    }
}

/// <summary>
/// Alerts inventory team on low stock
/// </summary>
public sealed class LowStockAlertHandler : INotificationHandler<LowStockNotification>
{
    private readonly TextWriter _output;

    public LowStockAlertHandler(TextWriter output) => _output = output;

    public async Task Handle(LowStockNotification notification, CancellationToken ct)
    {
        await _output.WriteLineAsync($"⚠️ Low Stock Alert: {notification.ProductName} has only {notification.CurrentStock} units!");
    }
}
