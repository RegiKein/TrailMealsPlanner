using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;
using TrailMealsPlanner.Domain.ValueObjects;

namespace TrailMealsPlanner.Domain.Tests;

public sealed class RationProjectTests
{
    [Fact]
    public void Constructor_CreatesProjectAndGeneratesDays()
    {
        var startDate = new DateTime(2026, 4, 20);

        var project = new RationProject(
            "Altai Trek",
            startDate,
            durationDays: 3,
            participantCount: 4,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));

        Assert.Equal("Altai Trek", project.Name);
        Assert.Equal(startDate, project.StartDate);
        Assert.Equal(startDate.AddDays(2), project.EndDate);
        Assert.Equal(3, project.DurationDays);
        Assert.Equal(4, project.ParticipantCount);
        Assert.Equal(ActivityType.Hiking, project.Profile.ActivityType);
        Assert.Equal(3, project.Days.Count);

        Assert.Collection(
            project.Days,
            day =>
            {
                Assert.Equal(1, day.DayNumber);
                Assert.Equal(startDate, day.Date);
                Assert.Equal(project.Id, day.RationProjectId);
            },
            day =>
            {
                Assert.Equal(2, day.DayNumber);
                Assert.Equal(startDate.AddDays(1), day.Date);
                Assert.Equal(project.Id, day.RationProjectId);
            },
            day =>
            {
                Assert.Equal(3, day.DayNumber);
                Assert.Equal(startDate.AddDays(2), day.Date);
                Assert.Equal(project.Id, day.RationProjectId);
            });
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_Throws_WhenNameIsEmpty(string invalidName)
    {
        var action = () => new RationProject(
            invalidName,
            new DateTime(2026, 4, 20),
            durationDays: 3,
            participantCount: 4,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));

        Assert.Throws<ArgumentException>(action);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Throws_WhenDurationIsInvalid(int invalidDuration)
    {
        var action = () => new RationProject(
            "Altai Trek",
            new DateTime(2026, 4, 20),
            durationDays: invalidDuration,
            participantCount: 4,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Constructor_Throws_WhenParticipantCountIsInvalid(int invalidParticipantCount)
    {
        var action = () => new RationProject(
            "Altai Trek",
            new DateTime(2026, 4, 20),
            durationDays: 3,
            participantCount: invalidParticipantCount,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void Constructor_Throws_WhenCompetitionFocusIsUsedOutsideCompetition()
    {
        var action = () => new RationProfile(
            ActivityType.Hiking,
            new EnvironmentConditions(
                TemperatureRange.Mild,
                WaterAvailability.Limited,
                AltitudeRange.Low,
                HumidityLevel.Normal),
            new LogisticsConstraints(
                WeightImportance.Medium,
                CookingPossibility.Full,
                ResupplyFrequency.Rare),
            CompetitionNutritionFocus.CarbHeavy);

        Assert.Throws<ArgumentException>(action);
    }
}
