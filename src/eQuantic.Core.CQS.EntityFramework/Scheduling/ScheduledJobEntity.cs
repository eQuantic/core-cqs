namespace eQuantic.Core.CQS.EntityFramework.Scheduling;

/// <summary>
/// EF Core entity for scheduled jobs
/// </summary>
public class ScheduledJobEntity
{
    public Guid Id { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string RequestType { get; set; } = "";
    public string RequestJson { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}