using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.ValueObjects;

namespace TrailMealsPlanner.Domain.Entities;

public sealed class Meal
{
    private readonly List<MealItem> items = [];

    public Meal(MealType type, Guid rationDayId)
    {
        Id = Guid.NewGuid();
        Type = type;
        RationDayId = rationDayId;
    }

    public Guid Id { get; }

    public MealType Type { get; }

    public Guid RationDayId { get; }

    public IReadOnlyList<MealItem> Items => items;

    public void AddDish(Guid dishId, decimal quantity)
    {
        var existingItem = items.FirstOrDefault(item => item.ReferencesDish(dishId));
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            return;
        }

        items.Add(new MealItem(dishId, null, quantity));
    }

    public NutritionInfo CalculateNutrition(
        IReadOnlyDictionary<Guid, Dish> dishesById,
        IReadOnlyDictionary<Guid, Product> productsById)
    {
        var nutrition = NutritionInfo.Zero;

        foreach (var item in items)
        {
            if (item.DishId is Guid dishId)
            {
                if (!dishesById.TryGetValue(dishId, out var dish))
                {
                    throw new InvalidOperationException($"Dish '{dishId}' was not provided for nutrition calculation.");
                }

                var dishNutrition = dish.CalculateNutrition(productsById);
                nutrition = nutrition.Add(new NutritionInfo(
                    dishNutrition.Calories * item.Quantity,
                    dishNutrition.Weight * item.Quantity,
                    dishNutrition.Protein * item.Quantity,
                    dishNutrition.Fat * item.Quantity,
                    dishNutrition.Carbs * item.Quantity));
            }
        }

        return nutrition;
    }
}
