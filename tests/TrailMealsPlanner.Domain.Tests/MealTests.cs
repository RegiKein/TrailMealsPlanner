using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Domain.Tests;

public sealed class MealTests
{
    [Fact]
    public void AddDish_AddsOrMergesMealItems()
    {
        var meal = new Meal(MealType.Dinner, Guid.NewGuid());
        var dishId = Guid.NewGuid();

        meal.AddDish(dishId, 1);
        meal.AddDish(dishId, 2);

        var item = Assert.Single(meal.Items);
        Assert.Equal(dishId, item.DishId);
        Assert.Equal(3, item.Quantity);
    }
}
