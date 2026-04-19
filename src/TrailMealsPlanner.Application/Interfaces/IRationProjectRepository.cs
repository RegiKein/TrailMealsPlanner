using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.Interfaces;

public interface IRationProjectRepository
{
    Task AddAsync(RationProject rationProject, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RationProject>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<RationProject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
