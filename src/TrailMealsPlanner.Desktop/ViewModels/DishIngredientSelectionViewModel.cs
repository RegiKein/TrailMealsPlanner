using System;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class DishIngredientSelectionViewModel
{
    public DishIngredientSelectionViewModel(Guid productId, string productName, decimal weight)
    {
        ProductId = productId;
        ProductName = productName;
        Weight = weight;
    }

    public Guid ProductId { get; }

    public string ProductName { get; }

    public decimal Weight { get; private set; }

    public string Summary => $"{ProductName} - {Weight:0.#} г";

    public void IncreaseWeight(decimal weight)
    {
        Weight += weight;
    }
}
