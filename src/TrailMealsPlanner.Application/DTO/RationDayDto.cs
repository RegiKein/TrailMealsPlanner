namespace TrailMealsPlanner.Application.DTO;

public sealed class RationDayDto
{
    public Guid Id { get; init; }

    public int DayNumber { get; init; }

    public DateTime Date { get; init; }

    public IReadOnlyList<MealDto> Meals { get; init; } = [];
}
