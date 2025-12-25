using System.Text.Json;
using Dapper;
using eQuantic.Core.CQS.Abstractions.Scheduling;
using eQuantic.Core.CQS.PostgreSql.Options;
using Npgsql;

namespace eQuantic.Core.CQS.PostgreSql.Scheduling;

/// <summary>PostgreSQL Job Scheduler</summary>
public class PostgreSqlJobScheduler(PostgreSqlOptions options) : IJobScheduler
{
    private readonly string _table = $"{options.Schema}.scheduled_jobs";
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private bool _initialized;

    private async Task EnsureTable()
    {
        if (_initialized || !options.AutoCreateTables) return;
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.ExecuteAsync($@"
            CREATE TABLE IF NOT EXISTS {_table} (
                id UUID PRIMARY KEY, scheduled_at TIMESTAMP NOT NULL, request_type TEXT NOT NULL, request_json TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_{_table.Replace(".", "_")}_schedule ON {_table} (scheduled_at)");
        _initialized = true;
    }

    public async Task<Guid> Schedule<TRequest>(TRequest request, DateTime executeAt, CancellationToken ct = default)
    {
        await EnsureTable();
        var id = Guid.NewGuid();
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.ExecuteAsync($@"INSERT INTO {_table} (id, scheduled_at, request_type, request_json) VALUES (@Id, @ScheduledAt, @RequestType, @RequestJson)",
            new { Id = id, ScheduledAt = executeAt, RequestType = typeof(TRequest).AssemblyQualifiedName, RequestJson = JsonSerializer.Serialize(request, _json) });
        return id;
    }

    public Task<Guid> Schedule<TRequest>(TRequest request, TimeSpan delay, CancellationToken ct = default) =>
        Schedule(request, DateTime.UtcNow.Add(delay), ct);

    public async Task<bool> Cancel(Guid jobId, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        var rows = await conn.ExecuteAsync($"DELETE FROM {_table} WHERE id = @Id", new { Id = jobId });
        return rows > 0;
    }

    public Task<IReadOnlyList<IScheduledJob>> GetPending(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<IScheduledJob>>(Array.Empty<IScheduledJob>()); // Simplified
}