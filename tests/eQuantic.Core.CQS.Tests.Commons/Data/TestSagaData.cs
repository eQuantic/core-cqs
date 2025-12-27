using eQuantic.Core.CQS.Abstractions.Sagas;

namespace eQuantic.Core.CQS.Tests.Commons.Data;

/// <summary>
/// Test saga data for integration tests.
/// Implements ISagaData with common test properties.
/// </summary>
public class TestSagaData : ISagaData
{
    public Guid SagaId { get; set; } = Guid.NewGuid();
    public SagaState State { get; set; } = SagaState.NotStarted;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int CurrentStep { get; set; }

    // Common test properties
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}
