using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.ValueObjects;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class GetRationAnalyticsHandler
{
    private readonly IRationProjectRepository rationRepository;
    private readonly IDishRepository dishRepository;
    private readonly IProductRepository productRepository;

    public GetRationAnalyticsHandler(
        IRationProjectRepository rationRepository,
        IDishRepository dishRepository,
        IProductRepository productRepository)
    {
        this.rationRepository = rationRepository;
        this.dishRepository = dishRepository;
        this.productRepository = productRepository;
    }

    public async Task<RationAnalyticsDto?> Handle(
        GetRationAnalyticsQuery query,
        CancellationToken cancellationToken = default)
    {
        var ration = await rationRepository.GetByIdAsync(query.RationId, cancellationToken);
        if (ration is null)
        {
            return null;
        }

        var dishes = await dishRepository.GetAllAsync(cancellationToken);
        var products = await productRepository.GetAllAsync(cancellationToken);
        var dishesById = dishes.ToDictionary(dish => dish.Id);
        var productsById = products.ToDictionary(product => product.Id);

        return new RationAnalyticsDto
        {
            RationId = ration.Id,
            Nutrition = ToDto(ration.CalculateNutrition(dishesById, productsById)),
            Days = ration.Days
                .OrderBy(day => day.DayNumber)
                .Select(day => new RationDayAnalyticsDto
                {
                    DayId = day.Id,
                    DayNumber = day.DayNumber,
                    Date = day.Date,
                    Nutrition = ToDto(day.CalculateNutrition(dishesById, productsById)),
                    Meals = day.Meals
                        .Select(meal => new MealAnalyticsDto
                        {
                            MealId = meal.Id,
                            MealType = meal.Type,
                            Nutrition = ToDto(meal.CalculateNutrition(dishesById, productsById))
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    private static NutritionInfoDto ToDto(NutritionInfo nutrition)
    {
        return new NutritionInfoDto
        {
            Calories = nutrition.Calories,
            Weight = nutrition.Weight,
            CaloriesPerGram = nutrition.CaloriesPerGram
        };
    }
}
