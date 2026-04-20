using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class SetProductPreferenceCommand
{
    public Guid ParticipantId { get; init; }

    public Guid ProductId { get; init; }

    public PreferenceLevel PreferenceLevel { get; init; }

    public string? Comment { get; init; }
}
