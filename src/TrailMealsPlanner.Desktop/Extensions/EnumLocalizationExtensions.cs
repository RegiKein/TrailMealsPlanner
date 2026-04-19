using TrailMealsPlanner.Desktop.Services;
using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Desktop.Extensions;

public static class EnumLocalizationExtensions
{
    public static string ToDisplay(this ActivityType value)
    {
        return LocalizationService.Instance.Get(value switch
        {
            ActivityType.Hiking => "ActivityType_Hiking",
            ActivityType.Mountain => "ActivityType_Mountain",
            ActivityType.Water => "ActivityType_Water",
            ActivityType.Ski => "ActivityType_Ski",
            ActivityType.Speleo => "ActivityType_Speleo",
            ActivityType.Cycling => "ActivityType_Cycling",
            ActivityType.AutoMoto => "ActivityType_AutoMoto",
            ActivityType.Sailing => "ActivityType_Sailing",
            ActivityType.Horseback => "ActivityType_Horseback",
            ActivityType.Alpine => "ActivityType_Alpine",
            ActivityType.Competition => "ActivityType_Competition",
            ActivityType.Hunting => "ActivityType_Hunting",
            ActivityType.WeekendTrip => "ActivityType_WeekendTrip",
            ActivityType.LeisureWalk => "ActivityType_LeisureWalk",
            _ => value.ToString()
        });
    }

    public static string ToDisplay(this TemperatureRange value)
    {
        return LocalizationService.Instance.Get(value switch
        {
            TemperatureRange.Hot => "Temperature_Hot",
            TemperatureRange.Mild => "Temperature_Mild",
            TemperatureRange.Cold => "Temperature_Cold",
            TemperatureRange.ExtremeCold => "Temperature_ExtremeCold",
            _ => value.ToString()
        });
    }

    public static string ToDisplay(this WaterAvailability value)
    {
        return LocalizationService.Instance.Get(value switch
        {
            WaterAvailability.Abundant => "Water_Abundant",
            WaterAvailability.Limited => "Water_Limited",
            WaterAvailability.Scarce => "Water_Scarce",
            WaterAvailability.None => "Water_None",
            _ => value.ToString()
        });
    }

    public static string ToDisplay(this AltitudeRange value)
    {
        return LocalizationService.Instance.Get(value switch
        {
            AltitudeRange.Low => "Altitude_Low",
            AltitudeRange.Medium => "Altitude_Medium",
            AltitudeRange.High => "Altitude_High",
            AltitudeRange.Extreme => "Altitude_Extreme",
            _ => value.ToString()
        });
    }

    public static string ToDisplay(this HumidityLevel value)
    {
        return LocalizationService.Instance.Get(value switch
        {
            HumidityLevel.Dry => "Humidity_Dry",
            HumidityLevel.Normal => "Humidity_Normal",
            HumidityLevel.Wet => "Humidity_Wet",
            _ => value.ToString()
        });
    }

    public static string ToDisplay(this WeightImportance value)
    {
        return LocalizationService.Instance.Get(value switch
        {
            WeightImportance.Low => "Weight_Low",
            WeightImportance.Medium => "Weight_Medium",
            WeightImportance.High => "Weight_High",
            WeightImportance.Critical => "Weight_Critical",
            _ => value.ToString()
        });
    }

    public static string ToDisplay(this CookingPossibility value)
    {
        return LocalizationService.Instance.Get(value switch
        {
            CookingPossibility.Full => "Cooking_Full",
            CookingPossibility.Limited => "Cooking_Limited",
            CookingPossibility.Minimal => "Cooking_Minimal",
            CookingPossibility.None => "Cooking_None",
            _ => value.ToString()
        });
    }

    public static string ToDisplay(this ResupplyFrequency value)
    {
        return LocalizationService.Instance.Get(value switch
        {
            ResupplyFrequency.Daily => "Resupply_Daily",
            ResupplyFrequency.EveryFewDays => "Resupply_EveryFewDays",
            ResupplyFrequency.Rare => "Resupply_Rare",
            ResupplyFrequency.None => "Resupply_None",
            _ => value.ToString()
        });
    }

    public static string ToDisplay(this CompetitionNutritionFocus value)
    {
        return LocalizationService.Instance.Get(value switch
        {
            CompetitionNutritionFocus.Balanced => "Competition_Balanced",
            CompetitionNutritionFocus.CarbHeavy => "Competition_CarbHeavy",
            CompetitionNutritionFocus.Lightweight => "Competition_Lightweight",
            CompetitionNutritionFocus.RecoveryFocused => "Competition_Recovery",
            _ => value.ToString()
        });
    }
}
