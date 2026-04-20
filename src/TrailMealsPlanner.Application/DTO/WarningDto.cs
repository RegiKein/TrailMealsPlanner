namespace TrailMealsPlanner.Application.DTO;

public sealed class WarningDto
{
    public string Code { get; init; } = string.Empty;

    public WarningSeverity Severity { get; init; }

    public WarningScope Scope { get; init; }

    public string Message { get; init; } = string.Empty;

    public Guid? RelatedEntityId { get; init; }
}
