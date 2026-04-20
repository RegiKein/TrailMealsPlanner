using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.Interfaces;

public interface IDayTemplateRepository
{
    Task AddAsync(DayTemplate template, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DayTemplate>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<DayTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
