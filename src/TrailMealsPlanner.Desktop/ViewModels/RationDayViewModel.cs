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
    private readonly Func<Guid, Task> copyDay;
    private readonly IReadOnlyList<MealCopyTargetViewModel> availableMealCopyTargets;

    [ObservableProperty]
    private IReadOnlyList<DayCopyTargetViewModel> availableCopyTargets = [];

    [ObservableProperty]
    private DayCopyTargetViewModel? selectedCopyTarget;

    [ObservableProperty]
    private string copyStatusMessage = string.Empty;

    public RationDayViewModel(
        RationDayDto day,
        RationDayAnalyticsDto? analytics,
        IReadOnlyList<DishOptionViewModel> availableDishes,
        IReadOnlyList<MealCopyTargetViewModel> availableMealCopyTargets,
        Func<Guid, Guid, decimal, Task> addDishToMeal,
        Func<Guid, Task> copyDay,
        Func<Guid, Guid, Task> copyMeal)
    {
        Id = day.Id;
        DayNumber = day.DayNumber;
        Date = day.Date;
        Nutrition = analytics is null
            ? new NutritionSummaryViewModel(new NutritionInfoDto())
            : new NutritionSummaryViewModel(analytics.Nutrition);
        this.availableMealCopyTargets = availableMealCopyTargets;
        this.addDishToMeal = addDishToMeal;
        this.copyDay = copyDay;

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
}
