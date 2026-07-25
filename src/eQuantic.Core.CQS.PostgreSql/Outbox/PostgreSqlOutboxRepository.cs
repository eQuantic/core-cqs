using Dapper;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.PostgreSql.Options;
using Npgsql;

namespace eQuantic.Core.CQS.PostgreSql.Outbox;

/// <summary>PostgreSQL Outbox Repository</summary>
public class PostgreSqlOutboxRepository(PostgreSqlOptions options) : IOutboxRepository
{
    private const int Pending = (int)OutboxMessageState.Pending;
    private const int Processed = (int)OutboxMessageState.Processed;
    private const int Failed = (int)OutboxMessageState.Failed;

    /// <summary>
    /// How long a claimed message stays invisible to other relays. A relay that
    /// dies mid-delivery releases its batch after this window instead of
    /// stranding it.
    /// </summary>
    private static readonly TimeSpan ClaimLease = TimeSpan.FromMinutes(1);

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
                processed_at TIMESTAMP, attempts INT NOT NULL DEFAULT 0, last_error TEXT,
                next_attempt_at TIMESTAMP, correlation_id TEXT, context TEXT
            );
            -- Existing deployments predate next_attempt_at and context.
            ALTER TABLE {_table} ADD COLUMN IF NOT EXISTS next_attempt_at TIMESTAMP;
            ALTER TABLE {_table} ADD COLUMN IF NOT EXISTS context TEXT;
            CREATE INDEX IF NOT EXISTS idx_{_table.Replace(".", "_")}_state ON {_table} (state, next_attempt_at, created_at)");
        _initialized = true;
    }

    public async Task Add(IOutboxMessage msg, CancellationToken ct = default)
    {
        await EnsureTable();
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.ExecuteAsync($@"
            INSERT INTO {_table} (id, message_type, payload, state, created_at, correlation_id, context)
            VALUES (@Id, @MessageType, @Payload, @State, @CreatedAt, @CorrelationId, @Context)",
            new { msg.Id, msg.MessageType, msg.Payload, State = (int)msg.State, msg.CreatedAt, msg.CorrelationId, msg.Context });
    }

    /// <inheritdoc />
    /// <remarks>
    /// Claims the batch atomically: the inner select takes only rows no other
    /// relay holds (<c>FOR UPDATE SKIP LOCKED</c>) and the enclosing update
    /// leases them by pushing <c>next_attempt_at</c> out, so several relay
    /// instances can run without ever delivering the same message twice. One
    /// statement, so it needs no transaction spanning the call.
    /// </remarks>
    public async Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize = 100, CancellationToken ct = default)
    {
        await EnsureTable();
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        var rows = await conn.QueryAsync<OutboxMessage>($@"
            WITH claimed AS (
                UPDATE {_table} SET next_attempt_at = CURRENT_TIMESTAMP + @Lease
                WHERE id IN (
                    SELECT id FROM {_table}
                    WHERE state = @Pending AND (next_attempt_at IS NULL OR next_attempt_at <= CURRENT_TIMESTAMP)
                    ORDER BY created_at
                    LIMIT @Limit
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING *
            )
            SELECT id, message_type as MessageType, payload, state, created_at as CreatedAt,
                   processed_at as ProcessedAt, attempts, last_error as LastError,
                   next_attempt_at as NextAttemptAt, correlation_id as CorrelationId, context
            FROM claimed ORDER BY created_at",
            new { Limit = batchSize, Pending, Lease = ClaimLease });
        return rows.ToList();
    }

    public async Task MarkProcessed(Guid messageId, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.ExecuteAsync(
            $"UPDATE {_table} SET state=@Processed, processed_at=CURRENT_TIMESTAMP, next_attempt_at=NULL WHERE id=@Id",
            new { Id = messageId, Processed });
    }

    /// <inheritdoc />
    public async Task MarkFailed(
        Guid messageId, string error, int maxAttempts, TimeSpan backoff, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        // attempts+1 is evaluated once and drives both the state and the
        // reschedule, so a message cannot be dead-lettered and rescheduled at once.
        await conn.ExecuteAsync($@"
            UPDATE {_table} SET
                attempts = attempts + 1,
                last_error = @Error,
                state = CASE WHEN attempts + 1 >= @MaxAttempts THEN @Failed ELSE @Pending END,
                next_attempt_at = CASE WHEN attempts + 1 >= @MaxAttempts THEN NULL
                                       ELSE CURRENT_TIMESTAMP + (@Backoff * (attempts + 1)) END
            WHERE id = @Id",
            new { Id = messageId, Error = error, MaxAttempts = maxAttempts, Backoff = backoff, Failed, Pending });
    }

    public async Task CleanupProcessed(TimeSpan olderThan, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.ExecuteAsync(
            $"DELETE FROM {_table} WHERE state=@Processed AND processed_at < @Cutoff",
            new { Cutoff = DateTime.UtcNow.Subtract(olderThan), Processed });
    }
}
