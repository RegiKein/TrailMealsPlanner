using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Desktop.Extensions;
using TrailMealsPlanner.Desktop.Models;
using TrailMealsPlanner.Desktop.Services;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;
using TrailMealsPlanner.Domain.ValueObjects;

namespace TrailMealsPlanner.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly CreateRationHandler createRationHandler;
    private readonly GetRationsHandler getRationsHandler;
    private readonly LocalizationService localizationService;
    private IReadOnlyList<RationProjectListItemDto> loadedProjects = [];
    private string statusMessageKey = "Status_EnterParameters";
    private object[] statusMessageArgs = [];
    private bool isRefreshingLocalizationState;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? startDate = DateTimeOffset.Now;

    [ObservableProperty]
    private int durationDays = 3;

    [ObservableProperty]
    private int participantCount = 2;

    [ObservableProperty]
    private IReadOnlyList<LanguageOption> languageOptions = [];

    [ObservableProperty]
    private LanguageOption? selectedLanguageOption;

    [ObservableProperty]
    private IReadOnlyList<EnumOption<ActivityType>> activityTypeOptions = [];

    [ObservableProperty]
    private EnumOption<ActivityType>? selectedActivityTypeOption;

    [ObservableProperty]
    private IReadOnlyList<EnumOption<TemperatureRange>> temperatureRangeOptions = [];

    [ObservableProperty]
    private EnumOption<TemperatureRange>? selectedTemperatureRangeOption;

    [ObservableProperty]
    private IReadOnlyList<EnumOption<WaterAvailability>> waterAvailabilityOptions = [];

    [ObservableProperty]
    private EnumOption<WaterAvailability>? selectedWaterAvailabilityOption;

    [ObservableProperty]
    private IReadOnlyList<EnumOption<AltitudeRange>> altitudeRangeOptions = [];

    [ObservableProperty]
    private EnumOption<AltitudeRange>? selectedAltitudeRangeOption;

    [ObservableProperty]
    private IReadOnlyList<EnumOption<HumidityLevel>> humidityLevelOptions = [];

    [ObservableProperty]
    private EnumOption<HumidityLevel>? selectedHumidityLevelOption;

    [ObservableProperty]
    private IReadOnlyList<EnumOption<WeightImportance>> weightImportanceOptions = [];

    [ObservableProperty]
    private EnumOption<WeightImportance>? selectedWeightImportanceOption;

    [ObservableProperty]
    private IReadOnlyList<EnumOption<CookingPossibility>> cookingPossibilityOptions = [];

    [ObservableProperty]
    private EnumOption<CookingPossibility>? selectedCookingPossibilityOption;

    [ObservableProperty]
    private IReadOnlyList<EnumOption<ResupplyFrequency>> resupplyFrequencyOptions = [];

    [ObservableProperty]
    private EnumOption<ResupplyFrequency>? selectedResupplyFrequencyOption;

    [ObservableProperty]
    private IReadOnlyList<EnumOption<CompetitionNutritionFocus>> competitionNutritionFocusOptions = [];

    [ObservableProperty]
    private EnumOption<CompetitionNutritionFocus>? selectedCompetitionNutritionFocusOption;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private RationProjectListItemViewModel? selectedRationProject;

    public MainWindowViewModel(
        CreateRationHandler createRationHandler,
        GetRationsHandler getRationsHandler,
        LocalizationService localizationService)
    {
        this.createRationHandler = createRationHandler;
        this.getRationsHandler = getRationsHandler;
        this.localizationService = localizationService;

        localizationService.CultureChanged += OnCultureChanged;
        RefreshLocalizedState();
        SetStatus("Status_EnterParameters");
    }

    public ObservableCollection<RationProjectListItemViewModel> RationProjects { get; } = [];

    public string WindowTitle => localizationService.Get("Window_Title");
    public string LanguageLabel => localizationService.Get("Language_Label");
    public string NewProjectTitle => localizationService.Get("NewProject_Title");
    public string NewProjectSubtitle => localizationService.Get("NewProject_Subtitle");
    public string NameLabel => localizationService.Get("Label_Name");
    public string NameWatermark => localizationService.Get("Watermark_Name");
    public string StartDateLabel => localizationService.Get("Label_StartDate");
    public string DurationDaysLabel => localizationService.Get("Label_DurationDays");
    public string ParticipantsLabel => localizationService.Get("Label_Participants");
    public string ActivityTypeLabel => localizationService.Get("Label_ActivityType");
    public string EnvironmentSectionTitle => localizationService.Get("Section_Environment");
    public string TemperatureLabel => localizationService.Get("Label_Temperature");
    public string WaterAvailabilityLabel => localizationService.Get("Label_WaterAvailability");
    public string AltitudeLabel => localizationService.Get("Label_Altitude");
    public string HumidityLabel => localizationService.Get("Label_Humidity");
    public string LogisticsSectionTitle => localizationService.Get("Section_Logistics");
    public string WeightImportanceLabel => localizationService.Get("Label_WeightImportance");
    public string CookingPossibilityLabel => localizationService.Get("Label_CookingPossibility");
    public string ResupplyFrequencyLabel => localizationService.Get("Label_ResupplyFrequency");
    public string CompetitionNutritionFocusTitle => localizationService.Get("Section_CompetitionNutritionFocus");
    public string CreateButtonText => localizationService.Get("Button_Create");
    public string SavedProjectsTitle => localizationService.Get("SavedProjects_Title");
    public string SavedProjectsSubtitle => localizationService.Get("SavedProjects_Subtitle");

    public bool IsCompetitionSelected => SelectedActivityTypeOption?.Value == ActivityType.Competition;

    partial void OnSelectedLanguageOptionChanged(LanguageOption? value)
    {
        if (isRefreshingLocalizationState || value is null)
        {
            return;
        }

        localizationService.SetCulture(value.CultureName);
    }

    partial void OnSelectedActivityTypeOptionChanged(EnumOption<ActivityType>? value)
    {
        if (isRefreshingLocalizationState || value is null)
        {
            return;
        }

        ApplyProfile(RationProfileFactory.CreateDefault(value.Value));
        OnPropertyChanged(nameof(IsCompetitionSelected));
    }

    [RelayCommand]
    private async Task CreateRation()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            SetStatus("Status_NameRequired");
            return;
        }

        if (DurationDays <= 0)
        {
            SetStatus("Status_DurationInvalid");
            return;
        }

        if (ParticipantCount <= 0)
        {
            SetStatus("Status_ParticipantCountInvalid");
            return;
        }

        var rationName = Name.Trim();

        await createRationHandler.Handle(new CreateRationCommand
        {
            Name = rationName,
            StartDate = StartDate?.DateTime ?? DateTime.Today,
            DurationDays = DurationDays,
            ParticipantCount = ParticipantCount,
            ActivityType = SelectedActivityTypeOption?.Value ?? ActivityType.Hiking,
            TemperatureRange = SelectedTemperatureRangeOption?.Value ?? TemperatureRange.Mild,
            WaterAvailability = SelectedWaterAvailabilityOption?.Value ?? WaterAvailability.Limited,
            AltitudeRange = SelectedAltitudeRangeOption?.Value ?? AltitudeRange.Low,
            HumidityLevel = SelectedHumidityLevelOption?.Value ?? HumidityLevel.Normal,
            WeightImportance = SelectedWeightImportanceOption?.Value ?? WeightImportance.Medium,
            CookingPossibility = SelectedCookingPossibilityOption?.Value ?? CookingPossibility.Full,
            ResupplyFrequency = SelectedResupplyFrequencyOption?.Value ?? ResupplyFrequency.Rare,
            CompetitionFocus = IsCompetitionSelected ? SelectedCompetitionNutritionFocusOption?.Value : null
        });

        await ReloadProjects();

        SelectedRationProject = RationProjects.FirstOrDefault(project => project.Name == rationName);
        SetStatus("Status_RationSaved", rationName);

        Name = string.Empty;
        StartDate = DateTimeOffset.Now;
        DurationDays = 3;
        ParticipantCount = 2;
        ApplyDefaultProfile(ActivityType.Hiking);
    }

    public async Task InitializeAsync()
    {
        await ReloadProjects();
    }

    [RelayCommand]
    private async Task LoadRations()
    {
        await ReloadProjects();
        SetStatus("Status_RationsLoaded", RationProjects.Count);
    }

    private async Task ReloadProjects()
    {
        loadedProjects = await getRationsHandler.Handle(new GetRationsQuery());
        RebuildProjectList();
    }

    private void RebuildProjectList()
    {
        RationProjects.Clear();

        foreach (var project in loadedProjects)
        {
            RationProjects.Add(new RationProjectListItemViewModel(project, localizationService));
        }
    }

    private void ApplyDefaultProfile(ActivityType activityType)
    {
        isRefreshingLocalizationState = true;
        SelectedActivityTypeOption = FindOption(ActivityTypeOptions, activityType);
        isRefreshingLocalizationState = false;

        ApplyProfile(RationProfileFactory.CreateDefault(activityType));
        OnPropertyChanged(nameof(IsCompetitionSelected));
    }

    private void ApplyProfile(RationProfile profile)
    {
        isRefreshingLocalizationState = true;

        SelectedTemperatureRangeOption = FindOption(TemperatureRangeOptions, profile.Environment.TemperatureRange);
        SelectedWaterAvailabilityOption = FindOption(WaterAvailabilityOptions, profile.Environment.WaterAvailability);
        SelectedAltitudeRangeOption = FindOption(AltitudeRangeOptions, profile.Environment.AltitudeRange);
        SelectedHumidityLevelOption = FindOption(HumidityLevelOptions, profile.Environment.HumidityLevel);
        SelectedWeightImportanceOption = FindOption(WeightImportanceOptions, profile.Logistics.WeightImportance);
        SelectedCookingPossibilityOption = FindOption(CookingPossibilityOptions, profile.Logistics.CookingPossibility);
        SelectedResupplyFrequencyOption = FindOption(ResupplyFrequencyOptions, profile.Logistics.ResupplyFrequency);
        SelectedCompetitionNutritionFocusOption = profile.CompetitionFocus is null
            ? null
            : FindOption(CompetitionNutritionFocusOptions, profile.CompetitionFocus.Value);

        isRefreshingLocalizationState = false;
        OnPropertyChanged(nameof(IsCompetitionSelected));
    }

    private void RefreshLocalizedState()
    {
        var selectedActivityType = SelectedActivityTypeOption?.Value ?? ActivityType.Hiking;
        var selectedTemperatureRange = SelectedTemperatureRangeOption?.Value ?? TemperatureRange.Mild;
        var selectedWaterAvailability = SelectedWaterAvailabilityOption?.Value ?? WaterAvailability.Limited;
        var selectedAltitudeRange = SelectedAltitudeRangeOption?.Value ?? AltitudeRange.Low;
        var selectedHumidityLevel = SelectedHumidityLevelOption?.Value ?? HumidityLevel.Normal;
        var selectedWeightImportance = SelectedWeightImportanceOption?.Value ?? WeightImportance.Medium;
        var selectedCookingPossibility = SelectedCookingPossibilityOption?.Value ?? CookingPossibility.Full;
        var selectedResupplyFrequency = SelectedResupplyFrequencyOption?.Value ?? ResupplyFrequency.Rare;
        var selectedCompetitionFocus = SelectedCompetitionNutritionFocusOption?.Value;

        isRefreshingLocalizationState = true;

        LanguageOptions =
        [
            new LanguageOption("en", localizationService.Get("Language_English")),
            new LanguageOption("ru", localizationService.Get("Language_Russian"))
        ];

        ActivityTypeOptions = Enum.GetValues<ActivityType>().Select(value => new EnumOption<ActivityType>(value, value.ToDisplay())).ToList();
        TemperatureRangeOptions = Enum.GetValues<TemperatureRange>().Select(value => new EnumOption<TemperatureRange>(value, value.ToDisplay())).ToList();
        WaterAvailabilityOptions = Enum.GetValues<WaterAvailability>().Select(value => new EnumOption<WaterAvailability>(value, value.ToDisplay())).ToList();
        AltitudeRangeOptions = Enum.GetValues<AltitudeRange>().Select(value => new EnumOption<AltitudeRange>(value, value.ToDisplay())).ToList();
        HumidityLevelOptions = Enum.GetValues<HumidityLevel>().Select(value => new EnumOption<HumidityLevel>(value, value.ToDisplay())).ToList();
        WeightImportanceOptions = Enum.GetValues<WeightImportance>().Select(value => new EnumOption<WeightImportance>(value, value.ToDisplay())).ToList();
        CookingPossibilityOptions = Enum.GetValues<CookingPossibility>().Select(value => new EnumOption<CookingPossibility>(value, value.ToDisplay())).ToList();
        ResupplyFrequencyOptions = Enum.GetValues<ResupplyFrequency>().Select(value => new EnumOption<ResupplyFrequency>(value, value.ToDisplay())).ToList();
        CompetitionNutritionFocusOptions = Enum.GetValues<CompetitionNutritionFocus>().Select(value => new EnumOption<CompetitionNutritionFocus>(value, value.ToDisplay())).ToList();

        SelectedLanguageOption = LanguageOptions.First(option =>
            option.CultureName.Equals(localizationService.CurrentCulture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase));

        SelectedActivityTypeOption = FindOption(ActivityTypeOptions, selectedActivityType);
        SelectedTemperatureRangeOption = FindOption(TemperatureRangeOptions, selectedTemperatureRange);
        SelectedWaterAvailabilityOption = FindOption(WaterAvailabilityOptions, selectedWaterAvailability);
        SelectedAltitudeRangeOption = FindOption(AltitudeRangeOptions, selectedAltitudeRange);
        SelectedHumidityLevelOption = FindOption(HumidityLevelOptions, selectedHumidityLevel);
        SelectedWeightImportanceOption = FindOption(WeightImportanceOptions, selectedWeightImportance);
        SelectedCookingPossibilityOption = FindOption(CookingPossibilityOptions, selectedCookingPossibility);
        SelectedResupplyFrequencyOption = FindOption(ResupplyFrequencyOptions, selectedResupplyFrequency);
        SelectedCompetitionNutritionFocusOption = selectedCompetitionFocus is null
            ? null
            : FindOption(CompetitionNutritionFocusOptions, selectedCompetitionFocus.Value);

        isRefreshingLocalizationState = false;

        RaiseLocalizedPropertiesChanged();
        RebuildProjectList();
        RenderStatusMessage();
        OnPropertyChanged(nameof(IsCompetitionSelected));
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        RefreshLocalizedState();
    }

    private void SetStatus(string key, params object[] args)
    {
        statusMessageKey = key;
        statusMessageArgs = args;
        RenderStatusMessage();
    }

    private void RenderStatusMessage()
    {
        StatusMessage = statusMessageArgs.Length == 0
            ? localizationService.Get(statusMessageKey)
            : localizationService.Format(statusMessageKey, statusMessageArgs);
    }

    private void RaiseLocalizedPropertiesChanged()
    {
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(LanguageLabel));
        OnPropertyChanged(nameof(NewProjectTitle));
        OnPropertyChanged(nameof(NewProjectSubtitle));
        OnPropertyChanged(nameof(NameLabel));
        OnPropertyChanged(nameof(NameWatermark));
        OnPropertyChanged(nameof(StartDateLabel));
        OnPropertyChanged(nameof(DurationDaysLabel));
        OnPropertyChanged(nameof(ParticipantsLabel));
        OnPropertyChanged(nameof(ActivityTypeLabel));
        OnPropertyChanged(nameof(EnvironmentSectionTitle));
        OnPropertyChanged(nameof(TemperatureLabel));
        OnPropertyChanged(nameof(WaterAvailabilityLabel));
        OnPropertyChanged(nameof(AltitudeLabel));
        OnPropertyChanged(nameof(HumidityLabel));
        OnPropertyChanged(nameof(LogisticsSectionTitle));
        OnPropertyChanged(nameof(WeightImportanceLabel));
        OnPropertyChanged(nameof(CookingPossibilityLabel));
        OnPropertyChanged(nameof(ResupplyFrequencyLabel));
        OnPropertyChanged(nameof(CompetitionNutritionFocusTitle));
        OnPropertyChanged(nameof(CreateButtonText));
        OnPropertyChanged(nameof(SavedProjectsTitle));
        OnPropertyChanged(nameof(SavedProjectsSubtitle));
    }

    private static EnumOption<T>? FindOption<T>(IEnumerable<EnumOption<T>> options, T value)
        where T : struct, Enum
    {
        return options.FirstOrDefault(option => EqualityComparer<T>.Default.Equals(option.Value, value));
    }
}
