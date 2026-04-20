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

    private Meal(Guid id, MealType type, Guid rationDayId, IEnumerable<MealItem> restoredItems)
        : this(type, rationDayId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Meal id is required.", nameof(id));
        }

        Id = id;
        items.Clear();
        items.AddRange(restoredItems ?? throw new ArgumentNullException(nameof(restoredItems)));
    }

    public Guid Id { get; }

    public MealType Type { get; }

    public Guid RationDayId { get; }

    public IReadOnlyList<MealItem> Items => items;

    public static Meal Restore(Guid id, MealType type, Guid rationDayId, IEnumerable<MealItem> items)
    {
        return new Meal(id, type, rationDayId, items);
    }

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

    internal Meal CloneTo(Guid targetRationDayId)
    {
        var clonedMeal = new Meal(Type, targetRationDayId);

        foreach (var item in items)
        {
            clonedMeal.items.Add(item.Clone());
        }

        return clonedMeal;
    }

    internal Meal CloneWithType(Guid targetRationDayId, MealType targetType)
    {
        var clonedMeal = new Meal(targetType, targetRationDayId);

        foreach (var item in items)
        {
            clonedMeal.items.Add(item.Clone());
        }

        return clonedMeal;
    }

    internal void ReplaceContentFrom(Meal sourceMeal)
    {
        ArgumentNullException.ThrowIfNull(sourceMeal);

        ReplaceItems(sourceMeal.items.Select(item => item.Clone()));
    }

    internal void ReplaceItems(IEnumerable<MealItem> newItems)
    {
        ArgumentNullException.ThrowIfNull(newItems);

        items.Clear();

        foreach (var item in newItems)
        {
            items.Add(item ?? throw new ArgumentNullException(nameof(newItems), "Meal item cannot be null."));
        }
    }
}
