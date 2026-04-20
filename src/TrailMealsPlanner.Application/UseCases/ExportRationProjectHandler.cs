using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class ExportRationProjectHandler
{
    private readonly IRationProjectFileService rationProjectFileService;

    public ExportRationProjectHandler(IRationProjectFileService rationProjectFileService)
    {
        this.rationProjectFileService = rationProjectFileService;
    }

    public async Task<string> Handle(ExportRationProjectCommand command, CancellationToken cancellationToken = default)
    {
        if (command.RationId == Guid.Empty)
        {
            throw new ArgumentException("Ration id is required.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.FilePath))
        {
            throw new ArgumentException("File path is required.", nameof(command));
        }

        return await rationProjectFileService.ExportAsync(command.RationId, command.FilePath, cancellationToken);
    }
}
