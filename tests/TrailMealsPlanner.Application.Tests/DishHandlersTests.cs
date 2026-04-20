using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.Tests;

public sealed class DishHandlersTests
{
    [Fact]
    public async Task CreateAndGetDishes_Works()
    {
        var productRepository = new TestProductRepository();
        var dishRepository = new TestDishRepository();
        var oats = new Product("Овсяные хлопья", 370, 12, 6, 62);
        var raisins = new Product("Изюм", 290, 3, 0.5m, 67);

        await productRepository.AddAsync(oats);
        await productRepository.AddAsync(raisins);

        var createHandler = new CreateDishHandler(dishRepository, productRepository);
        var getHandler = new GetDishesHandler(dishRepository, productRepository);

        var createdId = await createHandler.Handle(new CreateDishCommand
        {
            Name = "Каша с изюмом",
            Ingredients =
            [
                new CreateDishIngredientModel { ProductId = oats.Id, Weight = 80 },
                new CreateDishIngredientModel { ProductId = raisins.Id, Weight = 20 }
            ]
        });

        var dishes = await getHandler.Handle(new GetDishesQuery());

        var dish = Assert.Single(dishes);
        Assert.Equal(createdId, dish.Id);
        Assert.Equal("Каша с изюмом", dish.Name);
        Assert.Equal(354m, dish.Calories);
        Assert.Equal(2, dish.Ingredients.Count);
    }

    private sealed class TestDishRepository : IDishRepository
    {
        private readonly List<Dish> dishes = [];

        public Task AddAsync(Dish dish, CancellationToken cancellationToken = default)
        {
            dishes.Add(dish);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Dish>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Dish>>(dishes);
        }
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
