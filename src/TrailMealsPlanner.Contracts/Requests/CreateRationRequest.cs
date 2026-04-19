using TrailMealsPlanner.Contracts.SharedEnums;

namespace TrailMealsPlanner.Contracts.Requests;

public sealed class CreateRationRequest
{
    public string Name { get; init; } = string.Empty;

    public DateTime StartDate { get; init; }

    public int DurationDays { get; init; }

    public int ParticipantCount { get; init; }

    public ActivityTypeContract ActivityType { get; init; }

    public TemperatureRangeContract TemperatureRange { get; init; }

    public WaterAvailabilityContract WaterAvailability { get; init; }

    public AltitudeRangeContract AltitudeRange { get; init; }

    public HumidityLevelContract HumidityLevel { get; init; }

    public WeightImportanceContract WeightImportance { get; init; }

    public CookingPossibilityContract CookingPossibility { get; init; }

    public ResupplyFrequencyContract ResupplyFrequency { get; init; }

    public CompetitionNutritionFocusContract? CompetitionFocus { get; init; }
}
