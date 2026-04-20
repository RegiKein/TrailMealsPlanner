namespace TrailMealsPlanner.Application.DTO;

public sealed class RationWarningsDto
{
    public Guid RationId { get; init; }

    public IReadOnlyList<WarningDto> RationWarnings { get; init; } = [];

    public IReadOnlyList<RationDayWarningsDto> Days { get; init; } = [];
}
