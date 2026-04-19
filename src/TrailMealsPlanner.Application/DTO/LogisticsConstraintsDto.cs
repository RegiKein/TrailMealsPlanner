using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.DTO;

public sealed class LogisticsConstraintsDto
{
    public WeightImportance WeightImportance { get; init; }

    public CookingPossibility CookingPossibility { get; init; }

    public ResupplyFrequency ResupplyFrequency { get; init; }
}
