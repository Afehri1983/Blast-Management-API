using System.Text.Json.Serialization;

namespace BlastManagement.Api.Contracts.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(BlastCreatedPayload), typeDiscriminator: "BlastCreated")]
[JsonDerivedType(typeof(HoleAddedPayload), typeDiscriminator: "HoleAdded")]
[JsonDerivedType(typeof(HoleChargedPayload), typeDiscriminator: "HoleCharged")]
[JsonDerivedType(typeof(HoleMarkedReadyPayload), typeDiscriminator: "HoleMarkedReady")]
[JsonDerivedType(typeof(BlastFiredPayload), typeDiscriminator: "BlastFired")]
public abstract record EventPayloadResponse;

public sealed record BlastCreatedPayload(string Name) : EventPayloadResponse;

public sealed record HoleAddedPayload(
    Guid HoleId,
    string Name,
    double PositionX,
    double PositionY,
    double PositionZ,
    double Direction,
    double Inclination) : EventPayloadResponse;

public sealed record HoleChargedPayload(Guid HoleId) : EventPayloadResponse;

public sealed record HoleMarkedReadyPayload(Guid HoleId) : EventPayloadResponse;

public sealed record BlastFiredPayload(DateTimeOffset FiredAt) : EventPayloadResponse;
