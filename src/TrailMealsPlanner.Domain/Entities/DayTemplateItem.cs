namespace TrailMealsPlanner.Domain.Entities;

public sealed class DayTemplateItem
{
    public DayTemplateItem(Guid? dishId, Guid? productId, decimal quantity)
    {
        if (dishId is null && productId is null)
        {
            throw new ArgumentException("Template item must reference a dish or a product.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        Id = Guid.NewGuid();
        DishId = dishId;
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid Id { get; }

    public Guid? DishId { get; }

    public Guid? ProductId { get; }

    public decimal Quantity { get; }

    internal static DayTemplateItem FromMealItem(MealItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return new DayTemplateItem(item.DishId, item.ProductId, item.Quantity);
    }

    internal MealItem ToMealItem()
    {
        return new MealItem(DishId, ProductId, Quantity);
    }
}
