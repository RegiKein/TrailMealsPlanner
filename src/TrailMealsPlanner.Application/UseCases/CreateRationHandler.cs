using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class CreateRationHandler
{
    private readonly IRationProjectRepository repository;

    public CreateRationHandler(IRationProjectRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Guid> Handle(CreateRationCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentException("Ration name is required.", nameof(command));
        }

        if (command.DurationDays <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(command.DurationDays), "Duration must be greater than zero.");
        }

        if (command.ParticipantCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(command.ParticipantCount), "Participant count must be greater than zero.");
        }

        var rationProject = new RationProject(
            command.Name,
            command.StartDate,
            command.DurationDays,
            command.ParticipantCount,
            command.TourismType,
            command.Season);

        await repository.AddAsync(rationProject, cancellationToken);

        return rationProject.Id;
    }
}
