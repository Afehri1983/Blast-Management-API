using BlastManagement.Application.CommandHandlers;
using BlastManagement.Application.Projections;
using BlastManagement.Application.QueryHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace BlastManagement.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IBlastProjection, BlastProjection>();

        services.AddTransient<CreateBlastCommandHandler>();
        services.AddTransient<AddHoleCommandHandler>();
        services.AddTransient<ChargeHoleCommandHandler>();
        services.AddTransient<MarkHoleReadyCommandHandler>();
        services.AddTransient<FireBlastCommandHandler>();

        services.AddTransient<GetBlastQueryHandler>();
        services.AddTransient<GetBlastHistoryQueryHandler>();

        return services;
    }
}
