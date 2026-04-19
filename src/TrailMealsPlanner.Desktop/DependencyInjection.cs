using Microsoft.Extensions.DependencyInjection;
using TrailMealsPlanner.Application;
using TrailMealsPlanner.Desktop.Services;
using TrailMealsPlanner.Desktop.ViewModels;
using TrailMealsPlanner.Desktop.Views;
using TrailMealsPlanner.Infrastructure;

namespace TrailMealsPlanner.Desktop;

public static class DependencyInjection
{
    public static IServiceCollection AddDesktop(this IServiceCollection services)
    {
        services.AddSingleton(LocalizationService.Instance);
        services.AddApplication();
        services.AddInfrastructure();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}
