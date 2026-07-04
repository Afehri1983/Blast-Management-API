using System.Collections.Concurrent;
using BlastManagement.Application.Abstractions;
using BlastManagement.Application.Exceptions;
using BlastManagement.Domain.Events;

namespace BlastManagement.Infrastructure.EventStore;

public sealed class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentDictionary<Guid, List<EventEnvelope>> _streams = new();

    public Task<IReadOnlyList<EventEnvelope>> LoadAsync(Guid streamId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_streams.TryGetValue(streamId, out var stream))
        {
            return Task.FromResult<IReadOnlyList<EventEnvelope>>([]);
        }

        lock (stream)
        {
            return Task.FromResult<IReadOnlyList<EventEnvelope>>(stream.ToList());
        }
    }

    public Task AppendAsync(
        Guid streamId,
        int expectedVersion,
        IReadOnlyList<IDomainEvent> events,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (events.Count == 0)
        {
            return Task.CompletedTask;
        }

        var stream = _streams.GetOrAdd(streamId, _ => []);

        lock (stream)
        {
            if (stream.Count != expectedVersion)
            {
                throw new ConcurrencyException(
                    $"Stream '{streamId}' version mismatch. Expected {expectedVersion}, actual {stream.Count}.");
            }

            foreach (var domainEvent in events)
            {
                var version = stream.Count + 1;
                stream.Add(new EventEnvelope(streamId, version, DateTimeOffset.UtcNow, domainEvent));
            }
        }

        return Task.CompletedTask;
    }
}
