using System.Text.Json;
using eQuantic.Core.CQS.Abstractions.Outbox;

namespace eQuantic.Core.CQS.Outbox;

/// <summary>
/// Default outbox implementation
/// </summary>
public class Outbox : IOutbox
{
    private readonly IOutboxRepository _repository;

    public Outbox(IOutboxRepository repository)
    {
        _repository = repository;
    }

    public async Task Enqueue<T>(T message, string? correlationId = null, CancellationToken cancellationToken = default) where T : notnull
    {
        var outboxMessage = new OutboxMessage
        {
            MessageType = typeof(T).AssemblyQualifiedName ?? typeof(T).FullName ?? typeof(T).Name,
            Payload = JsonSerializer.Serialize(message),
            CorrelationId = correlationId
        };

        await _repository.Add(outboxMessage, cancellationToken).ConfigureAwait(false);
    }
}