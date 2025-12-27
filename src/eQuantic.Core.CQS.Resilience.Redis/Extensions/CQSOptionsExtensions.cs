using eQuantic.Core.CQS.Abstractions.Options;
using eQuantic.Core.CQS.Abstractions.Resilience;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace eQuantic.Core.CQS.Resilience.Redis.Extensions;

/// <summary>
/// Extension methods for configuring Redis dead letter handler.
/// </summary>
public static class CQSOptionsExtensions
{
    /// <summary>
    /// Configures Redis as the dead letter storage.
    /// </summary>
    public static CQSOptions UseRedisDeadLetter(
        this CQSOptions options, 
        Action<RedisDeadLetterOptions>? configure = null)
    {
        var redisOptions = new RedisDeadLetterOptions();
        configure?.Invoke(redisOptions);
        
        options.Services.AddSingleton(redisOptions);
        options.Services.AddSingleton<IDeadLetterHandler, RedisDeadLetterHandler>();
        
        return options;
    }

    /// <summary>
    /// Configures Redis dead letter with a new connection.
    /// </summary>
    public static CQSOptions UseRedisDeadLetter(
        this CQSOptions options,
        string connectionString,
        Action<RedisDeadLetterOptions>? configure = null)
    {
        var redisOptions = new RedisDeadLetterOptions();
        configure?.Invoke(redisOptions);
        
        options.Services.AddSingleton(redisOptions);
        options.Services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect(connectionString));
        options.Services.AddSingleton<IDeadLetterHandler, RedisDeadLetterHandler>();
        
        return options;
    }
}
