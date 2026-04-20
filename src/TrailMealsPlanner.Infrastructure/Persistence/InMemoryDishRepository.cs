using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Infrastructure.Persistence;

public sealed class InMemoryDishRepository : IDishRepository
{
    private readonly List<Dish> dishes = [];

    public Task AddAsync(Dish dish, CancellationToken cancellationToken = default)
    {
        dishes.Add(dish);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Dish>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Dish>>(dishes.ToList());
    }
}
