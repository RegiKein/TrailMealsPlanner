namespace TrailMealsPlanner.Desktop.Models;

public sealed class LanguageOption
{
    public LanguageOption(string cultureName, string display)
    {
        CultureName = cultureName;
        Display = display;
    }

    public string CultureName { get; }

    public string Display { get; }
}
