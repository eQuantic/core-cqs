namespace eQuantic.Core.CQS.Abstractions.Sagas;

/// <summary>
/// Represents the state of a saga
/// </summary>
public enum SagaState
{
    /// <summary>
    /// Saga is not yet started
    /// </summary>
    NotStarted,

    /// <summary>
    /// Saga is in progress
    /// </summary>
    InProgress,

    /// <summary>
    /// Saga completed successfully
    /// </summary>
    Completed,

    /// <summary>
    /// Saga failed and compensation was executed
    /// </summary>
    Compensated,

    /// <summary>
    /// Saga failed and compensation also failed
    /// </summary>
    Failed
}