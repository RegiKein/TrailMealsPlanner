using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Infrastructure.Persistence;

public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> products = [];

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        var existingIndex = products.FindIndex(existing => existing.Id == product.Id);
        if (existingIndex >= 0)
        {
            products[existingIndex] = product;
        }
        else
        {
            products.Add(product);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Product>>(products.ToList());
    }

    public Task<IReadOnlyList<Product>> GetByIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        var matchingProducts = products
            .Where(product => productIds.Contains(product.Id))
            .ToList();

        return Task.FromResult<IReadOnlyList<Product>>(matchingProducts);
    }
}
