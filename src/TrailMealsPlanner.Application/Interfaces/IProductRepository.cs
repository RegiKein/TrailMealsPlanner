using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetByIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);
}
