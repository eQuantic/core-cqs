using eQuantic.Core.CQS.EntityFramework.Outbox;
using eQuantic.Core.CQS.EntityFramework.Sagas;
using eQuantic.Core.CQS.EntityFramework.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.CQS.EntityFramework.Extensions;

/// <summary>
/// Extension methods for configuring CQS entities in EF Core
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configures the CQS entities (Saga, Outbox, ScheduledJobs) in your DbContext
    /// </summary>
    public static ModelBuilder ConfigureCQS(this ModelBuilder modelBuilder, string? tablePrefix = null, string? schema = null)
    {
        var prefix = tablePrefix ?? "";

        modelBuilder.Entity<SagaEntity>(entity =>
        {
            entity.ToTable($"{prefix}Sagas", schema);
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.State);
            entity.HasIndex(e => e.SagaType);
            entity.Property(e => e.Data).HasColumnType("text");
        });

        modelBuilder.Entity<OutboxEntity>(entity =>
        {
            entity.ToTable($"{prefix}OutboxMessages", schema);
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.State, e.CreatedAt });
            entity.Property(e => e.Payload).HasColumnType("text");
        });

        modelBuilder.Entity<ScheduledJobEntity>(entity =>
        {
            entity.ToTable($"{prefix}ScheduledJobs", schema);
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ScheduledAt);
            entity.Property(e => e.RequestJson).HasColumnType("text");
        });

        return modelBuilder;
    }
}