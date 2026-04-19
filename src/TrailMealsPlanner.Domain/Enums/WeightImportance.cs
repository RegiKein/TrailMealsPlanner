namespace TrailMealsPlanner.Domain.Enums;

/// <summary>
/// Expresses how aggressively ration weight should be optimized for the trip profile.
/// </summary>
public enum WeightImportance
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
