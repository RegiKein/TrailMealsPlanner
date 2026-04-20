using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;

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

    [Fact]
    public void ReplaceMealContent_DeepCopiesItemsAndKeepsTargetMealType()
    {
        var ration = new RationProject(
            "Altai Trek",
            new DateTime(2026, 8, 1),
            durationDays: 2,
            participantCount: 2,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));
        ration.GenerateDays();

        var sourceMeal = ration.Days[0].Meals[0];
        var targetMeal = ration.Days[1].Meals[2];
        var dishId = Guid.NewGuid();

        ration.AddDishToMeal(sourceMeal.Id, dishId, 2);
        ration.ReplaceMealContent(sourceMeal.Id, targetMeal.Id);

        Assert.Equal(MealType.Dinner, targetMeal.Type);
        var sourceItem = Assert.Single(sourceMeal.Items);
        var targetItem = Assert.Single(targetMeal.Items);
        Assert.Equal(dishId, targetItem.DishId);
        Assert.Equal(2, targetItem.Quantity);
        Assert.NotEqual(sourceItem.Id, targetItem.Id);
    }
}
