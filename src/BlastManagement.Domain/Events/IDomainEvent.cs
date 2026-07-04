namespace BlastManagement.Domain.Events;

public interface IDomainEvent
{
    Guid AggregateId { get; }
}
