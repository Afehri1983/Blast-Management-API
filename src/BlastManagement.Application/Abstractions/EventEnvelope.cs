using BlastManagement.Domain.Events;

namespace BlastManagement.Application.Abstractions;

public sealed record EventEnvelope(
    Guid AggregateId,
    int Version,
    DateTimeOffset Timestamp,
    IDomainEvent Event);
