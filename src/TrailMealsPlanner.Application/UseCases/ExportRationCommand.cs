namespace TrailMealsPlanner.Application.UseCases;

public sealed class ExportRationCommand
{
    public Guid RationId { get; init; }

    public RationExportFormat Format { get; init; }

    public string DestinationDirectory { get; init; } = string.Empty;
}
