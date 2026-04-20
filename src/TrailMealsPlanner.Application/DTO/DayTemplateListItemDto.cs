namespace TrailMealsPlanner.Application.DTO;

public sealed class DayTemplateListItemDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public int MealsCount { get; init; }

    public int ItemsCount { get; init; }

    public string Summary { get; init; } = string.Empty;
}
