namespace TrailMealsPlanner.Application.DTO;

public sealed class RationDetailsDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public RationProfileDto Profile { get; init; } = new();

    public IReadOnlyList<RationDayDto> Days { get; init; } = [];
}
