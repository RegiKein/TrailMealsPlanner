using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.DTO;

public sealed class FoodIssueDto
{
    public PreferenceLevel Severity { get; init; }

    public string Summary { get; init; } = string.Empty;
}
