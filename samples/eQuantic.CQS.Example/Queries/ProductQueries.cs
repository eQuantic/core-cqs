using eQuantic.Core.CQS.Abstractions.Queries;
using eQuantic.Core.CQS.Abstractions.Handlers;

namespace eQuantic.CQS.Example.Queries;

// ============================================================
// QUERIES - Read operations that always return data
// ============================================================

/// <summary>
/// Query to get a product by ID (returns ProductResult wrapper for nullability)
/// </summary>
public record GetProductQuery(int ProductId) : IQuery<ProductResult>;

/// <summary>
/// Query to search products with paging
/// </summary>
public record SearchProductsQuery(string? SearchTerm, int Page = 1, int PageSize = 10) : IQuery<ProductList>;

/// <summary>
/// Product DTO returned by queries
/// </summary>
public record ProductDto(int Id, string Name, decimal Price, int Stock);

/// <summary>
/// Wrapper for single product result (handles null case)
/// </summary>
public record ProductResult(ProductDto? Product);

/// <summary>
/// Wrapper for product list
/// </summary>
public record ProductList(List<ProductDto> Products);

// ============================================================
// QUERY HANDLERS
// ============================================================

/// <summary>
/// Handler for GetProductQuery
/// </summary>
public sealed class GetProductHandler : IQueryHandler<GetProductQuery, ProductResult>
{
    // Simulated product database
    private static readonly List<ProductDto> Products = new()
    {
        new(1, "Laptop", 999.99m, 50),
        new(2, "Mouse", 29.99m, 200),
        new(3, "Keyboard", 79.99m, 150),
        new(4, "Monitor", 349.99m, 75),
        new(5, "Headphones", 149.99m, 100)
    };

    public Task<ProductResult> Execute(GetProductQuery query, CancellationToken ct)
    {
        var product = Products.FirstOrDefault(p => p.Id == query.ProductId);
        return Task.FromResult(new ProductResult(product));
    }
}

/// <summary>
/// Handler for SearchProductsQuery
/// </summary>
public sealed class SearchProductsHandler : IQueryHandler<SearchProductsQuery, ProductList>
{
    private readonly TextWriter _output;

    public SearchProductsHandler(TextWriter output) => _output = output;

    public async Task<ProductList> Execute(SearchProductsQuery query, CancellationToken ct)
    {
        // Simulated product database
        var products = new List<ProductDto>
        {
            new(1, "Laptop", 999.99m, 50),
            new(2, "Mouse", 29.99m, 200),
            new(3, "Keyboard", 79.99m, 150),
            new(4, "Monitor", 349.99m, 75),
            new(5, "Headphones", 149.99m, 100)
        };

        var results = products
            .Where(p => string.IsNullOrEmpty(query.SearchTerm) || 
                        p.Name.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase))
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        await _output.WriteLineAsync($"🔍 Search Results: Found {results.Count} products");
        return new ProductList(results);
    }
}
