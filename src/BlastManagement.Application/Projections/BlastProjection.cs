using BlastManagement.Application.Abstractions;
using BlastManagement.Application.ReadModels;
using BlastManagement.Domain.Enums;
using BlastManagement.Domain.Events;

namespace BlastManagement.Application.Projections;

// In-memory projection: fast reads from a materialized view.
// In production, read models are often eventually consistent (fed asynchronously from the event stream).
internal sealed class BlastProjection : IBlastProjection
{
    private readonly Dictionary<Guid, BlastState> _blasts = new();

    public BlastReadModel? Get(Guid blastId) =>
        _blasts.TryGetValue(blastId, out var state) ? state.ToReadModel() : null;

    public void Apply(IReadOnlyList<EventEnvelope> envelopes)
    {
        foreach (var envelope in envelopes)
        {
            ApplyEvent(envelope.Event);
        }
    }

    private void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case BlastCreated created:
                _blasts[created.AggregateId] = new BlastState(
                    created.AggregateId,
                    created.Name,
                    BlastStatus.Planned,
                    dateBlasted: null);
                break;

            case HoleAdded added:
                var blast = GetBlastState(added.AggregateId);
                blast.Holes[added.HoleId] = new HoleReadModel(
                    added.HoleId,
                    added.AggregateId,
                    added.Name,
                    added.Position.X,
                    added.Position.Y,
                    added.Position.Z,
                    added.Direction,
                    added.Inclination,
                    HoleStatus.Planned);

                if (blast.Status == BlastStatus.Planned)
                {
                    blast.Status = BlastStatus.Loaded;
                }

                break;

            case HoleCharged charged:
                UpdateHoleStatus(charged.AggregateId, charged.HoleId, HoleStatus.Charged);
                break;

            case HoleMarkedReady markedReady:
                UpdateHoleStatus(markedReady.AggregateId, markedReady.HoleId, HoleStatus.Ready);
                break;

            case BlastFired fired:
                var firedBlast = GetBlastState(fired.AggregateId);
                firedBlast.Status = BlastStatus.Blasted;
                firedBlast.DateBlasted = fired.FiredAt;
                break;

            default:
                throw new InvalidOperationException(
                    $"Unknown domain event type '{domainEvent.GetType().Name}'.");
        }
    }

    private BlastState GetBlastState(Guid blastId)
    {
        if (!_blasts.TryGetValue(blastId, out var blast))
        {
            throw new InvalidOperationException($"Blast '{blastId}' was not found in the projection.");
        }

        return blast;
    }

    private void UpdateHoleStatus(Guid blastId, Guid holeId, HoleStatus status)
    {
        var blast = GetBlastState(blastId);

        if (!blast.Holes.TryGetValue(holeId, out var hole))
        {
            throw new InvalidOperationException($"Hole '{holeId}' was not found in the projection.");
        }

        blast.Holes[holeId] = hole with { Status = status };
    }

    private sealed class BlastState(
        Guid id,
        string name,
        BlastStatus status,
        DateTimeOffset? dateBlasted)
    {
        public Guid Id { get; } = id;
        public string Name { get; } = name;
        public BlastStatus Status { get; set; } = status;
        public DateTimeOffset? DateBlasted { get; set; } = dateBlasted;
        public Dictionary<Guid, HoleReadModel> Holes { get; } = new();

        public BlastReadModel ToReadModel() => new(
            Id,
            Name,
            Status,
            DateBlasted,
            Holes.Values.ToList());
    }
}
