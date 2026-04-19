using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class GetRationsHandler
{
    private readonly IRationProjectRepository repository;

    public GetRationsHandler(IRationProjectRepository repository)
    {
        this.repository = repository;
    }

    public async Task<IReadOnlyList<RationProjectListItemDto>> Handle(
        GetRationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var projects = await repository.GetAllAsync(cancellationToken);

        return projects
            .OrderByDescending(project => project.StartDate)
            .ThenBy(project => project.Name)
            .Select(project => new RationProjectListItemDto
            {
                Id = project.Id,
                Name = project.Name,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                DurationDays = project.DurationDays,
                ParticipantCount = project.ParticipantCount,
                TourismType = project.TourismType,
                Season = project.Season
            })
            .ToList();
    }
}
