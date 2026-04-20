namespace TrailMealsPlanner.Domain.Entities;

/// <summary>
/// Reusable meal structure for a day. The template is date-independent and can be applied to any ration day.
/// </summary>
public sealed class DayTemplate
{
    private readonly List<DayTemplateMeal> meals = [];

    public DayTemplate(string name, IEnumerable<DayTemplateMeal> meals, DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Template name is required.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(meals);

        Id = Guid.NewGuid();
        Name = name.Trim();
        CreatedAt = (createdAt ?? DateTime.UtcNow).ToUniversalTime();
        this.meals.AddRange(meals.Select(meal => meal ?? throw new ArgumentNullException(nameof(meals), "Template meal cannot be null.")));
    }

    public Guid Id { get; }

    public string Name { get; }

    public DateTime CreatedAt { get; }

    public IReadOnlyList<DayTemplateMeal> Meals => meals;

    public static DayTemplate CreateTemplateFromDay(RationDay day, string name)
    {
        ArgumentNullException.ThrowIfNull(day);
        return new DayTemplate(name, day.Meals.Select(DayTemplateMeal.FromMeal).ToList());
    }

    public static void ApplyTemplateToDay(DayTemplate template, RationDay targetDay)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(targetDay);
        targetDay.ReplaceContentFromTemplate(template);
    }
}
