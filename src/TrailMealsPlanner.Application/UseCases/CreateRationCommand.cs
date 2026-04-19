using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class CreateRationCommand
{
    public string Name { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public int DurationDays { get; set; }

    public int ParticipantCount { get; set; }

    public ActivityType ActivityType { get; set; }

    public TemperatureRange TemperatureRange { get; set; }

    public WaterAvailability WaterAvailability { get; set; }

    public AltitudeRange AltitudeRange { get; set; }

    public HumidityLevel HumidityLevel { get; set; }

    public WeightImportance WeightImportance { get; set; }

    public CookingPossibility CookingPossibility { get; set; }

    public ResupplyFrequency ResupplyFrequency { get; set; }

    public CompetitionNutritionFocus? CompetitionFocus { get; set; }
}
