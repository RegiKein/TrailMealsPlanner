using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class RationDayViewModel
{
    public RationDayViewModel(
        RationDayDto day,
        RationDayAnalyticsDto? analytics,
        IReadOnlyList<DishOptionViewModel> availableDishes,
        Func<Guid, Guid, decimal, Task> addDishToMeal)
    {
        Id = day.Id;
        DayNumber = day.DayNumber;
        Date = day.Date;
        Nutrition = analytics is null
            ? new NutritionSummaryViewModel(new NutritionInfoDto())
            : new NutritionSummaryViewModel(analytics.Nutrition);

        foreach (var meal in day.Meals)
        {
            var mealAnalytics = analytics?.Meals.FirstOrDefault(item => item.MealId == meal.Id);
            Meals.Add(new MealViewModel(
                meal,
                mealAnalytics,
                availableDishes,
                (dishId, quantity) => addDishToMeal(meal.Id, dishId, quantity)));
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
}
