using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Sagas;

namespace eQuantic.CQS.Example.Sagas;

// ============================================================
// SAGAS - Orchestrate multi-step transactions with compensation
// ============================================================

/// <summary>
/// Saga data for order processing
/// </summary>
public class OrderSagaData : SagaDataBase
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public decimal Amount { get; set; }
    public bool PaymentProcessed { get; set; }
    public bool InventoryReserved { get; set; }
    public bool ShippingScheduled { get; set; }
}

/// <summary>
/// Saga that orchestrates order processing with compensation
/// </summary>
public class OrderProcessingSaga : Saga<OrderSagaData>
{
    private readonly TextWriter _output;

    public OrderProcessingSaga(TextWriter output) => _output = output;

    protected override void ConfigureSteps()
    {
        // Step 1: Process Payment
        Step(
            name: "ProcessPayment",
            execute: async (data, ct) =>
            {
                await _output.WriteLineAsync($"💳 Processing payment of ${data.Amount:F2}...");
                await Task.Delay(100, ct); // Simulate API call
                data.PaymentProcessed = true;
                await _output.WriteLineAsync("   ✓ Payment processed");
            },
            compensate: async (data, ct) =>
            {
                await _output.WriteLineAsync("💳 Refunding payment...");
                data.PaymentProcessed = false;
                await _output.WriteLineAsync("   ✓ Payment refunded");
            }
        );

        // Step 2: Reserve Inventory
        Step(
            name: "ReserveInventory",
            execute: async (data, ct) =>
            {
                await _output.WriteLineAsync("📦 Reserving inventory...");
                await Task.Delay(100, ct); // Simulate API call
                data.InventoryReserved = true;
                await _output.WriteLineAsync("   ✓ Inventory reserved");
            },
            compensate: async (data, ct) =>
            {
                await _output.WriteLineAsync("📦 Releasing inventory...");
                data.InventoryReserved = false;
                await _output.WriteLineAsync("   ✓ Inventory released");
            }
        );

        // Step 3: Schedule Shipping
        Step(
            name: "ScheduleShipping",
            execute: async (data, ct) =>
            {
                await _output.WriteLineAsync("🚚 Scheduling shipping...");
                await Task.Delay(100, ct); // Simulate API call
                data.ShippingScheduled = true;
                await _output.WriteLineAsync("   ✓ Shipping scheduled");
            },
            compensate: async (data, ct) =>
            {
                await _output.WriteLineAsync("🚚 Cancelling shipment...");
                data.ShippingScheduled = false;
                await _output.WriteLineAsync("   ✓ Shipment cancelled");
            }
        );
    }
}
