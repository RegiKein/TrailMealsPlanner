using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Domain.Tests;

public sealed class NutritionInfoTests
{
    [Fact]
    public void ProductCalculateNutrition_ReturnsScaledValues()
    {
        var product = new Product("Орехи", 600, 20, 50, 20);

        var nutrition = product.CalculateNutrition(50);

        Assert.Equal(300, nutrition.Calories);
        Assert.Equal(50, nutrition.Weight);
        Assert.Equal(6, nutrition.CaloriesPerGram);
    }
}
