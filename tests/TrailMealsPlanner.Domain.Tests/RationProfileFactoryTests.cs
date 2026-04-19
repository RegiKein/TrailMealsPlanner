using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;
using TrailMealsPlanner.Domain.ValueObjects;

namespace TrailMealsPlanner.Domain.Tests;

public sealed class RationProfileFactoryTests
{
    [Theory]
    [InlineData(ActivityType.Hiking)]
    [InlineData(ActivityType.Mountain)]
    [InlineData(ActivityType.Water)]
    [InlineData(ActivityType.Ski)]
    [InlineData(ActivityType.Speleo)]
    [InlineData(ActivityType.Alpine)]
    [InlineData(ActivityType.Competition)]
    [InlineData(ActivityType.Hunting)]
    [InlineData(ActivityType.WeekendTrip)]
    [InlineData(ActivityType.LeisureWalk)]
    public void CreateDefault_ReturnsProfileForSupportedActivity(ActivityType activityType)
    {
        var profile = RationProfileFactory.CreateDefault(activityType);

        Assert.Equal(activityType, profile.ActivityType);
        Assert.NotNull(profile.Environment);
        Assert.NotNull(profile.Logistics);
    }

    [Fact]
    public void CreateDefault_Competition_HasCompetitionFocus()
    {
        var profile = RationProfileFactory.CreateDefault(ActivityType.Competition);

        Assert.Equal(ActivityType.Competition, profile.ActivityType);
        Assert.Equal(CompetitionNutritionFocus.CarbHeavy, profile.CompetitionFocus);
        Assert.Equal(WeightImportance.High, profile.Logistics.WeightImportance);
        Assert.Equal(CookingPossibility.Minimal, profile.Logistics.CookingPossibility);
    }

    [Fact]
    public void CreateDefault_Water_PrioritizesWaterAvailabilityAndLowerWeightPressure()
    {
        var profile = RationProfileFactory.CreateDefault(ActivityType.Water);

        Assert.Equal(WaterAvailability.Abundant, profile.Environment.WaterAvailability);
        Assert.Equal(CookingPossibility.Full, profile.Logistics.CookingPossibility);
        Assert.Equal(WeightImportance.Low, profile.Logistics.WeightImportance);
    }

    [Fact]
    public void CreateDefault_DoesNotDependOnSeason()
    {
        var profile = RationProfileFactory.CreateDefault(ActivityType.Hiking);

        Assert.Equal(ActivityType.Hiking, profile.ActivityType);
        Assert.Null(profile.CompetitionFocus);
    }

    [Fact]
    public void RationProfile_Throws_WhenCompetitionFocusIsUsedOutsideCompetition()
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
