using Microsoft.Extensions.DependencyInjection;
using TrailMealsPlanner.Application.UseCases;

namespace TrailMealsPlanner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateRationHandler>();
        services.AddTransient<GetRationsHandler>();

        return services;
    }
}
