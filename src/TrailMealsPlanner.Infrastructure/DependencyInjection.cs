using Microsoft.Extensions.DependencyInjection;
using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Infrastructure.Persistence;

namespace TrailMealsPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IRationProjectRepository, InMemoryRationProjectRepository>();

        return services;
    }
}
