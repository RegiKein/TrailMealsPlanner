using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Domain.ValueObjects;

/// <summary>
/// Aggregates the main dimensions used for ration planning.
/// Season is intentionally excluded from decision-making because
/// the same calendar season can imply different needs across activities.
/// </summary>
public sealed class RationProfile
{
    public RationProfile(
        ActivityType activityType,
        EnvironmentConditions environment,
        LogisticsConstraints logistics,
        CompetitionNutritionFocus? competitionFocus = null)
    {
        Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        Logistics = logistics ?? throw new ArgumentNullException(nameof(logistics));

        if (activityType != ActivityType.Competition && competitionFocus is not null)
        {
            throw new ArgumentException(
                "Competition nutrition focus can only be used for competition activity.",
                nameof(competitionFocus));
        }

        ActivityType = activityType;
        CompetitionFocus = competitionFocus;
    }

    public ActivityType ActivityType { get; }

    public EnvironmentConditions Environment { get; }

    public LogisticsConstraints Logistics { get; }

    public CompetitionNutritionFocus? CompetitionFocus { get; }
}
