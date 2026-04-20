using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.Interfaces;

public interface IParticipantRepository
{
    Task AddAsync(Participant participant, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Participant>> GetAllAsync(CancellationToken cancellationToken = default);
}
