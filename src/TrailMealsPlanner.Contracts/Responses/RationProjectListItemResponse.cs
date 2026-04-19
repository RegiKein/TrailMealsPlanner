using TrailMealsPlanner.Contracts.DTO;

namespace TrailMealsPlanner.Contracts.Responses;

public sealed class RationProjectListItemResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public int DurationDays { get; init; }

    public int ParticipantCount { get; init; }

    public RationProfileContract Profile { get; init; } = new();
}
