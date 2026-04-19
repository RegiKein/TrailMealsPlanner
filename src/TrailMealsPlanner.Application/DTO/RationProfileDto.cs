using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.DTO;

public sealed class RationProfileDto
{
    public ActivityType ActivityType { get; init; }

    public EnvironmentConditionsDto Environment { get; init; } = new();

    public LogisticsConstraintsDto Logistics { get; init; } = new();

    public CompetitionNutritionFocus? CompetitionFocus { get; init; }
}
