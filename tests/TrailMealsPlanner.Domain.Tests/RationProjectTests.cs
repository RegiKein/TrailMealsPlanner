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
        project.GenerateDays();

        Assert.Equal("Altai Trek", project.Name);
        Assert.Equal(startDate, project.StartDate);
        Assert.Equal(startDate.AddDays(2), project.EndDate);
        Assert.Equal(3, project.DurationDays);
        Assert.Equal(4, project.ParticipantCount);
        Assert.Equal(ActivityType.Hiking, project.Profile.ActivityType);
        Assert.Equal(3, project.Days.Count);
        Assert.Equal(4, project.Days[0].Meals.Count);
        Assert.Equal(
            new[] { MealType.Breakfast, MealType.Lunch, MealType.Dinner, MealType.Snack },
            project.Days[0].Meals.Select(meal => meal.Type).ToArray());

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

    [Fact]
    public void GenerateDays_RebuildsDaysWithoutDuplicates()
    {
        var project = new RationProject(
            "Altai Trek",
            new DateTime(2026, 4, 20),
            durationDays: 3,
            participantCount: 4,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));

        project.GenerateDays();
        project.GenerateDays();

        Assert.Equal(3, project.Days.Count);
        Assert.Equal(new[] { 1, 2, 3 }, project.Days.Select(day => day.DayNumber).ToArray());
        Assert.All(project.Days, day => Assert.Equal(4, day.Meals.Count));
    }

    [Fact]
    public void AddDishToMeal_AddsDishToSelectedMeal()
    {
        var project = new RationProject(
            "Altai Trek",
            new DateTime(2026, 4, 20),
            durationDays: 3,
            participantCount: 4,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));

        project.GenerateDays();

        var meal = project.Days[0].Meals[0];
        var dishId = Guid.NewGuid();

        project.AddDishToMeal(meal.Id, dishId, 2);

        var item = Assert.Single(meal.Items);
        Assert.Equal(dishId, item.DishId);
        Assert.Equal(2, item.Quantity);
    }
}
