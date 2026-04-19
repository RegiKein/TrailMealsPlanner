using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Domain.Entities;

public sealed class RationProject
{
    private readonly List<RationDay> days = [];

    public RationProject(
        string name,
        DateTime startDate,
        int durationDays,
        int participantCount,
        TourismType tourismType,
        Season season)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Ration project name is required.", nameof(name));
        }

        if (durationDays <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationDays), "Duration must be greater than zero.");
        }

        if (participantCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(participantCount), "Participant count must be greater than zero.");
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        StartDate = startDate.Date;
        DurationDays = durationDays;
        ParticipantCount = participantCount;
        TourismType = tourismType;
        Season = season;

        BuildDays();
    }

    public Guid Id { get; }

    public string Name { get; }

    public DateTime StartDate { get; }

    public DateTime EndDate => StartDate.AddDays(DurationDays - 1);

    public int DurationDays { get; }

    public int ParticipantCount { get; }

    public TourismType TourismType { get; }

    public Season Season { get; }

    public IReadOnlyList<RationDay> Days => days;

    private void BuildDays()
    {
        for (var dayNumber = 1; dayNumber <= DurationDays; dayNumber++)
        {
            days.Add(new RationDay(Id, StartDate.AddDays(dayNumber - 1), dayNumber));
        }
    }
}
