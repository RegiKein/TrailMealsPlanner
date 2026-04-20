using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public partial class RationDayViewModel : ViewModelBase
{
    private readonly Func<Guid, Guid, decimal, Task> addDishToMeal;
    private readonly Func<Guid, Guid, Task> applyTemplate;
    private readonly Func<Guid, Task> copyDay;
    private readonly IReadOnlyList<MealCopyTargetViewModel> availableMealCopyTargets;
    private readonly Func<Guid, string, Task> saveDayAsTemplate;

    [ObservableProperty]
    private IReadOnlyList<DayCopyTargetViewModel> availableCopyTargets = [];

    [ObservableProperty]
    private DayCopyTargetViewModel? selectedCopyTarget;

    [ObservableProperty]
    private string copyStatusMessage = string.Empty;

    [ObservableProperty]
    private string templateName = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<DayTemplateOptionViewModel> availableTemplates = [];

    [ObservableProperty]
    private DayTemplateOptionViewModel? selectedTemplate;

    [ObservableProperty]
    private string templateStatusMessage = string.Empty;

    [ObservableProperty]
    private bool confirmTemplateApply;

    [ObservableProperty]
    private IReadOnlyList<WarningItemViewModel> warnings = [];

    public RationDayViewModel(
        RationDayDto day,
        RationDayAnalyticsDto? analytics,
        IReadOnlyList<DishOptionViewModel> availableDishes,
        IReadOnlyList<MealCopyTargetViewModel> availableMealCopyTargets,
        Func<Guid, Guid, decimal, Task> addDishToMeal,
        Func<Guid, Task> copyDay,
        Func<Guid, Guid, Task> copyMeal,
        Func<Guid, string, Task> saveDayAsTemplate,
        Func<Guid, Guid, Task> applyTemplate)
    {
        Id = day.Id;
        DayNumber = day.DayNumber;
        Date = day.Date;
        TemplateName = $"День {day.DayNumber}";
        Nutrition = analytics is null
            ? new NutritionSummaryViewModel(new NutritionInfoDto())
            : new NutritionSummaryViewModel(analytics.Nutrition);
        this.availableMealCopyTargets = availableMealCopyTargets;
        this.addDishToMeal = addDishToMeal;
        this.copyDay = copyDay;
        this.saveDayAsTemplate = saveDayAsTemplate;
        this.applyTemplate = applyTemplate;

        foreach (var meal in day.Meals)
        {
            var mealAnalytics = analytics?.Meals.FirstOrDefault(item => item.MealId == meal.Id);
            var mealViewModel = new MealViewModel(
                meal,
                mealAnalytics,
                availableDishes,
                (dishId, quantity) => this.addDishToMeal(meal.Id, dishId, quantity),
                targetMealId => copyMeal(meal.Id, targetMealId));
            mealViewModel.UpdateCopyTargets(availableMealCopyTargets);
            Meals.Add(mealViewModel);
        }
    }

    public Guid Id { get; }

    public int DayNumber { get; }

    public DateTime Date { get; }

    public NutritionSummaryViewModel Nutrition { get; }

    public ObservableCollection<MealViewModel> Meals { get; } = [];

    public string Display => $"День {DayNumber} - {Date:dd.MM.yyyy}";

    public string ApplyTemplateButtonText => ConfirmTemplateApply ? "Подтвердить замену" : "Применить шаблон";

    public bool HasWarnings => Warnings.Count > 0;

    public string WarningBadgeText => Warnings.Count == 0
        ? string.Empty
        : $"{GetHighestSeverityLabel()} · {Warnings.Count}";

    public void UpdateDishOptions(IReadOnlyList<DishOptionViewModel> dishes)
    {
        foreach (var meal in Meals)
        {
            meal.UpdateDishOptions(dishes);
        }
    }

    public void UpdateCopyTargets(IReadOnlyList<DayCopyTargetViewModel> copyTargets)
    {
        AvailableCopyTargets = copyTargets
            .Where(target => target.Id != Id)
            .ToList();

        SelectedCopyTarget = SelectedCopyTarget is not null
            ? AvailableCopyTargets.FirstOrDefault(target => target.Id == SelectedCopyTarget.Id) ?? AvailableCopyTargets.FirstOrDefault()
            : AvailableCopyTargets.FirstOrDefault();
    }

    public void UpdateMealCopyTargets()
    {
        foreach (var meal in Meals)
        {
            meal.UpdateCopyTargets(availableMealCopyTargets);
        }
    }

    public void UpdateTemplateOptions(IReadOnlyList<DayTemplateOptionViewModel> templates)
    {
        AvailableTemplates = templates.ToList();
        SelectedTemplate = SelectedTemplate is not null
            ? AvailableTemplates.FirstOrDefault(template => template.Id == SelectedTemplate.Id) ?? AvailableTemplates.FirstOrDefault()
            : AvailableTemplates.FirstOrDefault();
        ConfirmTemplateApply = false;
    }

    public void UpdateWarnings(IReadOnlyList<WarningItemViewModel> warnings)
    {
        Warnings = warnings
            .OrderByDescending(warning => warning.Severity)
            .ThenBy(warning => warning.Message)
            .ToList();
        OnPropertyChanged(nameof(HasWarnings));
        OnPropertyChanged(nameof(WarningBadgeText));
    }

    partial void OnConfirmTemplateApplyChanged(bool value)
    {
        OnPropertyChanged(nameof(ApplyTemplateButtonText));
    }

    partial void OnSelectedTemplateChanged(DayTemplateOptionViewModel? value)
    {
        ConfirmTemplateApply = false;
        TemplateStatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task CopyDay()
    {
        if (SelectedCopyTarget is null)
        {
            CopyStatusMessage = "Выберите целевой день.";
            return;
        }

        await copyDay(SelectedCopyTarget.Id);
        CopyStatusMessage = $"День скопирован в {SelectedCopyTarget.Display}.";
    }

    [RelayCommand]
    private async Task SaveAsTemplate()
    {
        if (string.IsNullOrWhiteSpace(TemplateName))
        {
            TemplateStatusMessage = "Введите название шаблона.";
            return;
        }

        var normalizedName = TemplateName.Trim();
        await saveDayAsTemplate(Id, normalizedName);
        TemplateName = normalizedName;
        TemplateStatusMessage = $"Шаблон \"{normalizedName}\" сохранён.";
    }

    [RelayCommand]
    private async Task ApplyTemplate()
    {
        if (SelectedTemplate is null)
        {
            TemplateStatusMessage = "Выберите шаблон.";
            return;
        }

        if (!ConfirmTemplateApply)
        {
            ConfirmTemplateApply = true;
            TemplateStatusMessage = $"Шаблон \"{SelectedTemplate.Name}\" заменит текущее содержимое дня. Нажмите ещё раз для подтверждения.";
            return;
        }

        await applyTemplate(SelectedTemplate.Id, Id);
        ConfirmTemplateApply = false;
        TemplateStatusMessage = $"Шаблон \"{SelectedTemplate.Name}\" применён.";
    }

    private string GetHighestSeverityLabel()
    {
        var highestSeverity = Warnings.Max(warning => warning.Severity);
        return highestSeverity switch
        {
            Application.DTO.WarningSeverity.Critical => "Критично",
            Application.DTO.WarningSeverity.Warning => "Внимание",
            _ => "Инфо"
        };
    }
}
