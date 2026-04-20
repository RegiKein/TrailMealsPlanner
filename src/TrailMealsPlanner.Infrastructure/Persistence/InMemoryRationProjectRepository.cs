using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Infrastructure.Persistence;

public sealed class InMemoryRationProjectRepository : IRationProjectRepository
{
    private readonly List<RationProject> rationProjects = [];

    public Task AddAsync(RationProject rationProject, CancellationToken cancellationToken = default)
    {
        var existingIndex = rationProjects.FindIndex(project => project.Id == rationProject.Id);
        if (existingIndex >= 0)
        {
            rationProjects[existingIndex] = rationProject;
        }
        else
        {
            rationProjects.Add(rationProject);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<RationProject>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<RationProject>>(rationProjects.ToList());
    }

    public Task<RationProject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(rationProjects.FirstOrDefault(project => project.Id == id));
    }
}
