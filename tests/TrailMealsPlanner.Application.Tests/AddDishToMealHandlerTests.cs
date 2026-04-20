using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;

namespace TrailMealsPlanner.Application.Tests;

public sealed class AddDishToMealHandlerTests
{
    [Fact]
    public async Task Handle_AddsDishToMeal()
    {
        var ration = new RationProject(
            "Altai Trek",
            new DateTime(2026, 8, 1),
            durationDays: 2,
            participantCount: 2,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));
        ration.GenerateDays();

        var dish = new Dish("Каша");
        var rationRepository = new TestRationProjectRepository(ration);
        var dishRepository = new TestDishRepository(dish);
        var handler = new AddDishToMealHandler(rationRepository, dishRepository);

        var meal = ration.Days[0].Meals[0];

        await handler.Handle(new AddDishToMealCommand
        {
            RationId = ration.Id,
            MealId = meal.Id,
            DishId = dish.Id,
            Quantity = 2
        });

        var item = Assert.Single(meal.Items);
        Assert.Equal(dish.Id, item.DishId);
        Assert.Equal(2, item.Quantity);
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
