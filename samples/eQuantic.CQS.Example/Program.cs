using eQuantic.Core.CQS.Abstractions;
using eQuantic.Core.CQS.Abstractions.Notifications;
using eQuantic.Core.CQS.Extensions;
using eQuantic.CQS.Example.Commands;
using eQuantic.CQS.Example.Notifications;
using eQuantic.CQS.Example.Queries;
using eQuantic.CQS.Example.Sagas;
using eQuantic.CQS.Example.Streaming;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.CQS.Example;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           eQuantic.Core.CQS - Feature Demo                  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var services = new ServiceCollection();
        
        // Register CQS services using fluent API
        services.AddCQS(options => options.FromAssemblyContaining<Program>());
        
        // Register services
        services.AddSingleton<TextWriter>(Console.Out);
        services.AddTransient<OrderProcessingSaga>();
        
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var notificationPublisher = provider.GetRequiredService<INotificationPublisher>();

        // ============================================================
        // 1. COMMANDS Demo
        // ============================================================
        await DemoCommands(mediator);
        
        // ============================================================
        // 2. QUERIES Demo
        // ============================================================
        await DemoQueries(mediator);
        
        // ============================================================
        // 3. NOTIFICATIONS Demo
        // ============================================================
        await DemoNotifications(notificationPublisher);
        
        // ============================================================
        // 4. STREAMING Demo
        // ============================================================
        await DemoStreaming(mediator);
        
        // ============================================================
        // 5. SAGA Demo
        // ============================================================
        await DemoSaga(provider);

        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    Demo Complete! 🎉                        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
    }

    static async Task DemoCommands(IMediator mediator)
    {
        Console.WriteLine("┌────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ 1. COMMANDS - Create and Cancel Order                      │");
        Console.WriteLine("└────────────────────────────────────────────────────────────┘");
        
        // Create order (returns ID)
        var orderId = await mediator.ExecuteAsync(new CreateOrderCommand(
            "John Doe",
            new List<OrderItem>
            {
                new("Laptop", 1, 999.99m),
                new("Mouse", 2, 29.99m)
            }));
        
        Console.WriteLine();
        
        // Cancel order (no return)
        await mediator.ExecuteAsync(new CancelOrderCommand(orderId, "Customer requested"));
        
        Console.WriteLine();
    }

    static async Task DemoQueries(IMediator mediator)
    {
        Console.WriteLine("┌────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ 2. QUERIES - Search Products                               │");
        Console.WriteLine("└────────────────────────────────────────────────────────────┘");
        
        // Get single product
        var result = await mediator.ExecuteAsync(new GetProductQuery(1));
        if (result.Product != null)
        {
            Console.WriteLine($"🛒 Product Found: {result.Product.Name} - ${result.Product.Price:F2}");
        }
        
        // Search products
        var productList = await mediator.ExecuteAsync(new SearchProductsQuery("o", Page: 1, PageSize: 3));
        foreach (var p in productList.Products)
        {
            Console.WriteLine($"   • {p.Name}: ${p.Price:F2} ({p.Stock} in stock)");
        }
        
        Console.WriteLine();
    }

    static async Task DemoNotifications(INotificationPublisher publisher)
    {
        Console.WriteLine("┌────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ 3. NOTIFICATIONS - Multiple Handlers                       │");
        Console.WriteLine("└────────────────────────────────────────────────────────────┘");
        
        // Publish notification (handled by multiple handlers)
        await publisher.Publish(new OrderPlacedNotification(
            Guid.NewGuid(), 
            "Jane Smith", 
            1059.97m));
        
        await publisher.Publish(new LowStockNotification(
            1, 
            "Laptop", 
            CurrentStock: 5));
        
        Console.WriteLine();
    }

    static async Task DemoStreaming(IMediator mediator)
    {
        Console.WriteLine("┌────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ 4. STREAMING - Real-time Stock Prices                      │");
        Console.WriteLine("└────────────────────────────────────────────────────────────┘");
        
        var query = new StreamStockPricesQuery(new[] { "AAPL", "MSFT" });
        
        await foreach (var price in mediator.ExecuteStreamAsync(query))
        {
            Console.WriteLine($"   💹 {price.Symbol}: ${price.Price:F2} at {price.Timestamp:HH:mm:ss}");
        }
        
        Console.WriteLine();
    }

    static async Task DemoSaga(IServiceProvider provider)
    {
        Console.WriteLine("┌────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ 5. SAGA - Multi-step Transaction with Compensation         │");
        Console.WriteLine("└────────────────────────────────────────────────────────────┘");
        
        var saga = provider.GetRequiredService<OrderProcessingSaga>();
        
        var sagaData = new OrderSagaData
        {
            OrderId = Guid.NewGuid(),
            CustomerName = "Bob Wilson",
            Amount = 599.99m
        };
        
        Console.WriteLine($"🔄 Starting Saga for Order {sagaData.OrderId}...");
        Console.WriteLine();
        
        var result = await saga.Execute(sagaData);
        
        Console.WriteLine();
        Console.WriteLine(result.IsSuccess 
            ? $"✅ Saga completed successfully! State: {sagaData.State}" 
            : $"❌ Saga failed. Was compensated: {result.WasCompensated}");
        
        Console.WriteLine();
    }
}