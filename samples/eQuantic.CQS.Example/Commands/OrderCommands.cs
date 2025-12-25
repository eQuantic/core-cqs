using eQuantic.Core.CQS.Abstractions.Commands;

namespace eQuantic.CQS.Example.Commands;

// ============================================================
// COMMANDS - Write operations that may return a result
// ============================================================

/// <summary>
/// Command to create a new order (returns Order ID)
/// </summary>
public record CreateOrderCommand(string CustomerName, List<OrderItem> Items) : ICommand<Guid>;

/// <summary>
/// Command to cancel an order (no return value)
/// </summary>
public record CancelOrderCommand(Guid OrderId, string Reason) : ICommand;

/// <summary>
/// Order item for the command
/// </summary>
public record OrderItem(string ProductName, int Quantity, decimal Price);
