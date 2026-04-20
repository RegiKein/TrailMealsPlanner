using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class AddDishToMealHandler
{
    private readonly IRationProjectRepository rationRepository;
    private readonly IDishRepository dishRepository;

    public AddDishToMealHandler(IRationProjectRepository rationRepository, IDishRepository dishRepository)
    {
        this.rationRepository = rationRepository;
        this.dishRepository = dishRepository;
    }

    public async Task Handle(AddDishToMealCommand command, CancellationToken cancellationToken = default)
    {
        if (command.RationId == Guid.Empty)
        {
            throw new ArgumentException("Ration id is required.", nameof(command));
        }

        if (command.MealId == Guid.Empty)
        {
            throw new ArgumentException("Meal id is required.", nameof(command));
        }

        if (command.DishId == Guid.Empty)
        {
            throw new ArgumentException("Dish id is required.", nameof(command));
        }

        if (command.Quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(command), "Quantity must be greater than zero.");
        }

        var ration = await rationRepository.GetByIdAsync(command.RationId, cancellationToken);
        if (ration is null)
        {
            throw new InvalidOperationException($"Ration '{command.RationId}' was not found.");
        }

        var dishes = await dishRepository.GetAllAsync(cancellationToken);
        var dishExists = dishes.Any(dish => dish.Id == command.DishId);
        if (!dishExists)
        {
            throw new InvalidOperationException($"Dish '{command.DishId}' was not found.");
        }

        ration.AddDishToMeal(command.MealId, command.DishId, command.Quantity);
    }
}
