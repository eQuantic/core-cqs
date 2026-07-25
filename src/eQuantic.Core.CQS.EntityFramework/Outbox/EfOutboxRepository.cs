using eQuantic.Core.CQS.Abstractions.Outbox;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.CQS.EntityFramework.Outbox;

/// <summary>EF Core Outbox Repository</summary>
public class EfOutboxRepository<TContext> : IOutboxRepository
    where TContext : DbContext, ICqsDbContext
{
    private readonly TContext _context;

    public EfOutboxRepository(TContext context) => _context = context;

    public async Task Add(IOutboxMessage msg, CancellationToken ct = default)
    {
        _context.OutboxMessages.Add(new OutboxEntity
        {
            Id = msg.Id,
            MessageType = msg.MessageType,
            Payload = msg.Payload,
            State = (int)msg.State,
            CreatedAt = msg.CreatedAt,
            NextAttemptAt = msg.NextAttemptAt,
            CorrelationId = msg.CorrelationId,
            Context = msg.Context
        });
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    /// <remarks>
    /// At-least-once: EF Core has no provider-agnostic way to claim rows under a
    /// lock, so run a single relay instance. On PostgreSQL prefer
    /// <c>PostgreSqlOutboxRepository</c>, which claims its batch atomically.
    /// </remarks>
    public async Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize = 100, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var entities = await _context.OutboxMessages.AsNoTracking()
            .Where(m => m.State == (int)OutboxMessageState.Pending)
            .Where(m => m.NextAttemptAt == null || m.NextAttemptAt <= now)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);

        return entities.Select(e => new OutboxMessage
        {
            Id = e.Id,
            MessageType = e.MessageType,
            Payload = e.Payload,
            State = (OutboxMessageState)e.State,
            CreatedAt = e.CreatedAt,
            ProcessedAt = e.ProcessedAt,
            Attempts = e.Attempts,
            LastError = e.LastError,
            NextAttemptAt = e.NextAttemptAt,
            CorrelationId = e.CorrelationId,
            Context = e.Context
        }).ToList<IOutboxMessage>();
    }

    public async Task MarkProcessed(Guid messageId, CancellationToken ct = default)
    {
        var entity = await _context.OutboxMessages.FindAsync(new object[] { messageId }, ct);
        if (entity != null)
        {
            entity.State = (int)OutboxMessageState.Processed;
            entity.ProcessedAt = DateTime.UtcNow;
            entity.NextAttemptAt = null;
            await _context.SaveChangesAsync(ct);
        }
    }

    /// <inheritdoc />
    public async Task MarkFailed(
        Guid messageId, string error, int maxAttempts, TimeSpan backoff, CancellationToken ct = default)
    {
        var entity = await _context.OutboxMessages.FindAsync(new object[] { messageId }, ct);
        if (entity == null) return;

        entity.Attempts++;
        entity.LastError = error;

        var exhausted = entity.Attempts >= maxAttempts;
        entity.State = (int)(exhausted ? OutboxMessageState.Failed : OutboxMessageState.Pending);
        entity.NextAttemptAt = exhausted ? null : DateTime.UtcNow + backoff * entity.Attempts;

        await _context.SaveChangesAsync(ct);
    }

    public async Task CleanupProcessed(TimeSpan olderThan, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.Subtract(olderThan);
        var old = await _context.OutboxMessages
            .Where(m => m.State == (int)OutboxMessageState.Processed && m.ProcessedAt < cutoff)
            .ToListAsync(ct);
        _context.OutboxMessages.RemoveRange(old);
        await _context.SaveChangesAsync(ct);
    }
}