using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Domain.Tests;

public sealed class DishTests
{
    [Fact]
    public void CalculateNutrition_SumsIngredientNutrition()
    {
        var oats = new Product("Овсяные хлопья", 370, 12, 6, 62);
        var raisins = new Product("Изюм", 290, 3, 0.5m, 67);
        var dish = new Dish("Каша");

        dish.AddIngredient(oats.Id, 80);
        dish.AddIngredient(raisins.Id, 20);

        var nutrition = dish.CalculateNutrition(new Dictionary<Guid, Product>
        {
            [oats.Id] = oats,
            [raisins.Id] = raisins
        });

        Assert.Equal(354m, nutrition.Calories);
        Assert.Equal(10.2m, nutrition.Protein);
        Assert.Equal(4.9m, nutrition.Fat);
        Assert.Equal(63m, nutrition.Carbs);
    }
}
