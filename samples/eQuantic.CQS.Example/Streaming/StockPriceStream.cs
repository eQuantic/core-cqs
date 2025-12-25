using System.Runtime.CompilerServices;
using eQuantic.Core.CQS.Abstractions.Streaming;

namespace eQuantic.CQS.Example.Streaming;

// ============================================================
// STREAMING - IAsyncEnumerable for large datasets
// ============================================================

/// <summary>
/// Stream query to get real-time stock prices
/// </summary>
public record StreamStockPricesQuery(string[] Symbols) : IStreamQuery<StockPrice>;

/// <summary>
/// Stock price update
/// </summary>
public record StockPrice(string Symbol, decimal Price, DateTime Timestamp);

// ============================================================
// STREAM QUERY HANDLER
// ============================================================

/// <summary>
/// Streams stock prices in real-time (simulated)
/// </summary>
public sealed class StockPriceStreamHandler : IStreamQueryHandler<StreamStockPricesQuery, StockPrice>
{
    private readonly TextWriter _output;
    private static readonly Random Random = new();

    public StockPriceStreamHandler(TextWriter output) => _output = output;

    public async IAsyncEnumerable<StockPrice> Handle(
        StreamStockPricesQuery query, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await _output.WriteLineAsync($"📈 Streaming prices for: {string.Join(", ", query.Symbols)}");

        // Simulate 5 price updates
        for (int i = 0; i < 5 && !cancellationToken.IsCancellationRequested; i++)
        {
            foreach (var symbol in query.Symbols)
            {
                var basePrice = symbol switch
                {
                    "AAPL" => 180m,
                    "GOOGL" => 140m,
                    "MSFT" => 380m,
                    _ => 100m
                };

                var variation = (decimal)(Random.NextDouble() * 10 - 5);
                var price = new StockPrice(symbol, basePrice + variation, DateTime.UtcNow);

                yield return price;
            }

            await Task.Delay(500, cancellationToken); // Simulate real-time delay
        }

        await _output.WriteLineAsync("📈 Stream completed");
    }
}
