using eQuantic.Core.CQS.Abstractions.Options;
using eQuantic.Core.CQS.Abstractions.Outbox;
using eQuantic.Core.CQS.Data.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Data.Extensions;

/// <summary>
/// CQS registration for the native eQuantic.Core.Data persistence engine.
/// </summary>
public static class CQSOptionsExtensions
{
    /// <summary>
    /// Registers the native (no-ORM) <see cref="IOutboxRepository" />, so outbox messages persist through the
    /// same eQuantic.Core.Data unit of work as your aggregate writes — one transaction, on any supported store.
    /// <para>
    /// The <see cref="OutboxDataEntity" /> must be part of your native model and have a repository registered
    /// with your provider, for example <c>services.AddPostgreSqlRepository&lt;OutboxDataEntity, Guid&gt;()</c>
    /// (and included in <c>AddPostgreSqlDatabase(..., model =&gt; model.Entity&lt;OutboxDataEntity&gt;(...))</c>).
    /// </para>
    /// </summary>
    /// <param name="options">The CQS options.</param>
    /// <returns>The CQS options, for chaining.</returns>
    public static CQSOptions UseCoreDataOutbox(this CQSOptions options)
    {
        options.Services.AddScoped<IOutboxRepository, NativeOutboxRepository>();
        return options;
    }
}
