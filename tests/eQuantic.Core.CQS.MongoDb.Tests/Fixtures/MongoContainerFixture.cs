using DotNet.Testcontainers.Builders;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Testcontainers.MongoDb;
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
        
        await _container.StartAsync();
        
        var settings = MongoClientSettings.FromConnectionString(ConnectionString);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
        
        Client = new MongoClient(settings);
        Database = Client.GetDatabase("test_db");
        
        // Verify connection
        await Database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("MongoDB")]
public class MongoDbCollection : ICollectionFixture<MongoContainerFixture>
{
}
