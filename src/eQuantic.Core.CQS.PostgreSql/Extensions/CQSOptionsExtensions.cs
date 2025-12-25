using eQuantic.Core.CQS.Abstractions.Options;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Abstractions.Scheduling;
using eQuantic.Core.CQS.PostgreSql.Options;
using eQuantic.Core.CQS.PostgreSql.Outbox;
using eQuantic.Core.CQS.PostgreSql.Sagas;
using eQuantic.Core.CQS.PostgreSql.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.PostgreSql.Extensions;

/// <summary>
/// CQSOptions extension methods for PostgreSQL providers
/// </summary>
public static class CQSOptionsExtensions
{
    /// <summary>
    /// Configures PostgreSQL as the persistence provider for Sagas, Outbox, and Job Scheduler
    /// </summary>
    public static CQSOptions UsePostgreSql<TSagaData>(
        this CQSOptions options, 
        Action<PostgreSqlOptions>? configure = null)
        where TSagaData : ISagaData
    {
        var pgOptions = new PostgreSqlOptions();
        configure?.Invoke(pgOptions);

        options.Services.AddSingleton(pgOptions);
        options.Services.AddScoped<ISagaRepository<TSagaData>, PostgreSqlSagaRepository<TSagaData>>();
        options.Services.AddScoped<IOutboxRepository, PostgreSqlOutboxRepository>();
        options.Services.AddScoped<IJobScheduler, PostgreSqlJobScheduler>();

        return options;
    }

    /// <summary>
    /// Configures PostgreSQL as the persistence provider (without typed saga)
    /// </summary>
    public static CQSOptions UsePostgreSql(
        this CQSOptions options, 
        Action<PostgreSqlOptions>? configure = null)
    {
        var pgOptions = new PostgreSqlOptions();
        configure?.Invoke(pgOptions);

        options.Services.AddSingleton(pgOptions);
        options.Services.AddScoped<IOutboxRepository, PostgreSqlOutboxRepository>();
        options.Services.AddScoped<IJobScheduler, PostgreSqlJobScheduler>();

        return options;
    }
}
