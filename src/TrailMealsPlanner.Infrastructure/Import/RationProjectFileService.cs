using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.ValueObjects;

namespace TrailMealsPlanner.Infrastructure.Import;

public sealed class RationProjectFileService : IRationProjectFileService
{
    private const string FormatName = "TrailMealsPlanner.RationProject";
    private const int CurrentVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IDishRepository dishRepository;
    private readonly IProductRepository productRepository;
    private readonly IRationProjectRepository rationRepository;

    public RationProjectFileService(
        IRationProjectRepository rationRepository,
        IDishRepository dishRepository,
        IProductRepository productRepository)
    {
        this.rationRepository = rationRepository;
        this.dishRepository = dishRepository;
        this.productRepository = productRepository;
    }

    public async Task<string> ExportAsync(Guid rationId, string filePath, CancellationToken cancellationToken = default)
    {
        if (rationId == Guid.Empty)
        {
            throw new ArgumentException("Ration id is required.", nameof(rationId));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is required.", nameof(filePath));
        }

        var ration = await rationRepository.GetByIdAsync(rationId, cancellationToken);
        if (ration is null)
        {
            throw new InvalidOperationException($"Ration '{rationId}' was not found.");
        }

        var allDishes = await dishRepository.GetAllAsync(cancellationToken);
        var dishesById = allDishes.ToDictionary(dish => dish.Id);
        var referencedDishIds = ration.Days
            .SelectMany(day => day.Meals)
            .SelectMany(meal => meal.Items)
            .Where(item => item.DishId.HasValue)
            .Select(item => item.DishId!.Value)
            .Distinct()
            .ToList();

        var referencedDishes = referencedDishIds
            .Select(dishId => dishesById.TryGetValue(dishId, out var dish)
                ? dish
                : throw new InvalidOperationException($"Referenced dish '{dishId}' was not found for export."))
            .ToList();

        var referencedProductIds = referencedDishes
            .SelectMany(dish => dish.Ingredients)
            .Select(ingredient => ingredient.ProductId)
            .Concat(ration.Days
                .SelectMany(day => day.Meals)
                .SelectMany(meal => meal.Items)
                .Where(item => item.ProductId.HasValue)
                .Select(item => item.ProductId!.Value))
            .Distinct()
            .ToList();

        var products = await productRepository.GetByIdsAsync(referencedProductIds, cancellationToken);
        var productsById = products.ToDictionary(product => product.Id);

        foreach (var productId in referencedProductIds)
        {
            if (!productsById.ContainsKey(productId))
            {
                throw new InvalidOperationException($"Referenced product '{productId}' was not found for export.");
            }
        }

        var document = new RationProjectFileDocument
        {
            Format = FormatName,
            Version = CurrentVersion,
            ExportedAtUtc = DateTime.UtcNow,
            Ration = ToRationContract(ration),
            Dishes = referencedDishes.Select(ToDishContract).ToList(),
            Products = products.Select(ToProductContract).ToList()
        };

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(document, JsonOptions);
        await File.WriteAllTextAsync(filePath, json, Encoding.UTF8, cancellationToken);
        return filePath;
    }

