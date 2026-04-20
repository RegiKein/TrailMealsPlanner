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
        services.AddSingleton<DishCatalogViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<ProductCatalogViewModel>();
        services.AddSingleton<RationDetailsViewModel>();
        services.AddSingleton<DishCatalogView>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<ProductCatalogView>();
        services.AddSingleton<RationDetailsView>();

        return services;
    }
}
