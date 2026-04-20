using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;

namespace TrailMealsPlanner.Application.Tests;

public sealed class CopyMealHandlerTests
{
    [Fact]
    public async Task Handle_CopiesMealContentIntoTargetMeal()
    {
        var ration = new RationProject(
            "Altai Trek",
            new DateTime(2026, 8, 1),
            durationDays: 3,
            participantCount: 2,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));
        ration.GenerateDays();

        var sourceMeal = ration.Days[0].Meals[0];
        var targetMeal = ration.Days[2].Meals[2];
        var dishId = Guid.NewGuid();

        ration.AddDishToMeal(sourceMeal.Id, dishId, 2);

        var handler = new CopyMealHandler(new TestRationProjectRepository(ration));

        await handler.Handle(new CopyMealCommand
        {
            RationId = ration.Id,
            SourceMealId = sourceMeal.Id,
            TargetMealId = targetMeal.Id
        });

        Assert.Equal(MealType.Dinner, targetMeal.Type);
        var copiedItem = Assert.Single(targetMeal.Items);
        var sourceItem = Assert.Single(sourceMeal.Items);
        Assert.Equal(dishId, copiedItem.DishId);
        Assert.Equal(2, copiedItem.Quantity);
        Assert.NotEqual(sourceItem.Id, copiedItem.Id);
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
}
