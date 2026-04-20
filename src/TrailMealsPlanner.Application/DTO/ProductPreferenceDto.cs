using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.DTO;

public sealed class ProductPreferenceDto
{
    public Guid ParticipantId { get; init; }

    public string ParticipantName { get; init; } = string.Empty;

    public Guid ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public PreferenceLevel PreferenceLevel { get; init; }

    public string? Comment { get; init; }
}
