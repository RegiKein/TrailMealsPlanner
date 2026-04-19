using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Domain.ValueObjects;

/// <summary>
/// Replaces season-based assumptions with concrete environmental constraints.
/// </summary>
public sealed class EnvironmentConditions
{
    public EnvironmentConditions(
        TemperatureRange temperatureRange,
        WaterAvailability waterAvailability,
        AltitudeRange altitudeRange,
        HumidityLevel humidityLevel)
    {
        TemperatureRange = temperatureRange;
        WaterAvailability = waterAvailability;
        AltitudeRange = altitudeRange;
        HumidityLevel = humidityLevel;
    }

    public TemperatureRange TemperatureRange { get; }

    public WaterAvailability WaterAvailability { get; }

    public AltitudeRange AltitudeRange { get; }

    public HumidityLevel HumidityLevel { get; }
}
