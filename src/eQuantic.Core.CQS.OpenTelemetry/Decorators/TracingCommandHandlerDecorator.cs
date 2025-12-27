using eQuantic.Core.CQS.Abstractions.Commands;
using eQuantic.Core.CQS.Abstractions.Handlers;
using eQuantic.Core.CQS.Abstractions.Telemetry;
using eQuantic.Core.CQS.OpenTelemetry.Diagnostics;
using eQuantic.Core.CQS.OpenTelemetry.Options;

namespace eQuantic.Core.CQS.OpenTelemetry.Decorators;

/// <summary>
/// Decorator that adds tracing to command handlers without result.
/// Implements OCP - extends functionality without modifying original handler.
/// Implements DIP - depends on ICqsTelemetry abstraction.
/// </summary>
public sealed class TracingCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _inner;
    private readonly ICqsTelemetry _telemetry;
    private readonly OpenTelemetryOptions _options;
    
    public TracingCommandHandlerDecorator(
        ICommandHandler<TCommand> inner,
        ICqsTelemetry telemetry,
        OpenTelemetryOptions options)
    {
        _inner = inner;
        _telemetry = telemetry;
        _options = options;
    }
    
    public async Task Execute(TCommand command, CancellationToken cancellationToken)
    {
        if (!_options.TraceCommands)
        {
            await _inner.Execute(command, cancellationToken);
            return;
        }
        
        var commandType = typeof(TCommand).Name;
        
        using var _ = _telemetry.StartActivity(
            CqsActivitySource.Operations.Command,
            commandType,
            new Dictionary<string, object?>
            {
                [CqsActivitySource.Tags.CommandType] = commandType
            });
        
        try
        {
            await _inner.Execute(command, cancellationToken);
        }
        catch (Exception ex)
        {
            _telemetry.RecordException(ex);
            throw;
        }
    }
}

/// <summary>
/// Decorator that adds tracing to command handlers with result.
/// Implements OCP - extends functionality without modifying original handler.
/// Implements DIP - depends on ICqsTelemetry abstraction.
/// </summary>
public sealed class TracingCommandHandlerDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _inner;
    private readonly ICqsTelemetry _telemetry;
    private readonly OpenTelemetryOptions _options;
    
    public TracingCommandHandlerDecorator(
        ICommandHandler<TCommand, TResult> inner,
        ICqsTelemetry telemetry,
        OpenTelemetryOptions options)
    {
        _inner = inner;
        _telemetry = telemetry;
        _options = options;
    }
    
    public async Task<TResult> Execute(TCommand command, CancellationToken cancellationToken)
    {
        if (!_options.TraceCommands)
        {
            return await _inner.Execute(command, cancellationToken);
        }
        
        var commandType = typeof(TCommand).Name;
        
        using var _ = _telemetry.StartActivity(
            CqsActivitySource.Operations.Command,
            commandType,
            new Dictionary<string, object?>
            {
                [CqsActivitySource.Tags.CommandType] = commandType
            });
        
        try
        {
            return await _inner.Execute(command, cancellationToken);
        }
        catch (Exception ex)
        {
            _telemetry.RecordException(ex);
            throw;
        }
    }
}
