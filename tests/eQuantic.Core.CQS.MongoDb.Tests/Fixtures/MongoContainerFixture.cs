using DotNet.Testcontainers.Builders;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using eQuantic.Core.CQS.Tests.Commons.Fixtures;
using Xunit;

namespace eQuantic.Core.CQS.MongoDb.Tests.Fixtures;

/// <summary>MongoDB container fixture for integration tests</summary>
public class MongoContainerFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container;
    private static bool _serialzersRegistered = false;
    
    public IMongoClient Client { get; private set; } = null!;
    public IMongoDatabase Database { get; private set; } = null!;
    public string ConnectionString => _container.GetConnectionString();

    public MongoContainerFixture()
    {
        _container = new MongoDbBuilder()
            .WithImage("mongo:7")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(27017)
                .UntilMessageIsLogged("Waiting for connections"))
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Register global serializers only once
        if (!_serialzersRegistered)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            _serialzersRegistered = true;
        }
        
        // A bounded wait: whatever keeps a container from coming up, the suite reports it instead of
        // stalling the run until the agent's own timeout kills it.
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        await _container.StartAsync(timeout.Token);
        
        var settings = MongoClientSettings.FromConnectionString(ConnectionString);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
        
        Client = new MongoClient(settings);
        Database = Client.GetDatabase("test_db");
        
        // Verify connection
        await Database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
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

[CollectionDefinition("MongoDB")]
public class MongoDbCollection : ICollectionFixture<MongoContainerFixture>
{
}
