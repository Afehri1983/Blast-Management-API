using BlastManagement.Application.Abstractions;
using BlastManagement.Application.Commands;
using BlastManagement.Application.Exceptions;
using BlastManagement.Domain.Aggregates;

namespace BlastManagement.Application.CommandHandlers;

internal sealed class AddHoleCommandHandler(IEventStore eventStore)
{
    public async Task HandleAsync(AddHoleCommand command, CancellationToken cancellationToken)
    {
        var history = await eventStore.LoadAsync(command.BlastId, cancellationToken);

        if (history.Count == 0)
        {
            throw new NotFoundException($"Blast '{command.BlastId}' was not found.");
        }

        var blast = Blast.LoadFromHistory(history.Select(envelope => envelope.Event));

        blast.AddHole(
            command.HoleId,
            command.Name,
            command.Position,
            command.Direction,
            command.Inclination);

        await eventStore.AppendAsync(
            command.BlastId,
            history.Count,
            blast.GetUncommittedEvents(),
            cancellationToken);

        blast.ClearUncommittedEvents();
    }
}
