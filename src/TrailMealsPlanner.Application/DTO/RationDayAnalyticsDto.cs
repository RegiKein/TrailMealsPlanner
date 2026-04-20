namespace TrailMealsPlanner.Application.DTO;

public sealed class RationDayAnalyticsDto
{
    public Guid DayId { get; init; }

    public int DayNumber { get; init; }

    public DateTime Date { get; init; }

    public NutritionInfoDto Nutrition { get; init; } = new();

    public IReadOnlyList<MealAnalyticsDto> Meals { get; init; } = [];
}
