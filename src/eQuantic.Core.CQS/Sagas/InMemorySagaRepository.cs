using eQuantic.Core.CQS.Abstractions.Sagas;

namespace eQuantic.Core.CQS.Sagas;

/// <summary>
/// In-memory implementation of saga repository (for testing/development)
/// </summary>
/// <typeparam name="TData">The saga data type</typeparam>
public class InMemorySagaRepository<TData> : ISagaRepository<TData> where TData : ISagaData
{
    private readonly Dictionary<Guid, TData> _sagas = new();

    public Task Save(TData data, CancellationToken cancellationToken = default)
    {
        _sagas[data.SagaId] = data;
        return Task.CompletedTask;
    }

    public Task<TData?> Load(Guid sagaId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_sagas.GetValueOrDefault(sagaId));
    }

    public Task<IReadOnlyList<TData>> Find(Func<TData, bool> predicate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TData>>(_sagas.Values.Where(predicate).ToList());
    }

    public Task<IReadOnlyList<TData>> GetIncomplete(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TData>>(
            _sagas.Values.Where(s => s.State == SagaState.InProgress || s.State == SagaState.NotStarted).ToList());
    }

    public Task Delete(Guid sagaId, CancellationToken cancellationToken = default)
    {
        _sagas.Remove(sagaId);
        return Task.CompletedTask;
    }
}
