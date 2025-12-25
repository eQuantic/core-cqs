using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Abstractions.Scheduling;
using eQuantic.Core.CQS.EntityFramework.Outbox;
using eQuantic.Core.CQS.EntityFramework.Sagas;
using eQuantic.Core.CQS.EntityFramework.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.EntityFramework.Extensions;

/// <summary>Extension methods for registering EF Core providers</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds all EF Core CQS implementations using your existing DbContext</summary>
    public static IServiceCollection AddCQSEntityFramework<TSagaData, TContext>(this IServiceCollection services)
        where TSagaData : ISagaData
        where TContext : DbContext, ICqsDbContext
    {
        services.AddScoped<ISagaRepository<TSagaData>, EfSagaRepository<TSagaData, TContext>>();
        services.AddScoped<IOutboxRepository, EfOutboxRepository<TContext>>();
        services.AddScoped<IJobScheduler, EfJobScheduler<TContext>>();
        return services;
    }
}