using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.Tests;

public sealed class ProductHandlersTests
{
    [Fact]
    public async Task CreateAndGetProducts_Works()
    {
        var repository = new TestProductRepository();
        var createHandler = new CreateProductHandler(repository);
        var getHandler = new GetProductsHandler(repository);

        var createdId = await createHandler.Handle(new CreateProductCommand
        {
            Name = "Овсяные хлопья",
            CaloriesPer100g = 370,
            Protein = 12,
            Fat = 6,
            Carbs = 62
        });

        var products = await getHandler.Handle(new GetProductsQuery());

        var product = Assert.Single(products);
        Assert.Equal(createdId, product.Id);
        Assert.Equal("Овсяные хлопья", product.Name);
        Assert.Equal(370, product.CaloriesPer100g);
    }

    private sealed class TestProductRepository : IProductRepository
    {
        private readonly List<Product> products = [];

        public Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            products.Add(product);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Product>>(products);
        }

        public Task<IReadOnlyList<Product>> GetByIdsAsync(
            IReadOnlyCollection<Guid> productIds,
            CancellationToken cancellationToken = default)
        {
            var matchingProducts = products.Where(product => productIds.Contains(product.Id)).ToList();
            return Task.FromResult<IReadOnlyList<Product>>(matchingProducts);
        }
    }
}
