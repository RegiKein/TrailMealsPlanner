namespace TrailMealsPlanner.Application.DTO;

public sealed class MealItemDto
{
    public Guid? DishId { get; init; }

    public Guid? ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public FoodIssueDto? FoodIssue { get; init; }
}
