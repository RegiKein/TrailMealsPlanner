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
        services.AddSingleton<ProjectFileDialogService>();
        services.AddApplication();
        services.AddInfrastructure();
        services.AddSingleton<DayTemplatesViewModel>();
        services.AddSingleton<DishCatalogViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<ParticipantsCatalogViewModel>();
        services.AddSingleton<ProductCatalogViewModel>();
        services.AddSingleton<RationListViewModel>();
        services.AddSingleton<RationDetailsViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<DayTemplatesView>();
        services.AddSingleton<DishCatalogView>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<ParticipantsCatalogView>();
        services.AddSingleton<ProductCatalogView>();
        services.AddSingleton<RationListView>();
        services.AddSingleton<RationDetailsView>();
        services.AddSingleton<SettingsView>();

        return services;
    }
}
