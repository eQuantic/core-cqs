using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Abstractions.Scheduling;
using eQuantic.Core.CQS.PostgreSql.Options;
using eQuantic.Core.CQS.PostgreSql.Outbox;
using eQuantic.Core.CQS.PostgreSql.Sagas;
using eQuantic.Core.CQS.PostgreSql.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.PostgreSql.Extensions;

/// <summary>Extension methods for registering PostgreSQL providers</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds all PostgreSQL CQS implementations</summary>
    public static IServiceCollection AddCQSPostgreSql<TSagaData>(this IServiceCollection services, Action<PostgreSqlOptions>? configure = null)
        where TSagaData : ISagaData
    {
        var options = new PostgreSqlOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddScoped<ISagaRepository<TSagaData>, PostgreSqlSagaRepository<TSagaData>>();
        services.AddScoped<IOutboxRepository, PostgreSqlOutboxRepository>();
        services.AddScoped<IJobScheduler, PostgreSqlJobScheduler>();

        return services;
    }
}