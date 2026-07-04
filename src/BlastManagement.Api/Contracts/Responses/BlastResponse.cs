using BlastManagement.Domain.Enums;

namespace BlastManagement.Api.Contracts.Responses;

public sealed record BlastResponse(
    Guid Id,
    string Name,
    BlastStatus Status,
    DateTimeOffset? DateBlasted,
    IReadOnlyList<HoleResponse> Holes);
