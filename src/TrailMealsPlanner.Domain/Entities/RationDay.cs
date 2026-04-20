using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.ValueObjects;

namespace TrailMealsPlanner.Domain.Entities;

public sealed class RationDay
{
    private readonly List<Meal> meals = [];

    public RationDay(Guid rationProjectId, DateTime date, int dayNumber)
    {
        if (dayNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dayNumber), "Day number must be greater than zero.");
        }

        Id = Guid.NewGuid();
        RationProjectId = rationProjectId;
        Date = date.Date;
        DayNumber = dayNumber;
    }

    public Guid Id { get; }

    public DateTime Date { get; }

    public int DayNumber { get; }

    public Guid RationProjectId { get; }

    public IReadOnlyList<Meal> Meals => meals;

    public void AddDishToMeal(Guid mealId, Guid dishId, decimal quantity)
    {
        var meal = meals.FirstOrDefault(item => item.Id == mealId);
        if (meal is null)
        {
            throw new InvalidOperationException($"Meal '{mealId}' was not found in ration day '{Id}'.");
        }

        meal.AddDish(dishId, quantity);
    }

    public void InitializeDefaultMeals()
    {
        meals.Clear();
        meals.Add(new Meal(MealType.Breakfast, Id));
        meals.Add(new Meal(MealType.Lunch, Id));
        meals.Add(new Meal(MealType.Dinner, Id));
        meals.Add(new Meal(MealType.Snack, Id));
    }

    public NutritionInfo CalculateNutrition(
        IReadOnlyDictionary<Guid, Dish> dishesById,
        IReadOnlyDictionary<Guid, Product> productsById)
    {
        return meals.Aggregate(
            NutritionInfo.Zero,
            (current, meal) => current.Add(meal.CalculateNutrition(dishesById, productsById)));
    }
}
