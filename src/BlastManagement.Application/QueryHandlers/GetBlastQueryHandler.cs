using BlastManagement.Application.Exceptions;
using BlastManagement.Application.Projections;
using BlastManagement.Application.Queries;
using BlastManagement.Application.ReadModels;

namespace BlastManagement.Application.QueryHandlers;

internal sealed class GetBlastQueryHandler(IBlastProjection projection)
{
    public Task<BlastReadModel> HandleAsync(GetBlastQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var blast = projection.Get(query.BlastId);

        if (blast is null)
        {
            throw new NotFoundException($"Blast '{query.BlastId}' was not found.");
        }

        return Task.FromResult(blast);
    }
}
