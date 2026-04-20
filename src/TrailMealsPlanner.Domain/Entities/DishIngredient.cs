namespace TrailMealsPlanner.Domain.Entities;

public sealed class DishIngredient
{
    public DishIngredient(Guid productId, decimal weight)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        if (weight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be greater than zero.");
        }

        ProductId = productId;
        Weight = weight;
    }

    public Guid ProductId { get; }

    public decimal Weight { get; private set; }

    internal void IncreaseWeight(decimal weight)
    {
        if (weight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be greater than zero.");
        }

        Weight += weight;
    }
}
