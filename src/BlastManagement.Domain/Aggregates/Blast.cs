using BlastManagement.Domain.Enums;
using BlastManagement.Domain.Events;
using BlastManagement.Domain.Exceptions;
using BlastManagement.Domain.ValueObjects;

namespace BlastManagement.Domain.Aggregates;

public sealed class Blast : AggregateRoot
{
    private const double MaxAzimuthDegrees = 360.0;
    private const double MaxInclinationDegrees = 90.0;

    private readonly Dictionary<Guid, Hole> _holes = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public BlastStatus Status { get; private set; }
    public DateTimeOffset? DateBlasted { get; private set; }
    public IReadOnlyCollection<Hole> Holes => _holes.Values.ToList();

    private Blast()
    {
    }

    public static Blast Create(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Blast name is required.");
        }

        var blast = new Blast();
        blast.Raise(new BlastCreated(id, name));
        return blast;
    }

    public static Blast LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        var blast = new Blast();

        foreach (var domainEvent in history)
        {
            blast.Apply(domainEvent);
            blast.ApplyVersion(blast.Version + 1);
        }

        return blast;
    }

    public void AddHole(
        Guid holeId,
        string name,
        Position position,
        double direction,
        double inclination)
    {
        if (Status == BlastStatus.Blasted)
        {
            throw new DomainException("Cannot add holes to a blasted blast.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Hole name is required.");
        }

        if (_holes.ContainsKey(holeId))
        {
            throw new DomainException($"Hole '{holeId}' already exists on this blast.");
        }

        if (direction is < 0 or > MaxAzimuthDegrees)
        {
            throw new DomainException($"Direction must be between 0 and {MaxAzimuthDegrees} degrees.");
        }

        if (inclination is < 0 or > MaxInclinationDegrees)
        {
            throw new DomainException(
                $"Inclination must be between 0 and {MaxInclinationDegrees} degrees (0 = vertical downward).");
        }

        Raise(new HoleAdded(Id, holeId, name, position, direction, inclination));
    }

    public void ChargeHole(Guid holeId)
    {
        if (Status == BlastStatus.Blasted)
        {
            throw new DomainException("Cannot charge holes on a blasted blast.");
        }

        var hole = GetHole(holeId);

        if (hole.Status is HoleStatus.Charged or HoleStatus.Ready)
        {
            throw new DomainException($"Hole '{holeId}' is already charged or ready.");
        }

        Raise(new HoleCharged(Id, holeId));
    }

    public void MarkHoleReady(Guid holeId)
    {
        if (Status == BlastStatus.Blasted)
        {
            throw new DomainException("Cannot mark holes ready on a blasted blast.");
        }

        var hole = GetHole(holeId);

        if (hole.Status != HoleStatus.Charged)
        {
            throw new DomainException($"Hole '{holeId}' must be charged before it can be marked ready.");
        }

        Raise(new HoleMarkedReady(Id, holeId));
    }

    public void Fire(DateTimeOffset firedAt)
    {
        if (Status == BlastStatus.Blasted)
        {
            throw new DomainException("Blast has already been fired.");
        }

        if (_holes.Count == 0)
        {
            throw new DomainException("Cannot fire a blast with no holes.");
        }

        if (_holes.Values.Any(hole => hole.Status != HoleStatus.Ready))
        {
            throw new DomainException("All holes must be ready before firing the blast.");
        }

        Raise(new BlastFired(Id, firedAt));
    }

    protected override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case BlastCreated created:
                Id = created.AggregateId;
                Name = created.Name;
                Status = BlastStatus.Planned;
                break;

            case HoleAdded added:
                _holes[added.HoleId] = Hole.Create(
                    added.HoleId,
                    added.AggregateId,
                    added.Name,
                    added.Position,
                    added.Direction,
                    added.Inclination);

                // A blast moves to Loaded once drilling/charging preparation begins (first hole added).
                if (Status == BlastStatus.Planned)
                {
                    Status = BlastStatus.Loaded;
                }

                break;

            case HoleCharged charged:
                GetHole(charged.HoleId).Charge();
                break;

            case HoleMarkedReady markedReady:
                GetHole(markedReady.HoleId).MarkReady();
                break;

            case BlastFired fired:
                Status = BlastStatus.Blasted;
                DateBlasted = fired.FiredAt;
                break;

            default:
                throw new DomainException($"Unknown domain event type '{domainEvent.GetType().Name}'.");
        }
    }

    private Hole GetHole(Guid holeId)
    {
        if (!_holes.TryGetValue(holeId, out var hole))
        {
            throw new DomainException($"Hole '{holeId}' was not found on this blast.");
        }

        return hole;
    }
}
