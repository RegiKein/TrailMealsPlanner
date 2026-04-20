namespace TrailMealsPlanner.Application.DTO;

public sealed class DishIngredientDto
{
    public Guid ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public decimal Weight { get; init; }
}
