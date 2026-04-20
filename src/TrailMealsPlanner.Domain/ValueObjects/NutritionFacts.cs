namespace TrailMealsPlanner.Domain.ValueObjects;

public sealed class NutritionFacts
{
    public NutritionFacts(decimal calories, decimal protein, decimal fat, decimal carbs)
    {
        Calories = calories;
        Protein = protein;
        Fat = fat;
        Carbs = carbs;
    }

    public decimal Calories { get; }

    public decimal Protein { get; }

    public decimal Fat { get; }

    public decimal Carbs { get; }
}
