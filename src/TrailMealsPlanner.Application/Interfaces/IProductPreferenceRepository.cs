using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.Interfaces;

public interface IProductPreferenceRepository
{
    Task UpsertAsync(ProductPreference preference, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductPreference>> GetAllAsync(CancellationToken cancellationToken = default);
}
