using TrailMealsPlanner.Domain.ValueObjects;

namespace TrailMealsPlanner.Domain.Entities;

public sealed class Product
{
    public Product(
        string name,
        decimal caloriesPer100g,
        decimal protein,
        decimal fat,
        decimal carbs)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (caloriesPer100g < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(caloriesPer100g), "Calories cannot be negative.");
        }

        if (protein < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(protein), "Protein cannot be negative.");
        }

        if (fat < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fat), "Fat cannot be negative.");
        }

        if (carbs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(carbs), "Carbs cannot be negative.");
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        CaloriesPer100g = caloriesPer100g;
        Protein = protein;
        Fat = fat;
        Carbs = carbs;
    }

    private Product(
        Guid id,
        string name,
        decimal caloriesPer100g,
        decimal protein,
        decimal fat,
        decimal carbs)
        : this(name, caloriesPer100g, protein, fat, carbs)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(id));
        }

        Id = id;
    }

    public Guid Id { get; }

    public string Name { get; }

    public decimal CaloriesPer100g { get; }

    public decimal Protein { get; }

    public decimal Fat { get; }

    public decimal Carbs { get; }

    public static Product Restore(
        Guid id,
        string name,
        decimal caloriesPer100g,
        decimal protein,
        decimal fat,
        decimal carbs)
    {
        return new Product(id, name, caloriesPer100g, protein, fat, carbs);
    }

    public NutritionInfo CalculateNutrition(decimal weight)
    {
        if (weight < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight cannot be negative.");
        }

        var ratio = weight / 100m;

        return new NutritionInfo(
            CaloriesPer100g * ratio,
            weight,
            Protein * ratio,
            Fat * ratio,
            Carbs * ratio);
    }
}
