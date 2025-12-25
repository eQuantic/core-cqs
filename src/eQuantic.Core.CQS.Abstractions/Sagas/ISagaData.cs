namespace eQuantic.Core.CQS.Abstractions.Sagas;

/// <summary>
/// Base interface for saga data
/// </summary>
public interface ISagaData
{
    /// <summary>
    /// Unique identifier for this saga instance
    /// </summary>
    Guid SagaId { get; set; }

    /// <summary>
    /// Current state of the saga
    /// </summary>
    SagaState State { get; set; }

    /// <summary>
    /// When the saga was started
    /// </summary>
    DateTime StartedAt { get; set; }

    /// <summary>
    /// When the saga was completed or failed
    /// </summary>
    DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Current step in the saga
    /// </summary>
    int CurrentStep { get; set; }
}