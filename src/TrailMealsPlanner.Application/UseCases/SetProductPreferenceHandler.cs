using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class SetProductPreferenceHandler
{
    private readonly IProductPreferenceRepository preferenceRepository;

    public SetProductPreferenceHandler(IProductPreferenceRepository preferenceRepository)
    {
        this.preferenceRepository = preferenceRepository;
    }

    public Task Handle(SetProductPreferenceCommand command, CancellationToken cancellationToken = default)
    {
        var preference = new ProductPreference(
            command.ParticipantId,
            command.ProductId,
            command.PreferenceLevel,
            command.Comment);

        return preferenceRepository.UpsertAsync(preference, cancellationToken);
    }
}
