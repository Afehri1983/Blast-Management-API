using BlastManagement.Domain.Enums;

namespace BlastManagement.Application.ReadModels;

public sealed record HoleReadModel(
    Guid Id,
    Guid BlastId,
    string Name,
    double PositionX,
    double PositionY,
    double PositionZ,
    double Direction,
    double Inclination,
    HoleStatus Status);
