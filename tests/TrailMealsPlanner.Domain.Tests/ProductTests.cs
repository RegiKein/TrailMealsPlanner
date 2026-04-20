using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Domain.Tests;

public sealed class ProductTests
{
    [Fact]
    public void Constructor_CreatesProduct()
    {
        var product = new Product("Овсяные хлопья", 370, 12, 6, 62);

        Assert.Equal("Овсяные хлопья", product.Name);
        Assert.Equal(370, product.CaloriesPer100g);
        Assert.Equal(12, product.Protein);
        Assert.Equal(6, product.Fat);
        Assert.Equal(62, product.Carbs);
    }

    [Fact]
    public void Constructor_Throws_WhenNameIsEmpty()
    {
        var action = () => new Product("", 100, 1, 1, 1);

        Assert.Throws<ArgumentException>(action);
    }
}
