using eQuantic.Core.CQS.Abstractions.Options;
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

/// <summary>
/// CQSOptions extension methods for Redis providers
/// </summary>
public static class CQSOptionsExtensions
{
    /// <summary>
    /// Configures Redis as the persistence provider for Sagas, Outbox, and Job Scheduler
    /// </summary>
    /// <typeparam name="TSagaData">The saga data type to register</typeparam>
    /// <param name="options">The CQS options</param>
    /// <param name="configure">Redis configuration</param>
    /// <returns>The CQS options for chaining</returns>
    public static CQSOptions UseRedis<TSagaData>(
        this CQSOptions options, 
        Action<RedisOptions>? configure = null)
        where TSagaData : ISagaData
    {
        var redisOptions = new RedisOptions();
        configure?.Invoke(redisOptions);

        options.Services.AddSingleton(redisOptions);
        options.Services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect(redisOptions.ConnectionString));
        options.Services.AddScoped<ISagaRepository<TSagaData>, RedisSagaRepository<TSagaData>>();
        options.Services.AddScoped<IOutboxRepository, RedisOutboxRepository>();
        options.Services.AddScoped<IJobScheduler, RedisJobScheduler>();

        return options;
    }

    /// <summary>
    /// Configures Redis as the persistence provider (without typed saga)
    /// </summary>
    public static CQSOptions UseRedis(
        this CQSOptions options, 
        Action<RedisOptions>? configure = null)
    {
        var redisOptions = new RedisOptions();
        configure?.Invoke(redisOptions);

        options.Services.AddSingleton(redisOptions);
        options.Services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect(redisOptions.ConnectionString));
        options.Services.AddScoped<IOutboxRepository, RedisOutboxRepository>();
        options.Services.AddScoped<IJobScheduler, RedisJobScheduler>();

        return options;
    }
}
