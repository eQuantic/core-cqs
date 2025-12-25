namespace eQuantic.Core.CQS.Scheduling;

/// <summary>
/// Attribute to mark a handler as scheduled
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ScheduledAttribute : Attribute
{
    /// <summary>
    /// Cron expression for scheduling (e.g., "0 * * * *" for hourly)
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// Fixed delay between executions in milliseconds
    /// </summary>
    public int DelayMs { get; set; }

    /// <summary>
    /// Fixed interval between executions in milliseconds
    /// </summary>
    public int IntervalMs { get; set; }

    /// <summary>
    /// Whether to run immediately on startup
    /// </summary>
    public bool RunOnStartup { get; set; }
}