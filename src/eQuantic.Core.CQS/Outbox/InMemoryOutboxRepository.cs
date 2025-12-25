using System.Collections.Concurrent;
using eQuantic.Core.CQS.Abstractions.Outbox;

namespace eQuantic.Core.CQS.Outbox;

/// <summary>
/// In-memory outbox repository (for development/testing)
/// </summary>
public class InMemoryOutboxRepository : IOutboxRepository
{
    private readonly ConcurrentDictionary<Guid, IOutboxMessage> _messages = new();

    public Task Add(IOutboxMessage message, CancellationToken cancellationToken = default)
    {
        _messages.TryAdd(message.Id, message);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var pending = _messages.Values
            .Where(m => m.State == OutboxMessageState.Pending)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToList();

        return Task.FromResult<IReadOnlyList<IOutboxMessage>>(pending);
    }

    public Task MarkProcessed(Guid messageId, CancellationToken cancellationToken = default)
    {
        if (_messages.TryGetValue(messageId, out var message))
        {
            message.State = OutboxMessageState.Processed;
            message.ProcessedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task MarkFailed(Guid messageId, string error, CancellationToken cancellationToken = default)
    {
        if (_messages.TryGetValue(messageId, out var message))
        {
            message.State = OutboxMessageState.Failed;
            message.LastError = error;
            message.Attempts++;
        }
        return Task.CompletedTask;
    }

    public Task CleanupProcessed(TimeSpan olderThan, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.Subtract(olderThan);
        var toRemove = _messages.Values
            .Where(m => m.State == OutboxMessageState.Processed && m.ProcessedAt < cutoff)
            .Select(m => m.Id)
            .ToList();

        foreach (var id in toRemove)
        {
            _messages.TryRemove(id, out _);
        }

        return Task.CompletedTask;
    }
}