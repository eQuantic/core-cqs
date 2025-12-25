namespace eQuantic.Core.CQS.PostgreSql.Options;

/// <summary>Configuration options for PostgreSQL providers</summary>
public class PostgreSqlOptions
{
    /// <summary>Connection string</summary>
    public string ConnectionString { get; set; } = "Host=localhost;Database=cqs;Username=postgres;Password=postgres";
    /// <summary>Schema name</summary>
    public string Schema { get; set; } = "public";
    /// <summary>Auto-create tables</summary>
    public bool AutoCreateTables { get; set; } = true;
}