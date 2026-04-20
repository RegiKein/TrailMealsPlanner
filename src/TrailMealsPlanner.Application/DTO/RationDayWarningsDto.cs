namespace TrailMealsPlanner.Application.DTO;

public sealed class RationDayWarningsDto
{
    public Guid DayId { get; init; }

    public int DayNumber { get; init; }

    public IReadOnlyList<WarningDto> Warnings { get; init; } = [];
}
