using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.DTO;

public sealed class MealAnalyticsDto
{
    public Guid MealId { get; init; }

    public MealType MealType { get; init; }

    public NutritionInfoDto Nutrition { get; init; } = new();
}
