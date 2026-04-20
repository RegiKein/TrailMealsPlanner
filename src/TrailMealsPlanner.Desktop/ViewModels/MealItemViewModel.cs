using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class MealItemViewModel
{
    public MealItemViewModel(MealItemDto item)
    {
        Name = item.Name;
        Quantity = item.Quantity;
    }

    public string Name { get; }

    public decimal Quantity { get; }

    public string Display => $"{Name} x {Quantity:0.##}";
}
