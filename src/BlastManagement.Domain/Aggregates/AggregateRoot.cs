using BlastManagement.Domain.Events;

namespace BlastManagement.Domain.Aggregates;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _uncommittedEvents = [];

    public int Version { get; private set; }

    public IReadOnlyList<IDomainEvent> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

    protected void Raise(IDomainEvent domainEvent)
    {
        Apply(domainEvent);
        _uncommittedEvents.Add(domainEvent);
    }

    protected abstract void Apply(IDomainEvent domainEvent);

    protected void ApplyVersion(int version) => Version = version;
}
