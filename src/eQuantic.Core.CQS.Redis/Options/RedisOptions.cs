namespace eQuantic.Core.CQS.Redis.Options;

/// <summary>
/// Configuration options for Redis providers
/// </summary>
public class RedisOptions
{
    /// <summary>Redis connection string</summary>
    public string ConnectionString { get; set; } = "localhost:6379";
    /// <summary>Key prefix for all data</summary>
    public string KeyPrefix { get; set; } = "cqs:";
    /// <summary>Default expiration (null = no expiration)</summary>
    public TimeSpan? DefaultExpiration { get; set; }
    /// <summary>Database number to use</summary>
    public int Database { get; set; } = 0;
}