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
                Profile = new RationProfileDto
                {
                    ActivityType = project.Profile.ActivityType,
                    CompetitionFocus = project.Profile.CompetitionFocus,
                    Environment = new EnvironmentConditionsDto
                    {
                        TemperatureRange = project.Profile.Environment.TemperatureRange,
                        WaterAvailability = project.Profile.Environment.WaterAvailability,
                        AltitudeRange = project.Profile.Environment.AltitudeRange,
                        HumidityLevel = project.Profile.Environment.HumidityLevel
                    },
                    Logistics = new LogisticsConstraintsDto
                    {
                        WeightImportance = project.Profile.Logistics.WeightImportance,
                        CookingPossibility = project.Profile.Logistics.CookingPossibility,
                        ResupplyFrequency = project.Profile.Logistics.ResupplyFrequency
                    }
                }
            })
            .ToList();
    }
}
