using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.Interfaces;

public interface IDishRepository
{
    Task AddAsync(Dish dish, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Dish>> GetAllAsync(CancellationToken cancellationToken = default);
}
