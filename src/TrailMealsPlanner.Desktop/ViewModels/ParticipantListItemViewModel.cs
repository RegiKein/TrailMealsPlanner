using System;
using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class ParticipantListItemViewModel
{
    public ParticipantListItemViewModel(ParticipantDto participant)
    {
        Id = participant.Id;
        Name = participant.Name;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string Display => Name;
}
