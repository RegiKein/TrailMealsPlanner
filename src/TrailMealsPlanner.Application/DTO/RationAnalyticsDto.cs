namespace TrailMealsPlanner.Application.DTO;

public sealed class RationAnalyticsDto
{
    public Guid RationId { get; init; }

    public NutritionInfoDto Nutrition { get; init; } = new();

    public IReadOnlyList<RationDayAnalyticsDto> Days { get; init; } = [];
}
