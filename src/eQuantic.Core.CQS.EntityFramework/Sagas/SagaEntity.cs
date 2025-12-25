namespace eQuantic.Core.CQS.EntityFramework.Sagas;

/// <summary>
/// EF Core entity for Saga data
/// </summary>
public class SagaEntity
{
    public Guid Id { get; set; }
    public string SagaType { get; set; } = "";
    public int State { get; set; }
    public int CurrentStep { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Data { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}