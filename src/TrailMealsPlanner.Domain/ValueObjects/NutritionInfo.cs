namespace TrailMealsPlanner.Domain.ValueObjects;

public sealed class NutritionInfo
{
    public static NutritionInfo Zero { get; } = new(0, 0, 0, 0, 0);

    public NutritionInfo(decimal calories, decimal weight, decimal protein, decimal fat, decimal carbs)
    {
        if (calories < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(calories), "Calories cannot be negative.");
        }

        if (weight < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight cannot be negative.");
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

        Calories = calories;
        Weight = weight;
        Protein = protein;
        Fat = fat;
        Carbs = carbs;
    }

    public decimal Calories { get; }

    public decimal Weight { get; }

    public decimal Protein { get; }

    public decimal Fat { get; }

    public decimal Carbs { get; }

    public decimal CaloriesPerGram => Weight == 0 ? 0 : Calories / Weight;

    public NutritionInfo Add(NutritionInfo other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return new NutritionInfo(
            Calories + other.Calories,
            Weight + other.Weight,
            Protein + other.Protein,
            Fat + other.Fat,
            Carbs + other.Carbs);
    }
}
