using TrailMealsPlanner.Application.Interfaces;
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

        var repository = new TestRationProjectRepository(project);
        var dishRepository = new TestDishRepository(dish);
        var handler = new GetRationByIdHandler(repository, dishRepository);

        var result = await handler.Handle(new GetRationByIdQuery { Id = project.Id });

        Assert.NotNull(result);
        Assert.Equal(project.Id, result!.Id);
        Assert.Equal("Altai Trek", result.Name);
        Assert.Equal(3, result.Days.Count);
        Assert.Equal(1, result.Days[0].DayNumber);
        Assert.Equal(new DateTime(2026, 8, 1), result.Days[0].Date);
        Assert.Equal(4, result.Days[0].Meals.Count);
        Assert.Equal(MealType.Breakfast, result.Days[0].Meals[0].Type);
        Assert.Single(result.Days[0].Meals[0].Items);
        Assert.Equal("Каша", result.Days[0].Meals[0].Items[0].Name);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenProjectDoesNotExist()
    {
        var repository = new TestRationProjectRepository();
        var dishRepository = new TestDishRepository();
        var handler = new GetRationByIdHandler(repository, dishRepository);

        var result = await handler.Handle(new GetRationByIdQuery { Id = Guid.NewGuid() });

        Assert.Null(result);
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
}
