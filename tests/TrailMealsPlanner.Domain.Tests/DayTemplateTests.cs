using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;

namespace TrailMealsPlanner.Domain.Tests;

public sealed class DayTemplateTests
{
    [Fact]
    public void CreateTemplateFromDay_CopiesMealsAndItemsWithoutKeepingReferences()
    {
        var project = new RationProject(
            "Altai Trek",
            new DateTime(2026, 8, 1),
            durationDays: 2,
            participantCount: 2,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));
        project.GenerateDays();

        var sourceDay = project.Days[0];
        var meal = sourceDay.Meals[0];
        var dishId = Guid.NewGuid();
        project.AddDishToMeal(meal.Id, dishId, 2);

        var template = DayTemplate.CreateTemplateFromDay(sourceDay, "Ходовой день");

        Assert.Equal("Ходовой день", template.Name);
        Assert.Equal(sourceDay.Meals.Count, template.Meals.Count);

        var templateMeal = Assert.Single(template.Meals, item => item.Type == meal.Type);
        var templateItem = Assert.Single(templateMeal.Items);
        var sourceItem = Assert.Single(meal.Items);

        Assert.Equal(sourceItem.DishId, templateItem.DishId);
        Assert.Equal(sourceItem.Quantity, templateItem.Quantity);
        Assert.NotEqual(sourceItem.Id, templateItem.Id);
    }

    [Fact]
    public void ApplyTemplateToDay_ReplacesTargetDayContentWithDeepCopy()
    {
        var project = new RationProject(
            "Altai Trek",
            new DateTime(2026, 8, 1),
            durationDays: 2,
            participantCount: 2,
            profile: RationProfileFactory.CreateDefault(ActivityType.Hiking));
        project.GenerateDays();

        var sourceDay = project.Days[0];
        var targetDay = project.Days[1];
        var sourceMeal = sourceDay.Meals[0];
        var sourceDishId = Guid.NewGuid();
        var targetDishId = Guid.NewGuid();

        project.AddDishToMeal(sourceMeal.Id, sourceDishId, 2);
        project.AddDishToMeal(targetDay.Meals[0].Id, targetDishId, 1);

        var template = DayTemplate.CreateTemplateFromDay(sourceDay, "Типовой день");

        DayTemplate.ApplyTemplateToDay(template, targetDay);

        var appliedMeal = Assert.Single(targetDay.Meals, meal => meal.Type == sourceMeal.Type);
        var appliedItem = Assert.Single(appliedMeal.Items);

        Assert.Equal(sourceDishId, appliedItem.DishId);
        Assert.Equal(2, appliedItem.Quantity);
        Assert.DoesNotContain(targetDay.Meals.SelectMany(meal => meal.Items), item => item.DishId == targetDishId);
        Assert.NotEqual(sourceMeal.Id, appliedMeal.Id);
    }
}
