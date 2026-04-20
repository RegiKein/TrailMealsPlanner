using Microsoft.Extensions.DependencyInjection;
using TrailMealsPlanner.Application.UseCases;

namespace TrailMealsPlanner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<AddDishToMealHandler>();
        services.AddTransient<ApplyDayTemplateHandler>();
        services.AddTransient<CopyDayHandler>();
        services.AddTransient<CopyMealHandler>();
        services.AddTransient<CreateDishHandler>();
        services.AddTransient<CreateProductHandler>();
        services.AddTransient<CreateRationHandler>();
        services.AddTransient<ExportRationHandler>();
        services.AddTransient<GetDayTemplatesHandler>();
        services.AddTransient<GetDishesHandler>();
        services.AddTransient<GetRationAnalyticsHandler>();
        services.AddTransient<GetRationByIdHandler>();
        services.AddTransient<GetRationWarningsHandler>();
        services.AddTransient<GetProductsHandler>();
        services.AddTransient<GetRationsHandler>();
        services.AddTransient<SaveDayAsTemplateHandler>();

        return services;
    }
}
