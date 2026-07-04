using BlastManagement.Application.Abstractions;
using BlastManagement.Application.Exceptions;
using BlastManagement.Application.Queries;

namespace BlastManagement.Application.QueryHandlers;

internal sealed class GetBlastHistoryQueryHandler(IEventStore eventStore)
{
    public async Task<IReadOnlyList<EventEnvelope>> HandleAsync(
        GetBlastHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var history = await eventStore.LoadAsync(query.BlastId, cancellationToken);

        if (history.Count == 0)
        {
            throw new NotFoundException($"Blast '{query.BlastId}' was not found.");
        }

        return history;
    }
}
