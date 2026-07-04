using BlastManagement.Application.Abstractions;
using BlastManagement.Application.Commands;
using BlastManagement.Domain.Aggregates;

namespace BlastManagement.Application.CommandHandlers;

internal sealed class CreateBlastCommandHandler(IEventStore eventStore)
{
    public async Task<Guid> HandleAsync(CreateBlastCommand command, CancellationToken cancellationToken)
    {
        var blastId = Guid.NewGuid();
        var blast = Blast.Create(blastId, command.Name);

        await eventStore.AppendAsync(
            blastId,
            expectedVersion: 0,
            blast.GetUncommittedEvents(),
            cancellationToken);

        blast.ClearUncommittedEvents();
        return blastId;
    }
}
