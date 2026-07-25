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

    /// <inheritdoc />
    /// <remarks>At-least-once: the in-memory store cannot lock, so run a single relay.</remarks>
    public Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var pending = _messages.Values
            .Where(m => m.State == OutboxMessageState.Pending)
            .Where(m => m.NextAttemptAt is null || m.NextAttemptAt <= now)
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

    public Task MarkFailed(
        Guid messageId,
        string error,
        int maxAttempts,
        TimeSpan backoff,
        CancellationToken cancellationToken = default)
    {
        if (_messages.TryGetValue(messageId, out var message))
        {
            message.Attempts++;
            message.LastError = error;

            var exhausted = message.Attempts >= maxAttempts;
            message.State = exhausted ? OutboxMessageState.Failed : OutboxMessageState.Pending;
            message.NextAttemptAt = exhausted ? null : DateTime.UtcNow + backoff * message.Attempts;
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