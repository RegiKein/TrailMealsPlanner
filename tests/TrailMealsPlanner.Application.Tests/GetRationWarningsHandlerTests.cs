using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Application.Services;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;

namespace TrailMealsPlanner.Application.Tests;

public sealed class GetRationWarningsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsEmptyDayWarningsForUnfilledRation()
    {
        var ration = new RationProject(
            "Empty ration",
            new DateTime(2026, 8, 1),
            durationDays: 1,
            participantCount: 1,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));
        ration.GenerateDays();

        var handler = CreateHandler(ration, [], [], [], []);
        var result = await handler.Handle(new GetRationWarningsQuery { RationId = ration.Id });

        Assert.NotNull(result);
        Assert.Contains(result!.RationWarnings, warning => warning.Code == "ration.empty");
        Assert.Contains(result.Days.Single().Warnings, warning => warning.Code == "day.empty");
    }

    [Fact]
    public async Task Handle_ReturnsDayWarningsForLowCaloriesHighWeightAndEmptyMeals()
    {
        var product = new Product("Watery stew", 100, 3, 2, 12);
        var dish = new Dish("Heavy pot");
        dish.AddIngredient(product.Id, 400);

        var ration = new RationProject(
            "Alpine push",
            new DateTime(2026, 8, 1),
            durationDays: 1,
            participantCount: 1,
            profile: RationProfileFactory.CreateDefault(ActivityType.Alpine));
        ration.GenerateDays();
        ration.AddDishToMeal(ration.Days[0].Meals[0].Id, dish.Id, 2);

        var handler = CreateHandler(ration, [dish], [product], [], []);
        var result = await handler.Handle(new GetRationWarningsQuery { RationId = ration.Id });

        Assert.NotNull(result);
        var dayWarnings = result!.Days.Single().Warnings;
        Assert.Contains(dayWarnings, warning => warning.Code == "day.low-calories");
        Assert.Contains(dayWarnings, warning => warning.Code == "day.overweight");
        Assert.Contains(dayWarnings, warning => warning.Code == "day.low-density");
        Assert.Contains(dayWarnings, warning => warning.Code == "meal.empty");
    }

    [Fact]
    public async Task Handle_ReturnsMacroImbalanceWarningForRation()
    {
        var butter = new Product("Butter", 900, 1, 100, 0);
        var butterDish = new Dish("Butter block");
        butterDish.AddIngredient(butter.Id, 350);

        var ration = new RationProject(
            "Fat-heavy ration",
            new DateTime(2026, 8, 1),
            durationDays: 1,
            participantCount: 1,
            profile: RationProfileFactory.CreateDefault(ActivityType.LeisureWalk));
        ration.GenerateDays();
        ration.AddDishToMeal(ration.Days[0].Meals[0].Id, butterDish.Id, 1);

        var handler = CreateHandler(ration, [butterDish], [butter], [], []);
        var result = await handler.Handle(new GetRationWarningsQuery { RationId = ration.Id });

        Assert.NotNull(result);
        Assert.Contains(result!.RationWarnings, warning => warning.Code == "ration.macro-imbalance");
    }

    [Fact]
    public async Task Handle_ReturnsAllergyWarningForMeal()
    {
        var peanuts = new Product("Peanuts", 600, 25, 50, 10);
        var trailMix = new Dish("Trail mix");
        trailMix.AddIngredient(peanuts.Id, 80);

        var participant = new Participant("Anna");
        var preference = new ProductPreference(participant.Id, peanuts.Id, PreferenceLevel.Allergy);

        var ration = new RationProject(
            "Test ration",
            new DateTime(2026, 8, 1),
            durationDays: 1,
            participantCount: 2,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));
        ration.GenerateDays();
        ration.AddDishToMeal(ration.Days[0].Meals[0].Id, trailMix.Id, 1);

        var handler = CreateHandler(ration, [trailMix], [peanuts], [participant], [preference]);
        var result = await handler.Handle(new GetRationWarningsQuery { RationId = ration.Id });

        Assert.NotNull(result);
        Assert.Contains(result!.Days.Single().Warnings, warning => warning.Code == "meal.allergy");
    }

    private static GetRationWarningsHandler CreateHandler(
        RationProject ration,
        IReadOnlyList<Dish> dishes,
        IReadOnlyList<Product> products,
        IReadOnlyList<Participant> participants,
        IReadOnlyList<ProductPreference> preferences)
    {
        return new GetRationWarningsHandler(
            new TestRationProjectRepository(ration),
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
