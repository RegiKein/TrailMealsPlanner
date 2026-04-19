using TrailMealsPlanner.Contracts.SharedEnums;

namespace TrailMealsPlanner.Contracts.DTO;

public sealed class EnvironmentConditionsContract
{
    public TemperatureRangeContract TemperatureRange { get; init; }

    public WaterAvailabilityContract WaterAvailability { get; init; }

    public AltitudeRangeContract AltitudeRange { get; init; }

    public HumidityLevelContract HumidityLevel { get; init; }
}
