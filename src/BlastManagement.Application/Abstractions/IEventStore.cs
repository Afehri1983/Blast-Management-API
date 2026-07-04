using BlastManagement.Domain.Events;

namespace BlastManagement.Application.Abstractions;

public interface IEventStore
{
    Task<IReadOnlyList<EventEnvelope>> LoadAsync(Guid streamId, CancellationToken cancellationToken);

    Task AppendAsync(
        Guid streamId,
        int expectedVersion,
        IReadOnlyList<IDomainEvent> events,
        CancellationToken cancellationToken);
}
