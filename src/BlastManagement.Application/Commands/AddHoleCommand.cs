using BlastManagement.Domain.ValueObjects;

namespace BlastManagement.Application.Commands;

public sealed record AddHoleCommand(
    Guid BlastId,
    Guid HoleId,
    string Name,
    Position Position,
    double Direction,
    double Inclination);
