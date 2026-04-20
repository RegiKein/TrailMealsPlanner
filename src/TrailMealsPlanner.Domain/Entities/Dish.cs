using TrailMealsPlanner.Domain.ValueObjects;

namespace TrailMealsPlanner.Domain.Entities;

public sealed class Dish
{
    private readonly List<DishIngredient> ingredients = [];

    public Dish(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Dish name is required.", nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
    }

    public Guid Id { get; }

    public string Name { get; }

    public IReadOnlyList<DishIngredient> Ingredients => ingredients;

    public void AddIngredient(Guid productId, decimal weight)
    {
        var existingIngredient = ingredients.FirstOrDefault(ingredient => ingredient.ProductId == productId);
        if (existingIngredient is not null)
        {
            existingIngredient.IncreaseWeight(weight);
            return;
        }

        ingredients.Add(new DishIngredient(productId, weight));
    }

    public NutritionInfo CalculateNutrition(IReadOnlyDictionary<Guid, Product> productsById)
    {
        var nutrition = NutritionInfo.Zero;

        foreach (var ingredient in ingredients)
        {
            if (!productsById.TryGetValue(ingredient.ProductId, out var product))
            {
                throw new InvalidOperationException($"Product '{ingredient.ProductId}' was not provided for nutrition calculation.");
            }

            nutrition = nutrition.Add(product.CalculateNutrition(ingredient.Weight));
        }

        return nutrition;
    }
}
