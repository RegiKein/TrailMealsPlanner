namespace TrailMealsPlanner.Application.UseCases;

public sealed class CreateProductCommand
{
    public string Name { get; set; } = string.Empty;

    public decimal CaloriesPer100g { get; set; }

    public decimal Protein { get; set; }

    public decimal Fat { get; set; }

    public decimal Carbs { get; set; }
}
