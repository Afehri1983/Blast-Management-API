namespace BlastManagement.Application.Commands;

public sealed record MarkHoleReadyCommand(Guid BlastId, Guid HoleId);
