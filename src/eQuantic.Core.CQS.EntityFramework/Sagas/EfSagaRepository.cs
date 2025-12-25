using System.Text.Json;
using eQuantic.Core.CQS.Abstractions.Sagas;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.CQS.EntityFramework.Sagas;

/// <summary>EF Core Saga Repository</summary>
public class EfSagaRepository<TData, TContext> : ISagaRepository<TData>
    where TData : ISagaData
    where TContext : DbContext, ICqsDbContext
{
    private readonly TContext _context;
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly string _sagaType = typeof(TData).Name;

    public EfSagaRepository(TContext context) => _context = context;

    public async Task Save(TData data, CancellationToken ct = default)
    {
        var entity = await _context.Sagas.FindAsync(new object[] { data.SagaId }, ct);
        if (entity == null)
        {
            entity = new SagaEntity { Id = data.SagaId, SagaType = _sagaType };
            _context.Sagas.Add(entity);
        }
        entity.State = (int)data.State;
        entity.CurrentStep = data.CurrentStep;
        entity.StartedAt = data.StartedAt;
        entity.CompletedAt = data.CompletedAt;
        entity.Data = JsonSerializer.Serialize(data, _json);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<TData?> Load(Guid sagaId, CancellationToken ct = default)
    {
        var entity = await _context.Sagas.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sagaId && s.SagaType == _sagaType, ct);
        return entity == null ? default : JsonSerializer.Deserialize<TData>(entity.Data, _json);
    }

    public async Task<IReadOnlyList<TData>> Find(Func<TData, bool> predicate, CancellationToken ct = default)
    {
        var entities = await _context.Sagas.AsNoTracking()
            .Where(s => s.SagaType == _sagaType).ToListAsync(ct);
        return entities.Select(e => JsonSerializer.Deserialize<TData>(e.Data, _json)!)
            .Where(predicate).ToList();
    }

    public async Task<IReadOnlyList<TData>> GetIncomplete(CancellationToken ct = default)
    {
        var states = new[] { (int)SagaState.InProgress, (int)SagaState.NotStarted };
        var entities = await _context.Sagas.AsNoTracking()
            .Where(s => s.SagaType == _sagaType && states.Contains(s.State)).ToListAsync(ct);
        return entities.Select(e => JsonSerializer.Deserialize<TData>(e.Data, _json)!).ToList();
    }

    public async Task Delete(Guid sagaId, CancellationToken ct = default)
    {
        var entity = await _context.Sagas.FindAsync(new object[] { sagaId }, ct);
        if (entity != null)
        {
            _context.Sagas.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}