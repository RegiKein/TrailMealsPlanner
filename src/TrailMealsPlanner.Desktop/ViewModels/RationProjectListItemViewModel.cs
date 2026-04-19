using System;
using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Desktop.Extensions;
using TrailMealsPlanner.Desktop.Services;
using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class RationProjectListItemViewModel
{
    public RationProjectListItemViewModel(RationProjectListItemDto project, LocalizationService localizationService)
    {
        Id = project.Id;
        Name = project.Name;
        StartDate = project.StartDate;
        EndDate = project.EndDate;
        DurationDays = project.DurationDays;
        ParticipantCount = project.ParticipantCount;
        ActivityTypeValue = project.Profile.ActivityType;
        Summary = $"{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy} | {DurationDays} {localizationService.Get("Unit_DaysShort")} | {ParticipantCount} {localizationService.Get("Unit_PeopleShort")}";
        Environment = string.Join(
            ", ",
            project.Profile.Environment.TemperatureRange.ToDisplay(),
            project.Profile.Environment.WaterAvailability.ToDisplay(),
            project.Profile.Environment.AltitudeRange.ToDisplay(),
            project.Profile.Environment.HumidityLevel.ToDisplay());
        Logistics = string.Join(
            ", ",
            project.Profile.Logistics.WeightImportance.ToDisplay(),
            project.Profile.Logistics.CookingPossibility.ToDisplay(),
            project.Profile.Logistics.ResupplyFrequency.ToDisplay());
        CompetitionFocus = project.Profile.CompetitionFocus?.ToDisplay();
        Metadata = CompetitionFocus is null
            ? $"{Environment} | {Logistics}"
            : $"{Environment} | {Logistics} | {CompetitionFocus}";
    }

    public Guid Id { get; }

    public string Name { get; }

    public DateTime StartDate { get; }

    public DateTime EndDate { get; }

    public int DurationDays { get; }

    public int ParticipantCount { get; }

    public ActivityType ActivityTypeValue { get; }

    public string Summary { get; }

    public string Environment { get; }

    public string Logistics { get; }

    public string? CompetitionFocus { get; }

    public string Metadata { get; }
}
