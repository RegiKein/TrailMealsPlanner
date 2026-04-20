using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Infrastructure.Persistence;

public sealed class InMemoryProductPreferenceRepository : IProductPreferenceRepository
{
    private readonly List<ProductPreference> preferences = [];

    public Task UpsertAsync(ProductPreference preference, CancellationToken cancellationToken = default)
    {
        var existingPreference = preferences.FirstOrDefault(item =>
            item.ParticipantId == preference.ParticipantId &&
            item.ProductId == preference.ProductId);

        if (existingPreference is null)
        {
            preferences.Add(preference);
            return Task.CompletedTask;
        }

        existingPreference.Update(preference.PreferenceLevel, preference.Comment);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ProductPreference>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ProductPreference>>(preferences.ToList());
    }
}
