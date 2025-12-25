namespace eQuantic.Core.CQS.Abstractions.Sagas;

/// <summary>
/// Interface for saga persistence
/// </summary>
/// <typeparam name="TData">The saga data type</typeparam>
public interface ISagaRepository<TData> where TData : ISagaData
{
    /// <summary>
    /// Saves or updates saga data
    /// </summary>
    Task Save(TData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads saga data by ID
    /// </summary>
    Task<TData?> Load(Guid sagaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds sagas matching a predicate
    /// </summary>
    Task<IReadOnlyList<TData>> Find(Func<TData, bool> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all incomplete sagas (for recovery)
    /// </summary>
    Task<IReadOnlyList<TData>> GetIncomplete(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes saga data
    /// </summary>
    Task Delete(Guid sagaId, CancellationToken cancellationToken = default);
}