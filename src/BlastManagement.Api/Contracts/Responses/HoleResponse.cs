using BlastManagement.Domain.Enums;

namespace BlastManagement.Api.Contracts.Responses;

public sealed record HoleResponse(
    Guid Id,
    Guid BlastId,
    string Name,
    double PositionX,
    double PositionY,
    double PositionZ,
    double Direction,
    double Inclination,
    HoleStatus Status);
