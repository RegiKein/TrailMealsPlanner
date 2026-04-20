using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class GetRationRecommendationsHandler
{
    private readonly IRationProjectRepository rationRepository;
    private readonly IDishRepository dishRepository;
    private readonly IProductRepository productRepository;
    private readonly IRationProfileRecommendationService recommendationService;

    public GetRationRecommendationsHandler(
        IRationProjectRepository rationRepository,
        IDishRepository dishRepository,
        IProductRepository productRepository,
        IRationProfileRecommendationService recommendationService)
    {
        this.rationRepository = rationRepository;
        this.dishRepository = dishRepository;
        this.productRepository = productRepository;
        this.recommendationService = recommendationService;
    }

    public async Task<IReadOnlyList<RecommendationDto>> Handle(
        GetRationRecommendationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var ration = await rationRepository.GetByIdAsync(query.RationId, cancellationToken);
        if (ration is null)
        {
            return [];
        }

        var dishes = await dishRepository.GetAllAsync(cancellationToken);
        var products = await productRepository.GetAllAsync(cancellationToken);

        return recommendationService.BuildRecommendations(
            ration,
            dishes.ToDictionary(dish => dish.Id),
            products.ToDictionary(product => product.Id));
    }
}
