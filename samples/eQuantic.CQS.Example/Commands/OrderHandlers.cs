using eQuantic.Core.CQS.Abstractions.Handlers;

namespace eQuantic.CQS.Example.Commands;

// ============================================================
// COMMAND HANDLERS - Execute business logic
// ============================================================

/// <summary>
/// Handler for CreateOrderCommand
/// </summary>
public sealed class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly TextWriter _output;

    public CreateOrderHandler(TextWriter output) => _output = output;

    public async Task<Guid> Execute(CreateOrderCommand command, CancellationToken ct)
    {
        var orderId = Guid.NewGuid();
        var total = command.Items.Sum(i => i.Quantity * i.Price);

        await _output.WriteLineAsync($"📦 Order Created:");
        await _output.WriteLineAsync($"   ID: {orderId}");
        await _output.WriteLineAsync($"   Customer: {command.CustomerName}");
        await _output.WriteLineAsync($"   Items: {command.Items.Count}");
        await _output.WriteLineAsync($"   Total: ${total:F2}");

        return orderId;
    }
}

/// <summary>
/// Handler for CancelOrderCommand (no return)
/// </summary>
public sealed class CancelOrderHandler : ICommandHandler<CancelOrderCommand>
{
    private readonly TextWriter _output;

    public CancelOrderHandler(TextWriter output) => _output = output;

    public async Task Execute(CancelOrderCommand command, CancellationToken ct)
    {
        await _output.WriteLineAsync($"❌ Order Cancelled:");
        await _output.WriteLineAsync($"   ID: {command.OrderId}");
        await _output.WriteLineAsync($"   Reason: {command.Reason}");
    }
}
