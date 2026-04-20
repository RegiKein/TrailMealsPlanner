using System;
using System.Collections.ObjectModel;
using System.Linq;
using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class DishListItemViewModel
{
    public DishListItemViewModel(DishListItemDto dish)
    {
        Id = dish.Id;
        Name = dish.Name;
        Calories = dish.Calories;
        Protein = dish.Protein;
        Fat = dish.Fat;
        Carbs = dish.Carbs;
        Ingredients = new ObservableCollection<DishIngredientListItemViewModel>(
            dish.Ingredients.Select(ingredient => new DishIngredientListItemViewModel(ingredient)));
    }

    public Guid Id { get; }

    public string Name { get; }

    public decimal Calories { get; }

    public decimal Protein { get; }

    public decimal Fat { get; }

    public decimal Carbs { get; }

    public ObservableCollection<DishIngredientListItemViewModel> Ingredients { get; }

    public string Summary => $"{Calories:0.#} ккал | Б {Protein:0.#} | Ж {Fat:0.#} | У {Carbs:0.#}";
}
