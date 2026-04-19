using TrailMealsPlanner.Domain.ValueObjects;

namespace TrailMealsPlanner.Domain.Entities;

public sealed class RationProject
{
    private readonly List<RationDay> days = [];

    public RationProject(
        string name,
        DateTime startDate,
        int durationDays,
        int participantCount,
        RationProfile profile)
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

        Profile = profile ?? throw new ArgumentNullException(nameof(profile));

        Id = Guid.NewGuid();
        Name = name.Trim();
        StartDate = startDate.Date;
        DurationDays = durationDays;
        ParticipantCount = participantCount;

        BuildDays();
    }

    public Guid Id { get; }

    public string Name { get; }

    public DateTime StartDate { get; }

    public DateTime EndDate => StartDate.AddDays(DurationDays - 1);

    public int DurationDays { get; }

    public int ParticipantCount { get; }

    public RationProfile Profile { get; }

    public IReadOnlyList<RationDay> Days => days;

    private void BuildDays()
    {
        for (var dayNumber = 1; dayNumber <= DurationDays; dayNumber++)
        {
            days.Add(new RationDay(Id, StartDate.AddDays(dayNumber - 1), dayNumber));
        }
    }
}
