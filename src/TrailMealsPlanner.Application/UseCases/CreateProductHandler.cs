using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class CreateProductHandler
{
    private readonly IProductRepository repository;

    public CreateProductHandler(IProductRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Guid> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = new Product(
            command.Name,
            command.CaloriesPer100g,
            command.Protein,
            command.Fat,
            command.Carbs);

        await repository.AddAsync(product, cancellationToken);
        return product.Id;
    }
}
