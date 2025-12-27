using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;
using Xunit;

namespace eQuantic.Core.CQS.PostgreSql.Tests.Fixtures;

/// <summary>PostgreSQL container fixture for integration tests</summary>
public class PostgreSqlContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    
    public string ConnectionString => _container.GetConnectionString();

    public PostgreSqlContainerFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_db")
            .WithUsername("test")
            .WithPassword("test")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(5432)
                .UntilMessageIsLogged("database system is ready to accept connections"))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("PostgreSql")]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlContainerFixture>
{
}
