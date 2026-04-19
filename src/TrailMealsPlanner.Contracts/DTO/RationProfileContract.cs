using TrailMealsPlanner.Contracts.SharedEnums;

namespace TrailMealsPlanner.Contracts.DTO;

public sealed class RationProfileContract
{
    public ActivityTypeContract ActivityType { get; init; }

    public EnvironmentConditionsContract Environment { get; init; } = new();

    public LogisticsConstraintsContract Logistics { get; init; } = new();

    public CompetitionNutritionFocusContract? CompetitionFocus { get; init; }
}
