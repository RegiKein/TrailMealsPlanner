using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class MealItemViewModel
{
    public MealItemViewModel(MealItemDto item)
    {
        Name = item.Name;
        Quantity = item.Quantity;
        FoodIssueSummary = item.FoodIssue?.Summary;
    }

    public string Name { get; }

    public decimal Quantity { get; }

    public string? FoodIssueSummary { get; }

    public bool HasFoodIssue => !string.IsNullOrWhiteSpace(FoodIssueSummary);

    public string Display => HasFoodIssue
        ? $"{Name} x {Quantity:0.##} [{FoodIssueSummary}]"
        : $"{Name} x {Quantity:0.##}";
}
