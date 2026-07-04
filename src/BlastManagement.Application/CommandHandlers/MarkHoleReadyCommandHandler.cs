using BlastManagement.Application.Abstractions;
using BlastManagement.Application.Commands;
using BlastManagement.Application.Exceptions;
using BlastManagement.Application.Projections;
using BlastManagement.Domain.Aggregates;

namespace BlastManagement.Application.CommandHandlers;

internal sealed class MarkHoleReadyCommandHandler(IEventStore eventStore, IBlastProjection projection)
{
    public async Task HandleAsync(MarkHoleReadyCommand command, CancellationToken cancellationToken)
    {
        var history = await eventStore.LoadAsync(command.BlastId, cancellationToken);

        if (history.Count == 0)
        {
            throw new NotFoundException($"Blast '{command.BlastId}' was not found.");
        }

        var blast = Blast.LoadFromHistory(history.Select(envelope => envelope.Event));

        blast.MarkHoleReady(command.HoleId);

        await eventStore.AppendAsync(
            command.BlastId,
            history.Count,
            blast.GetUncommittedEvents(),
            cancellationToken);

        var stream = await eventStore.LoadAsync(command.BlastId, cancellationToken);
        projection.Apply(stream.Skip(history.Count).ToList());

        blast.ClearUncommittedEvents();
    }
}
