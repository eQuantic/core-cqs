using System.Text.Json;
using Dapper;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.PostgreSql.Options;
using Npgsql;

namespace eQuantic.Core.CQS.PostgreSql.Sagas;

/// <summary>PostgreSQL Saga Repository</summary>
public class PostgreSqlSagaRepository<TData>(PostgreSqlOptions options) : ISagaRepository<TData>
    where TData : ISagaData
{
    private readonly string _table = $"{options.Schema}.saga_{typeof(TData).Name.ToLowerInvariant()}";
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private bool _initialized;

    private async Task EnsureTable(CancellationToken ct)
    {
        if (_initialized || !options.AutoCreateTables) return;
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.ExecuteAsync($@"
            CREATE TABLE IF NOT EXISTS {_table} (
                saga_id UUID PRIMARY KEY, state INT NOT NULL, current_step INT NOT NULL,
                started_at TIMESTAMP NOT NULL, completed_at TIMESTAMP, data JSONB NOT NULL
            )");
        _initialized = true;
    }

    public async Task Save(TData data, CancellationToken ct = default)
    {
        await EnsureTable(ct);
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.ExecuteAsync($@"
            INSERT INTO {_table} (saga_id, state, current_step, started_at, completed_at, data)
            VALUES (@SagaId, @State, @CurrentStep, @StartedAt, @CompletedAt, @Data::jsonb)
            ON CONFLICT (saga_id) DO UPDATE SET state=@State, current_step=@CurrentStep, completed_at=@CompletedAt, data=@Data::jsonb",
            new { data.SagaId, State = (int)data.State, data.CurrentStep, data.StartedAt, data.CompletedAt, Data = JsonSerializer.Serialize(data, _json) });
    }

    public async Task<TData?> Load(Guid sagaId, CancellationToken ct = default)
    {
        await EnsureTable(ct);
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        var json = await conn.QueryFirstOrDefaultAsync<string>($"SELECT data FROM {_table} WHERE saga_id = @SagaId", new { SagaId = sagaId });
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<TData>(json, _json);
    }

    public async Task<IReadOnlyList<TData>> Find(Func<TData, bool> predicate, CancellationToken ct = default)
    {
        await EnsureTable(ct);
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        var list = await conn.QueryAsync<string>($"SELECT data FROM {_table}");
        return list.Select(j => JsonSerializer.Deserialize<TData>(j, _json)!).Where(predicate).ToList();
    }

    public Task<IReadOnlyList<TData>> GetIncomplete(CancellationToken ct = default) =>
        Find(s => s.State is SagaState.InProgress or SagaState.NotStarted, ct);

    public async Task Delete(Guid sagaId, CancellationToken ct = default)
    {
        await EnsureTable(ct);
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.ExecuteAsync($"DELETE FROM {_table} WHERE saga_id = @SagaId", new { SagaId = sagaId });
    }
}