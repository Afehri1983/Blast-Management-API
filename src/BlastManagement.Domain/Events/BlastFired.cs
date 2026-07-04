namespace BlastManagement.Domain.Events;

public sealed record BlastFired(Guid AggregateId, DateTimeOffset FiredAt) : IDomainEvent;
