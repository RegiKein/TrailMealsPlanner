namespace TrailMealsPlanner.Domain.Entities;

public sealed class MealItem
{
    public MealItem(Guid? dishId, Guid? productId, decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        var hasDish = dishId.HasValue && dishId.Value != Guid.Empty;
        var hasProduct = productId.HasValue && productId.Value != Guid.Empty;

        if (hasDish == hasProduct)
        {
            throw new ArgumentException("Meal item must reference either a dish or a product.");
        }

        Id = Guid.NewGuid();
        DishId = hasDish ? dishId : null;
        ProductId = hasProduct ? productId : null;
        Quantity = quantity;
    }

    private MealItem(Guid id, Guid? dishId, Guid? productId, decimal quantity)
        : this(dishId, productId, quantity)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Meal item id is required.", nameof(id));
        }

        Id = id;
    }

    public Guid Id { get; }

    public Guid? DishId { get; }

    public Guid? ProductId { get; }

    public decimal Quantity { get; private set; }

    public static MealItem Restore(Guid id, Guid? dishId, Guid? productId, decimal quantity)
    {
        return new MealItem(id, dishId, productId, quantity);
    }

    public bool ReferencesDish(Guid dishId)
    {
        return DishId == dishId;
    }

    internal void IncreaseQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        Quantity += quantity;
    }

    internal MealItem Clone()
    {
        return new MealItem(DishId, ProductId, Quantity);
    }
}
