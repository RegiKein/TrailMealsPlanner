using Microsoft.Extensions.DependencyInjection;
using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Infrastructure.Export;
using TrailMealsPlanner.Infrastructure.Persistence;

namespace TrailMealsPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IRationExportService, RationExportService>();
        services.AddSingleton<IDayTemplateRepository, InMemoryDayTemplateRepository>();
        services.AddSingleton<IDishRepository, InMemoryDishRepository>();
        services.AddSingleton<IParticipantRepository, InMemoryParticipantRepository>();
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddSingleton<IProductPreferenceRepository, InMemoryProductPreferenceRepository>();
        services.AddSingleton<IRationProjectRepository, InMemoryRationProjectRepository>();

        return services;
    }
}
