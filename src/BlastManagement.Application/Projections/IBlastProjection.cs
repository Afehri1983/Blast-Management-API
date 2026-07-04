using BlastManagement.Application.Abstractions;
using BlastManagement.Application.ReadModels;

namespace BlastManagement.Application.Projections;

public interface IBlastProjection
{
    BlastReadModel? Get(Guid blastId);

    void Apply(IReadOnlyList<EventEnvelope> envelopes);
}
