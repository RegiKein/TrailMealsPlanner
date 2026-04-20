using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Desktop.ViewModels;

public partial class MealViewModel : ViewModelBase
{
    private readonly Func<Guid, decimal, Task> addDishToMeal;

    [ObservableProperty]
    private IReadOnlyList<DishOptionViewModel> availableDishes = [];

    [ObservableProperty]
    private DishOptionViewModel? selectedDish;

    [ObservableProperty]
    private decimal quantity = 1;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private NutritionSummaryViewModel nutrition = new(new NutritionInfoDto());

    public MealViewModel(
        MealDto meal,
        MealAnalyticsDto? analytics,
        IReadOnlyList<DishOptionViewModel> availableDishes,
        Func<Guid, decimal, Task> addDishToMeal)
    {
        Id = meal.Id;
        Type = meal.Type;
        this.addDishToMeal = addDishToMeal;
        Nutrition = analytics is null
            ? new NutritionSummaryViewModel(new NutritionInfoDto())
            : new NutritionSummaryViewModel(analytics.Nutrition);

        foreach (var item in meal.Items)
        {
            Items.Add(new MealItemViewModel(item));
        }

        UpdateDishOptions(availableDishes);
    }

    public Guid Id { get; }

    public MealType Type { get; }

    public ObservableCollection<MealItemViewModel> Items { get; } = [];

    public string Display => Type switch
    {
        MealType.Breakfast => "Завтрак",
        MealType.Lunch => "Обед",
        MealType.Dinner => "Ужин",
        MealType.Snack => "Перекус",
        MealType.Emergency => "Аварийный запас",
        _ => Type.ToString()
    };

    public void UpdateDishOptions(IReadOnlyList<DishOptionViewModel> dishes)
    {
        AvailableDishes = dishes;
        SelectedDish = SelectedDish is not null
            ? dishes.FirstOrDefault(dish => dish.Id == SelectedDish.Id) ?? dishes.FirstOrDefault()
            : dishes.FirstOrDefault();
    }

    [RelayCommand]
    private async Task AddDish()
    {
        if (SelectedDish is null)
        {
            StatusMessage = "Сначала создайте хотя бы одно блюдо.";
            return;
        }

        if (Quantity <= 0)
        {
            StatusMessage = "Количество должно быть больше нуля.";
            return;
        }

        await addDishToMeal(SelectedDish.Id, Quantity);
        StatusMessage = $"Блюдо \"{SelectedDish.Name}\" добавлено.";
        Quantity = 1;
    }
}
