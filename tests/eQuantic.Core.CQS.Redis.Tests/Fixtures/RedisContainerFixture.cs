using DotNet.Testcontainers.Builders;
using StackExchange.Redis;
using Testcontainers.Redis;
using eQuantic.Core.CQS.Tests.Commons.Fixtures;
using Xunit;

namespace eQuantic.Core.CQS.Redis.Tests.Fixtures;

/// <summary>Redis container fixture for integration tests</summary>
public class RedisContainerFixture : IAsyncLifetime
{
    private readonly RedisContainer _container;
    
    public IConnectionMultiplexer Connection { get; private set; } = null!;
    public string ConnectionString => _container.GetConnectionString();

    public RedisContainerFixture()
    {
        _container = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
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

        await _container.StartAsync();
        Connection = await ConnectionMultiplexer.ConnectAsync(ConnectionString);
    }

    public async Task DisposeAsync()
    {
        await Connection.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("Redis")]
public class RedisCollection : ICollectionFixture<RedisContainerFixture>
{
}
