namespace TrailMealsPlanner.Domain.Entities;

public sealed class RationDay
{
    public RationDay(Guid rationProjectId, DateTime date, int dayNumber)
    {
        if (dayNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dayNumber), "Day number must be greater than zero.");
        }

        Id = Guid.NewGuid();
        RationProjectId = rationProjectId;
        Date = date.Date;
        DayNumber = dayNumber;
    }

    public Guid Id { get; }

    public DateTime Date { get; }

    public int DayNumber { get; }

    public Guid RationProjectId { get; }
}
