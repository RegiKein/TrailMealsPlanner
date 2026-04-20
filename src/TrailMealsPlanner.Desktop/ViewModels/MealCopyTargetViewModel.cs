using System;
using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class MealCopyTargetViewModel
{
    public MealCopyTargetViewModel(Guid id, int dayNumber, DateTime date, MealType mealType)
    {
        Id = id;
        DayNumber = dayNumber;
        Date = date;
        MealType = mealType;
    }

    public Guid Id { get; }

    public int DayNumber { get; }

    public DateTime Date { get; }

    public MealType MealType { get; }

    public string Display => $"День {DayNumber} - {Date:dd.MM.yyyy} - {ToMealDisplay(MealType)}";

    private static string ToMealDisplay(MealType mealType)
    {
        return mealType switch
        {
            MealType.Breakfast => "Завтрак",
            MealType.Lunch => "Обед",
            MealType.Dinner => "Ужин",
            MealType.Snack => "Перекус",
            MealType.Emergency => "Аварийный запас",
            _ => mealType.ToString()
        };
    }
}
