using System;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class DayCopyTargetViewModel
{
    public DayCopyTargetViewModel(Guid id, int dayNumber, DateTime date)
    {
        Id = id;
        DayNumber = dayNumber;
        Date = date;
    }

    public Guid Id { get; }

    public int DayNumber { get; }

    public DateTime Date { get; }

    public string Display => $"День {DayNumber} - {Date:dd.MM.yyyy}";
}
