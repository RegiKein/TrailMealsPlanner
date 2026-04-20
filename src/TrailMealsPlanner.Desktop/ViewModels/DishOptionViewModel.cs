using System;
using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class DishOptionViewModel
{
    public DishOptionViewModel(DishListItemDto dish)
    {
        Id = dish.Id;
        Name = dish.Name;
        Summary = dish.Ingredients.Count == 0
            ? $"{dish.Calories:0.#} ккал"
            : $"{dish.Calories:0.#} ккал, {dish.Ingredients.Count} ингредиентов";
    }

    public Guid Id { get; }

    public string Name { get; }

    public string Summary { get; }

    public override string ToString()
    {
        return Name;
    }
}
