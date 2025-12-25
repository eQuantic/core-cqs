using Dapper;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.PostgreSql.Options;
using Npgsql;

namespace eQuantic.Core.CQS.PostgreSql.Outbox;

/// <summary>PostgreSQL Outbox Repository</summary>
public class PostgreSqlOutboxRepository(PostgreSqlOptions options) : IOutboxRepository
{
    private readonly string _table = $"{options.Schema}.outbox";
    private bool _initialized;

    private async Task EnsureTable()
    {
        if (_initialized || !options.AutoCreateTables) return;
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.ExecuteAsync($@"
            CREATE TABLE IF NOT EXISTS {_table} (
                id UUID PRIMARY KEY, message_type TEXT NOT NULL, payload TEXT NOT NULL,
                state INT NOT NULL DEFAULT 0, created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                processed_at TIMESTAMP, attempts INT NOT NULL DEFAULT 0, last_error TEXT, correlation_id TEXT
            );
            CREATE INDEX IF NOT EXISTS idx_{_table.Replace(".", "_")}_state ON {_table} (state, created_at)");
        _initialized = true;
    }

    public async Task Add(IOutboxMessage msg, CancellationToken ct = default)
    {
        await EnsureTable();
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.ExecuteAsync($@"
            INSERT INTO {_table} (id, message_type, payload, state, created_at, correlation_id)
            VALUES (@Id, @MessageType, @Payload, @State, @CreatedAt, @CorrelationId)",
            new { msg.Id, msg.MessageType, msg.Payload, State = (int)msg.State, msg.CreatedAt, msg.CorrelationId });
    }

    public async Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize = 100, CancellationToken ct = default)
    {
        await EnsureTable();
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        var rows = await conn.QueryAsync<OutboxMessage>($@"
            SELECT id, message_type as MessageType, payload, state, created_at as CreatedAt, processed_at as ProcessedAt, attempts, last_error as LastError, correlation_id as CorrelationId
            FROM {_table} WHERE state = 0 ORDER BY created_at LIMIT @Limit", new { Limit = batchSize });
        return rows.ToList();
    }

    public async Task MarkProcessed(Guid messageId, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.ExecuteAsync($"UPDATE {_table} SET state=1, processed_at=CURRENT_TIMESTAMP WHERE id=@Id", new { Id = messageId });
    }

    public async Task MarkFailed(Guid messageId, string error, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.ExecuteAsync($"UPDATE {_table} SET state=3, last_error=@Error, attempts=attempts+1 WHERE id=@Id", new { Id = messageId, Error = error });
    }

    public async Task CleanupProcessed(TimeSpan olderThan, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.ExecuteAsync($"DELETE FROM {_table} WHERE state=1 AND processed_at < @Cutoff", new { Cutoff = DateTime.UtcNow.Subtract(olderThan) });
    }
}