using TrailMealsPlanner.Application.UseCases;

namespace TrailMealsPlanner.Application.Interfaces;

public interface IRationExportService
{
    Task<string> ExportAsync(
        Guid rationId,
        RationExportFormat format,
        string destinationDirectory,
        CancellationToken cancellationToken = default);
}
