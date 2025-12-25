using eQuantic.Core.CQS.EntityFramework.Outbox;
using eQuantic.Core.CQS.EntityFramework.Sagas;
using eQuantic.Core.CQS.EntityFramework.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.CQS.EntityFramework;

/// <summary>
/// Interface for DbContext that includes CQS tables
/// </summary>
public interface ICqsDbContext
{
    DbSet<SagaEntity> Sagas { get; }
    DbSet<OutboxEntity> OutboxMessages { get; }
    DbSet<ScheduledJobEntity> ScheduledJobs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}