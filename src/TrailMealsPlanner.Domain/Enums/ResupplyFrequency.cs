namespace TrailMealsPlanner.Domain.Enums;

/// <summary>
/// Describes how often the group can restock food during the route.
/// </summary>
public enum ResupplyFrequency
{
    Daily = 0,
    EveryFewDays = 1,
    Rare = 2,
    None = 3
}
