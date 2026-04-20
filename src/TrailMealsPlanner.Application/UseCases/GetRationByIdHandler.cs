using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class GetRationByIdHandler
{
    private readonly IRationProjectRepository repository;
    private readonly IDishRepository dishRepository;

    public GetRationByIdHandler(IRationProjectRepository repository, IDishRepository dishRepository)
    {
        this.repository = repository;
        this.dishRepository = dishRepository;
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
        var dishesById = dishes.ToDictionary(dish => dish.Id, dish => dish.Name);

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
                                    Name = item.DishId is Guid dishId && dishesById.TryGetValue(dishId, out var dishName)
                                        ? dishName
                                        : "Unknown item",
                                    Quantity = item.Quantity
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
