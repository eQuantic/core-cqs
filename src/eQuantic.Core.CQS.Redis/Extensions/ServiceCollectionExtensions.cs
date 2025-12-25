using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Abstractions.Scheduling;
using eQuantic.Core.CQS.Redis.Options;
using eQuantic.Core.CQS.Redis.Outbox;
using eQuantic.Core.CQS.Redis.Sagas;
using eQuantic.Core.CQS.Redis.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace eQuantic.Core.CQS.Redis.Extensions;

/// <summary>Extension methods for registering Redis providers</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds all Redis CQS implementations</summary>
    public static IServiceCollection AddCQSRedis<TSagaData>(this IServiceCollection services, Action<RedisOptions>? configure = null)
        where TSagaData : ISagaData
    {
        var options = new RedisOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(options.ConnectionString));
        services.AddScoped<ISagaRepository<TSagaData>, RedisSagaRepository<TSagaData>>();
        services.AddScoped<IOutboxRepository, RedisOutboxRepository>();
        services.AddScoped<IJobScheduler, RedisJobScheduler>();

        return services;
    }
}