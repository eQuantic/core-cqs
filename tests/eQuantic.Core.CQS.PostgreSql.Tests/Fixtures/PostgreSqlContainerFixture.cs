using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;
using eQuantic.Core.CQS.Tests.Commons.Fixtures;
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
        // the collection fixture initializes even when every test in it is skipped, so without
        // Docker this would hang waiting for a container that can never start
        if (!DockerEnvironment.IsAvailable)
        {
            return;
        }

        // A bounded wait: whatever keeps a container from coming up, the suite reports it instead of
        // stalling the run until the agent's own timeout kills it.
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        await _container.StartAsync(timeout.Token);
    }

    public async Task DisposeAsync()
    {
        if (!DockerEnvironment.IsAvailable)
        {
            return;
        }

        await _container.DisposeAsync();
    }
}

[CollectionDefinition("PostgreSql")]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlContainerFixture>
{
}
