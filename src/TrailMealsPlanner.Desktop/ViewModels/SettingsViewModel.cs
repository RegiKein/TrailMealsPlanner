using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using TrailMealsPlanner.Desktop.Models;
using TrailMealsPlanner.Desktop.Services;

namespace TrailMealsPlanner.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly LocalizationService localizationService;
    private bool isRefreshing;

    [ObservableProperty]
    private IReadOnlyList<LanguageOption> languageOptions = [];

    [ObservableProperty]
    private LanguageOption? selectedLanguageOption;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public SettingsViewModel(LocalizationService localizationService)
    {
        this.localizationService = localizationService;
        localizationService.CultureChanged += OnCultureChanged;
        Refresh();
    }

    public string Title => "Настройки";

    public string LanguageLabel => localizationService.Get("Language_Label");

    partial void OnSelectedLanguageOptionChanged(LanguageOption? value)
    {
        if (!isRefreshing && value is not null)
        {
            localizationService.SetCulture(value.CultureName);
            StatusMessage = $"Язык интерфейса: {value.Display}.";
        }
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        Refresh();
    }

    private void Refresh()
    {
        isRefreshing = true;
        LanguageOptions =
        [
            new LanguageOption("en", localizationService.Get("Language_English")),
            new LanguageOption("ru", localizationService.Get("Language_Russian"))
        ];
        SelectedLanguageOption = LanguageOptions.First(option =>
            option.CultureName.Equals(localizationService.CurrentCulture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase));
        StatusMessage = $"Текущий язык: {SelectedLanguageOption.Display}.";
        isRefreshing = false;
        OnPropertyChanged(nameof(LanguageLabel));
    }
}
