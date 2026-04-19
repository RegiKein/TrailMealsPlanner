using System;
using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class RationProjectListItemViewModel
{
    public RationProjectListItemViewModel(RationProjectListItemDto project)
    {
        Id = project.Id;
        Name = project.Name;
        StartDate = project.StartDate;
        EndDate = project.EndDate;
        DurationDays = project.DurationDays;
        ParticipantCount = project.ParticipantCount;
        TourismType = project.TourismType.ToString();
        Season = project.Season.ToString();
    }

    public Guid Id { get; }

    public string Name { get; }

    public DateTime StartDate { get; }

    public DateTime EndDate { get; }

    public int DurationDays { get; }

    public int ParticipantCount { get; }

    public string TourismType { get; }

    public string Season { get; }

    public string Summary => $"{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy} | {DurationDays} дн. | {ParticipantCount} уч.";

    public string Metadata => $"{TourismType} | {Season}";
}
