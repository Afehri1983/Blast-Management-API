namespace BlastManagement.Domain.Events;

public sealed record HoleCharged(Guid AggregateId, Guid HoleId) : IDomainEvent;
