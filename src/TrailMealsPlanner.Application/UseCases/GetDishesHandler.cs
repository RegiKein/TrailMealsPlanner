using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class GetDishesHandler
{
    private readonly IDishRepository dishRepository;
    private readonly IProductRepository productRepository;

    public GetDishesHandler(IDishRepository dishRepository, IProductRepository productRepository)
    {
        this.dishRepository = dishRepository;
        this.productRepository = productRepository;
    }

    public async Task<IReadOnlyList<DishListItemDto>> Handle(
        GetDishesQuery query,
        CancellationToken cancellationToken = default)
    {
        var dishes = await dishRepository.GetAllAsync(cancellationToken);
        var products = await productRepository.GetAllAsync(cancellationToken);
        var productsById = products.ToDictionary(product => product.Id);

        return dishes
            .OrderBy(dish => dish.Name)
            .Select(dish =>
            {
                var nutrition = dish.CalculateNutrition(productsById);

                return new DishListItemDto
                {
                    Id = dish.Id,
                    Name = dish.Name,
                    Calories = nutrition.Calories,
                    Protein = nutrition.Protein,
                    Fat = nutrition.Fat,
                    Carbs = nutrition.Carbs,
                    Ingredients = dish.Ingredients
                        .Select(ingredient => new DishIngredientDto
                        {
                            ProductId = ingredient.ProductId,
                            ProductName = productsById.TryGetValue(ingredient.ProductId, out var product)
                                ? product.Name
                                : "Unknown product",
                            Weight = ingredient.Weight
                        })
                        .OrderBy(ingredient => ingredient.ProductName)
                        .ToList()
                };
            })
            .ToList();
    }
}
