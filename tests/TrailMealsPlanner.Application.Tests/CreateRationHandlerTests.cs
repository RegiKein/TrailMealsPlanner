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
            TourismType = TourismType.Mountain,
            Season = Season.Spring
        };

        var createdId = await handler.Handle(command);

        Assert.NotEqual(Guid.Empty, createdId);
        var saved = Assert.Single(repository.Projects);
        Assert.Equal(createdId, saved.Id);
        Assert.Equal("Kuznetsky Alatau", saved.Name);
        Assert.Equal(4, saved.DurationDays);
        Assert.Equal(5, saved.ParticipantCount);
        Assert.Equal(TourismType.Mountain, saved.TourismType);
        Assert.Equal(Season.Spring, saved.Season);
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
            TourismType = TourismType.Mountain,
            Season = Season.Spring
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
            TourismType = TourismType.Mountain,
            Season = Season.Spring
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => handler.Handle(command));
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
