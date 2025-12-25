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

/// <summary>Extension methods for registering MongoDB providers</summary>
public static class ServiceCollectionExtensions
{
    static ServiceCollectionExtensions()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }

    /// <summary>Adds all MongoDB CQS implementations</summary>
    public static IServiceCollection AddCQSMongoDb<TSagaData>(this IServiceCollection services, Action<MongoDbOptions>? configure = null)
        where TSagaData : ISagaData
    {
        var options = new MongoDbOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IMongoClient>(_ => new MongoClient(options.ConnectionString));
        services.AddScoped<ISagaRepository<TSagaData>, MongoDbSagaRepository<TSagaData>>();
        services.AddScoped<IOutboxRepository, MongoDbOutboxRepository>();
        services.AddScoped<IJobScheduler, MongoDbJobScheduler>();

        return services;
    }
}