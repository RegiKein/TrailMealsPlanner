namespace TrailMealsPlanner.Application.DTO;

public sealed class DishListItemDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal Calories { get; init; }

    public decimal Protein { get; init; }

    public decimal Fat { get; init; }

    public decimal Carbs { get; init; }

    public IReadOnlyList<DishIngredientDto> Ingredients { get; init; } = [];
}
