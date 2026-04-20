namespace TrailMealsPlanner.Application.Interfaces;

public interface IRationProjectFileService
{
    Task<string> ExportAsync(Guid rationId, string filePath, CancellationToken cancellationToken = default);

    Task<Guid> ImportAsync(string filePath, CancellationToken cancellationToken = default);
}
