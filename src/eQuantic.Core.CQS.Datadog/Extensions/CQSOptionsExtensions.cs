using eQuantic.Core.CQS.Abstractions.Options;
using eQuantic.Core.CQS.Abstractions.Telemetry;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.Datadog.Extensions;

/// <summary>
/// Extension methods for configuring Datadog telemetry.
/// </summary>
public static class CQSOptionsExtensions
{
    /// <summary>
    /// Configures Datadog as the telemetry provider.
    /// </summary>
    public static CQSOptions UseDatadog(this CQSOptions options, string? serviceName = null)
    {
        options.Services.AddSingleton<ICqsTelemetry>(new DatadogTelemetryAdapter(serviceName));
        return options;
    }
}
