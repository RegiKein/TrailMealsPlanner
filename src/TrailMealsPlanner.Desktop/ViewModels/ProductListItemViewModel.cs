using System;
using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class ProductListItemViewModel
{
    public ProductListItemViewModel(ProductListItemDto product)
    {
        Id = product.Id;
        Name = product.Name;
        CaloriesPer100g = product.CaloriesPer100g;
        Protein = product.Protein;
        Fat = product.Fat;
        Carbs = product.Carbs;
    }

    public Guid Id { get; }

    public string Name { get; }

    public decimal CaloriesPer100g { get; }

    public decimal Protein { get; }

    public decimal Fat { get; }

    public decimal Carbs { get; }

    public string Summary => $"{CaloriesPer100g:0.#} ккал | Б {Protein:0.#} | Ж {Fat:0.#} | У {Carbs:0.#}";

    public override string ToString()
    {
        return Name;
    }
}
