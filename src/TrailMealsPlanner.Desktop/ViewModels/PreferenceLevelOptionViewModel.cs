using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class PreferenceLevelOptionViewModel
{
    public PreferenceLevelOptionViewModel(PreferenceLevel value, string display)
    {
        Value = value;
        Display = display;
    }

    public PreferenceLevel Value { get; }

    public string Display { get; }
}
