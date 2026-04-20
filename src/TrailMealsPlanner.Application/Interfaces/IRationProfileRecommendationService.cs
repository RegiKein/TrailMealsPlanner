using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.Interfaces;

public interface IRationProfileRecommendationService
{
    IReadOnlyList<RecommendationDto> BuildRecommendations(
        RationProject ration,
        IReadOnlyDictionary<Guid, Dish> dishesById,
        IReadOnlyDictionary<Guid, Product> productsById);
}
