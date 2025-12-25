namespace eQuantic.Core.CQS.MongoDb.Scheduling;

/// <summary>MongoDB Job document</summary>
internal class MongoDbJob
{
    public Guid Id { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string RequestType { get; set; } = "";
    public string RequestJson { get; set; } = "";
}