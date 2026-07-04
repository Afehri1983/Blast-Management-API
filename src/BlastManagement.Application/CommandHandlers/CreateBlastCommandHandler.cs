using BlastManagement.Application.Abstractions;
using BlastManagement.Application.Commands;
using BlastManagement.Application.Projections;
using BlastManagement.Domain.Aggregates;

namespace BlastManagement.Application.CommandHandlers;

internal sealed class CreateBlastCommandHandler(IEventStore eventStore, IBlastProjection projection)
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

        var stream = await eventStore.LoadAsync(blastId, cancellationToken);
        projection.Apply(stream);

        blast.ClearUncommittedEvents();
        return blastId;
    }
}
