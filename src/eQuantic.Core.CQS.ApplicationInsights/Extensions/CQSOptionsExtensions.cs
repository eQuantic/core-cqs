using eQuantic.Core.CQS.Abstractions.Options;
using eQuantic.Core.CQS.Abstractions.Telemetry;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.ApplicationInsights.Extensions;

/// <summary>
/// Extension methods for configuring Application Insights telemetry.
/// </summary>
public static class CQSOptionsExtensions
{
    /// <summary>
    /// Configures Application Insights as the telemetry provider.
    /// Requires TelemetryClient to be registered in DI.
    /// </summary>
    public static CQSOptions UseApplicationInsights(this CQSOptions options)
    {
        options.Services.AddSingleton<ICqsTelemetry>(sp =>
        {
            var telemetryClient = sp.GetRequiredService<TelemetryClient>();
            return new ApplicationInsightsTelemetryAdapter(telemetryClient);
        });
        
        return options;
    }

    /// <summary>
    /// Configures Application Insights with a connection string.
    /// </summary>
    public static CQSOptions UseApplicationInsights(this CQSOptions options, string connectionString)
    {
        options.Services.AddSingleton<ICqsTelemetry>(
            new ApplicationInsightsTelemetryAdapter(connectionString));
        
        return options;
    }
}
