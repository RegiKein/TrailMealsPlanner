using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class GetRationByIdHandler
{
    private readonly IGroupPreferenceAnalysisService groupPreferenceAnalysisService;
    private readonly IDishRepository dishRepository;
    private readonly IParticipantRepository participantRepository;
    private readonly IProductPreferenceRepository preferenceRepository;
    private readonly IRationProjectRepository repository;
    private readonly IProductRepository productRepository;

    public GetRationByIdHandler(
        IRationProjectRepository repository,
        IDishRepository dishRepository,
        IProductRepository productRepository,
        IParticipantRepository participantRepository,
        IProductPreferenceRepository preferenceRepository,
        IGroupPreferenceAnalysisService groupPreferenceAnalysisService)
    {
        this.repository = repository;
        this.dishRepository = dishRepository;
        this.productRepository = productRepository;
        this.participantRepository = participantRepository;
        this.preferenceRepository = preferenceRepository;
        this.groupPreferenceAnalysisService = groupPreferenceAnalysisService;
    }

    public async Task<RationDetailsDto?> Handle(
        GetRationByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var project = await repository.GetByIdAsync(query.Id, cancellationToken);
        if (project is null)
        {
            return null;
        }

        var dishes = await dishRepository.GetAllAsync(cancellationToken);
        var products = await productRepository.GetAllAsync(cancellationToken);
        var participants = await participantRepository.GetAllAsync(cancellationToken);
        var preferences = await preferenceRepository.GetAllAsync(cancellationToken);
        var dishesById = dishes.ToDictionary(dish => dish.Id);
        var productsById = products.ToDictionary(product => product.Id);
        var dishNamesById = dishes.ToDictionary(dish => dish.Id, dish => dish.Name);
        var productNamesById = products.ToDictionary(product => product.Id, product => product.Name);

        return new RationDetailsDto
        {
            Id = project.Id,
            Name = project.Name,
            Profile = new RationProfileDto
            {
                ActivityType = project.Profile.ActivityType,
                CompetitionFocus = project.Profile.CompetitionFocus,
                Environment = new EnvironmentConditionsDto
                {
                    TemperatureRange = project.Profile.Environment.TemperatureRange,
                    WaterAvailability = project.Profile.Environment.WaterAvailability,
                    AltitudeRange = project.Profile.Environment.AltitudeRange,
                    HumidityLevel = project.Profile.Environment.HumidityLevel
                },
                Logistics = new LogisticsConstraintsDto
                {
                    WeightImportance = project.Profile.Logistics.WeightImportance,
                    CookingPossibility = project.Profile.Logistics.CookingPossibility,
                    ResupplyFrequency = project.Profile.Logistics.ResupplyFrequency
                }
            },
            Days = project.Days
                .OrderBy(day => day.DayNumber)
                .Select(day => new RationDayDto
                {
                    Id = day.Id,
                    DayNumber = day.DayNumber,
                    Date = day.Date,
                    Meals = day.Meals
                        .Select(meal => new MealDto
                        {
                            Id = meal.Id,
                            Type = meal.Type,
                            Items = meal.Items
                                .Select(item => new MealItemDto
                                {
                                    DishId = item.DishId,
                                    ProductId = item.ProductId,
                                    Name = ResolveItemName(item.DishId, item.ProductId, dishNamesById, productNamesById),
                                    Quantity = item.Quantity,
                                    FoodIssue = groupPreferenceAnalysisService.AnalyzeMealItem(
                                        item,
                                        dishesById,
                                        productsById,
                                        participants,
                                        preferences)
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    private static string ResolveItemName(
        Guid? dishId,
        Guid? productId,
        IReadOnlyDictionary<Guid, string> dishNamesById,
        IReadOnlyDictionary<Guid, string> productNamesById)
    {
        if (dishId is Guid resolvedDishId && dishNamesById.TryGetValue(resolvedDishId, out var dishName))
        {
            return dishName;
        }

        if (productId is Guid resolvedProductId && productNamesById.TryGetValue(resolvedProductId, out var productName))
        {
            return productName;
        }

        return "Unknown item";
    }
}
