using BlastManagement.Application.Abstractions;
using BlastManagement.Infrastructure.EventStore;
using Microsoft.Extensions.DependencyInjection;

namespace BlastManagement.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        return services;
    }
}
