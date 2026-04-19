using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.DTO;

public sealed class RationProjectListItemDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public int DurationDays { get; init; }

    public int ParticipantCount { get; init; }

    public TourismType TourismType { get; init; }

    public Season Season { get; init; }
}
