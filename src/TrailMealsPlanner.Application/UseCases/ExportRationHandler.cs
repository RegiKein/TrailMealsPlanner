using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class ExportRationHandler
{
    private readonly IRationExportService exportService;

    public ExportRationHandler(IRationExportService exportService)
    {
        this.exportService = exportService;
    }

    public async Task<string> Handle(ExportRationCommand command, CancellationToken cancellationToken = default)
    {
        if (command.RationId == Guid.Empty)
        {
            throw new ArgumentException("Ration id is required.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.DestinationDirectory))
        {
            throw new ArgumentException("Destination directory is required.", nameof(command));
        }

        return await exportService.ExportAsync(
            command.RationId,
            command.Format,
            command.DestinationDirectory,
            cancellationToken);
    }
}
