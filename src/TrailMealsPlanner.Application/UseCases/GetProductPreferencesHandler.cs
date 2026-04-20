using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class GetProductPreferencesHandler
{
    private readonly IParticipantRepository participantRepository;
    private readonly IProductPreferenceRepository preferenceRepository;
    private readonly IProductRepository productRepository;

    public GetProductPreferencesHandler(
        IParticipantRepository participantRepository,
        IProductPreferenceRepository preferenceRepository,
        IProductRepository productRepository)
    {
        this.participantRepository = participantRepository;
        this.preferenceRepository = preferenceRepository;
        this.productRepository = productRepository;
    }

    public async Task<IReadOnlyList<ProductPreferenceDto>> Handle(
        GetProductPreferencesQuery query,
        CancellationToken cancellationToken = default)
    {
        var participants = await participantRepository.GetAllAsync(cancellationToken);
        var products = await productRepository.GetAllAsync(cancellationToken);
        var preferences = await preferenceRepository.GetAllAsync(cancellationToken);
        var participantNamesById = participants.ToDictionary(participant => participant.Id, participant => participant.Name);
        var productNamesById = products.ToDictionary(product => product.Id, product => product.Name);

        return preferences
            .Where(preference => participantNamesById.ContainsKey(preference.ParticipantId) && productNamesById.ContainsKey(preference.ProductId))
            .OrderBy(preference => participantNamesById[preference.ParticipantId])
            .ThenBy(preference => productNamesById[preference.ProductId])
            .Select(preference => new ProductPreferenceDto
            {
                ParticipantId = preference.ParticipantId,
                ParticipantName = participantNamesById[preference.ParticipantId],
                ProductId = preference.ProductId,
                ProductName = productNamesById[preference.ProductId],
                PreferenceLevel = preference.PreferenceLevel,
                Comment = preference.Comment
            })
            .ToList();
    }
}
