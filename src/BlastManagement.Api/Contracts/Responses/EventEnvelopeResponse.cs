namespace BlastManagement.Api.Contracts.Responses;

public sealed record EventEnvelopeResponse(
    Guid AggregateId,
    int Version,
    DateTimeOffset Timestamp,
    string EventType,
    EventPayloadResponse Payload);
