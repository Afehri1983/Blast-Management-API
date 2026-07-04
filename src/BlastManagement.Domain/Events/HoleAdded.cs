using BlastManagement.Domain.ValueObjects;

namespace BlastManagement.Domain.Events;

public sealed record HoleAdded(
    Guid AggregateId,
    Guid HoleId,
    string Name,
    Position Position,
    double Direction,
    double Inclination) : IDomainEvent;
