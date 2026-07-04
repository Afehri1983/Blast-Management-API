using BlastManagement.Domain.Enums;

namespace BlastManagement.Application.ReadModels;

public sealed record BlastReadModel(
    Guid Id,
    string Name,
    BlastStatus Status,
    DateTimeOffset? DateBlasted,
    IReadOnlyList<HoleReadModel> Holes);
