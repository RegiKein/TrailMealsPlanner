using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class GetDayTemplatesHandler
{
    private readonly IDayTemplateRepository repository;

    public GetDayTemplatesHandler(IDayTemplateRepository repository)
    {
        this.repository = repository;
    }

    public async Task<IReadOnlyList<DayTemplateListItemDto>> Handle(
        GetDayTemplatesQuery query,
        CancellationToken cancellationToken = default)
    {
        var templates = await repository.GetAllAsync(cancellationToken);

        return templates
            .OrderBy(template => template.Name)
            .ThenBy(template => template.CreatedAt)
            .Select(template => new DayTemplateListItemDto
            {
                Id = template.Id,
                Name = template.Name,
                CreatedAt = template.CreatedAt,
                MealsCount = template.Meals.Count,
                ItemsCount = template.Meals.Sum(meal => meal.Items.Count),
                Summary = $"{template.Meals.Count} meals, {template.Meals.Sum(meal => meal.Items.Count)} items"
            })
            .ToList();
    }
}
