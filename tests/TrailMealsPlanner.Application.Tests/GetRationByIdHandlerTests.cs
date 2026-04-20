using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Application.Services;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;

namespace TrailMealsPlanner.Application.Tests;

public sealed class GetRationByIdHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsProjectWithDays()
    {
        var project = new RationProject(
            "Altai Trek",
            new DateTime(2026, 8, 1),
            durationDays: 3,
            participantCount: 2,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));
        project.GenerateDays();

        var dish = new Dish("Каша");
        project.AddDishToMeal(project.Days[0].Meals[0].Id, dish.Id, 1);

        var handler = CreateHandler(project, [dish], [], [], []);

        var result = await handler.Handle(new GetRationByIdQuery { Id = project.Id });

        Assert.NotNull(result);
        Assert.Equal(project.Id, result!.Id);
        Assert.Equal("Altai Trek", result.Name);
        Assert.Equal(ActivityType.Hiking, result.Profile.ActivityType);
        Assert.Equal(3, result.Days.Count);
        Assert.Equal(1, result.Days[0].DayNumber);
        Assert.Equal(new DateTime(2026, 8, 1), result.Days[0].Date);
        Assert.Equal(4, result.Days[0].Meals.Count);
        Assert.Equal(MealType.Breakfast, result.Days[0].Meals[0].Type);
        Assert.Single(result.Days[0].Meals[0].Items);
        Assert.Equal("Каша", result.Days[0].Meals[0].Items[0].Name);
    }

    [Fact]
    public async Task Handle_ReturnsFoodIssue_WhenDishContainsAllergen()
    {
        var project = new RationProject(
            "Altai Trek",
            new DateTime(2026, 8, 1),
            durationDays: 1,
            participantCount: 2,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));
        project.GenerateDays();

        var peanuts = new Product("Peanuts", 600, 25, 50, 10);
        var dish = new Dish("Nut mix");
        dish.AddIngredient(peanuts.Id, 80);
        project.AddDishToMeal(project.Days[0].Meals[0].Id, dish.Id, 1);

        var participant = new Participant("Anna");
        var preference = new ProductPreference(participant.Id, peanuts.Id, PreferenceLevel.Allergy);
        var handler = CreateHandler(project, [dish], [peanuts], [participant], [preference]);

        var result = await handler.Handle(new GetRationByIdQuery { Id = project.Id });

        var item = Assert.Single(result!.Days[0].Meals[0].Items);
        Assert.NotNull(item.FoodIssue);
        Assert.Contains("Аллергия", item.FoodIssue!.Summary);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenProjectDoesNotExist()
    {
        var handler = CreateHandler(null, [], [], [], []);

        var result = await handler.Handle(new GetRationByIdQuery { Id = Guid.NewGuid() });

        Assert.Null(result);
    }

    private static GetRationByIdHandler CreateHandler(
        RationProject? project,
        IReadOnlyList<Dish> dishes,
        IReadOnlyList<Product> products,
        IReadOnlyList<Participant> participants,
        IReadOnlyList<ProductPreference> preferences)
    {
        return new GetRationByIdHandler(
            new TestRationProjectRepository(project is null ? [] : [project]),
            new TestDishRepository(dishes),
            new TestProductRepository(products),
            new TestParticipantRepository(participants),
            new TestProductPreferenceRepository(preferences),
            new GroupPreferenceAnalysisService());
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

        public TestDishRepository(IEnumerable<Dish> dishes)
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

        public TestProductRepository(IEnumerable<Product> products)
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

        public Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Product>>(products.Where(product => productIds.Contains(product.Id)).ToList());
        }
    }

    private sealed class TestParticipantRepository : IParticipantRepository
    {
        private readonly List<Participant> participants;

        public TestParticipantRepository(IEnumerable<Participant> participants)
        {
            this.participants = participants.ToList();
        }

        public Task AddAsync(Participant participant, CancellationToken cancellationToken = default)
        {
            participants.Add(participant);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Participant>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Participant>>(participants);
        }
    }

    private sealed class TestProductPreferenceRepository : IProductPreferenceRepository
    {
        private readonly List<ProductPreference> preferences;

        public TestProductPreferenceRepository(IEnumerable<ProductPreference> preferences)
        {
            this.preferences = preferences.ToList();
        }

        public Task UpsertAsync(ProductPreference preference, CancellationToken cancellationToken = default)
        {
            preferences.Add(preference);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ProductPreference>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ProductPreference>>(preferences);
        }
    }
}
