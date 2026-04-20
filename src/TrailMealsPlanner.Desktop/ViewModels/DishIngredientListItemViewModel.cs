using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class DishIngredientListItemViewModel
{
    public DishIngredientListItemViewModel(DishIngredientDto ingredient)
    {
        ProductName = ingredient.ProductName;
        Weight = ingredient.Weight;
    }

    public string ProductName { get; }

    public decimal Weight { get; }

    public string Summary => $"{ProductName} - {Weight:0.#} г";
}
