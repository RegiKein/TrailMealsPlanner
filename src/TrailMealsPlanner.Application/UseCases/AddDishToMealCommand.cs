namespace TrailMealsPlanner.Application.UseCases;

public sealed class AddDishToMealCommand
{
    public Guid RationId { get; init; }

    public Guid MealId { get; init; }

    public Guid DishId { get; init; }

    public decimal Quantity { get; init; } = 1;
}
