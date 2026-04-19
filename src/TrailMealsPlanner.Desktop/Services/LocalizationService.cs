using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace TrailMealsPlanner.Desktop.Services;

public sealed class LocalizationService
{
    private readonly ResourceManager resourceManager;

    private LocalizationService()
    {
        resourceManager = new ResourceManager("TrailMealsPlanner.Desktop.Resources.Strings", Assembly.GetExecutingAssembly());
        CurrentCulture = ResolveInitialCulture();
        ApplyCulture(CurrentCulture);
    }

    public static LocalizationService Instance { get; } = new();

    public event EventHandler? CultureChanged;

    public CultureInfo CurrentCulture { get; private set; }

    public string Get(string key)
    {
        return resourceManager.GetString(key, CurrentCulture) ?? key;
    }

    public string Format(string key, params object[] args)
    {
        return string.Format(CurrentCulture, Get(key), args);
    }

    public void SetCulture(string cultureName)
    {
        SetCulture(new CultureInfo(cultureName));
    }

    public void SetCulture(CultureInfo culture)
    {
        if (CurrentCulture.Name.Equals(culture.Name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        CurrentCulture = culture;
        ApplyCulture(culture);
        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    private static CultureInfo ResolveInitialCulture()
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return language.Equals("ru", StringComparison.OrdinalIgnoreCase)
            ? new CultureInfo("ru")
            : new CultureInfo("en");
    }

    private static void ApplyCulture(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
