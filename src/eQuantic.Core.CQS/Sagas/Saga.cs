using eQuantic.Core.CQS.Abstractions.Sagas;

namespace eQuantic.Core.CQS.Sagas;

/// <summary>
/// Base class for saga orchestrators
/// </summary>
/// <typeparam name="TData">The saga data type</typeparam>
public abstract class Saga<TData> where TData : ISagaData
{
    private readonly List<SagaStep<TData>> _steps = new();

    /// <summary>
    /// Gets the saga data
    /// </summary>
    public TData Data { get; private set; } = default!;

    /// <summary>
    /// Configures the saga steps
    /// </summary>
    protected abstract void ConfigureSteps();

    /// <summary>
    /// Adds a step to the saga
    /// </summary>
    protected void Step(string name, Func<TData, CancellationToken, Task> execute, Func<TData, CancellationToken, Task>? compensate = null)
    {
        _steps.Add(new SagaStep<TData>
        {
            Name = name,
            Execute = execute,
            Compensate = compensate
        });
    }

    /// <summary>
    /// Executes the saga
    /// </summary>
    public async Task<SagaResult> Execute(TData data, CancellationToken cancellationToken = default)
    {
        Data = data;
        _steps.Clear();
        ConfigureSteps();

        data.State = SagaState.InProgress;
        data.StartedAt = DateTime.UtcNow;
        data.CurrentStep = 0;

        var executedSteps = new Stack<SagaStep<TData>>();

        try
        {
            foreach (var step in _steps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                await step.Execute(data, cancellationToken).ConfigureAwait(false);
                executedSteps.Push(step);
                data.CurrentStep++;
            }

            data.State = SagaState.Completed;
            data.CompletedAt = DateTime.UtcNow;

            return SagaResult.Success(data.SagaId);
        }
        catch (Exception ex)
        {
            // Compensate in reverse order
            var compensationErrors = new List<Exception>();

            while (executedSteps.Count > 0)
            {
                var step = executedSteps.Pop();
                
                if (step.Compensate is not null)
                {
                    try
                    {
                        await step.Compensate(data, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception compEx)
                    {
                        compensationErrors.Add(compEx);
                    }
                }
            }

            if (compensationErrors.Count > 0)
            {
                data.State = SagaState.Failed;
                data.CompletedAt = DateTime.UtcNow;
                return SagaResult.Failure(data.SagaId, ex, compensationErrors);
            }

            data.State = SagaState.Compensated;
            data.CompletedAt = DateTime.UtcNow;
            return SagaResult.Compensated(data.SagaId, ex);
        }
    }
}