using BlastManagement.Application.Abstractions;
using BlastManagement.Application.ReadModels;
using BlastManagement.Api.Contracts.Responses;
using BlastManagement.Domain.Events;

namespace BlastManagement.Api.Mapping;

internal static class ApiMappers
{
    public static BlastResponse ToResponse(BlastReadModel blast) =>
        new(
            blast.Id,
            blast.Name,
            blast.Status,
            blast.DateBlasted,
            blast.Holes.Select(ToResponse).ToList());

    public static HoleResponse ToResponse(HoleReadModel hole) =>
        new(
            hole.Id,
            hole.BlastId,
            hole.Name,
            hole.PositionX,
            hole.PositionY,
            hole.PositionZ,
            hole.Direction,
            hole.Inclination,
            hole.Status);

    public static EventEnvelopeResponse ToResponse(EventEnvelope envelope)
    {
        var domainEvent = envelope.Event;

        return new EventEnvelopeResponse(
            envelope.AggregateId,
            envelope.Version,
            envelope.Timestamp,
            domainEvent.GetType().Name,
            MapEventPayload(domainEvent));
    }

    private static EventPayloadResponse MapEventPayload(IDomainEvent domainEvent) =>
        domainEvent switch
        {
            BlastCreated created => new BlastCreatedPayload(created.Name),
            HoleAdded added => new HoleAddedPayload(
                added.HoleId,
                added.Name,
                added.Position.X,
                added.Position.Y,
                added.Position.Z,
                added.Direction,
                added.Inclination),
            HoleCharged charged => new HoleChargedPayload(charged.HoleId),
            HoleMarkedReady markedReady => new HoleMarkedReadyPayload(markedReady.HoleId),
            BlastFired fired => new BlastFiredPayload(fired.FiredAt),
            _ => throw new InvalidOperationException(
                $"Unknown domain event type '{domainEvent.GetType().Name}'.")
        };
}
