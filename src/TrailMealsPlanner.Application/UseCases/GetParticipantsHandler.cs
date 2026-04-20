using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class GetParticipantsHandler
{
    private readonly IParticipantRepository repository;

    public GetParticipantsHandler(IParticipantRepository repository)
    {
        this.repository = repository;
    }

    public async Task<IReadOnlyList<ParticipantDto>> Handle(
        GetParticipantsQuery query,
        CancellationToken cancellationToken = default)
    {
        var participants = await repository.GetAllAsync(cancellationToken);
        return participants
            .OrderBy(participant => participant.Name)
            .Select(participant => new ParticipantDto
            {
                Id = participant.Id,
                Name = participant.Name
            })
            .ToList();
    }
}
