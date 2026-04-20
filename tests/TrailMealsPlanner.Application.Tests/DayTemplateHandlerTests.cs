using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;

namespace TrailMealsPlanner.Application.Tests;

public sealed class DayTemplateHandlerTests
{
    [Fact]
    public async Task SaveDayAsTemplate_SavesTemplateFromSelectedDay()
    {
        var ration = CreateRation();
        var repository = new TestRationProjectRepository(ration);
        var templateRepository = new TestDayTemplateRepository();
        var meal = ration.Days[0].Meals[0];
        var dishId = Guid.NewGuid();

        ration.AddDishToMeal(meal.Id, dishId, 2);

        var handler = new SaveDayAsTemplateHandler(repository, templateRepository);

        var templateId = await handler.Handle(new SaveDayAsTemplateCommand
        {
            RationId = ration.Id,
            DayId = ration.Days[0].Id,
            Name = "Ходовой день"
        });

        var template = Assert.Single(templateRepository.Templates);
        Assert.Equal(template.Id, templateId);
        Assert.Equal("Ходовой день", template.Name);
        Assert.Equal(dishId, template.Meals[0].Items[0].DishId);
    }

    [Fact]
    public async Task ApplyDayTemplate_ReplacesTargetDayContent()
    {
        var ration = CreateRation();
        var rationRepository = new TestRationProjectRepository(ration);
        var templateRepository = new TestDayTemplateRepository();
        var sourceMeal = ration.Days[0].Meals[0];
        var targetMeal = ration.Days[1].Meals[0];
        var sourceDishId = Guid.NewGuid();
        var targetDishId = Guid.NewGuid();

        ration.AddDishToMeal(sourceMeal.Id, sourceDishId, 3);
        ration.AddDishToMeal(targetMeal.Id, targetDishId, 1);

        var saveHandler = new SaveDayAsTemplateHandler(rationRepository, templateRepository);
        var applyHandler = new ApplyDayTemplateHandler(rationRepository, templateRepository);

        var templateId = await saveHandler.Handle(new SaveDayAsTemplateCommand
        {
            RationId = ration.Id,
            DayId = ration.Days[0].Id,
            Name = "Стартовый день"
        });

        await applyHandler.Handle(new ApplyDayTemplateCommand
        {
            RationId = ration.Id,
            TemplateId = templateId,
            TargetDayId = ration.Days[1].Id
        });

        var replacedMeal = Assert.Single(ration.Days[1].Meals, meal => meal.Type == sourceMeal.Type);
        var replacedItem = Assert.Single(replacedMeal.Items);

        Assert.Equal(sourceDishId, replacedItem.DishId);
        Assert.Equal(3, replacedItem.Quantity);
        Assert.DoesNotContain(ration.Days[1].Meals.SelectMany(meal => meal.Items), item => item.DishId == targetDishId);
    }

    [Fact]
    public async Task GetDayTemplates_ReturnsSummaryList()
    {
        var ration = CreateRation();
        var rationRepository = new TestRationProjectRepository(ration);
        var templateRepository = new TestDayTemplateRepository();
        var meal = ration.Days[0].Meals[0];

        ration.AddDishToMeal(meal.Id, Guid.NewGuid(), 2);

        var saveHandler = new SaveDayAsTemplateHandler(rationRepository, templateRepository);
        await saveHandler.Handle(new SaveDayAsTemplateCommand
        {
            RationId = ration.Id,
            DayId = ration.Days[0].Id,
            Name = "Маршевый день"
        });

        var getHandler = new GetDayTemplatesHandler(templateRepository);
        var templates = await getHandler.Handle(new GetDayTemplatesQuery());

        var template = Assert.Single(templates);
        Assert.Equal("Маршевый день", template.Name);
        Assert.Equal(4, template.MealsCount);
        Assert.Equal(1, template.ItemsCount);
    }

    private static RationProject CreateRation()
    {
        var ration = new RationProject(
            "Altai Trek",
            new DateTime(2026, 8, 1),
            durationDays: 2,
            participantCount: 2,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));
        ration.GenerateDays();
        return ration;
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

    private sealed class TestDayTemplateRepository : IDayTemplateRepository
    {
        public List<DayTemplate> Templates { get; } = [];

        public Task AddAsync(DayTemplate template, CancellationToken cancellationToken = default)
        {
            Templates.Add(template);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<DayTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<DayTemplate>>(Templates);
        }

        public Task<DayTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Templates.FirstOrDefault(template => template.Id == id));
        }
    }
}