    public async Task<Guid> ImportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is required.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Project file was not found.", filePath);
        }

        var json = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);
        var document = JsonSerializer.Deserialize<RationProjectFileDocument>(json, JsonOptions)
            ?? throw new InvalidOperationException("Project file is empty or invalid.");

        ValidateDocument(document);

        foreach (var product in document.Products)
        {
            await productRepository.AddAsync(
                Product.Restore(
                    product.Id,
                    product.Name,
                    product.CaloriesPer100g,
                    product.Protein,
                    product.Fat,
                    product.Carbs),
                cancellationToken);
        }

        foreach (var dish in document.Dishes)
        {
            await dishRepository.AddAsync(
                Dish.Restore(
                    dish.Id,
                    dish.Name,
                    dish.Ingredients.Select(ingredient => new DishIngredient(ingredient.ProductId, ingredient.Weight))),
                cancellationToken);
        }

        var rationContract = document.Ration;
        var ration = RationProject.Restore(
            rationContract.Id,
            rationContract.Name,
            rationContract.StartDate,
            rationContract.DurationDays,
            rationContract.ParticipantCount,
            new RationProfile(
                rationContract.Profile.ActivityType,
                new EnvironmentConditions(
                    rationContract.Profile.Environment.TemperatureRange,
                    rationContract.Profile.Environment.WaterAvailability,
                    rationContract.Profile.Environment.AltitudeRange,
                    rationContract.Profile.Environment.HumidityLevel),
                new LogisticsConstraints(
                    rationContract.Profile.Logistics.WeightImportance,
                    rationContract.Profile.Logistics.CookingPossibility,
                    rationContract.Profile.Logistics.ResupplyFrequency),
                rationContract.Profile.CompetitionFocus),
            rationContract.Days.Select(day => RationDay.Restore(
                day.Id,
                rationContract.Id,
                day.Date,
                day.DayNumber,
                day.Meals.Select(meal => Meal.Restore(
                    meal.Id,
                    meal.Type,
                    day.Id,
                    meal.Items.Select(item => MealItem.Restore(item.Id, item.DishId, item.ProductId, item.Quantity)))))));

        await rationRepository.AddAsync(ration, cancellationToken);
        return ration.Id;
    }

    private static void ValidateDocument(RationProjectFileDocument document)
    {
        if (!string.Equals(document.Format, FormatName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Unsupported project file format.");
        }

        if (document.Version <= 0 || document.Version > CurrentVersion)
        {
            throw new InvalidOperationException($"Unsupported project file version '{document.Version}'.");
        }

        if (document.Ration is null)
        {
            throw new InvalidOperationException("Project file does not contain ration data.");
        }
    }

    private static RationProjectContract ToRationContract(RationProject ration)
    {
        return new RationProjectContract
        {
            Id = ration.Id,
            Name = ration.Name,
            StartDate = ration.StartDate,
            DurationDays = ration.DurationDays,
            ParticipantCount = ration.ParticipantCount,
            Profile = new RationProfileContract
            {
                ActivityType = ration.Profile.ActivityType,
                CompetitionFocus = ration.Profile.CompetitionFocus,
                Environment = new EnvironmentConditionsContract
                {
                    TemperatureRange = ration.Profile.Environment.TemperatureRange,
                    WaterAvailability = ration.Profile.Environment.WaterAvailability,
                    AltitudeRange = ration.Profile.Environment.AltitudeRange,
                    HumidityLevel = ration.Profile.Environment.HumidityLevel
                },
                Logistics = new LogisticsConstraintsContract
                {
                    WeightImportance = ration.Profile.Logistics.WeightImportance,
                    CookingPossibility = ration.Profile.Logistics.CookingPossibility,
                    ResupplyFrequency = ration.Profile.Logistics.ResupplyFrequency
                }
            },
            Days = ration.Days
                .OrderBy(day => day.DayNumber)
                .Select(day => new RationDayContract
                {
                    Id = day.Id,
                    DayNumber = day.DayNumber,
                    Date = day.Date,
                    Meals = day.Meals.Select(meal => new MealContract
                    {
                        Id = meal.Id,
                        Type = meal.Type,
                        Items = meal.Items.Select(item => new MealItemContract
                        {
                            Id = item.Id,
                            DishId = item.DishId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity
                        }).ToList()
                    }).ToList()
                })
                .ToList()
        };
    }

    private static DishContract ToDishContract(Dish dish)
    {
        return new DishContract
        {
            Id = dish.Id,
            Name = dish.Name,
            Ingredients = dish.Ingredients
                .Select(ingredient => new DishIngredientContract
                {
                    ProductId = ingredient.ProductId,
                    Weight = ingredient.Weight
                })
                .ToList()
        };
    }

    private static ProductContract ToProductContract(Product product)
    {
        return new ProductContract
        {
            Id = product.Id,
            Name = product.Name,
            CaloriesPer100g = product.CaloriesPer100g,
            Protein = product.Protein,
            Fat = product.Fat,
            Carbs = product.Carbs
        };
    }

    private sealed class RationProjectFileDocument
    {
        public string Format { get; init; } = string.Empty;

        public int Version { get; init; }

        public DateTime ExportedAtUtc { get; init; }

        public RationProjectContract Ration { get; init; } = new();

        public IReadOnlyList<DishContract> Dishes { get; init; } = [];

        public IReadOnlyList<ProductContract> Products { get; init; } = [];
    }

    private sealed class RationProjectContract
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public DateTime StartDate { get; init; }

        public int DurationDays { get; init; }

        public int ParticipantCount { get; init; }

        public RationProfileContract Profile { get; init; } = new();

        public IReadOnlyList<RationDayContract> Days { get; init; } = [];
    }

    private sealed class RationProfileContract
    {
        public ActivityType ActivityType { get; init; }

        public EnvironmentConditionsContract Environment { get; init; } = new();

        public LogisticsConstraintsContract Logistics { get; init; } = new();

        public CompetitionNutritionFocus? CompetitionFocus { get; init; }
    }

    private sealed class EnvironmentConditionsContract
    {
        public TemperatureRange TemperatureRange { get; init; }

        public WaterAvailability WaterAvailability { get; init; }

        public AltitudeRange AltitudeRange { get; init; }

        public HumidityLevel HumidityLevel { get; init; }
    }

    private sealed class LogisticsConstraintsContract
    {
        public WeightImportance WeightImportance { get; init; }

        public CookingPossibility CookingPossibility { get; init; }

        public ResupplyFrequency ResupplyFrequency { get; init; }
    }

    private sealed class RationDayContract
    {
        public Guid Id { get; init; }

        public int DayNumber { get; init; }

        public DateTime Date { get; init; }

        public IReadOnlyList<MealContract> Meals { get; init; } = [];
    }

    private sealed class MealContract
    {
        public Guid Id { get; init; }

        public MealType Type { get; init; }

        public IReadOnlyList<MealItemContract> Items { get; init; } = [];
    }

    private sealed class MealItemContract
    {
        public Guid Id { get; init; }

        public Guid? DishId { get; init; }

        public Guid? ProductId { get; init; }

        public decimal Quantity { get; init; }
    }

    private sealed class DishContract
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public IReadOnlyList<DishIngredientContract> Ingredients { get; init; } = [];
    }

    private sealed class DishIngredientContract
    {
        public Guid ProductId { get; init; }

        public decimal Weight { get; init; }
    }

    private sealed class ProductContract
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public decimal CaloriesPer100g { get; init; }

        public decimal Protein { get; init; }

        public decimal Fat { get; init; }

        public decimal Carbs { get; init; }
    }
}
