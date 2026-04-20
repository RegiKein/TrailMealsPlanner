namespace TrailMealsPlanner.Application.DTO;

public sealed class ProductListItemDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal CaloriesPer100g { get; init; }

    public decimal Protein { get; init; }

    public decimal Fat { get; init; }

    public decimal Carbs { get; init; }
}
