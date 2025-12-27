using eQuantic.Core.CQS.EntityFramework;
using eQuantic.Core.CQS.EntityFramework.Extensions;
using eQuantic.Core.CQS.EntityFramework.Outbox;
using eQuantic.Core.CQS.EntityFramework.Sagas;
using eQuantic.Core.CQS.EntityFramework.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.CQS.EntityFramework.Tests.Fixtures;

/// <summary>Test DbContext for EntityFramework tests</summary>
public class TestDbContext : DbContext, ICqsDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<SagaEntity> Sagas { get; set; } = null!;
    public DbSet<OutboxEntity> OutboxMessages { get; set; } = null!;
    public DbSet<ScheduledJobEntity> ScheduledJobs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureCQS();
    }
}
