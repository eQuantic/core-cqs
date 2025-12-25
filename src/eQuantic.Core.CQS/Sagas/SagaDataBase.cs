using eQuantic.Core.CQS.Abstractions.Sagas;

namespace eQuantic.Core.CQS.Sagas;

/// <summary>
/// Base class for saga data with default implementations
/// </summary>
public abstract class SagaDataBase : ISagaData
{
    public Guid SagaId { get; set; } = Guid.NewGuid();
    public SagaState State { get; set; } = SagaState.NotStarted;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int CurrentStep { get; set; }
}
