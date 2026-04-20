using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class ImportRationProjectHandler
{
    private readonly IRationProjectFileService rationProjectFileService;

    public ImportRationProjectHandler(IRationProjectFileService rationProjectFileService)
    {
        this.rationProjectFileService = rationProjectFileService;
    }

    public async Task<Guid> Handle(ImportRationProjectCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.FilePath))
        {
            throw new ArgumentException("File path is required.", nameof(command));
        }

        return await rationProjectFileService.ImportAsync(command.FilePath, cancellationToken);
    }
}
