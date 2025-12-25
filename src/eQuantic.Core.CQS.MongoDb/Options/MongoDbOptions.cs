namespace eQuantic.Core.CQS.MongoDb.Options;

/// <summary>Configuration options for MongoDB providers</summary>
public class MongoDbOptions
{
    /// <summary>MongoDB connection string</summary>
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    /// <summary>Database name</summary>
    public string DatabaseName { get; set; } = "cqs";
    /// <summary>Collection prefix</summary>
    public string CollectionPrefix { get; set; } = "";
}