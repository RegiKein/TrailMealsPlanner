using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.DTO;

public sealed class EnvironmentConditionsDto
{
    public TemperatureRange TemperatureRange { get; init; }

    public WaterAvailability WaterAvailability { get; init; }

    public AltitudeRange AltitudeRange { get; init; }

    public HumidityLevel HumidityLevel { get; init; }
}
