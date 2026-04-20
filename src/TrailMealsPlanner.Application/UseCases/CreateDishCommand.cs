namespace TrailMealsPlanner.Application.UseCases;

public sealed class CreateDishCommand
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<CreateDishIngredientModel> Ingredients { get; init; } = [];
}

public sealed class CreateDishIngredientModel
{
    public Guid ProductId { get; init; }

    public decimal Weight { get; init; }
}
