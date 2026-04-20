using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class CreateDishHandler
{
    private readonly IDishRepository dishRepository;
    private readonly IProductRepository productRepository;

    public CreateDishHandler(IDishRepository dishRepository, IProductRepository productRepository)
    {
        this.dishRepository = dishRepository;
        this.productRepository = productRepository;
    }

    public async Task<Guid> Handle(CreateDishCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentException("Dish name is required.", nameof(command));
        }

        if (command.Ingredients.Count == 0)
        {
            throw new ArgumentException("Dish must contain at least one ingredient.", nameof(command));
        }

        var productIds = command.Ingredients
            .Select(ingredient => ingredient.ProductId)
            .Distinct()
            .ToArray();

        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);
        var productsById = products.ToDictionary(product => product.Id);

        var missingProductIds = productIds.Where(productId => !productsById.ContainsKey(productId)).ToArray();
        if (missingProductIds.Length > 0)
        {
            throw new InvalidOperationException(
                $"Products not found: {string.Join(", ", missingProductIds)}.");
        }

        var dish = new Dish(command.Name);
        foreach (var ingredient in command.Ingredients)
        {
            dish.AddIngredient(ingredient.ProductId, ingredient.Weight);
        }

        _ = dish.CalculateNutrition(productsById);

        await dishRepository.AddAsync(dish, cancellationToken);
        return dish.Id;
    }
}
