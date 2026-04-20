using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Infrastructure.Persistence;

public sealed class InMemoryDayTemplateRepository : IDayTemplateRepository
{
    private readonly List<DayTemplate> templates = [];

    public Task AddAsync(DayTemplate template, CancellationToken cancellationToken = default)
    {
        templates.Add(template);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<DayTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<DayTemplate>>(templates.ToList());
    }

    public Task<DayTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(templates.FirstOrDefault(template => template.Id == id));
    }
}
