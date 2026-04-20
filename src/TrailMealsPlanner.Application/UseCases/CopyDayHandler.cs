using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class CopyDayHandler
{
    private readonly IRationProjectRepository repository;

    public CopyDayHandler(IRationProjectRepository repository)
    {
        this.repository = repository;
    }

    public async Task Handle(CopyDayCommand command, CancellationToken cancellationToken = default)
    {
        if (command.RationId == Guid.Empty)
        {
            throw new ArgumentException("Ration id is required.", nameof(command));
        }

        var ration = await repository.GetByIdAsync(command.RationId, cancellationToken);
        if (ration is null)
        {
            throw new InvalidOperationException($"Ration '{command.RationId}' was not found.");
        }

        ration.ReplaceDayContent(command.SourceDayId, command.TargetDayId);
    }
}
