using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.Interfaces;

public interface IGroupPreferenceAnalysisService
{
    FoodIssueDto? AnalyzeMealItem(
        MealItem mealItem,
        IReadOnlyDictionary<Guid, Dish> dishesById,
        IReadOnlyDictionary<Guid, Product> productsById,
        IReadOnlyList<Participant> participants,
        IReadOnlyList<ProductPreference> preferences);
}
