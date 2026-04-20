using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Domain.Entities;

public sealed class DayTemplateMeal
{
    private readonly List<DayTemplateItem> items = [];

    public DayTemplateMeal(MealType type, IEnumerable<DayTemplateItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        Id = Guid.NewGuid();
        Type = type;

        foreach (var item in items)
        {
            this.items.Add(item ?? throw new ArgumentNullException(nameof(items), "Template meal item cannot be null."));
        }
    }

    public Guid Id { get; }

    public MealType Type { get; }

    public IReadOnlyList<DayTemplateItem> Items => items;

    internal static DayTemplateMeal FromMeal(Meal meal)
    {
        ArgumentNullException.ThrowIfNull(meal);
        return new DayTemplateMeal(meal.Type, meal.Items.Select(DayTemplateItem.FromMealItem).ToList());
    }

    internal Meal ToMeal(Guid rationDayId)
    {
        var meal = new Meal(Type, rationDayId);
        meal.ReplaceItems(items.Select(item => item.ToMealItem()));
        return meal;
    }
}
