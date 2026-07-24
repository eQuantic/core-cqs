using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Options;

namespace eQuantic.Core.CQS.Data.Outbox;

/// <summary>
/// An <see cref="IOutboxRepository" /> backed by the native eQuantic.Core.Data engine. Its defining property is
/// that <see cref="Add" /> <b>stages</b> the message on the ambient <see cref="IUnitOfWork" /> rather than
/// committing it, so when the caller commits — the same commit that writes the aggregate — the message and the
/// aggregate land in <b>one transaction</b>. This is the transactional-outbox guarantee, and it holds on every
/// store the engine supports.
/// <para>
/// The relay-side operations (<see cref="GetPending" />, <see cref="MarkProcessed" />, <see cref="MarkFailed" />,
/// <see cref="CleanupProcessed" />) run on their own and take effect immediately: the mark operations are
/// server-side set-based updates and the cleanup is a set-based delete — no row is loaded to change its state.
/// </para>
/// </summary>
public sealed class NativeOutboxRepository : IOutboxRepository
{
    private readonly IAsyncRepository<OutboxDataEntity, Guid> _repository;

    /// <summary>Initializes the repository over a native aggregate repository for <see cref="OutboxDataEntity" />.</summary>
    /// <param name="repository">The native repository, built over the ambient unit of work.</param>
    public NativeOutboxRepository(IAsyncRepository<OutboxDataEntity, Guid> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    /// <remarks>Stages the message on the ambient unit of work; it is persisted when that unit of work commits.</remarks>
    public Task Add(IOutboxMessage message, CancellationToken cancellationToken = default) =>
        _repository.AddAsync(new OutboxDataEntity
        {
            Id = message.Id,
            MessageType = message.MessageType,
            Payload = message.Payload,
            State = (int)message.State,
            CreatedAt = message.CreatedAt,
            ProcessedAt = message.ProcessedAt,
            Attempts = message.Attempts,
            LastError = message.LastError,
            CorrelationId = message.CorrelationId,
        }, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<IOutboxMessage>> GetPending(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var pending = (int)OutboxMessageState.Pending;
        var options = new QueryOptions<OutboxDataEntity>()
            .Where(message => message.State == pending)
            .OrderBy(message => message.CreatedAt);

        var page = await _repository.GetPagedAsync(new PageRequest(1, batchSize), options, cancellationToken)
            .ConfigureAwait(false);

        return page.Items.Select(Map).ToList();
    }

    /// <inheritdoc />
    public Task MarkProcessed(Guid messageId, CancellationToken cancellationToken = default)
    {
        var processedAt = DateTime.UtcNow;
        return _repository.UpdateManyAsync(
            message => message.Id == messageId,
            message => new OutboxDataEntity { State = (int)OutboxMessageState.Processed, ProcessedAt = processedAt },
            cancellationToken);
    }

    /// <inheritdoc />
    public Task MarkFailed(Guid messageId, string error, CancellationToken cancellationToken = default) =>
        _repository.UpdateManyAsync(
            message => message.Id == messageId,
            message => new OutboxDataEntity
            {
                State = (int)OutboxMessageState.Failed,
                Attempts = message.Attempts + 1,
                LastError = error,
            },
            cancellationToken);

    /// <inheritdoc />
    public Task CleanupProcessed(TimeSpan olderThan, CancellationToken cancellationToken = default)
    {
        var processed = (int)OutboxMessageState.Processed;
        var threshold = DateTime.UtcNow - olderThan;
        return _repository.DeleteManyAsync(
            message => message.State == processed && message.ProcessedAt != null && message.ProcessedAt < threshold,
            cancellationToken);
    }

    private static IOutboxMessage Map(OutboxDataEntity entity) => new OutboxMessage
    {
        Id = entity.Id,
        MessageType = entity.MessageType,
        Payload = entity.Payload,
        State = (OutboxMessageState)entity.State,
        CreatedAt = entity.CreatedAt,
        ProcessedAt = entity.ProcessedAt,
        Attempts = entity.Attempts,
        LastError = entity.LastError,
        CorrelationId = entity.CorrelationId,
    };
}
