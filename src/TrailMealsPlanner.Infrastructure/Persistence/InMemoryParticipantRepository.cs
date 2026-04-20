using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Infrastructure.Persistence;

public sealed class InMemoryParticipantRepository : IParticipantRepository
{
    private readonly List<Participant> participants = [];

    public Task AddAsync(Participant participant, CancellationToken cancellationToken = default)
    {
        participants.Add(participant);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Participant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Participant>>(participants.ToList());
    }
}
