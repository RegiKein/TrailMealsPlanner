namespace TrailMealsPlanner.Application.UseCases;

public sealed class ExportRationProjectCommand
{
    public Guid RationId { get; init; }

    public string FilePath { get; init; } = string.Empty;
}
