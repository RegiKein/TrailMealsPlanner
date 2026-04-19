using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.ValueObjects;

namespace TrailMealsPlanner.Domain.Services;

/// <summary>
/// Provides practical default profiles for supported activity types.
/// These presets are a starting point for UI forms and future recommendation logic.
/// </summary>
public static class RationProfileFactory
{
    public static RationProfile CreateDefault(ActivityType activityType)
    {
        return activityType switch
        {
            ActivityType.Hiking => Build(
                activityType,
                TemperatureRange.Mild,
                WaterAvailability.Limited,
                AltitudeRange.Medium,
                HumidityLevel.Normal,
                WeightImportance.Medium,
                CookingPossibility.Full,
                ResupplyFrequency.Rare),
            ActivityType.Mountain => Build(
                activityType,
                TemperatureRange.Cold,
                WaterAvailability.Limited,
                AltitudeRange.High,
                HumidityLevel.Normal,
                WeightImportance.High,
                CookingPossibility.Limited,
                ResupplyFrequency.Rare),
            ActivityType.Water => Build(
                activityType,
                TemperatureRange.Mild,
                WaterAvailability.Abundant,
                AltitudeRange.Low,
                HumidityLevel.Wet,
                WeightImportance.Low,
                CookingPossibility.Full,
                ResupplyFrequency.EveryFewDays),
            ActivityType.Ski => Build(
                activityType,
                TemperatureRange.Cold,
                WaterAvailability.Limited,
                AltitudeRange.Medium,
                HumidityLevel.Dry,
                WeightImportance.High,
                CookingPossibility.Limited,
                ResupplyFrequency.Rare),
            ActivityType.Speleo => Build(
                activityType,
                TemperatureRange.Mild,
                WaterAvailability.Limited,
                AltitudeRange.Low,
                HumidityLevel.Wet,
                WeightImportance.High,
                CookingPossibility.None,
                ResupplyFrequency.None),
            ActivityType.Cycling => Build(
                activityType,
                TemperatureRange.Mild,
                WaterAvailability.Limited,
                AltitudeRange.Medium,
                HumidityLevel.Normal,
                WeightImportance.High,
                CookingPossibility.Limited,
                ResupplyFrequency.EveryFewDays),
            ActivityType.AutoMoto => Build(
                activityType,
                TemperatureRange.Mild,
                WaterAvailability.Limited,
                AltitudeRange.Low,
                HumidityLevel.Normal,
                WeightImportance.Low,
                CookingPossibility.Full,
                ResupplyFrequency.Daily),
            ActivityType.Sailing => Build(
                activityType,
                TemperatureRange.Mild,
                WaterAvailability.Abundant,
                AltitudeRange.Low,
                HumidityLevel.Wet,
                WeightImportance.Medium,
                CookingPossibility.Full,
                ResupplyFrequency.EveryFewDays),
            ActivityType.Horseback => Build(
                activityType,
                TemperatureRange.Mild,
                WaterAvailability.Limited,
                AltitudeRange.Medium,
                HumidityLevel.Normal,
                WeightImportance.Medium,
                CookingPossibility.Limited,
                ResupplyFrequency.EveryFewDays),
            ActivityType.Alpine => Build(
                activityType,
                TemperatureRange.Cold,
                WaterAvailability.Scarce,
                AltitudeRange.Extreme,
                HumidityLevel.Dry,
                WeightImportance.Critical,
                CookingPossibility.Minimal,
                ResupplyFrequency.None),
            ActivityType.Competition => Build(
                activityType,
                TemperatureRange.Mild,
                WaterAvailability.Limited,
                AltitudeRange.Medium,
                HumidityLevel.Normal,
                WeightImportance.High,
                CookingPossibility.Minimal,
                ResupplyFrequency.Daily,
                CompetitionNutritionFocus.CarbHeavy),
            ActivityType.Hunting => Build(
                activityType,
                TemperatureRange.Cold,
                WaterAvailability.Limited,
                AltitudeRange.Medium,
                HumidityLevel.Normal,
                WeightImportance.Medium,
                CookingPossibility.Limited,
                ResupplyFrequency.None),
            ActivityType.WeekendTrip => Build(
                activityType,
                TemperatureRange.Mild,
                WaterAvailability.Limited,
                AltitudeRange.Low,
                HumidityLevel.Normal,
                WeightImportance.Low,
                CookingPossibility.Full,
                ResupplyFrequency.Daily),
            ActivityType.LeisureWalk => Build(
                activityType,
                TemperatureRange.Mild,
                WaterAvailability.Abundant,
                AltitudeRange.Low,
                HumidityLevel.Normal,
                WeightImportance.Low,
                CookingPossibility.Full,
                ResupplyFrequency.Daily),
            _ => throw new ArgumentOutOfRangeException(nameof(activityType), activityType, "Unsupported activity type.")
        };
    }

    private static RationProfile Build(
        ActivityType activityType,
        TemperatureRange temperatureRange,
        WaterAvailability waterAvailability,
        AltitudeRange altitudeRange,
        HumidityLevel humidityLevel,
        WeightImportance weightImportance,
        CookingPossibility cookingPossibility,
        ResupplyFrequency resupplyFrequency,
        CompetitionNutritionFocus? competitionFocus = null)
    {
        return new RationProfile(
            activityType,
            new EnvironmentConditions(temperatureRange, waterAvailability, altitudeRange, humidityLevel),
            new LogisticsConstraints(weightImportance, cookingPossibility, resupplyFrequency),
            competitionFocus);
    }
}
