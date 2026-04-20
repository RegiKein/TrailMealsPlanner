using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;

namespace TrailMealsPlanner.Application.Tests;

public sealed class GetRationAnalyticsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAnalyticsForRation()
    {
        var oats = new Product("Овсяные хлопья", 370, 12, 6, 62);
        var raisins = new Product("Изюм", 290, 3, 0.5m, 67);
        var dish = new Dish("Каша с изюмом");
        dish.AddIngredient(oats.Id, 80);
        dish.AddIngredient(raisins.Id, 20);

        var ration = new RationProject(
            "Altai Trek",
            new DateTime(2026, 8, 1),
            durationDays: 2,
            participantCount: 2,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));
        ration.GenerateDays();
        ration.AddDishToMeal(ration.Days[0].Meals[0].Id, dish.Id, 2);

        var handler = new GetRationAnalyticsHandler(
            new TestRationProjectRepository(ration),
            new TestDishRepository(dish),
            new TestProductRepository(oats, raisins));

        var result = await handler.Handle(new GetRationAnalyticsQuery { RationId = ration.Id });

        Assert.NotNull(result);
        Assert.Equal(ration.Id, result!.RationId);
        Assert.Equal(708, result.Nutrition.Calories);
        Assert.Equal(200, result.Nutrition.Weight);
        Assert.Equal(3.54m, result.Nutrition.CaloriesPerGram);
        Assert.Equal(2, result.Days.Count);
        Assert.Equal(708, result.Days[0].Nutrition.Calories);
        Assert.Equal(200, result.Days[0].Nutrition.Weight);
        Assert.Equal(MealType.Breakfast, result.Days[0].Meals[0].MealType);
    }

    private sealed class TestRationProjectRepository : IRationProjectRepository
    {
        private readonly List<RationProject> projects;

        public TestRationProjectRepository(params RationProject[] projects)
        {
            this.projects = projects.ToList();
        }

        public Task AddAsync(RationProject rationProject, CancellationToken cancellationToken = default)
        {
            projects.Add(rationProject);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RationProject>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<RationProject>>(projects);
        }

        public Task<RationProject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(projects.FirstOrDefault(project => project.Id == id));
        }
    }

    private sealed class TestDishRepository : IDishRepository
    {
        private readonly List<Dish> dishes;

        public TestDishRepository(params Dish[] dishes)
        {
            this.dishes = dishes.ToList();
        }

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
        private readonly List<Product> products;

        public TestProductRepository(params Product[] products)
        {
            this.products = products.ToList();
        }

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
