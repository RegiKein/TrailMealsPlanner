using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class GetProductsHandler
{
    private readonly IProductRepository repository;

    public GetProductsHandler(IProductRepository repository)
    {
        this.repository = repository;
    }

    public async Task<IReadOnlyList<ProductListItemDto>> Handle(
        GetProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        var products = await repository.GetAllAsync(cancellationToken);

        return products
            .OrderBy(product => product.Name)
            .Select(product => new ProductListItemDto
            {
                Id = product.Id,
                Name = product.Name,
                CaloriesPer100g = product.CaloriesPer100g,
                Protein = product.Protein,
                Fat = product.Fat,
                Carbs = product.Carbs
            })
            .ToList();
    }
}
