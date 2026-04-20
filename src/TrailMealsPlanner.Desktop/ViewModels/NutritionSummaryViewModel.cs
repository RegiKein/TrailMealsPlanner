using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class NutritionSummaryViewModel
{
    public NutritionSummaryViewModel(NutritionInfoDto nutrition)
    {
        Calories = nutrition.Calories;
        Weight = nutrition.Weight;
        CaloriesPerGram = nutrition.CaloriesPerGram;
    }

    public decimal Calories { get; }

    public decimal Weight { get; }

    public decimal CaloriesPerGram { get; }

    public string CaloriesDisplay => $"{Calories:0.#} ккал";

    public string WeightDisplay => $"{Weight:0.#} г";

    public string DensityDisplay => $"{CaloriesPerGram:0.##} ккал/г";
}
