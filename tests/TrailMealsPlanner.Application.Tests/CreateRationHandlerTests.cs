using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.Tests;

public sealed class CreateRationHandlerTests
{
    [Fact]
    public async Task Handle_CreatesAndPersistsProject()
    {
        var repository = new TestRationProjectRepository();
        var handler = new CreateRationHandler(repository);

        var command = new CreateRationCommand
        {
            Name = "Kuznetsky Alatau",
            StartDate = new DateTime(2026, 5, 10),
            DurationDays = 4,
            ParticipantCount = 5,
            ActivityType = ActivityType.Mountain,
            TemperatureRange = TemperatureRange.Cold,
            WaterAvailability = WaterAvailability.Limited,
            AltitudeRange = AltitudeRange.High,
            HumidityLevel = HumidityLevel.Normal,
            WeightImportance = WeightImportance.High,
            CookingPossibility = CookingPossibility.Limited,
            ResupplyFrequency = ResupplyFrequency.Rare
        };

        var createdId = await handler.Handle(command);

        Assert.NotEqual(Guid.Empty, createdId);
        var saved = Assert.Single(repository.Projects);
        Assert.Equal(createdId, saved.Id);
        Assert.Equal("Kuznetsky Alatau", saved.Name);
        Assert.Equal(4, saved.DurationDays);
        Assert.Equal(5, saved.ParticipantCount);
        Assert.Equal(ActivityType.Mountain, saved.Profile.ActivityType);
        Assert.Equal(TemperatureRange.Cold, saved.Profile.Environment.TemperatureRange);
        Assert.Equal(WeightImportance.High, saved.Profile.Logistics.WeightImportance);
    }

    [Fact]
    public async Task Handle_CreatesCompetitionProject_WithCompetitionFocus()
    {
        var repository = new TestRationProjectRepository();
        var handler = new CreateRationHandler(repository);

        var command = new CreateRationCommand
        {
            Name = "Trail Race",
            StartDate = new DateTime(2026, 7, 5),
            DurationDays = 2,
            ParticipantCount = 1,
            ActivityType = ActivityType.Competition,
            TemperatureRange = TemperatureRange.Mild,
            WaterAvailability = WaterAvailability.Limited,
            AltitudeRange = AltitudeRange.Medium,
            HumidityLevel = HumidityLevel.Normal,
            WeightImportance = WeightImportance.High,
            CookingPossibility = CookingPossibility.Minimal,
            ResupplyFrequency = ResupplyFrequency.Daily,
            CompetitionFocus = CompetitionNutritionFocus.CarbHeavy
        };

        var createdId = await handler.Handle(command);

        var saved = Assert.Single(repository.Projects);
        Assert.Equal(createdId, saved.Id);
        Assert.Equal(CompetitionNutritionFocus.CarbHeavy, saved.Profile.CompetitionFocus);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Handle_Throws_WhenNameIsInvalid(string invalidName)
    {
        var repository = new TestRationProjectRepository();
        var handler = new CreateRationHandler(repository);

        var command = new CreateRationCommand
        {
            Name = invalidName,
            StartDate = new DateTime(2026, 5, 10),
            DurationDays = 4,
            ParticipantCount = 5,
            ActivityType = ActivityType.Mountain,
            TemperatureRange = TemperatureRange.Cold,
            WaterAvailability = WaterAvailability.Limited,
            AltitudeRange = AltitudeRange.High,
            HumidityLevel = HumidityLevel.Normal,
            WeightImportance = WeightImportance.High,
            CookingPossibility = CookingPossibility.Limited,
            ResupplyFrequency = ResupplyFrequency.Rare
        };

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command));
        Assert.Empty(repository.Projects);
    }

    [Fact]
    public async Task Handle_Throws_WhenParticipantCountIsInvalid()
    {
        var repository = new TestRationProjectRepository();
        var handler = new CreateRationHandler(repository);

        var command = new CreateRationCommand
        {
            Name = "Kuznetsky Alatau",
            StartDate = new DateTime(2026, 5, 10),
            DurationDays = 4,
            ParticipantCount = 0,
            ActivityType = ActivityType.Mountain,
            TemperatureRange = TemperatureRange.Cold,
            WaterAvailability = WaterAvailability.Limited,
            AltitudeRange = AltitudeRange.High,
            HumidityLevel = HumidityLevel.Normal,
            WeightImportance = WeightImportance.High,
            CookingPossibility = CookingPossibility.Limited,
            ResupplyFrequency = ResupplyFrequency.Rare
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => handler.Handle(command));
        Assert.Empty(repository.Projects);
    }

    [Fact]
    public async Task Handle_Throws_WhenCompetitionFocusIsUsedForNonCompetition()
    {
        var repository = new TestRationProjectRepository();
        var handler = new CreateRationHandler(repository);

        var command = new CreateRationCommand
        {
            Name = "Weekend Trail",
            StartDate = new DateTime(2026, 5, 10),
            DurationDays = 2,
            ParticipantCount = 2,
            ActivityType = ActivityType.Hiking,
            TemperatureRange = TemperatureRange.Mild,
            WaterAvailability = WaterAvailability.Limited,
            AltitudeRange = AltitudeRange.Low,
            HumidityLevel = HumidityLevel.Normal,
            WeightImportance = WeightImportance.Medium,
            CookingPossibility = CookingPossibility.Full,
            ResupplyFrequency = ResupplyFrequency.Rare,
            CompetitionFocus = CompetitionNutritionFocus.Lightweight
        };

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command));
        Assert.Empty(repository.Projects);
    }

    private sealed class TestRationProjectRepository : IRationProjectRepository
    {
        public List<RationProject> Projects { get; } = [];

        public Task AddAsync(RationProject rationProject, CancellationToken cancellationToken = default)
        {
            Projects.Add(rationProject);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RationProject>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<RationProject>>(Projects);
        }

        public Task<RationProject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Projects.FirstOrDefault(project => project.Id == id));
        }
    }
}
