namespace eQuantic.Core.CQS.Sagas;

/// <summary>
/// Result of a saga execution
/// </summary>
public record SagaResult
{
    public Guid SagaId { get; init; }
    public bool IsSuccess { get; init; }
    public bool WasCompensated { get; init; }
    public Exception? Error { get; init; }
    public IReadOnlyList<Exception> CompensationErrors { get; init; } = Array.Empty<Exception>();

    public static SagaResult Success(Guid sagaId) => new()
    {
        SagaId = sagaId,
        IsSuccess = true
    };

    public static SagaResult Compensated(Guid sagaId, Exception error) => new()
    {
        SagaId = sagaId,
        IsSuccess = false,
        WasCompensated = true,
        Error = error
    };

    public static SagaResult Failure(Guid sagaId, Exception error, IList<Exception> compensationErrors) => new()
    {
        SagaId = sagaId,
        IsSuccess = false,
        WasCompensated = false,
        Error = error,
        CompensationErrors = compensationErrors.ToArray()
    };
}