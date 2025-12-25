using eQuantic.Core.CQS.Abstractions.Options;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Abstractions.Scheduling;
using eQuantic.Core.CQS.EntityFramework.Outbox;
using eQuantic.Core.CQS.EntityFramework.Sagas;
using eQuantic.Core.CQS.EntityFramework.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.EntityFramework.Extensions;

/// <summary>
/// CQSOptions extension methods for Entity Framework Core providers
/// </summary>
public static class CQSOptionsExtensions
{
    /// <summary>
    /// Configures Entity Framework Core as the persistence provider for Sagas, Outbox, and Job Scheduler
    /// </summary>
    /// <typeparam name="TSagaData">The saga data type</typeparam>
    /// <typeparam name="TContext">Your DbContext that implements ICqsDbContext</typeparam>
    public static CQSOptions UseEntityFramework<TSagaData, TContext>(this CQSOptions options)
        where TSagaData : ISagaData
        where TContext : DbContext, ICqsDbContext
    {
        options.Services.AddScoped<ISagaRepository<TSagaData>, EfSagaRepository<TSagaData, TContext>>();
        options.Services.AddScoped<IOutboxRepository, EfOutboxRepository<TContext>>();
        options.Services.AddScoped<IJobScheduler, EfJobScheduler<TContext>>();

        return options;
    }

    /// <summary>
    /// Configures Entity Framework Core as the persistence provider (without typed saga)
    /// </summary>
    /// <typeparam name="TContext">Your DbContext that implements ICqsDbContext</typeparam>
    public static CQSOptions UseEntityFramework<TContext>(this CQSOptions options)
        where TContext : DbContext, ICqsDbContext
    {
        options.Services.AddScoped<IOutboxRepository, EfOutboxRepository<TContext>>();
        options.Services.AddScoped<IJobScheduler, EfJobScheduler<TContext>>();

        return options;
    }
}
