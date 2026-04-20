using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class AddParticipantHandler
{
    private readonly IParticipantRepository repository;

    public AddParticipantHandler(IParticipantRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Guid> Handle(AddParticipantCommand command, CancellationToken cancellationToken = default)
    {
        var participant = new Participant(command.Name);
        await repository.AddAsync(participant, cancellationToken);
        return participant.Id;
    }
}
