using System.Diagnostics;
using System.Diagnostics.Metrics;
using eQuantic.Core.CQS.Abstractions.Telemetry;
using eQuantic.Core.CQS.OpenTelemetry.Options;

namespace eQuantic.Core.CQS.OpenTelemetry.Diagnostics;

/// <summary>
/// OpenTelemetry implementation of ICqsTelemetry.
/// Implements DIP - depends on ICqsTelemetry abstraction.
/// </summary>
public sealed class OpenTelemetryAdapter : ICqsTelemetry
{
    private readonly OpenTelemetryOptions _options;
    private readonly Meter _meter;
    private static readonly AsyncLocal<Activity?> CurrentActivity = new();
    
    public OpenTelemetryAdapter(OpenTelemetryOptions options)
    {
        _options = options;
        _meter = new Meter(CqsActivitySource.SourceName, options.ServiceVersion);
        CqsActivitySource.Initialize(options.ServiceVersion);
    }
    
    /// <inheritdoc />
    public IDisposable StartActivity(string operationType, string operationName, IDictionary<string, object?>? tags = null)
    {
        var activity = CqsActivitySource.Source.StartActivity(
            $"{operationType}.{operationName}",
            ActivityKind.Internal);
        
        if (activity == null)
            return NullCqsTelemetry.Instance.StartActivity(operationType, operationName, tags);
        
        activity.SetTag(CqsActivitySource.Tags.OperationType, operationType);
        activity.SetTag(CqsActivitySource.Tags.OperationName, operationName);
        
        // Add global tags
        foreach (var tag in _options.GlobalTags)
        {
            activity.SetTag(tag.Key, tag.Value);
        }
        
        // Add custom tags
        if (tags != null)
        {
            foreach (var tag in tags)
            {
                activity.SetTag(tag.Key, tag.Value);
            }
        }
        
        CurrentActivity.Value = activity;
        return new ActivityScope(activity, this);
    }
    
    /// <inheritdoc />
    public void RecordException(Exception exception)
    {
        var activity = CurrentActivity.Value ?? Activity.Current;
        if (activity == null || !_options.RecordExceptions) return;
        
        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag(CqsActivitySource.Tags.Success, false);
        activity.SetTag(CqsActivitySource.Tags.ErrorType, exception.GetType().Name);
        
        // Record exception event
        activity.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
        {
            { "exception.type", exception.GetType().FullName },
            { "exception.message", exception.Message },
            { "exception.stacktrace", exception.StackTrace }
        }));
    }
    
    /// <inheritdoc />
    public void SetTag(string key, object? value)
    {
        var activity = CurrentActivity.Value ?? Activity.Current;
        activity?.SetTag(key, value);
    }
    
    /// <inheritdoc />
    public void RecordMetric(string name, double value, IDictionary<string, object?>? tags = null)
    {
        if (!_options.CollectMetrics) return;
        
        var counter = _meter.CreateCounter<double>(name);
        
        if (tags != null)
        {
            var tagList = new TagList();
            foreach (var tag in tags.Where(t => t.Value != null))
            {
                tagList.Add(tag.Key, tag.Value);
            }
            counter.Add(value, tagList);
        }
        else
        {
            counter.Add(value);
        }
    }
    
    private void MarkSuccess()
    {
        var activity = CurrentActivity.Value ?? Activity.Current;
        if (activity == null) return;
        
        if (activity.Status == ActivityStatusCode.Unset)
        {
            activity.SetStatus(ActivityStatusCode.Ok);
            activity.SetTag(CqsActivitySource.Tags.Success, true);
        }
    }
    
    /// <summary>
    /// Scope that disposes the activity when done.
    /// </summary>
    private sealed class ActivityScope : IDisposable
    {
        private readonly Activity _activity;
        private readonly OpenTelemetryAdapter _adapter;
        
        public ActivityScope(Activity activity, OpenTelemetryAdapter adapter)
        {
            _activity = activity;
            _adapter = adapter;
        }
        
        public void Dispose()
        {
            _adapter.MarkSuccess();
            _activity.Dispose();
            CurrentActivity.Value = null;
        }
    }
}
