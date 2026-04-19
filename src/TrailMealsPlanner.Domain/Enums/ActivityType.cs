namespace TrailMealsPlanner.Domain.Enums;

/// <summary>
/// Defines the primary trip or activity format that drives ration defaults.
/// Season is no longer the primary classifier because practical ration needs
/// depend more on activity, environment, and logistics than on calendar month.
/// </summary>
public enum ActivityType
{
    Hiking = 0,
    Mountain = 1,
    Water = 2,
    Ski = 3,
    Speleo = 4,
    Cycling = 5,
    AutoMoto = 6,
    Sailing = 7,
    Horseback = 8,
    Alpine = 9,
    Competition = 10,
    Hunting = 11,
    WeekendTrip = 12,
    LeisureWalk = 13
}
