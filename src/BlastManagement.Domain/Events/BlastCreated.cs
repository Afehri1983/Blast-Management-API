namespace BlastManagement.Domain.Events;

public sealed record BlastCreated(Guid AggregateId, string Name) : IDomainEvent;
