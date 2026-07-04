using BlastManagement.Domain.Enums;
using BlastManagement.Domain.ValueObjects;

namespace BlastManagement.Domain.Aggregates;

public sealed class Hole
{
    public Guid Id { get; private set; }
    public Guid BlastId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Position Position { get; private set; } = new(0, 0, 0);
    public double Direction { get; private set; }
    public double Inclination { get; private set; }
    public HoleStatus Status { get; private set; }

    private Hole()
    {
    }

    internal static Hole Create(
        Guid id,
        Guid blastId,
        string name,
        Position position,
        double direction,
        double inclination)
    {
        return new Hole
        {
            Id = id,
            BlastId = blastId,
            Name = name,
            Position = position,
            Direction = direction,
            Inclination = inclination,
            Status = HoleStatus.Planned
        };
    }

    internal void Charge() => Status = HoleStatus.Charged;

    internal void MarkReady() => Status = HoleStatus.Ready;
}
