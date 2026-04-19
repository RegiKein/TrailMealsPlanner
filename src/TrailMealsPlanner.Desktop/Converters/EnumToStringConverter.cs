using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TrailMealsPlanner.Desktop.Extensions;
using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Desktop.Converters;

public sealed class EnumToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            ActivityType activityType => activityType.ToDisplay(),
            TemperatureRange temperatureRange => temperatureRange.ToDisplay(),
            WaterAvailability waterAvailability => waterAvailability.ToDisplay(),
            AltitudeRange altitudeRange => altitudeRange.ToDisplay(),
            HumidityLevel humidityLevel => humidityLevel.ToDisplay(),
            WeightImportance weightImportance => weightImportance.ToDisplay(),
            CookingPossibility cookingPossibility => cookingPossibility.ToDisplay(),
            ResupplyFrequency resupplyFrequency => resupplyFrequency.ToDisplay(),
            CompetitionNutritionFocus competitionNutritionFocus => competitionNutritionFocus.ToDisplay(),
            Enum enumValue => enumValue.ToString(),
            null => string.Empty,
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
