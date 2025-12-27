using eQuantic.Core.CQS.Abstractions.Resilience;
using eQuantic.Core.CQS.Abstractions.Sagas;
using eQuantic.Core.CQS.Resilience.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eQuantic.Core.CQS.Resilience.Monitoring;

/// <summary>
/// Background service that monitors sagas for timeouts.
/// Implements SRP - only handles timeout monitoring.
/// </summary>
public class SagaTimeoutBackgroundService<TSagaData> : BackgroundService
    where TSagaData : ISagaData
{
    private readonly ISagaRepository<TSagaData> _repository;
    private readonly ISagaTimeoutPolicy _timeoutPolicy;
    private readonly ResilienceOptions _options;
    private readonly ILogger<SagaTimeoutBackgroundService<TSagaData>> _logger;

    public SagaTimeoutBackgroundService(
        ISagaRepository<TSagaData> repository,
        ISagaTimeoutPolicy timeoutPolicy,
        ResilienceOptions options,
        ILogger<SagaTimeoutBackgroundService<TSagaData>> logger)
    {
        _repository = repository;
        _timeoutPolicy = timeoutPolicy;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Saga timeout monitor started for {SagaType}", typeof(TSagaData).Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckTimeoutsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking saga timeouts");
            }

            await Task.Delay(_options.TimeoutCheckInterval, stoppingToken);
        }

        _logger.LogInformation("Saga timeout monitor stopped for {SagaType}", typeof(TSagaData).Name);
    }

    private async Task CheckTimeoutsAsync(CancellationToken ct)
    {
        var incompleteSagas = await _repository.GetIncomplete(ct);
        var now = DateTime.UtcNow;

        foreach (var saga in incompleteSagas)
        {
            var timeout = _timeoutPolicy.GetTimeout(saga);
            var elapsed = now - saga.StartedAt;

            if (elapsed > timeout)
            {
                _logger.LogWarning(
                    "Saga {SagaId} has timed out. Elapsed: {Elapsed}, Timeout: {Timeout}",
                    saga.SagaId, elapsed, timeout);

                try
                {
                    await _timeoutPolicy.OnTimeoutAsync(saga, ct);
                    await _repository.Save(saga, ct);
                    
                    _logger.LogInformation("Saga {SagaId} marked as timed out", saga.SagaId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to handle timeout for saga {SagaId}", saga.SagaId);
                }
            }
        }
    }
}
