using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class CreateRationCommand
{
    public string Name { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public int DurationDays { get; set; }

    public int ParticipantCount { get; set; }

    public TourismType TourismType { get; set; }

    public Season Season { get; set; }
}
