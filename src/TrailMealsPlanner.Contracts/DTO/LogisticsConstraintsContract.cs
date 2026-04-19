using TrailMealsPlanner.Contracts.SharedEnums;

namespace TrailMealsPlanner.Contracts.DTO;

public sealed class LogisticsConstraintsContract
{
    public WeightImportanceContract WeightImportance { get; init; }

    public CookingPossibilityContract CookingPossibility { get; init; }

    public ResupplyFrequencyContract ResupplyFrequency { get; init; }
}
