namespace BlastManagement.Api.Contracts.Requests;

public sealed record AddHoleRequest(
    Guid HoleId,
    string Name,
    double PositionX,
    double PositionY,
    double PositionZ,
    double Direction,
    double Inclination);
