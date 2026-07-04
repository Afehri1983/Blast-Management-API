namespace BlastManagement.Domain.Events;

public sealed record HoleMarkedReady(Guid AggregateId, Guid HoleId) : IDomainEvent;
