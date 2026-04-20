using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;

namespace TrailMealsPlanner.Application.Tests;

public sealed class CopyDayHandlerTests
{
    [Fact]
    public async Task Handle_CopiesSourceDayContentIntoTargetDay()
    {
        var ration = new RationProject(
            "Altai Trek",
            new DateTime(2026, 8, 1),
            durationDays: 3,
            participantCount: 2,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));
        ration.GenerateDays();

        var sourceDay = ration.Days[0];
        var targetDay = ration.Days[2];
        var sourceMeal = sourceDay.Meals[0];
        var dishId = Guid.NewGuid();

        ration.AddDishToMeal(sourceMeal.Id, dishId, 3);

        var handler = new CopyDayHandler(new TestRationProjectRepository(ration));

        await handler.Handle(new CopyDayCommand
        {
            RationId = ration.Id,
            SourceDayId = sourceDay.Id,
            TargetDayId = targetDay.Id
        });

        var copiedDay = ration.Days.Single(day => day.DayNumber == targetDay.DayNumber);
        var copiedMeal = Assert.Single(copiedDay.Meals, meal => meal.Type == sourceMeal.Type);
        var copiedItem = Assert.Single(copiedMeal.Items);

        Assert.Equal(targetDay.Date, copiedDay.Date);
        Assert.NotEqual(targetDay.Id, copiedDay.Id);
        Assert.Equal(dishId, copiedItem.DishId);
        Assert.Equal(3, copiedItem.Quantity);
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
