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
            CorrelationId = msg.CorrelationId
        });
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize = 100, CancellationToken ct = default)
    {
        var entities = await _context.OutboxMessages.AsNoTracking()
            .Where(m => m.State == (int)OutboxMessageState.Pending)
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
            CorrelationId = e.CorrelationId
        }).ToList<IOutboxMessage>();
    }

    public async Task MarkProcessed(Guid messageId, CancellationToken ct = default)
    {
        var entity = await _context.OutboxMessages.FindAsync(new object[] { messageId }, ct);
        if (entity != null)
        {
            entity.State = (int)OutboxMessageState.Processed;
            entity.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task MarkFailed(Guid messageId, string error, CancellationToken ct = default)
    {
        var entity = await _context.OutboxMessages.FindAsync(new object[] { messageId }, ct);
        if (entity != null)
        {
            entity.State = (int)OutboxMessageState.Failed;
            entity.LastError = error;
            entity.Attempts++;
            await _context.SaveChangesAsync(ct);
        }
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