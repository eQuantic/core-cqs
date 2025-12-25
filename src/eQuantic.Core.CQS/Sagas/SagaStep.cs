using eQuantic.Core.CQS.Abstractions.Sagas;

namespace eQuantic.Core.CQS.Sagas;

/// <summary>
/// Represents a step in a saga with execute and compensate actions
/// </summary>
/// <typeparam name="TData">The saga data type</typeparam>
public class SagaStep<TData> where TData : ISagaData
{
    /// <summary>
    /// Name of this step for logging/debugging
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The action to execute for this step
    /// </summary>
    public required Func<TData, CancellationToken, Task> Execute { get; init; }

    /// <summary>
    /// The compensation action to run if a later step fails
    /// </summary>
    public Func<TData, CancellationToken, Task>? Compensate { get; init; }
}