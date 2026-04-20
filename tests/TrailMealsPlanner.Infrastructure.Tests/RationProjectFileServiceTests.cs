using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.Services;
using TrailMealsPlanner.Infrastructure.Import;
using TrailMealsPlanner.Infrastructure.Persistence;

namespace TrailMealsPlanner.Infrastructure.Tests;

public sealed class RationProjectFileServiceTests
{
    [Fact]
    public async Task ExportAndImport_RoundTripsRationWithReferencedCatalogData()
    {
        var sourceRationRepository = new InMemoryRationProjectRepository();
        var sourceDishRepository = new InMemoryDishRepository();
        var sourceProductRepository = new InMemoryProductRepository();

        var buckwheat = Product.Restore(Guid.NewGuid(), "Buckwheat", 340, 12, 3, 68);
        var stew = Product.Restore(Guid.NewGuid(), "Stew", 220, 16, 17, 1);
        await sourceProductRepository.AddAsync(buckwheat);
        await sourceProductRepository.AddAsync(stew);

        var breakfast = Dish.Restore(
            Guid.NewGuid(),
            "Camp breakfast",
            [
                new DishIngredient(buckwheat.Id, 80),
                new DishIngredient(stew.Id, 60)
            ]);
        await sourceDishRepository.AddAsync(breakfast);

        var ration = new RationProject(
            "Altai Transfer",
            new DateTime(2026, 8, 1),
            2,
            3,
            RationProfileFactory.CreateDefault(ActivityType.Hiking));
        ration.GenerateDays();
        ration.AddDishToMeal(ration.Days[0].Meals[0].Id, breakfast.Id, 2);
        await sourceRationRepository.AddAsync(ration);

        var exportService = new RationProjectFileService(sourceRationRepository, sourceDishRepository, sourceProductRepository);
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.trailration");

        try
        {
            await exportService.ExportAsync(ration.Id, filePath);

            var targetRationRepository = new InMemoryRationProjectRepository();
            var targetDishRepository = new InMemoryDishRepository();
            var targetProductRepository = new InMemoryProductRepository();
            var importService = new RationProjectFileService(targetRationRepository, targetDishRepository, targetProductRepository);

            var importedRationId = await importService.ImportAsync(filePath);
            var importedRation = await targetRationRepository.GetByIdAsync(importedRationId);

            Assert.NotNull(importedRation);
            Assert.Equal(ration.Id, importedRation.Id);
            Assert.Equal("Altai Transfer", importedRation.Name);
            Assert.Equal(2, importedRation.Days.Count);

            var importedBreakfastMeal = importedRation.Days[0].Meals.Single(meal => meal.Type == MealType.Breakfast);
            var importedItem = Assert.Single(importedBreakfastMeal.Items);
            Assert.Equal(breakfast.Id, importedItem.DishId);
            Assert.Equal(2, importedItem.Quantity);

            var importedDishes = await targetDishRepository.GetAllAsync();
            var importedProducts = await targetProductRepository.GetAllAsync();
            Assert.Contains(importedDishes, dish => dish.Id == breakfast.Id && dish.Name == "Camp breakfast");
            Assert.Contains(importedProducts, product => product.Id == buckwheat.Id && product.Name == "Buckwheat");
            Assert.Contains(importedProducts, product => product.Id == stew.Id && product.Name == "Stew");
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public async Task Import_RejectsUnsupportedProjectVersion()
    {
        var rationRepository = new InMemoryRationProjectRepository();
        var dishRepository = new InMemoryDishRepository();
        var productRepository = new InMemoryProductRepository();
        var service = new RationProjectFileService(rationRepository, dishRepository, productRepository);
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.trailration");
        var json = """
                   {
                     "format": "TrailMealsPlanner.RationProject",
                     "version": 999,
                     "exportedAtUtc": "2026-04-21T00:00:00Z",
                     "ration": {
                       "id": "11111111-1111-1111-1111-111111111111",
                       "name": "Broken",
                       "startDate": "2026-08-01T00:00:00",
                       "durationDays": 1,
                       "participantCount": 1,
                       "profile": {
                         "activityType": "Hiking",
                         "environment": {
                           "temperatureRange": "Mild",
                           "waterAvailability": "Limited",
                           "altitudeRange": "Low",
                           "humidityLevel": "Normal"
                         },
                         "logistics": {
                           "weightImportance": "Medium",
                           "cookingPossibility": "Full",
                           "resupplyFrequency": "Rare"
                         },
                         "competitionFocus": null
                       },
                       "days": []
                     },
                     "dishes": [],
                     "products": []
                   }
                   """;

        try
        {
            await File.WriteAllTextAsync(filePath, json);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ImportAsync(filePath));
            Assert.Contains("Unsupported project file version", exception.Message);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
