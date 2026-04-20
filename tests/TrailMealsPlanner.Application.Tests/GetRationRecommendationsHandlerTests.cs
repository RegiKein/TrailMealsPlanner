using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Application.Services;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;

namespace TrailMealsPlanner.Application.Tests;

public sealed class GetRationRecommendationsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsWaterSpecificRecommendations()
    {
        var ration = new RationProject(
            "Water route",
            new DateTime(2026, 8, 1),
            durationDays: 2,
            participantCount: 2,
            profile: RationProfileFactory.CreateDefault(ActivityType.Water));
        ration.GenerateDays();

        var handler = CreateHandler(ration);

        var result = await handler.Handle(new GetRationRecommendationsQuery { RationId = ration.Id });

        Assert.Contains(result, item => item.Message.Contains("водного маршрута", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_ReturnsSpeleoSnackRecommendation_WhenSnacksAreEmpty()
    {
        var ration = new RationProject(
            "Cave route",
            new DateTime(2026, 8, 1),
            durationDays: 1,
            participantCount: 2,
            profile: RationProfileFactory.CreateDefault(ActivityType.Speleo));
        ration.GenerateDays();

        var product = new Product("Porridge", 350, 10, 5, 60);
        var dish = new Dish("Hot porridge");
        dish.AddIngredient(product.Id, 120);
        ration.AddDishToMeal(ration.Days[0].Meals.First(meal => meal.Type == MealType.Breakfast).Id, dish.Id, 1);

        var handler = CreateHandler(ration, dish, product);

        var result = await handler.Handle(new GetRationRecommendationsQuery { RationId = ration.Id });

        Assert.Contains(result, item => item.Message.Contains("snack", StringComparison.OrdinalIgnoreCase)
            || item.Message.Contains("перекус", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_LimitsRecommendationCount()
    {
        var ration = new RationProject(
            "Alpine race",
            new DateTime(2026, 8, 1),
            durationDays: 3,
            participantCount: 1,
            profile: RationProfileFactory.CreateDefault(ActivityType.Alpine));
        ration.GenerateDays();

        var product = new Product("Soup", 80, 2, 1, 14);
        var dish = new Dish("Thin soup");
        dish.AddIngredient(product.Id, 500);
        ration.AddDishToMeal(ration.Days[0].Meals[0].Id, dish.Id, 2);

        var handler = CreateHandler(ration, dish, product);

        var result = await handler.Handle(new GetRationRecommendationsQuery { RationId = ration.Id });

        Assert.InRange(result.Count, 1, 7);
    }

    private static GetRationRecommendationsHandler CreateHandler(
        RationProject ration,
        params object[] items)
    {
        var dishes = items.OfType<Dish>().ToArray();
        var products = items.OfType<Product>().ToArray();

        return new GetRationRecommendationsHandler(
            new TestRationProjectRepository(ration),
            new TestDishRepository(dishes),
            new TestProductRepository(products),
            new RationProfileRecommendationService());
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
            return Task.FromResult<IReadOnlyList<Product>>(products.Where(product => productIds.Contains(product.Id)).ToList());
        }
    }
}
