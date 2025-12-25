using eQuantic.Core.CQS.Abstractions.Options;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Abstractions.Scheduling;
using eQuantic.Core.CQS.MongoDb.Options;
using eQuantic.Core.CQS.MongoDb.Outbox;
using eQuantic.Core.CQS.MongoDb.Sagas;
using eQuantic.Core.CQS.MongoDb.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace eQuantic.Core.CQS.MongoDb.Extensions;

/// <summary>
/// CQSOptions extension methods for MongoDB providers
/// </summary>
public static class CQSOptionsExtensions
{
    static CQSOptionsExtensions()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }

    /// <summary>
    /// Configures MongoDB as the persistence provider for Sagas, Outbox, and Job Scheduler
    /// </summary>
    public static CQSOptions UseMongoDb<TSagaData>(
        this CQSOptions options, 
        Action<MongoDbOptions>? configure = null)
        where TSagaData : ISagaData
    {
        var mongoOptions = new MongoDbOptions();
        configure?.Invoke(mongoOptions);

        options.Services.AddSingleton(mongoOptions);
        options.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoOptions.ConnectionString));
        options.Services.AddScoped<ISagaRepository<TSagaData>, MongoDbSagaRepository<TSagaData>>();
        options.Services.AddScoped<IOutboxRepository, MongoDbOutboxRepository>();
        options.Services.AddScoped<IJobScheduler, MongoDbJobScheduler>();

        return options;
    }

    /// <summary>
    /// Configures MongoDB as the persistence provider (without typed saga)
    /// </summary>
    public static CQSOptions UseMongoDb(
        this CQSOptions options, 
        Action<MongoDbOptions>? configure = null)
    {
        var mongoOptions = new MongoDbOptions();
        configure?.Invoke(mongoOptions);

        options.Services.AddSingleton(mongoOptions);
        options.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoOptions.ConnectionString));
        options.Services.AddScoped<IOutboxRepository, MongoDbOutboxRepository>();
        options.Services.AddScoped<IJobScheduler, MongoDbJobScheduler>();

        return options;
    }
}
