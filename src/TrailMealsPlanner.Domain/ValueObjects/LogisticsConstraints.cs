using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Domain.ValueObjects;

/// <summary>
/// Captures non-environmental constraints that drive ration practicality.
/// </summary>
public sealed class LogisticsConstraints
{
    public LogisticsConstraints(
        WeightImportance weightImportance,
        CookingPossibility cookingPossibility,
        ResupplyFrequency resupplyFrequency)
    {
        WeightImportance = weightImportance;
        CookingPossibility = cookingPossibility;
        ResupplyFrequency = resupplyFrequency;
    }

    public WeightImportance WeightImportance { get; }

    public CookingPossibility CookingPossibility { get; }

    public ResupplyFrequency ResupplyFrequency { get; }
}
