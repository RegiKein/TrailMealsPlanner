using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.DTO;

public sealed class MealDto
{
    public Guid Id { get; init; }

    public MealType Type { get; init; }

    public IReadOnlyList<MealItemDto> Items { get; init; } = [];
}
